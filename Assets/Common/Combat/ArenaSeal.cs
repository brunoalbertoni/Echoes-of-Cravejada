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
        [UnityEngine.Serialization.FormerlySerializedAs("slideTime")] [Tooltip("Segundos pra grade descer ou subir.")]
        [SerializeField] private float tempoDeslize = 0.25f;

        [UnityEngine.Serialization.FormerlySerializedAs("boss")] [Tooltip("O chefe desta arena. Vazio: fecha com qualquer chefe em luta (cenas de um chefe so).")]
        [SerializeField] private MonoBehaviour chefe;

        private Collider2D _parede;
        private SpriteRenderer _barras;
        private Color _cor;
        private float _fechada;
        private bool _estavaLutando;

        private void Awake()
        {
            _parede = GetComponent<Collider2D>();
            _barras = GetComponentInChildren<SpriteRenderer>();
            if (_barras != null) _cor = _barras.color;

            Aplicar(0f);
        }

        /// <summary>Usado pelos geradores de fase, onde ha mais de um chefe na cena: a grade so ouve este.</summary>
        public void Observar(MonoBehaviour novoChefe)
        {
            chefe = novoChefe;
        }

        /// <summary>Se algum chefe esta em luta agora.</summary>
        private static bool LutandoChefeAlgum =>
            BossRoster.Atual != null;

        /// <summary>Se o chefe desta arena esta em luta.</summary>
        private bool LutandoChefeProprio =>
            BossRoster.EstaLutando(chefe);

        private void Update()
        {
            bool lutando = chefe != null ? LutandoChefeProprio : LutandoChefeAlgum;

            if (lutando != _estavaLutando)
            {
                _estavaLutando = lutando;
                (lutando ? Sfx.BaixoSeloArena : Sfx.CimaSeloArena).Tocar(transform.position);
            }

            float alvo = lutando ? 1f : 0f;
            _fechada = Mathf.MoveTowards(_fechada, alvo, Time.deltaTime / Mathf.Max(0.01f, tempoDeslize));
            Aplicar(_fechada);

            // Fecha na hora em que a luta comeca, mesmo com a grade ainda descendo.
            _parede.enabled = lutando;
        }

        private void Aplicar(float fechada)
        {
            if (_barras == null) return;

            _barras.enabled = fechada > 0.01f;
            _barras.color = new Color(_cor.r, _cor.g, _cor.b, _cor.a * fechada);
        }
    }
}
