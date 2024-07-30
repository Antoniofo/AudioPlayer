using System;
using System.IO;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using SCPSLAudioApi.AudioCore;
using UnityEngine;
using MapGeneration;
using PlayerRoles;
using RemoteAdmin;
using System.Linq;
using System.Collections.Generic;
using Mirror;

namespace AudioPlayer

{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]

    public class PlaySound : ParentCommand, IUsageProvider
    {
        public PlaySound() => LoadGeneratedCommands();
        public override string Command => "audioplayer";

        public override string[] Aliases => new[] { "audio" };

        public override string Description => "play/playurl/list/stop an audio";

        public string[] Usage { get; } = new string[3]
        {
            "play/list/stop",
            "audioName",
            "displayName"
        };

        public override void LoadGeneratedCommands()
        {            
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("AudioPlayer"))
            {
                response = "You don't have the permission";
                return false;
            }
            if (arguments.Count <= 0)
            {
                response = "No arguments given\nUsage: audio|audioplayer play/playurl/list/stop [filename] [displayName]";
                return false;
            }

            switch (arguments.At(0))
            {
                case "list":
                    string[] files = Directory.GetFiles(Plugin.instance.Config.AudioFilePath);
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
                        response = "Not enough argument to play a sound\nUsage: audio|audioplayer play/playurl/list/stop [filename] [displayName]";
                        return false;
                    }

                    string sound = Path.Combine(Plugin.instance.Config.AudioFilePath, arguments.At(1));
                    string displayName = arguments.At(2);
                                        
                    bool ret = Plugin.instance.PlaySound(sound, displayName, 99, false);
                    if (ret){
                        response = "Playing ...";
                        return true;
                    }else{
                        response = "Last sound not finished or file doesn't exist";
                        return false;
                    }
                case "playurl":
                    if (arguments.Count < 3)
                    {
                        response = "Not enough argument to play a sound\nUsage: audio|audioplayer play/playurl/list/stop [filename] [displayName]";
                        return false;
                    }
                    string urlsound = arguments.At(1);
                    string urldisplayName = arguments.At(2);
                    bool urlret = Plugin.instance.PlaySound(urlsound, urldisplayName, 98, true);
                    if (urlret)
                    {
                        response = "Playing ...";
                        return true;
                    }
                    else
                    {
                        response = "Last sound not finished or file doesn't exist";
                        return false;
                    }
                case "stop":                    
                    List<ReferenceHub> listofshit = Plugin.AudioPlayers.Where(x => !x.nicknameSync.Network_myNickSync.Equals("Facility Announcement")).ToList();                    
                    for (int i = 0; i < listofshit.Count; i++)
                    {
                        var audioPlayer = AudioPlayerBase.Get(listofshit[i]);
                        Plugin.instance.Stop(audioPlayer);
                    }                    
                    response = "Sounds Stoped";
                    return true;
                default:
                    response = "No subcommand recognized\nUsage: audio|audioplayer play/playurl/list/stop [filename] [displayName]";
                    return false;

            }

        }
    }
}
