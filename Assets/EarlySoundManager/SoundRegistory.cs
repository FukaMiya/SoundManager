using UnityEngine;

namespace Early.SoundManager
{
    [CreateAssetMenu(fileName = "SoundRegistory", menuName = "SoundManager/SoundRegistory")]
    public sealed class SoundRegistory : ScriptableObject
    {
        public SoundEntry[] SoundEntries;
    }

    [System.Serializable]
    public sealed class SoundEntry
    {
        public string key;
        public AudioClip clip;
    }
}