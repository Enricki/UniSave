namespace voidHedge.SaveSystem
{
    /// <summary>
    /// Interface for any system that needs to be saved and loaded.
    /// Each implementer is responsible for its own serialization format.
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Unique identifier for this savable. 
        /// Used as a key in the save file.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Loading priority. Lower numbers are loaded first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Serialize the current state to a JSON string.
        /// Return null or empty to skip saving this object.
        /// </summary>
        string CaptureStateJson();

        /// <summary>
        /// Restore state from a JSON string.
        /// </summary>
        void RestoreStateJson(string json);
    }
}