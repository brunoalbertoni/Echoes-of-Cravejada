using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um cristal ressonante: golpeado com o cinzel, ele soa uma nota — um
    /// ruido forte, que vem dele e nao do Scrap. Por isso serve de isca: os
    /// que cacam pelo som correm pro cristal.
    ///
    /// Cada um tem a sua nota (um numero e uma cor). A Porta Harmonica escuta
    /// a ordem em que eles sao tocados.
    ///
    /// Um cristal corrompido tem um veio de Oxide dentro e nao soa: da so um
    /// baque surdo. Tres golpes de cinzel quebram o veio e o afinam de volta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ResonantCrystal : MonoBehaviour, IDamageable
    {
        [UnityEngine.Serialization.FormerlySerializedAs("note")] [Tooltip("Qual nota ele toca. A Porta Harmonica compara com a sequencia dela.")]
        [SerializeField] private int nota;

        [UnityEngine.Serialization.FormerlySerializedAs("noteColor")] [SerializeField] private Color corNota = Color.white;

        [UnityEngine.Serialization.FormerlySerializedAs("radius")] [Tooltip("Ate onde a nota se ouve, em tiles.")]
        [SerializeField] private float raioArea = 14f;

        [UnityEngine.Serialization.FormerlySerializedAs("corrupted")] [Tooltip("Se comeca com um veio de Oxide, sem soar.")]
        [SerializeField] private bool corrompida;

        [UnityEngine.Serialization.FormerlySerializedAs("hitsToCleanse")] [Tooltip("Quantos golpes ate o veio de Oxide quebrar.")]
        [SerializeField] private int golpesParaLimpeza = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("litTime")] [Tooltip("Quanto tempo ele fica aceso depois de golpeado. Os pilares da broca ficam mais.")]
        [SerializeField] private float tempoAceso = 0.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("sprite")] [SerializeField] private SpriteRenderer sprite;
        [UnityEngine.Serialization.FormerlySerializedAs("vein")] [SerializeField] private SpriteRenderer veia;
        [UnityEngine.Serialization.FormerlySerializedAs("echo")] [SerializeField] private EchoCrystal eco;

        private int _golpesVeia;
        private float _claraoAte;

        public int Nota => nota;
        public Color CorNota => corNota;
        public bool EstaCorrompida => corrompida;

        /// <summary>Se ainda esta aceso do ultimo golpe. A broca so crava num pilar aceso.</summary>
        public bool EstaAceso => Time.time < _claraoAte;

        /// <summary>
        /// A broca esta vindo pra ele. Apagado, ele pulsa: e o aviso de que
        /// precisa soar de novo antes de ela chegar.
        /// </summary>
        public bool Mirado { get; set; }

        /// <summary>Avisa que ele soou. A Porta Harmonica escuta.</summary>
        public event System.Action<ResonantCrystal> Soou;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(int novoNota, Color novoCor, bool corrompidaComeca,
            SpriteRenderer novoSprite, SpriteRenderer novoVeia, EchoCrystal novoEco)
        {
            nota = novoNota;
            corNota = novoCor;
            corrompida = corrompidaComeca;
            sprite = novoSprite;
            veia = novoVeia;
            eco = novoEco;
        }

        private void OnEnable()
        {
            AreaPurifier.Purificando += Limpar;
        }

        private void OnDisable()
        {
            AreaPurifier.Purificando -= Limpar;
        }

        private void Start()
        {
            if (AreaPurifier.EstaPurificado) corrompida = false;
            if (veia != null) veia.enabled = corrompida;
        }

        /// <summary>O veio de Oxide se dissolve: e o que a purificacao faz com todos de uma vez.</summary>
        public void Limpar()
        {
            corrompida = false;
            if (veia != null) veia.enabled = false;
        }

        /// <summary>Usado pelos geradores de fase: quanto tempo fica aceso depois do golpe.</summary>
        public void ConfigurarTempoAceso(float segundos)
        {
            tempoAceso = segundos;
        }

        /// <summary>
        /// Faz o cristal acender na cor dele sem soar. E como a porta "toca" a
        /// sequencia pro jogador ver.
        /// </summary>
        public void Previa()
        {
            _claraoAte = Time.time + 0.45f;
            if (eco != null) eco.Piscada();

            // A porta "toca" a sequencia: a nota soa, um pouco mais baixa que o golpe.
            Sfx.NotaCristal.Tocar(transform.position, volume: 0.7f, tom2: Sfx.TomDaNotaDoCristal(nota));
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (corrompida)
            {
                // Desafinado: um baque curto, que mal se ouve. Cada golpe racha o veio.
                NoiseSystem.Emitir(transform.position, 3f, 0.3f);
                Sfx.OpacoCristal.Tocar(transform.position);
                _golpesVeia++;
                _claraoAte = Time.time + 0.08f;
                if (_golpesVeia >= golpesParaLimpeza) Limpar();
                return;
            }

            _claraoAte = Time.time + tempoAceso;
            if (eco != null) eco.Piscada();
            Sfx.NotaCristal.Tocar(transform.position, tom2: Sfx.TomDaNotaDoCristal(nota));

            NoiseSystem.Emitir(transform.position, raioArea, 1f);
            Soou?.Invoke(this);
        }

        private void Update()
        {
            if (sprite != null)
            {
                // Aceso ele mostra a cor da nota; apagado, um resto dela.
                Color apagado = Color.Lerp(Color.black, corNota, 0.45f);
                if (Mirado && Time.time >= _claraoAte)
                {
                    float pulso = 0.5f + 0.5f * Mathf.Sin(Time.time * 16f);
                    apagado = Color.Lerp(apagado, Color.white, pulso * 0.6f);
                }

                sprite.color = Time.time < _claraoAte ? corNota : apagado;
            }
        }
    }
}
