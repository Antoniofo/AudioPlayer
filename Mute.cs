using CommandSystem;
using Exiled.API.Features;
using System;

namespace AudioPlayer

{
    [CommandHandler(typeof(ClientCommandHandler))]

    public class Mute : ParentCommand
    {
        public Mute() => LoadGeneratedCommands();
        public override string Command => "mute_announce";

        public override string[] Aliases => new string[] { "mute_fa" };

        public override string Description => "Mute the announce of the respawn";
                
        public override void LoadGeneratedCommands()
        {            
        }
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {            
            Player ply = Player.Get(sender);
            if(ply == null)
            {
                response = "Player not found (You are you !?)";
                return false;
            }
            string plyID = ply.UserId;
            using (var playerRepo = new PlayerRepository(Plugin.instance.Config.DatabaseFilePath))
            {
                PlayerDB playerdb = playerRepo.GetPlayerByUserId(plyID);                
                if (playerdb != null)
                {                    
                    if (playerdb.Mute == 2)
                    {                        
                        playerdb.Mute = 1;
                        playerRepo.UpdatePlayer(playerdb);
                        API.SoundPlayer.MutedAnnounce.Remove(plyID);

                    }
                    else
                    {
                        playerdb.Mute = 2;
                        playerRepo.UpdatePlayer(playerdb);
                        API.SoundPlayer.MutedAnnounce.Add(plyID);
                    }
                }
                else
                {                    
                    playerRepo.InsertPlayer(new PlayerDB() { UserId = plyID, Mute = 2 });
                    API.SoundPlayer.MutedAnnounce.Add(plyID);
                }
                playerdb = playerRepo.GetPlayerByUserId(plyID);                
                response = playerdb.Mute == 2 ? "You have muted the Facility Announce" : "You have unmuted the Facility Announce";
                
            }
            return true;
        }
    }
}