using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.Handlers;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Server;
using SCPSLAudioApi.AudioCore;
using UnityEngine;
using MEC;
using Mirror;
using Exiled.Events.EventArgs.Map;

namespace AudioPlayer
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Antoniofo";

        public override string Name => "AudioPlayer";

        public override Version Version => new Version(1, 0, 0);

        public override string Prefix => "audioplayer";

        public static List<ReferenceHub> AudioPlayers = new List<ReferenceHub>();

	public static int Ids = 999;

        public override void OnEnabled()
        {
            base.OnEnabled();
            SCPSLAudioApi.Startup.SetupDependencies();

            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
        }

        private void OnNTFAnnounce(AnnouncingNtfEntranceEventArgs obj)
        {
            obj.IsAllowed = false;
        }

        private void OnFinishedTrack(AudioPlayerBase playerBase, string track, bool directPlay, ref int nextQueuePos)
        {
	    AudioPlayers.Remove(playerBase.Owner);
            foreach (var player in AudioPlayers)
            {
                if (!player) continue;
                var audioPlayer = AudioPlayerBase.Get(player);
                if (!audioPlayer) continue;

                if (audioPlayer.CurrentPlay != null)
                {
                    audioPlayer.Stoptrack(true);
                    audioPlayer.OnDestroy();
                }

                player.gameObject.transform.position = new Vector3(-9999f, -9999f, -9999f);
                Timing.CallDelayed(0.5f, () =>
                {
                    NetworkServer.Destroy(player.gameObject);
                });
                //NetworkConnectionToClient conn = player.connectionToClient;
                //player.OnDestroy();
                //CustomNetworkManager.TypedSingleton.OnServerDisconnect(conn);
                //NetworkServer.Destroy(player.gameObject);
            }

        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
        }

        private void OnRespawnTeam(RespawningTeamEventArgs ev)
        {
            if (ev.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
            {                
                if (AudioPlayers.Where(x => x.PlayerIsConnected(x.Owner)).Count > 0)
                    return;
		Plugin.PlaySound(Config.path, "Facility Announcement", 998);
            }
        }
	
	public static void PlaySound(string soundName, botName, id = Plugins.Ids)
	{
	    string fullPath = Path.Combine(Config.path,soundName);
            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            FakeConnection fakeConnection = new FakeConnection(Plugin.Ids++);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);

            hubPlayer.nicknameSync.Network_myNickSync = botName;
            AudioPlayerBase audioPlayer = AudioPlayerBase.Get(hubPlayer);
            AudioPlayers.Add(hubPlayer);
            audioPlayer.Enqueue(fullPath, -1);
            audioPlayer.Play(0);


	}
    }
}
