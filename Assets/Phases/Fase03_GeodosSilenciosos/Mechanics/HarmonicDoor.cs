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
        [UnityEngine.Serialization.FormerlySerializedAs("crystals")] [Tooltip("Os cristais da sala. Todos sao escutados, entrem ou nao na sequencia.")]
        [SerializeField] private ResonantCrystal[] cristais;

        [UnityEngine.Serialization.FormerlySerializedAs("sequence")] [Tooltip("As notas, na ordem em que precisam soar.")]
        [SerializeField] private int[] sequencia;

        [UnityEngine.Serialization.FormerlySerializedAs("gate")] [SerializeField] private Gate portao;

        [UnityEngine.Serialization.FormerlySerializedAs("glyphs")] [Tooltip("As gemas gravadas em cima da porta, uma por nota. Acendem conforme a sequencia avanca.")]
        [SerializeField] private SpriteRenderer[] glifos;

        [UnityEngine.Serialization.FormerlySerializedAs("hidden")] [Tooltip("Porta que nao entrega a ordem: nao toca a previa. As gemas so contam quantas notas ja foram.")]
        [SerializeField] private bool oculto;

        private int _proximo;
        private bool _aberto;
        private bool _previsto;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(ResonantCrystal[] novoCristais, int[] novoSequencia, Gate novoPortao,
            SpriteRenderer[] novoGlifos, bool novoOculto = false)
        {
            cristais = novoCristais;
            sequencia = novoSequencia;
            portao = novoPortao;
            glifos = novoGlifos;
            oculto = novoOculto;
        }

        private void OnEnable()
        {
            if (cristais == null) return;

            foreach (ResonantCrystal cristal in cristais)
            {
                if (cristal != null) cristal.Soou += AoSoar;
            }
        }

        private void OnDisable()
        {
            if (cristais == null) return;

            foreach (ResonantCrystal cristal in cristais)
            {
                if (cristal != null) cristal.Soou -= AoSoar;
            }
        }

        private void Start()
        {
            MostrarProgresso();
        }

        private void AoSoar(ResonantCrystal cristal)
        {
            if (_aberto || sequencia == null || sequencia.Length == 0) return;

            if (cristal.Nota == sequencia[_proximo])
            {
                _proximo++;
            }
            else
            {
                // Errou: recomeca. Se a nota errada for a primeira da
                // sequencia, ela ja vale como recomeco. So soa o erro se ja
                // havia o que perder.
                if (_proximo > 0) Sfx.ErradoEnigma.Tocar(transform.position, atraso: 0.25f);
                _proximo = cristal.Nota == sequencia[0] ? 1 : 0;
            }

            MostrarProgresso();

            if (_proximo < sequencia.Length) return;

            _aberto = true;
            Sfx.ResolvidoEnigma.Tocar(transform.position, atraso: 0.4f);
            if (portao != null) portao.Abrir();
        }

        /// <summary>As gemas ja acertadas ficam acesas; as outras, apagadas.</summary>
        private void MostrarProgresso()
        {
            if (glifos == null) return;

            for (int i = 0; i < glifos.Length; i++)
            {
                if (glifos[i] == null) continue;

                Color cor = glifos[i].color;
                cor.a = _aberto || i < _proximo ? 1f : 0.45f;
                glifos[i].color = cor;
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (_aberto || _previsto || oculto) return;
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            _previsto = true;
            StartCoroutine(Previa());
        }

        /// <summary>Toca a sequencia uma vez, acendendo cada cristal na sua vez.</summary>
        private IEnumerator Previa()
        {
            yield return new WaitForSeconds(0.6f);

            foreach (int nota in sequencia)
            {
                foreach (ResonantCrystal cristal in cristais)
                {
                    if (cristal != null && cristal.Nota == nota) cristal.Previa();
                }

                yield return new WaitForSeconds(0.7f);
            }
        }
    }
}
