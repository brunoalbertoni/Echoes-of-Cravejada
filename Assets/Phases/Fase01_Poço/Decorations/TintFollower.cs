using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Pinta os sprites filhos com a cor de um tilemap (vezes um fator). E o
    /// que faz a cenografia presa na parede do fundo mudar de cor junto com
    /// ela quando a area e purificada, em vez de ficar verde no meio do dourado.
    /// </summary>
    public class TintFollower : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("source")] [SerializeField] private Tilemap fonte;

        [UnityEngine.Serialization.FormerlySerializedAs("factor")] [Tooltip("Multiplica a cor do tilemap: acima de 1 deixa a cenografia um tom mais clara que a parede.")]
        [SerializeField] private float fator = 1.2f;

        private SpriteRenderer[] _renderizadores;
        private Color _aplicado = new Color(-1f, -1f, -1f, -1f);

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Tilemap novoFonte, float novoFator)
        {
            fonte = novoFonte;
            fator = novoFator;
        }

        private void Awake()
        {
            _renderizadores = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            if (fonte == null) return;

            Color cor = fonte.color;
            if (cor == _aplicado) return;
            _aplicado = cor;

            var tom = new Color(
                Mathf.Clamp01(cor.r * fator),
                Mathf.Clamp01(cor.g * fator),
                Mathf.Clamp01(cor.b * fator),
                1f);

            foreach (SpriteRenderer renderizador in _renderizadores)
            {
                if (renderizador != null) renderizador.color = tom;
            }
        }
    }
}
