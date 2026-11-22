using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma poca rasa de acido que some sozinha: o que o Acid Bloater deixa ao
    /// estourar. Machuca quem pisar, mas nao joga de volta — e rasa demais pra
    /// afundar.
    /// </summary>
    public class AcidPuddle : MonoBehaviour
    {
        private float _esquerda;
        private float _total;
        private SpriteRenderer _sprite;

        /// <summary>
        /// Derrama uma poca centrada em ground. O sprite da superficie tem o
        /// pivo no canto de baixo, a esquerda: por isso o objeto vai meia
        /// largura pra esquerda, e o gatilho acompanha o desenho — machuca so
        /// onde se ve acido.
        /// </summary>
        public static void Derramar(Vector3 solo, float largura, float tempo, Sprite sprite)
        {
            var pocaAcido = Prefabs.Criar("AcidPuddle");
            pocaAcido.transform.position = solo - new Vector3(largura * 0.5f, 0f, 0f);

            var renderizador = Prefabs.Componente<SpriteRenderer>(pocaAcido);
            renderizador.sprite = sprite;
            renderizador.drawMode = SpriteDrawMode.Tiled;
            renderizador.size = new Vector2(largura, 0.4f);
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = -5;

            var gatilho = Prefabs.Componente<BoxCollider2D>(pocaAcido);
            gatilho.isTrigger = true;
            gatilho.size = new Vector2(largura - 0.4f, 0.35f);
            gatilho.offset = new Vector2(largura * 0.5f, 0.18f);

            var corpo = Prefabs.Componente<Rigidbody2D>(pocaAcido);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = pocaAcido.AddComponent<AcidPuddle>();
            roteiro._esquerda = tempo;
            roteiro._total = tempo;
            roteiro._sprite = renderizador;
        }

        private void Update()
        {
            _esquerda -= Time.deltaTime;

            // Vai sumindo no ultimo segundo.
            if (_sprite != null)
            {
                Color cor = _sprite.color;
                cor.a = Mathf.Clamp01(_esquerda);
                _sprite.color = cor;
            }

            if (_esquerda <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Queimar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Queimar(outro);
        }

        private void Queimar(Collider2D outro)
        {
            // No finalzinho ja esta seca demais pra queimar.
            if (_esquerda < _total * 0.1f) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida == null) return;

            Sfx.ChiarAcido.Tocar(vida.transform.position);
            vida.LevarDano(1, transform.position);
        }
    }
}
