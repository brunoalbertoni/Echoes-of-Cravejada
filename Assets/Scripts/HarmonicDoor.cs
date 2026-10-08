using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A Porta Harmonica: so abre quando os cristais ressonantes da sala sao
    /// tocados na ordem certa.
    ///
    /// Ao chegar perto dela, a porta "toca" a sequencia uma vez — os cristais
    /// acendem na ordem, cada um na sua cor. As mesmas cores ficam gravadas em
    /// cima da porta, da esquerda pra direita, pra quem perdeu ou prefere ler.
    ///
    /// Tocar a nota errada zera a tentativa. Nao ha punicao alem do barulho —
    /// que, neste distrito, ja e punicao.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HarmonicDoor : MonoBehaviour
    {
        [Tooltip("Os cristais da sala. Todos sao escutados, entrem ou nao na sequencia.")]
        [SerializeField] private ResonantCrystal[] crystals;

        [Tooltip("As notas, na ordem em que precisam soar.")]
        [SerializeField] private int[] sequence;

        [SerializeField] private Gate gate;

        [Tooltip("As gemas gravadas em cima da porta, uma por nota. Acendem conforme a sequencia avanca.")]
        [SerializeField] private SpriteRenderer[] glyphs;

        private int _next;
        private bool _open;
        private bool _previewed;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(ResonantCrystal[] newCrystals, int[] newSequence, Gate newGate,
            SpriteRenderer[] newGlyphs)
        {
            crystals = newCrystals;
            sequence = newSequence;
            gate = newGate;
            glyphs = newGlyphs;
        }

        private void OnEnable()
        {
            if (crystals == null) return;

            foreach (ResonantCrystal crystal in crystals)
            {
                if (crystal != null) crystal.Rang += OnRang;
            }
        }

        private void OnDisable()
        {
            if (crystals == null) return;

            foreach (ResonantCrystal crystal in crystals)
            {
                if (crystal != null) crystal.Rang -= OnRang;
            }
        }

        private void Start()
        {
            ShowProgress();
        }

        private void OnRang(ResonantCrystal crystal)
        {
            if (_open || sequence == null || sequence.Length == 0) return;

            if (crystal.Note == sequence[_next])
            {
                _next++;
            }
            else
            {
                // Errou: recomeca. Se a nota errada for a primeira da
                // sequencia, ela ja vale como recomeco.
                _next = crystal.Note == sequence[0] ? 1 : 0;
            }

            ShowProgress();

            if (_next < sequence.Length) return;

            _open = true;
            if (gate != null) gate.Open();
        }

        /// <summary>As gemas ja acertadas ficam acesas; as outras, apagadas.</summary>
        private void ShowProgress()
        {
            if (glyphs == null) return;

            for (int i = 0; i < glyphs.Length; i++)
            {
                if (glyphs[i] == null) continue;

                Color color = glyphs[i].color;
                color.a = _open || i < _next ? 1f : 0.45f;
                glyphs[i].color = color;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_open || _previewed) return;
            if (other.GetComponentInParent<ScrapController>() == null) return;

            _previewed = true;
            StartCoroutine(Preview());
        }

        /// <summary>Toca a sequencia uma vez, acendendo cada cristal na sua vez.</summary>
        private IEnumerator Preview()
        {
            yield return new WaitForSeconds(0.6f);

            foreach (int note in sequence)
            {
                foreach (ResonantCrystal crystal in crystals)
                {
                    if (crystal != null && crystal.Note == note) crystal.Preview();
                }

                yield return new WaitForSeconds(0.7f);
            }
        }
    }
}
