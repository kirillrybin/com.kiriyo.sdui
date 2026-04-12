using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDUI.Core
{
    // Builders register actions by string key; game code subscribes handlers.
    // Example actions: "open_shop", "close_popup", "claim_reward:daily"
    public class ActionDispatcher
    {
        private readonly Dictionary<string, Action<string>> _handlers = new();

        // Register a handler prefix. "claim_reward" handles "claim_reward:daily", etc.
        public void Register(string actionKey, Action<string> handler)
        {
            _handlers[actionKey] = handler;
        }

        public void Dispatch(string fullAction)
        {
            if (string.IsNullOrEmpty(fullAction))
                return;

            // Support "key:payload" format
            var separatorIdx = fullAction.IndexOf(':');
            var key = separatorIdx >= 0 ? fullAction[..separatorIdx] : fullAction;
            var payload = separatorIdx >= 0 ? fullAction[(separatorIdx + 1)..] : string.Empty;

            if (_handlers.TryGetValue(key, out var handler))
            {
                handler(payload);
            }
            else
            {
                Debug.LogWarning($"[SDUI] No handler registered for action: '{key}'");
            }
        }
    }
}
