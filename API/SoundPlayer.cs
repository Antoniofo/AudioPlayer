using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using MEC;
using NVorbis;
using UnityEngine;
using UnityEngine.Networking;
using UserSettings.ServerSpecific;

namespace AudioPlayerManager.API
{
    public static class SoundPlayer
    {
        public static void PlayGlobalAudio(string clip, bool fromWeb)
        {
            if (!fromWeb)
            {
                if (!AudioClipStorage.AudioClips.ContainsKey(clip))
                    AudioClipStorage.LoadClip(Path.Combine(Plugin.Instance.Config.AudioFilePath, clip + ".ogg"));
            }
            else
            {
                string[] sub = clip.Split('/');
                if (!AudioClipStorage.AudioClips.ContainsKey(sub[sub.Length - 1]))
                    Timing.RunCoroutine(LoadClip(clip));
            }


            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet("Global AudioPlayer",
                onIntialCreation: (p) => { p.AddSpeaker("Main", isSpatial: false, maxDistance: 5000f); },
                condition: ShouldPlay);

            audioPlayer.AddClip(Path.GetFileNameWithoutExtension(clip));

            //Tests
#if DEBUG
            Dictionary<string, AudioClipData>.Enumerator itr = AudioClipStorage.AudioClips.GetEnumerator();
            while (itr.MoveNext())
            {
                Log.Debug("Global CLips: " + itr.Current.Key + " : " + itr.Current.Value.Name);
            }

            Dictionary<int, AudioClipPlayback>.Enumerator itr2 = audioPlayer.ClipsById.GetEnumerator();
            while (itr2.MoveNext())
            {
                Log.Debug("AudioPlayer CLips: " + itr2.Current.Key + " : " + itr2.Current.Value.Clip);
            }
#endif
        }

        public static void PlayLocalAudio(string clip, bool fromWeb, Vector3 position, int distance)
        {
            if (!fromWeb)
            {
                if (!AudioClipStorage.AudioClips.ContainsKey(clip))
                    AudioClipStorage.LoadClip(Path.Combine(Plugin.Instance.Config.AudioFilePath, clip + ".ogg"));
            }
            else
            {
                string[] sub = clip.Split('/');
                if (!AudioClipStorage.AudioClips.ContainsKey(sub[sub.Length - 1]))
                    Timing.RunCoroutine(LoadClip(clip));
            }


            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Local player", onIntialCreation: (p) =>
            {
                // This created speaker will be in 3D space.
                p.AddSpeaker("Main", position: position, isSpatial: true, minDistance: 5f, maxDistance: distance);
            }, condition: ShouldPlay);

            audioPlayer.AddClip(Path.GetFileNameWithoutExtension(clip));

            //Tests
#if DEBUG
            Dictionary<string, AudioClipData>.Enumerator itr = AudioClipStorage.AudioClips.GetEnumerator();
            while (itr.MoveNext())
            {
                Log.Debug("Global CLips: " + itr.Current.Key + " : " + itr.Current.Value.Name);
            }

            Dictionary<int, AudioClipPlayback>.Enumerator itr2 = audioPlayer.ClipsById.GetEnumerator();
            while (itr2.MoveNext())
            {
                Log.Debug("AudioPlayer CLips: " + itr2.Current.Key + " : " + itr2.Current.Value.Clip);
            }
#endif
        }

        public static IEnumerator<float> LoadClip(string url, string name = null)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                yield return Timing.WaitUntilDone(uwr.SendWebRequest());

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    ServerConsole.AddLog($"[AudioPlayer] Failed loading clip from {url}: {uwr.error}");
                    yield break;
                }

                byte[] audioData = uwr.downloadHandler.data;

                // Infer name if not provided
                if (string.IsNullOrEmpty(name))
                    name = Path.GetFileNameWithoutExtension(url);

                // Check for duplicates
                if (AudioClipStorage.AudioClips.ContainsKey(name))
                {
                    ServerConsole.AddLog($"[AudioPlayer] Clip {name} already loaded from {url}.");
                    yield break;
                }

                // Determine extension
                string extension = Path.GetExtension(url).ToLowerInvariant();

                float[] samples = null;
                int sampleRate = 0;
                int channels = 0;

                switch (extension)
                {
                    case ".ogg":
                        using (var stream = new MemoryStream(audioData))
                        using (var reader = new VorbisReader(stream, false))
                        {
                            sampleRate = reader.SampleRate;
                            channels = reader.Channels;

                            samples = new float[reader.TotalSamples * channels];
                            reader.ReadSamples(samples);
                        }

                        break;
                    default:
                        ServerConsole.AddLog($"[AudioPlayer] Unsupported format: {extension}");
                        yield break;
                }

                // Store the clip
                AudioClipStorage.AudioClips.Add(name, new AudioClipData(name, sampleRate, channels, samples));
                ServerConsole.AddLog($"[AudioPlayer] Successfully loaded clip {name} from {url}.");
            }
        }

        public static bool ShouldPlay(ReferenceHub hub)
        {
            Player player = Player.Get(hub);

            SettingBase.TryGetSetting(player, Plugin.Instance.Config.SettingId, out SettingBase settings);
            SSTwoButtonsSetting setting =
                ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(player.ReferenceHub,
                    Plugin.Instance.Config.SettingId);

            return setting.SyncIsB;
        }
        /*
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
                if (setting.SyncIsB)
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
        }*/
    }
}