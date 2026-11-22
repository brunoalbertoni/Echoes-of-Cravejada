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
        [UnityEngine.Serialization.FormerlySerializedAs("move")] [SerializeField] private ScrapMove mover = ScrapMove.PuloParede;

        [UnityEngine.Serialization.FormerlySerializedAs("art")] [Tooltip("O desenho da placa: pisca de leve enquanto o painel esta aberto.")]
        [SerializeField] private SpriteRenderer arte;

        private bool _dentro;
        private float _mostrado;
        private float _abertosEm;
        private bool _aberto;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(ScrapMove novoMover, SpriteRenderer novoArte)
        {
            mover = novoMover;
            arte = novoArte;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            // A mini cena recomeca a cada chegada, se o painel ja tinha fechado.
            if (_mostrado <= 0.01f) _abertosEm = Time.unscaledTime;
            _dentro = true;
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (outro.GetComponentInParent<ScrapController>() == null) return;
            _dentro = false;
        }

        private void OnDisable()
        {
            _dentro = false;
            _mostrado = 0f;
        }

        private bool Desejado => _dentro && !Cinematic.Ativo && !AbilityCard.Mostrando && Npc.Conversando == null;

        private void Update()
        {
            float alvo = Desejado ? 1f : 0f;
            if (alvo > 0f && _mostrado <= 0.01f) _abertosEm = Time.unscaledTime;

            // O guia de movimentos abrindo e fechando.
            bool aberto = alvo > 0f;
            if (aberto != _aberto)
            {
                _aberto = aberto;
                (aberto ? Sfx.AbertoPlaca : Sfx.FecharPlaca).Tocar();
            }

            _mostrado = Mathf.MoveTowards(_mostrado, alvo, Time.unscaledDeltaTime * 4f);

            if (arte != null)
            {
                float brilho = _mostrado * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f));
                arte.color = Color.Lerp(Color.white, new Color(1f, 0.95f, 0.75f), brilho * 0.6f);
            }
        }

        private void OnGUI()
        {
            if (_mostrado <= 0.001f) return;

            MoveGuide.Info info = MoveGuide.Obter(mover);
            float suavidade = Mathf.SmoothStep(0f, 1f, _mostrado);

            const float margem = 16f;
            const float controle = 14f;
            float largura = Mathf.Min(370f, Screen.width * 0.45f);
            float interno = largura - controle * 2f;
            float alturaDemo = Mathf.Round(interno * 0.5f);

            GUIStyle estiloComo = MoveGuide.Estilo(15, false, TextAnchor.UpperLeft, true);
            float alturaComo = 0f;
            foreach (string linha2 in info.ComoPara) alturaComo += estiloComo.CalcHeight(new GUIContent(linha2), interno - 14f) + 5f;

            const float alturaChave = 30f;
            float altura = controle + 18f + 30f + alturaDemo + 12f + alturaChave + 8f + 20f + 10f + alturaComo + controle;

            // Desliza da borda direita pra dentro.
            float x = Screen.width + 8f - (largura + margem + 8f) * suavidade;
            var caixa = new Rect(x, margem, largura, altura);

            MoveGuide.Alfa = Mathf.Clamp01(_mostrado * 1.5f);
            MoveGuide.Caixa(caixa);

            float y = caixa.y + controle;
            MoveGuide.Texto(new Rect(caixa.x + controle, y, interno, 18f), "TUTORIAL", MoveGuide.Estilo(12, true), MoveGuide.EscuroOuro * 1.6f);

            y += 18f;
            MoveGuide.Texto(new Rect(caixa.x + controle, y, interno, 28f), info.Titulo, MoveGuide.Estilo(22, true), MoveGuide.Ouro);

            y += 30f;
            float tempo = Time.unscaledTime - _abertosEm;
            MoveGuide.Demonstrar(mover, new Rect(caixa.x + controle, y, interno, alturaDemo), tempo);

            y += alturaDemo + 12f;
            MoveGuide.Teclas(info.Chaves, caixa.x + controle, y, alturaChave, mover, tempo);

            y += alturaChave + 8f;
            MoveGuide.Texto(new Rect(caixa.x + controle, y, interno, 20f), info.Controle, MoveGuide.Estilo(13), MoveGuide.Esmaecido);

            y += 30f;
            foreach (string linha2 in info.ComoPara)
            {
                float alturaLinha2 = estiloComo.CalcHeight(new GUIContent(linha2), interno - 14f);
                MoveGuide.Preencher(new Rect(caixa.x + controle + 1f, y + 6f, 5f, 5f), MoveGuide.Ouro);
                MoveGuide.Texto(new Rect(caixa.x + controle + 14f, y, interno - 14f, alturaLinha2), linha2, estiloComo, MoveGuide.Papel);
                y += alturaLinha2 + 5f;
            }

            MoveGuide.Alfa = 1f;
        }
    }
}
