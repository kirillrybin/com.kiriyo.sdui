using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;

namespace SDUI.Core
{
	public interface IComponentBuilder
	{
		string Type { get; }
		UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct);
	}
}