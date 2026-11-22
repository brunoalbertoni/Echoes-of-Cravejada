using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um fim do jogo, tocado por uma das escolhas (o trono ou a coroa): a
    /// camera fecha no ponto da escolha, a sala clareia, o filme do final
    /// passa e, no preto, entra a tela de fim (EndScreen) daquele final, com
    /// a historia curta e, depois de uns segundos, a placa de voltar ao menu.
    ///
    /// Depois disso o controle nao volta.
    /// </summary>
    public class FinaleDirector : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("polishCartoon")] [SerializeField] private FilmCutscene desenhoPolimento;
        [UnityEngine.Serialization.FormerlySerializedAs("conduit")] [SerializeField] private SpriteRenderer conduto;
        [UnityEngine.Serialization.FormerlySerializedAs("conduitClean")] [SerializeField] private Sprite limpoConduto;
        [UnityEngine.Serialization.FormerlySerializedAs("globalLight")] [SerializeField] private Light2D luzGlobal;
        [UnityEngine.Serialization.FormerlySerializedAs("focus")] [SerializeField] private Vector2 foco;

        [UnityEngine.Serialization.FormerlySerializedAs("endId")] [Header("A tela de fim")]
        [Tooltip("O nome do final em Resources/EndScreen ({id}_title, {id}_line0...).")]
        [SerializeField] private string idFim = "column";
        [UnityEngine.Serialization.FormerlySerializedAs("lineCount")] [SerializeField] private int quantidadeLinha = 5;

        [UnityEngine.Serialization.FormerlySerializedAs("spores")] [Tooltip("Esporos verdes subindo na tela de fim (o Oxide ainda esta la).")]
        [SerializeField] private bool esporos;

        private bool _iniciado;

        /// <summary>Se o final ja comecou (a trilha sonora troca pra musica do fim).</summary>
        public static bool Correndo { get; private set; }

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Correndo = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(FilmCutscene polimento, SpriteRenderer novoConduto, Sprite novoLimpoConduto, Light2D global, Vector2 novoFoco)
        {
            desenhoPolimento = polimento;
            conduto = novoConduto;
            limpoConduto = novoLimpoConduto;
            luzGlobal = global;
            foco = novoFoco;
        }

        /// <summary>Usado pelos geradores de fase: qual tela de fim, quantas linhas e se tem esporos.</summary>
        public void ConfigurarFinal(string novoId, int novoLinhas, bool novoEsporos)
        {
            idFim = novoId;
            quantidadeLinha = novoLinhas;
            esporos = novoEsporos;
        }

        public void Iniciar()
        {
            if (_iniciado) return;
            _iniciado = true;
            Correndo = true;
            StartCoroutine(Executar());
        }

        private IEnumerator Executar()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) controlador.ControleTravado = true;
            if (!Cinematic.Ativo) Cinematic.Iniciar();

            ScrapCameraFollow.Direcionar(foco + Vector2.up * 3f, 0.9f, 0.8f);

            // O golpe: a sala clareia antes do filme.
            Sfx.LuzFinalJogo.Tocar(atraso: 0.3f);
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                if (luzGlobal != null) luzGlobal.intensity = Mathf.Lerp(luzGlobal.intensity, 1.1f, Time.deltaTime);
                yield return null;
            }

            if (conduto != null && limpoConduto != null) conduto.sprite = limpoConduto;

            // O filme termina no preto; a tela de fim entra por baixo dele.
            if (desenhoPolimento != null && desenhoPolimento.EstaProntoUso) yield return desenhoPolimento.Tocar(() => EndScreen.Mostrar(idFim, quantidadeLinha, esporos));
            else EndScreen.Mostrar(idFim, quantidadeLinha, esporos);
        }
    }
}
