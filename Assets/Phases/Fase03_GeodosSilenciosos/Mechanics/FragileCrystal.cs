using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma plataforma de cristal fino. Aguenta o Scrap andando e pulando baixo;
    /// com barulho forte em cima dela — dash, golpe, queda alta — ela trinca e
    /// se desfaz. Volta depois de um tempo.
    ///
    /// E o teste de peso do District 3: o corpo de ferro tem que aprender a
    /// pisar leve.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class FragileCrystal : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("breakLoudness")] [Tooltip("A partir de que forca o ruido a quebra. Passo faz 0.15 e pulo 0.2; golpe 0.8, dash 1.")]
        [SerializeField, Range(0f, 1f)] private float volumeQuebra = 0.35f;

        [UnityEngine.Serialization.FormerlySerializedAs("crackTime")] [Tooltip("Segundos entre trincar e se desfazer: o tempo de sair de cima.")]
        [SerializeField] private float tempoRachadura = 0.35f;

        [UnityEngine.Serialization.FormerlySerializedAs("regrowTime")] [Tooltip("Segundos ate ela se refazer.")]
        [SerializeField] private float tempoRecrescer = 4f;

        [UnityEngine.Serialization.FormerlySerializedAs("sprite")] [SerializeField] private SpriteRenderer sprite;

        private BoxCollider2D _colisor;
        private bool _quebrando;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(SpriteRenderer novoSprite)
        {
            sprite = novoSprite;
        }

        private void Awake()
        {
            _colisor = GetComponent<BoxCollider2D>();
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;
            ScrapHealth.Morreu += Restaurar;
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;
            ScrapHealth.Morreu -= Restaurar;
        }

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            if (_quebrando || volumeRuido2 < volumeQuebra) return;

            // So conta barulho feito em cima dela (ou encostado).
            Bounds limites = _colisor.bounds;
            limites.Expand(new Vector3(0.6f, 1.6f, 0f));
            if (!limites.Contains(new Vector3(pontoOrigem.x, pontoOrigem.y, limites.center.z))) return;

            StartCoroutine(Quebrar());
        }

        private IEnumerator Quebrar()
        {
            _quebrando = true;
            Sfx.RachaduraCristal.Tocar(transform.position);

            // Trinca: pisca por um instante antes de ceder.
            for (float t = 0f; t < tempoRachadura; t += Time.deltaTime)
            {
                if (sprite != null) sprite.enabled = Mathf.FloorToInt(t * 30f) % 2 == 0;
                yield return null;
            }

            _colisor.enabled = false;
            if (sprite != null) sprite.enabled = false;
            Sfx.QuebraCristal.Tocar(transform.position);

            yield return new WaitForSeconds(tempoRecrescer);
            Restaurar();
        }

        private void Restaurar()
        {
            StopAllCoroutines();

            _quebrando = false;
            if (_colisor != null) _colisor.enabled = true;
            if (sprite != null) sprite.enabled = true;
        }
    }
}
