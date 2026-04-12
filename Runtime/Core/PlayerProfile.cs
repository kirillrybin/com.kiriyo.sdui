namespace SDUI.Core
{
	public interface IPlayerProfile
	{
		string UserId { get; }
		string Language { get; }
		void SetUserId(string userId);
		
		/// <summary>
		/// Changes the active language. Does NOT reload any pages —
		/// call <see cref="SDUIService.InvalidateCache"/> and re-navigate after.
		/// </summary>
		void SetLanguage(string languageCode);
	}

	public class PlayerProfile : IPlayerProfile
	{
		private const string FallbackLanguage = "en";

		public string UserId { get; private set; }
		public string Language { get; private set; }

		public PlayerProfile(string userId, string language = FallbackLanguage)
		{
			UserId = userId;
			Language = string.IsNullOrWhiteSpace(language) ? FallbackLanguage : language;
		}

		public void SetUserId(string userId)
		{
			UserId = userId;
		}

		public void SetLanguage(string languageCode)
		{
			Language = string.IsNullOrWhiteSpace(languageCode) ? FallbackLanguage : languageCode;
		}
	}
}