using UnityEngine;

namespace Early.SoundManager
{
    internal sealed class SeHandle : ISeHandle
    {
        private readonly AudioSource audioSource;
        private readonly ISoundService soundService;

        private System.Action currentFadingCompletedAction = null;
        private float volumeFadeTimer = 0f;
        private float volumeFadeDuration = 0f;
        private float startVolume = 0f;
        private float endVolume = 0f;
        private float pitchFadeTimer = 0f;
        private float pitchFadeDuration = 0f;
        private float startPitch = 0f;
        private float endPitch = 0f;
        private float previousVolume = 0f;

        public SeHandle()
        {
        }

        public SeHandle(AudioSource audioSource, ISoundService soundService)
        {
            this.audioSource = audioSource;
            this.soundService = soundService;
            soundService.OnTicked += Tick;
            IsValid = true;
        }

        private void Tick()
        {
            if (!IsValid) return;

            if (volumeFadeTimer < volumeFadeDuration)
            {
                volumeFadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(volumeFadeTimer / volumeFadeDuration);
                SetVolumeRaw(Mathf.Lerp(startVolume, endVolume, t));
                if (volumeFadeTimer >= volumeFadeDuration)
                {
                    volumeFadeTimer = 0f;
                    volumeFadeDuration = 0f;
                    OnVolumeChanged?.Invoke();

                    currentFadingCompletedAction?.Invoke();
                    currentFadingCompletedAction = null;
                }
            }

            if (pitchFadeTimer < pitchFadeDuration)
            {
                pitchFadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(pitchFadeTimer / pitchFadeDuration);
                SetPitch(Mathf.Lerp(startPitch, endPitch, t));
                if (pitchFadeTimer >= pitchFadeDuration)
                {
                    pitchFadeTimer = 0f;
                    pitchFadeDuration = 0f;
                    OnPitchChanged?.Invoke();
                }
            }

            if (!audioSource.isPlaying)
            {
                Stop();
            }
        }

#region ISeHandle Implementation
        public float Volume => audioSource != null ? audioSource.volume : 0f;
        public float Pitch => audioSource != null ? audioSource.pitch : 0f;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public bool IsValid { get; private set; } = false;
        public event System.Action OnPaused;
        public event System.Action OnResumed;
        public event System.Action OnVolumeChanged;
        public event System.Action OnPitchChanged;
        public event System.Action OnCompleted;

        public void Stop()
        {
            if (!IsValid) return;

            audioSource.Stop();
            OnCompleted?.Invoke();
        }

        public void Stop(SoundFadingOptions fadingOptions)
        {
            if (!IsValid) return;

            currentFadingCompletedAction = Stop;
            volumeFadeTimer = 0f;
            volumeFadeDuration = fadingOptions.FadeDuration;
            startVolume = audioSource.volume;
            endVolume = 0f;
        }

        public void Pause()
        {
            if (!IsValid || !IsPlaying) return;

            audioSource.Pause();
            OnPaused?.Invoke();
        }

        public void Pause(SoundFadingOptions fadingOptions)
        {
            if (!IsValid || !IsPlaying) return;

            previousVolume = audioSource.volume;
            currentFadingCompletedAction = Pause;
            volumeFadeTimer = 0f;
            volumeFadeDuration = fadingOptions.FadeDuration;
            startVolume = audioSource.volume;
            endVolume = 0f;
        }

        public void Resume()
        {
            if (!IsValid || IsPlaying) return;

            audioSource.UnPause();
            OnResumed?.Invoke();
        }

        public void Resume(SoundFadingOptions fadingOptions)
        {
            if (!IsValid || IsPlaying) return;

            currentFadingCompletedAction = Resume;
            volumeFadeTimer = 0f;
            volumeFadeDuration = fadingOptions.FadeDuration;
            startVolume = audioSource.volume;
            endVolume = previousVolume * soundService.SeVolume * soundService.MasterVolume;
        }

        public void SetVolumeRaw(float volume)
        {
            if (!IsValid) return;

            audioSource.volume = volume;
            OnVolumeChanged?.Invoke();
        }

        public void SetVolume(float volume)
        {
            if (!IsValid) return;

            audioSource.volume = volume * soundService.SeVolume * soundService.MasterVolume;
            OnVolumeChanged?.Invoke();
        }

        public void SetVolume(float volume, SoundFadingOptions crossFadingOptions)
        {
            if (!IsValid) return;

            volumeFadeTimer = 0f;
            volumeFadeDuration = crossFadingOptions.FadeDuration;
            startVolume = audioSource.volume;
            endVolume = volume * soundService.SeVolume * soundService.MasterVolume;
        }

        public void SetPitch(float pitch)
        {
            if (!IsValid) return;

            audioSource.pitch = pitch;
            OnPitchChanged?.Invoke();
        }

        public void SetPitch(float pitch, SoundFadingOptions crossFadingOptions)
        {
            if (!IsValid) return;

            pitchFadeTimer = 0f;
            pitchFadeDuration = crossFadingOptions.FadeDuration;
            startPitch = audioSource.pitch;
            endPitch = pitch;
        }

        public AudioSource Release()
        {
            IsValid = false;
            return audioSource;
        }

        public void Dispose()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
            IsValid = false;
            soundService.OnTicked -= Tick;
        }
#endregion
    }
}