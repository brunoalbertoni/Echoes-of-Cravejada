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
        public static string IdNascimentoPendente;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            IdNascimentoPendente = null;
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
        [UnityEngine.Serialization.FormerlySerializedAs("sceneName")] [SerializeField] private string nomeCena;
        [UnityEngine.Serialization.FormerlySerializedAs("spawnId")] [SerializeField] private string idNascimento;

        private bool _usado;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoNomeCena, string novoIdNascimento)
        {
            nomeCena = novoNomeCena;
            idNascimento = novoIdNascimento;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (_usado) return;
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            if (!Application.CanStreamedLevelBeLoaded(nomeCena))
            {
                Debug.LogError("[SceneExit] A cena " + nomeCena + " nao esta no Build Settings. " +
                               "Gere a fase pelo menu Echoes, que ela e adicionada sozinha.", this);
                return;
            }

            _usado = true;

            // A tela fecha, carrega com o loading no canto e abre do outro lado.
            GameManager.Instancia.TrocarCena(nomeCena, idNascimento);
        }
    }
}
