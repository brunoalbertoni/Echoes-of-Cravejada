using System;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um som preso a uma coisa do mapa (o vapor de um cano, uma caldeira, uma
    /// poca de acido, um cristal): toca em loop e cresce quando a camera chega
    /// perto, puxando pro lado em que a coisa esta.
    ///
    /// O SoundDirector poe estes sozinho quando a cena abre; nada precisa ser
    /// gerado de novo.
    /// </summary>
    public class AmbientEmitter : MonoBehaviour
    {
        private AudioSource _source;
        private Func<float> _strength;
        private float _volume;
        private float _inner;
        private float _outer;
        private float _level;
        private bool _paused;
        private bool _started;

        private static Camera _camera;

        /// <summary>
        /// Prende um som em loop a <paramref name="target"/>.
        /// </summary>
        /// <param name="volume">Volume quando a camera esta em cima (0 a 1).</param>
        /// <param name="inner">Ate esta distancia (em tiles), volume cheio.</param>
        /// <param name="outer">A partir desta, silencio.</param>
        /// <param name="strength">Quanto ele esta soando agora (0 a 1): o vapor desligado, a poca seca. Null: sempre.</param>
        public static AmbientEmitter Attach(GameObject target, AudioClip clip, float volume, float inner, float outer,
            Func<float> strength = null)
        {
            if (target == null || clip == null) return null;

            var emitter = target.AddComponent<AmbientEmitter>();
            var source = target.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.priority = 160;
            // Cada um num ponto diferente do loop e num tom um pouco diferente: varios juntos nao soam como um so.
            source.pitch = UnityEngine.Random.Range(0.94f, 1.06f);

            emitter._source = source;
            emitter._volume = volume;
            emitter._inner = inner;
            emitter._outer = Mathf.Max(inner + 0.5f, outer);
            emitter._strength = strength;
            return emitter;
        }

        private void Update()
        {
            if (_source == null) return;

            if (_camera == null || !_camera.isActiveAndEnabled) _camera = Camera.main;
            if (_camera == null) return;

            Vector2 offset = (Vector2)(transform.position - _camera.transform.position);
            float distance = offset.magnitude;
            float near = 1f - Mathf.Clamp01((distance - _inner) / (_outer - _inner));
            near *= near;

            float strength = 1f;
            if (_strength != null)
            {
                try { strength = Mathf.Clamp01(_strength()); }
                catch (Exception) { strength = 0f; }
            }

            float target = near * strength * _volume * SoundDirector.EmitterLevel;
            _level = Mathf.MoveTowards(_level, target, Time.unscaledDeltaTime * 1.5f);

            _source.volume = _level;
            _source.panStereo = Mathf.Clamp(offset.x / _outer, -1f, 1f) * 0.6f;

            // Longe demais: para de tocar (nao gasta voz), e volta de onde estava.
            if (_level > 0.001f)
            {
                if (_paused)
                {
                    _source.UnPause();
                }
                else if (!_source.isPlaying)
                {
                    _source.Play();
                    if (!_started) _source.time = UnityEngine.Random.Range(0f, Mathf.Max(0f, _source.clip.length - 0.1f));
                }

                _started = true;
                _paused = false;
            }
            else if (_source.isPlaying)
            {
                _source.Pause();
                _paused = true;
            }
        }
    }
}
