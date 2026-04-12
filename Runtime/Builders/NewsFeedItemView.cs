using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SDUI.Core;

namespace SDUI.Builders
{
    public class NewsFeedItemView : MonoBehaviour
    {
        [SerializeField] private Image    _thumbnail;
        [SerializeField] private RectTransform _textContainer;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _date;
        [SerializeField] private Button   _readMore;
        [SerializeField] private GameObject _pinnedBadge;

        public void Setup(string title, string body, string publishedAt,
            string imageUrl, string action, bool pinned, ActionDispatcher dispatcher)
        {
            _title.text = title;
            _body.text  = body;
            _date.text  = FormatDate(publishedAt);

            if (_pinnedBadge != null)
                _pinnedBadge.SetActive(pinned);

            if (_readMore != null)
            {
                var hasAction = !string.IsNullOrEmpty(action);
                _readMore.gameObject.SetActive(hasAction);

                if (hasAction)
                    _readMore.onClick.AddListener(() => dispatcher.Dispatch(action));
            }
            
            if (string.IsNullOrEmpty(imageUrl))
            {
                if (_thumbnail != null)
                    _thumbnail.gameObject.SetActive(false);
                _textContainer.offsetMin = Vector2.zero;
                return;
            }

            if (_thumbnail != null)
                LoadImageAsync(imageUrl, destroyCancellationToken).Forget();
        }

        private static string FormatDate(string iso)
        {
            if (System.DateTime.TryParse(iso, out var dt))
                return dt.ToString("MMM dd, yyyy");
            return string.Empty;
        }

        private async UniTaskVoid LoadImageAsync(string url, CancellationToken ct)
        {
            using var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
            await req.SendWebRequest().WithCancellation(ct);

            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                _thumbnail.gameObject.SetActive(false);
                return;
            }

            var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
            _thumbnail.sprite = Sprite.Create(
                tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }
    }
}