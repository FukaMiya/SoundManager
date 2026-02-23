using UnityEngine;
using UnityEngine.Pool;

namespace Early.SoundManager
{
    public sealed class SoundManager : ISoundService, ITickable
    {
        private const int defaultPoolCapacity = 10;
        private const int defaultPoolMaxSize = 20;

        private readonly SoundRegistory soundRegistory;
        private readonly ObjectPool<AudioSource> availableAudioSources;
        private IBgmHandle currentBgm;
        private IBgmHandle nextBgm;
        private float bgmSwitchTimer = 0f;
        private float bgmSwitchDuration = 0f;
        private float targetSwitchStartVolume = 0f;
        private float sourceSwitchEndVolume = 0f;

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
            this.soundRegistory = soundRegistory;
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

        public void Tick()
        {
            CrossFadeBgm();
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
            var audioSource = PlaySeInternal(GetAvailableAudioSource(), clip, options);
            var handle = new SeHandle(audioSource, this);
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

            var audioSource = PlayBgmInternal(GetAvailableAudioSource(), clip, options);
            currentBgm = new BgmHandle(audioSource, this);
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

            if (bgmSwitchDuration > 0f)
            {
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
#endregion

#region Private Helper Methods
        private bool TryGetAudioClipByKey(string key, out AudioClip clip)
        {
            if (soundRegistory == null || soundRegistory.SoundEntries == null)
            {
                Debug.LogWarning("Sound registory is not set or empty.");
                clip = null;
                return false;
            }

            var entry = System.Array.Find(soundRegistory.SoundEntries, e => e.key == key);
            if (entry != null)
            {
                clip = entry.clip;
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
    
        private AudioSource PlaySeInternal(AudioSource audioSource, AudioClip clip, SoundOptions options)
        {
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.volume = options.Volume * SeVolume * MasterVolume;
            audioSource.pitch = options.Pitch;
            audioSource.spatialBlend = options.Spatialize ? 1.0f : 0.0f;
            audioSource.rolloffMode = options.RolloffMode;
            audioSource.minDistance = options.MinDistance;
            audioSource.maxDistance = options.MaxDistance;
            audioSource.transform.position = options.Position;
            audioSource.Play();
            return audioSource;
        }

        private AudioSource PlayBgmInternal(AudioSource audioSource, AudioClip clip, SoundOptions options)
        {
            audioSource.loop = true;
            audioSource.clip = clip;
            audioSource.volume = options.Volume * BgmVolume * MasterVolume;
            audioSource.pitch = options.Pitch;
            audioSource.spatialBlend = options.Spatialize ? 1.0f : 0.0f;
            audioSource.rolloffMode = options.RolloffMode;
            audioSource.minDistance = options.MinDistance;
            audioSource.maxDistance = options.MaxDistance;
            audioSource.transform.position = options.Position;
            audioSource.Play();
            return audioSource;
        }

        private IBgmHandle SwitchBgmInternal(AudioSource audioSource, AudioClip clip, SoundOptions options, SoundFadingOptions crossFadingOptions)
        {
            bgmSwitchDuration = crossFadingOptions.FadeDuration;
            bgmSwitchTimer = 0f;
            targetSwitchStartVolume = currentBgm.Volume;
            sourceSwitchEndVolume = options.Volume * BgmVolume * MasterVolume;
            nextBgm = new BgmHandle(audioSource, this);

            audioSource.loop = true;
            audioSource.clip = clip;
            audioSource.volume = 0;
            audioSource.pitch = options.Pitch;
            audioSource.spatialBlend = options.Spatialize ? 1.0f : 0.0f;
            audioSource.rolloffMode = options.RolloffMode;
            audioSource.minDistance = options.MinDistance;
            audioSource.maxDistance = options.MaxDistance;
            audioSource.transform.position = options.Position;
            audioSource.Play();
            return new BgmHandle(audioSource, this);
        }

        private void CrossFadeBgm()
        {
            if (bgmSwitchDuration <= 0) return;

            bgmSwitchTimer += Time.deltaTime;
            var t = Mathf.InverseLerp(0, bgmSwitchDuration, bgmSwitchTimer);
            var sVolume = Mathf.Lerp(targetSwitchStartVolume, 0, t);
            var tVolume = Mathf.Lerp(0, sourceSwitchEndVolume, t);
            currentBgm.SetVolumeRaw(sVolume);
            nextBgm.SetVolumeRaw(tVolume);
            if (bgmSwitchTimer >= bgmSwitchDuration)
            {
                bgmSwitchTimer = 0f;
                bgmSwitchDuration = 0f;
                availableAudioSources.Release(currentBgm.Release());
                currentBgm = nextBgm;
                nextBgm = null;
            }
        }
#endregion

#region Object Pooling Callbacks
        private AudioSource OnCreatePoolObject()
        {
            var audioSource = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
            return audioSource;
        }

        private void OnGetFromPool(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(AudioSource audioSource)
        {
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