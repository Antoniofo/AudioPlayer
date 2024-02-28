using System;
using System.IO;
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

        public int Ids = 999;

        public static Plugin instance;

        public override void OnEnabled()
        {
            base.OnEnabled();
            SCPSLAudioApi.Startup.SetupDependencies();
            Plugin.instance = this;
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
            var player = playerBase.Owner;
            Log.Info("Track Finished");
            if (playerBase.CurrentPlay != null)
            {
                playerBase.Stoptrack(true);
                playerBase.OnDestroy();
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
            var hub = AudioPlayers.Where(x => x.PlayerId == playerBase.Owner.PlayerId).FirstOrDefault();
            if (hub != null)
            {
                AudioPlayers.Remove(hub);
            }
            
            foreach(var pla in AudioPlayers)
            {
                var audioplayer = AudioPlayerBase.Get(pla);
                if(audioplayer.CurrentPlay == null)
                {
                    AudioPlayers.Remove(pla);                    
                }
                Log.Info(pla.PlayerId);
            }
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Plugin.instance = null;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
        }

        private void OnRespawnTeam(RespawningTeamEventArgs ev)
        {
            Log.Info("Respawn");
            if (ev.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
            {
                Log.Info("MTF");
                foreach (var player in AudioPlayers)
                {
                    Log.Info(AudioPlayers.Count + " " + AudioPlayers);
                    if (AudioPlayers.Where(x => x == player).Count() > 0)
                        return;
                }

                PlaySound("mtf.ogg", "Facility Announcement", 998);
            }
            else if (ev.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency)
            {
                Log.Info("Chaos");
                foreach (var player in AudioPlayers)
                {
                    Log.Info(AudioPlayers.Count + " "+ AudioPlayers);
                    if (AudioPlayers.Where(x => x == player).Count() > 0)
                        return;
                }
                PlaySound("chaos.ogg", "Facility Announcement", 998);
            }
        }

        public void PlaySound(string soundName, string botName, int id = -1, float volume = 20f)
        {
            Log.Info("playsound "+ id);
            if (id == -1)
            {
                id = Ids++;
            }

            string fullPath = Path.Combine(Config.path, soundName);
            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            FakeConnection fakeConnection = new FakeConnection(id);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
            Log.Info("after connecting fake player");
            hubPlayer.nicknameSync.Network_myNickSync = botName;
            AudioPlayerBase audioPlayer = AudioPlayerBase.Get(hubPlayer);
            AudioPlayers.Add(hubPlayer);           
            audioPlayer.Enqueue(fullPath, -1);
            audioPlayer.Volume = volume;
            audioPlayer.Play(0);


        }
    }
}
