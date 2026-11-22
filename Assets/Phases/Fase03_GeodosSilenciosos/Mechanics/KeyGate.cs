using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma grade com fechadura. Chegar perto com a chave no bolso gira a
    /// fechadura e abre a grade (a chave e gasta). Sem ela, um aviso — uma
    /// vez por aproximacao, pra nao encher a tela.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KeyGate : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("keyId")] [SerializeField] private string idChave;
        [UnityEngine.Serialization.FormerlySerializedAs("keyName")] [SerializeField] private string nomeChave = "a chave";
        [UnityEngine.Serialization.FormerlySerializedAs("gate")] [SerializeField] private Gate portao;

        [UnityEngine.Serialization.FormerlySerializedAs("lockSprite")] [Tooltip("O desenho da fechadura: some quando abre.")]
        [SerializeField] private SpriteRenderer spriteTrava;

        [SaveProgress("Opened")]
        private static readonly System.Collections.Generic.HashSet<string> Abertos =
            new System.Collections.Generic.HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Abertos.Clear();
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoIdChave, string novoNomeChave, Gate novoPortao, SpriteRenderer novoTrava)
        {
            idChave = novoIdChave;
            nomeChave = novoNomeChave;
            portao = novoPortao;
            spriteTrava = novoTrava;
        }

        private void Start()
        {
            // Ja aberta numa visita anterior: fica aberta.
            if (Abertos.Contains(idChave)) Abrir(false);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (outro.GetComponentInParent<ScrapController>() == null) return;
            if (Abertos.Contains(idChave)) return;

            if (!KeyItems.Tem(idChave))
            {
                Sfx.ChocalharTrava.Tocar(transform.position);
                return;
            }

            KeyItems.Entregar(idChave);
            Sfx.DesbloquearTrava.Tocar(transform.position);
            Abrir(true);
        }

        private void Abrir(bool altoSom)
        {
            Abertos.Add(idChave);
            if (spriteTrava != null) spriteTrava.enabled = false;

            // A fechadura rangendo e a grade correndo: quem esta perto escuta.
            if (altoSom) NoiseSystem.Emitir(transform.position, 10f, 0.7f);
            if (portao != null) portao.Abrir();

            GetComponent<Collider2D>().enabled = false;
        }
    }
}
