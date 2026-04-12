using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SDUI.Core;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace SDUI.Http
{
    [UsedImplicitly]
    public class SDUIHttpClient: ISDUIHttpClient
    {
        private readonly IPlayerProfile _profile;
       
        public SDUIHttpClient(IPlayerProfile profile)
        {
            _profile = profile;
        }

        public async UniTask<JToken> GetAsync(string url, CancellationToken ct = default)
        {
            using var req = UnityWebRequest.Get(WithLang(url));
            await req.SendWebRequest().WithCancellation(ct);
            EnsureSuccess(req);
            return JToken.Parse(req.downloadHandler.text);
        }

        public async UniTask<JObject> PostAsync(string url, JObject body, CancellationToken ct = default)
        {
            var raw = Encoding.UTF8.GetBytes(body.ToString());
            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler   = new UploadHandlerRaw(raw) { contentType = "application/json" },
                downloadHandler = new DownloadHandlerBuffer()
            };
            await req.SendWebRequest().WithCancellation(ct);
            EnsureSuccess(req);
            return ParseResponse(req);
        }

        public async UniTask<JObject> PatchAsync(string url, JObject body = null, CancellationToken ct = default)
        {
            var raw = body != null
                ? Encoding.UTF8.GetBytes(body.ToString())
                : Array.Empty<byte>();

            using var req = new UnityWebRequest(url, "PATCH");
            req.uploadHandler = new UploadHandlerRaw(raw) { contentType = "application/json" };
            req.downloadHandler = new DownloadHandlerBuffer();

            await req.SendWebRequest().WithCancellation(ct);
            EnsureSuccess(req);
            return ParseResponse(req);
        }

        public async UniTask<JObject> DeleteAsync(string url, CancellationToken ct = default)
        {
            using var req = new UnityWebRequest(url, "DELETE")
            {
                downloadHandler = new DownloadHandlerBuffer()
            };
            await req.SendWebRequest().WithCancellation(ct);
            EnsureSuccess(req);
            return ParseResponse(req);
        }

        public async UniTask<Sprite> GetSpriteAsync(string url, CancellationToken ct = default)
        {
            using var req = UnityWebRequestTexture.GetTexture(url);
            await req.SendWebRequest().WithCancellation(ct);
            EnsureSuccess(req);
            var tex = DownloadHandlerTexture.GetContent(req);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }

        private static void EnsureSuccess(UnityWebRequest req)
        {
            if (req.result != UnityWebRequest.Result.Success)
                throw new SDUIHttpException(req.responseCode, req.error, req.url);
        }

        private static JObject ParseResponse(UnityWebRequest req)
        {
            var text = req.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(text))
                throw new SDUIHttpException(req.responseCode, "Empty response body", req.url);
            return JObject.Parse(text);
        }
        
        // Appends &lang= to any URL that doesn't already have it
        private string WithLang(string url)
        {
            var lang = _profile.Language;
            if (string.IsNullOrEmpty(lang))
                return url;

            var separator = url.Contains('?') ? '&' : '?';
            return $"{url}{separator}lang={lang}";
        }
    }

    public class SDUIHttpException : Exception
    {
        public long StatusCode { get; }
        public string Url { get; }

        public SDUIHttpException(long statusCode, string message, string url)
            : base($"[SDUI] HTTP {statusCode} — {message} ({url})")
        {
            StatusCode = statusCode;
            Url = url;
        }
    }
}