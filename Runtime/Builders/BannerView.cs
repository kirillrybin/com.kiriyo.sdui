using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	public class BannerView : MonoBehaviour
	{
		[SerializeField] private Image   _background;
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _subtitle;
		[SerializeField] private Button   _ctaButton;
		[SerializeField] private TMP_Text _ctaLabel;

		public void Setup(string title, string subtitle, string ctaLabel,
			string imageUrl, string action, Core.ActionDispatcher dispatcher)
		{
			_title.text    = title;
			_subtitle.text = subtitle;
			_ctaLabel.text = ctaLabel;

			_ctaButton.onClick.AddListener(() => dispatcher.Dispatch(action));

			if (!string.IsNullOrEmpty(imageUrl))
				LoadImageAsync(imageUrl, destroyCancellationToken).Forget();
		}

		private async UniTaskVoid LoadImageAsync(string url, CancellationToken ct)
		{
			using var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
			await req.SendWebRequest().WithCancellation(ct);

			if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
				return;

			var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
			_background.sprite = Sprite.Create(
				tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
		}
	}
}