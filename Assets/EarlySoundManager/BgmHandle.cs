using UnityEngine;

namespace Early.SoundManager
{
    internal sealed class BgmHandle : IBgmHandle
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

        public BgmHandle()
        {
        }
        
        public BgmHandle(AudioSource audioSource, ISoundService soundService)
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
        }

#region IBgmHandle Implementation
        public float Volume => audioSource != null ? audioSource.volume : 0f;
        public float Pitch => audioSource != null ? audioSource.pitch : 0f;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public bool IsValid { get; private set; } = false;
        public event System.Action OnPaused;
        public event System.Action OnResumed;
        public event System.Action OnVolumeChanged;
        public event System.Action OnPitchChanged;

        public void Pause()
        {
            if (!IsValid || !IsPlaying) return;

            audioSource.Pause();
            OnPaused?.Invoke();
        }

        public void Pause(SoundFadingOptions fadingOptions)
        {
            if (!IsValid || !IsPlaying) return;

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
            endVolume = 1f;
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

            audioSource.volume = volume * soundService.BgmVolume * soundService.MasterVolume;
            OnVolumeChanged?.Invoke();
        }

        public void SetVolume(float volume, SoundFadingOptions crossFadingOptions)
        {
            if (!IsValid) return;

            volumeFadeTimer = 0f;
            volumeFadeDuration = crossFadingOptions.FadeDuration;
            startVolume = audioSource.volume;
            endVolume = volume * soundService.BgmVolume * soundService.MasterVolume;
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