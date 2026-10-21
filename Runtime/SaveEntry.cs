using System;

namespace voidHedge.SaveSystem
{
    /// <summary>
    /// One serialized chunk of data from a single ISavable.
    /// </summary>
    [Serializable]
    public class SaveEntry
    {
        public string key;
        public string json;
    }
}