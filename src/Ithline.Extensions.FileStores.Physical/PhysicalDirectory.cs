namespace Ithline.Extensions.FileStores.Physical;

internal sealed class PhysicalDirectory : IBlobFile, IDirectoryContents
{
    private readonly DirectoryInfo _info;
    private readonly bool _includeSubDirectories;

    public PhysicalDirectory(DirectoryInfo info, bool includeSubDirectories)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
        _includeSubDirectories = includeSubDirectories;
    }

    public string Name => _info.Name;
    public string ProviderPath => _info.FullName;
    public long Length => -1;
    public DateTimeOffset LastModified => _info.LastWriteTimeUtc;
    public bool IsDirectory => true;

    public Task<Stream> OpenReadAsync()
    {
        throw new InvalidOperationException("Cannot create file stream from a directory.");
    }

    public async IAsyncEnumerator<IBlobFile> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        IEnumerable<FileSystemInfo> entries;
        try
        {
            entries = _info.EnumerateFileSystemInfos("*", _includeSubDirectories
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Cannot enumerate file system.", ex);
        }

        foreach (var entry in entries)
        {
            if (entry is FileInfo file)
            {
                yield return new PhysicalFile(file);
            }
            else if (entry is DirectoryInfo dir)
            {
                yield return new PhysicalDirectory(dir, _includeSubDirectories);
            }
            else
            {
                throw new InvalidOperationException("Unexpected file system info.");
            }
        }
    }
}
