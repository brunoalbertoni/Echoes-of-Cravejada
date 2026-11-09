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
        [Tooltip("Tempo maximo esperando o chao aparecer antes de soltar assim mesmo.")]
        [SerializeField] private float maxWait = 5f;

        private void Start()
        {
            string id = SceneTravel.PendingSpawnId;
            SceneTravel.PendingSpawnId = null;

            Vector3 position = transform.position;

            if (!string.IsNullOrEmpty(id))
            {
                bool found = false;

                foreach (SpawnPoint point in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
                {
                    if (point.Id != id) continue;

                    position = point.transform.position;
                    found = true;
                    break;
                }

                if (!found) Debug.LogWarning("[SceneArrival] Nao ha SpawnPoint com o id " + id + " nesta cena.", this);
            }

            StartCoroutine(Settle(position));
        }

        /// <summary>Poe o Scrap no lugar e so devolve a fisica quando houver chao embaixo dele.</summary>
        private IEnumerator Settle(Vector3 position)
        {
            var body = GetComponent<Rigidbody2D>();
            var controller = GetComponent<ScrapController>();
            var respawn = GetComponent<ScrapRespawn>();

            if (controller != null) controller.ControlLocked = true;
            if (body != null) body.simulated = false;

            Place(position, body, respawn);

            int ground = LayerMask.GetMask("Ground");
            float waited = 0f;

            // Sem a layer (cena montada na mao), nao ha o que esperar.
            while (ground != 0 && waited < maxWait)
            {
                Vector2 above = (Vector2)position + Vector2.up * 0.5f;
                if (Physics2D.Raycast(above, Vector2.down, 4f, ground).collider != null) break;

                waited += Time.unscaledDeltaTime;
                yield return null;

                // Enquanto espera, garante que ninguem o tirou do lugar.
                Place(position, body, respawn);
            }

            if (waited >= maxWait)
            {
                Debug.LogWarning("[SceneArrival] O chao nao apareceu embaixo do ponto de chegada.", this);
            }

            if (body != null)
            {
                body.simulated = true;
                body.linearVelocity = Vector2.zero;
            }

            if (controller != null) controller.ControlLocked = false;
        }

        private void Place(Vector3 position, Rigidbody2D body, ScrapRespawn respawn)
        {
            transform.position = position;

            if (body != null)
            {
                body.position = position;
                body.linearVelocity = Vector2.zero;
            }

            if (respawn != null) respawn.SetSafePosition(position);

            // Senao a camera faz a viagem inteira desde o ponto inicial da cena.
            Camera main = Camera.main;
            var follow = main != null ? main.GetComponent<ScrapCameraFollow>() : null;
            if (follow != null) follow.SnapToTarget();
        }
    }
}
