using Exiled.API.Features;
using LiteDB;
using MEC;
using Mirror;
using SCPSLAudioApi.AudioCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features.Core.UserSettings;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace AudioPlayer.API
{
    public class SoundPlayer
    {
        public static List<ReferenceHub> AudioPlayers = new List<ReferenceHub>();

        public static void Stop(AudioPlayerBase playerBase)
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
                Timing.CallDelayed(0.5f, () => { NetworkServer.Destroy(player.gameObject); });
            }

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

        public static bool PlaySound(string soundName, string botName, int id, bool url)
        {
            foreach (var player in AudioPlayers)
            {
                if (AudioPlayers.Any(x =>
                        x.nicknameSync.Network_myNickSync.Equals(botName) &&
                        AudioPlayerBase.Get(x).PlaybackCoroutine.IsRunning))
                    return false;
            }

            string fullPath = url ? soundName : Path.Combine(Plugin.Instance.Config.AudioFilePath, soundName);
            if (!File.Exists(fullPath) && !url)
            {
                return false;
            }

            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Exiled.API.Features.Components.FakeConnection fakeConnection =
                new Exiled.API.Features.Components.FakeConnection(id);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);

            hubPlayer.nicknameSync.Network_myNickSync = botName;
            AudioPlayerBase audioPlayer = AudioPlayerBase.Get(hubPlayer);
            AudioPlayers.Add(hubPlayer);
            audioPlayer.Enqueue(fullPath, -1);
            audioPlayer.Volume = Plugin.Instance.Config.Volume;
            audioPlayer.AllowUrl = url;

            foreach (Player player in Player.List)
            {
                SettingBase.TryGetSetting(player, Plugin.Instance.Config.SettingId, out SettingBase settings);
                SSTwoButtonsSetting setting =
                    ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(player.ReferenceHub,
                        Plugin.Instance.Config.SettingId);
                if (setting.SyncIsA)
                {
                    audioPlayer.BroadcastTo.Add(player.Id);
                }
            }

            audioPlayer.BroadcastTo.Add(hubPlayer.PlayerId);
            audioPlayer.Play(0);

            //Cleanup audioplayer that crashes cause i didn't find a way to use the audioapi to error handle that
            List<ReferenceHub> listofshit = AudioPlayers;
            for (int i = 0; i < listofshit.Count; i++)
            {
                var audioPlayerToStop = AudioPlayerBase.Get(listofshit[i]);
                if (!audioPlayerToStop.PlaybackCoroutine.IsRunning)
                {
                    Stop(audioPlayerToStop);
                }
            }

            return true;
        }

        public static bool PlaySoundAtPlace(string soundName, Vector3 place, float distance, string botName, int id,
            bool url)
        {
            foreach (var player in AudioPlayers)
            {
                if (AudioPlayers.Any(x =>
                        x.nicknameSync.Network_myNickSync.Equals(botName) &&
                        AudioPlayerBase.Get(x).PlaybackCoroutine.IsRunning))
                    return false;
            }

            string fullPath = url ? soundName : Path.Combine(Plugin.Instance.Config.AudioFilePath, soundName);
            if (!File.Exists(fullPath) && !url)
            {
                return false;
            }

            var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Exiled.API.Features.Components.FakeConnection fakeConnection =
                new Exiled.API.Features.Components.FakeConnection(id);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);


            hubPlayer.nicknameSync.Network_myNickSync = botName;
            AudioPlayerBase audioPlayer = AudioPlayerBase.Get(hubPlayer);
            AudioPlayers.Add(hubPlayer);
            audioPlayer.Enqueue(fullPath, -1);
            audioPlayer.Volume = Plugin.Instance.Config.Volume;
            audioPlayer.AllowUrl = url;

            foreach (Player player in Player.List)
            {
                if (Vector3.Distance(player.Position, place) < distance)
                {
                    audioPlayer.BroadcastTo.Add(player.Id);
                }
            }

            audioPlayer.BroadcastTo.Add(hubPlayer.PlayerId);
            audioPlayer.Play(0);

            //Cleanup audioplayer that crashes cause i didn't find a way to use the audioapi to error handle that
            List<ReferenceHub> listofshit = AudioPlayers;
            for (int i = 0; i < listofshit.Count; i++)
            {
                var audioPlayerToStop = AudioPlayerBase.Get(listofshit[i]);
                if (!audioPlayerToStop.PlaybackCoroutine.IsRunning)
                {
                    Stop(audioPlayerToStop);
                }
            }

            return true;
        }
    }
}