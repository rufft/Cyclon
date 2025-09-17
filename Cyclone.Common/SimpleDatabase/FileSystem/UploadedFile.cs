using Cyclone.Common.SimpleEntity;

namespace Cyclone.Common.SimpleDatabase.FileSystem;

public class UploadedFile : BaseEntity, IFileEntity
{
    private UploadedFile() { }

    public UploadedFile(string fileName, FileType fileType, string storagePath)
    {
        FileName = fileName;
        FileType = fileType;
        StoragePath = storagePath;
    }

    public string FileName { get; set; }
    public FileType FileType { get; set; }
    public string StoragePath { get; set; }
    
    public static FileCategory GetCategory(FileType type) => type switch
    {
        FileType.ImageJpeg or FileType.ImagePng or FileType.ImageGif or FileType.ImageWebp
            or FileType.ImageBmp or FileType.ImageSvg or FileType.ImageTiff or FileType.ImageHeic
            or FileType.ImageAvif or FileType.ImageIco => FileCategory.Image,

        FileType.ArchiveZip or FileType.Archive7Z or FileType.ArchiveRar or FileType.ArchiveTar
            or FileType.ArchiveGzip or FileType.ArchiveBzip2 or FileType.ArchiveXz => FileCategory.Archive,

        FileType.TableCsv or FileType.TableXls or FileType.TableXlsx
            or FileType.TableOds or FileType.TableNumbers => FileCategory.Table,

        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

public interface IFileEntity;

public enum FileType
{
    // Images
    ImageJpeg,
    ImagePng,
    ImageGif,
    ImageWebp,
    ImageBmp,
    ImageSvg,
    ImageTiff,
    ImageHeic,
    ImageAvif,
    ImageIco,

    // Archives
    ArchiveZip,
    Archive7Z,
    ArchiveRar,
    ArchiveTar,
    ArchiveGzip,
    ArchiveBzip2,
    ArchiveXz,

    // Spreadsheets / Tables
    TableCsv,
    TableXls,
    TableXlsx,
    TableOds,
    TableNumbers
}

public enum FileCategory
{
    Image,
    Archive,
    Table
}


