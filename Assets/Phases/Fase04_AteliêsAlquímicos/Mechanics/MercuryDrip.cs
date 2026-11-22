using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma goteira de mercurio corrompido no teto: de tempos em tempos solta
    /// uma gota gorda, que cai reto. Se acerta o Scrap, gruda no corpo e ele
    /// fica lento por alguns segundos. Nao tira vida — tira o ritmo.
    /// </summary>
    public class MercuryDrip : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("interval")] [SerializeField] private float intervalo = 2.4f;
        [UnityEngine.Serialization.FormerlySerializedAs("delay")] [SerializeField] private float atraso;

        [UnityEngine.Serialization.FormerlySerializedAs("fall")] [Tooltip("Quantos tiles a gota cai antes de bater no chao.")]
        [SerializeField] private float queda = 6f;

        [UnityEngine.Serialization.FormerlySerializedAs("slowTime")] [SerializeField] private float tempoLento = 3f;
        [UnityEngine.Serialization.FormerlySerializedAs("dropSprite")] [SerializeField] private Sprite spriteQueda;

        private float _cronometro;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoIntervalo, float novoAtraso, float novoQueda, Sprite novoSpriteQueda)
        {
            intervalo = novoIntervalo;
            atraso = novoAtraso;
            queda = novoQueda;
            spriteQueda = novoSpriteQueda;
        }

        private void Start()
        {
            _cronometro = atraso;
        }

        private void Update()
        {
            _cronometro -= Time.deltaTime;
            if (_cronometro > 0f) return;

            _cronometro = intervalo;
            MercuryDrop.Cair(transform.position, queda, tempoLento, spriteQueda);

            // A gota se soltando do teto: um plique fino, antes do estalo no chao.
            Sfx.GotejarTeto.Tocar(transform.position);
        }
    }
}
