using Cyclone.Common.SimpleDatabase;
using Cyclone.Common.SimpleDatabase.FileSystem;
using Cyclone.Common.SimpleResponse;
using Microsoft.AspNetCore.Hosting;
using ImageMagick;
using Path = System.IO.Path;

namespace Cyclone.Common.SimpleService;

public class FileService(SimpleDbContext db, IWebHostEnvironment env) : SimpleService<UploadedFile, SimpleDbContext>(db)
{
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
        
        if (!EnsureAllowed(fileType, contentType, ext))
            return "Не разрешенный формат файла";
        
        var id = Guid.NewGuid();
        var relative = BuildRelativePath(id, ext);
        var webRoot = env.WebRootPath ?? throw new InvalidOperationException("WebRootPath не настроен.");
        var physicalPath = Path.Combine(webRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        await using (var src = file.OpenReadStream())
        await using (var dst = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
        {
            await src.CopyToAsync(dst);
        }

        var entity = new UploadedFile(originalName, fileType, physicalPath);

        return await CreateAsync(entity);
    }

    public async Task<Response<UploadedFile>> UploadImageAsync(IFile file)
    {
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
        if (string.IsNullOrWhiteSpace(file.ContentType))
            return "Не выходит получить тип файла";
        
        var contentType = file.ContentType;

        if (!FileTypeMap.TryGetByMime(contentType, out var fileType))
            return $"Не вышло получить тип файла из {contentType}";

        if (UploadedFile.GetCategory(fileType) is not FileCategory.Table)
            return $"Не верный формат файла ({contentType}, а ожидается таблица)";
        
        return await UploadFileAsync(file);
    }

    public async Task<Response<UploadedFile>> CompresImageAndUploadAsync(
        UploadedFile file,
        double displayWidth,
        double displayHeight)
    {
        if (UploadedFile.GetCategory(file.FileType) is not FileCategory.Image)
            return "Входной файл не является изображением.";

        var originalPath = file.StoragePath;

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

        image.Resize(size);

        image.Format = MagickFormat.Ico;

        var newId = Guid.NewGuid();
        var relative = BuildRelativePath(newId, ".ico");
        var webRoot = env.WebRootPath ?? throw new InvalidOperationException("WebRootPath не настроен.");
        var physicalPath = Path.Combine(webRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        await image.WriteAsync(physicalPath);

        var icoEntity = new UploadedFile(file.FileName, FileType.ImageIco, physicalPath);

        return await CreateAsync(icoEntity);
    }
}