namespace Ithline.Extensions.FileStores;

/// <summary>
/// Represents an abstraction over a specialized blob storage.
/// </summary>
public interface IBlobFileStore
{
    /// <summary>
    /// Retrieves information about the given entry within the file store.
    /// </summary>
    /// <param name="path">The path within the file store.</param>
    /// <returns>A <see cref="IBlobFile"/> object representing the entry, or <see langword="null" /> if the entry does not exist.</returns>
    public Task<IBlobFile?> GetFileAsync(string path);

    /// <summary>
    /// Enumerates the content (files and directories) in a given directory within the file store.
    /// </summary>
    /// <param name="path">The path of the directory to enumerate, or <see langword="null" /> to enumerate the root of the file store.</param>
    /// <param name="includeSubDirectories">A flag to indicate whether to get the contents from just the top directory or from all sub-directories as well.</param>
    /// <returns>The list of files and directories in the given directory.</returns>
    public IDirectoryContents GetDirectoryContentsAsync(string path, bool includeSubDirectories = false);

    /// <summary>
    /// Creates a new file in the file store from the contents of an input stream.
    /// </summary>
    /// <param name="path">The path of the file to be created.</param>
    /// <param name="stream">The stream whose contents to write to the new file.</param>
    /// <param name="overwrite"><see langword="true" /> to overwrite if a file already exists; <see langword="false" /> to throw an exception if the file already exists.</param>
    public Task<IBlobFile> CreateFileAsync(string path, Stream stream, bool overwrite = false);

    /// <summary>
    /// Deletes a file in the file store if it exists.
    /// </summary>
    /// <param name="path">The path of the file to be deleted.</param>
    /// <returns><see langword="true" /> if the file was deleted; <see langword="false" /> if the file did not exist.</returns>
    public Task<bool> DeleteFileAsync(string path);
}
