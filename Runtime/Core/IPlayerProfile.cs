namespace SDUI.Core
{
	public interface IPlayerProfile
	{
		string UserId { get; }
		string Language { get; }
		void SetUserId(string userId);
		void SetLanguage(string languageCode);
	}
}