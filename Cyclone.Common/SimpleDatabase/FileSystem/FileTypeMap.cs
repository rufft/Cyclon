namespace Cyclone.Common.SimpleDatabase.FileSystem;

public static class FileTypeMap
{
    public static readonly IReadOnlyDictionary<FileType, (string Mime, string[] Ext)> Map =
        new Dictionary<FileType, (string, string[])>
        {
            // Images
            [FileType.ImageJpeg] = ("image/jpeg", [".jpg", ".jpeg"]),
            [FileType.ImagePng]  = ("image/png", [".png"]),
            [FileType.ImageGif]  = ("image/gif", [".gif"]),
            [FileType.ImageWebp] = ("image/webp", [".webp"]),
            [FileType.ImageBmp]  = ("image/bmp", [".bmp"]),
            [FileType.ImageSvg]  = ("image/svg+xml", [".svg"]),
            [FileType.ImageTiff] = ("image/tiff", [".tif", ".tiff"]),
            [FileType.ImageHeic] = ("image/heic", [".heic"]),
            [FileType.ImageAvif] = ("image/avif", [".avif"]),
            [FileType.ImageIco]  = ("image/x-icon", [".ico"]),

            // Archives
            [FileType.ArchiveZip]   = ("application/zip", [".zip"]),
            [FileType.Archive7Z]    = ("application/x-7z-compressed", [".7z"]),
            [FileType.ArchiveRar]   = ("application/x-rar-compressed", [".rar"]),
            [FileType.ArchiveTar]   = ("application/x-tar", [".tar"]),
            [FileType.ArchiveGzip]  = ("application/gzip", [".gz"]),
            [FileType.ArchiveBzip2] = ("application/x-bzip2", [".bz2"]),
            [FileType.ArchiveXz]    = ("application/x-xz", [".xz"]),

            // Tables / Spreadsheets
            [FileType.TableCsv]     = ("text/csv", [".csv"]),
            [FileType.TableXls]     = ("application/vnd.ms-excel", [".xls"]),
            [FileType.TableXlsx]    = ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [".xlsx"]),
            [FileType.TableOds]     = ("application/vnd.oasis.opendocument.spreadsheet", [".ods"]),
            [FileType.TableNumbers] = ("application/vnd.apple.numbers", [".numbers"]),
        };

    public static bool TryGetByMime(string mime, out FileType type)
    {
        foreach (var kv in Map)
            if (string.Equals(kv.Value.Mime, mime, StringComparison.OrdinalIgnoreCase))
            { type = kv.Key; return true; }
        type = default; return false;
    }
}