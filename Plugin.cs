using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using LiteDB;
using MEC;
using Mirror;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AudioPlayer
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Antoniofo";

        public override string Name => "AudioPlayer";

        public override Version Version => new Version(2, 0, 0);

        public override Version RequiredExiledVersion => new Version(8, 11, 0);

        public override string Prefix => "audioplayer";

        public static List<ReferenceHub> AudioPlayers = new List<ReferenceHub>();

        public static Plugin instance;

        public List<string> MutedAnnounce;

        public override void OnEnabled()
        {            
            SCPSLAudioApi.Startup.SetupDependencies();
            Plugin.instance = this;
            MutedAnnounce = new List<string>();
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {            
            Plugin.instance = null;
            MutedAnnounce = null;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawnTeam;
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= OnFinishedTrack;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= OnNTFAnnounce;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            base.OnDisabled();
        }

        private void OnRoundStart()
        {
            AudioPlayers.Clear();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {

            using (var playerRepo = new PlayerRepository(Config.DatabaseFilePath))
            {
                PlayerDB playerdb = playerRepo.GetPlayerByUserId(ev.Player.UserId);
                if (playerdb != null)
                {
                    if (playerdb.Mute == 2 && !MutedAnnounce.Contains(ev.Player.UserId))
                    {
                        MutedAnnounce.Add(ev.Player.UserId);
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
            Stop(playerBase);

        }

        public void Stop(AudioPlayerBase playerBase)
        {
            var player = playerBase.Owner;
            Log.Debug("Track Finished");
            if (playerBase.CurrentPlay != null)
            {
                playerBase.Stoptrack(true);
                playerBase.OnDestroy();
            }

            if (player.gameObject != null)
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

            foreach (var pla in AudioPlayers)
            {
                var audioplayer = AudioPlayerBase.Get(pla);
                if (audioplayer.CurrentPlay == null)
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
            Log.Debug("playsound " + id);
            foreach (var player in AudioPlayers)
            {
                Log.Debug("audioplayers: " + AudioPlayers.Count);
                if (AudioPlayers.Any(x => x.nicknameSync.Network_myNickSync.Equals(botName) && AudioPlayerBase.Get(x).PlaybackCoroutine.IsRunning))
                    return false;
            }

            string fullPath = url ? soundName : Path.Combine(Config.AudioFilePath, soundName);
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
