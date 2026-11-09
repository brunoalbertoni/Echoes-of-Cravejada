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
        [SerializeField] private Tilemap source;

        [Tooltip("Multiplica a cor do tilemap: acima de 1 deixa a cenografia um tom mais clara que a parede.")]
        [SerializeField] private float factor = 1.2f;

        private SpriteRenderer[] _renderers;
        private Color _applied = new Color(-1f, -1f, -1f, -1f);

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Tilemap newSource, float newFactor)
        {
            source = newSource;
            factor = newFactor;
        }

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            if (source == null) return;

            Color color = source.color;
            if (color == _applied) return;
            _applied = color;

            var tint = new Color(
                Mathf.Clamp01(color.r * factor),
                Mathf.Clamp01(color.g * factor),
                Mathf.Clamp01(color.b * factor),
                1f);

            foreach (SpriteRenderer renderer in _renderers)
            {
                if (renderer != null) renderer.color = tint;
            }
        }
    }
}
