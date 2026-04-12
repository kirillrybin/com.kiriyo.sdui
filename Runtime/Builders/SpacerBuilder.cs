using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SDUI.Core;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	[UsedImplicitly]
	public class SpacerBuilder: IComponentBuilder
	{
		public string Type => "spacer";
		public UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var go = new GameObject("Spacer");
			go.transform.SetParent(parent, false);

			var rt = go.AddComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector2.zero;

			var layoutElement = go.AddComponent<LayoutElement>();
			layoutElement.flexibleWidth  = json["flexibleWidth"]?.Value<float>()  ?? 1f;
			layoutElement.flexibleHeight = json["flexibleHeight"]?.Value<float>() ?? 0f;
			
			if (json["minWidth"] is { } minW)
				layoutElement.minWidth = minW.Value<float>();

			if (json["minHeight"] is { } minH)
				layoutElement.minHeight = minH.Value<float>();

			return UniTask.FromResult(go);
		}
	}
}