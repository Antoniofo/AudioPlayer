using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using Exiled.API.Features.Core.UserSettings;
using UserSettings.ServerSpecific;

namespace AudioPlayer
{
    public class Plugin : Plugin<Config, Translation>
    {
        public override string Author => "Antoniofo";

        public override string Name => "AudioPlayer";

        public override Version Version => new Version(2, 3, 0);

        public override Version RequiredExiledVersion => new Version(9, 3, 0);

        public override string Prefix => "audioplayer";


        public static Plugin Instance;


        public override void OnEnabled()
        {
            SCPSLAudioApi.Startup.SetupDependencies();
            Instance = this;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            AudioPlayerBase.OnFinishedTrack += OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            IEnumerable<SettingBase> settings = new List<SettingBase>()
            {
                new TwoButtonsSetting(Config.SettingId, Translation.SettingLabel, Translation.SettingA, Translation.SettingB, true,
                    Translation.HintDescription, new HeaderSetting("AudioPlayer"))
            };
            SettingBase.Register(settings);
            SettingBase.SendToAll();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            AudioPlayerBase.OnFinishedTrack -= OnFinishedTrack;
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