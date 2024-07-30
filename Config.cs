using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;

namespace AudioPlayer
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public string AudioFilePath { get; set; } = "/home/container/.config/EXILED/Configs/audio";

        public bool PlayMtfSound { get; set; } = true;

        public string MtfSoundFilePath {get; set;} = "mtf.ogg";

        public bool PlayChaosSound { get; set; } = true;

        public string ChaosSoundFilePath {get; set;} = "chaos.ogg";

        [Description("Do not put over 100 because it could break the VoiceChat for everyplayer and they'll have to restart SL")]
        public float Volume {get; set;} = 20f;
    }
}
