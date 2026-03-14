namespace Ithline.Extensions.FileStores.Physical;

/// <summary>
/// Represents a file store backed by file system.
/// </summary>
public sealed class PhysicalBlobFileStore : IBlobFileStore
{
    private readonly string _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalBlobFileStore"/> with the specified root path.
    /// </summary>
    /// <param name="root">The root directory used to store files.</param>
    /// <exception cref="ArgumentException"><paramref name="root"/> is <see langword="null"/>, empty string, contains only white-space characters or is not a rooted path.</exception>
    public PhysicalBlobFileStore(string root)
    {
        if (!Path.IsPathRooted(root))
        {
            throw new ArgumentException("The path must be absolute.", nameof(root));
        }

        // When we do matches in GetFullPath, we want to only match full directory names.
        var fullRoot = Path.GetFullPath(root);
        fullRoot = PathUtils.EnsureTrailingSlash(fullRoot);

        _root = fullRoot;
    }

    /// <inheritdoc/>
    public Task<IBlobFile?> GetFileAsync(string path)
    {
        if (string.IsNullOrEmpty(path) || PathUtils.HasInvalidPathChars(path))
        {
            return Task.FromResult<IBlobFile?>(null);
        }

        // Relative paths starting with leading slashes are okay
        path = PathUtils.TrimSeparators(path);
        var fullPath = this.GetFullPath(path);
        if (fullPath is null)
        {
            return Task.FromResult<IBlobFile?>(null);
        }

        var fileInfo = new FileInfo(fullPath);
        if (!fileInfo.Exists)
        {
            return Task.FromResult<IBlobFile?>(null);
        }

        return Task.FromResult<IBlobFile?>(new PhysicalBlobFile(fileInfo));
    }

    /// <inheritdoc/>
    public IBlobDirectoryContents GetDirectoryContentsAsync(string path, bool includeSubDirectories = false)
    {
        if (string.IsNullOrEmpty(path) || PathUtils.HasInvalidPathChars(path))
        {
            return NotFoundDirectoryContents.Singleton;
        }

        // Relative paths starting with leading slashes are okay
        path = PathUtils.TrimSeparators(path);
        var fullPath = this.GetFullPath(path);
        if (fullPath is null)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        var directoryInfo = new DirectoryInfo(fullPath);
        if (!directoryInfo.Exists)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        return new PhysicalBlobDirectory(directoryInfo, includeSubDirectories);
    }

    /// <inheritdoc/>
    public async Task<IBlobFile> CreateFileAsync(string path, ReadOnlyMemory<byte> buffer, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        path = PathUtils.TrimSeparators(path);
        var fullPath = this.GetFullPath(path) ?? throw new ArgumentException("Path cannot be rooted or navigate above the store root.", nameof(path));

        var fileInfo = new FileInfo(fullPath);
        if (!overwrite && fileInfo.Exists)
        {
            throw new ArgumentException($"File '{path}' already exists.", nameof(path));
        }

        // we ensure directory exists
        fileInfo.Directory?.Create();

        using (var fs = fileInfo.Create())
        {
            await fs.WriteAsync(buffer).ConfigureAwait(false);
            await fs.FlushAsync().ConfigureAwait(false);
        }

        return new PhysicalBlobFile(fileInfo);
    }

    /// <inheritdoc/>
    public async Task<IBlobFile> CreateFileAsync(string path, Stream stream, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(stream);

        path = PathUtils.TrimSeparators(path);
        var fullPath = this.GetFullPath(path) ?? throw new ArgumentException("Path cannot be rooted or navigate above the store root.", nameof(path));

        var fileInfo = new FileInfo(fullPath);
        if (!overwrite && fileInfo.Exists)
        {
            throw new ArgumentException($"File '{path}' already exists.", nameof(path));
        }

        // we ensure directory exists
        fileInfo.Directory?.Create();

        using (var fs = fileInfo.Create())
        {
            await stream.CopyToAsync(fs).ConfigureAwait(false);
            await fs.FlushAsync().ConfigureAwait(false);
        }

        return new PhysicalBlobFile(fileInfo);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteFileAsync(string path)
    {
        path = PathUtils.TrimSeparators(path);
        var fullPath = this.GetFullPath(path);
        if (fullPath is null)
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    private string? GetFullPath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return null;
        }

        if (PathUtils.PathNavigatesAboveRoot(path))
        {
            return null;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(Path.Combine(_root, path));
        }
        catch
        {
            return null;
        }

        if (!this.IsUnderneathRoot(fullPath))
        {
            return null;
        }

        return fullPath;
    }

    private bool IsUnderneathRoot(string fullPath)
    {
        return fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase);
    }
}
