namespace Early.SoundManager
{
    public readonly struct SoundFadingOptions
    {
        public readonly float FadeDuration;
        public readonly CancelBehaviour CancelBehaviour;

        public SoundFadingOptions(float fadeDuration, CancelBehaviour cancelBehaviour = CancelBehaviour.Cancel)
        {
            FadeDuration = fadeDuration;
            CancelBehaviour = cancelBehaviour;
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

    public enum CancelBehaviour
    {
        Cancel,
        Complete,
        Ignore
    }
}