using UnityEngine;

namespace Early.SoundManager
{
    public interface ISoundService : System.IDisposable
    {
        float MasterVolume { get; set; }
        float BgmVolume { get; set; }
        float SeVolume { get; set; }
        event System.Action OnTicked;
        ISeHandle PlaySe(AudioClip clip);
        ISeHandle PlaySe(AudioClip clip, SoundOptions options);
        ISeHandle PlaySe(string key);
        ISeHandle PlaySe(string key, SoundOptions options);
        IBgmHandle PlayBgm(AudioClip clip);
        IBgmHandle PlayBgm(AudioClip clip, SoundOptions options);
        IBgmHandle PlayBgm(string key);
        IBgmHandle PlayBgm(string key, SoundOptions options);
        IBgmHandle SwitchBgm(AudioClip clip);
        IBgmHandle SwitchBgm(AudioClip clip, SoundOptions options);
        IBgmHandle SwitchBgm(AudioClip clip, SoundFadingOptions fadingOptions);
        IBgmHandle SwitchBgm(AudioClip clip, SoundOptions options, SoundFadingOptions fadingOptions);
        IBgmHandle SwitchBgm(string key);
        IBgmHandle SwitchBgm(string key, SoundOptions options);
        IBgmHandle SwitchBgm(string key, SoundFadingOptions fadingOptions);
        IBgmHandle SwitchBgm(string key, SoundOptions options, SoundFadingOptions fadingOptions);
    }

    public interface ISoundHandle : System.IDisposable
    {
        float Volume { get; }
        float Pitch { get; }
        bool IsPlaying { get; }
        bool IsValid { get; }
        event System.Action OnPaused;
        event System.Action OnResumed;
        event System.Action OnVolumeChanged;
        event System.Action OnPitchChanged;
        void Pause();
        void Pause(SoundFadingOptions fadingOptions);
        void Resume();
        void Resume(SoundFadingOptions fadingOptions);
        void SetVolumeRaw(float volume);
        void SetVolume(float volume);
        void SetVolume(float volume, SoundFadingOptions fadeingOptions);
        void SetPitch(float pitch);
        void SetPitch(float pitch, SoundFadingOptions fadeingOptions);
        AudioSource Release();
    }

    public interface ISeHandle : ISoundHandle
    {
        event System.Action OnCompleted;
        void Stop();
        void Stop(SoundFadingOptions fadingOptions);
    }

    public interface IBgmHandle : ISoundHandle
    {
    }
}