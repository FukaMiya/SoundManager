using UnityEngine;

namespace Early.SoundManager
{
    internal sealed class BgmHandle : IBgmHandle
    {
        private readonly AudioSource audioSource;
        private readonly ISoundService soundService;

        private float baseVolume = 0f;
        private float basePitch = 0f;
        private float volumeFadeMultiplier = 1f;
        private float pitchFadeMultiplier = 1f;

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
        }

        public void SetVolume(float volume)
        {
            if (!IsValid) return;

            baseVolume = volume;
            ApplyVolume();
        }

        public void SetVolume(float volume, SoundFadingOptions crossFadingOptions)
        {
            if (!IsValid) return;
        }

        public void SetVolumeFadeMultiplier(float multiplier)
        {
            if (!IsValid) return;

            volumeFadeMultiplier = multiplier;
            ApplyVolume();
        }

        public void SetPitch(float pitch)
        {
            if (!IsValid) return;

            basePitch = pitch;
            ApplyPitch();
        }

        public void SetPitch(float pitch, SoundFadingOptions crossFadingOptions)
        {
            if (!IsValid) return;
        }

        public void SetPitchFadeMultiplier(float multiplier)
        {
            if (!IsValid) return;

            pitchFadeMultiplier = multiplier;
            ApplyPitch();
        }

        public AudioSource Release()
        {
            Dispose();
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

#region Private Helper Methods
        private void ApplyVolume()
        {
            audioSource.volume = baseVolume * volumeFadeMultiplier * soundService.BgmVolume * soundService.MasterVolume;
            OnVolumeChanged?.Invoke();
        }

        private void ApplyPitch()
        {
            audioSource.pitch = basePitch * pitchFadeMultiplier;
            OnPitchChanged?.Invoke();
        }
#endregion
    }
}    