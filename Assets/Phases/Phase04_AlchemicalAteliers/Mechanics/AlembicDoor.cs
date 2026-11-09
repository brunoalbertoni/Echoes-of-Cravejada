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
        [SerializeField] private string[] reagentIds;
        [SerializeField] private string[] reagentNames;
        [SerializeField] private Gate gate;
        [SerializeField] private SpriteRenderer body;

        [Tooltip("O desenho com 0, 1, 2 e 3 encaixes acesos.")]
        [SerializeField] private Sprite[] stages;

        private bool _warned;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string[] ids, string[] names, Gate newGate, SpriteRenderer newBody, Sprite[] newStages)
        {
            reagentIds = ids;
            reagentNames = names;
            gate = newGate;
            body = newBody;
            stages = newStages;
        }

        private int Placed
        {
            get
            {
                int count = 0;
                if (reagentIds == null) return 0;
                foreach (string id in reagentIds) if (KeyItems.WasDelivered(id)) count++;
                return count;
            }
        }

        private void Start()
        {
            Refresh();
            if (Placed >= reagentIds.Length && gate != null) gate.Open();
        }

        private void Refresh()
        {
            if (body == null || stages == null || stages.Length == 0) return;
            body.sprite = stages[Mathf.Clamp(Placed, 0, stages.Length - 1)];
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null || reagentIds == null) return;

            bool placedAny = false;
            string last = null;

            for (int i = 0; i < reagentIds.Length; i++)
            {
                if (!KeyItems.Deliver(reagentIds[i])) continue;

                placedAny = true;
                last = reagentNames != null && i < reagentNames.Length ? reagentNames[i] : "o reagente";
            }

            int placed = Placed;
            Refresh();

            if (placedAny)
            {
                NoiseSystem.Emit(transform.position, 8f, 0.4f);
                Sfx.AlembicInsert.Play(transform.position);

                if (placed >= reagentIds.Length)
                {
                    Sfx.AlembicBoil.Play(transform.position, delay: 0.5f);
                    HudNotice.Show("O Grande Alambique ferve. A grade do mezanino se abriu.", 5f);
                    if (gate != null) gate.Open();
                }
                else
                {
                    HudNotice.Show($"{last} encaixado no alambique ({placed}/{reagentIds.Length})", 4f);
                }

                return;
            }

            if (placed < reagentIds.Length && !_warned)
            {
                _warned = true;
                HudNotice.Show($"O Grande Alambique tem três encaixes. Faltam {reagentIds.Length - placed} reagentes.", 4f);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() != null) _warned = false;
        }
    }
}
