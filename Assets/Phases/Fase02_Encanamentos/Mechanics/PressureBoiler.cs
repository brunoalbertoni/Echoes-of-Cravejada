using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma caldeira corrompida. Cada golpe do cinzel sobe a pressao; no
    /// terceiro ela dispara, pisca por um instante — o tempo de se afastar — e
    /// explode. A explosao derruba as paredes rachadas ligadas a ela e machuca
    /// quem estiver perto, inimigo ou Scrap.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressureBoiler : MonoBehaviour, IDamageable, ICueDone
    {
        [UnityEngine.Serialization.FormerlySerializedAs("hitsToBurst")] [Tooltip("Quantos golpes ate ela disparar.")]
        [SerializeField] private int golpesParaRajada = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("fuse")] [Tooltip("Segundos entre disparar e explodir.")]
        [SerializeField] private float pavio = 1.4f;

        [UnityEngine.Serialization.FormerlySerializedAs("blastRadius")] [SerializeField] private float raioExplosao = 4f;

        [UnityEngine.Serialization.FormerlySerializedAs("walls")] [Tooltip("As paredes rachadas que a explosao derruba.")]
        [SerializeField] private GameObject[] paredes;

        [UnityEngine.Serialization.FormerlySerializedAs("sprite")] [SerializeField] private SpriteRenderer sprite;
        [UnityEngine.Serialization.FormerlySerializedAs("blast")] [SerializeField] private ParticleSystem explosao;

        private int _golpes;
        private bool _rajada;

        /// <summary>Quao cheia de pressao ela esta, de 0 a 1. O ponteiro do manometro dela pode usar.</summary>
        public float Pressao => Mathf.Clamp01(_golpes / (float)Mathf.Max(1, golpesParaRajada));

        bool ICueDone.ProntoDeixa => _rajada;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(GameObject[] novoParedes, SpriteRenderer novoSprite, ParticleSystem novoExplosao)
        {
            paredes = novoParedes;
            sprite = novoSprite;
            explosao = novoExplosao;
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_rajada) return;

            _golpes++;

            // Cada golpe soa no casco um pouco mais agudo: a pressao subindo.
            Sfx.PressaoCaldeira.Tocar(transform.position, tom2: 1f + 0.12f * (_golpes - 1));

            if (_golpes >= golpesParaRajada)
            {
                _rajada = true;
                Sfx.PavioCaldeira.Tocar(transform.position);
                StartCoroutine(Estourar());
                return;
            }

            if (sprite != null) StartCoroutine(Piscada(0.12f, new Color(1f, 0.7f, 0.4f)));
        }

        private IEnumerator Piscada(float tempo, Color cor)
        {
            sprite.color = cor;
            yield return new WaitForSeconds(tempo);
            sprite.color = Color.white;
        }

        private IEnumerator Estourar()
        {
            // Pisca cada vez mais rapido ate estourar.
            for (float t = 0f; t < pavio; t += Time.deltaTime)
            {
                if (sprite != null)
                {
                    float taxa = Mathf.Lerp(6f, 22f, t / pavio);
                    sprite.color = Mathf.FloorToInt(t * taxa) % 2 == 0 ? new Color(1f, 0.45f, 0.3f) : Color.white;
                }

                yield return null;
            }

            if (explosao != null) explosao.Play();
            Sfx.ExplosaoCaldeira.Tocar(transform.position);

            if (paredes != null)
            {
                foreach (GameObject parede in paredes)
                {
                    if (parede != null) parede.SetActive(false);
                }
            }

            Vector2 centro = (Vector2)transform.position + Vector2.up;

            foreach (Collider2D golpe in Physics2D.OverlapCircleAll(centro, raioExplosao))
            {
                if (golpe.transform == transform) continue;

                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null)
                {
                    vida.LevarDano(1, centro);
                    continue;
                }

                var danificavel = golpe.GetComponentInParent<IDamageable>();
                if (danificavel != null && !ReferenceEquals(danificavel, this)) danificavel.ReceberDano(3, centro);
            }

            // Fica ali, apagada e escura: ja deu o que tinha.
            if (sprite != null) sprite.color = new Color(0.35f, 0.32f, 0.32f);
        }
    }
}
