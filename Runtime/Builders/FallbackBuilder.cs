using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	[UsedImplicitly]
	public class FallbackBuilder:  Core.IComponentBuilder
	{
		public string Type => "__fallback__";
		
		public UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var type = json["type"]?.Value<string>() ?? "unknown";

			var go    = new GameObject($"Fallback_{type}", typeof(RectTransform));
			go.transform.SetParent(parent, false);

			var image = go.AddComponent<Image>();
			image.color = new Color(0.8f, 0.1f, 0.1f, 0.6f);

			var layout = go.AddComponent<LayoutElement>();
			layout.minHeight = 40f;

			var textGo = new GameObject("Label", typeof(RectTransform));
			textGo.transform.SetParent(go.transform, false);

			var rect = textGo.GetComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			var text = textGo.AddComponent<TextMeshProUGUI>();
			text.text = $"[unknown: {type}]";
			text.fontSize = 12f;
			text.color = Color.white;
			text.alignment = TextAlignmentOptions.Center;

			return UniTask.FromResult(go);
		}
	}
}