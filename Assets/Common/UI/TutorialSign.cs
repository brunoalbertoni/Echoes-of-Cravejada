using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma placa de tutorial fincada no chao. Enquanto o Scrap esta na area
    /// dela, um painel desliza pro canto de cima, a direita, com o nome do
    /// movimento, a mini cena do Scrap fazendo e as teclas. Nao precisa
    /// apertar nada: e so chegar perto. Saiu da area, o painel recolhe.
    ///
    /// Some durante cena, conversa e card de conquista, pra nao empilhar
    /// caixa em cima de caixa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TutorialSign : MonoBehaviour
    {
        [SerializeField] private ScrapMove move = ScrapMove.WallJump;

        [Tooltip("O desenho da placa: pisca de leve enquanto o painel esta aberto.")]
        [SerializeField] private SpriteRenderer art;

        private bool _inside;
        private float _shown;
        private float _openedAt;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(ScrapMove newMove, SpriteRenderer newArt)
        {
            move = newMove;
            art = newArt;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null) return;

            // A mini cena recomeca a cada chegada, se o painel ja tinha fechado.
            if (_shown <= 0.01f) _openedAt = Time.unscaledTime;
            _inside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null) return;
            _inside = false;
        }

        private void OnDisable()
        {
            _inside = false;
            _shown = 0f;
        }

        private bool Wanted => _inside && !Cinematic.Active && !AbilityCard.Showing && Npc.Talking == null;

        private void Update()
        {
            float target = Wanted ? 1f : 0f;
            if (target > 0f && _shown <= 0.01f) _openedAt = Time.unscaledTime;

            _shown = Mathf.MoveTowards(_shown, target, Time.unscaledDeltaTime * 4f);

            if (art != null)
            {
                float glow = _shown * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f));
                art.color = Color.Lerp(Color.white, new Color(1f, 0.95f, 0.75f), glow * 0.6f);
            }
        }

        private void OnGUI()
        {
            if (_shown <= 0.001f) return;

            MoveGuide.Info info = MoveGuide.Get(move);
            float ease = Mathf.SmoothStep(0f, 1f, _shown);

            const float margin = 16f;
            const float pad = 14f;
            float width = Mathf.Min(370f, Screen.width * 0.45f);
            float inner = width - pad * 2f;
            float demoHeight = Mathf.Round(inner * 0.5f);

            GUIStyle howStyle = MoveGuide.Style(15, false, TextAnchor.UpperLeft, true);
            float howHeight = 0f;
            foreach (string line in info.HowTo) howHeight += howStyle.CalcHeight(new GUIContent(line), inner - 14f) + 5f;

            const float keyHeight = 30f;
            float height = pad + 18f + 30f + demoHeight + 12f + keyHeight + 8f + 20f + 10f + howHeight + pad;

            // Desliza da borda direita pra dentro.
            float x = Screen.width + 8f - (width + margin + 8f) * ease;
            var box = new Rect(x, margin, width, height);

            MoveGuide.Alpha = Mathf.Clamp01(_shown * 1.5f);
            MoveGuide.Box(box);

            float y = box.y + pad;
            MoveGuide.Text(new Rect(box.x + pad, y, inner, 18f), "TUTORIAL", MoveGuide.Style(12, true), MoveGuide.GoldDark * 1.6f);

            y += 18f;
            MoveGuide.Text(new Rect(box.x + pad, y, inner, 28f), info.Title, MoveGuide.Style(22, true), MoveGuide.Gold);

            y += 30f;
            float time = Time.unscaledTime - _openedAt;
            MoveGuide.Demo(move, new Rect(box.x + pad, y, inner, demoHeight), time);

            y += demoHeight + 12f;
            MoveGuide.Keys(info.Keys, box.x + pad, y, keyHeight, move, time);

            y += keyHeight + 8f;
            MoveGuide.Text(new Rect(box.x + pad, y, inner, 20f), info.Pad, MoveGuide.Style(13), MoveGuide.Faded);

            y += 30f;
            foreach (string line in info.HowTo)
            {
                float lineHeight = howStyle.CalcHeight(new GUIContent(line), inner - 14f);
                MoveGuide.Fill(new Rect(box.x + pad + 1f, y + 6f, 5f, 5f), MoveGuide.Gold);
                MoveGuide.Text(new Rect(box.x + pad + 14f, y, inner - 14f, lineHeight), line, howStyle, MoveGuide.Paper);
                y += lineHeight + 5f;
            }

            MoveGuide.Alpha = 1f;
        }
    }
}
