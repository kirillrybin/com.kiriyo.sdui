using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Core
{
    public interface IUIBuilder
    {
        UniTask BuildPageAsync(JObject pageJson, Transform root, CancellationToken ct = default);
    }

    // Entry point for building a full page/screen from a JSON layout.
    // Handles recursive "children" expansion.
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
            // Clear previous UI
            foreach (Transform child in root)
                Object.Destroy(child.gameObject);
            
            if (root.GetComponent<VerticalLayoutGroup>() == null)
            {
                var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.spacing = 8f;
                layout.padding = new RectOffset(16, 16, 16, 16);
            }

            var children = pageJson["children"] as JArray ?? new JArray(pageJson);

            foreach (var node in children)
            {
                if (node is not JObject componentJson)
                    continue;

                var go = await _factory.BuildAsync(componentJson, root, ct);

                // Recursively build nested children
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
