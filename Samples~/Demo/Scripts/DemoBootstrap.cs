using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniSave;
using UniSave.Storage;

public class DemoBootstrap : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text statsText;
    public Button addMoneyButton;
    public Button levelUpButton;
    public Button saveButton;
    public Button loadButton;
    public Button resetButton;
    public Button deleteButton;

    [Header("Game Data")]
    public PlayerData playerData;
    public SettingsData settingsData;

    private void Awake()
    {
        // Find the SaveManager instance to initialize it
        if (SaveManager.Instance != null)
        {
            var storage = new JsonFileStorage("unisave_demo.json");
            SaveManager.Instance.Initialize(storage);
        }
        else
        {
            Debug.LogError("[Demo] SaveManager is missing from the scene!");
        }
    }

    private void Start()
    {
        if (SaveManager.Instance == null) return;

        // Register the savable objects
        SaveManager.Instance.Register(playerData);
        SaveManager.Instance.Register(settingsData);

        // ==========================================
        // 1. DATA MODIFIERS (and MarkDirty calls)
        // ==========================================

        addMoneyButton.onClick.AddListener(() =>
        {
            playerData.AddMoney(100);
            SaveManager.Instance.MarkDirty(); //
            UpdateUI();
        });

        levelUpButton.onClick.AddListener(() =>
        {
            playerData.LevelUp();
            SaveManager.Instance.MarkDirty(); //
            UpdateUI();
        });

        // ==========================================
        // 2. RESET (Memory only - NO MarkDirty)
        // ==========================================

        resetButton.onClick.AddListener(() =>
        {
            // Change fields directly. 
            // We don't call MarkDirty() here because we don't want to auto-save this reset.
            playerData.level = 1;
            playerData.money = 0;
            UpdateUI();
            Debug.Log("Data reset in memory (not saved to file).");
        });

        // ==========================================
        // 3. SAVE / LOAD / DELETE
        // ==========================================

        saveButton.onClick.AddListener(async () =>
        {
            await SaveManager.Instance.SaveAsync();
            Debug.Log("Manual Save Triggered");
        });

        loadButton.onClick.AddListener(async () =>
        {
            await SaveManager.Instance.LoadAsync();
            UpdateUI();
            Debug.Log("Data loaded from file.");
        });

        deleteButton.onClick.AddListener(async () =>
        {
            await SaveManager.Instance.DeleteSaveAsync();
            playerData.level = 1;
            playerData.money = 0;
            UpdateUI();
            Debug.Log("Save file deleted and memory reset.");
        });

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (statsText != null && playerData != null)
            statsText.text = $"Level: {playerData.level}\nMoney: {playerData.money}$";
    }
}