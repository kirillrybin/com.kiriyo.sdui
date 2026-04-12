using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using SDUI.Core;
using SDUI.Http;

namespace SDUI.Builders
{
    public sealed class NewsFeedItemPrefab { public GameObject Value; }

    [UsedImplicitly]
    public class NewsFeedBuilder : IComponentBuilder
    {
        public string Type => "news_feed";

        private readonly NewsFeedItemPrefab _itemPrefab;
        private readonly ActionDispatcher _dispatcher;
        private readonly ISDUIHttpClient _http;
        private readonly SDUIConfig _config;

        public NewsFeedBuilder(NewsFeedItemPrefab itemPrefab, ActionDispatcher dispatcher, ISDUIHttpClient http, SDUIConfig config, IPlayerProfile profile)
        {
            _itemPrefab = itemPrefab;
            _dispatcher = dispatcher;
            _http = http;
            _config = config;
        }

        public async UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
        {
            var limit   = json["limit"]?.Value<int>()    ?? 10;
            var dataUrl = json["dataUrl"]?.Value<string>() ?? "/news";

            var scrollObj = new GameObject("NewsFeedScroll");
            scrollObj.transform.SetParent(parent, false);

            var scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.sizeDelta = Vector2.zero;

            var layoutElement = scrollObj.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1f;
            layoutElement.minHeight      = json["minHeight"]?.Value<float>() ?? 200f;

            var maskImage = scrollObj.AddComponent<Image>();
            maskImage.color = Color.white;

            var mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var scrollRect       = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            // Content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);

            var contentRt       = contentObj.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot     = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;

            var layout = contentObj.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth  = true;
            layout.childControlHeight = false;
            layout.spacing            = 12f;
            layout.padding            = new RectOffset(16, 16, 16, 16);

            var fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            
            var items = await FetchNewsAsync(dataUrl, limit, ct);
            foreach (var item in items)
                BuildNewsItem(item, contentObj.transform);

            return scrollObj;
        }

        private void BuildNewsItem(JObject item, Transform parent)
        {
            var go = Object.Instantiate(_itemPrefab.Value, parent);
            go.GetComponent<NewsFeedItemView>()
                .Setup(title: item["title"]?.Value<string>() ?? string.Empty,
                    body: item["body"]?.Value<string>() ?? string.Empty,
                    publishedAt: item["publishedAt"]?.Value<string>() ?? string.Empty,
                    imageUrl: item["imageUrl"]?.Value<string>() ?? string.Empty,
                    action: item["action"]?.Value<string>() ?? string.Empty,
                    pinned: item["pinned"]?.Value<bool>() ?? false,
                    dispatcher: _dispatcher);
        }

        private async UniTask<List<JObject>> FetchNewsAsync(string dataUrl, int limit, CancellationToken ct)
        {
            var url = $"{_config.BaseUrl}{dataUrl}?limit={limit}";
            var result = await _http.GetAsync(url, ct);
            var list = new List<JObject>();

            foreach (var item in (JArray) result)
                list.Add((JObject) item);

            return list;
        }
    }
}