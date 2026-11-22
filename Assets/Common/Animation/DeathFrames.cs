using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A animacao de morte de quem e animado pelo MotionFrames: quando o
    /// Damageable morre, desliga o cerebro e o andar, toca os quadros de
    /// morte e so entao deixa ele sumir (Damageable.DeathAnimation). Quando o
    /// Scrap morre e todos voltam, religa tudo.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DeathFrames : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [SerializeField] private Sprite[] quadros;
        [UnityEngine.Serialization.FormerlySerializedAs("framesPerSecond")] [SerializeField] private float quadrosPorSegundo = 10f;

        [UnityEngine.Serialization.FormerlySerializedAs("silence")] [Tooltip("O que para de pensar e andar enquanto ele morre (o cerebro e o MotionFrames).")]
        [SerializeField] private Behaviour[] silenciar;

        private Damageable _danificavel;
        private SpriteRenderer _sprite;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite[] novoQuadros, float novoQuadrosPorSegundo, Behaviour[] novoSilenciar)
        {
            quadros = novoQuadros;
            quadrosPorSegundo = novoQuadrosPorSegundo;
            silenciar = novoSilenciar;
        }

        private void Awake()
        {
            _danificavel = GetComponent<Damageable>();
            _sprite = GetComponent<SpriteRenderer>();

            _danificavel.AnimacaoMorte = Tocar;
            _danificavel.Revivido += Acordar;
        }

        private void OnDestroy()
        {
            if (_danificavel != null) _danificavel.Revivido -= Acordar;
        }

        private IEnumerator Tocar()
        {
            DefinirSilenciado(true);

            if (quadros != null)
            {
                float passo = 1f / Mathf.Max(1f, quadrosPorSegundo);
                foreach (Sprite quadro in quadros)
                {
                    if (quadro != null && _sprite != null) FrameAnimator.Exibir(_sprite, quadro);
                    yield return new WaitForSeconds(passo);
                }
            }
        }

        private void Acordar()
        {
            DefinirSilenciado(false);
            if (_sprite != null && quadros != null && quadros.Length > 0) _sprite.color = Color.white;
        }

        private void DefinirSilenciado(bool silenciado)
        {
            if (silenciar == null) return;
            foreach (Behaviour parte in silenciar)
            {
                if (parte != null) parte.enabled = !silenciado;
            }
        }
    }
}
