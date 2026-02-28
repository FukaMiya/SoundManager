using UnityEngine;

namespace Early.SoundManager
{
    internal sealed class BgmHandle : IBgmHandle
    {
        private readonly AudioSource audioSource;
        private readonly ISoundService soundService;

        public float BaseVolume { get; private set; } = 1f;
        public float BasePitch { get; private set; } = 1f;

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

            soundService.SetFadingTimer(this, new SoundVolumeFadingStatus(
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
            OnResumed?.Invoke();
        }

        public void Resume(SoundFadingOptions fadingOptions)
        {
            if (!IsValid || IsPlaying) return;

            soundService.SetFadingTimer(this, new SoundVolumeFadingStatus(
                fadingOptions.FadeDuration,
                0,
                BaseVolume,
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

            soundService.SetFadingTimer(this, new SoundVolumeFadingStatus(
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

            soundService.SetFadingTimer(this, new SoundPitchFadingStatus(
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
            soundService.OnTicked -= Tick;
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