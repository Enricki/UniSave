using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using voidHedge.SaveSystem;

namespace voidHedge.SaveSystem.Storage
{
    /// <summary>
    /// Storage implementation that writes JSON to a file in Application.persistentDataPath.
    /// Best for standalone platforms (Windows, macOS, Android, iOS).
    /// </summary>
    public class JsonFileStorage : ISaveStorage
    {
        private readonly string _filePath;

        /// <summary>
        /// Creates a file-based storage.
        /// </summary>
        /// <param name="fileName">File name (e.g. "save.json").</param>
        public JsonFileStorage(string fileName)
        {
            _filePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public bool HasData()
        {
            return File.Exists(_filePath);
        }

        public async UniTask WriteAsync(string data)
        {
            try
            {
                // Offload blocking file I/O to a thread pool to avoid frame drops.
                // On WebGL this falls back to main thread but File I/O is memory-backed there.
                await UniTask.RunOnThreadPool(() => File.WriteAllText(_filePath, data));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniSave] Failed to write file '{_filePath}': {e.Message}");
            }
        }

        public async UniTask<string> ReadAsync()
        {
            if (!HasData()) return null;

            try
            {
                return await UniTask.RunOnThreadPool(() => File.ReadAllText(_filePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniSave] Failed to read file '{_filePath}': {e.Message}");
                return null;
            }
        }

        public async UniTask DeleteAsync()
        {
            if (!HasData()) return;

            try
            {
                await UniTask.RunOnThreadPool(() => File.Delete(_filePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniSave] Failed to delete file '{_filePath}': {e.Message}");
            }
        }
    }
}