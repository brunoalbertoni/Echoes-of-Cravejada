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
        [Tooltip("Qual nota ele toca. A Porta Harmonica compara com a sequencia dela.")]
        [SerializeField] private int note;

        [SerializeField] private Color noteColor = Color.white;

        [Tooltip("Ate onde a nota se ouve, em tiles.")]
        [SerializeField] private float radius = 14f;

        [Tooltip("Se comeca com um veio de Oxide, sem soar.")]
        [SerializeField] private bool corrupted;

        [Tooltip("Quantos golpes ate o veio de Oxide quebrar.")]
        [SerializeField] private int hitsToCleanse = 3;

        [Tooltip("Quanto tempo ele fica aceso depois de golpeado. Os pilares da broca ficam mais.")]
        [SerializeField] private float litTime = 0.5f;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private SpriteRenderer vein;
        [SerializeField] private EchoCrystal echo;

        private int _veinHits;
        private float _flashUntil;

        public int Note => note;
        public Color NoteColor => noteColor;
        public bool IsCorrupted => corrupted;

        /// <summary>Se ainda esta aceso do ultimo golpe. A broca so crava num pilar aceso.</summary>
        public bool IsLit => Time.time < _flashUntil;

        /// <summary>
        /// A broca esta vindo pra ele. Apagado, ele pulsa: e o aviso de que
        /// precisa soar de novo antes de ela chegar.
        /// </summary>
        public bool Targeted { get; set; }

        /// <summary>Avisa que ele soou. A Porta Harmonica escuta.</summary>
        public event System.Action<ResonantCrystal> Rang;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(int newNote, Color newColor, bool startsCorrupted,
            SpriteRenderer newSprite, SpriteRenderer newVein, EchoCrystal newEcho)
        {
            note = newNote;
            noteColor = newColor;
            corrupted = startsCorrupted;
            sprite = newSprite;
            vein = newVein;
            echo = newEcho;
        }

        private void OnEnable()
        {
            AreaPurifier.Purifying += Cleanse;
        }

        private void OnDisable()
        {
            AreaPurifier.Purifying -= Cleanse;
        }

        private void Start()
        {
            if (AreaPurifier.IsPurified) corrupted = false;
            if (vein != null) vein.enabled = corrupted;
        }

        /// <summary>O veio de Oxide se dissolve: e o que a purificacao faz com todos de uma vez.</summary>
        public void Cleanse()
        {
            corrupted = false;
            if (vein != null) vein.enabled = false;
        }

        /// <summary>Usado pelos geradores de fase: quanto tempo fica aceso depois do golpe.</summary>
        public void ConfigureLitTime(float seconds)
        {
            litTime = seconds;
        }

        /// <summary>
        /// Faz o cristal acender na cor dele sem soar. E como a porta "toca" a
        /// sequencia pro jogador ver.
        /// </summary>
        public void Preview()
        {
            _flashUntil = Time.time + 0.45f;
            if (echo != null) echo.Flash();

            // A porta "toca" a sequencia: a nota soa, um pouco mais baixa que o golpe.
            Sfx.CrystalNote.Play(transform.position, volume: 0.7f, pitch: Sfx.CrystalNotePitch(note));
        }

        public void Damage(int amount, Vector2 from)
        {
            if (corrupted)
            {
                // Desafinado: um baque curto, que mal se ouve. Cada golpe racha o veio.
                NoiseSystem.Emit(transform.position, 3f, 0.3f);
                Sfx.CrystalDull.Play(transform.position);
                _veinHits++;
                _flashUntil = Time.time + 0.08f;
                if (_veinHits >= hitsToCleanse) Cleanse();
                return;
            }

            _flashUntil = Time.time + litTime;
            if (echo != null) echo.Flash();
            Sfx.CrystalNote.Play(transform.position, pitch: Sfx.CrystalNotePitch(note));

            NoiseSystem.Emit(transform.position, radius, 1f);
            Rang?.Invoke(this);
        }

        private void Update()
        {
            if (sprite != null)
            {
                // Aceso ele mostra a cor da nota; apagado, um resto dela.
                Color dim = Color.Lerp(Color.black, noteColor, 0.45f);
                if (Targeted && Time.time >= _flashUntil)
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 16f);
                    dim = Color.Lerp(dim, Color.white, pulse * 0.6f);
                }

                sprite.color = Time.time < _flashUntil ? noteColor : dim;
            }
        }
    }
}
