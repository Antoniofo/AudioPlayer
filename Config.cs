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

        public string path { get; set; } = "/home/container/.config/EXILED/Configs/audio/ntf.ogg";
    }
}
