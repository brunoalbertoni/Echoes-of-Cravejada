using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O que o Boiler Master deixa ao cair. Encostar da o Pressure Jump ao
    /// Scrap: um segundo pulo, no ar.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressureJumpPickup : MonoBehaviour
    {
        private float _idade;
        private Vector3 _pontoOrigem;

        public static void Soltar(Vector3 posicao, Sprite sprite)
        {
            var coletavel = Prefabs.Criar("PressureJumpModule");
            coletavel.transform.position = DropSpot.PontoLivre(posicao + Vector3.up * 0.8f);

            var renderizador = Prefabs.Componente<SpriteRenderer>(coletavel);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = -1;

            var gatilho = Prefabs.Componente<CircleCollider2D>(coletavel);
            gatilho.isTrigger = true;
            gatilho.radius = 0.7f;

            coletavel.AddComponent<PressureJumpPickup>();
            Sfx.QuedaModulo.Tocar(posicao);
        }

        private void Awake()
        {
            _pontoOrigem = transform.position;
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            transform.position = _pontoOrigem + Vector3.up * (Mathf.Sin(_idade * 3f) * 0.15f);
        }

        // Fica um instante no ar antes de poder ser pego: nao entra sozinho no Scrap ao lado.
        private const float AtrasoArmar = 1.2f;

        private void OnTriggerEnter2D(Collider2D outro) => Coletar(outro);
        private void OnTriggerStay2D(Collider2D outro) => Coletar(outro);

        private void Coletar(Collider2D outro)
        {
            if (_idade < AtrasoArmar || outro.GetComponentInParent<ScrapController>() == null) return;

            Sfx.RecarregarVapor.Tocar();
            ScrapController.DesbloquearSaltoDePressao();
            Destroy(gameObject);
        }
    }
}
