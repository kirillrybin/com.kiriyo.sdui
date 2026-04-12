using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	public sealed class ButtonPrefab
	{
		public Button Value;
	}

	[UsedImplicitly]
	public class ButtonBuilder : Core.IComponentBuilder
	{
		public string Type => "button";

		private const float DefaultMinHeight = 44f;

		private readonly Button _prefab;
		private readonly Core.ActionDispatcher _dispatcher;

		public ButtonBuilder(ButtonPrefab prefab, Core.ActionDispatcher dispatcher)
		{
			_prefab = prefab.Value;
			_dispatcher = dispatcher;
		}

		public UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var go = Object.Instantiate(_prefab, parent).gameObject;
			var label = json["label"]?.Value<string>() ?? string.Empty;
			var action = json["action"]?.Value<string>() ?? string.Empty;

			go.GetComponentInChildren<TMP_Text>().text = label;
			go.GetComponent<Button>().onClick.AddListener(() => _dispatcher.Dispatch(action));
			go.name = $"Btn_{label}";

			if (json["backgroundColor"] != null && ColorUtility.TryParseHtmlString(json["backgroundColor"].Value<string>(), out var bgColor))
			{
				go.GetComponent<Image>().color = bgColor;
			}

			if (json["textColor"] != null && ColorUtility.TryParseHtmlString(json["textColor"].Value<string>(), out var textColor))
			{
				go.GetComponentInChildren<TMP_Text>().color = textColor;
			}

			ApplyLayout(go, json);

			return UniTask.FromResult(go);
		}

		private static void ApplyLayout(GameObject go, JObject json)
		{
			var layoutElement = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();

			var width  = json["width"]?.Value<float>();
			var height = json["height"]?.Value<float>();

			if (width.HasValue)
			{
				layoutElement.preferredWidth = width.Value;
				layoutElement.minWidth       = width.Value;
				layoutElement.flexibleWidth  = 0f;
			}
			else
			{
				layoutElement.flexibleWidth = 1f;
			}

			if (height.HasValue)
			{
				layoutElement.preferredHeight = height.Value;
				layoutElement.minHeight       = height.Value;
				layoutElement.flexibleHeight  = 0f;
			}
			else
			{
				layoutElement.minHeight       = DefaultMinHeight;
				layoutElement.preferredHeight = DefaultMinHeight;
			}
		}
	}
}