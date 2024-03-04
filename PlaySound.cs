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

    public class PlaySound : ICommand, IUsageProvider
    {
        public string Command => "audioplayer";

        public string[] Aliases => new[] { "audio" };

        public string Description => "play/list/stop an audio";

        public string[] Usage { get; } = new string[3]
        {
            "play/list/stop",
            "audioName",
            "displayName"            
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
                                        
                    bool ret = Plugin.instance.PlaySound(sound, displayName, 99);
                    if (ret){
                        response = "Playing ...";
                        return true;
                    }else{
                        response = "Last sound not finished";
                        return false;
                    }
                case "stop":
                    foreach (var player in AudioPlayers)
                    {
                        Log.Info(AudioPlayers.Count + " "+ AudioPlayers);
                        if (AudioPlayers.Any(x => x.nicknameSync.Network_myNickSync.Equals("Facility Announcement")))
                            continue;
                        var audioPlayer = AudioPlayerBase.Get(player);
                        Plugin.Stop(audioPlayer);
                    }
                    response = "Sounds Stoped";
                    return true;
                default:
                    response = "No subcommand recognized";
                    return false;

            }

        }
    }
}
