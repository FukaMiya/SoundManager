using UnityEngine;

namespace Early.SoundManager
{
    internal sealed class SeHandle : ISeHandle
    {
        private readonly AudioSource audioSource;
        private readonly ISoundService soundService;

        public float BaseVolume { get; private set; } = 0f;
        public float BasePitch { get; private set; } = 0f;
        private float previousBaseVolume = 0f;

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

            soundService.SetFadingTimer(this, new SoundVolumeFadingStatus(
                fadingOptions.FadeDuration,
                BaseVolume,
                0,
                () => Stop()
            ));
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

            previousBaseVolume = BaseVolume;
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
            audioSource.volume = BaseVolume * soundService.SeVolume * soundService.MasterVolume;
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