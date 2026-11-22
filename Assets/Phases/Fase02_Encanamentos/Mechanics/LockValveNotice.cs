using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Vai numa valvula do District 2 cujo efeito acontece longe da vista de
    /// quem gira: as tres do fim das alas, que abrem as travas da Escadaria da
    /// Praca. Quando o volante gira, uma cena curta e muda: o Scrap para, a
    /// camera vai ate a trava, ela se abre, e a camera volta. Nada na tela.
    /// </summary>
    [RequireComponent(typeof(Valve))]
    public class LockValveNotice : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("countsAsLock")] [Tooltip("Se abre uma das travas da Escadaria (e mostra a cena).")]
        [SerializeField] private bool quantidadesComoTrava = true;

        [UnityEngine.Serialization.FormerlySerializedAs("extra")] [Tooltip("Sobra de quando havia aviso escrito. Nao vai mais pra tela.")]
        [SerializeField] private string extra;

        private Valve _valvula;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(bool novoQuantidadesComoTrava, string novoExtra)
        {
            quantidadesComoTrava = novoQuantidadesComoTrava;
            extra = novoExtra;
        }

        private void Awake()
        {
            _valvula = GetComponent<Valve>();
            if (quantidadesComoTrava) _valvula.PortoesAdiar = true;
        }

        private void OnEnable()
        {
            _valvula.Girado += AoGirar;
        }

        private void OnDisable()
        {
            _valvula.Girado -= AoGirar;
        }

        private void AoGirar(Valve valvula)
        {
            if (quantidadesComoTrava) StartCoroutine(MostrarTrava());
        }

        private IEnumerator MostrarTrava()
        {
            Gate[] portoes = _valvula.Portoes;
            if (portoes == null || portoes.Length == 0) yield break;

            Vector2 foco = Vector2.zero;
            int quantidade = 0;
            foreach (Gate portao in portoes)
            {
                if (portao == null) continue;
                foco += (Vector2)portao.transform.position;
                quantidade++;
            }

            if (quantidade == 0) yield break;
            foco /= quantidade;

            ScrapController sucata = ScrapRef.Controlador;
            if (sucata != null) sucata.ControleTravado = true;

            // Deixa o volante terminar de girar antes de sair.
            yield return new WaitForSeconds(0.5f);

            ScrapCameraFollow.Direcionar(foco, 0.85f, 0.55f);
            yield return new WaitForSeconds(1.3f);

            foreach (Gate portao in portoes)
            {
                if (portao != null) portao.Abrir();
            }

            ScrapCameraFollow.Tremer(0.08f, 0.5f);
            yield return new WaitForSeconds(1.6f);

            ScrapCameraFollow.Liberar();
            yield return new WaitForSeconds(0.9f);

            if (sucata != null) sucata.ControleTravado = false;
        }
    }
}
