using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma lasca de pedra que pula do chao onde a Geode Drill vai sair por
    /// baixo. E so o aviso: nao machuca. Sobe, cai e some.
    /// </summary>
    public class DrillPebble : MonoBehaviour
    {
        private const float Gravidade = 30f;

        private Vector2 _velocidadeVetor;
        private float _ate;
        private SpriteRenderer _sprite;

        public static void Saltitar(Vector2 de, Sprite sprite)
        {
            var pedrinha = Prefabs.Criar("DrillPebble");
            pedrinha.transform.position = de;
            pedrinha.transform.localScale = Vector3.one * 0.55f;
            pedrinha.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var renderizador = Prefabs.Componente<SpriteRenderer>(pedrinha);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 6;
            renderizador.color = new Color(0.75f, 0.72f, 0.80f);

            var roteiro = pedrinha.AddComponent<DrillPebble>();
            roteiro._sprite = renderizador;
            roteiro._velocidadeVetor = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(5f, 7.5f));
            roteiro._ate = Time.time + 0.55f;

            Sfx.EstaloPedrinha.Tocar(de);
        }

        private void Update()
        {
            _velocidadeVetor.y -= Gravidade * Time.deltaTime;
            transform.position += (Vector3)(_velocidadeVetor * Time.deltaTime);
            transform.Rotate(0f, 0f, 720f * Time.deltaTime);

            float esquerda = _ate - Time.time;
            if (_sprite != null && esquerda < 0.2f) _sprite.color = new Color(0.75f, 0.72f, 0.80f, Mathf.Clamp01(esquerda / 0.2f));
            if (esquerda <= 0f) Destroy(gameObject);
        }
    }
}
