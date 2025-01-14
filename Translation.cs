using Exiled.API.Interfaces;

namespace AudioPlayer;

public class Translation : ITranslation
{
    public string SettingLabel { get; set; } = "Mute Facility announcement";
    public string SettingA { get; set; } = "Mute";
    public string SettingB { get; set; } = "Unmute";
    public string HintDescription { get; set; } = "This will make all the announcement of wave muted";
}