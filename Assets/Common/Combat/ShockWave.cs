using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A onda de choque da segunda fase: corre rente ao chao pra um lado ate
    /// sair da arena. E baixa de proposito — pula-se por cima.
    /// </summary>
    public class ShockWave : MonoBehaviour
    {
        private float _velocidade;
        private float _minX;
        private float _maxX;

        public static void Lancar(Vector3 de, float velocidade, float minX, float maxX, Sprite sprite)
        {
            var onda = Prefabs.Criar("ShockWave");
            onda.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(onda);
            renderizador.sprite = sprite;
            renderizador.flipX = velocidade < 0f;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 2;

            var gatilho = Prefabs.Componente<BoxCollider2D>(onda);
            gatilho.isTrigger = true;
            gatilho.size = new Vector2(0.9f, 0.6f);
            gatilho.offset = new Vector2(0f, 0.3f);

            var corpo = Prefabs.Componente<Rigidbody2D>(onda);
            corpo.bodyType = RigidbodyType2D.Kinematic;

            // As duas ondas da pancada saem no mesmo quadro: o cue nao dobra.
            Sfx.OndaChoque.Tocar(de);

            var roteiro = onda.AddComponent<ShockWave>();
            roteiro._velocidade = velocidade;
            roteiro._minX = minX;
            roteiro._maxX = maxX;
        }

        private void Update()
        {
            transform.position += Vector3.right * (_velocidade * Time.deltaTime);

            float x = transform.position.x;
            if (x < _minX || x > _maxX) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
