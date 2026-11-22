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
        private AudioSource _fonte;
        private Func<float> _forca;
        private float _volume;
        private float _interno;
        private float _externo;
        private float _nivel;
        private bool _pausado;
        private bool _iniciado;

        private static Camera _camera;

        /// <summary>
        /// Prende um som em loop a <paramref name="alvo"/>.
        /// </summary>
        /// <param name="volume">Volume quando a camera esta em cima (0 a 1).</param>
        /// <param name="interno">Ate esta distancia (em tiles), volume cheio.</param>
        /// <param name="externo">A partir desta, silencio.</param>
        /// <param name="forca2">Quanto ele esta soando agora (0 a 1): o vapor desligado, a poca seca. Null: sempre.</param>
        public static AmbientEmitter Prender(GameObject alvo, AudioClip clipe, float volume, float interno, float externo,
            Func<float> forca2 = null)
        {
            if (alvo == null || clipe == null) return null;

            var emissor = alvo.AddComponent<AmbientEmitter>();
            var fonte = alvo.AddComponent<AudioSource>();
            fonte.clip = clipe;
            fonte.loop = true;
            fonte.playOnAwake = false;
            fonte.spatialBlend = 0f;
            fonte.volume = 0f;
            fonte.priority = 160;
            // Cada um num ponto diferente do loop e num tom um pouco diferente: varios juntos nao soam como um so.
            fonte.pitch = UnityEngine.Random.Range(0.94f, 1.06f);

            emissor._fonte = fonte;
            emissor._volume = volume;
            emissor._interno = interno;
            emissor._externo = Mathf.Max(interno + 0.5f, externo);
            emissor._forca = forca2;
            return emissor;
        }

        private void Update()
        {
            if (_fonte == null) return;

            if (_camera == null || !_camera.isActiveAndEnabled) _camera = Camera.main;
            if (_camera == null) return;

            Vector2 deslocamento = (Vector2)(transform.position - _camera.transform.position);
            float distancia = deslocamento.magnitude;
            float perto = 1f - Mathf.Clamp01((distancia - _interno) / (_externo - _interno));
            perto *= perto;

            float forca2 = 1f;
            if (_forca != null)
            {
                try { forca2 = Mathf.Clamp01(_forca()); }
                catch (Exception) { forca2 = 0f; }
            }

            float alvo = perto * forca2 * _volume * SoundDirector.NivelEmissor;
            _nivel = Mathf.MoveTowards(_nivel, alvo, Time.unscaledDeltaTime * 1.5f);

            _fonte.volume = _nivel * GameAudio.NivelEfeitos;
            _fonte.panStereo = Mathf.Clamp(deslocamento.x / _externo, -1f, 1f) * 0.6f;

            // Longe demais: para de tocar (nao gasta voz), e volta de onde estava.
            if (_nivel > 0.001f)
            {
                if (_pausado)
                {
                    _fonte.UnPause();
                }
                else if (!_fonte.isPlaying)
                {
                    _fonte.Play();
                    if (!_iniciado) _fonte.time = UnityEngine.Random.Range(0f, Mathf.Max(0f, _fonte.clip.length - 0.1f));
                }

                _iniciado = true;
                _pausado = false;
            }
            else if (_fonte.isPlaying)
            {
                _fonte.Pause();
                _pausado = true;
            }
        }
    }
}
