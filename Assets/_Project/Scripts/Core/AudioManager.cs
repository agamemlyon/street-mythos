using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StreetMythos.Core
{
    // Sons du jeu : bruitages courts dans Resources/Sons (dans le build), musiques lues à la demande
    // depuis StreamingAssets/music (hors budget de démarrage). Persistant entre les scènes.
    public sealed class AudioManager : MonoBehaviour
    {
        static AudioManager _instance;
        readonly Dictionary<string, AudioClip> _sfx = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, AudioClip> _music = new Dictionary<string, AudioClip>();
        AudioSource _sfxSource, _musicSource;
        string _currentMusic;

        public static float SfxVolume = 0.8f, MusicVolume = 0.45f;

        static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Audio");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AudioManager>();
                    _instance._sfxSource = go.AddComponent<AudioSource>();
                    _instance._musicSource = go.AddComponent<AudioSource>();
                    _instance._musicSource.loop = true;
                    foreach (var c in Resources.LoadAll<AudioClip>("Sons")) _instance._sfx[c.name] = c;
                }
                return _instance;
            }
        }

        // Joue un bruitage ; un nom sans variante tire au hasard parmi « nom_1 », « nom_2 »… s'ils existent
        public static void Play(string name, float volume = 1f)
        {
            var a = Instance;
            if (!a._sfx.TryGetValue(name, out var clip))
            {
                var variants = new List<AudioClip>();
                for (int i = 0; i < 6; i++)
                    if (a._sfx.TryGetValue($"{name}_{i}", out var v)) variants.Add(v);
                if (variants.Count == 0) return;
                clip = variants[Random.Range(0, variants.Count)];
            }
            a._sfxSource.pitch = Random.Range(0.94f, 1.06f);
            a._sfxSource.PlayOneShot(clip, volume * SfxVolume);
        }

        // Change de musique (fondu) ; le fichier est chargé une seule fois
        public static void Music(string track)
        {
            var a = Instance;
            if (a._currentMusic == track) return;
            a._currentMusic = track;
            a.StopAllCoroutines();
            a.StartCoroutine(a.SwitchMusic(track));
        }

        IEnumerator SwitchMusic(string track)
        {
            for (float v = _musicSource.volume; v > 0; v -= Time.unscaledDeltaTime * 1.5f)
            {
                _musicSource.volume = v;
                yield return null;
            }
            _musicSource.Stop();
            if (string.IsNullOrEmpty(track)) yield break;

            if (!_music.TryGetValue(track, out var clip))
            {
                string url = System.IO.Path.Combine(Application.streamingAssetsPath, "music", track + ".mp3");
                if (!url.Contains("://")) url = "file://" + url;
                using (var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
                {
                    yield return req.SendWebRequest();
                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"[Audio] musique {track} introuvable : {req.error}");
                        yield break;
                    }
                    clip = DownloadHandlerAudioClip.GetContent(req);
                    _music[track] = clip;
                }
            }
            if (_currentMusic != track) yield break;
            _musicSource.clip = clip;
            _musicSource.volume = 0;
            _musicSource.Play();
            for (float v = 0; v < MusicVolume; v += Time.unscaledDeltaTime * 0.8f)
            {
                _musicSource.volume = v;
                yield return null;
            }
            _musicSource.volume = MusicVolume;
        }
    }
}
