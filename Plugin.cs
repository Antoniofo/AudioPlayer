using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using System;
using System.Collections.Generic;
using System.IO;
using Exiled.API.Features.Core.UserSettings;
using UserSettings.ServerSpecific;

namespace AudioPlayerManager
{
    public class Plugin : Plugin<Config, Translation>
    {
        public override string Author => "Antoniofo";

        public override string Name => "AudioPlayer";

        public override Version Version => new Version(2, 4, 0);

        public override Version RequiredExiledVersion => new Version(9, 6, 0);

        public override string Prefix => "audioplayer";


        public static Plugin Instance;


        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Map.AnnouncingChaosEntrance += OnChaosAnnounce;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            IEnumerable<SettingBase> settings = new List<SettingBase>()
            {
                new TwoButtonsSetting(Config.SettingId, Translation.SettingLabel, Translation.SettingA,
                    Translation.SettingB, true,
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
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            base.OnDisabled();
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

        private void OnChaosAnnounce(AnnouncingChaosEntranceEventArgs obj)
        {
            if (Config.PlayChaosSound)
                obj.IsAllowed = false;
        }

        

        private void OnRespawnTeam(RespawningTeamEventArgs ev)
        {
            if (ev.Wave.Faction == PlayerRoles.Faction.FoundationStaff && Config.PlayMtfSound)
            {
                API.SoundPlayer.PlayGlobalAudio("mtf", false);
            }
            else if (ev.Wave.Faction == PlayerRoles.Faction.FoundationEnemy && Config.PlayChaosSound)
            {
                API.SoundPlayer.PlayGlobalAudio("chaos", false);
            }
        }
    }
}