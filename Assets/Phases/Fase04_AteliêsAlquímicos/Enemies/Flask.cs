using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um frasco arremessado em arco. Onde cai, estoura e deixa uma poca de
    /// Oxide. Acertar o Scrap em cheio tambem machuca.
    /// </summary>
    public class Flask : MonoBehaviour
    {
        private const float Gravidade = 22f;
        private const float TempoVoo = 0.9f;

        private Vector2 _velocidadeVetor;
        private float _soloY;
        private float _idade;
        private Sprite _spritePoca;

        public static void Arremessar(Vector2 de, Vector2 alvo, Sprite sprite, Sprite spritePoca)
        {
            var frasco = Prefabs.Criar("Flask");
            frasco.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(frasco);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 3;

            var gatilho = Prefabs.Componente<CircleCollider2D>(frasco);
            gatilho.isTrigger = true;
            gatilho.radius = 0.25f;

            var corpo = Prefabs.Componente<Rigidbody2D>(frasco);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            // Cai onde o Scrap esta agora, depois de um tempo fixo de voo.
            Vector2 delta = alvo - de;

            var roteiro = frasco.AddComponent<Flask>();
            roteiro._velocidadeVetor = new Vector2(delta.x / TempoVoo, delta.y / TempoVoo + 0.5f * Gravidade * TempoVoo);
            roteiro._soloY = alvo.y;
            roteiro._spritePoca = spritePoca;
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            _velocidadeVetor.y -= Gravidade * Time.deltaTime;

            transform.position += (Vector3)(_velocidadeVetor * Time.deltaTime);
            transform.Rotate(0f, 0f, 540f * Time.deltaTime);

            // So estoura na descida, ao chegar na altura do alvo.
            if (_velocidadeVetor.y < 0f && transform.position.y <= _soloY) Estourar();
            else if (_idade > 3f) Destroy(gameObject);
        }

        private void Estourar()
        {
            Sfx.QuebraFrasco.Tocar(transform.position);
            AcidPuddle.Derramar(new Vector3(transform.position.x, _soloY, 0f), 2.6f, 3f, _spritePoca);
            NoiseSystem.Emitir(transform.position, 7f, 0.5f);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida == null) return;

            vida.LevarDano(1, transform.position);
            Estourar();
        }
    }
}
