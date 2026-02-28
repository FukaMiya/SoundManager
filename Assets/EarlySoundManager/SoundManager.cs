using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Early.SoundManager
{
    public sealed class SoundManager : ISoundService, ITickable
    {
        private const int defaultPoolCapacity = 10;
        private const int defaultPoolMaxSize = 20;

        private readonly ObjectPool<AudioSource> availableAudioSources;
        private readonly Dictionary<string, AudioClip> audioClipCache = new ();
        private readonly Dictionary<ISoundHandle, ISoundFadingStatus> fadingTimers = new ();
        private readonly List<ISoundHandle> handlesToRemove = new ();
        private IBgmHandle currentBgm;
        private IBgmHandle nextBgm;

        public SoundManager()
        {
            this.availableAudioSources = new ObjectPool<AudioSource>
            (
                createFunc: OnCreatePoolObject,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: defaultPoolCapacity,
                maxSize: defaultPoolMaxSize
            );
        }
        public SoundManager(SoundRegistory soundRegistory)
        {
            this.availableAudioSources = new ObjectPool<AudioSource>
            (
                createFunc: OnCreatePoolObject,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: defaultPoolCapacity,
                maxSize: defaultPoolMaxSize
            );

            SetupSoundRegistory(soundRegistory);
        }

        public void Tick()
        {
            Fade();
            OnTicked?.Invoke();
        }

#region ISoundService Implementation
        public float MasterVolume { get; set; } = 1.0f;
        public float SeVolume { get; set; } = 1.0f;
        public float BgmVolume { get; set; } = 1.0f;

        public event System.Action OnTicked;

        public ISeHandle PlaySe(AudioClip clip)
        {
            return PlaySe(clip, SoundOptions.Default);
        }

        public ISeHandle PlaySe(AudioClip clip, SoundOptions options)
        {
            var handle = PlaySeInternal(GetAvailableAudioSource(), clip, options);
            handle.OnCompleted += () => availableAudioSources.Release(handle.Release());
            return handle;
        }

        public ISeHandle PlaySe(string key)
        {
            return PlaySe(key, SoundOptions.Default);
        }

        public ISeHandle PlaySe(string key, SoundOptions options)
        {
            if (TryGetAudioClipByKey(key, out var clip))
            {
                return PlaySe(clip, options);
            }
            else
            {
                return new SeHandle();
            }
        }

        public IBgmHandle PlayBgm(AudioClip clip)
        {
            return PlayBgm(clip, SoundOptions.Default);
        }

        public IBgmHandle PlayBgm(AudioClip clip, SoundOptions options)
        {
            if (currentBgm != null && currentBgm.IsValid)
            {
                return SwitchBgm(clip, options);
            }

            currentBgm = PlayBgmInternal(GetAvailableAudioSource(), clip, options);
            return currentBgm;
        }

        public IBgmHandle PlayBgm(string key)
        {
            return PlayBgm(key, SoundOptions.Default);
        }

        public IBgmHandle PlayBgm(string key, SoundOptions options)
        {
            if (TryGetAudioClipByKey(key, out var clip))
            {
                return PlayBgm(clip, options);
            }
            else
            {
                return new BgmHandle();
            }
        }

        public IBgmHandle SwitchBgm(AudioClip clip)
        {
            return SwitchBgm(clip, SoundOptions.Default);
        }

        public IBgmHandle SwitchBgm(AudioClip clip, SoundOptions options)
        {
            if (currentBgm != null && currentBgm.IsValid)
            {
                availableAudioSources.Release(currentBgm.Release());
            }

            return PlayBgm(clip, options);
        }

        public IBgmHandle SwitchBgm(AudioClip clip, SoundFadingOptions fadingOptions)
        {
            return SwitchBgm(clip, SoundOptions.Default, fadingOptions);
        }

        public IBgmHandle SwitchBgm(AudioClip clip, SoundOptions options, SoundFadingOptions fadingOptions)
        {
            if (currentBgm == null || !currentBgm.IsValid || fadingOptions.FadeDuration <= 0f)
            {
                return PlayBgm(clip, options);
            }

            if (fadingTimers.TryGetValue(currentBgm, out var _))
            {
                fadingTimers.Remove(currentBgm);
                if (nextBgm != null)
                {
                    
                }
                availableAudioSources.Release(currentBgm.Release());
                currentBgm = nextBgm;
                nextBgm = null;
            }

            return SwitchBgmInternal(GetAvailableAudioSource(), clip, options, fadingOptions);
        }

        public IBgmHandle SwitchBgm(string key)
        {
            return SwitchBgm(key, SoundOptions.Default);
        }

        public IBgmHandle SwitchBgm(string key, SoundOptions options)
        {
            if (TryGetAudioClipByKey(key, out var clip))
            {
                return SwitchBgm(clip, options);
            }
            else
            {
                return new BgmHandle();
            }
        }

        public IBgmHandle SwitchBgm(string key, SoundFadingOptions fadingOptions)
        {
            return SwitchBgm(key, SoundOptions.Default, fadingOptions);
        }

        public IBgmHandle SwitchBgm(string key, SoundOptions options, SoundFadingOptions crossFadingOptions)
        {
            if (TryGetAudioClipByKey(key, out var clip))
            {
                return SwitchBgm(clip, options, crossFadingOptions);
            }
            else
            {
                return new BgmHandle();
            }
        }

        void ISoundService.SetFadingTimer(ISoundHandle handle, ISoundFadingStatus fadingStatus)
        {
            if (handle != null && handle.IsValid)
            {
                fadingTimers[handle] = fadingStatus;
            }
        }

        public void Dispose()
        {
            if (currentBgm != null && currentBgm.IsValid)
            {
                availableAudioSources.Release(currentBgm.Release());
                currentBgm = null;
            }
            if (nextBgm != null && nextBgm.IsValid)
            {
                availableAudioSources.Release(nextBgm.Release());
                nextBgm = null;
            }
            availableAudioSources.Clear();
            audioClipCache.Clear();
            fadingTimers.Clear();
        }
#endregion

#region Private Helper Methods
        private bool TryGetAudioClipByKey(string key, out AudioClip clip)
        {
            if (audioClipCache.Count == 0)
            {
                Debug.LogWarning("Audio clip cache is not initialized.");
                clip = null;
                return false;
            }

            if (audioClipCache.TryGetValue(key, out clip))
            {
                return true;
            }
            else
            {
                Debug.LogWarning($"Sound with key '{key}' not found in registory.");
                clip = null;
                return false;
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            return availableAudioSources.Get();
        }

        private AudioSource SetAudioSourceParams(ISoundHandle handle, AudioSource audioSource, SoundOptions options)
        {
            handle.SetVolume(options.Volume);
            handle.SetPitch(options.Pitch);
            audioSource.spatialBlend = options.Spatialize ? 1.0f : 0.0f;
            audioSource.rolloffMode = options.RolloffMode;
            audioSource.minDistance = options.MinDistance;
            audioSource.maxDistance = options.MaxDistance;
            audioSource.transform.position = options.Position;
            return audioSource;
        }
    
        private ISeHandle PlaySeInternal(AudioSource audioSource, AudioClip clip, SoundOptions options)
        {
            var handle = new SeHandle(audioSource, this);
            audioSource.loop = false;
            audioSource.clip = clip;
            SetAudioSourceParams(handle, audioSource, options);
            audioSource.Play();
            return handle;
        }

        private IBgmHandle PlayBgmInternal(AudioSource audioSource, AudioClip clip, SoundOptions options)
        {
            var handle = new BgmHandle(audioSource, this);
            audioSource.loop = true;
            audioSource.clip = clip;
            SetAudioSourceParams(handle, audioSource, options);
            audioSource.Play();
            return handle;
        }

        private IBgmHandle SwitchBgmInternal(AudioSource audioSource, AudioClip clip, SoundOptions options, SoundFadingOptions crossFadingOptions)
        {
            nextBgm = new BgmHandle(audioSource, this);
            fadingTimers[nextBgm] = new SoundVolumeFadingStatus(crossFadingOptions.FadeDuration, 0, options.Volume, null);
            fadingTimers[currentBgm] = new SoundVolumeFadingStatus(
                crossFadingOptions.FadeDuration,
                currentBgm.Volume,
                0,
                () =>
                {
                    if (currentBgm != null)
                    {
                        availableAudioSources.Release(currentBgm.Release());
                    }
                    currentBgm = nextBgm;
                    nextBgm = null;
                }
            );

            audioSource.loop = true;
            audioSource.clip = clip;
            SetAudioSourceParams(nextBgm, audioSource, options);
            audioSource.Play();
            return nextBgm;
        }

        private void Fade()
        {
            handlesToRemove.Clear();
            foreach (var (handle, fadingStatus) in fadingTimers)
            {
                if (fadingStatus.Duration <= 0) continue;

                fadingStatus.Timer += Time.deltaTime;
                var t = Mathf.InverseLerp(0, fadingStatus.Duration, fadingStatus.Timer);
                var newValue = Mathf.Lerp(fadingStatus.StartValue, fadingStatus.EndValue, t);
                if (handle is ISeHandle seHandle)
                {
                    seHandle.SetVolume(newValue);
                }
                else if (handle is IBgmHandle bgmHandle)
                {
                    bgmHandle.SetVolume(newValue);
                }

                if (fadingStatus.Timer >= fadingStatus.Duration)
                {
                    handlesToRemove.Add(handle);
                }
            }

            foreach (var handle in handlesToRemove)
            {
                fadingTimers[handle].OnCompleted?.Invoke();
                fadingTimers.Remove(handle);
            }
        }

        private void SetupSoundRegistory(SoundRegistory soundRegistory)
        {
            if (soundRegistory == null || soundRegistory.SoundEntries == null)
            {
                Debug.LogWarning("Sound registory is not set or empty.");
                return;
            }

            foreach (var entry in soundRegistory.SoundEntries)
            {
                if (!audioClipCache.ContainsKey(entry.key))
                {
                    audioClipCache.Add(entry.key, entry.clip);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key '{entry.key}' found in sound registory. Skipping.");
                }
            }
        }
#endregion

#region Object Pooling Callbacks
        private AudioSource OnCreatePoolObject()
        {
            var audioSource = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(audioSource.gameObject);
            return audioSource;
        }

        private void OnGetFromPool(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;

            audioSource.Stop();
            audioSource.clip = null;
            audioSource.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(AudioSource audioSource)
        {
            if (audioSource == null) return;

            Object.Destroy(audioSource.gameObject);
        }
#endregion
    }

    public interface ITickable
#if VCONTAINER_SUPPORT
    : VContainer.Unity.ITickable
    {
    }
#else
    {
        void Tick();
    }
#endif
}