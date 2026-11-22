using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Coracao de Quartzo visto pelo janelao atras do trono. So cenario:
    /// ninguem mexe nele durante a luta, mas ele conta a luta.
    ///
    ///   fase 1      branco, pulsando devagar. Quando o Rei avisa um golpe
    ///               (puxa forca), os veios acendem subindo ate o trono.
    ///   transformacao   a rachadura abre e o verde toma o Coracao e os veios.
    ///   fase 2      verde, batendo rapido e torto, como coracao doente.
    ///   depois      verde e fraco, quase parado: espera a escolha no Altar.
    /// </summary>
    public class QuartzHeartView : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("radiant")] [SerializeField] private SpriteRenderer radiante;
        [UnityEngine.Serialization.FormerlySerializedAs("corrupt")] [SerializeField] private SpriteRenderer corrompido;
        [UnityEngine.Serialization.FormerlySerializedAs("veins")] [SerializeField] private SpriteRenderer veias;
        [UnityEngine.Serialization.FormerlySerializedAs("glow")] [SerializeField] private Light2D brilho;
        [UnityEngine.Serialization.FormerlySerializedAs("king")] [SerializeField] private SilverKing rei;

        [UnityEngine.Serialization.FormerlySerializedAs("corruptTime")] [Tooltip("Segundos que o verde leva pra tomar o Coracao na transformacao.")]
        [SerializeField] private float tempoCorrompido = 3f;

        private static readonly Color VeiaLimpo = new Color(0.78f, 0.90f, 1f);
        private static readonly Color VeiaVerde = new Color(0.45f, 1f, 0.66f);

        private float _corrupcao;
        private float _desenho;
        private float _relogio;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(SpriteRenderer novoRadiante, SpriteRenderer novoCorrompido, SpriteRenderer novoVeias, Light2D novoBrilho, SilverKing novoRei)
        {
            radiante = novoRadiante;
            corrompido = novoCorrompido;
            veias = novoVeias;
            brilho = novoBrilho;
            rei = novoRei;
        }

        private void Start()
        {
            _corrupcao = SilverKing.Derrotado || (rei != null && rei.EstaDoisFase) ? 1f : 0f;
        }

        private void Update()
        {
            bool doisFase = SilverKing.Derrotado || (rei != null && (rei.EstaDoisFase || rei.EstaTransformando));
            bool caido = SilverKing.Derrotado || (rei != null && rei.EstaDerrubado);

            _corrupcao = Mathf.MoveTowards(_corrupcao, doisFase ? 1f : 0f, Time.deltaTime / Mathf.Max(0.1f, doisFase ? tempoCorrompido : 0.5f));

            bool desenhando = rei != null && !caido && rei.EstaForcaDesenhando;
            _desenho = Mathf.MoveTowards(_desenho, desenhando ? 1f : 0f, Time.deltaTime * (desenhando ? 5f : 1.6f));

            // O pulso: devagar e redondo na fase 1; rapido, em batida dupla, na 2; quase parado no fim.
            float taxa = caido ? 0.35f : (_corrupcao > 0.5f ? 2.1f : 0.9f);
            _relogio += Time.deltaTime * taxa;
            float batida;
            if (_corrupcao > 0.5f && !caido)
            {
                float t = Mathf.Repeat(_relogio, 1f);
                batida = Mathf.Max(Mathf.Exp(-Mathf.Pow((t - 0.1f) * 12f, 2f)), 0.7f * Mathf.Exp(-Mathf.Pow((t - 0.32f) * 12f, 2f)));
            }
            else
            {
                batida = 0.5f + 0.5f * Mathf.Sin(_relogio * Mathf.PI * 2f);
            }

            // Na transformacao ele treme.
            if (rei != null && rei.EstaTransformando) batida = Mathf.Max(batida, Random.value);

            if (radiante != null) radiante.color = new Color(1f, 1f, 1f, 1f - _corrupcao);
            if (corrompido != null) corrompido.color = new Color(1f, 1f, 1f, _corrupcao);

            if (veias != null)
            {
                Color tonalidade = Color.Lerp(VeiaLimpo, VeiaVerde, _corrupcao);
                tonalidade.a = Mathf.Clamp01(0.18f + batida * 0.22f + _desenho * 0.6f);
                veias.color = tonalidade;
            }

            if (brilho != null)
            {
                brilho.color = Color.Lerp(VeiaLimpo, VeiaVerde, _corrupcao);
                brilho.intensity = (caido ? 0.25f : 0.45f) + batida * 0.35f + _desenho * 0.8f;
            }
        }
    }
}
