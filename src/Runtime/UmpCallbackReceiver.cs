using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Appegy.Ump
{
    [Preserve]
    internal sealed class UmpCallbackReceiver : MonoBehaviour
    {
        public Action<string> OnMessage;

        [Preserve]
        public void OnUmpMessage(string payload)
        {
            OnMessage?.Invoke(payload);
        }
    }
}
