using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma gota de mercurio que pinga da nuvem do Alquimista quando ele
    /// destila. Cai reto ate o chao e, se pegar o Scrap no caminho, machuca.
    /// Diferente da goteira do teto (MercuryDrop), que so deixa lento.
    /// </summary>
    public class MercuryBead : MonoBehaviour
    {
        private const float Gravidade = 26f;

        private Vector2 _velocidadeVetor;
        private float _chaoY;

        public static void Cair(Vector3 de, float chaoY, Sprite sprite)
        {
            Criar(de, new Vector2(0f, -2f), chaoY, sprite);
        }

        /// <summary>Um respingo: sai com velocidade e cai em arco.</summary>
        public static void Respingar(Vector3 de, Vector2 velocidadeVetorial, float chaoY, Sprite sprite)
        {
            Criar(de, velocidadeVetorial, chaoY, sprite);
        }

        private static void Criar(Vector3 de, Vector2 velocidadeVetorial, float chaoY, Sprite sprite)
        {
            var gota = Prefabs.Criar("MercuryBead");
            gota.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(gota);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 4;

            var gatilho = Prefabs.Componente<CircleCollider2D>(gota);
            gatilho.isTrigger = true;
            gatilho.radius = 0.2f;

            var corpo = Prefabs.Componente<Rigidbody2D>(gota);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = gota.AddComponent<MercuryBead>();
            roteiro._chaoY = chaoY;
            roteiro._velocidadeVetor = velocidadeVetorial;

            // Morreu o Scrap no meio: as gotas no ar somem com a luta.
            Destroy(gota, 6f);
        }

        private void Update()
        {
            _velocidadeVetor.y -= Gravidade * Time.deltaTime;
            transform.position += (Vector3)(_velocidadeVetor * Time.deltaTime);

            if (_velocidadeVetor.y < 0f && transform.position.y <= _chaoY)
            {
                Sfx.PousarGota.Tocar(transform.position);
                Destroy(gameObject);
            }
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
