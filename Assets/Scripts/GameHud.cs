using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O HUD provisorio: vida, Opaque Shards, gemas, os avisos, as barras dos
    /// chefes e a caixa de dialogo. E desenhado com IMGUI, sem asset nenhum, pra
    /// as mecanicas poderem ser testadas ja — troque por uma interface de verdade
    /// depois.
    ///
    /// Tudo o que e texto solto mora numa caixa do mesmo jeito: borda dourada e
    /// fundo escuro. Os avisos (progresso salvo, habilidade nova, item pego...)
    /// se empilham no alto da tela, pra dois ao mesmo tempo nao se atropelarem;
    /// os pedidos de botao ("E: conversar", "Segure E...") ficam embaixo do meio.
    /// </summary>
    public class GameHud : MonoBehaviour
    {
        private ScrapHealth _health;
        private GUIStyle _label;
        private GUIStyle _line;
        private GUIStyle _speech;
        private bool _checkedForEcho;
        private bool _showNoise;

        // Os avisos deste quadro, na ordem em que vao ser empilhados.
        private readonly List<string> _notices = new List<string>();

        private static readonly Color HeartFull = new Color(0.95f, 0.80f, 0.45f);
        private static readonly Color HeartEmpty = new Color(0.25f, 0.20f, 0.18f);
        private static readonly Color ShardColor = new Color(0.60f, 0.72f, 0.78f);
        private static readonly Color GemColor = new Color(0.55f, 0.85f, 0.95f);
        private static readonly Color BoxFill = new Color(0.07f, 0.06f, 0.08f, 0.96f);
        private static readonly Color PortraitFill = new Color(0.13f, 0.11f, 0.13f, 1f);

        private const float BoxBorder = 3f;
        private const float PromptY = 0.70f;

        private void OnGUI()
        {
            if (_health == null) _health = FindAnyObjectByType<ScrapHealth>();
            EnsureStyles();

            _notices.Clear();

            DrawStatus();
            DrawDialogue();
            DrawDistrict1();
            DrawDistrict2();
            DrawDistrict3();

            if (HudNotice.Showing) _notices.Add(HudNotice.Text);

            DrawCinematic();

            // Por ultimo, pra ficarem por cima de tudo.
            DrawNotices();
        }

        /// <summary>
        /// As faixas pretas de cena, deslizando pra dentro e pra fora, e o
        /// escuro da tela inteira (a cena do despertar comeca no preto).
        /// </summary>
        private static void DrawCinematic()
        {
            float bars = Cinematic.Letterbox;
            if (bars > 0f)
            {
                float height = Screen.height * 0.11f * Mathf.SmoothStep(0f, 1f, bars);
                Fill(new Rect(0f, 0f, Screen.width, height), Color.black);
                Fill(new Rect(0f, Screen.height - height, Screen.width, height), Color.black);
            }

            if (Cinematic.Fade > 0f)
            {
                Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, Mathf.Clamp01(Cinematic.Fade)));
            }
        }

        private void EnsureStyles()
        {
            if (_label != null) return;

            _label = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            _label.normal.textColor = Color.white;

            // Uma linha so, sem quebrar: e o que vai dentro das caixas de aviso.
            _line = new GUIStyle(_label) { wordWrap = false, alignment = TextAnchor.MiddleCenter };

            _speech = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, alignment = TextAnchor.UpperLeft };
            _speech.normal.textColor = new Color(0.93f, 0.90f, 0.82f);
        }

        // ------------------------------------------------------------------
        // Vida, shards e gemas
        // ------------------------------------------------------------------

        private void DrawStatus()
        {
            const float margin = 16f;
            const float size = 18f;

            if (_health != null)
            {
                for (int i = 0; i < _health.MaxHealth; i++)
                {
                    var rect = new Rect(margin + i * (size + 6f), margin, size, size);
                    Fill(new Rect(rect.x - 2f, rect.y - 2f, size + 4f, size + 4f), Color.black);
                    Fill(rect, i < _health.Health ? HeartFull : HeartEmpty);
                }
            }

            Fill(new Rect(margin, margin + 32f, 12f, 16f), ShardColor);
            GUI.Label(new Rect(margin + 20f, margin + 27f, 200f, 28f), ScrapWallet.Shards.ToString(), _label);

            DrawGems(margin, margin + 58f);
        }

        /// <summary>
        /// As gemas lapidadas: um losango e a conta. So aparece depois da
        /// primeira, e brilha por um instante quando chega uma nova.
        /// </summary>
        private void DrawGems(float x, float y)
        {
            if (GemPurse.Gems <= 0) return;

            float glow = Mathf.Clamp01(1f - (Time.time - GemPurse.LastGainTime) / 1.5f);
            Color gem = Color.Lerp(GemColor, Color.white, glow * 0.7f);

            // Um losango de faixas: largo no meio, fino nas pontas.
            int[] widths = { 4, 8, 12, 8, 4 };
            for (int i = 0; i < widths.Length; i++)
            {
                Fill(new Rect(x + 6f - widths[i] * 0.5f, y + i * 3f, widths[i], 3f), gem);
            }

            GUI.Label(new Rect(x + 20f, y - 4f, 200f, 28f), GemPurse.Gems.ToString(), _label);
        }

        // ------------------------------------------------------------------
        // Os distritos
        // ------------------------------------------------------------------

        /// <summary>O District 1: a alavanca, o Rusty Scrapper e os avisos da bancada e da purificacao.</summary>
        private void DrawDistrict1()
        {
            RustyLever lever = RustyLever.Nearby;
            if (lever != null) DrawPrompt("Segure E para raspar a ferrugem", lever.Progress);

            RustyScrapper boss = RustyScrapper.Fighting;
            if (boss != null) BossBar("O Coletor Enferrujado", boss.HealthFraction, new Color(0.45f, 0.75f, 0.55f));

            if (Time.time - Workbench.LastRestTime < 2.5f) _notices.Add("Bancada: progresso salvo");
            if (Time.time - AreaPurifier.PurifiedTime < 5f) _notices.Add("Área purificada");

            if (Time.time - ScrapDash.UnlockTime < 5f)
            {
                _notices.Add("Sobrecarga de Velocidade conquistada: Shift esquerdo");
            }

            if (Time.time - RewardPickup.ChassisTime < 5f) _notices.Add("Reforço de chassi encontrado: +1 de vida");
        }

        /// <summary>O District 2: valvulas, o Boiler Master, a loja de Crisol e os avisos.</summary>
        private void DrawDistrict2()
        {
            Valve valve = Valve.Nearby;
            if (valve != null) DrawPrompt("Segure E para raspar e girar a válvula", valve.Progress);

            BoilerMaster master = BoilerMaster.Fighting;
            if (master != null)
            {
                BossBar(master.IsExposed ? "O Mestre das Caldeiras — FORNALHA EXPOSTA" : "O Mestre das Caldeiras",
                    master.HealthFraction,
                    master.IsExposed ? new Color(1f, 0.55f, 0.25f) : new Color(0.55f, 0.6f, 0.7f));
            }

            Crisol shop = Crisol.ShopOpen;
            if (shop != null) DrawShop(shop);

            if (Time.time - ScrapGems.RubyGivenTime < 6f) _notices.Add("Rubi bruto recebido: crave-o numa Bancada de Ourives");
            if (Time.time - ScrapGems.RubySocketedTime < 6f) _notices.Add("Rubi cravado: o cinzel bate mais forte");
            if (Time.time - ScrapController.PressureJumpTime < 6f) _notices.Add("Salto de Pressão conquistado: pule de novo no ar");
            if (Time.time - RisingAcidTrigger.StartTime < 5f) _notices.Add("O ácido está subindo. Suba!");
        }

        /// <summary>O District 3 (e o 4): raspar e polir, os chefes, as gemas e o medidor de ruido.</summary>
        private void DrawDistrict3()
        {
            if (HoldAction.Owner != null) DrawPrompt(HoldAction.Text, Mathf.Clamp01(HoldAction.Progress));

            TheResonant elite = TheResonant.Fighting;
            if (elite != null) BossBar("O Ressonante", elite.HealthFraction, new Color(0.60f, 0.80f, 1f));

            GeodeDrill drill = GeodeDrill.Fighting;
            if (drill != null)
            {
                BossBar(drill.IsExposed ? "A Broca dos Geodos — MOTOR EXPOSTO" : "A Broca dos Geodos",
                    drill.HealthFraction,
                    drill.IsExposed ? new Color(1f, 0.55f, 0.25f) : new Color(0.62f, 0.55f, 0.80f));
            }

            if (Time.time - ScrapGems.SapphireGivenTime < 6f) _notices.Add("Safira bruta recebida: crave-a numa Bancada de Ourives");
            if (Time.time - ScrapGems.SapphireSocketedTime < 6f) _notices.Add("Safira cravada: passos e quedas fazem menos ruído");
            if (Time.time - ScrapChisel.ResonanceTime < 8f) _notices.Add("Golpe Ressonante conquistado: segure o golpe e solte");
            if (Time.time < ScrapController.SlowedUntil) _notices.Add("Mercúrio grudado no corpo: você está lento");

            // O medidor de ruido: sobe na hora do som e desce em um segundo.
            // So aparece em cena que tenha cristal de eco, pra nao poluir as outras.
            if (!_checkedForEcho)
            {
                _checkedForEcho = true;
                _showNoise = FindAnyObjectByType<EchoCrystal>() != null;
            }

            if (!_showNoise) return;

            float age = Time.time - NoiseSystem.LastTime;
            float level = NoiseSystem.LastLoudness * Mathf.Clamp01(1f - age);

            const float barWidth = 120f;
            float barX = 16f;
            // Abaixo do contador de gemas, quando ele aparece.
            float barY = GemPurse.Gems > 0 ? 112f : 86f;

            GUI.Label(new Rect(barX, barY - 24f, 200f, 26f), "Ruído", _label);
            Fill(new Rect(barX - 2f, barY - 2f, barWidth + 4f, 14f), Color.black);
            Fill(new Rect(barX, barY, barWidth, 10f), HeartEmpty);
            Fill(new Rect(barX, barY, barWidth * level, 10f),
                level > 0.3f ? new Color(0.95f, 0.45f, 0.75f) : new Color(0.7f, 0.6f, 0.95f));
        }

        /// <summary>
        /// A loja do Crisol: o que se tem nas duas moedas, a lista (o item
        /// marcado em dourado, o que nao da pra pagar apagado), a descricao do
        /// marcado, o que ele respondeu e as teclas.
        /// </summary>
        private void DrawShop(Crisol shop)
        {
            const float rowHeight = 32f;
            float width = Mathf.Min(Screen.width - 60f, 680f);
            float height = 120f + shop.Items.Count * rowHeight + 96f;
            float x = (Screen.width - width) * 0.5f;
            float y = (Screen.height - height) * 0.5f;

            DrawBox(new Rect(x, y, width, height));

            Color previous = GUI.contentColor;

            GUI.contentColor = HeartFull;
            GUI.Label(new Rect(x + 18f, y + 12f, width, 28f), "Loja de Crisol", _label);
            GUI.contentColor = previous;

            string funds = $"{ScrapWallet.Shards} {Shop.CurrencyName(Currency.Shards, ScrapWallet.Shards)}   ·   " +
                           $"{GemPurse.Gems} {Shop.CurrencyName(Currency.Gems, GemPurse.Gems)}";
            GUI.Label(new Rect(x + 18f, y + 44f, width - 36f, 28f), funds, _label);

            float listY = y + 86f;
            Fill(new Rect(x + 12f, listY - 6f, width - 24f, 2f), HeartEmpty);

            for (int i = 0; i < shop.Items.Count; i++)
            {
                ShopItem item = shop.Items[i];
                bool selected = i == shop.Selected;
                bool soldOut = Shop.SoldOut(item);
                int price = Shop.Price(item);
                bool affordable = Shop.Funds(item.Currency) >= price;

                var row = new Rect(x + 12f, listY + i * rowHeight, width - 24f, rowHeight - 2f);
                if (selected) Fill(row, new Color(0.25f, 0.20f, 0.12f, 1f));

                string cost = soldOut
                    ? (item.MaxCount == 1 ? "comprado" : "esgotado")
                    : $"{price} {Shop.CurrencyName(item.Currency, price)}";

                GUI.contentColor = selected ? HeartFull : (soldOut || !affordable ? new Color(0.6f, 0.58f, 0.55f) : Color.white);
                GUI.Label(new Rect(row.x + 8f, row.y + 2f, row.width * 0.65f, 28f), $"{i + 1}.  {item.Name}", _label);

                Vector2 costSize = _label.CalcSize(new GUIContent(cost));
                GUI.Label(new Rect(row.xMax - costSize.x - 10f, row.y + 2f, costSize.x + 4f, 28f), cost, _label);
            }

            GUI.contentColor = previous;

            float footY = listY + shop.Items.Count * rowHeight + 6f;
            Fill(new Rect(x + 12f, footY - 4f, width - 24f, 2f), HeartEmpty);

            if (shop.Items.Count > 0)
            {
                GUI.Label(new Rect(x + 18f, footY + 2f, width - 36f, 28f), shop.Items[shop.Selected].Description, _speech);
            }

            GUI.contentColor = HeartFull;
            GUI.Label(new Rect(x + 18f, footY + 32f, width - 36f, 28f), shop.LastMessage, _label);
            GUI.contentColor = previous;

            GUI.Label(new Rect(x + 18f, y + height - 32f, width - 36f, 28f),
                "W/S: escolher   ·   Espaço: comprar   ·   E: sair", _label);
        }

        private void BossBar(string title, float fraction, Color color)
        {
            float width = Screen.width * 0.5f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - 46f;

            GUI.Label(new Rect(x, y - 26f, 520f, 28f), title, _label);
            Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
            Fill(new Rect(x, y, width, 12f), HeartEmpty);
            Fill(new Rect(x, y, width * fraction, 12f), color);
        }

        // ------------------------------------------------------------------
        // Caixas
        // ------------------------------------------------------------------

        /// <summary>A caixa de sempre: borda dourada por fora, fundo escuro por dentro.</summary>
        private static void DrawBox(Rect inner)
        {
            Fill(new Rect(inner.x - BoxBorder, inner.y - BoxBorder,
                inner.width + BoxBorder * 2f, inner.height + BoxBorder * 2f), HeartFull);
            Fill(inner, BoxFill);
        }

        /// <summary>Os avisos do quadro, um embaixo do outro, cada um na sua caixa, a partir do alto.</summary>
        private void DrawNotices()
        {
            float y = Screen.height * 0.12f;

            foreach (string text in _notices)
            {
                Vector2 size = _line.CalcSize(new GUIContent(text));
                float width = Mathf.Min(size.x + 40f, Screen.width - 40f);
                const float height = 40f;

                var box = new Rect((Screen.width - width) * 0.5f, y, width, height);
                DrawBox(box);
                GUI.Label(box, text, _line);

                y += height + BoxBorder * 2f + 8f;
            }
        }

        /// <summary>
        /// Um pedido de botao, numa caixa embaixo do meio da tela. Com
        /// progresso (segurar pra raspar), ganha a barra embaixo do texto.
        /// </summary>
        private void DrawPrompt(string text, float progress = -1f)
        {
            // Numa cena o jogador nao tem o controle: nao ha botao a pedir.
            if (Cinematic.Active) return;

            Vector2 size = _line.CalcSize(new GUIContent(text));
            bool bar = progress >= 0f;

            float width = Mathf.Max(size.x + 40f, bar ? 280f : 0f);
            float height = bar ? 58f : 40f;
            var box = new Rect((Screen.width - width) * 0.5f, Screen.height * PromptY, width, height);

            DrawBox(box);
            GUI.Label(new Rect(box.x, box.y, box.width, 40f), text, _line);

            if (!bar) return;

            var track = new Rect(box.x + 20f, box.y + 38f, box.width - 40f, 10f);
            Fill(track, HeartEmpty);
            Fill(new Rect(track.x, track.y, track.width * Mathf.Clamp01(progress), track.height), HeartFull);
        }

        // ------------------------------------------------------------------
        // Dialogo
        // ------------------------------------------------------------------

        /// <summary>
        /// A caixa de fala de um NPC, com o retrato de quem fala a esquerda, ou
        /// o pedido de botao de que da pra conversar.
        /// </summary>
        private void DrawDialogue()
        {
            Npc talking = Npc.Talking;

            if (talking == null)
            {
                Npc nearby = Npc.Nearby;
                if (nearby != null) DrawPrompt("E: " + nearby.Prompt + " " + nearby.DisplayName);
                return;
            }

            string line = talking.CurrentLine;
            if (line == null) return;

            float width = Mathf.Min(Screen.width - 80f, 900f);
            const float height = 140f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 70f;

            DrawBox(new Rect(x, y, width, height));

            // O texto comeca depois do retrato, quando ha retrato.
            float textX = x + 18f;
            if (DrawPortrait(talking.Portrait, new Rect(x + 12f, y + 12f, height - 24f, height - 24f)))
            {
                textX = x + height + 6f;
            }

            float textWidth = x + width - 18f - textX;

            Color previous = GUI.contentColor;
            GUI.contentColor = HeartFull;
            GUI.Label(new Rect(textX, y + 10f, textWidth, 28f), talking.DisplayName, _label);
            GUI.contentColor = previous;

            GUI.Label(new Rect(textX, y + 42f, textWidth, height - 72f), line, _speech);
            GUI.Label(new Rect(x + width - 150f, y + height - 32f, 140f, 26f), "E: continuar", _label);
        }

        /// <summary>
        /// O retrato: o sprite de quem fala, olhando pra direita (pro texto),
        /// num quadro escuro. Aumenta em passos inteiros pra o pixel art nao
        /// borrar, e fica de pe no fundo do quadro. Devolve falso se nao ha o
        /// que desenhar.
        /// </summary>
        private static bool DrawPortrait(SpriteRenderer portrait, Rect frame)
        {
            Sprite sprite = portrait != null ? portrait.sprite : null;
            if (sprite == null || sprite.texture == null) return false;

            Fill(new Rect(frame.x - 2f, frame.y - 2f, frame.width + 4f, frame.height + 4f), HeartEmpty);
            Fill(frame, PortraitFill);

            Rect pixels = sprite.rect;
            float room = frame.width - 12f;
            float fit = Mathf.Min(room / pixels.width, room / pixels.height);
            float scale = fit >= 1f ? Mathf.Floor(fit) : fit;

            float drawWidth = pixels.width * scale;
            float drawHeight = pixels.height * scale;
            var target = new Rect(
                frame.x + (frame.width - drawWidth) * 0.5f,
                frame.y + frame.height - 6f - drawHeight,
                drawWidth, drawHeight);

            Texture texture = sprite.texture;
            var uv = new Rect(
                pixels.x / texture.width, pixels.y / texture.height,
                pixels.width / texture.width, pixels.height / texture.height);

            Color previous = GUI.color;
            GUI.color = portrait.color;
            GUI.DrawTextureWithTexCoords(target, texture, uv);
            GUI.color = previous;

            return true;
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
