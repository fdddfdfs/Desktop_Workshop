using Steamworks;

public static class SettingsStorage
{
    private const float StartVolume = 0.5f;
    private const int DefaultGraphic = 5;

    public static DataFloat SoundVolume { get; } = new (nameof(SoundVolume), StartVolume);
    
    public static DataInt IsSoundMuted { get; } = new (nameof(IsSoundMuted), 0);
    
    public static DataInt SoundInterval { get; } = new (nameof(SoundInterval), 60);
    
    public static DataInt EventInterval { get; } = new (nameof(EventInterval), 60);

    public static DataFloat MusicVolume { get; } = new(nameof(MusicVolume), 0.1f);

    public static DataInt Graphic { get; } = new(nameof(Graphic), DefaultGraphic);

    public static DataInt Localization { get; } = new(
        nameof(Localization),
        GetSteamUILanguageOrDefault());

    private static int GetSteamUILanguageOrDefault()
    {
        const int defaultLanguage = (int)global::Localization.DefaultLanguage;

        if (!SteamManager.Initialized)
            return defaultLanguage;
        
        string steamUILanguageString = SteamApps.GetCurrentGameLanguage();
        if (Languages.SteamUILanguages.TryGetValue(steamUILanguageString, out Languages.Language steamUiLanguage))
        {
            return (int)steamUiLanguage;
        }

        return defaultLanguage;
    }
}