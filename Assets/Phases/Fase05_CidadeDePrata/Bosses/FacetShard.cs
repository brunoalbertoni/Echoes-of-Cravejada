using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma faceta de luz da coroa do Rei de Prata. Voa reto e ricocheteia no
    /// terreno (parede, chao, teto) algumas vezes antes de se apagar. Na chuva
    /// da fase 2 ela so cai, sem ricochete. Machuca quem tocar.
    /// </summary>
    public class FacetShard : MonoBehaviour
    {
        private Vector2 _velocidadeVetor;
        private int _quiques;
        private int _mascara;
        private float _ate;

        public static void Lancar(Vector2 de, Vector2 velocidadeVetorial, int quiques, Sprite sprite, float duracaoVida = 5f)
        {
            var fragmento = Prefabs.Criar("FacetShard");
            fragmento.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(fragmento);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 8;

            var gatilho = Prefabs.Componente<CircleCollider2D>(fragmento);
            gatilho.isTrigger = true;
            gatilho.radius = 0.25f;

            var corpo = Prefabs.Componente<Rigidbody2D>(fragmento);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = fragmento.AddComponent<FacetShard>();
            roteiro._velocidadeVetor = velocidadeVetorial;
            roteiro._quiques = quiques;
            roteiro._mascara = LayerMask.GetMask("Ground");
            roteiro._ate = Time.time + duracaoVida;

            ScrapHealth.Morreu += roteiro.Sumir;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= Sumir;
        }

        private void Sumir()
        {
            if (this != null) Destroy(gameObject);
        }

        private void Update()
        {
            if (Time.time > _ate)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 posicao = transform.position;
            Vector2 passo = _velocidadeVetor * Time.deltaTime;

            // A primeira parede de verdade: sacada (plataforma) a lasca atravessa.
            RaycastHit2D golpe = default;
            float maisPerto = float.MaxValue;
            foreach (RaycastHit2D candidato in Physics2D.RaycastAll(posicao, passo.normalized, passo.magnitude + 0.2f, _mascara))
            {
                if (candidato.collider == null || candidato.collider.isTrigger || SilverKing.EPassavel(candidato.point)) continue;
                if (candidato.distance < maisPerto) { maisPerto = candidato.distance; golpe = candidato; }
            }

            if (golpe.collider != null)
            {
                if (_quiques <= 0)
                {
                    Destroy(gameObject);
                    return;
                }

                _quiques--;
                _velocidadeVetor = Vector2.Reflect(_velocidadeVetor, golpe.normal);
                Sfx.QuiqueFaceta.Tocar(golpe.point);
                transform.position = golpe.point + golpe.normal * 0.25f;
                return;
            }

            transform.position = posicao + passo;
            transform.Rotate(0f, 0f, 540f * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida == null) return;

            vida.LevarDano(1, transform.position);
            Destroy(gameObject);
        }
    }
}
