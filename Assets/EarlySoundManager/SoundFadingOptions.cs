using System.Threading;

namespace Early.SoundManager
{
    public readonly struct SoundFadingOptions
    {
        public readonly CancellationToken CancellationToken;
        public readonly float FadeDuration;

        public SoundFadingOptions(float fadeDuration, CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            FadeDuration = fadeDuration;
        }
    }

    [System.Serializable]
    public struct SerializableSoundFadingOptions
    {
        public float FadeDuration;

        public static implicit operator SoundFadingOptions(SerializableSoundFadingOptions options)
        {
            return new SoundFadingOptions(options.FadeDuration);
        }
    }
}