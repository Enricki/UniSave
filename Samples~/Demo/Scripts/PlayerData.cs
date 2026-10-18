using UnityEngine;
using UniSave;

public class PlayerData : MonoBehaviour, ISavable
{
    public int level = 1;
    public int money = 0;

    public string Key => "PlayerData";
    public int Priority => 10;

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public void LevelUp()
    {
        level++;
    }

    public string CaptureStateJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void RestoreStateJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}