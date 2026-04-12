using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	public sealed class ImagePrefab { public Image Value; }

	[UsedImplicitly]
	public class ImageBuilder : Core.IComponentBuilder
	{
		public string Type => "image";

		private readonly Image _prefab;

		public ImageBuilder(ImagePrefab prefab)
		{
			_prefab = prefab.Value;
		}

		public async UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var go = Object.Instantiate(_prefab, parent).gameObject;

			ApplyLayout(go, json);
			ApplyAlignment(go, json["align"]?.Value<string>() ?? "center");

			var url = json["url"]?.Value<string>();
			if (!string.IsNullOrEmpty(url))
			{
				var sprite = await LoadSpriteAsync(url, ct);
				if (sprite != null)
				{
					var img = go.GetComponent<Image>();
					img.sprite = sprite;
					img.preserveAspect = true;

					// If no size specified in JSON — fall back to texture size
					var layout = go.GetComponent<LayoutElement>();
					if (layout.preferredWidth <= 0 && layout.preferredHeight <= 0)
					{
						layout.preferredWidth = sprite.texture.width;
						layout.preferredHeight = sprite.texture.height;
					}
				}
			}

			return go;
		}

		private static void ApplyLayout(GameObject go, JObject json)
		{
			var width = json["width"]?.Value<float>() ?? 0f;
			var height = json["height"]?.Value<float>() ?? 0f;

			var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
			if (width > 0)
				layout.preferredWidth = width;

			if (height > 0)
				layout.preferredHeight = height;

			// Aspect ratio fitter when only one dimension is provided
			if (width > 0 ^ height > 0)
			{
				var aspectFitter = go.GetComponent<AspectRatioFitter>() ?? go.AddComponent<AspectRatioFitter>();
				aspectFitter.aspectMode = width > 0
					? AspectRatioFitter.AspectMode.WidthControlsHeight
					: AspectRatioFitter.AspectMode.HeightControlsWidth;
			}
		}

		private static void ApplyAlignment(GameObject go, string align)
		{
			var rt = go.GetComponent<RectTransform>();

			rt.anchorMin = rt.anchorMax = align switch
			{
				"left" => new Vector2(0f, 0.5f),
				"right" => new Vector2(1f, 0.5f),
				_ => new Vector2(0.5f, 0.5f),
			};

			rt.pivot = rt.anchorMin;
			rt.anchoredPosition = Vector2.zero;
		}

		private static async UniTask<Sprite> LoadSpriteAsync(string url, CancellationToken ct)
		{
			using var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
			await req.SendWebRequest().WithCancellation(ct);

			if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
			{
				Debug.LogWarning($"[SDUI] Failed to load image: {url}");
				return null;
			}

			var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
			return Sprite.Create(tex,
				new Rect(0,
					0,
					tex.width,
					tex.height),
				Vector2.one * 0.5f);
		}
	}
}