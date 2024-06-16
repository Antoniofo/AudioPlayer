﻿using CommandSystem;
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
            int plyID = ply.Id;
            if (Plugin.instance.MutedAnnounce.Contains(plyID))
            {
                Plugin.instance.MutedAnnounce.Remove(plyID);             
                
            }
            else
            {
                Plugin.instance.MutedAnnounce.Add(plyID);                
            }
            response = Plugin.instance.MutedAnnounce.Contains(plyID) ? "You have muted the Facility Announce": "You have unmuted the Facility Announce";
            return true;
        }
    }
}