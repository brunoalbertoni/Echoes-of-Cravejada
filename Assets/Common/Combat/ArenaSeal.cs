using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A grade que fecha a entrada da arena de um chefe (no District 1, a boca
    /// do poco do Coletor; no District 2, a passagem da sala pre-boss; no
    /// District 3, a boca da toca do Capataz e a entrada da broca). Fica
    /// aberta; fecha quando ele acorda e abre quando a luta acaba — ele caiu,
    /// ou o Scrap caiu e a luta recomecou do zero.
    ///
    /// Fechada, e chao e teto: nao se volta pra Antessala no meio da luta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ArenaSeal : MonoBehaviour
    {
        [Tooltip("Segundos pra grade descer ou subir.")]
        [SerializeField] private float slideTime = 0.25f;

        [Tooltip("O chefe desta arena. Vazio: fecha com qualquer chefe em luta (cenas de um chefe so).")]
        [SerializeField] private MonoBehaviour boss;

        private Collider2D _wall;
        private SpriteRenderer _bars;
        private Color _color;
        private float _shut;

        private void Awake()
        {
            _wall = GetComponent<Collider2D>();
            _bars = GetComponentInChildren<SpriteRenderer>();
            if (_bars != null) _color = _bars.color;

            Apply(0f);
        }

        /// <summary>Usado pelos geradores de fase, onde ha mais de um chefe na cena: a grade so ouve este.</summary>
        public void Watch(MonoBehaviour newBoss)
        {
            boss = newBoss;
        }

        /// <summary>Se algum chefe esta em luta agora.</summary>
        private static bool AnyBossFighting =>
            RustyScrapper.Fighting != null || BoilerMaster.Fighting != null
            || TheResonant.Fighting != null || GeodeDrill.Fighting != null
            || MercuryAlchemist.Fighting != null || SilverKing.Fighting != null;

        /// <summary>Se o chefe desta arena esta em luta.</summary>
        private bool OwnBossFighting =>
            boss == RustyScrapper.Fighting || boss == BoilerMaster.Fighting
            || boss == TheResonant.Fighting || boss == GeodeDrill.Fighting
            || boss == MercuryAlchemist.Fighting || boss == SilverKing.Fighting;

        private void Update()
        {
            bool fighting = boss != null ? OwnBossFighting : AnyBossFighting;

            float target = fighting ? 1f : 0f;
            _shut = Mathf.MoveTowards(_shut, target, Time.deltaTime / Mathf.Max(0.01f, slideTime));
            Apply(_shut);

            // Fecha na hora em que a luta comeca, mesmo com a grade ainda descendo.
            _wall.enabled = fighting;
        }

        private void Apply(float shut)
        {
            if (_bars == null) return;

            _bars.enabled = shut > 0.01f;
            _bars.color = new Color(_color.r, _color.g, _color.b, _color.a * shut);
        }
    }
}
