using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O recado que uma cena deixa pra proxima: em qual SpawnPoint o Scrap deve
    /// aparecer. Fica num campo estatico porque e a unica coisa que atravessa
    /// a troca de cena.
    /// </summary>
    public static class SceneTravel
    {
        public static string PendingSpawnId;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            PendingSpawnId = null;
        }
    }

    /// <summary>
    /// Uma passagem pra outra cena. Quando o Scrap encosta, carrega a cena e
    /// avisa em qual SpawnPoint ele deve aparecer do outro lado.
    ///
    /// A cena de destino precisa estar no Build Settings; os geradores de fase
    /// ja poem a que eles criam.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneExit : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string spawnId;

        private bool _used;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newSceneName, string newSpawnId)
        {
            sceneName = newSceneName;
            spawnId = newSpawnId;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_used) return;
            if (other.GetComponentInParent<ScrapController>() == null) return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[SceneExit] A cena " + sceneName + " nao esta no Build Settings. " +
                               "Gere a fase pelo menu Echoes, que ela e adicionada sozinha.", this);
                return;
            }

            _used = true;
            SceneTravel.PendingSpawnId = spawnId;
            SceneManager.LoadScene(sceneName);
        }
    }
}
