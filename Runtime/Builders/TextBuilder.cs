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

			if (json["textColor"]?.Value<string>() is { } hex &&
				ColorUtility.TryParseHtmlString(hex, out var textColor))
				text.color = textColor;

			if (json["align"]?.Value<string>() is { } align)
				ApplyAlign(text, align);

			return UniTask.FromResult(go);
		}

		private static void ApplyStyle(TMP_Text text, string styleName)
		{
			var styleSheet = TMP_Settings.defaultStyleSheet;
			if (styleSheet == null)
			{
				Debug.LogWarning("[SDUI] TMP default style sheet is not assigned in TMP Settings");
				return;
			}

			var style = styleSheet.GetStyle(styleName);
			if (style != null)
				text.textStyle = style;
			else
				Debug.LogWarning($"[SDUI] TMP style not found: '{styleName}'");
		}

		private static void ApplyAlign(TMP_Text text, string align)
		{
			text.alignment = align switch
			{
				"left"   => TextAlignmentOptions.Left,
				"center" => TextAlignmentOptions.Center,
				"right"  => TextAlignmentOptions.Right,
				_        => text.alignment
			};
		}
	}
}