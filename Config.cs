using Exiled.API.Interfaces;
using System.ComponentModel;
using System.IO;

namespace AudioPlayer
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public string AudioFilePath { get; set; } = Path.Combine(Exiled.API.Features.Paths.Configs, "audio");

        public bool PlayMtfSound { get; set; } = false;

        public string MtfSoundFilePath {get; set;} = "mtf.ogg";

        public bool PlayChaosSound { get; set; } = false;

        public string ChaosSoundFilePath {get; set;} = "chaos.ogg";

        [Description("Do not put over 100 because it could break the VoiceChat for everyplayer and they'll have to restart SL")]
        public float Volume {get; set;} = 20f;
        [Description("Change this if it collide with an other plugin")]
        public int SettingId { get; set; } = 668;
    }
}
