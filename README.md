# UniSave

Async save system for Unity. Designed to be flexible, allocation-friendly, and easy to integrate into any project.

## Features

- **Async by design**: Uses [UniTask](https://github.com/Cysharp/UniTask) for non-blocking I/O operations.
- **Storage agnostic**: Comes with `PlayerPrefsStorage` and `JsonFileStorage` out of the box, but you can easily implement your own (e.g., Cloud, custom file system).
- **Auto-save & Dirty Flags**: Save only when data changes and prevent frame drops.
- **Priority Loading**: Control the order in which your systems load their data.
- **Versioning**: Built-in support for save versioning with automatic reset on mismatch.

## Installation

This package requires [UniTask](https://github.com/Cysharp/UniTask) to be installed in your project.

1. Open Unity Package Manager (`Window -> Package Manager`).
2. Click the `+` button in the top left corner and select **"Add package from git URL..."**.
3. Paste your repository URL and click Add.

## Quick Start

### 1. Setup Save Manager

Add the `SaveManager` component to a GameObject in your scene. It's recommended to make it persistent across scenes.

```csharp
using UniSave;
using UniSave.Storage;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;

    private void Awake()
    {
        // Initialize with your preferred storage backend
        ISaveStorage storage = new JsonFileStorage("game_save.json");
        saveManager.Initialize(storage);
    }
}
```

### 2. Create Savable Data

Implement the `ISavable` interface for any class that needs to be saved.

```csharp
using UniSave;
using UnityEngine;

public class PlayerData : MonoBehaviour, ISavable
{
    public string Key => "PlayerData";
    public int Priority => 10; // Load order (lower loads first)
    
    private int _level = 1;
    private int _money = 0;

    public void AddMoney(int amount)
    {
        _money += amount;
        // Notify the system that data has changed
        SaveManager.Instance.MarkDirty();
    }

    public string CaptureStateJson()
    {
        // Serialize state to JSON
        return JsonUtility.ToJson(new PlayerSaveData { level = _level, money = _money });
    }

    public void RestoreStateJson(string json)
    {
        // Deserialize state from JSON
        var data = JsonUtility.FromJson<PlayerSaveData>(json);
        _level = data.level;
        _money = data.money;
    }

    [System.Serializable]
    private class PlayerSaveData
    {
        public int level;
        public int money;
    }
}
```

### 3. Register Data

Don't forget to register your savable objects before the save system starts.

```csharp
private void Start()
{
    SaveManager.Instance.Register(playerData);
}
```

## API Reference

- `SaveManager.Instance.MarkDirty()` — Tell the system that data has changed and needs to be saved.
- `SaveManager.Instance.SaveAsync()` — Force save immediately.
- `SaveManager.Instance.LoadAsync()` — Manually trigger loading from storage.
- `SaveManager.Instance.DeleteSaveAsync()` — Clear the save file.