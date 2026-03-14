namespace Ithline.Extensions.FileStores.Physical;

internal sealed class PhysicalBlobFile : IBlobFile
{
    private readonly FileInfo _info;

    public PhysicalBlobFile(FileInfo info)
    {
        _info = info;
    }

    public string Name => _info.Name;
    public string ProviderPath => _info.FullName;
    public long Length => _info.Length;
    public DateTimeOffset LastModified => _info.LastWriteTimeUtc;
    public bool IsDirectory => false;

    public Task<Stream> OpenReadAsync()
    {
        // We are setting buffer size to 1 to prevent FileStream from allocating it's internal buffer
        // 0 causes constructor to throw
        var bufferSize = 1;
        var stream = new FileStream(
            ProviderPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult<Stream>(stream);
    }
}
