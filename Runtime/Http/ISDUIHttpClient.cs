using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;

namespace SDUI.Http
{
	public interface ISDUIHttpClient
	{
		UniTask<JToken>  GetAsync   (string url, CancellationToken ct = default);
		UniTask<JObject> PostAsync  (string url, JObject body, CancellationToken ct = default);
		UniTask<JObject> PatchAsync (string url, JObject body = null, CancellationToken ct = default);
		UniTask<JObject> DeleteAsync(string url, CancellationToken ct = default);
		UniTask<Sprite>  GetSpriteAsync(string url, CancellationToken ct = default);
	}
}