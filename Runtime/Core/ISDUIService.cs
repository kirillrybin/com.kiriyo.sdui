using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace SDUI.Core
{
	public interface ISDUIService
	{
		event Action OnLoadingStarted;
		event Action OnLoadingFinished;
		event Action<Exception> OnLoadingFailed;

		UniTask LoadPageAsync(string pageName, Transform root, CancellationToken ct = default);
		UniTask ChangeLanguageAsync(string languageCode, string currentPage, Transform root, CancellationToken ct = default);
		void InvalidateCache(string pageName = null);
	}
}