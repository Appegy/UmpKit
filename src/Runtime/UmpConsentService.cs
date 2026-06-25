using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Appegy.Ump
{
    public sealed class UmpConsentService : IUmpConsentService
    {
        private const string GameObjectName = "UmpKit";
        private const string AndroidClass = "com.appegy.ump.UmpKitConsent";

        private UmpCallbackReceiver _receiver;
        private UniTaskCompletionSource<string> _pending;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _UmpInitialize(int debugGeography, string testDevice);
        [DllImport("__Internal")] private static extern void _UmpShowFormIfRequired();
        [DllImport("__Internal")] private static extern void _UmpShowPrivacyOptionsForm();
        [DllImport("__Internal")] private static extern void _UmpResetConsent();
        [DllImport("__Internal")] private static extern bool _UmpIsPrivacyOptionsRequired();
#endif

        public bool IsPrivacyOptionsRequired
        {
            get
            {
#if UNITY_EDITOR
                return false;
#elif UNITY_ANDROID
                using var cls = new AndroidJavaClass(AndroidClass);
                return cls.CallStatic<bool>("IsPrivacyOptionsRequired");
#elif UNITY_IOS
                return _UmpIsPrivacyOptionsRequired();
#else
                return false;
#endif
            }
        }

        public UniTask<UmpResult> InitializeAsync(UmpInitializeParameters parameters, CancellationToken ct)
        {
#if UNITY_EDITOR
            return UniTask.FromResult(new UmpResult(UmpConsentStatus.NotRequired));
#else
            return CallAsync(ct, () => NativeInitialize(parameters));
#endif
        }

        public UniTask<UmpResult> ShowFormIfRequiredAsync(CancellationToken ct)
        {
#if UNITY_EDITOR
            return UniTask.FromResult(new UmpResult(UmpConsentStatus.NotRequired));
#else
            return CallAsync(ct, NativeShowFormIfRequired);
#endif
        }

        public UniTask<UmpResult> ShowPrivacyOptionsFormAsync(CancellationToken ct)
        {
#if UNITY_EDITOR
            return UniTask.FromResult(new UmpResult(UmpConsentStatus.NotRequired));
#else
            return CallAsync(ct, NativeShowPrivacyOptions);
#endif
        }

        public UniTask ResetConsentAsync(CancellationToken ct)
        {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            using var cls = new AndroidJavaClass(AndroidClass);
            cls.CallStatic("ResetConsent");
#elif UNITY_IOS
            _UmpResetConsent();
#endif
            return UniTask.CompletedTask;
        }

        private async UniTask<UmpResult> CallAsync(CancellationToken ct, Action invokeNative)
        {
            EnsureReceiver();

            var tcs = new UniTaskCompletionSource<string>();
            _pending = tcs;

            using (ct.Register(() => tcs.TrySetCanceled(ct)))
            {
                invokeNative();
                var raw = await tcs.Task;
                return Parse(raw);
            }
        }

        private void EnsureReceiver()
        {
            if (_receiver != null)
            {
                return;
            }

            var go = new GameObject(GameObjectName);
            UnityEngine.Object.DontDestroyOnLoad(go);
            _receiver = go.AddComponent<UmpCallbackReceiver>();
            _receiver.OnMessage = raw => _pending?.TrySetResult(raw);
        }

        private void NativeInitialize(UmpInitializeParameters parameters)
        {
            var geography = (int)parameters.DebugGeography;
            var device = parameters.TestDeviceHashedId ?? string.Empty;
#if UNITY_ANDROID && !UNITY_EDITOR
            using var cls = new AndroidJavaClass(AndroidClass);
            cls.CallStatic("Initialize", geography, device);
#elif UNITY_IOS && !UNITY_EDITOR
            _UmpInitialize(geography, device);
#endif
        }

        private void NativeShowFormIfRequired()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var cls = new AndroidJavaClass(AndroidClass);
            cls.CallStatic("ShowFormIfRequired");
#elif UNITY_IOS && !UNITY_EDITOR
            _UmpShowFormIfRequired();
#endif
        }

        private void NativeShowPrivacyOptions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var cls = new AndroidJavaClass(AndroidClass);
            cls.CallStatic("ShowPrivacyOptionsForm");
#elif UNITY_IOS && !UNITY_EDITOR
            _UmpShowPrivacyOptionsForm();
#endif
        }

        private static UmpResult Parse(string raw)
        {
            var parts = (raw ?? string.Empty).Split(new[] { '|' }, 5);
            var status = (UmpConsentStatus)ParseInt(parts, 1);
            var error = ParseInt(parts, 2);
            if (error == 0)
            {
                return new UmpResult(status);
            }

            var code = (UmpErrorCode)(error - 1);
            var nativeCode = ParseInt(parts, 3);
            var message = parts.Length > 4 ? parts[4] : string.Empty;
            return new UmpResult(status, new UmpError(code, nativeCode, message));
        }

        private static int ParseInt(string[] parts, int index)
        {
            if (parts.Length <= index)
            {
                return 0;
            }

            return int.TryParse(parts[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
        }
    }
}
