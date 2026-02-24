using UnityEngine;

namespace Early.SoundManager
{
    public readonly struct SoundOptions
    {
        public readonly float Volume;
        public readonly float Pitch;
        public readonly bool Spatialize;
        public readonly Vector3 Position;
        public readonly AudioRolloffMode RolloffMode;
        public readonly float MinDistance;
        public readonly float MaxDistance;

        public SoundOptions(
            float volume = 1.0f,
            float pitch = 1.0f,
            bool spatialize = false,
            Vector3 position = default,
            AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic,
            float minDistance = 1.0f,
            float maxDistance = 500.0f
        )
        {
            Volume = volume;
            Pitch = pitch;
            Spatialize = spatialize;
            Position = position;
            RolloffMode = rolloffMode;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        public static SoundOptions Default => new (
            volume: 1.0f,
            pitch: 1.0f,
            spatialize: false,
            position: default,
            rolloffMode: AudioRolloffMode.Logarithmic,
            minDistance: 1.0f,
            maxDistance: 500.0f
        );
    }

    [System.Serializable]
    public struct SerializableSoundOptions
    {
        public float Volume;
        public float Pitch;
        public bool Spatialize;
        public Vector3 Position;
        public AudioRolloffMode RolloffMode;
        public float MinDistance;
        public float MaxDistance;

        public static implicit operator SoundOptions(SerializableSoundOptions options)
        {
            return new SoundOptions(
                volume: options.Volume,
                pitch: options.Pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }
    }

    public static class SoundOptionsExtensions
    {
        public static SoundOptions WithVolume(this SoundOptions options, float volume)
        {
            return new SoundOptions(
                volume: volume,
                pitch: options.Pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }

        public static SoundOptions WithVolume(this SerializableSoundOptions options, float volume)
        {
            return new SoundOptions(
                volume: volume,
                pitch: options.Pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }

        public static SoundOptions WithPitch(this SoundOptions options, float pitch)
        {
            return new SoundOptions(
                volume: options.Volume,
                pitch: pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }

        public static SoundOptions WithPitch(this SerializableSoundOptions options, float pitch)
        {
            return new SoundOptions(
                volume: options.Volume,
                pitch: pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }

        public static SoundOptions WithVolumeAndPitch(this SoundOptions options, float volume, float pitch)
        {
            return new SoundOptions(
                volume: volume,
                pitch: pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }

        public static SoundOptions WithVolumeAndPitch(this SerializableSoundOptions options, float volume, float pitch)
        {
            return new SoundOptions(
                volume: volume,
                pitch: pitch,
                spatialize: options.Spatialize,
                position: options.Position,
                rolloffMode: options.RolloffMode,
                minDistance: options.MinDistance,
                maxDistance: options.MaxDistance
            );
        }
    }
}