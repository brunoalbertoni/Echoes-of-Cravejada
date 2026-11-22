using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um efeito em loop preso a uma coisa que o controla: o motor da broca,
    /// o raio do Rei, o som de segurar E no Altar. Quem o prende diz, todo
    /// quadro, o quanto ele deve soar (<see cref="Nivel"/>) e em que tom
    /// (<see cref="Tom"/>); ele chega la suave e some com a distancia da
    /// camera, como os outros sons do mapa.
    ///
    /// Diferente do <see cref="AmbientEmitter"/>, que e ambiente e baixa na
    /// luta, este e parte da acao.
    /// </summary>
    public class SfxLoop : MonoBehaviour
    {
        /// <summary>O quanto ele soa agora (0 a 1).</summary>
        public float Nivel { get; set; }

        /// <summary>O tom (1 = o do arquivo).</summary>
        public float Tom { get; set; } = 1f;

        /// <summary>Volume por segundo com que ele chega ao nivel pedido.</summary>
        public float Esmaecimento { get; set; } = 4f;

        /// <summary>Som da tela (true) ou daquele ponto do mapa (false).</summary>
        public bool Tela { get; set; }

        private AudioSource _fonte;
        private SfxCue _deixa;
        private float _volume;

        /// <summary>Prende o loop de <paramref name="deixa"/> em <paramref name="alvo"/>, calado.</summary>
        public static SfxLoop Prender(GameObject alvo, SfxCue deixa)
        {
            if (alvo == null || deixa == null) return null;

            var repeticao = alvo.AddComponent<SfxLoop>();
            var fonte = alvo.AddComponent<AudioSource>();
            fonte.clip = SfxPlayer.Carregar(deixa.Clipe);
            fonte.loop = true;
            fonte.playOnAwake = false;
            fonte.spatialBlend = 0f;
            fonte.volume = 0f;
            fonte.priority = 80;

            repeticao._fonte = fonte;
            repeticao._deixa = deixa;
            return repeticao;
        }

        /// <summary>Cala na hora, sem desbotar.</summary>
        public void Silenciar()
        {
            Nivel = 0f;
            _volume = 0f;
            if (_fonte != null) _fonte.Stop();
        }

        private void Update()
        {
            if (_fonte == null || _fonte.clip == null) return;

            float panorama = 0f;
            float ganho = Tela ? 1f : SfxPlayer.Distancia(transform.position, out panorama);
            float alvo = Sfx.Volume * _deixa.Volume * Mathf.Clamp01(Nivel) * ganho * GameAudio.NivelEfeitos;

            _volume = Mathf.MoveTowards(_volume, alvo, Time.unscaledDeltaTime * Esmaecimento);
            _fonte.volume = _volume;
            _fonte.panStereo = panorama;
            _fonte.pitch = Mathf.MoveTowards(_fonte.pitch, _deixa.Tom * Tom, Time.unscaledDeltaTime * 2f);

            if (_volume > 0.001f && !_fonte.isPlaying)
            {
                _fonte.time = Random.Range(0f, _fonte.clip.length * 0.9f);
                _fonte.Play();
            }
            else if (_volume <= 0.001f && _fonte.isPlaying)
            {
                _fonte.Stop();
            }
        }
    }
}
