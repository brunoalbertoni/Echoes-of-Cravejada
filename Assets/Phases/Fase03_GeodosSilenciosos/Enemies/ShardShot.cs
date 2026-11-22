using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Um estilhaco de cristal: voa reto ate o alcance acabar ou acertar o Scrap.</summary>
    public class ShardShot : MonoBehaviour
    {
        private Vector2 _velocidadeVetor;
        private float _esquerda;

        public static void Disparar(Vector2 de, Vector2 velocidadeVetorial, float alcance, Sprite sprite)
        {
            var plano = Prefabs.Criar("ShardShot");
            plano.transform.position = de;
            plano.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocidadeVetorial.y, velocidadeVetorial.x) * Mathf.Rad2Deg);

            var renderizador = Prefabs.Componente<SpriteRenderer>(plano);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 3;

            var gatilho = Prefabs.Componente<CircleCollider2D>(plano);
            gatilho.isTrigger = true;
            gatilho.radius = 0.22f;

            var corpo = Prefabs.Componente<Rigidbody2D>(plano);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = plano.AddComponent<ShardShot>();
            roteiro._velocidadeVetor = velocidadeVetorial;
            roteiro._esquerda = alcance;
        }

        private void Update()
        {
            Vector2 passo = _velocidadeVetor * Time.deltaTime;
            transform.position += (Vector3)passo;

            _esquerda -= passo.magnitude;
            if (_esquerda <= 0f)
            {
                // Acabou o alcance: o cristal se desfaz.
                Sfx.GolpePlanoFragmento.Tocar(transform.position, volume: 0.4f);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida == null) return;

            vida.LevarDano(1, transform.position);
            Sfx.GolpePlanoFragmento.Tocar(transform.position);
            Destroy(gameObject);
        }
    }
}
