using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Toca as musicas da trilha com duas fontes, pra uma entrar enquanto a
    /// outra sai: troca de musica e a volta do loop sao sempre em fade.
    ///
    /// Quem usa pede a musica que quer agora (<see cref="Want"/>) todo quadro;
    /// pedir a mesma de novo nao reinicia.
    /// </summary>
    public sealed class MusicPlayer
    {
        private sealed class Voice
        {
            public AudioSource Source;
            public MusicTrack Track;
            public float Level;
            public float Target;
            public float Speed;
            public float Since;
        }

        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();

        private readonly Voice[] _voices = new Voice[2];
        private Voice _current;

        /// <summary>A musica tocando agora (a que entrou por ultimo), ou null.</summary>
        public MusicTrack Current => _current != null ? _current.Track : null;

        /// <summary>A ultima musica de tocar uma vez so que chegou ao fim (e nao foi pedida outra depois).</summary>
        private MusicTrack _finished;

        /// <summary>Se esta musica (de tocar uma vez so) ja tocou ate o fim.</summary>
        public bool HasFinished(MusicTrack track) => track != null && _finished == track;

        /// <summary>Multiplica o volume de tudo (o volume geral e o da cena).</summary>
        public float Volume { get; set; } = 1f;

        public MusicPlayer(GameObject host)
        {
            for (int i = 0; i < _voices.Length; i++)
            {
                var source = host.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.priority = 0;
                source.volume = 0f;
                _voices[i] = new Voice { Source = source };
            }
        }

        /// <summary>Carrega uma musica pelo nome (cache). Null se o arquivo nao existe.</summary>
        public static AudioClip Load(string clip)
        {
            if (string.IsNullOrEmpty(clip)) return null;
            if (Clips.TryGetValue(clip, out AudioClip cached) && cached != null) return cached;

            AudioClip loaded = Resources.Load<AudioClip>("Soundtrack/Music/" + clip);
            if (loaded == null) Debug.LogWarning("[Som] Musica nao encontrada: Resources/Soundtrack/Music/" + clip);
            Clips[clip] = loaded;
            return loaded;
        }

        /// <summary>
        /// A musica que deve tocar agora. Null: silencio (a atual sai em fade).
        /// Com <paramref name="restart"/>, recomeca mesmo se ja for ela.
        /// </summary>
        public void Want(MusicTrack track, float fade = SoundtrackCatalog.CrossfadeTime, bool restart = false)
        {
            if (!restart && _current != null && _current.Track == track) return;
            // A que ja tocou ate o fim nao volta sozinha.
            if (!restart && track != null && track == _finished) return;
            if (track == null && _current == null) return;

            if (_current != null) FadeOut(_current, fade);
            _current = null;
            _finished = null;

            if (track == null) return;

            AudioClip clip = Load(track.Clip);
            if (clip == null)
            {
                // Sem o arquivo, finge que tocou: quem pediu nao fica esperando pra sempre.
                if (!track.Loop) _finished = track;
                return;
            }

            Voice voice = FreeVoice();
            Begin(voice, track, clip, track.Start, fade);
            _current = voice;
        }

        /// <summary>Tira tudo, em fade.</summary>
        public void Stop(float fade)
        {
            foreach (Voice voice in _voices) FadeOut(voice, fade);
            _current = null;
        }

        public void Update(float deltaTime)
        {
            foreach (Voice voice in _voices)
            {
                if (voice.Track == null) continue;

                voice.Level = Mathf.MoveTowards(voice.Level, voice.Target, voice.Speed * deltaTime);
                voice.Source.volume = voice.Level * voice.Track.Volume * Volume;

                if (voice.Target <= 0f && voice.Level <= 0f)
                {
                    voice.Source.Stop();
                    voice.Track = null;
                    continue;
                }

                if (voice != _current) continue;

                AudioClip clip = voice.Source.clip;
                if (clip == null) continue;

                // Logo depois do Play a fonte pode ainda nao ter comecado (streaming): espera um pouco antes de julgar.
                bool settled = Time.unscaledTime - voice.Since > 1f;

                if (voice.Track.Loop)
                {
                    // Chegando no ponto de volta: a outra fonte entra no comeco do loop.
                    float end = voice.Track.LoopEnd > 0f ? Mathf.Min(voice.Track.LoopEnd, clip.length) : clip.length;
                    if (voice.Source.time >= end - SoundtrackCatalog.LoopFadeTime || (settled && !voice.Source.isPlaying))
                    {
                        Voice next = Other(voice);
                        FadeOut(voice, SoundtrackCatalog.LoopFadeTime);
                        Begin(next, voice.Track, clip, voice.Track.LoopFrom, SoundtrackCatalog.LoopFadeTime);
                        _current = next;
                    }
                }
                else if (settled && !voice.Source.isPlaying)
                {
                    _finished = voice.Track;
                    voice.Track = null;
                    _current = null;
                }
            }
        }

        private void Begin(Voice voice, MusicTrack track, AudioClip clip, float at, float fade)
        {
            voice.Source.Stop();
            voice.Source.clip = clip;
            voice.Source.volume = 0f;
            voice.Source.Play();
            voice.Source.time = Mathf.Clamp(at, 0f, Mathf.Max(0f, clip.length - 1f));
            voice.Since = Time.unscaledTime;
            voice.Track = track;
            voice.Level = 0f;
            voice.Target = 1f;
            voice.Speed = 1f / Mathf.Max(0.05f, fade);
            voice.Source.volume = 0f;
        }

        private static void FadeOut(Voice voice, float fade)
        {
            if (voice == null || voice.Track == null) return;

            voice.Target = 0f;
            voice.Speed = Mathf.Max(voice.Level, 0.01f) / Mathf.Max(0.05f, fade);
        }

        private Voice FreeVoice()
        {
            // A que estiver mais baixa: a outra pode estar saindo em fade.
            return _voices[0].Track == null ? _voices[0]
                : _voices[1].Track == null ? _voices[1]
                : _voices[0].Level <= _voices[1].Level ? _voices[0] : _voices[1];
        }

        private Voice Other(Voice voice)
        {
            return voice == _voices[0] ? _voices[1] : _voices[0];
        }
    }
}
