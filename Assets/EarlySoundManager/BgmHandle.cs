using UnityEngine;

namespace Early.SoundManager
{
    internal sealed class BgmHandle : IBgmHandle
    {
        private readonly AudioSource audioSource;
        private readonly ISoundService soundService;
        private float previousBaseVolume = 1f;

        public BgmHandle()
        {
        }
        
        public BgmHandle(AudioSource audioSource, ISoundService soundService)
        {
            this.audioSource = audioSource;
            this.soundService = soundService;
            soundService.OnMasterVolumeChanged += ApplyVolume;
            soundService.OnBgmVolumeChanged += ApplyVolume;
            IsValid = true;
        }

#region IBgmHandle Implementation
        public float Volume => audioSource != null ? audioSource.volume : 0f;
        public float Pitch => audioSource != null ? audioSource.pitch : 0f;
        public float BaseVolume { get; private set; } = 1f;
        public float BasePitch { get; private set; } = 1f;
        public float Time => audioSource != null ? audioSource.time : 0f;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public bool IsPaused { get; private set; } = false;
        public bool IsValid { get; private set; } = false;
        public event System.Action OnPaused;
        public event System.Action OnResumed;
        public event System.Action OnVolumeChanged;
        public event System.Action OnPitchChanged;

        public void Pause()
        {
            if (!IsValid || !IsPlaying) return;

            audioSource.Pause();
            IsPaused = true;
            OnPaused?.Invoke();
        }

        public void Pause(SoundFadingOptions fadingOptions)
        {
            if (!IsValid || !IsPlaying) return;

            previousBaseVolume = BaseVolume;
            soundService.SetFadingTimer(this, new SoundFadingStatus(
                SoundFadingType.Volume,
                fadingOptions.FadeDuration,
                BaseVolume,
                0,
                () => Pause()
            ));
        }

        public void Resume()
        {
            if (!IsValid || IsPlaying) return;

            audioSource.UnPause();
            IsPaused = false;
            OnResumed?.Invoke();
        }

        public void Resume(SoundFadingOptions fadingOptions)
        {
            if (!IsValid || IsPlaying) return;

            soundService.SetFadingTimer(this, new SoundFadingStatus(
                SoundFadingType.Volume,
                fadingOptions.FadeDuration,
                0,
                previousBaseVolume,
                () => Resume()
            ));
        }

        public void SetVolume(float volume)
        {
            if (!IsValid) return;

            BaseVolume = volume;
            ApplyVolume();
        }

        public void SetVolume(float volume, SoundFadingOptions fadingOptions)
        {
            if (!IsValid) return;

            soundService.SetFadingTimer(this, new SoundFadingStatus(
                SoundFadingType.Volume,
                fadingOptions.FadeDuration,
                BaseVolume,
                volume
            ));
        }

        public void SetPitch(float pitch)
        {
            if (!IsValid) return;

            BasePitch = pitch;
            ApplyPitch();
        }

        public void SetPitch(float pitch, SoundFadingOptions fadingOptions)
        {
            if (!IsValid) return;

            soundService.SetFadingTimer(this, new SoundFadingStatus(
                SoundFadingType.Pitch,
                fadingOptions.FadeDuration,
                BasePitch,
                pitch
            ));
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
            soundService.OnMasterVolumeChanged -= ApplyVolume;
            soundService.OnBgmVolumeChanged -= ApplyVolume;
        }
#endregion

#region Private Helper Methods
        private void ApplyVolume()
        {
            audioSource.volume = BaseVolume * soundService.BgmVolume * soundService.MasterVolume;
            OnVolumeChanged?.Invoke();
        }

        private void ApplyPitch()
        {
            audioSource.pitch = BasePitch;
            OnPitchChanged?.Invoke();
        }
#endregion
    }
}    