using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um geiser de vapor escaldante que brota do chao: primeiro um aviso fraco
    /// no lugar, depois a coluna de verdade, que machuca, e entao some.
    /// E o que o Boiler Master solta na segunda fase.
    /// </summary>
    public class SteamGeyser : MonoBehaviour
    {
        private const float TempoAlerta = 0.8f;
        private const float TempoQueima = 1.0f;
        private const float Altura = 7f;

        private SpriteRenderer _sprite;
        private BoxCollider2D _gatilho;
        private float _idade;

        public static void EntrarEmErupcao(Vector3 solo, Sprite sprite)
        {
            var geiser = Prefabs.Criar("SteamGeyser");
            geiser.transform.position = solo;

            var renderizador = Prefabs.Componente<SpriteRenderer>(geiser);
            renderizador.sprite = sprite;
            renderizador.drawMode = SpriteDrawMode.Tiled;
            renderizador.size = new Vector2(1.5f, Altura);
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 4;

            var gatilho = Prefabs.Componente<BoxCollider2D>(geiser);
            gatilho.isTrigger = true;
            gatilho.size = new Vector2(1.1f, Altura);
            gatilho.offset = new Vector2(0f, Altura * 0.5f);
            gatilho.enabled = false;

            var corpo = Prefabs.Componente<Rigidbody2D>(geiser);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            var roteiro = geiser.AddComponent<SteamGeyser>();
            roteiro._sprite = renderizador;
            roteiro._gatilho = gatilho;

            Sfx.AlertaGeiser.Tocar(solo);
        }

        private void Update()
        {
            bool estavaQueimando = _idade >= TempoAlerta;
            _idade += Time.deltaTime;

            bool queimando = _idade >= TempoAlerta;
            if (queimando && !estavaQueimando) Sfx.RajadaGeiser.Tocar(transform.position);
            _gatilho.enabled = queimando;

            // No aviso ele pisca fraco; depois fica cheio.
            float alfa = queimando ? 0.9f : (Mathf.FloorToInt(_idade * 14f) % 2 == 0 ? 0.35f : 0.15f);
            _sprite.color = new Color(1f, 1f, 1f, alfa);

            if (_idade >= TempoAlerta + TempoQueima) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Escaldar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Escaldar(outro);
        }

        private void Escaldar(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
