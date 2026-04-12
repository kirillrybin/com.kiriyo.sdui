using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SDUI.Builders;
using UnityEngine;

namespace SDUI.Core
{
    public class ComponentFactory
    {
        private readonly Dictionary<string, IComponentBuilder> _builders = new();
        private readonly FallbackBuilder _fallback = new();

        // VContainer resolves IReadOnlyList<IComponentBuilder> automatically
        // when multiple types are registered via AsImplementedInterfaces()
        public ComponentFactory(IReadOnlyList<IComponentBuilder> builders)
        {
            foreach (var builder in builders)
                _builders[builder.Type] = builder;
        }

        public async UniTask<GameObject> BuildAsync(JObject json, Transform parent,
            System.Threading.CancellationToken ct = default)
        {
            var type = json["type"]?.Value<string>();

            if (type == null)
            {
                Debug.LogWarning("[SDUI] Component JSON missing 'type' field — rendering fallback");
                return await _fallback.BuildAsync(json, parent, ct);
            }

            if (!_builders.TryGetValue(type, out var builder))
            {
                Debug.LogWarning($"[SDUI] Unknown component type: '{type}' — rendering fallback");
                return await _fallback.BuildAsync(json, parent, ct);
            }

            return await builder.BuildAsync(json, parent, ct);
        }
    }
}
