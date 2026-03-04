using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
#if !UNITASK_SUPPORT && !UNITY_2023_1_OR_NEWER
using System.Threading.Tasks;
#endif

namespace Early.SoundManager
{
    public sealed class SeHandleAwaiter : INotifyCompletion
    {
        private readonly ISeHandle handle;

        public SeHandleAwaiter(ISeHandle handle)
        {
            this.handle = handle;
        }

        public bool IsCompleted => handle.IsPlaying == false;

        public void GetResult()
        {

        }

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
                return;
            }

            void OnHandleCompleted()
            {
                handle.OnCompleted -= OnHandleCompleted;
                continuation();
            }
            handle.OnCompleted += OnHandleCompleted;
        }
    }

    public static class SeHandleAsyncExtensions
    {
        public static SeHandleAwaiter GetAwaiter(this ISeHandle handle) => new (handle);

#if UNITASK_SUPPORT
        public static Cysharp.Threading.Tasks.UniTask ToUniTask(this ISeHandle handle)
        {
            if (!handle.IsPlaying) return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            handle.OnCompleted += () => tcs.TrySetResult();
            return tcs.Task;
        }

        public static Cysharp.Threading.Tasks.UniTask ToUniTask(this ISeHandle handle, CancellationToken cancellationToken)
        {
            if (!handle.IsPlaying) return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            handle.OnCompleted += () => tcs.TrySetResult();
            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            });
            return tcs.Task;
        }
#elif UNITY_2023_1_OR_NEWER
        public static Awaitable ToAwaitable(this ISeHandle handle)
        {
            var source = new AwaitableCompletionSource();
            if (!handle.IsPlaying)
            {
                source.SetResult();
                return source.Awaitable;
            }
            handle.OnCompleted += () => source.SetResult();
            return source.Awaitable;
        }

        public static Awaitable ToAwaitable(this ISeHandle handle, CancellationToken cancellationToken)
        {
            var source = new AwaitableCompletionSource();
            if (!handle.IsPlaying)
            {
                source.SetResult();
                return source.Awaitable;
            }
            handle.OnCompleted += () => source.SetResult();
            cancellationToken.Register(() =>
            {
                source.SetCanceled();
            });
            return source.Awaitable;
        }
#else
        public static System.Threading.Tasks.ValueTask ToValueTask(this ISeHandle handle)
        {
            if (!handle.IsPlaying) return default;
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            handle.OnCompleted += () => tcs.TrySetResult(true);
            return new System.Threading.Tasks.ValueTask(tcs.Task);
        }

        public static System.Threading.Tasks.ValueTask ToValueTask(this ISeHandle handle, CancellationToken cancellationToken)
        {
            if (!handle.IsPlaying) return default;
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            handle.OnCompleted += () => tcs.TrySetResult(true);
            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            });
            return new System.Threading.Tasks.ValueTask(tcs.Task);
        }
#endif
    }
}
