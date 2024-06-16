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

        public override Version Version => new Version(1, 2, 0);

        public override string Prefix => "audioplayer";

        public static List<ReferenceHub> AudioPlayers = new List<ReferenceHub>();        

        public static Plugin instance;

        public List<int> MutedAnnounce = new List<int>();

        public override void OnEnabled()
        {
            base.OnEnabled();
            SCPSLAudioApi.Startup.SetupDependencies();
            Plugin.instance = this;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
        }

        private void OnRoundStart()
        {
            AudioPlayers.Clear();
            MutedAnnounce.Clear();            
        }

        private void OnNTFAnnounce(AnnouncingNtfEntranceEventArgs obj)
        {
            obj.IsAllowed = false;
        }

        private void OnFinishedTrack(AudioPlayerBase playerBase, string track, bool directPlay, ref int nextQueuePos)
        {
            Stop(playerBase);
        }

        public void Stop(AudioPlayerBase playerBase){
            var player = playerBase.Owner;
            Log.Debug("Track Finished");
            if (playerBase.CurrentPlay != null)
            {
                playerBase.Stoptrack(true);
                playerBase.OnDestroy();
            }

            if(player.gameObject != null)
            {
                player.gameObject.transform.position = new Vector3(-9999f, -9999f, -9999f);
                Timing.CallDelayed(0.5f, () =>
                {
                    NetworkServer.Destroy(player.gameObject);
                });
            }            
            
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
                Log.Debug(pla.PlayerId);
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
            Log.Debug("Respawn");
            if (ev.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
            {
                Log.Debug("MTF");

                PlaySound(Config.mtfSound, "Facility Announcement", 998);
            }
            else if (ev.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency)
            {
                Log.Debug("Chaos");
                
                PlaySound(Config.chaosSound, "Facility Announcement", 998);
            }
        }

        public bool PlaySound(string soundName, string botName, int id)
        {
            Log.Debug("playsound "+ id);       
            foreach (var player in AudioPlayers)
            {
                Log.Debug(AudioPlayers.Count + " "+ AudioPlayers);
                if (AudioPlayers.Any(x => x.nicknameSync.Network_myNickSync.Equals(botName)))
                    return false;
            }

            string fullPath = Path.Combine(Config.path, soundName);
            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            FakeConnection fakeConnection = new FakeConnection(id);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
            Log.Debug("after connecting fake player");
            hubPlayer.nicknameSync.Network_myNickSync = botName;
            AudioPlayerBase audioPlayer = AudioPlayerBase.Get(hubPlayer);
            AudioPlayers.Add(hubPlayer);
            audioPlayer.Enqueue(fullPath, -1);
            audioPlayer.Volume = Config.volume;
            foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List.Where(x => !MutedAnnounce.Contains(x.Id)))
            {
                audioPlayer.BroadcastTo.Add(player.Id);
            }
            audioPlayer.BroadcastTo.Add(hubPlayer.PlayerId);
            foreach (var pl in audioPlayer.BroadcastTo)
            {
                Log.Debug(pl);
            }
            audioPlayer.Play(0);
            return true;

        }
    }
}
