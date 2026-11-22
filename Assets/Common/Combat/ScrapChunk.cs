using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um pedaco de sucata arremessado em arco. Machuca o Scrap se encostar e
    /// some ao bater no chao.
    /// </summary>
    public class ScrapChunk : MonoBehaviour
    {
        private Vector2 _velocidadeVetor;
        private float _gravidade;
        private float _chaoY;
        private float _idade;

        public static void Arremessar(Vector3 de, Vector2 velocidadeVetorial, float gravidade, float chaoY, Sprite sprite)
        {
            var pedaco = Prefabs.Criar("ScrapChunk");
            pedaco.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(pedaco);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 2;

            var gatilho = Prefabs.Componente<CircleCollider2D>(pedaco);
            gatilho.isTrigger = true;
            gatilho.radius = 0.3f;

            var corpo = Prefabs.Componente<Rigidbody2D>(pedaco);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = pedaco.AddComponent<ScrapChunk>();
            roteiro._velocidadeVetor = velocidadeVetorial;
            roteiro._gravidade = gravidade;
            roteiro._chaoY = chaoY;
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            _velocidadeVetor.y -= _gravidade * Time.deltaTime;

            transform.position += (Vector3)(_velocidadeVetor * Time.deltaTime);
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);

            if (transform.position.y <= _chaoY)
            {
                Sfx.PousarPedaco.Tocar(transform.position);
                Destroy(gameObject);
            }
            else if (_idade > 5f)
            {
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
