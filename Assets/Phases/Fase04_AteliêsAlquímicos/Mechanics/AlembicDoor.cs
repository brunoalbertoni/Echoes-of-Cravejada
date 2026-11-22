using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Grande Alambique do atelie: tres encaixes na base, um pra cada
    /// reagente. Chegar perto com reagentes no bolso encaixa todos; cada um
    /// acende o seu encaixe. Com os tres, o alambique ferve e a grade do
    /// mezanino (o caminho pro Grande Laboratorio) se abre.
    ///
    /// Sem nada no bolso, um aviso — uma vez por aproximacao.
    /// </summary>
    public class AlembicDoor : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("reagentIds")] [SerializeField] private string[] idsReagente;
        [UnityEngine.Serialization.FormerlySerializedAs("reagentNames")] [SerializeField] private string[] nomesReagente;
        [UnityEngine.Serialization.FormerlySerializedAs("gate")] [SerializeField] private Gate portao;
        [UnityEngine.Serialization.FormerlySerializedAs("body")] [SerializeField] private SpriteRenderer corpo;

        [UnityEngine.Serialization.FormerlySerializedAs("stages")] [Tooltip("O desenho com 0, 1, 2 e 3 encaixes acesos.")]
        [SerializeField] private Sprite[] estagios;

        private bool _avisado;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string[] ids, string[] nomes, Gate novoPortao, SpriteRenderer novoCorpo, Sprite[] novoEstagios)
        {
            idsReagente = ids;
            nomesReagente = nomes;
            portao = novoPortao;
            corpo = novoCorpo;
            estagios = novoEstagios;
        }

        private int Colocado
        {
            get
            {
                int quantidade = 0;
                if (idsReagente == null) return 0;
                foreach (string id in idsReagente) if (KeyItems.FoiEntregue(id)) quantidade++;
                return quantidade;
            }
        }

        private void Start()
        {
            Atualizar();
            if (Colocado >= idsReagente.Length && portao != null) portao.Abrir();
        }

        private void Atualizar()
        {
            if (corpo == null || estagios == null || estagios.Length == 0) return;
            corpo.sprite = estagios[Mathf.Clamp(Colocado, 0, estagios.Length - 1)];
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (outro.GetComponentInParent<ScrapController>() == null || idsReagente == null) return;

            bool algumColocado = false;
            string ultimo = null;

            for (int i = 0; i < idsReagente.Length; i++)
            {
                if (!KeyItems.Entregar(idsReagente[i])) continue;

                algumColocado = true;
                ultimo = nomesReagente != null && i < nomesReagente.Length ? nomesReagente[i] : "o reagente";
            }

            int colocado = Colocado;
            Atualizar();

            if (algumColocado)
            {
                NoiseSystem.Emitir(transform.position, 8f, 0.4f);
                Sfx.InserirAlambique.Tocar(transform.position);

                if (colocado >= idsReagente.Length)
                {
                    Sfx.FerverAlambique.Tocar(transform.position, atraso: 0.5f);
                    if (portao != null) portao.Abrir();
                }

                return;
            }

            if (colocado < idsReagente.Length && !_avisado)
            {
                _avisado = true;
            }
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (outro.GetComponentInParent<ScrapController>() != null) _avisado = false;
        }
    }
}
