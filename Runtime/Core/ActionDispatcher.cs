using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDUI.Core
{
    public class ActionDispatcher
    {
        private readonly Dictionary<string, Action<string>> _handlers = new();

        public void Register(string actionKey, Action<string> handler)
        {
            _handlers[actionKey] = handler;
        }

        public void Dispatch(string fullAction)
        {
            if (string.IsNullOrEmpty(fullAction))
                return;

            var separatorIdx = fullAction.IndexOf(':');
            var key = separatorIdx >= 0 ? fullAction[..separatorIdx] : fullAction;
            var payload = separatorIdx >= 0 ? fullAction[(separatorIdx + 1)..] : string.Empty;

            if (_handlers.TryGetValue(key, out var handler))
                handler(payload);
            else
                Debug.LogWarning($"[SDUI] No handler registered for action: '{key}'");
        }
    }
}
