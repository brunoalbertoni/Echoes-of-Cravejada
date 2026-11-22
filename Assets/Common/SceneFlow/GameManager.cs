using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O ponto central do fluxo do jogo: comecar, continuar, trocar de cena,
    /// parar o tempo, salvar e sair. As telas e mecanicas pedem essas acoes
    /// aqui, pelo <see cref="Instancia"/>.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instancia;

        public static GameManager Instancia => instancia;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            instancia = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Inicializar()
        {
            if (instancia != null) return;

            var hospedeiro = new GameObject("GameManager");
            DontDestroyOnLoad(hospedeiro);
            instancia = hospedeiro.AddComponent<GameManager>();
        }

        private void OnDestroy()
        {
            if (instancia == this) instancia = null;
        }

        /// <summary>Apaga o progresso da memoria e abre a primeira fase.</summary>
        public void NovoJogo()
        {
            SaveSystem.ReiniciarProgresso();
            TrocarCena(MainMenu.PrimeiroCena, null);
        }

        /// <summary>Carrega o save e abre a cena dele. Falso se nao houver o que continuar.</summary>
        public bool ContinuarJogo()
        {
            string cena = SaveSystem.Carregar();
            if (string.IsNullOrEmpty(cena) || !Application.CanStreamedLevelBeLoaded(cena)) return false;

            TrocarCena(cena, null);
            return true;
        }

        public void SalvarJogo(Vector3 bancada)
        {
            SaveSystem.Salvar(bancada);
        }

        public void TrocarCena(string cena, string pontoDeChegada, bool telaJaPreta = false)
        {
            SceneLoader.Ir(cena, pontoDeChegada, telaJaPreta);
        }

        public void VoltarAoMenu()
        {
            TrocarCena(MainMenu.NomeCena, null);
        }

        /// <summary>Para o mundo e devolve a escala de tempo que estava valendo.</summary>
        public float CongelarTempo()
        {
            float anterior = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            return anterior;
        }

        public void RestaurarTempo(float escala)
        {
            Time.timeScale = escala;
        }

        public void SairDoJogo()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
