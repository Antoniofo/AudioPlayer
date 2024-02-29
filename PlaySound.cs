using System;
using System.IO;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using UnityEngine;
using MapGeneration;
using PlayerRoles;
using RemoteAdmin;

namespace AudioPlayer

{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]

    public class ChangeLight : ICommand, IUsageProvider
    {
        public string Command => "audioplayer";

        public string[] Aliases => new[] { "audio" };

        public string Description => "play an audio";

        public string[] Usage { get; } = new string[3]
        {
            "play/list",
            "audioName",
            "displayName",
            "[volume]"
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("AudioPlayer"))
            {
                response = "You don't have the permission";
                return false;
            }
            if (arguments.Count <= 0)
            {
                response = "No arguments given";
                return false;
            }

            switch (arguments.At(0))
            {
                case "list":
                    string[] files = Directory.GetFiles(Plugin.instance.Config.path);
                    string listSound = "Here are the current available sounds : \n";
                    foreach (string file in files)
                    {
                        listSound += "- " + Path.GetFileName(file) + "\n";
                    }
                    response = listSound;
                    return true;
                case "play":
                    if (arguments.Count < 3)
                    {
                        response = "Not enough argument to play a sound";
                        return false;
                    }

                    string sound = Path.Combine(Plugin.instance.Config.path, arguments.At(1));
                    string displayName = arguments.At(2);
                    
                    if (arguments.Count == 4){
                        try{
                            float volume;
                            float.TryParse(arguments.At(3), out volume);
                            Plugin.instance.PlaySound(sound, displayName, volume:volume);
                        }catch(Exception ex){
                            response = "Volume badly sent";
                            return false;
                        }
                        
                    }else{
                        Plugin.instance.PlaySound(sound, displayName);
                    }

                    response = "Playing ...";
                    return true;
                default:
                    response = "No subcommand recognized";
                    return false;

            }

        }
    }
}
