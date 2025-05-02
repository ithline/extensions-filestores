namespace Ithline.Extensions.FileStores;

/// <summary>
/// Represents a file in the given file provider.
/// </summary>
public interface IBlobFile
{
    /// <summary>
    /// Gets the name of the file or directory, not including any path.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the full path to the file or directory inside the provider.
    /// </summary>
    public string ProviderPath { get; }

    /// <summary>
    /// Gets the length of the file in bytes, or -1 for a directory or nonexistent file.
    /// </summary>
    public long Length { get; }

    /// <summary>
    /// Gets the time when the file was last modified.
    /// </summary>
    public DateTimeOffset LastModified { get; }

    /// <summary>
    /// Gets a value that indicates whether <see cref="IBlobFileStore.GetDirectoryContentsAsync(string, bool)" /> has enumerated a subdirectory.
    /// </summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Returns file contents as a read-only stream.
    /// </summary>
    /// <returns>The file stream.</returns>
    /// <remarks>The caller should dispose the stream when complete.</remarks>
    public Task<Stream> OpenReadAsync();
}
