using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.Net.Configuration;
using Exiled.API.Features.Core.UserSettings;
using UserSettings.ServerSpecific;

namespace AudioPlayer
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Antoniofo";

        public override string Name => "AudioPlayer";

        public override Version Version => new Version(2, 2, 0);

        public override Version RequiredExiledVersion => new Version(9, 0, 1);

        public override string Prefix => "audioplayer";


        public static Plugin Instance;


        public override void OnEnabled()
        {
            SCPSLAudioApi.Startup.SetupDependencies();
            Plugin.Instance = this;
            API.SoundPlayer.MutedAnnounce = new List<string>();
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            IEnumerable<SettingBase> settings = new List<SettingBase>()
            {
                new TwoButtonsSetting(Config.SettingId, "Mute Facility announcement", "Mute", "Unmute", true,
                    "This will make all the announcement of wave muted", new HeaderSetting("AudioPlayer"))
            };
            SettingBase.Register(settings);
            SettingBase.SendToAll();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Plugin.Instance = null;
            API.SoundPlayer.MutedAnnounce = null;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            base.OnDisabled();
        }

        private void OnRoundStart()
        {
            API.SoundPlayer.AudioPlayers.Clear();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub);
            using (var playerRepo = new PlayerRepository(Config.DatabaseFilePath))
            {
                PlayerDB playerdb = playerRepo.GetPlayerByUserId(ev.Player.UserId);
                if (playerdb != null)
                {
                    if (playerdb.Mute == 2 && !API.SoundPlayer.MutedAnnounce.Contains(ev.Player.UserId))
                    {
                        API.SoundPlayer.MutedAnnounce.Add(ev.Player.UserId);
                    }
                }
                else
                {
                    playerRepo.InsertPlayer(new PlayerDB() { UserId = ev.Player.UserId, Mute = 1 });
                }
            }
        }

        private void OnNTFAnnounce(AnnouncingNtfEntranceEventArgs obj)
        {
            if (Config.PlayMtfSound)
                obj.IsAllowed = false;
        }

        private void OnFinishedTrack(AudioPlayerBase playerBase, string track, bool directPlay, ref int nextQueuePos)
        {
            API.SoundPlayer.Stop(playerBase);
        }

        private void OnRespawnTeam(RespawningTeamEventArgs ev)
        {
            if (ev.Wave.Faction == PlayerRoles.Faction.FoundationStaff && Config.PlayMtfSound)
            {
                API.SoundPlayer.PlaySound(Config.MtfSoundFilePath, "Facility Announcement", 998, false);
            }
            else if (ev.Wave.Faction == PlayerRoles.Faction.FoundationEnemy && Config.PlayChaosSound)
            {
                API.SoundPlayer.PlaySound(Config.ChaosSoundFilePath, "Facility Announcement", 998, false);
            }
        }
    }
}