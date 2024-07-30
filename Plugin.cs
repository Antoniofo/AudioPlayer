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

        public override Version Version => new Version(1, 3, 0);

        public override string Prefix => "audioplayer";

        public static List<ReferenceHub> AudioPlayers = new List<ReferenceHub>();        

        public static Plugin instance;

        public List<string> MutedAnnounce;

        public override void OnEnabled()
        {
            base.OnEnabled();
            SCPSLAudioApi.Startup.SetupDependencies();
            Plugin.instance = this;
            MutedAnnounce = new List<string>();
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += OnFinishedTrack;            
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Plugin.instance = null;
            MutedAnnounce = null;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
        }

        private void OnRoundStart()
        {
            AudioPlayers.Clear();               
        }

        private void OnNTFAnnounce(AnnouncingNtfEntranceEventArgs obj)
        {
            if(Config.PlayMtfSound)
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
            }
        }        

        private void OnRespawnTeam(RespawningTeamEventArgs ev)
        {
            Log.Debug("Respawn");
            if (ev.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox && Config.PlayMtfSound)
            {
                Log.Debug("MTF");

                PlaySound(Config.MtfSoundFilePath, "Facility Announcement", 998, false);
            }
            else if (ev.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency && Config.PlayChaosSound)
            {
                Log.Debug("Chaos");
                
                PlaySound(Config.ChaosSoundFilePath, "Facility Announcement", 998, false);
            }
        }

        public bool PlaySound(string soundName, string botName, int id, bool url)
        {
            Log.Debug("playsound "+ id);
            foreach (var player in AudioPlayers)
            {
                Log.Debug("audioplayers: " + AudioPlayers.Count);
                if (AudioPlayers.Any(x => x.nicknameSync.Network_myNickSync.Equals(botName) && AudioPlayerBase.Get(x).PlaybackCoroutine.IsRunning))
                    return false;
            }           

            string fullPath =  url ? soundName : Path.Combine(Config.AudioFilePath, soundName);
            if (!File.Exists(fullPath) && !url)
            {
                return false;
            }
            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Exiled.API.Features.Components.FakeConnection fakeConnection = new Exiled.API.Features.Components.FakeConnection(id);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);            

            hubPlayer.nicknameSync.Network_myNickSync = botName;
            AudioPlayerBase audioPlayer = AudioPlayerBase.Get(hubPlayer);
            AudioPlayers.Add(hubPlayer);
            audioPlayer.Enqueue(fullPath, -1);
            audioPlayer.Volume = Config.Volume;
            audioPlayer.AllowUrl = url;
            foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List.Where(x => !MutedAnnounce.Contains(x.UserId)))
            {
                audioPlayer.BroadcastTo.Add(player.Id);
            }
            audioPlayer.BroadcastTo.Add(hubPlayer.PlayerId);
            foreach (var pl in audioPlayer.BroadcastTo)
            {
                Log.Debug(pl);
            }
            audioPlayer.Play(0);

            //Cleanup audioplayer that crashes cause i didn't find a way to use the audioapi to error handle that
            List<ReferenceHub> listofshit = AudioPlayers.Where(x => !x.nicknameSync.Network_myNickSync.Equals("Facility Announcement")).ToList();
            for (int i = 0; i < listofshit.Count; i++)
            {
                var audioPlayerToStop = AudioPlayerBase.Get(listofshit[i]);
                if (!audioPlayerToStop.PlaybackCoroutine.IsRunning)
                {
                    instance.Stop(audioPlayerToStop);
                }                                
            }
            return true;

        }
    }
}
