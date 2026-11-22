using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Uma gota de mercurio caindo. Quem solta e a MercuryDrip.</summary>
    public class MercuryDrop : MonoBehaviour
    {
        private float _velocidade;
        private float _chaoY;
        private float _tempoLento;

        public static void Cair(Vector3 de, float distancia, float tempoLento, Sprite sprite)
        {
            var quedaItem = Prefabs.Criar("MercuryDrop");
            quedaItem.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(quedaItem);
            renderizador.sprite = sprite;
            renderizador.color = new Color(0.80f, 1f, 0.85f);
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 3;

            var gatilho = Prefabs.Componente<CircleCollider2D>(quedaItem);
            gatilho.isTrigger = true;
            gatilho.radius = 0.22f;

            var corpo = Prefabs.Componente<Rigidbody2D>(quedaItem);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = quedaItem.AddComponent<MercuryDrop>();
            roteiro._chaoY = de.y - distancia;
            roteiro._tempoLento = tempoLento;
        }

        private void Update()
        {
            // Engorda um instante no teto e depois despenca.
            _velocidade += 22f * Time.deltaTime;
            transform.position += Vector3.down * (_velocidade * Time.deltaTime);

            if (transform.position.y <= _chaoY)
            {
                Sfx.GotejarMercurio.Tocar(transform.position);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            controlador.Desacelerar(_tempoLento);
            Sfx.EsparramarMercurio.Tocar();
            Destroy(gameObject);
        }
    }
}
