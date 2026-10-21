using Cysharp.Threading.Tasks;
using UnityEngine;

namespace voidHedge.SaveSystem.Storage
{
    /// <summary>
    /// Storage implementation based on Unity's PlayerPrefs.
    /// Works on all platforms including WebGL.
    /// </summary>
    public class PlayerPrefsStorage : ISaveStorage
    {
        private readonly string _key;

        /// <summary>
        /// Creates a new PlayerPrefs-based storage.
        /// </summary>
        /// <param name="key">Unique PlayerPrefs key for this save slot.</param>
        public PlayerPrefsStorage(string key)
        {
            _key = key;
        }

        public bool HasData()
        {
            return PlayerPrefs.HasKey(_key);
        }

        public UniTask WriteAsync(string data)
        {
            PlayerPrefs.SetString(_key, data);
            PlayerPrefs.Save(); // Force flush to disk/IndexedDB
            return UniTask.CompletedTask;
        }

        public UniTask<string> ReadAsync()
        {
            if (!PlayerPrefs.HasKey(_key))
                return UniTask.FromResult<string>(null);

            return UniTask.FromResult(PlayerPrefs.GetString(_key));
        }

        public UniTask DeleteAsync()
        {
            PlayerPrefs.DeleteKey(_key);
            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }
    }
}