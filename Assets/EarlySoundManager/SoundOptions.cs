using UnityEngine;

namespace Early.SoundManager
{
    public readonly struct SoundOptions
    {
        public readonly float Volume;
        public readonly float Pitch;
        public readonly bool Spatialize;
        public readonly AudioRolloffMode RolloffMode;
        public readonly float MinDistance;
        public readonly float MaxDistance;

        public SoundOptions(
            float volume = 1.0f,
            float pitch = 1.0f,
            bool spatialize = false,
            AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic,
            float minDistance = 1.0f,
            float maxDistance = 500.0f
        )
        {
            Volume = volume;
            Pitch = pitch;
            Spatialize = spatialize;
            RolloffMode = rolloffMode;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        public static SoundOptions Default => new (
            volume: 1.0f,
            pitch: 1.0f,
            spatialize: false,
            rolloffMode: AudioRolloffMode.Logarithmic,
            minDistance: 1.0f,
            maxDistance: 500.0f
        );
    }
}