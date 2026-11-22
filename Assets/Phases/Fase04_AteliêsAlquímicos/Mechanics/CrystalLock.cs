using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma fechadura de cristal: abre o portao dela quando um raio de luz
    /// alquimica fica em cima por um instante. Aberta, fica aberta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrystalLock : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("gate")] [SerializeField] private Gate portao;
        [UnityEngine.Serialization.FormerlySerializedAs("sprite")] [SerializeField] private SpriteRenderer sprite;
        [UnityEngine.Serialization.FormerlySerializedAs("glow")] [SerializeField] private Light2D brilho;

        [UnityEngine.Serialization.FormerlySerializedAs("chargeTime")] [Tooltip("Segundos de luz ate abrir.")]
        [SerializeField] private float tempoCarga = 0.6f;

        private float _carga;
        private float _acesoAte;
        private bool _aberto;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Gate novoPortao, SpriteRenderer novoSprite, Light2D novoBrilho)
        {
            portao = novoPortao;
            sprite = novoSprite;
            brilho = novoBrilho;
        }

        /// <summary>Chamado pelo raio, a cada quadro em que ele bate aqui.</summary>
        public void Iluminar()
        {
            _acesoAte = Time.time + 0.1f;
        }

        private void Update()
        {
            bool aceso = _aberto || Time.time < _acesoAte;

            if (!_aberto)
            {
                _carga = aceso ? _carga + Time.deltaTime : 0f;

                if (_carga >= tempoCarga)
                {
                    _aberto = true;
                    Sfx.AbertoTravaCristal.Tocar(transform.position);
                    if (portao != null) portao.Abrir();
                    NoiseSystem.Emitir(transform.position, 12f, 0.5f);
                }
            }

            if (sprite != null) sprite.color = aceso ? Color.white : new Color(0.45f, 0.45f, 0.55f);
            if (brilho != null) brilho.intensity = Mathf.MoveTowards(brilho.intensity, aceso ? 1.2f : 0.15f, 4f * Time.deltaTime);
        }
    }
}
