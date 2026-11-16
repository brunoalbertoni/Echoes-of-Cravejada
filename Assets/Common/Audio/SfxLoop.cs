using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um efeito em loop preso a uma coisa que o controla: o motor da broca,
    /// o raio do Rei, o som de segurar E no Altar. Quem o prende diz, todo
    /// quadro, o quanto ele deve soar (<see cref="Level"/>) e em que tom
    /// (<see cref="Pitch"/>); ele chega la suave e some com a distancia da
    /// camera, como os outros sons do mapa.
    ///
    /// Diferente do <see cref="AmbientEmitter"/>, que e ambiente e baixa na
    /// luta, este e parte da acao.
    /// </summary>
    public class SfxLoop : MonoBehaviour
    {
        /// <summary>O quanto ele soa agora (0 a 1).</summary>
        public float Level { get; set; }

        /// <summary>O tom (1 = o do arquivo).</summary>
        public float Pitch { get; set; } = 1f;

        /// <summary>Volume por segundo com que ele chega ao nivel pedido.</summary>
        public float Fade { get; set; } = 4f;

        /// <summary>Som da tela (true) ou daquele ponto do mapa (false).</summary>
        public bool Screen { get; set; }

        private AudioSource _source;
        private SfxCue _cue;
        private float _volume;

        /// <summary>Prende o loop de <paramref name="cue"/> em <paramref name="target"/>, calado.</summary>
        public static SfxLoop Attach(GameObject target, SfxCue cue)
        {
            if (target == null || cue == null) return null;

            var loop = target.AddComponent<SfxLoop>();
            var source = target.AddComponent<AudioSource>();
            source.clip = SfxPlayer.Load(cue.Clip);
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.priority = 80;

            loop._source = source;
            loop._cue = cue;
            return loop;
        }

        /// <summary>Cala na hora, sem desbotar.</summary>
        public void Silence()
        {
            Level = 0f;
            _volume = 0f;
            if (_source != null) _source.Stop();
        }

        private void Update()
        {
            if (_source == null || _source.clip == null) return;

            float pan = 0f;
            float gain = Screen ? 1f : SfxPlayer.Distance(transform.position, out pan);
            float target = Sfx.Volume * _cue.Volume * Mathf.Clamp01(Level) * gain;

            _volume = Mathf.MoveTowards(_volume, target, Time.unscaledDeltaTime * Fade);
            _source.volume = _volume;
            _source.panStereo = pan;
            _source.pitch = Mathf.MoveTowards(_source.pitch, _cue.Pitch * Pitch, Time.unscaledDeltaTime * 2f);

            if (_volume > 0.001f && !_source.isPlaying)
            {
                _source.time = Random.Range(0f, _source.clip.length * 0.9f);
                _source.Play();
            }
            else if (_volume <= 0.001f && _source.isPlaying)
            {
                _source.Stop();
            }
        }
    }
}
