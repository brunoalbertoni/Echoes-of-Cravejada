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
        [SerializeField] private SpriteRenderer radiant;
        [SerializeField] private SpriteRenderer corrupt;
        [SerializeField] private SpriteRenderer veins;
        [SerializeField] private Light2D glow;
        [SerializeField] private SilverKing king;

        [Tooltip("Segundos que o verde leva pra tomar o Coracao na transformacao.")]
        [SerializeField] private float corruptTime = 3f;

        private static readonly Color CleanVein = new Color(0.78f, 0.90f, 1f);
        private static readonly Color GreenVein = new Color(0.45f, 1f, 0.66f);

        private float _corruption;
        private float _draw;
        private float _clock;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SpriteRenderer newRadiant, SpriteRenderer newCorrupt, SpriteRenderer newVeins, Light2D newGlow, SilverKing newKing)
        {
            radiant = newRadiant;
            corrupt = newCorrupt;
            veins = newVeins;
            glow = newGlow;
            king = newKing;
        }

        private void Start()
        {
            _corruption = SilverKing.Defeated || (king != null && king.IsPhaseTwo) ? 1f : 0f;
        }

        private void Update()
        {
            bool phaseTwo = SilverKing.Defeated || (king != null && (king.IsPhaseTwo || king.IsTransforming));
            bool fallen = SilverKing.Defeated || (king != null && king.IsDown);

            _corruption = Mathf.MoveTowards(_corruption, phaseTwo ? 1f : 0f, Time.deltaTime / Mathf.Max(0.1f, phaseTwo ? corruptTime : 0.5f));

            bool drawing = king != null && !fallen && king.IsDrawingPower;
            _draw = Mathf.MoveTowards(_draw, drawing ? 1f : 0f, Time.deltaTime * (drawing ? 5f : 1.6f));

            // O pulso: devagar e redondo na fase 1; rapido, em batida dupla, na 2; quase parado no fim.
            float rate = fallen ? 0.35f : (_corruption > 0.5f ? 2.1f : 0.9f);
            _clock += Time.deltaTime * rate;
            float beat;
            if (_corruption > 0.5f && !fallen)
            {
                float t = Mathf.Repeat(_clock, 1f);
                beat = Mathf.Max(Mathf.Exp(-Mathf.Pow((t - 0.1f) * 12f, 2f)), 0.7f * Mathf.Exp(-Mathf.Pow((t - 0.32f) * 12f, 2f)));
            }
            else
            {
                beat = 0.5f + 0.5f * Mathf.Sin(_clock * Mathf.PI * 2f);
            }

            // Na transformacao ele treme.
            if (king != null && king.IsTransforming) beat = Mathf.Max(beat, Random.value);

            if (radiant != null) radiant.color = new Color(1f, 1f, 1f, 1f - _corruption);
            if (corrupt != null) corrupt.color = new Color(1f, 1f, 1f, _corruption);

            if (veins != null)
            {
                Color tone = Color.Lerp(CleanVein, GreenVein, _corruption);
                tone.a = Mathf.Clamp01(0.18f + beat * 0.22f + _draw * 0.6f);
                veins.color = tone;
            }

            if (glow != null)
            {
                glow.color = Color.Lerp(CleanVein, GreenVein, _corruption);
                glow.intensity = (fallen ? 0.25f : 0.45f) + beat * 0.35f + _draw * 0.8f;
            }
        }
    }
}
