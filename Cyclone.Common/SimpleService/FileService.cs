using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleDatabase.FileSystem;
using Cyclone.Common.SimpleResponse;
using Microsoft.AspNetCore.Hosting;
using ImageMagick;
using Serilog;
using Serilog.Context;
using Path = System.IO.Path;

namespace Cyclone.Common.SimpleService;

public class FileService<TDbContext>(TDbContext db, IWebHostEnvironment env, ILogger logger) 
    : SimpleService<UploadedFile, TDbContext>(db, logger) 
    where TDbContext : SimpleDbContext
{
    private readonly ILogger _logger = logger;

    private static string BuildRelativePath(Guid id, string? ext) =>
        Path.Combine("uploads",
            DateTime.UtcNow.Year.ToString("0000"),
            DateTime.UtcNow.Month.ToString("00"),
            id.ToString("N") + (string.IsNullOrWhiteSpace(ext) ? "" : ext.ToLowerInvariant()));

    private static bool EnsureAllowed(FileType type, string contentType, string fileName)
    {
        var (expectedMime, exts) = FileTypeMap.Map[type];

        if (!contentType.Equals(expectedMime, StringComparison.OrdinalIgnoreCase))
            return false;

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return exts.Contains(ext);
    }

    private async Task<Response<UploadedFile>> UploadFileAsync(IFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        // ограничение размера (пример: до 100 МБ)
        const long maxBytes = 100 * 1024 * 1024;
        if (file.Length is <= 0 or > maxBytes)
            return $"Недопустимый размер файла: {file.Length} байт (max {maxBytes}).";

        if (string.IsNullOrWhiteSpace(file.ContentType))
            return "Не выходит получить тип файла";

        var originalName = string.IsNullOrWhiteSpace(file.Name) ? "upload" : Path.GetFileName(file.Name);
        var ext = Path.GetExtension(originalName);
        var contentType = file.ContentType;

        FileTypeMap.TryGetByMime(contentType, out var fileType);

        // Логируем входные параметры
        using var act = LogContext.PushProperty("Action", "UploadFile");
        using var origName = LogContext.PushProperty("OriginalFileName", originalName);
        using var content = LogContext.PushProperty("ContentType", contentType);
        using var size = LogContext.PushProperty("FileSize", file.Length);
        using var property = LogContext.PushProperty("FileType", fileType.ToString());

        _logger.Information("Start uploading file {OriginalFileName} ({ContentType}), size {FileSize} bytes", originalName, contentType, file.Length);

        if (!EnsureAllowed(fileType, contentType, originalName))
        {
            _logger.Warning("File {OriginalFileName} with content type {ContentType} is not allowed", originalName, contentType);
            return "Не разрешенный формат файла";
        }

        var id = Guid.NewGuid();
        var newName = id.ToString("N") + ext;
        var relative = BuildRelativePath(id, ext);
        var webRoot = env.WebRootPath ?? throw new InvalidOperationException("WebRootPath не настроен.");
        var physicalPath = Path.Combine(webRoot, relative);

        // Логируем пути и целевой файл
        using var rel = LogContext.PushProperty("RelativePath", relative);
        using var phys = LogContext.PushProperty("PhysicalPath", physicalPath);
        using var entityId = LogContext.PushProperty("EntityId", id);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            await using (var src = file.OpenReadStream())
            await using (var dst = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            {
                await src.CopyToAsync(dst);
            }

            _logger.Information("File {OriginalFileName} saved to {PhysicalPath}", originalName, physicalPath);

            var entity = new UploadedFile(newName, fileType, physicalPath);

            // CreateAsync логирует создание сущности (и ошибки) — но логируем факт передачи в CreateAsync
            _logger.Information("Creating DB entity for uploaded file {EntityId} (name: {NewName})", id, newName);

            var result = await CreateAsync(entity);

            if (result.Success)
            {
                _logger.Information("Uploaded file persisted. EntityId: {EntityId}, StoragePath: {StoragePath}", entity.Id, entity.StoragePath);
            }
            else
            {
                _logger.Warning("Failed to persist uploaded file {EntityId}: {Error}", id, result.Errors);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while uploading file {OriginalFileName} to {PhysicalPath}", originalName, physicalPath);
            // Попытка удалить частично записанный файл (если он есть)
            try
            {
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.Information("Deleted partial file at {PhysicalPath}", physicalPath);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.Warning(cleanupEx, "Failed to cleanup partial file at {PhysicalPath}", physicalPath);
            }

            return "Ошибка при обработке файла: " + ex.Message;
        }
    }

    public async Task<Response<UploadedFile>> UploadImageAsync(IFile file)
    {
        using var act = LogContext.PushProperty("Action", "UploadImage");
        _logger.Information("UploadImageAsync called for file {FileName}", file.Name);

        if (string.IsNullOrWhiteSpace(file.ContentType))
            return "Не выходит получить тип файла";

        var contentType = file.ContentType;

        if (!FileTypeMap.TryGetByMime(contentType, out var fileType))
            return $"Не вышло получить тип файла из {contentType}";

        if (UploadedFile.GetCategory(fileType) is not FileCategory.Image)
            return $"Не верный формат файла ({contentType}, а ожидается изображение)";

        return await UploadFileAsync(file);
    }

    public async Task<Response<UploadedFile>> UploadArchiveAsync(IFile file)
    {
        using var act = LogContext.PushProperty("Action", "UploadArchive");
        _logger.Information("UploadArchiveAsync called for file {FileName}", file.Name);

        if (string.IsNullOrWhiteSpace(file.ContentType))
            return "Не выходит получить тип файла";

        var contentType = file.ContentType;

        if (!FileTypeMap.TryGetByMime(contentType, out var fileType))
            return $"Не вышло получить тип файла из {contentType}";

        if (UploadedFile.GetCategory(fileType) is not FileCategory.Archive)
            return $"Не верный формат файла ({contentType}, а ожидается архив)";

        return await UploadFileAsync(file);
    }

    public async Task<Response<UploadedFile>> UploadTableAsync(IFile file)
    {
        using var act = LogContext.PushProperty("Action", "UploadTable");
        _logger.Information("UploadTableAsync called for file {FileName}", file.Name);

        if (string.IsNullOrWhiteSpace(file.ContentType))
            return "Не выходит получить тип файла";

        var contentType = file.ContentType;

        if (!FileTypeMap.TryGetByMime(contentType, out var fileType))
            return $"Не вышло получить тип файла из {contentType}";

        if (UploadedFile.GetCategory(fileType) is not FileCategory.Table)
            return $"Не верный формат файла ({contentType}, а ожидается таблица)";

        return await UploadFileAsync(file);
    }

    public async Task<Response<UploadedFile>> CompressImageAndUploadAsync(
        UploadedFile file,
        double displayWidth,
        double displayHeight)
    {
        using var act = LogContext.PushProperty("Action", "CompressImageAndUpload");
        using var entity = LogContext.PushProperty("SourceEntityId", file.Id);
        using var path = LogContext.PushProperty("SourcePath", file.StoragePath);
        using var dims = LogContext.PushProperty("DisplayDims", $"{displayWidth}x{displayHeight}");

        _logger.Information("CompressImageAndUploadAsync called for entity {EntityId}, path {SourcePath}, target display {DisplayDims}",
            file.Id, file.StoragePath, $"{displayWidth}x{displayHeight}");

        if (UploadedFile.GetCategory(file.FileType) is not FileCategory.Image)
        {
            _logger.Warning("Input entity {EntityId} is not an image", file.Id);
            return "Входной файл не является изображением.";
        }

        var originalPath = file.StoragePath;

        try
        {
            using var image = new MagickImage(originalPath);

            const int maxSize = 256;
            var ratio = displayWidth / displayHeight;
            int targetWidth, targetHeight;
            if (ratio >= 1)
            {
                targetWidth = maxSize;
                targetHeight = (int)Math.Round(maxSize / ratio);
            }
            else
            {
                targetHeight = maxSize;
                targetWidth = (int)Math.Round(maxSize * ratio);
            }

            var size = new MagickGeometry((uint)targetWidth, (uint)targetHeight) { IgnoreAspectRatio = false };

            _logger.Information("Resizing image {EntityId} from {OriginalWidth}x{OriginalHeight} to {TargetWidth}x{TargetHeight}",
                file.Id, image.Width, image.Height, targetWidth, targetHeight);

            image.Resize(size);
            image.Format = MagickFormat.Ico;

            var newId = Guid.NewGuid();
            var newName = newId.ToString("N") + ".ico";
            var relative = BuildRelativePath(newId, ".ico");
            var webRoot = env.WebRootPath ?? throw new InvalidOperationException("WebRootPath не настроен.");
            var physicalPath = Path.Combine(webRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            await image.WriteAsync(physicalPath);

            _logger.Information("Compressed image written to {PhysicalPath}", physicalPath);

            var icoEntity = new UploadedFile(newName, FileType.ImageIco, physicalPath);

            _logger.Information("Creating DB entity for compressed image {NewEntityId}", newId);
            var result = await CreateAsync(icoEntity);

            if (result.Success)
            {
                _logger.Information("Compressed image entity created. EntityId: {EntityId}, Path: {Path}", icoEntity.Id, icoEntity.StoragePath);
            }
            else
            {
                _logger.Warning("Failed to persist compressed image entity: {Error}", result.Errors);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while compressing/uploading image for entity {EntityId}", file.Id);
            return "Ошибка при обработке изображения: " + ex.Message;
        }
    }
}
