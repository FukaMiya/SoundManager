using System.Threading;
using UnityEngine;

#if UNITASK_SUPPORT
using AsyncResult = Cysharp.Threading.Tasks.UniTask;
#elif UNITY_2023_1_OR_NEWER
using AsyncResult = UnityEngine.Awaitable;
#else
using System.Threading.Tasks;
using AsyncResult = System.Threading.Tasks.ValueTask;
#endif

namespace Early.SoundManager
{
    public static class SoundServiceAsyncExtensions
    {
        #region PlaySeAsync

        public static AsyncResult PlaySeAsync(this ISoundService service, AudioClip clip, CancellationToken cancellationToken = default)
            => SeCompletionAsync(service.PlaySe(clip), cancellationToken);

        public static AsyncResult PlaySeAsync(this ISoundService service, AudioClip clip, SoundOptions options, CancellationToken cancellationToken = default)
            => SeCompletionAsync(service.PlaySe(clip, options), cancellationToken);

        public static AsyncResult PlaySeAsync(this ISoundService service, string key, CancellationToken cancellationToken = default)
            => SeCompletionAsync(service.PlaySe(key), cancellationToken);

        public static AsyncResult PlaySeAsync(this ISoundService service, string key, SoundOptions options, CancellationToken cancellationToken = default)
            => SeCompletionAsync(service.PlaySe(key, options), cancellationToken);

        #endregion

        #region SwitchBgmAsync

        public static AsyncResult SwitchBgmAsync(this ISoundService service, AudioClip clip, SoundFadingOptions fadingOptions, out IBgmHandle result, BgmTrackId trackId = default, CancellationToken cancellationToken = default)
            => (result = service.SwitchBgm(clip, fadingOptions, trackId)).WaitForCurrentFadeAsync(fadingOptions.CancelBehaviour, cancellationToken);

        public static AsyncResult SwitchBgmAsync(this ISoundService service, AudioClip clip, SoundOptions options, SoundFadingOptions fadingOptions, out IBgmHandle result, BgmTrackId trackId = default, CancellationToken cancellationToken = default)
            => (result = service.SwitchBgm(clip, options, fadingOptions, trackId)).WaitForCurrentFadeAsync(fadingOptions.CancelBehaviour, cancellationToken);

        public static AsyncResult SwitchBgmAsync(this ISoundService service, string key, SoundFadingOptions fadingOptions, out IBgmHandle result, BgmTrackId trackId = default, CancellationToken cancellationToken = default)
            => (result = service.SwitchBgm(key, fadingOptions, trackId)).WaitForCurrentFadeAsync(fadingOptions.CancelBehaviour, cancellationToken);

        public static AsyncResult SwitchBgmAsync(this ISoundService service, string key, SoundOptions options, SoundFadingOptions fadingOptions, out IBgmHandle result, BgmTrackId trackId = default, CancellationToken cancellationToken = default)
            => (result = service.SwitchBgm(key, options, fadingOptions, trackId)).WaitForCurrentFadeAsync(fadingOptions.CancelBehaviour, cancellationToken);

        #endregion

        #region Private Helpers

        private static AsyncResult SeCompletionAsync(ISeHandle handle, CancellationToken cancellationToken)
        {
#if UNITASK_SUPPORT
            return handle.ToUniTask(cancellationToken);
#elif UNITY_2023_1_OR_NEWER
            return handle.ToAwaitable(cancellationToken);
#else
            return handle.ToValueTask(cancellationToken);
#endif
        }

        #endregion
    }
}
