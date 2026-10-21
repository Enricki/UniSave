using System;
using System.Collections.Generic;

namespace voidHedge.SaveSystem
{
    /// <summary>
    /// Root container for a save file.
    /// Holds version info and all serialized entries.
    /// </summary>
    [Serializable]
    public class SaveEnvelope
    {
        public int version;
        public List<SaveEntry> entries = new();
    }
}