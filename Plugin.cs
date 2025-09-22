using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Server;
using System;
using System.IO;

namespace AudioPlayerManager
{
    public class Plugin : Plugin<Config, Translation>
    {
        public override string Author => "Antoniofo";

        public override string Name => "AudioPlayer";

        public override Version Version => new Version(2, 4, 5);

        public override Version RequiredExiledVersion => new Version(9, 9, 2);

        public override string Prefix => "audioplayer";


        public static Plugin Instance;


        public override void OnEnabled()
        {
            Instance = this;

            if (!Directory.Exists(Config.AudioFilePath))
            {
                Log.Warn("audio directory doesn't exist. Creating...");
                Directory.CreateDirectory(Config.AudioFilePath);
            }

            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Map.AnnouncingChaosEntrance += OnChaosAnnounce;
            ServerSpecificSettings.RegisterSettings();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
            Exiled.Events.Handlers.Map.AnnouncingChaosEntrance -= OnChaosAnnounce;
            ServerSpecificSettings.UnregisterSettings();
            base.OnDisabled();
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
                API.SoundPlayer.PlayGlobalAudio(Path.GetFileNameWithoutExtension(Config.MtfSoundFilePath), false);
            }
            else if (ev.Wave.Faction == PlayerRoles.Faction.FoundationEnemy && Config.PlayChaosSound)
            {
                API.SoundPlayer.PlayGlobalAudio(Path.GetFileNameWithoutExtension(Config.ChaosSoundFilePath), false);
            }
        }
    }
}