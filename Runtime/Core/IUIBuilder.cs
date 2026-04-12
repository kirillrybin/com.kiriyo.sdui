using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;

namespace SDUI.Core
{
	public interface IUIBuilder
	{
		UniTask BuildPageAsync(JObject pageJson, Transform root, CancellationToken ct = default);
	}
}