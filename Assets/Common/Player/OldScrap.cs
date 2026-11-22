using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A "old scrap": o que fica no lugar onde o Scrap morreu, guardando os
    /// Opaque Shards que ele carregava. Encostar devolve tudo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OldScrap : MonoBehaviour
    {
        private int _fragmentos;
        private float _idade;

        public static OldScrap Soltar(Vector3 posicao, int fragmentos, Sprite sprite)
        {
            var sucata = Prefabs.Criar("OldScrap");
            sucata.transform.position = DropSpot.PontoLivre(posicao + Vector3.up * 0.5f) - Vector3.up * 0.5f;

            var renderizador = Prefabs.Componente<SpriteRenderer>(sucata);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = -2;

            var gatilho = Prefabs.Componente<CircleCollider2D>(sucata);
            gatilho.isTrigger = true;
            gatilho.radius = 0.7f;
            gatilho.offset = new Vector2(0f, 0.5f);

            var antigo = sucata.AddComponent<OldScrap>();
            antigo._fragmentos = fragmentos;
            return antigo;
        }

        private void Update()
        {
            _idade += Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Recuperar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Recuperar(outro);
        }

        private void Recuperar(Collider2D outro)
        {
            // Ele renasce longe dali, mas a carencia evita pegar no mesmo quadro.
            if (_idade < 0.5f) return;
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            ScrapWallet.Adicionar(_fragmentos);
            Sfx.RecuperadosFragmentos.Tocar();
            Destroy(gameObject);
        }
    }
}
