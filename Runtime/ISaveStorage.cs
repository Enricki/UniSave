using Cysharp.Threading.Tasks;

namespace UniSave
{
    /// <summary>
    /// Abstraction over data storage (files, PlayerPrefs, cloud, etc.).
    /// All methods are async to support slow I/O and remote backends.
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>
        /// Checks if any data exists in the storage.
        /// Should be synchronous — it's just a key/file existence check.
        /// </summary>
        bool HasData();

        /// <summary>
        /// Writes the entire save payload as a single string.
        /// </summary>
        UniTask WriteAsync(string data);

        /// <summary>
        /// Reads the save payload. Returns null if no data exists.
        /// </summary>
        UniTask<string> ReadAsync();

        /// <summary>
        /// Deletes the save data from storage.
        /// </summary>
        UniTask DeleteAsync();
    }
}