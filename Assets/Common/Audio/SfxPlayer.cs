using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Quem toca os efeitos sonoros. Nasce sozinho ao dar Play, atravessa as
    /// cenas e guarda um punhado de vozes: cada <see cref="SfxCue"/> tocado
    /// pega uma livre (ou a mais velha, se todas estiverem ocupadas).
    ///
    /// Ao abrir uma cena, prende o <see cref="ScrapSounds"/> no Scrap (passos,
    /// quedas, a lixa). Nenhuma cena precisa ser regerada.
    ///
    /// Os sons tocam mesmo com o tempo parado (o card de habilidade para o jogo).
    /// </summary>
    public class SfxPlayer : MonoBehaviour
    {
        private const int VoiceCount = 24;

        /// <summary>Ate esta distancia da camera (em tiles) um som do mapa soa cheio.</summary>
        private const float Near = 9f;

        /// <summary>A partir desta, silencio.</summary>
        private const float Far = 22f;

        private static SfxPlayer _instance;
        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();

        private readonly AudioSource[] _voices = new AudioSource[VoiceCount];
        private readonly float[] _startedAt = new float[VoiceCount];
        private readonly Dictionary<SfxCue, float> _lastPlayed = new Dictionary<SfxCue, float>();
        private readonly Dictionary<SfxCue, int> _lastVariant = new Dictionary<SfxCue, int>();

        private Camera _camera;
        private float _lastInterface = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
            Clips.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (_instance != null) return;

            var host = new GameObject("SfxPlayer");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<SfxPlayer>();
        }

        private void Awake()
        {
            for (int i = 0; i < VoiceCount; i++)
            {
                var voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.loop = false;
                voice.spatialBlend = 0f;
                voice.priority = 64;
                _voices[i] = voice;
                _startedAt[i] = -100f;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_instance == this) _instance = null;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (ScrapController scrap in FindObjectsByType<ScrapController>())
            {
                if (scrap.GetComponent<ScrapSounds>() == null) scrap.gameObject.AddComponent<ScrapSounds>();
            }
        }

        /// <summary>Se tocou um som de interface ou de coleta ha menos de <paramref name="seconds"/>.</summary>
        public static bool InterfaceSoundedWithin(float seconds)
        {
            return _instance != null && Time.unscaledTime - _instance._lastInterface < seconds;
        }

        /// <summary>Carrega (uma vez so) um arquivo de Resources/Sfx.</summary>
        public static AudioClip Load(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Clips.TryGetValue(name, out AudioClip cached)) return cached;

            AudioClip clip = Resources.Load<AudioClip>("Sfx/" + name);
            if (clip == null) Debug.LogWarning("[Som] Efeito nao encontrado: Resources/Sfx/" + name);
            Clips[name] = clip;
            return clip;
        }

        public static void Play(SfxCue cue, Vector2? at, float volume, float pitch, float delay)
        {
            if (_instance != null && cue != null) _instance.PlayCue(cue, at, volume, pitch, delay);
        }

        private void PlayCue(SfxCue cue, Vector2? at, float volume, float pitch, float delay)
        {
            float now = Time.unscaledTime;
            if (_lastPlayed.TryGetValue(cue, out float last) && now - last < cue.Cooldown) return;

            float gain = 1f;
            float pan = 0f;

            if (at.HasValue)
            {
                if (_camera == null || !_camera.isActiveAndEnabled) _camera = Camera.main;
                if (_camera != null)
                {
                    Vector2 offset = at.Value - (Vector2)_camera.transform.position;
                    float near = 1f - Mathf.Clamp01((offset.magnitude - Near) / (Far - Near));
                    gain = near * near;
                    pan = Mathf.Clamp(offset.x / 12f, -1f, 1f) * 0.5f;
                }
            }

            if (gain <= 0.01f) return;

            AudioClip clip = Load(PickClip(cue));
            if (clip == null) return;

            _lastPlayed[cue] = now;
            if (cue.Interface) _lastInterface = now;

            int index = FreeVoice();
            AudioSource voice = _voices[index];
            voice.Stop();
            voice.clip = clip;
            voice.volume = Mathf.Clamp01(Sfx.Volume * cue.Volume * volume * gain);
            voice.pitch = cue.Pitch * pitch * (1f + Random.Range(-cue.PitchJitter, cue.PitchJitter));
            voice.panStereo = pan;
            _startedAt[index] = now + delay;

            if (delay > 0f) voice.PlayDelayed(delay);
            else voice.Play();
        }

        /// <summary>O arquivo desta vez: com variacoes, uma qualquer menos a ultima.</summary>
        private string PickClip(SfxCue cue)
        {
            if (cue.Variants <= 1) return cue.Clip;

            _lastVariant.TryGetValue(cue, out int previous);
            int pick = Random.Range(1, cue.Variants + 1);
            if (pick == previous) pick = pick % cue.Variants + 1;

            _lastVariant[cue] = pick;
            return cue.Clip + "_" + pick;
        }

        /// <summary>Uma voz parada; se nao houver, a que comecou ha mais tempo.</summary>
        private int FreeVoice()
        {
            int oldest = 0;
            for (int i = 0; i < VoiceCount; i++)
            {
                if (!_voices[i].isPlaying && Time.unscaledTime >= _startedAt[i]) return i;
                if (_startedAt[i] < _startedAt[oldest]) oldest = i;
            }

            return oldest;
        }
    }
}
