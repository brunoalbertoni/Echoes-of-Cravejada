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

        // Os avisos deste quadro, na ordem em que vao ser empilhados.
        private readonly List<string> _notices = new List<string>();

        private static readonly Color HeartFull = new Color(0.95f, 0.80f, 0.45f);
        private static readonly Color HeartEmpty = new Color(0.25f, 0.20f, 0.18f);
        private static readonly Color ShardColor = new Color(0.60f, 0.72f, 0.78f);
        private static readonly Color GemColor = new Color(0.55f, 0.85f, 0.95f);
        private static readonly Color BoxFill = new Color(0.07f, 0.06f, 0.08f, 0.96f);

        private const float BoxBorder = 3f;
        private const float PromptY = 0.70f;

        private void OnGUI()
        {
            // A bancada esta fotografando a tela pra gravura: o HUD sai da foto.
            if (SaveEngraving.HideHud) return;

            if (_health == null) _health = FindAnyObjectByType<ScrapHealth>();
            EnsureStyles();

            _notices.Clear();

            // As faixas de cena vao primeiro: a caixa de fala fica por cima delas.
            DrawLetterbox();

            DrawStatus();
            DrawDialogue();
            DrawDistrict1();
            DrawDistrict2();
            DrawDistrict3();

            if (HudNotice.Showing) _notices.Add(HudNotice.Text);

            DrawScreenCover();

            // Por ultimo, pra ficarem por cima de tudo.
            DrawNotices();
        }

        /// <summary>As faixas pretas de cena, deslizando pra dentro e pra fora.</summary>
        private static void DrawLetterbox()
        {
            float bars = Cinematic.Letterbox;
            if (bars <= 0f) return;

            float height = Screen.height * 0.11f * Mathf.SmoothStep(0f, 1f, bars);
            Fill(new Rect(0f, 0f, Screen.width, height), Color.black);
            Fill(new Rect(0f, Screen.height - height, Screen.width, height), Color.black);
        }

        /// <summary>
        /// O clarao e o escuro da tela inteira (a cena do despertar comeca no
        /// preto). Ficam por cima do HUD.
        /// </summary>
        private static void DrawScreenCover()
        {
            if (Cinematic.Flash > 0f)
            {
                Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(1f, 0.97f, 0.88f, Mathf.Clamp01(Cinematic.Flash)));
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

        // Os icones (Resources/HudIcons, desenhados pelo HudIconArt). Em pixel art, sempre em dobro.
        private const float IconScale = 2f;
        private static Texture2D _heartFullIcon;
        private static Texture2D _heartEmptyIcon;
        private static Texture2D _shardIcon;
        private static Texture2D _gemIcon;
        private static bool _iconsLoaded;

        private static void LoadIcons()
        {
            if (_iconsLoaded) return;
            _iconsLoaded = true;

            _heartFullIcon = Resources.Load<Texture2D>("HudIcons/heart_full");
            _heartEmptyIcon = Resources.Load<Texture2D>("HudIcons/heart_empty");
            _shardIcon = Resources.Load<Texture2D>("HudIcons/shard");
            _gemIcon = Resources.Load<Texture2D>("HudIcons/gem");
        }

        /// <summary>Um icone em dobro com o canto em (x, y). Devolve a largura dele na tela.</summary>
        private static float Icon(Texture2D icon, float x, float y, Color tint)
        {
            var rect = new Rect(x, y, icon.width * IconScale, icon.height * IconScale);
            Color previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(rect, icon);
            GUI.color = previous;
            return rect.width;
        }

        /// <summary>
        /// A vida (coracoes de sucata: cheios com a brasa acesa, gastos
        /// apagados), os fragmentos (a pepita azul) e as gemas (o rubi).
        /// </summary>
        private void DrawStatus()
        {
            const float margin = 16f;
            LoadIcons();

            float y = margin;

            if (_health != null)
            {
                if (_heartFullIcon != null && _heartEmptyIcon != null)
                {
                    float step = _heartFullIcon.width * IconScale + 2f;
                    for (int i = 0; i < _health.MaxHealth; i++)
                    {
                        bool full = i < _health.Health;

                        // O ultimo coracao cheio bate quando a vida esta por um fio.
                        float beat = full && _health.Health == 1 ? Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 2f : 0f;
                        Icon(full ? _heartFullIcon : _heartEmptyIcon, margin + i * step, y - beat, Color.white);
                    }

                    y += _heartFullIcon.height * IconScale + 6f;
                }
                else
                {
                    const float size = 18f;
                    for (int i = 0; i < _health.MaxHealth; i++)
                    {
                        var rect = new Rect(margin + i * (size + 6f), y, size, size);
                        Fill(new Rect(rect.x - 2f, rect.y - 2f, size + 4f, size + 4f), Color.black);
                        Fill(rect, i < _health.Health ? HeartFull : HeartEmpty);
                    }

                    y += 32f;
                }
            }

            if (_shardIcon != null)
            {
                float width = Icon(_shardIcon, margin + 4f, y, Color.white);
                GUI.Label(new Rect(margin + 4f + width + 6f, y - 1f, 200f, 28f), ScrapWallet.Shards.ToString(), _label);
                y += _shardIcon.height * IconScale + 4f;
            }
            else
            {
                Fill(new Rect(margin, y, 12f, 16f), ShardColor);
                GUI.Label(new Rect(margin + 20f, y - 5f, 200f, 28f), ScrapWallet.Shards.ToString(), _label);
                y += 26f;
            }

            DrawGems(margin + 4f, y);
            if (GemPurse.Gems > 0) y += (_gemIcon != null ? _gemIcon.height * IconScale : 16f) + 8f;

            DrawInventory(margin, y + 4f);
        }

        private static readonly Color SlotFill = new Color(0.06f, 0.05f, 0.07f, 0.55f);
        private static readonly Color SlotRim = new Color(0.78f, 0.6f, 0.3f, 0.55f);

        /// <summary>
        /// O inventario: uma fileira de casinhas com o icone de cada coisa que
        /// o Scrap carrega pra usar ou entregar.
        /// </summary>
        private void DrawInventory(float x, float y)
        {
            var items = InventoryIcons.Current();
            if (items.Count == 0) return;

            const float slot = 38f;
            int hovered = -1;
            Vector2 mouse = Event.current.mousePosition;
            for (int i = 0; i < items.Count; i++)
            {
                var rect = new Rect(x + i * (slot + 4f), y, slot, slot);
                Fill(rect, SlotRim);
                Fill(new Rect(rect.x + 1f, rect.y + 1f, slot - 2f, slot - 2f), SlotFill);

                Texture2D icon = items[i].Icon;
                float w = icon.width * IconScale, h = icon.height * IconScale;
                GUI.DrawTexture(new Rect(rect.x + (slot - w) * 0.5f, rect.y + (slot - h) * 0.5f, w, h), icon);

                if (items[i].Count > 1)
                    GUI.Label(new Rect(rect.xMax - 16f, rect.yMax - 20f, 20f, 20f), items[i].Count.ToString(), _label);

                if (rect.Contains(mouse)) hovered = i;
            }

            if (hovered >= 0)
                DrawItemTip(items[hovered], new Rect(x + hovered * (slot + 4f), y, slot, slot));
        }

        private GUIStyle _tipName;
        private GUIStyle _tipUse;
        private static readonly Color TipFill = new Color(0.07f, 0.055f, 0.06f, 0.94f);
        private static readonly Color TipRim = new Color(0.78f, 0.6f, 0.3f, 0.9f);

        /// <summary>A plaquinha embaixo da casinha: o nome do item e o que fazer com ele.</summary>
        private void DrawItemTip(InventoryIcons.Entry item, Rect slotRect)
        {
            if (string.IsNullOrEmpty(item.Name)) return;

            if (_tipName == null)
            {
                _tipName = new GUIStyle(_label) { fontSize = 16, wordWrap = false };
                _tipName.normal.textColor = new Color(1f, 0.88f, 0.6f);
                _tipUse = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = false };
                _tipUse.normal.textColor = new Color(0.85f, 0.82f, 0.76f);
            }

            Vector2 nameSize = _tipName.CalcSize(new GUIContent(item.Name));
            Vector2 useSize = string.IsNullOrEmpty(item.Use) ? Vector2.zero : _tipUse.CalcSize(new GUIContent(item.Use));
            float w = Mathf.Round(Mathf.Max(nameSize.x, useSize.x) + 16f);
            float h = Mathf.Round(nameSize.y + useSize.y + 8f);
            var box = new Rect(slotRect.x, slotRect.yMax + 6f, w, h);

            // Moldura de dois tons, cantos cortados, como as casinhas.
            Fill(new Rect(box.x + 2f, box.y, w - 4f, h), TipRim);
            Fill(new Rect(box.x, box.y + 2f, w, h - 4f), TipRim);
            Fill(new Rect(box.x + 2f, box.y + 2f, w - 4f, h - 4f), TipFill);
            // A setinha apontando pra casinha.
            Fill(new Rect(slotRect.center.x - 3f, box.y - 4f, 6f, 4f), TipRim);
            Fill(new Rect(slotRect.center.x - 1f, box.y - 6f, 2f, 2f), TipRim);

            GUI.Label(new Rect(box.x + 8f, box.y + 3f, w, nameSize.y), item.Name, _tipName);
            if (useSize.y > 0f)
                GUI.Label(new Rect(box.x + 8f, box.y + 3f + nameSize.y, w, useSize.y), item.Use, _tipUse);
        }

        /// <summary>
        /// As gemas lapidadas: o rubi e a conta. So aparece depois da
        /// primeira, e brilha por um instante quando chega uma nova.
        /// </summary>
        private void DrawGems(float x, float y)
        {
            if (GemPurse.Gems <= 0) return;

            float glow = Mathf.Clamp01(1f - (Time.time - GemPurse.LastGainTime) / 1.5f);

            if (_gemIcon != null)
            {
                // Ao chegar uma nova, ele clareia e da um pulinho.
                float hop = glow * 3f;
                float width = Icon(_gemIcon, x, y - hop, Color.Lerp(Color.white, new Color(1f, 0.9f, 0.9f), glow));
                GUI.Label(new Rect(x + width + 6f, y - 1f, 200f, 28f), GemPurse.Gems.ToString(), _label);
                return;
            }

            Color gem = Color.Lerp(GemColor, Color.white, glow * 0.7f);
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
            if (lever != null) DrawPrompt("Golpeie a alavanca com o cinzel");

            RustyScrapper boss = RustyScrapper.Fighting;
            if (boss != null) BossBar("O Coletor Enferrujado", boss.HealthFraction, new Color(0.45f, 0.75f, 0.55f));

            // A bancada nao avisa com frase: quem conta que salvou e a gravura (SaveEngraving).
            Workbench bench = Workbench.Nearby;
            if (bench != null && bench.CanSave) DrawPrompt(Workbench.Current == bench ? "E: salvar de novo" : "E: salvar na Bancada de Ourives");

            // As habilidades novas (dash, pulo duplo, golpe ressonante) nao
            // viram aviso aqui: quem mostra e o AbilityCard, na tela inteira.

        }

        /// <summary>O District 2: valvulas, o Boiler Master, a loja de Crisol e os avisos.</summary>
        private void DrawDistrict2()
        {
            Valve valve = Valve.Nearby;
            if (valve != null) DrawPrompt("Golpeie a válvula com o cinzel");

            BoilerMaster master = BoilerMaster.Fighting;
            if (master != null)
            {
                BossBar(master.IsExposed ? "O Mestre das Caldeiras — FORNALHA EXPOSTA" : "O Mestre das Caldeiras",
                    master.HealthFraction,
                    master.IsExposed ? new Color(1f, 0.55f, 0.25f) : new Color(0.55f, 0.6f, 0.7f));
            }

            Crisol shop = Crisol.ShopOpen;
            if (shop != null) DrawShop(shop);

        }

        /// <summary>O District 3 (e o 4): raspar e polir, os chefes, as gemas e as estatuas.</summary>
        private void DrawDistrict3()
        {
            if (HoldAction.Owner != null) DrawPrompt(HoldAction.Text, Mathf.Clamp01(HoldAction.Progress));

            TheResonant elite = TheResonant.Fighting;
            if (elite != null)
            {
                BossBar(elite.IsExposed ? "O Capataz — PRESO" : "O Capataz", elite.HealthFraction, new Color(0.60f, 0.80f, 1f));
            }

            SilverKing king = SilverKing.Fighting;
            if (king != null && !king.IsTransforming)
            {
                BossBar(king.IsPhaseTwo ? "O Rei de Prata — CORROMPIDO" : "O Rei de Prata",
                    king.HealthFraction,
                    king.IsPhaseTwo ? new Color(0.45f, 1f, 0.62f) : new Color(1f, 0.95f, 0.78f));
            }

            MercuryAlchemist alchemist = MercuryAlchemist.Fighting;
            if (alchemist != null)
            {
                BossBar(alchemist.IsSilver ? "O Alquimista de Mercúrio — EM PRATA" : "O Alquimista de Mercúrio",
                    alchemist.HealthFraction,
                    alchemist.IsSilver ? new Color(0.92f, 0.95f, 1f) : new Color(0.60f, 0.66f, 0.80f));
            }

            GeodeDrill drill = GeodeDrill.Fighting;
            if (drill != null)
            {
                BossBar(drill.IsExposed ? "A Broca dos Geodos — MOTOR EXPOSTO" : "A Broca dos Geodos",
                    drill.HealthFraction,
                    drill.IsExposed ? new Color(1f, 0.55f, 0.25f) : new Color(0.62f, 0.55f, 0.80f));
            }

            if (Time.time - ScrapGems.SapphireGivenTime < 6f) _notices.Add("Safira bruta recebida: crave-a numa Bancada de Ourives");
            if (Time.time - ScrapGems.SapphireSocketedTime < 6f) _notices.Add("Safira cravada: passos e quedas fazem menos ruído");

            // Nao ha medidor de ruido: o barulho se sente pelo que ele acende
            // e pelo que ele acorda.
            StatueButton statue = StatueButton.Nearby;
            if (statue != null && statue.Enabled && HoldAction.Owner == null && Npc.Talking == null && Npc.Nearby == null)
            {
                DrawPrompt("E: apertar o botão da estátua");
            }
        }

        /// <summary>
        /// A loja do Crisol: o que se tem nas duas moedas, a lista (o item
        /// marcado em dourado, o que nao da pra pagar apagado), a descricao do
        /// marcado, o que ele respondeu e as teclas.
        /// </summary>
        private void DrawShop(Crisol shop)
        {
            EnsureShopStyles();

            const float rowHeight = 46f;
            const float headerH = 84f;
            const float footerH = 46f;
            float width = Mathf.Min(Screen.width - 40f, 860f);
            float listH = Mathf.Max(shop.Items.Count * rowHeight + 12f, 250f);
            float height = Mathf.Min(headerH + listH + footerH + 20f, Screen.height - 40f);
            float x = Mathf.Round((Screen.width - width) * 0.5f);
            float y = Mathf.Round((Screen.height - height) * 0.5f);

            // O fundo escurece o jogo atras, pra loja ser o centro.
            Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.55f));

            var frame = new Rect(x, y, width, height);
            Panel(frame, ShopWood, ShopBrass, ShopBrassDark);
            Rivets(frame);

            Color previous = GUI.contentColor;

            // -- Cabecalho: o retrato do Crisol, o nome da loja, a carteira.
            var header = new Rect(x + 14f, y + 14f, width - 28f, headerH - 14f);
            Fill(new Rect(header.x, header.yMax - 2f, header.width, 2f), ShopBrassDark);

            float textX = header.x + 6f;
            Texture2D bust = SpeakerPortraits.Get(SpeakerPortraits.IdFor(shop.DisplayName), false);
            if (bust != null)
            {
                float size = header.height - 12f;
                var portrait = new Rect(header.x + 2f, header.y + 2f, size, size);
                Panel(portrait, ShopInk, ShopBrass, ShopBrassDark);
                GUI.DrawTexture(new Rect(portrait.x + 3f, portrait.y + 3f, size - 6f, size - 6f), bust, ScaleMode.ScaleToFit);
                textX = portrait.xMax + 14f;
            }

            GUI.Label(new Rect(textX, header.y + 4f, 400f, 30f), "Oficina do Crisol", _shopTitle);
            GUI.Label(new Rect(textX, header.y + 34f, 400f, 22f), "Ourives · peças e lembranças", _shopSub);

            float walletX = header.xMax - 6f;
            walletX = Coin(walletX, header.y + 18f, _gemIcon, GemPurse.Gems, ShopGemTint);
            Coin(walletX - 18f, header.y + 18f, _shardIcon, ScrapWallet.Shards, ShopShardTint);

            // -- A lista a esquerda, o detalhe a direita.
            float bodyY = header.yMax + 10f;
            float listW = Mathf.Round(width * 0.52f);
            var list = new Rect(x + 14f, bodyY, listW, height - (bodyY - y) - footerH - 6f);
            var detail = new Rect(list.xMax + 10f, bodyY, width - listW - 38f, list.height);

            Fill(list, ShopInk);
            for (int i = 0; i < shop.Items.Count; i++)
            {
                ShopItem item = shop.Items[i];
                bool selected = i == shop.Selected;
                bool soldOut = Shop.SoldOut(item);
                int price = Shop.Price(item);
                bool affordable = Shop.Funds(item.Currency) >= price;
                bool dim = soldOut || !affordable;

                var row = new Rect(list.x + 6f, list.y + 6f + i * rowHeight, list.width - 12f, rowHeight - 6f);

                // O mouse: passar por cima marca a fileira, clicar compra.
                if (ShopMouse(row, out bool clicked))
                {
                    if (clicked) shop.ClickBuy(i);
                    else shop.Hover(i);
                    selected = i == shop.Selected;
                }

                if (selected)
                {
                    Fill(row, ShopBrass);
                    Fill(new Rect(row.x + 2f, row.y + 2f, row.width - 4f, row.height - 4f), ShopSelected);
                    // A setinha de escolha, pulsando de leve.
                    float nudge = Mathf.Round(Mathf.PingPong(Time.unscaledTime * 6f, 3f));
                    Fill(new Rect(row.x - 2f + nudge, row.center.y - 4f, 4f, 8f), ShopBrassLight);
                    Fill(new Rect(row.x + 2f + nudge, row.center.y - 2f, 2f, 4f), ShopBrassLight);
                }
                else if (i % 2 == 1)
                {
                    Fill(row, new Color(1f, 1f, 1f, 0.025f));
                }

                // A casinha do icone.
                var slot = new Rect(row.x + 12f, row.y + 3f, row.height - 6f, row.height - 6f);
                Fill(slot, selected ? ShopBrass : ShopBrassDark);
                Fill(new Rect(slot.x + 2f, slot.y + 2f, slot.width - 4f, slot.height - 4f), ShopWood);
                Texture2D icon = ShopIcons.Get(item.Id);
                if (icon != null)
                {
                    GUI.color = dim && !selected ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
                    GUI.DrawTexture(new Rect(slot.center.x - 12f, slot.center.y - 12f, 24f, 24f), icon);
                    GUI.color = Color.white;
                }

                _shopItem.normal.textColor = selected ? ShopBrassLight : dim ? ShopMuted : ShopText;
                GUI.Label(new Rect(slot.xMax + 10f, row.y, row.width - slot.width - 120f, row.height), item.Name, _shopItem);

                // O preco, com o icone da moeda; ou o selo de vendido.
                if (soldOut)
                {
                    string stamp = item.MaxCount == 1 ? "COMPRADO" : "ESGOTADO";
                    Vector2 size = _shopStamp.CalcSize(new GUIContent(stamp));
                    var tag = new Rect(row.xMax - size.x - 20f, row.center.y - 11f, size.x + 12f, 22f);
                    Fill(tag, ShopMuted);
                    Fill(new Rect(tag.x + 1f, tag.y + 1f, tag.width - 2f, tag.height - 2f), ShopInk);
                    GUI.Label(tag, stamp, _shopStamp);
                }
                else
                {
                    Price(row.xMax - 10f, row.center.y, item.Currency, price, affordable);
                }
            }

            // -- O detalhe do marcado: icone grande, nome, descricao, preco.
            Panel(detail, ShopInk, ShopBrassDark, ShopBrassDark);
            if (shop.Items.Count > 0)
            {
                ShopItem item = shop.Items[shop.Selected];
                var big = new Rect(detail.center.x - 36f, detail.y + 16f, 72f, 72f);
                Panel(big, ShopWood, ShopBrass, ShopBrassDark);
                Texture2D icon = ShopIcons.Get(item.Id);
                if (icon != null) GUI.DrawTexture(new Rect(big.x + 12f, big.y + 12f, 48f, 48f), icon);

                GUI.Label(new Rect(detail.x + 12f, big.yMax + 8f, detail.width - 24f, 28f), item.Name, _shopDetailName);
                Fill(new Rect(detail.center.x - 30f, big.yMax + 38f, 60f, 2f), ShopBrassDark);
                GUI.Label(new Rect(detail.x + 16f, big.yMax + 46f, detail.width - 32f, 80f), item.Description, _shopDesc);

                if (!string.IsNullOrEmpty(shop.LastMessage))
                    GUI.Label(new Rect(detail.x + 12f, detail.yMax - 34f, detail.width - 24f, 26f), shop.LastMessage, _shopReply);
            }

            // -- Rodape: as teclas como teclinhas. "comprar" e "sair" tambem
            // se clicam com o mouse.
            float keyY = y + height - footerH + 6f;
            float keyX = x + 20f;
            keyX = KeyHint(keyX, keyY, "W/S", "escolher");
            float buyX = keyX + 22f;
            keyX = KeyHint(buyX, keyY, "Espaço", "comprar");
            if (ShopMouse(new Rect(buyX, keyY, keyX - buyX, 32f), out bool buyClick) && buyClick) shop.ClickBuy(shop.Selected);
            float quitX = keyX + 22f;
            float quitEnd = KeyHint(quitX, keyY, "E", "sair");
            if (ShopMouse(new Rect(quitX, keyY, quitEnd - quitX, 32f), out bool quitClick) && quitClick) shop.ClickClose();
            GUI.Label(new Rect(quitEnd + 22f, keyY + 4f, 260f, 24f), "ou clique no item", _shopSub);

            GUI.contentColor = previous;
        }

        // ------------------------------------------------------------------
        // Pecas da loja
        // ------------------------------------------------------------------

        private static readonly Color ShopWood = new Color(0.13f, 0.09f, 0.08f, 0.98f);
        private static readonly Color ShopInk = new Color(0.07f, 0.05f, 0.06f, 1f);
        private static readonly Color ShopSelected = new Color(0.27f, 0.19f, 0.10f, 1f);
        private static readonly Color ShopBrass = new Color(0.82f, 0.62f, 0.30f);
        private static readonly Color ShopBrassLight = new Color(1f, 0.87f, 0.55f);
        private static readonly Color ShopBrassDark = new Color(0.42f, 0.29f, 0.15f);
        private static readonly Color ShopText = new Color(0.92f, 0.88f, 0.80f);
        private static readonly Color ShopMuted = new Color(0.52f, 0.47f, 0.43f);
        private static readonly Color ShopShardTint = new Color(0.72f, 0.92f, 1f);
        private static readonly Color ShopGemTint = new Color(1f, 0.6f, 0.6f);

        private GUIStyle _shopTitle, _shopSub, _shopItem, _shopPrice, _shopStamp, _shopDetailName, _shopDesc, _shopReply, _shopKey, _shopCoin;

        private void EnsureShopStyles()
        {
            if (_shopTitle != null) return;

            _shopTitle = new GUIStyle(_label) { fontSize = 24 };
            _shopTitle.normal.textColor = ShopBrassLight;
            _shopSub = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Italic };
            _shopSub.normal.textColor = ShopMuted;
            _shopItem = new GUIStyle(_label) { fontSize = 17, alignment = TextAnchor.MiddleLeft, wordWrap = false, clipping = TextClipping.Clip };
            _shopPrice = new GUIStyle(_label) { fontSize = 17, alignment = TextAnchor.MiddleRight };
            _shopStamp = new GUIStyle(_label) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            _shopStamp.normal.textColor = ShopMuted;
            _shopDetailName = new GUIStyle(_label) { fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _shopDetailName.normal.textColor = ShopBrassLight;
            _shopDesc = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, alignment = TextAnchor.UpperCenter };
            _shopDesc.normal.textColor = ShopText;
            _shopReply = new GUIStyle(_label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _shopReply.normal.textColor = ShopBrassLight;
            _shopKey = new GUIStyle(_label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            _shopKey.normal.textColor = ShopInk;
            _shopCoin = new GUIStyle(_label) { fontSize = 22 };
        }

        /// <summary>Uma placa: borda escura por fora, filete de latao, fundo.</summary>
        private static void Panel(Rect r, Color fill, Color rim, Color shadow)
        {
            Fill(new Rect(r.x - 3f, r.y - 3f, r.width + 6f, r.height + 6f), ShopInk);
            Fill(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), rim);
            Fill(new Rect(r.x, r.y + r.height, r.width, 2f), shadow);
            Fill(r, fill);
        }

        /// <summary>Os rebites de latao nos cantos da moldura.</summary>
        private static void Rivets(Rect r)
        {
            foreach (Vector2 c in new[] { new Vector2(r.x + 6f, r.y + 6f), new Vector2(r.xMax - 10f, r.y + 6f),
                         new Vector2(r.x + 6f, r.yMax - 10f), new Vector2(r.xMax - 10f, r.yMax - 10f) })
            {
                Fill(new Rect(c.x, c.y, 4f, 4f), ShopBrassDark);
                Fill(new Rect(c.x, c.y, 2f, 2f), ShopBrassLight);
            }
        }

        /// <summary>Um valor da carteira, alinhado a direita em right. Devolve onde comecou.</summary>
        private float Coin(float right, float y, Texture2D icon, int value, Color tint)
        {
            string text = value.ToString();
            Vector2 size = _shopCoin.CalcSize(new GUIContent(text));
            float w = icon != null ? icon.width * IconScale : 0f;
            float h = icon != null ? icon.height * IconScale : 0f;
            float left = right - size.x - w - 8f;

            if (icon != null) DrawCentered(icon, new Rect(left, y + (size.y - h) * 0.5f, w, h));
            _shopCoin.normal.textColor = tint;
            GUI.Label(new Rect(left + w + 6f, y, size.x + 4f, size.y), text, _shopCoin);
            return left;
        }

        /// <summary>
        /// O preco de uma fileira: o icone da moeda numa casinha fixa na
        /// direita e o numero numa coluna fixa colada nela, os dois centrados
        /// na mesma linha. Assim os precos de todas as fileiras ficam um
        /// embaixo do outro, seja em fragmento (o cristal torto, com a linha
        /// de baixo vazia) ou em gema.
        /// </summary>
        private void Price(float right, float centerY, Currency currency, int price, bool affordable)
        {
            Texture2D icon = currency == Currency.Gems ? _gemIcon : _shardIcon;
            const float box = 20f;
            const float column = 52f;

            _shopPrice.normal.textColor = affordable ? ShopText : new Color(0.85f, 0.42f, 0.38f);
            GUI.Label(new Rect(right - box - 6f - column, centerY - 15f, column, 30f), price.ToString(), _shopPrice);

            if (icon != null)
            {
                GUI.color = affordable ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                DrawCentered(icon, new Rect(right - box, centerY - box * 0.5f, box, box));
                GUI.color = Color.white;
            }
        }

        private Vector2 _lastMouse = new Vector2(-1f, -1f);

        /// <summary>
        /// O mouse em cima de uma area da loja. So conta como "passou por
        /// cima" quando ele de fato se mexe (senao o mouse parado brigaria com
        /// W/S); o clique do botao esquerdo vale sempre.
        /// </summary>
        private bool ShopMouse(Rect area, out bool clicked)
        {
            Event e = Event.current;
            clicked = false;
            if (!area.Contains(e.mousePosition)) return false;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                clicked = true;
                e.Use();
                return true;
            }

            if (e.type == EventType.Repaint && (e.mousePosition - _lastMouse).sqrMagnitude > 0.5f)
            {
                _lastMouse = e.mousePosition;
                return true;
            }

            return false;
        }

        private readonly Dictionary<Texture2D, Rect> _visible = new Dictionary<Texture2D, Rect>();

        /// <summary>
        /// Desenha um icone de pixel com a parte desenhada (sem a borda
        /// transparente) no meio da caixa, em escala inteira.
        /// </summary>
        private void DrawCentered(Texture2D icon, Rect box)
        {
            if (!_visible.TryGetValue(icon, out Rect used))
            {
                used = new Rect(0f, 0f, icon.width, icon.height);
                if (icon.isReadable)
                {
                    int x0 = icon.width, y0 = icon.height, x1 = -1, y1 = -1;
                    Color32[] pixels = icon.GetPixels32();
                    for (int y = 0; y < icon.height; y++)
                    {
                        for (int x = 0; x < icon.width; x++)
                        {
                            if (pixels[x + y * icon.width].a == 0) continue;
                            x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x);
                            y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y);
                        }
                    }

                    if (x1 >= 0) used = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
                }

                _visible[icon] = used;
            }

            float scale = Mathf.Max(1f, Mathf.Floor(Mathf.Min(box.width / used.width, box.height / used.height)));
            float w = icon.width * scale;
            float h = icon.height * scale;

            // A parte desenhada no centro da caixa (y da textura cresce pra cima).
            float vx = box.center.x - (used.x + used.width * 0.5f) * scale;
            float vy = box.center.y - (icon.height - used.y - used.height * 0.5f) * scale;
            GUI.DrawTexture(new Rect(Mathf.Round(vx), Mathf.Round(vy), w, h), icon);
        }

        /// <summary>Uma teclinha de latao com o que ela faz ao lado. Devolve onde terminou.</summary>
        private float KeyHint(float x, float y, string key, string action)
        {
            Vector2 keySize = _shopKey.CalcSize(new GUIContent(key));
            var cap = new Rect(x, y + 4f, Mathf.Max(keySize.x + 14f, 26f), 24f);
            Fill(new Rect(cap.x, cap.y + 2f, cap.width, cap.height), ShopBrassDark);
            Fill(new Rect(cap.x, cap.y, cap.width, cap.height - 2f), ShopBrass);
            Fill(new Rect(cap.x + 2f, cap.y + 2f, cap.width - 4f, 2f), ShopBrassLight);
            GUI.Label(new Rect(cap.x, cap.y - 1f, cap.width, cap.height - 2f), key, _shopKey);

            Vector2 size = _shopSub.CalcSize(new GUIContent(action));
            GUI.Label(new Rect(cap.xMax + 8f, y + 6f, size.x + 4f, 22f), action, _shopSub);
            return cap.xMax + 8f + size.x;
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
        /// A caixa de fala de um NPC, com o busto de quem fala a esquerda, ou
        /// o pedido de botao de que da pra conversar.
        /// </summary>
        private void DrawDialogue()
        {
            // A fala de cena (um chefe falando) tem a vez.
            if (ScriptedDialogue.Active)
            {
                DrawSpeech(ScriptedDialogue.Speaker, ScriptedDialogue.PortraitId, ScriptedDialogue.CurrentLine, null);
                return;
            }

            Npc talking = Npc.Talking;

            if (talking == null)
            {
                _speakerShown = null;
                Npc nearby = Npc.Nearby;
                if (nearby != null && !Cinematic.Active) DrawPrompt("E: " + nearby.Prompt + " " + nearby.DisplayName);
                return;
            }

            string line = talking.CurrentLine;
            if (line == null) return;

            DrawSpeech(talking.DisplayName, talking.PortraitId, line, talking.Document);
        }

        // Quem esta na caixa agora, e desde quando (pra o busto entrar deslizando).
        private string _speakerShown;
        private float _speakerSince;
        private GUIStyle _speakerName;
        private static readonly Color SpeechFill = new Color(0.025f, 0.03f, 0.045f, 0.86f);

        /// <summary>
        /// A caixa de fala, como numa novela visual: o busto grande de quem fala
        /// saindo de baixo da tela, a esquerda, por cima da borda da caixa; a
        /// caixa escura e translucida com cantoneiras finas na cor de quem
        /// fala; o nome em cima, a linha embaixo e a setinha de continuar no
        /// canto. Se ha o que ler, o desenho aparece grande em cima.
        /// </summary>
        private void DrawSpeech(string speaker, string portraitId, string line, Sprite document)
        {
            if (line == null) return;

            // A grade de pixel da interface (320x180 ampliada).
            float u = Mathf.Max(1f, Mathf.Floor(Screen.height / 180f));
            Color accent = SpeakerPortraits.Accent(portraitId);

            string key = portraitId + "|" + speaker;
            if (key != _speakerShown)
            {
                _speakerShown = key;
                _speakerSince = Time.unscaledTime;
            }

            float age = Time.unscaledTime - _speakerSince;

            // O busto: inteiro, em passos inteiros, uns 70% da altura da tela.
            bool blink = Mathf.Repeat(Time.unscaledTime + 0.7f, 3.8f) < 0.12f;
            Texture2D bust = portraitId != null ? SpeakerPortraits.Get(portraitId, blink) : null;
            float scale = bust != null ? Mathf.Max(1f, Mathf.Floor(Screen.height * 0.72f / bust.height)) : 0f;
            float bustW = bust != null ? bust.width * scale : 0f;
            float bustH = bust != null ? bust.height * scale : 0f;

            float boxH = Mathf.Round(Screen.height * 0.27f / u) * u;
            float boxY = Screen.height - boxH - 3f * u;
            float boxX = Mathf.Round(bustW * 0.62f / u) * u + 6f * u;
            float boxW = Mathf.Min(Screen.width - boxX - 6f * u, Mathf.Round(Screen.width * 0.66f / u) * u);
            if (bust == null) boxX = Mathf.Round((Screen.width - boxW) * 0.5f / u) * u;

            // O que se le aparece grande, no meio da tela, em cima da caixa.
            if (document != null) DrawDocument(document, new Rect(40f, 24f, Screen.width - 80f, boxY - 24f - 20f));

            // A caixa: o fundo, a luz fininha em cima e as cantoneiras.
            var box = new Rect(boxX, boxY, boxW, boxH);
            Fill(box, SpeechFill);
            Fill(new Rect(box.x, box.y, box.width, u), new Color(accent.r, accent.g, accent.b, 0.12f));
            Corners(box, u, 7f * u, new Color(accent.r, accent.g, accent.b, 0.9f));

            // O busto por cima da borda esquerda, entrando de lado.
            float textX = box.x + 8f * u;
            if (bust != null)
            {
                float enter = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 0.22f));
                float bx = Mathf.Round((box.x - bustW * 0.62f - (1f - enter) * 16f * u) / scale) * scale;
                float by = Screen.height - bustH;

                Color previous = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, enter);
                GUI.DrawTexture(new Rect(bx, by, bustW, bustH), bust);
                GUI.color = previous;

                textX = box.x + bustW * 0.38f + 6f * u;
            }

            float textW = box.xMax - 12f * u - textX;

            int nameSize = Mathf.RoundToInt(5.4f * u);
            if (_speakerName == null || _speakerName.fontSize != nameSize)
            {
                _speakerName = new GUIStyle(GUI.skin.label) { fontSize = nameSize, fontStyle = FontStyle.Bold, wordWrap = false };
                _speech.fontSize = Mathf.RoundToInt(5f * u);
            }

            if (!string.IsNullOrEmpty(speaker))
            {
                Color previous = GUI.contentColor;
                GUI.contentColor = accent;
                GUI.Label(new Rect(textX, box.y + 5f * u, textW, 9f * u), speaker, _speakerName);
                GUI.contentColor = previous;
            }

            GUI.Label(new Rect(textX, box.y + 16f * u, textW, box.height - 22f * u), line, _speech);

            // A setinha de continuar, quicando no canto.
            float bob = Mathf.Repeat(Time.unscaledTime, 0.9f) < 0.45f ? 0f : u;
            float ax = box.xMax - 10f * u;
            float ay = box.yMax - 8f * u + bob;
            Fill(new Rect(ax, ay, 5f * u, u), accent);
            Fill(new Rect(ax + u, ay + u, 3f * u, u), accent);
            Fill(new Rect(ax + 2f * u, ay + 2f * u, u, u), accent);
        }

        /// <summary>As quatro cantoneiras em L, finas, nos cantos da caixa.</summary>
        private static void Corners(Rect box, float thick, float length, Color color)
        {
            float l = box.x - thick, r = box.xMax, t = box.y - thick, b = box.yMax;
            Fill(new Rect(l, t, length, thick), color);
            Fill(new Rect(l, t, thick, length), color);
            Fill(new Rect(r - length + thick, t, length, thick), color);
            Fill(new Rect(r, t, thick, length), color);
            Fill(new Rect(l, b, length, thick), color);
            Fill(new Rect(l, b - length + thick, thick, length), color);
            Fill(new Rect(r - length + thick, b, length, thick), color);
            Fill(new Rect(r, b - length + thick, thick, length), color);
        }

        /// <summary>
        /// O desenho do que se le (a carta, a placa, o muro): escurece o jogo
        /// atras e mostra a arte o maior possivel dentro da area, em passos
        /// inteiros quando cabe pelo menos o dobro, pra o pixel art nao borrar.
        /// </summary>
        private static void DrawDocument(Sprite sprite, Rect area)
        {
            if (sprite.texture == null || area.width <= 0f || area.height <= 0f) return;

            Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.6f));

            Rect pixels = sprite.rect;
            float fit = Mathf.Min(area.width / pixels.width, area.height / pixels.height);
            float scale = fit >= 2f ? Mathf.Floor(fit) : fit;

            float drawWidth = pixels.width * scale;
            float drawHeight = pixels.height * scale;
            var target = new Rect(
                area.x + (area.width - drawWidth) * 0.5f,
                area.y + (area.height - drawHeight) * 0.5f,
                drawWidth, drawHeight);

            Texture texture = sprite.texture;
            var uv = new Rect(
                pixels.x / texture.width, pixels.y / texture.height,
                pixels.width / texture.width, pixels.height / texture.height);

            GUI.DrawTextureWithTexCoords(target, texture, uv);
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
