using System;
using UnityEngine;

namespace SDUI.Core
{
	[CreateAssetMenu(fileName = "SDUIConfig", menuName = "SDUI/Config")]
	public class SDUIConfig : ScriptableObject
	{
		[field: SerializeField]
		public string BaseUrl { get; private set; } = "https://api.yourgame.com/sdui";
		
		[field: SerializeField]
		public string DefaultLanguage { get; private set; } = "en";
		
		[SerializeField]
		private string[] _supportedLanguages = { "en", "ru" };
		
		[Tooltip("Cache lifetime in seconds. Set 0 to disable caching.")]
		public float CacheTtlSeconds = 300f;
		
		[Tooltip("Maximum number of pages kept in navigation history. Oldest entry is dropped when the limit is reached.")]
		public int MaxNavigationHistoryDepth = 10;

		public TimeSpan CacheTtl => TimeSpan.FromSeconds(CacheTtlSeconds);
 
		public string[] SupportedLanguages => _supportedLanguages;

	}
}