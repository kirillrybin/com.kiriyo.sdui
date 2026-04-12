using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace SDUI.Core
{
	[UsedImplicitly]
	public class SDUINavigator
	{
		private readonly ISDUIService _sdui;
		private readonly SDUIConfig _config;
		private readonly Stack<string> _history = new();
		private Transform _root;
		private CancellationToken _ct;

		public string CurrentPage { get; private set; }
		public Transform Root => _root;

		public SDUINavigator(ISDUIService sdui, SDUIConfig config)
		{
			_sdui = sdui;
			_config = config;
		}

		public void Initialize(Transform root, CancellationToken ct)
		{
			_root = root;
			_ct = ct;
		}

		public async UniTask GoToAsync(string pageName)
		{
			if (CurrentPage != null)
			{
				if (_history.Count >= _config.MaxNavigationHistoryDepth)
				{
					Debug.LogWarning($"[SDUI] Navigation history limit ({_config.MaxNavigationHistoryDepth}) reached — oldest entry dropped");
					TrimHistory();
				}

				_history.Push(CurrentPage);
			}

			CurrentPage = pageName;
			await _sdui.LoadPageAsync(pageName, _root, _ct);
		}

		public async UniTask BackAsync()
		{
			if (_history.Count == 0)
			{
				Debug.LogWarning("[SDUI] Navigation history is empty");
				return;
			}

			CurrentPage = _history.Pop();
			await _sdui.LoadPageAsync(CurrentPage, _root, _ct);
		}

		public bool CanGoBack => _history.Count > 0;
		
		private void TrimHistory()
		{
			var entries = _history.ToArray();       // top → bottom order
			_history.Clear();
			for (var i = entries.Length - 2; i >= 0; i--)  // skip last (oldest)
				_history.Push(entries[i]);
		}
	}
}