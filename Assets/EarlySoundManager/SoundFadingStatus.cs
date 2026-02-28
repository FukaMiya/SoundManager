namespace Early.SoundManager
{
    internal sealed class SoundVolumeFadingStatus : ISoundFadingStatus
    {
        public float Timer { get; set; }
        public float Duration { get; set; }
        public float StartValue { get; set; }
        public float EndValue { get; set; }
        public System.Action OnCompleted { get; set; }

        public SoundVolumeFadingStatus(float duration, float startValue, float endValue, System.Action onCompleted = null)
        {
            Timer = 0f;
            Duration = duration;
            StartValue = startValue;
            EndValue = endValue;
            OnCompleted = onCompleted;
        }
    }

    internal sealed class SoundPitchFadingStatus : ISoundFadingStatus
    {
        public float Timer { get; set; }
        public float Duration { get; set; }
        public float StartValue { get; set; }
        public float EndValue { get; set; }
        public System.Action OnCompleted { get; set; }

        public SoundPitchFadingStatus(float duration, float startValue, float endValue, System.Action onCompleted = null)
        {
            Timer = 0f;
            Duration = duration;
            StartValue = startValue;
            EndValue = endValue;
            OnCompleted = onCompleted;
        }
    }
}