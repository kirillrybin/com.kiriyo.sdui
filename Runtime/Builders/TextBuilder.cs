using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

namespace SDUI.Builders
{
	public sealed class TextPrefab { public TMP_Text Value; }

	[UsedImplicitly]
	public class TextBuilder : Core.IComponentBuilder
	{
		public string Type => "text";

		private readonly TMP_Text _prefab;

		public TextBuilder(TextPrefab prefab)
		{
			_prefab = prefab.Value;
		}

		public UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var go   = Object.Instantiate(_prefab, parent).gameObject;
			var text = go.GetComponent<TMP_Text>();

			text.text = json["value"]?.Value<string>() ?? string.Empty;

			if (json["style"]?.Value<string>() is { } style)
				ApplyStyle(text, style);

			// textColor overrides style color if both are present
			if (json["textColor"]?.Value<string>() is { } hex &&
				ColorUtility.TryParseHtmlString(hex, out var textColor))
				text.color = textColor;

			if (json["align"]?.Value<string>() is { } align)
				ApplyAlign(text, align);

			return UniTask.FromResult(go);
		}

		private static void ApplyStyle(TMP_Text text, string style)
		{
			switch (style)
			{
				case "header":
					text.fontSize = 32;
					text.fontStyle = FontStyles.Bold;
					break;
				case "body":
					text.fontSize = 18;
					break;
				case "caption":
					text.fontSize = 14;
					text.color = new Color(0.6f, 0.6f, 0.6f);
					break;
			}
		}

		private static void ApplyAlign(TMP_Text text, string align)
		{
			text.alignment = align switch
			{
				"left"   => TextAlignmentOptions.Left,
				"center" => TextAlignmentOptions.Center,
				"right"  => TextAlignmentOptions.Right,
				_        => text.alignment   // unknown value — keep prefab default
			};
		}
	}
}