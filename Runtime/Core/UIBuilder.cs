using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SDUI.Core
{
    [UsedImplicitly]
    public class UIBuilder : IUIBuilder
    {
        private readonly ComponentFactory _factory;

        public UIBuilder(ComponentFactory factory)
        {
            _factory = factory;
        }

        public async UniTask BuildPageAsync(JObject pageJson, Transform root, CancellationToken ct = default)
        {
            if (root.GetComponent<VerticalLayoutGroup>() == null)
                throw new InvalidOperationException(
                    $"[SDUI] '{root.name}' is missing VerticalLayoutGroup.");
            
            // Clear previous UI
            foreach (Transform child in root)
                Object.Destroy(child.gameObject);
            
            var children = pageJson["children"] as JArray ?? new JArray(pageJson);

            foreach (var node in children)
            {
                if (node is not JObject componentJson)
                    continue;

                var go = await _factory.BuildAsync(componentJson, root, ct);

                var nested = componentJson["children"] as JArray;
                if (nested != null && go != null)
                    await BuildChildrenAsync(nested, go.transform, ct);
            }
        }

        private async UniTask BuildChildrenAsync(JArray children, Transform parent, CancellationToken ct)
        {
            foreach (var node in children)
            {
                if (node is not JObject componentJson)
                    continue;

                var go = await _factory.BuildAsync(componentJson, parent, ct);

                var nested = componentJson["children"] as JArray;
                if (nested != null && go != null)
                    await BuildChildrenAsync(nested, go.transform, ct);
            }
        }
    }
}
