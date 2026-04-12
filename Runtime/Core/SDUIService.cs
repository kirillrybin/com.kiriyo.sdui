using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SDUI.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace SDUI.Core
{
    [UsedImplicitly]
    public class SDUIService: ISDUIService
    {
        public event Action OnLoadingStarted;
        public event Action OnLoadingFinished;
        
        private readonly IUIBuilder _builder;
        private readonly ISDUIHttpClient _http;
        private readonly IPlayerProfile _profile;
        private readonly SDUIConfig _config;
        
        private readonly Dictionary<string, CacheEntry> _cache = new();
        
        public SDUIService(IUIBuilder builder, ISDUIHttpClient http, SDUIConfig config, IPlayerProfile profile)
        {
            _builder = builder;
            _http = http;
            _profile = profile;
            _config =  config;
        }

        public async UniTask LoadPageAsync(string pageName, Transform root, CancellationToken ct = default)
        {
            OnLoadingStarted?.Invoke();
            try
            {
                var json = await FetchAsync(pageName, ct);
                await _builder.BuildPageAsync(json, root, ct);
            }
            finally
            {
                OnLoadingFinished?.Invoke();
            }
        }

        private async UniTask<JObject> FetchAsync(string pageName, CancellationToken ct)
        {
            var lang = ResolveLanguage();
            var cacheKey = $"{pageName}|{lang}";
            
            if (_cache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired(_config.CacheTtl))
                return entry.Data;

            var url  = $"{_config.BaseUrl}/pages/{pageName}.json?userId={_profile.UserId}";
            var json = await _http.GetAsync(url, ct);
            
            _cache[cacheKey] = new CacheEntry((JObject)json);
            return (JObject)json;
        }
        
        public async UniTask ChangeLanguageAsync(string languageCode, string currentPage,
            Transform root, CancellationToken ct = default)
        {
            _profile.SetLanguage(languageCode);
            InvalidateCache();
            await LoadPageAsync(currentPage, root, ct);
        }

        public void InvalidateCache(string pageName = null)
        {
            if (pageName == null)
            {
                _cache.Clear();
                return;
            }

            var prefix = $"{pageName}|";
            var keys   = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keys)
                _cache.Remove(key);
        }
        
        private string ResolveLanguage()
        {
            var lang = _profile.Language;
            return string.IsNullOrWhiteSpace(lang) ? _config.DefaultLanguage : lang;
        }
        
        private readonly struct CacheEntry
        {
            public readonly JObject Data;
            private readonly DateTime _createdAt;

            public CacheEntry(JObject data)
            {
                Data      = data;
                _createdAt = DateTime.UtcNow;
            }

            public bool IsExpired(TimeSpan ttl) => DateTime.UtcNow - _createdAt >= ttl;
        }
    }
}
