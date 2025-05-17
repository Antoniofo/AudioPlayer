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
        public static List<AudioPlayer> audioPlayers = new List<AudioPlayer>();

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


            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet(
                $"Local {(int)position.x}{(int)position.y}{(int)position.z}", onIntialCreation: (p) =>
                {
                    // This created speaker will be in 3D space.
                    p.AddSpeaker("Main", position: position, isSpatial: true, minDistance: 5f, maxDistance: distance);
                }, condition: ShouldPlay);

            audioPlayers.Add(audioPlayer);
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
    }
}