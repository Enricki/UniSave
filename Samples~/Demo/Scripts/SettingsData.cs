using UnityEngine;
using UniSave;

public class SettingsData : MonoBehaviour, ISavable
{
    public float volume = 0.8f;

    public string Key => "Settings";
    public int Priority => 5; // Loads before PlayerData

    public string CaptureStateJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void RestoreStateJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}