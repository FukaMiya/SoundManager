namespace Early.SoundManager
{
    internal interface ISoundFadingStatus
    {
        SoundFadingType FadingType { get; set; }
        float Timer { get; set; }
        float Duration { get; set; }
        float StartValue { get; set; }
        float EndValue { get; set; }
        System.Action OnCompleted { get; set; }
    }

    internal enum SoundFadingType
    {
        Volume,
        Pitch
    }

    internal sealed class SoundFadingStatus : ISoundFadingStatus
    {
        public SoundFadingType FadingType { get; set; }
        public float Timer { get; set; }
        public float Duration { get; set; }
        public float StartValue { get; set; }
        public float EndValue { get; set; }
        public System.Action OnCompleted { get; set; }

        public SoundFadingStatus(SoundFadingType fadingType, float duration, float startValue, float endValue, System.Action onCompleted = null)
        {
            FadingType = fadingType;
            Timer = 0f;
            Duration = duration;
            StartValue = startValue;
            EndValue = endValue;
            OnCompleted = onCompleted;
        }
    }
}