using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace voidHedge.SaveSystem
{
    /// <summary>
    /// Core save system manager.
    /// Handles registration of ISavables, auto-saving, and file I/O.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Interval between auto-saves in seconds (real time).")]
        [SerializeField] private float autoSaveInterval = 5f;

        [Tooltip("If true, the system will automatically load data on Start.")]
        [SerializeField] private bool loadOnStart = true;

        [Tooltip("Current save version. Mismatch will trigger a save reset.")]
        [SerializeField] private int currentVersion = 1;

        private ISaveStorage _storage;
        private readonly List<ISavable> _savables = new();

        private bool _isDirty;
        private bool _isLoading;
        private bool _isSaving;
        private float _timer;

        public bool IsLoading => _isLoading;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        /// <summary>
        /// Initializes the save system with a specific storage backend.
        /// Must be called before any save/load operations.
        /// </summary>
        public void Initialize(ISaveStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public bool HasSave()
        {
            return _storage != null && _storage.HasData();
        }

        private async void Start()
        {
            // Ждем один кадр, чтобы все остальные скрипты успели сделать Initialize()
            await UniTask.Yield();

            if (_storage == null)
            {
                Debug.LogError("[UniSave] SaveManager has no storage initialized. Call Initialize() first.");
                return;
            }

            // Еще один кадр, чтобы все ISavables зарегистрировались в своих Start()
            await UniTask.Yield();

            if (loadOnStart && _storage.HasData())
            {
                await LoadAsync();
            }
        }

        private void Update()
        {
            if (_isLoading || _isSaving || _storage == null) return;
            if (!_isDirty) return;

            _timer += Time.unscaledDeltaTime;
            if (_timer < autoSaveInterval) return;

            _timer = 0f;
            SaveAsync().Forget();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && _isDirty && !_isLoading && !_isSaving) SaveAsync().Forget();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && _isDirty && !_isLoading && !_isSaving) SaveAsync().Forget();
        }

        private void OnApplicationQuit()
        {
            if (_isDirty && !_isLoading && !_isSaving) SaveAsync().Forget();
        }

        // ==================== Registration ====================

        /// <summary>
        /// Registers a savable object. Must be called by the object itself in Start().
        /// </summary>
        public void Register(ISavable savable)
        {
            if (savable == null) return;

            foreach (var s in _savables)
            {
                if (s.Key == savable.Key)
                {
                    Debug.LogError($"[UniSave] Duplicate SaveKey detected: '{savable.Key}'. Registration rejected.");
                    return;
                }
            }

            _savables.Add(savable);
        }

        public void Unregister(ISavable savable)
        {
            if (savable == null) return;
            _savables.Remove(savable);
        }

        // ==================== Dirty State ====================

        /// <summary>
        /// Marks the save data as dirty, triggering auto-save on the next interval.
        /// Call this whenever relevant game data changes.
        /// </summary>
        public void MarkDirty()
        {
            if (_isLoading) return;
            _isDirty = true;
        }

        // ==================== Save / Load ====================

        /// <summary>
        /// Forces an immediate save. Can be called manually (e.g. from a UI button).
        /// </summary>
        public async UniTask SaveAsync()
        {
            if (_storage == null || _isLoading || _isSaving) return;

            _isSaving = true;
            try
            {
                var envelope = new SaveEnvelope { version = currentVersion };

                foreach (var savable in _savables)
                {
                    try
                    {
                        string json = savable.CaptureStateJson();
                        if (string.IsNullOrEmpty(json)) continue;

                        envelope.entries.Add(new SaveEntry { key = savable.Key, json = json });
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[UniSave] Failed to capture {savable.Key}: {e.Message}");
                    }
                }

                string payload = JsonUtility.ToJson(envelope);
                await _storage.WriteAsync(payload);

                _isDirty = false;
                _timer = 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniSave] Save failed: {e.Message}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// Loads data from storage and restores all registered ISavables.
        /// </summary>
        public async UniTask LoadAsync()
        {
            if (_storage == null) return;

            _isLoading = true;
            try
            {
                string payload = await _storage.ReadAsync();
                if (string.IsNullOrEmpty(payload)) return;

                SaveEnvelope envelope = null;
                try
                {
                    envelope = JsonUtility.FromJson<SaveEnvelope>(payload);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniSave] Failed to parse JSON: {e.Message}");
                    await _storage.DeleteAsync();
                    return;
                }

                if (envelope == null) return;

                if (envelope.version != currentVersion)
                {
                    Debug.LogWarning($"[UniSave] Version mismatch ({envelope.version} vs {currentVersion}). Save reset.");
                    await _storage.DeleteAsync();
                    return;
                }

                // Load in priority order (lower goes first)
                var sorted = _savables.OrderBy(s => s.Priority).ToList();

                foreach (var savable in sorted)
                {
                    var entry = envelope.entries.FirstOrDefault(e => e.key == savable.Key);
                    if (entry == null || string.IsNullOrEmpty(entry.json)) continue;

                    try
                    {
                        savable.RestoreStateJson(entry.json);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[UniSave] Failed to restore {savable.Key}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniSave] Load failed: {e.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Deletes the current save data from storage.
        /// </summary>
        public async UniTask DeleteSaveAsync()
        {
            if (_storage == null) return;
            await _storage.DeleteAsync();
            _isDirty = false;
            _timer = 0f;
            Debug.Log("[UniSave] Save data deleted.");
        }
    }
}