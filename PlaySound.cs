using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AudioPlayerManager

{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PlaySound : ParentCommand, IUsageProvider
    {
        public PlaySound() => LoadGeneratedCommands();
        public override string Command => "audioplayer";

        public override string[] Aliases => new[] { "audio" };

        public override string Description => "play/atplace/list/stop an audio";

        public string[] Usage { get; } = new string[2]
        {
            "play/list/stop/atplace",
            "audioName"
        };

        public override void LoadGeneratedCommands()
        {
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("AudioPlayer"))
            {
                response = "You don't have the permission";
                return false;
            }

            if (arguments.Count <= 0)
            {
                response = $"No arguments given\n{ReturnUsages()}";
                return false;
            }

            switch (arguments.At(0))
            {
                case "list":
                    string[] files = Directory.GetFiles(Plugin.Instance.Config.AudioFilePath);
                    string listSound = "Here are the current available sounds : \n";
                    foreach (string file in files)
                    {
                        string path = Path.GetFileName(file);
                        if (path.EndsWith(".ogg"))
                        {
                            listSound += "- " + path + "\n";
                        }
                    }

                    response = listSound;
                    return true;
                case "play":
                    if (arguments.Count < 2)
                    {
                        response = $"Not enough argument to play a sound\n{ReturnPlayUsage()}";
                        return false;
                    }

                    string sound = arguments.At(1);
                    API.SoundPlayer.PlayGlobalAudio(sound, IsUrl(arguments.At(1)));


                    response = "Playing ...";
                    return true;

                case "stop":
                    bool local = false;
                    if (arguments.Count > 1)
                    {
                        bool.TryParse(arguments.At(1), out bool localtoo);
                        local = localtoo;
                        if (localtoo)
                        {
                            foreach (AudioPlayer ap in API.SoundPlayer.audioPlayers)
                            {
                                if (ap != null)
                                {
                                    ap.RemoveAllClips();
                                    ap.Destroy();    
                                }
                            }

                            API.SoundPlayer.audioPlayers.Clear();
                        }    
                    }
                    
                    
                    if (AudioPlayer.TryGet("Global AudioPlayer", out AudioPlayer aplayer))
                    {
                        aplayer.RemoveAllClips();                        
                    }
                    

                    response = local ? "All Audio stopped" : "Global Audio stopped";
                    return true;

                case "atplace":

                    if (arguments.Count < 6)
                    {
                        response = $"Not enough argument to play a sound\n{ReturnPlaceUsage()}";
                        return false;
                    }

                    response = "Failed to parse numbers for position";
                    if (!int.TryParse(arguments.At(1), out int coorX))
                        return false;
                    if (!int.TryParse(arguments.At(2), out int coorY))
                        return false;
                    if (!int.TryParse(arguments.At(3), out int coorZ))
                        return false;
                    if (!int.TryParse(arguments.At(4), out int distance))
                        return false;

                    string soundPlace = arguments.At(5);
                    API.SoundPlayer.PlayLocalAudio(soundPlace, IsUrl(soundPlace), new(coorX, coorY, coorZ),
                        distance);

                    response = $"Playing at {coorX} {coorY} {coorZ} with range of {distance}...";
                    return true;

                default:
                    response = $"No subcommand recognized\n{ReturnUsages()}";
                    return false;
            }
        }

        public static bool IsUrl(string input)
        {
            // Regular expression pattern to match URLs (basic version)
            string urlPattern = @"^(https?|ftp|file):\/\/[^\s/$.?#].[^\s]*$";
            return Regex.IsMatch(input, urlPattern, RegexOptions.IgnoreCase);
        }

        public static string ReturnUsages()
        {
            return
                "Usage: audio|audioplayer play/list/stop/atplace [[x] [y] [z] [distance]] [[filename/URL]|[true/false]]";
        }

        public static string ReturnPlaceUsage()
        {
            return "Usage: audio|audioplayer atplace x y z distance filename/URL";
        }

        public static string ReturnPlayUsage()
        {
            return "Usage: audio|audioplayer play filename/URL";
        }
    }
}