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
	public class PanelBuilder : IComponentBuilder
	{
		public string Type => "container";

		public UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var name = json["id"]?.Value<string>() ?? json["name"]?.Value<string>() ?? "Container";
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);

			var rt = go.AddComponent<RectTransform>();
			var orientation = json["orientation"]?.Value<string>() ?? "vertical";
			var spacing = json["spacing"]?.Value<float>() ?? 8f;
			var childAlignment = ParseChildAlignment(json["childAlignment"]?.Value<string>());

			if (orientation == "horizontal")
			{
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.one;
				rt.sizeDelta = Vector2.zero;

				var hGroup = go.AddComponent<HorizontalLayoutGroup>();
				hGroup.childControlWidth = true;
				hGroup.childForceExpandWidth = false;
				hGroup.childControlHeight = true;
				hGroup.childAlignment = childAlignment;
				hGroup.spacing = spacing;
			}
			else
			{
				rt.anchorMin = new Vector2(0f, 1f);
				rt.anchorMax = new Vector2(1f, 1f);
				rt.sizeDelta = Vector2.zero;
				rt.pivot = new Vector2(0.5f, 1f);

				var vGroup = go.AddComponent<VerticalLayoutGroup>();
				vGroup.childControlWidth = true;
				vGroup.childForceExpandWidth = false;
				vGroup.childControlHeight = true;
				vGroup.childAlignment = childAlignment;
				vGroup.spacing = spacing;

				var csf = go.AddComponent<ContentSizeFitter>();
				csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
				csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			}

			ApplyLayout(go, json);

			return UniTask.FromResult(go);
		}

		private static TextAnchor ParseChildAlignment(string value) =>
			value switch
			{
				"upperLeft" => TextAnchor.UpperLeft,
				"upperCenter" => TextAnchor.UpperCenter,
				"upperRight" => TextAnchor.UpperRight,
				"middleLeft" => TextAnchor.MiddleLeft,
				"middleCenter" => TextAnchor.MiddleCenter,
				"middleRight" => TextAnchor.MiddleRight,
				"lowerLeft" => TextAnchor.LowerLeft,
				"lowerCenter" => TextAnchor.LowerCenter,
				"lowerRight" => TextAnchor.LowerRight,
				_ => TextAnchor.UpperLeft // default
			};

		private static void ApplyLayout(GameObject go, JObject json)
		{
			var width = json["width"]?.Value<float>();
			var height = json["height"]?.Value<float>();
			var flexibleWidth = json["flexibleWidth"]?.Value<float>();

			if (!width.HasValue && !height.HasValue && !flexibleWidth.HasValue)
				return;

			var layoutElement = go.AddComponent<LayoutElement>();

			if (width.HasValue)
			{
				layoutElement.preferredWidth = width.Value;
				layoutElement.minWidth = width.Value;
				layoutElement.flexibleWidth = 0f;
			}
			else if (flexibleWidth.HasValue)
			{
				layoutElement.flexibleWidth = flexibleWidth.Value;
			}

			if (height.HasValue)
			{
				layoutElement.preferredHeight = height.Value;
				layoutElement.minHeight = height.Value;
				layoutElement.flexibleHeight = 0f;
			}
		}
	}
}