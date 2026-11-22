using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Toca uma lista de quadros em loop: o parado dos descartados do
    /// District 1 (respirando, com o detalhe de cada um mexendo).
    ///
    /// O quadro de piscar, se houver, so aparece um ciclo em tres; nos
    /// outros fica o anterior. Nao mexe no flipX: quem vira o rosto e o Npc.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteLoop : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [SerializeField] private Sprite[] quadros;
        [UnityEngine.Serialization.FormerlySerializedAs("framesPerSecond")] [SerializeField] private float quadrosPorSegundo = 6f;

        [UnityEngine.Serialization.FormerlySerializedAs("blinkFrame")] [Tooltip("O quadro de olhos fechados. -1: nao pisca.")]
        [SerializeField] private int quadroPiscar = -1;

        private SpriteRenderer _renderizador;
        private float _deslocamento;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite[] novoQuadros, float novoQuadrosPorSegundo, int novoQuadroPiscar)
        {
            quadros = novoQuadros;
            quadrosPorSegundo = novoQuadrosPorSegundo;
            quadroPiscar = novoQuadroPiscar;
        }

        private void Awake()
        {
            _renderizador = GetComponent<SpriteRenderer>();

            // Cada um num ponto do ciclo, pra nao respirarem todos juntos.
            _deslocamento = Mathf.Abs(transform.position.x * 0.37f + transform.position.y * 0.11f);
        }

        private void Update()
        {
            if (quadros == null || quadros.Length == 0) return;

            float relogio = (Time.time + _deslocamento) * quadrosPorSegundo;
            int quadro = (int)relogio % quadros.Length;

            if (quadro == quadroPiscar && quadro > 0 && (int)(relogio / quadros.Length) % 3 != 0) quadro--;

            if (quadros[quadro] != null) FrameAnimator.Exibir(_renderizador, quadros[quadro]);
        }
    }
}
