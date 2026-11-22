using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Fica no Scrap. Ao comecar a cena, ve se ele veio de outra por uma
    /// SceneExit e, se veio, o leva pro SpawnPoint combinado. Sem recado
    /// nenhum, ele fica onde a cena o deixou.
    ///
    /// Tambem segura o Scrap parado ate o chao existir. Numa cena carregada no
    /// meio do jogo o colisor do terreno leva alguns quadros pra ser montado, e
    /// sem isso ele atravessava o piso e ia parar la embaixo.
    /// </summary>
    public class SceneArrival : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxWait")] [Tooltip("Tempo maximo esperando o chao aparecer antes de soltar assim mesmo.")]
        [SerializeField] private float esperaMax = 5f;

        private void Start()
        {
            string id = SceneTravel.IdNascimentoPendente;
            SceneTravel.IdNascimentoPendente = null;

            Vector3 posicao = transform.position;

            if (!string.IsNullOrEmpty(id))
            {
                bool encontrado = false;

                foreach (SpawnPoint ponto in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
                {
                    if (ponto.Id != id) continue;

                    posicao = ponto.transform.position;
                    encontrado = true;
                    break;
                }

                if (!encontrado) Debug.LogWarning("[SceneArrival] Nao ha SpawnPoint com o id " + id + " nesta cena.", this);
            }

            // Veio de um save: aparece de pe na bancada em que salvou, e ela e o checkpoint.
            if (SaveSystem.PegarBancadaPendente(out Vector3 bancada2))
            {
                posicao = bancada2;
                var vida = GetComponent<ScrapHealth>();
                if (vida != null) vida.DescansarEm(bancada2);
            }

            StartCoroutine(Assentar(posicao));
        }

        /// <summary>Poe o Scrap no lugar e so devolve a fisica quando houver chao embaixo dele.</summary>
        private IEnumerator Assentar(Vector3 posicao)
        {
            var corpo = GetComponent<Rigidbody2D>();
            var controlador = GetComponent<ScrapController>();
            var renascimento = GetComponent<ScrapRespawn>();

            if (controlador != null) controlador.ControleTravado = true;
            if (corpo != null) corpo.simulated = false;

            Posicionar(posicao, corpo, renascimento);

            int solo = LayerMask.GetMask("Ground");
            float esperou = 0f;

            // Sem a layer (cena montada na mao), nao ha o que esperar.
            while (solo != 0 && esperou < esperaMax)
            {
                Vector2 acima = (Vector2)posicao + Vector2.up * 0.5f;
                if (Physics2D.Raycast(acima, Vector2.down, 4f, solo).collider != null) break;

                esperou += Time.unscaledDeltaTime;
                yield return null;

                // Enquanto espera, garante que ninguem o tirou do lugar.
                Posicionar(posicao, corpo, renascimento);
            }

            if (esperou >= esperaMax)
            {
                Debug.LogWarning("[SceneArrival] O chao nao apareceu embaixo do ponto de chegada.", this);
            }

            if (corpo != null)
            {
                corpo.simulated = true;
                corpo.linearVelocity = Vector2.zero;
            }

            if (controlador != null) controlador.ControleTravado = false;
        }

        private void Posicionar(Vector3 posicao, Rigidbody2D corpo, ScrapRespawn renascimento)
        {
            transform.position = posicao;

            if (corpo != null)
            {
                corpo.position = posicao;
                corpo.linearVelocity = Vector2.zero;
            }

            if (renascimento != null) renascimento.DefinirPosicaoSegura(posicao);

            // Senao a camera faz a viagem inteira desde o ponto inicial da cena.
            Camera principal = Camera.main;
            var seguir = principal != null ? principal.GetComponent<ScrapCameraFollow>() : null;
            if (seguir != null) seguir.IrDiretoAoAlvo();
        }
    }
}
