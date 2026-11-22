using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um jato de vapor escaldante: voa reto ate o alcance acabar ou bater no
    /// Scrap. Atravessa parede de proposito — some pelo alcance, que quem
    /// atira mede ate a parede.
    /// </summary>
    public class SteamBolt : MonoBehaviour
    {
        private float _velocidade;
        private float _esquerda;

        public static void Disparar(Vector3 de, float velocidade, float alcance, Sprite sprite)
        {
            var projetil = Prefabs.Criar("SteamBolt");
            projetil.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(projetil);
            renderizador.sprite = sprite;
            renderizador.flipX = velocidade < 0f;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 3;

            var gatilho = Prefabs.Componente<BoxCollider2D>(projetil);
            gatilho.isTrigger = true;
            gatilho.size = new Vector2(0.8f, 0.35f);

            var corpo = Prefabs.Componente<Rigidbody2D>(projetil);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = projetil.AddComponent<SteamBolt>();
            roteiro._velocidade = velocidade;
            roteiro._esquerda = alcance;
        }

        private void Update()
        {
            float passo = _velocidade * Time.deltaTime;
            transform.position += Vector3.right * passo;

            _esquerda -= Mathf.Abs(passo);
            if (_esquerda <= 0f)
            {
                // Acabou o alcance: o vapor se desfaz no ar, baixinho.
                Sfx.GolpeProjetilVapor.Tocar(transform.position, volume: 0.4f);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida == null) return;

            vida.LevarDano(1, transform.position);
            Sfx.GolpeProjetilVapor.Tocar(transform.position);
            Destroy(gameObject);
        }
    }
}
