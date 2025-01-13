using CommandSystem;
using Exiled.Permissions.Extensions;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AudioPlayer

{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]

    public class PlaySound : ParentCommand, IUsageProvider
    {
        public PlaySound() => LoadGeneratedCommands();
        public override string Command => "audioplayer";

        public override string[] Aliases => new[] { "audio" };

        public override string Description => "play/atplace/list/stop an audio";

        public string[] Usage { get; } = new string[3]
        {
            "play/list/stop/atplace",
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
                        if (path.EndsWith(".ogg")) {
                            listSound += "- " + path + "\n";
                        }
                        
                    }
                    response = listSound;
                    return true;
                case "play":
                    if (arguments.Count < 3)
                    {
                        response = $"Not enough argument to play a sound\n{ReturnPlayUsage()}";
                        return false;
                    }

                    bool ret;
                    string displayName = arguments.At(2);
                    string sound;

                    if (IsUrl(arguments.At(1)))
                    {
                        sound = arguments.At(1);
                        ret = API.SoundPlayer.PlaySound(sound, displayName, 98, true);
                    }
                    else
                    {
                        sound = Path.Combine(Plugin.Instance.Config.AudioFilePath, arguments.At(1));
                        ret = API.SoundPlayer.PlaySound(sound, displayName, 99, false);
                    }

                    if (ret)
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
                    List<ReferenceHub> listofshit;

                    bool excludeFacilityAnnouncement = true;

                    if (arguments.Count >= 2)
                    {
                        if (bool.TryParse(arguments.At(1), out bool includeAllPlayers))
                        {
                            excludeFacilityAnnouncement = !includeAllPlayers;
                        }
                    }

                    if (excludeFacilityAnnouncement)
                    {
                        listofshit = API.SoundPlayer.AudioPlayers
                            .Where(player => !player.nicknameSync.Network_myNickSync.Equals("Facility Announcement"))
                            .ToList();
                    }
                    else
                    {
                        listofshit = API.SoundPlayer.AudioPlayers.ToList();
                    }

                    for (int i = 0; i < listofshit.Count; i++)
                    {
                        var audioPlayer = AudioPlayerBase.Get(listofshit[i]);
                        API.SoundPlayer.Stop(audioPlayer);
                    }
                    response = "Sounds Stoped";
                    return true;

                case "atplace":
                    if (arguments.Count < 7)
                    {
                        response = $"Not enough argument to play a sound\n{ReturnPlaceUsage()}";
                        return false;
                    }
                    response = "Failed to parse numbers for position";
                    if (!int.TryParse(arguments.At(1), out int CoorX))
                        return false;
                    if (!int.TryParse(arguments.At(2), out int CoorY))
                        return false;
                    if (!int.TryParse(arguments.At(3), out int CoorZ))
                        return false;
                    if (!int.TryParse(arguments.At(4), out int Distance))
                        return false;

                    string soundPlace;
                    string displayNametest = arguments.At(6);
                    bool retPlace;
                    if (IsUrl(arguments.At(1)))
                    {
                        soundPlace = arguments.At(5);
                        retPlace = API.SoundPlayer.PlaySoundAtPlace(soundPlace, new(CoorX, CoorY, CoorZ), Distance, displayNametest, 97, true);
                    }
                    else
                    {
                        soundPlace = Path.Combine(Plugin.Instance.Config.AudioFilePath, arguments.At(5));
                        retPlace = API.SoundPlayer.PlaySoundAtPlace(soundPlace, new(CoorX, CoorY, CoorZ), Distance, displayNametest, 96, false); ;
                    }

                    if (retPlace)
                    {
                        response = $"Playing at {CoorX} {CoorY} {CoorZ} with range of {Distance}...";
                        return true;
                    }
                    else
                    {
                        response = "Last sound not finished or file doesn't exist";
                        return false;
                    }
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
            return "Usage: audio|audioplayer play/list/stop/atplace [[x] [y] [z] [distance]] [[filename/URL]|[true/false]] [displayName]";
        }

        public static string ReturnPlaceUsage()
        {
            return "Usage: audio|audioplayer atplace x y z distance filename/URL displayName";
        }

        public static string ReturnPlayUsage()
        {
            return "Usage: audio|audioplayer play filename/URL displayName";
        }
    }
}
