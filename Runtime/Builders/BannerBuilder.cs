using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SDUI.Core;
using SDUI.Http;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	public sealed class BannerPrefab { public GameObject Value; }
	
	[UsedImplicitly]
	public class BannerBuilder : IComponentBuilder
	{
		public string Type => "banner";

		private readonly BannerPrefab _prefab;
		private readonly ActionDispatcher _dispatcher;
		private readonly ISDUIHttpClient _http;
		private readonly SDUIConfig _config;

		public BannerBuilder(BannerPrefab prefab, ActionDispatcher dispatcher, ISDUIHttpClient http, SDUIConfig config, IPlayerProfile profile)
		{
			_prefab = prefab;
			_dispatcher = dispatcher;
			_http = http;
			_config = config;
		}

		public async UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var bannerData = await ResolveBannerDataAsync(json, ct);

			var go     = Object.Instantiate(_prefab.Value, parent);
			var height = json["height"]?.Value<float>() ?? 200f;

			go.GetComponent<LayoutElement>().preferredHeight = height;
			go.GetComponent<BannerView>().Setup(
				title:      bannerData["title"]?.Value<string>()    ?? string.Empty,
				subtitle:   bannerData["subtitle"]?.Value<string>() ?? string.Empty,
				ctaLabel:   bannerData["ctaLabel"]?.Value<string>() ?? string.Empty,
				imageUrl:   bannerData["imageUrl"]?.Value<string>() ?? string.Empty,
				action:     bannerData["action"]?.Value<string>()   ?? string.Empty,
				dispatcher: _dispatcher
			);

			return go;
		}
		
		private async UniTask<JObject> ResolveBannerDataAsync(JObject json, CancellationToken ct)
		{
			var dataUrl = json["dataUrl"]?.Value<string>();

			if (string.IsNullOrEmpty(dataUrl))
				return json;

			var result = await _http.GetAsync($"{_config.BaseUrl}{dataUrl}", ct);
			return (JObject)result;
		}
	}
}