#nullable enable
using Exiled.API.Features.Core.UserSettings;
using System.Collections.Generic;

namespace AudioPlayerManager;

public class ServerSpecificSettings
{
    public static HeaderSetting SettingsHeader { get; set; } =
        new HeaderSetting(Plugin.Instance.Config.HeaderId, "AudioPlayerManager");

    public static IEnumerable<SettingBase>? BaseSettings { get; private set; }
    public static TwoButtonsSetting? YesOrNoSetting { get; set; }

    public static void RegisterSettings()
    {
        YesOrNoSetting = new TwoButtonsSetting(Plugin.Instance.Config.SettingId,
            Plugin.Instance.Translation.SettingLabel, Plugin.Instance.Translation.SettingA,
            Plugin.Instance.Translation.SettingB, true,
            Plugin.Instance.Translation.HintDescription, header: SettingsHeader);
        BaseSettings = [YesOrNoSetting];
        SettingBase.Register(BaseSettings);
    }

    public static void UnregisterSettings()
    {
        SettingBase.Unregister(settings: BaseSettings);
    }
}