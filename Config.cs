using System;
using System.Collections.Generic;
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

        public string path { get; set; } = "/home/container/.config/EXILED/Configs/audio";

        public string mtfSound {get; set;} = "mtf.ogg";

        public string chaosSound {get; set;} = "chaos.ogg";

        [Description("Do not put over 100 because it could break the VoiceChat for everyplayer and they'll have to restart SL")]
        public float volume {get; set;} = 20f;
    }
}
