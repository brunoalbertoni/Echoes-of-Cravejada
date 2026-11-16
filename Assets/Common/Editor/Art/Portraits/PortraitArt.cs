using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os retratos de busto da caixa de dialogo: quem fala aparece grande, a
    /// esquerda, saindo de baixo da tela e olhando pra direita (pro texto).
    /// Cada um segue o desenho do personagem no mapa (as folhas que o jogo
    /// usa de verdade):
    ///
    ///   refugo     DiscardedArt 0  lata de lampiao, vidro rachado com o caco aceso, vara de gancho
    ///   rebite     DiscardedArt 1  cobre redondo cheio de rebites, capuz de couro, lupa de ourives
    ///   ferrolho   DiscardedArt 2  caldeira com manometro, cabeca de parafuso, oculos na testa, sem um braco
    ///   pequena    DiscardedArt 3  lata de conserva, olhos grandes, antena torta, giz
    ///   plumber    District2NpcArt 0  latao curvado, mascara de solda levantada, olhos cansados
    ///   solda      District2NpcArt 1  lenco vermelho, a mascara debaixo do braco
    ///   crisol     District2NpcArt 2  prata riscada, monoculo, bigorna nas costas, lustra uma gema
    ///   miner      brass_welder tingido  capacete com lampiao (fase 3)
    ///   miner_f    a Mineira: o mesmo, com a tranca de cabo e o lenco
    ///   assistant  LabFoeArt.Assistant  cobre, jaleco limpo, oculos na testa, prancheta
    ///   king       KingArt (regal)  prata 925, coroa de quartzo, barba pontuda, manto vermelho, cetro
    ///   king_corrupt KingArt (corrupt) o mesmo tomado pelo Oxide, o Coracao verde no peito
    ///   generic    um autonomo de latao qualquer (reserva)
    ///
    /// Cada retrato tem 144x168 e sai em dois quadros: olhos abertos e a
    /// piscada. Desenhado em relevo (<see cref="PortraitRelief"/>).
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class PortraitArt
    {
        public const int Width = 144;
        public const int Height = 168;

        public static readonly string[] Ids =
        {
            "refugo", "rebite", "ferrolho", "pequena", "plumber", "solda", "crisol",
            "miner", "miner_f", "assistant", "king", "king_corrupt", "generic",
        };

        // ------------------------------------------------------------------
        // Materiais
        // ------------------------------------------------------------------

        private static readonly PCol Tin = PCol.Hex(0x9ea2ac);
        private static readonly PCol Iron = PCol.Hex(0x5c5a68);
        private static readonly PCol DarkIron = PCol.Hex(0x3a3844);
        private static readonly PCol Brass = PCol.Hex(0xc89c48);
        private static readonly PCol Copper = PCol.Hex(0xb46c46);
        private static readonly PCol Silver = PCol.Hex(0xd6dae4);
        private static readonly PCol Leather = PCol.Hex(0x7e5234);
        private static readonly PCol Cloth = PCol.Hex(0x8c8472);
        private static readonly PCol Wood = PCol.Hex(0x6e4a2e);
        private static readonly PCol Gold = PCol.Hex(0xe4b452);
        private static readonly PCol Verdigris = PCol.Hex(0x4e9a7c);
        private static readonly PCol RustSpot = PCol.Hex(0x8a4a26);
        private static readonly PCol Visor = PCol.Hex(0x1a1822);
        private static readonly PCol EyeCore = PCol.Hex(0xfff8e2);
        private static readonly PCol EyeGlow = PCol.Hex(0xffce80);
        private static readonly PCol Velvet = PCol.Hex(0x8e2438);
        private static readonly PCol Quartz = PCol.Hex(0xe6f0fa);
        private static readonly PCol OxideGlow = PCol.Hex(0x78ffaa);
        private static readonly PCol OxideCore = PCol.Hex(0xe8ffee);

        /// <summary>O retrato pronto (blink: o quadro da piscada).</summary>
        public static PixelCanvas Draw(string id, bool blink)
        {
            var r = new PortraitRelief(Width, Height);
            List<Action<PixelCanvas>> after = null;

            switch (id)
            {
                case "refugo": Refugo(r, blink); break;
                case "rebite": Rebite(r, blink); break;
                case "ferrolho": Ferrolho(r, blink); break;
                case "pequena": Pequena(r, blink); break;
                case "plumber": Plumber(r, blink); break;
                case "solda": Solda(r, blink); break;
                case "crisol": Crisol(r, blink); break;
                case "miner": Miner(r, blink, false); break;
                case "miner_f": Miner(r, blink, true); break;
                case "assistant": Assistant(r, blink); break;
                case "king": King(r, blink, false); break;
                case "king_corrupt": after = King(r, blink, true); break;
                default: Generic(r, blink); break;
            }

            PixelCanvas canvas = r.Bake();
            if (after != null) foreach (Action<PixelCanvas> a in after) a(canvas);
            return canvas;
        }

        // ------------------------------------------------------------------
        // Pecas que se repetem
        // ------------------------------------------------------------------

        /// <summary>O visor escuro com os dois olhos acesos (ou apagados, na piscada).</summary>
        private static void VisorEyes(PortraitRelief r, float x0, float y0, float x1, float y1, float z, bool blink,
            PCol core, PCol glow, float eyeR = 3.6f, float squint = 1f)
        {
            r.Box(x0, y0, x1, y1, 5f, z, 2.5f, Visor, 0.95f, 0f, 2f);
            float cy = (y0 + y1) * 0.5f;
            float w = x1 - x0;
            float[] xs = { x0 + w * 0.3f, x0 + w * 0.76f };
            float[] rs = { eyeR * 0.85f, eyeR };

            for (int i = 0; i < 2; i++)
            {
                if (blink)
                {
                    r.Box(xs[i] - rs[i], cy - 0.6f, xs[i] + rs[i], cy + 0.9f, 0.5f, z + 3f, 0f, glow * 0.45f, 0f, 0f, 1f, true);
                    continue;
                }

                r.Dome(xs[i], cy, rs[i] + 1.2f, (rs[i] + 1.2f) * squint, z + 3f, 0f, glow, 0f, true);
                r.Dome(xs[i] - 0.4f, cy - 0.4f, rs[i] * 0.55f, rs[i] * 0.55f * squint, z + 3f, 0f, core, 0f, true);
                r.Glow(xs[i], cy, rs[i] * 3.2f, glow, 0.55f);
            }
        }

        /// <summary>O pescoco: um cilindro grosso de gomos, com a arruela embaixo.</summary>
        private static void Neck(PortraitRelief r, float x, float y0, float y1, PCol c)
        {
            r.Tube(x, y0, x, y1, 13f, 14f, 10f, 5f, c, 0.45f);
            for (float y = y0 + 6; y < y1 - 4; y += 6) r.Seam(x - 12, y, x + 12, y);
            r.Box(x - 20, y1 - 6, x + 20, y1 + 4, 4f, 14f, 3f, c, 0.6f, 3f);
        }

        /// <summary>Os ombros e o comeco do peito, cortados embaixo pela tela.</summary>
        private static void Shoulders(PortraitRelief r, float x0, float y0, float x1, PCol c, float gloss = 0.5f, float radius = 22f)
        {
            r.Box(x0, y0, x1, Height + 30, radius, 6f, 7f, c, gloss, 10f, 6f);
        }

        // ------------------------------------------------------------------
        // Distrito 1: os descartados
        // ------------------------------------------------------------------

        private static void Refugo(PortraitRelief r, bool blink)
        {
            // A vara de gancho, atras de tudo, encostada no ombro.
            r.Tube(18, 170, 34, 18, 2.6f, 2.2f, 2f, 3f, Wood, 0.2f);
            r.Ring(40, 20, 4f, 7f, 4f, 2f, Iron, 0.6f);
            r.Erase((x, y) => x > 36 && y > 18 && y < 30 && x < 50 && r.Height(x, y) > 3.5f && r.Height(x, y) < 7f);

            Shoulders(r, 12, 124, 134, Iron);
            r.Patina(12, 124, 134, 168, 0.3f, 11, Verdigris);

            // O peito e a lanterna: moldura de latao, vidro rachado e o caco aceso.
            r.Box(42, 120, 114, 200, 6f, 14f, 4f, Brass, 0.7f, 3f, 3f);
            r.Box(50, 128, 106, 200, 3f, 13f, 1.5f, PCol.Hex(0x5f8a88), 0.95f, 2f, 2f);
            r.Poly(new (float, float)[] { (76, 168), (70, 150), (78, 136), (86, 146), (88, 168) }, 16f, 2f, PCol.Hex(0xffe8a8), 0f, 2f, true);
            r.Poly(new (float, float)[] { (77, 160), (75, 150), (79, 142), (83, 150), (83, 160) }, 17f, 0f, PCol.Hex(0xfffbe8), 0f, 1f, true);
            r.Glow(80, 152, 26, PCol.Hex(0xffd890), 0.6f);
            r.Seam(52, 134, 66, 148, 1.5f);
            r.Seam(66, 148, 62, 166, 1.5f);
            r.Seam(98, 130, 92, 144, 1.5f);
            r.Rivets(46, 124, 110, 124, 7, 1.6f, Brass);
            r.Patina(42, 120, 114, 168, 0.22f, 12, Verdigris);

            // O pescoco e a cabeca de lata, com a alca de lampiao em cima.
            Neck(r, 78, 100, 126, DarkIron);
            r.Seam(70, 110, 89, 110);
            r.Seam(70, 116, 90, 116);
            r.Ring(80, 44, 15f, 19f, 14f, 3f, Iron, 0.7f, 0.9f);
            r.Box(44, 40, 118, 108, 18f, 18f, 8f, Tin, 0.65f, 10f, 5f);
            r.Seam(46, 58, 116, 58);
            r.Rivets(50, 52, 112, 52, 6, 1.7f, Tin);
            r.Dent(58, 84, 7f, 3f);
            r.Dent(70, 98, 4f, 2f);
            r.Patina(44, 40, 118, 108, 0.26f, 13, Verdigris);
            r.Speckle(44, 40, 118, 108, 30, 14, RustSpot);

            VisorEyes(r, 76, 64, 116, 82, 30f, blink, EyeCore, EyeGlow);
            r.Rim = PCol.Hex(0xffd8a0);
        }

        private static void Rebite(PortraitRelief r, bool blink)
        {
            // O capuz de couro por tras, caindo nos ombros como capa.
            r.Box(6, 112, 138, 220, 34f, 4f, 6f, Leather, 0.2f, 12f, 6f);
            r.Dome(74, 70, 52, 54, 6f, 20f, Leather, 0.15f);
            r.Seam(30, 50, 54, 22);
            r.Seam(24, 96, 20, 130);

            // O avental de couro com as alcas e os rebites.
            r.Box(36, 132, 112, 220, 8f, 22f, 3f, PCol.Hex(0x93633c), 0.25f, 4f);
            r.Tube(44, 132, 50, 112, 3f, 3f, 24f, 2f, PCol.Hex(0x5a3a26), 0.2f);
            r.Tube(104, 132, 100, 112, 3f, 3f, 24f, 2f, PCol.Hex(0x5a3a26), 0.2f);
            r.Box(56, 146, 92, 168, 3f, 24f, 2f, PCol.Hex(0x83552f), 0.2f);
            r.Rivets(40, 136, 108, 136, 6, 1.6f, Brass);

            // A cabeca de cobre redonda, cheia de rebites, saindo do capuz.
            r.Dome(80, 78, 38, 37, 18f, 18f, Copper, 0.7f);
            r.Seam(44, 80, 116, 76);
            r.Seam(80, 42, 82, 114);
            for (int i = 0; i < 9; i++)
            {
                double a = Math.PI * (0.15 + i * 0.09);
                r.Rivet(80 + (float)Math.Cos(a) * 33f, 78 - (float)Math.Sin(a) * 31f, 1.6f, Copper);
            }

            r.Rivets(48, 80, 114, 76, 8, 1.5f, Copper);
            r.Patina(40, 40, 120, 116, 0.18f, 21, Verdigris);
            r.Speckle(40, 40, 120, 116, 25, 22, RustSpot);

            // A borda do capuz por cima da testa.
            r.Tube(36, 66, 86, 38, 5f, 4f, 30f, 4f, PCol.Hex(0x6a4428), 0.2f);

            // Um olho; o outro atras da lupa de ourives.
            r.Box(78, 82, 100, 96, 4f, 34f, 2f, Visor, 0.95f, 0f, 2f);
            if (blink) r.Box(80, 88, 92, 90, 0.5f, 37f, 0f, EyeGlow * 0.45f, 0f, 0f, 1f, true);
            else
            {
                r.Dome(86, 89, 4.6f, 4.6f, 37f, 0f, EyeGlow, 0f, true);
                r.Dome(85.5f, 88.5f, 2.2f, 2.2f, 37f, 0f, EyeCore, 0f, true);
                r.Glow(86, 89, 12, EyeGlow, 0.5f);
            }

            r.Tube(100, 78, 122, 74, 7f, 6f, 34f, 5f, Brass, 0.8f);
            r.Seam(108, 70, 108, 82);
            r.Ring(123, 74, 5f, 9f, 40f, 3f, Gold, 0.9f);
            r.Dome(123, 74, 5.2f, 5.2f, 38f, 0f, blink ? PCol.Hex(0x5a7c86) : PCol.Hex(0xbfefff), 0f, true);
            if (!blink)
            {
                r.Dome(121.5f, 72.5f, 1.6f, 1.6f, 38f, 0f, PCol.Hex(0xffffff), 0f, true);
                r.Glow(123, 74, 16, PCol.Hex(0xbfefff), 0.4f);
            }

            r.Rim = PCol.Hex(0xffc89a);
        }

        private static void Ferrolho(PortraitRelief r, bool blink)
        {
            // A chave inglesa apoiada no ombro direito, atras da cabeca.
            r.Tube(92, 150, 132, 96, 4f, 4f, 10f, 3f, Iron, 0.7f);
            r.Box(122, 78, 142, 100, 3f, 12f, 3f, Iron, 0.7f);
            r.Erase((x, y) => x >= 129 && x <= 135 && y >= 76 && y <= 89);

            // O corpo de caldeira: ferro com cintas de latao e o manometro no peito.
            Shoulders(r, 8, 118, 138, Iron, 0.55f, 26f);
            r.Box(8, 140, 138, 148, 3f, 16f, 2f, Brass, 0.8f);
            r.Rivets(14, 144, 132, 144, 10, 1.5f, Brass);
            r.Ring(94, 158, 10f, 14f, 20f, 3f, Brass, 0.85f);
            r.Dome(94, 158, 10f, 10f, 19f, 1f, PCol.Hex(0xe8e0c8), 0.5f);
            r.Tint((x, y) => (Math.Abs((x - 94) - (y - 158) * -0.9f) < 0.8f && x >= 94 && x <= 101 && y <= 158 && y >= 151) ? 1f : 0f, PCol.Hex(0xb02a24));
            for (int i = 0; i < 7; i++)
            {
                double a = Math.PI * (0.85 + i * 0.22);
                int tx = (int)(94 + Math.Cos(a) * 8), ty = (int)(158 + Math.Sin(a) * 8);
                r.Tint((x, y) => x == tx && y == ty ? 1f : 0f, PCol.Hex(0x3a3030));
            }

            // O braco que falta: o toco com os fios saindo.
            r.Box(4, 122, 30, 146, 8f, 14f, 4f, DarkIron, 0.5f, 4f);
            r.Tube(10, 128, 2, 118, 1.4f, 1.2f, 18f, 1f, PCol.Hex(0xb03a2c), 0.4f);
            r.Tube(16, 126, 10, 112, 1.4f, 1.2f, 18f, 1f, PCol.Hex(0xd8b040), 0.4f);
            r.Tube(22, 126, 22, 114, 1.2f, 1f, 18f, 1f, PCol.Hex(0x3a6cb0), 0.4f);
            r.Patina(4, 118, 138, 168, 0.25f, 31, Verdigris);
            r.Speckle(4, 118, 138, 168, 30, 32, RustSpot);

            // O pescoco e a cabeca de parafuso sextavado.
            Neck(r, 76, 104, 126, DarkIron);
            var hex = new List<(float, float)>();
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3;
                hex.Add((80 + (float)Math.Cos(a) * 44f, 72 + (float)Math.Sin(a) * 42f));
            }

            r.Poly(hex.ToArray(), 18f, 9f, Iron, 0.65f, 7f);
            r.Box(42, 26, 118, 38, 4f, 22f, 4f, DarkIron, 0.6f);
            r.Seam(58, 38, 102, 38);
            r.Scratches(40, 36, 120, 108, 18, 33, PCol.Hex(0xb8b8c4), 0.5f);
            r.Patina(38, 34, 122, 110, 0.2f, 34, Verdigris);
            r.Dent(56, 92, 6f, 2.5f);

            // Os oculos de protecao na testa.
            r.Box(40, 44, 120, 52, 2f, 26f, 1.5f, Leather, 0.2f);
            r.Ring(70, 47, 6f, 10f, 30f, 3f, Brass, 0.85f);
            r.Dome(70, 47, 6.4f, 6.4f, 29f, 2f, PCol.Hex(0x6aa0a8), 0.95f);
            r.Ring(96, 47, 6f, 10f, 30f, 3f, Brass, 0.85f);
            r.Dome(96, 47, 6.4f, 6.4f, 29f, 2f, PCol.Hex(0x6aa0a8), 0.95f);

            VisorEyes(r, 78, 66, 118, 80, 32f, blink, EyeCore, EyeGlow, 3.4f, 0.6f);
            r.Rim = PCol.Hex(0xffd2a0);
        }

        private static void Pequena(PortraitRelief r, bool blink)
        {
            // O corpo de lata de conserva, com o rotulo e os frisos.
            r.Box(30, 128, 118, 220, 10f, 10f, 4f, Tin, 0.75f, 12f, 3f);
            r.Tint((x, y) => y >= 140 && y <= 156 ? 1f : 0f, PCol.Hex(0xb04a38));
            r.Tint((x, y) => y >= 145 && y <= 151 && x > 46 && x < 104 ? 1f : 0f, PCol.Hex(0xe8dcb8));
            r.Seam(30, 134, 118, 134);
            r.Seam(30, 162, 118, 162);
            r.Speckle(30, 128, 118, 168, 30, 41, RustSpot);

            // A mao com o giz.
            r.Tube(104, 168, 116, 148, 5f, 5f, 18f, 3f, Copper, 0.6f);
            r.Dome(118, 146, 7f, 6f, 22f, 4f, Copper, 0.6f);
            r.Box(116, 128, 123, 145, 2f, 26f, 2f, PCol.Hex(0xf2f0e6), 0.1f);

            // A antena torta, com a bolinha acesa.
            r.Tube(70, 64, 62, 38, 1.8f, 1.6f, 16f, 1.5f, Iron, 0.6f);
            r.Tube(62, 38, 76, 22, 1.6f, 1.5f, 16f, 1.5f, Iron, 0.6f);
            r.Dome(77, 20, 4.6f, 4.6f, 18f, 0f, PCol.Hex(0xffe08a), 0f, true);
            r.Dome(76, 19, 2f, 2f, 18f, 0f, PCol.Hex(0xfffbe6), 0f, true);
            r.Glow(77, 20, 16, PCol.Hex(0xffd070), 0.55f);

            // A cabeca de cobre, grande pro corpo, com os olhos enormes.
            Neck(r, 76, 116, 132, DarkIron);
            r.Box(38, 56, 116, 122, 22f, 18f, 8f, Copper, 0.7f, 10f, 5f);
            r.Seam(40, 70, 114, 70);
            r.Rivets(46, 64, 108, 64, 5, 1.6f, Copper);
            r.Patina(38, 56, 116, 122, 0.16f, 42, Verdigris);

            r.Box(62, 78, 116, 108, 9f, 30f, 2.5f, Visor, 0.95f, 0f, 2f);
            PCol cyan = PCol.Hex(0x7ae6ff);
            float[] ex = { 76, 102 };
            float[] er = { 7.5f, 8.5f };
            for (int i = 0; i < 2; i++)
            {
                if (blink)
                {
                    r.Box(ex[i] - er[i], 93, ex[i] + er[i], 95, 1f, 33f, 0f, cyan * 0.5f, 0f, 0f, 1f, true);
                    continue;
                }

                r.Dome(ex[i], 93, er[i], er[i] * 1.1f, 33f, 0f, cyan, 0f, true);
                r.Dome(ex[i] + 0.5f, 94, er[i] * 0.55f, er[i] * 0.6f, 33f, 0f, PCol.Hex(0x1e5a7a), 0f, true);
                r.Dome(ex[i] - er[i] * 0.35f, 93 - er[i] * 0.45f, 2f, 2f, 33f, 0f, PCol.Hex(0xffffff), 0f, true);
                r.Glow(ex[i], 93, er[i] * 2.6f, cyan, 0.45f);
            }

            r.Rim = PCol.Hex(0x9ff0ff);
        }

        // ------------------------------------------------------------------
        // Distrito 2: os Tarnished e o Crisol
        // ------------------------------------------------------------------

        private static void Plumber(PortraitRelief r, bool blink)
        {
            // Os ombros de latao, curvados pra frente, e o avental.
            r.Poly(new (float, float)[] { (4, 168), (10, 128), (40, 112), (110, 118), (138, 138), (142, 168) }, 6f, 8f, Brass, 0.55f, 7f);
            r.Box(30, 134, 116, 220, 6f, 14f, 3f, Cloth, 0.15f, 3f);
            r.Box(60, 146, 86, 166, 2f, 16f, 2f, PCol.Hex(0x7a5236), 0.15f);
            r.Tube(36, 136, 44, 116, 3f, 3f, 16f, 2f, PCol.Hex(0x5e5446), 0.15f);
            r.Patina(4, 112, 142, 168, 0.25f, 51, Verdigris);

            // A chave de cano em que ele se apoia, subindo pela direita.
            r.Tube(130, 172, 128, 112, 3.6f, 3.6f, 20f, 3f, DarkIron, 0.6f);
            r.Box(118, 100, 140, 114, 3f, 22f, 3f, DarkIron, 0.6f);
            r.Box(120, 92, 132, 101, 2f, 22f, 2f, Iron, 0.6f);

            // A cabeca de latao, mais baixa, com a mascara levantada na testa.
            Neck(r, 70, 108, 124, DarkIron);
            r.Box(40, 60, 112, 120, 20f, 18f, 8f, Brass, 0.65f, 9f, 5f);
            r.Seam(42, 76, 110, 76);
            r.Rivets(46, 70, 106, 70, 5, 1.6f, Brass);
            r.Patina(40, 60, 112, 120, 0.22f, 52, Verdigris);
            r.Speckle(40, 60, 112, 120, 25, 53, PCol.Hex(0x3a2a20));

            r.Poly(new (float, float)[] { (34, 58), (38, 28), (60, 18), (110, 22), (122, 40), (118, 62), (70, 66) }, 26f, 6f, DarkIron, 0.55f, 5f);
            r.Box(70, 30, 106, 44, 3f, 31f, 2f, PCol.Hex(0x2e5a4a), 0.95f);
            r.Rivet(40, 52, 3f, Iron);
            r.Rivet(116, 56, 2.4f, Iron);
            r.Scratches(36, 20, 120, 64, 10, 54, PCol.Hex(0x8a8a96), 0.5f);

            // Os olhos cansados: a palpebra de latao cobre a metade de cima.
            VisorEyes(r, 76, 84, 114, 100, 32f, blink, EyeCore, EyeGlow, 3.4f);
            if (!blink)
            {
                r.Box(78, 85, 112, 92, 2f, 36f, 1.5f, Brass, 0.5f);
            }

            r.Rim = PCol.Hex(0xffd8a0);
        }

        private static void Solda(PortraitRelief r, bool blink)
        {
            // Ombros de latao e o colete de couro.
            Shoulders(r, 10, 122, 134, Brass, 0.6f, 24f);
            r.Box(36, 128, 110, 220, 8f, 14f, 3f, Leather, 0.2f, 4f);
            r.Seam(73, 130, 73, 168);
            r.Rivets(68, 138, 68, 162, 3, 1.4f, Brass);
            r.Patina(10, 122, 134, 168, 0.15f, 61, Verdigris);

            // A mascara de solda debaixo do braco.
            r.Box(2, 128, 40, 168, 10f, 18f, 6f, DarkIron, 0.6f, 6f);
            r.Box(10, 140, 34, 150, 2f, 23f, 1.5f, PCol.Hex(0x2e5a4a), 0.95f);
            r.Tube(32, 168, 40, 140, 6f, 6f, 24f, 3f, Brass, 0.6f);

            // A cabeca de cobre queimado, com o lenco vermelho amarrado.
            Neck(r, 76, 104, 126, DarkIron);
            r.Box(46, 52, 114, 114, 20f, 18f, 8f, PCol.Hex(0xa8603c), 0.6f, 9f, 5f);
            r.Speckle(46, 52, 114, 114, 40, 62, PCol.Hex(0x3a2420), 0.5f);
            r.Patina(46, 70, 114, 114, 0.12f, 63, Verdigris);

            PCol red = PCol.Hex(0xb03a30);
            r.Dome(78, 56, 38, 22, 20f, 12f, red, 0.25f);
            r.Erase((x, y) => y > 62 && y < 80 && x > 50 && r.Height(x, y) > 21f && r.Height(x, y) < 33f && x > 112);
            r.Tube(42, 62, 116, 62, 4f, 4f, 30f, 3f, PCol.Hex(0x98302a), 0.25f);
            r.Tube(42, 62, 24, 76, 4f, 3f, 26f, 3f, red, 0.25f);
            r.Tube(42, 62, 26, 88, 3.5f, 2.5f, 25f, 3f, PCol.Hex(0x98302a), 0.25f);
            r.Dome(42, 62, 6f, 5f, 32f, 3f, red, 0.25f);
            r.Speckle(20, 34, 118, 90, 60, 64, PCol.Hex(0xe8d0b0), 0.35f);

            VisorEyes(r, 76, 74, 114, 90, 30f, blink, EyeCore, EyeGlow);
            r.Rim = PCol.Hex(0xffc0a0);
        }

        private static void Crisol(PortraitRelief r, bool blink)
        {
            PCol scratch = PCol.Hex(0xf6f8ff);
            PCol gouge = PCol.Hex(0x7c8090);

            // A bigorna de lapidario nas costas: o chifre aparece atras do ombro.
            r.Poly(new (float, float)[] { (2, 92), (34, 88), (50, 96), (50, 114), (22, 112), (8, 104) }, 4f, 5f, DarkIron, 0.65f, 4f);
            r.Box(24, 108, 50, 132, 3f, 6f, 3f, Iron, 0.5f);

            // Ombros de prata, a gola alta pontuda e a lapela.
            Shoulders(r, 10, 124, 136, Silver, 0.8f, 20f);
            r.Poly(new (float, float)[] { (42, 128), (46, 100), (60, 118), (76, 132), (64, 168), (44, 168) }, 18f, 4f, Silver, 0.8f, 3f);
            r.Poly(new (float, float)[] { (110, 128), (108, 100), (96, 118), (80, 132), (90, 168), (110, 168) }, 18f, 4f, Silver, 0.8f, 3f);
            r.Box(70, 128, 86, 160, 4f, 20f, 3f, PCol.Hex(0x3c3a54), 0.4f, 2f);
            r.Scratches(10, 120, 136, 168, 40, 71, scratch, 0.6f);
            r.Scratches(10, 120, 136, 168, 14, 72, gouge, 0.5f);

            // A mao com a gema e o pano de lustrar.
            r.Box(98, 144, 132, 168, 6f, 22f, 3f, PCol.Hex(0x8c7a5c), 0.1f, 3f);
            r.Dome(120, 146, 9f, 8f, 26f, 4f, Silver, 0.8f);
            r.Poly(new (float, float)[] { (114, 136), (120, 128), (127, 136), (120, 144) }, 30f, 0f, PCol.Hex(0x9ae6ff), 0f, 1f, true);
            r.Poly(new (float, float)[] { (117, 136), (120, 131), (123, 136), (120, 139) }, 30f, 0f, PCol.Hex(0xeaffff), 0f, 1f, true);
            r.Glow(120, 136, 18, PCol.Hex(0x9ae6ff), 0.55f);

            // A cabeca alta de prata riscada, com a crista em cima.
            Neck(r, 78, 104, 128, PCol.Hex(0x9ca0b0));
            r.Poly(new (float, float)[] { (60, 26), (66, 12), (72, 22), (80, 8), (88, 22), (94, 12), (100, 26) }, 16f, 4f, Silver, 0.8f, 3f);
            r.Box(48, 22, 114, 112, 20f, 20f, 8f, Silver, 0.85f, 10f, 5f);
            r.Seam(50, 38, 112, 38);
            r.Scratches(48, 22, 114, 112, 46, 73, scratch, 0.65f);
            r.Scratches(48, 22, 114, 112, 18, 74, gouge, 0.55f);
            r.Dent(62, 84, 6f, 2f);

            // O visor fino, o olho de tras e o monoculo de ouro no da frente.
            r.Box(80, 60, 116, 74, 4f, 30f, 2f, Visor, 0.95f, 0f, 2f);
            if (blink) r.Box(83, 66, 92, 68, 0.5f, 33f, 0f, EyeGlow * 0.45f, 0f, 0f, 1f, true);
            else
            {
                r.Dome(88, 67, 3.6f, 3.6f, 33f, 0f, EyeGlow, 0f, true);
                r.Dome(87.6f, 66.6f, 1.8f, 1.8f, 33f, 0f, EyeCore, 0f, true);
                r.Glow(88, 67, 10, EyeGlow, 0.5f);
            }

            r.Ring(106, 67, 7f, 10f, 36f, 3f, Gold, 0.95f);
            r.Dome(106, 67, 7.4f, 7.4f, 34f, 0f, blink ? PCol.Hex(0x5a7c86) : PCol.Hex(0xc8f2ff), 0f, true);
            if (!blink)
            {
                r.Dome(108, 69, 3f, 3f, 34f, 0f, PCol.Hex(0xfff2d0), 0f, true);
                r.Dome(103, 64, 1.6f, 1.6f, 34f, 0f, PCol.Hex(0xffffff), 0f, true);
                r.Glow(106, 67, 16, PCol.Hex(0xc8f2ff), 0.4f);
            }

            // A corrente do monoculo, descendo ate a lapela.
            for (int i = 0; i < 12; i++)
            {
                float t = i / 11f;
                float cx = 108 + t * -14f;
                float cy = 77 + t * 56f + (float)Math.Sin(t * Math.PI) * 4f;
                r.Ring(cx, cy, 0.6f, 1.9f, 40f, 1f, Gold, 0.9f);
            }

            r.Rim = PCol.Hex(0xbfe8ff);
        }

        // ------------------------------------------------------------------
        // Distrito 3 e 4
        // ------------------------------------------------------------------

        private static void Miner(PortraitRelief r, bool blink, bool woman)
        {
            PCol body = Brass * new PCol(0.88f, 0.94f, 1.02f);

            // A tranca de cabo da Mineira, caindo nas costas.
            if (woman)
            {
                for (int i = 0; i < 8; i++)
                {
                    float y = 80 + i * 8f;
                    r.Dome(36 - i * 0.8f, y, 6f, 5f, 4f, 3f, i % 2 == 0 ? PCol.Hex(0x3a3a46) : PCol.Hex(0x4a4a58), 0.6f);
                }
            }

            Shoulders(r, 10, 124, 136, body, 0.55f, 22f);
            r.Box(10, 132, 136, 140, 2f, 14f, 1.5f, Leather, 0.2f);
            r.Box(56, 130, 74, 142, 2f, 16f, 2f, Brass, 0.8f);

            // A picareta no ombro.
            r.Tube(94, 168, 132, 112, 3f, 3f, 18f, 2f, Wood, 0.2f);
            r.Poly(new (float, float)[] { (112, 100), (126, 106), (142, 126), (132, 112), (122, 112), (110, 106) }, 20f, 3f, Iron, 0.7f, 2f);

            if (woman)
            {
                // O lenco no pescoco.
                r.Poly(new (float, float)[] { (52, 118), (104, 118), (96, 136), (78, 146), (60, 136) }, 16f, 4f, PCol.Hex(0x3e6a96), 0.2f, 3f);
            }

            r.Speckle(10, 118, 136, 168, 60, 81, PCol.Hex(0x6a6070), 0.45f);

            // A cabeca de latao frio e o capacete com o lampiao.
            Neck(r, 76, 106, 126, DarkIron);
            r.Box(46, 50, 112, 114, 20f, 18f, 8f, body, 0.6f, 9f, 5f);
            r.Speckle(46, 50, 112, 114, 50, 82, PCol.Hex(0x5a5462), 0.4f);
            r.Patina(46, 50, 112, 114, 0.12f, 83, Verdigris);

            r.Dome(78, 54, 42, 24, 20f, 14f, woman ? PCol.Hex(0x8a6a3a) : PCol.Hex(0xb08a3c), 0.6f);
            r.Erase((x, y) => y > 56 && y < 80 && r.Height(x, y) > 20f && r.Height(x, y) < 35f && (x < 44 || x > 116));
            r.Tube(34, 58, 122, 58, 3f, 3f, 30f, 2f, PCol.Hex(0x6a5030), 0.4f);
            r.Seam(78, 32, 78, 56);
            r.Ring(104, 44, 4f, 8f, 36f, 2f, Iron, 0.7f);
            PCol lamp = woman ? PCol.Hex(0xe8fff0) : PCol.Hex(0xfff0b8);
            r.Dome(104, 44, 4.6f, 4.6f, 34f, 0f, lamp, 0f, true);
            r.Glow(108, 44, 26, lamp, 0.55f);

            VisorEyes(r, 76, 70, 114, 86, 30f, blink, EyeCore, EyeGlow);
            r.Rim = PCol.Hex(0xb0d0ff);
        }

        private static void Assistant(PortraitRelief r, bool blink)
        {
            PCol coat = PCol.Hex(0xe2e0d6);

            // O jaleco limpo, com as lapelas e o bolso de canetas.
            Shoulders(r, 12, 124, 134, coat, 0.15f, 24f);
            r.Box(62, 124, 90, 220, 3f, 10f, 2f, PCol.Hex(0x9c5a3a), 0.6f);
            r.Poly(new (float, float)[] { (60, 124), (74, 168), (52, 168), (40, 132) }, 16f, 4f, coat, 0.15f, 3f);
            r.Poly(new (float, float)[] { (92, 124), (78, 168), (100, 168), (112, 132) }, 16f, 4f, coat, 0.15f, 3f);
            r.Box(28, 146, 46, 166, 2f, 16f, 1.5f, coat, 0.15f);
            r.Box(32, 138, 34, 150, 0.5f, 18f, 1f, PCol.Hex(0x3a5ab0), 0.5f);
            r.Box(38, 140, 40, 150, 0.5f, 18f, 1f, PCol.Hex(0xb03a3a), 0.5f);

            // A prancheta.
            r.Box(96, 132, 136, 172, 3f, 22f, 3f, Wood, 0.3f);
            r.Box(100, 138, 132, 172, 1f, 25f, 1f, PCol.Hex(0xf0ead6), 0.1f);
            r.Box(108, 128, 124, 140, 2f, 27f, 2f, Iron, 0.8f);
            for (int i = 0; i < 4; i++)
            {
                int ly = 146 + i * 6;
                r.Tint((x, y) => y == ly && x > 103 && x < 128 - (i % 2) * 6 ? 1f : 0f, PCol.Hex(0x6a6a80));
            }

            r.Dome(98, 152, 8f, 7f, 28f, 4f, Copper, 0.6f);

            // A cabeca de cobre polido e os oculos na testa.
            Neck(r, 76, 106, 126, DarkIron);
            r.Box(46, 40, 112, 114, 22f, 18f, 8f, Copper, 0.8f, 10f, 5f);
            r.Seam(48, 62, 110, 62);
            r.Rivets(52, 104, 106, 104, 5, 1.4f, Copper);

            r.Box(44, 44, 114, 52, 2f, 26f, 1.5f, Leather, 0.2f);
            r.Ring(72, 46, 6f, 10f, 30f, 3f, Brass, 0.9f);
            r.Dome(72, 46, 6.4f, 6.4f, 29f, 2f, PCol.Hex(0x8ad8e0), 0.95f);
            r.Ring(98, 46, 6f, 10f, 30f, 3f, Brass, 0.9f);
            r.Dome(98, 46, 6.4f, 6.4f, 29f, 2f, PCol.Hex(0x8ad8e0), 0.95f);

            VisorEyes(r, 76, 72, 114, 88, 30f, blink, EyeCore, EyeGlow);
            r.Rim = PCol.Hex(0xb8f0e0);
        }

        // ------------------------------------------------------------------
        // O Rei de Prata
        // ------------------------------------------------------------------

        private static readonly string[] Digits925 =
        {
            "111 111 111",
            "1 1   1 1  ",
            "111 111 111",
            "  1 1     1",
            "111 111 111",
        };

        private static List<Action<PixelCanvas>> King(PortraitRelief r, bool blink, bool corrupt)
        {
            var after = new List<Action<PixelCanvas>>();
            PCol silver = corrupt ? PCol.Hex(0xb8c0c8) : Silver;
            PCol velvet = corrupt ? PCol.Hex(0x5a2a2a) : Velvet;
            PCol eye = corrupt ? OxideGlow : PCol.Hex(0xa8e4ff);
            PCol crown = corrupt ? PCol.Hex(0x9cf0c0) : Quartz;

            // O manto de veludo nos ombros, com a barra dourada.
            r.Box(0, 118, 144, 230, 30f, 4f, 7f, velvet, 0.25f, 12f, 6f);
            r.Tint((x, y) => (y > 160 || x < 6 || x > 137) && r.Height(x, y) < 9f ? 1f : 0f, Gold);
            if (corrupt) r.Erase((x, y) => y > 150 && (x < 10 || x > 134) && ArtKit.Hash(x / 3, y / 4, 91) % 3 == 0);

            // O peitoral de prata e a sobreveste vermelha com a marca 925.
            r.Box(30, 126, 114, 230, 16f, 12f, 6f, silver, 0.9f, 8f, 5f);
            r.Box(48, 140, 96, 230, 4f, 16f, 2f, velvet, 0.3f, 3f);
            for (int row = 0; row < Digits925.Length; row++)
            {
                for (int col = 0; col < Digits925[row].Length; col++)
                {
                    if (Digits925[row][col] != '1') continue;
                    int px = 62 + col, py = 150 + row;
                    r.Tint((x, y) => x == px && y == py ? 1f : 0f, Gold);
                }
            }

            // A gola de arminho.
            for (int i = 0; i < 11; i++)
            {
                float x = 22 + i * 10f;
                float y = 124 + (float)Math.Abs(i - 5) * 1.6f;
                r.Dome(x, y, 9f, 8f, 18f, 6f, PCol.Hex(0xf4f0e8), 0.2f);
            }

            r.Speckle(12, 114, 132, 134, 25, 92, PCol.Hex(0x1a1a20), 1f);

            // A corrente de ouro atravessando o peito.
            for (int i = 0; i < 16; i++)
            {
                float t = i / 15f;
                float cx = 34 + t * 76f;
                float cy = 134 + (float)Math.Sin(t * Math.PI) * 10f;
                r.Ring(cx, cy, 0.8f, 2.6f, 26f, 1.5f, Gold, 0.95f);
            }

            // O cetro de ouro com o cristal, na mao, pela direita.
            r.Tube(128, 172, 130, 70, 3f, 3f, 24f, 3f, Gold, 0.9f);
            for (int i = 0; i < 4; i++) r.Dome(129.5f, 150 - i * 22f, 5f, 3.5f, 26f, 3f, Gold, 0.9f);
            r.Poly(new (float, float)[] { (130, 46), (138, 58), (130, 72), (122, 58) }, 28f, 0f, crown, 0f, 1f, true);
            r.Poly(new (float, float)[] { (130, 50), (133, 58), (130, 64), (127, 58) }, 28f, 0f, PCol.Hex(0xffffff), 0f, 1f, true);
            r.Glow(130, 58, 20, crown, 0.5f);
            r.Dome(124, 152, 9f, 8f, 28f, 5f, silver, 0.85f);

            // A cabeca: prata esculpida, nariz reto, barba pontuda.
            Neck(r, 76, 108, 126, PCol.Hex(0x9ca0b0));
            r.Box(50, 44, 110, 116, 16f, 20f, 8f, silver, 0.9f, 10f, 5f);
            r.Poly(new (float, float)[] { (104, 66), (116, 88), (106, 92) }, 28f, 4f, silver, 0.9f, 3f);
            r.Poly(new (float, float)[] { (60, 100), (112, 96), (106, 116), (96, 138), (78, 120) }, 26f, 5f, silver, 0.8f, 3f);
            r.Seam(84, 104, 94, 132);
            r.Seam(96, 102, 100, 126);
            r.Seam(74, 106, 86, 120);
            r.Seam(90, 96, 110, 96);
            r.Box(68, 76, 102, 80, 1f, 30f, 1.5f, silver, 0.9f);

            r.Box(84, 80, 104, 88, 2f, 27f, 1.5f, Visor, 0.9f);
            if (blink) r.Box(88, 83, 101, 85, 0.5f, 30f, 0f, eye * 0.45f, 0f, 0f, 1f, true);
            else
            {
                r.Box(87, 82, 102, 86, 1.5f, 30f, 0f, eye, 0f, 0f, 1f, true);
                r.Box(96, 82, 99, 86, 0.5f, 30f, 0f, PCol.Hex(0xffffff), 0f, 0f, 1f, true);
                r.Glow(95, 84, 14, eye, 0.55f);
            }

            // A coroa: o aro de ouro com as gemas e as pontas de quartzo.
            float[] spikes = { 18, 26, 22, 34, 24, 30, 20 };
            for (int i = 0; i < spikes.Length; i++)
            {
                float x = 52 + i * 9.5f;
                r.Poly(new (float, float)[] { (x - 5, 42), (x, 42 - spikes[i]), (x + 5, 42) }, 22f, 3f, crown, 0.95f, 2.5f, corrupt && i % 2 == 0);
            }

            r.Box(46, 36, 114, 50, 3f, 24f, 4f, Gold, 0.95f, 4f, 3f);
            for (int i = 0; i < 5; i++)
            {
                PCol gem = i % 2 == 0 ? PCol.Hex(0x4a8ae0) : PCol.Hex(0xd04a4a);
                r.Dome(54 + i * 13f, 43, 2.6f, 2.6f, 28f, 2f, gem, 0.95f);
            }

            r.Scratches(46, 36, 116, 120, 10, 93, PCol.Hex(0xffffff), 0.5f);

            if (corrupt)
            {
                // O Oxide tomou a prata: azinhavre por tudo e o Coracao verde no peito.
                r.Patina(0, 30, 144, 168, 0.5f, 94, Verdigris, 0.09f);
                r.Dome(60, 160, 14f, 14f, 22f, 0f, PCol.Hex(0x10201a), 0.2f);
                r.Poly(new (float, float)[] { (60, 148), (70, 160), (60, 172), (50, 160) }, 26f, 0f, OxideGlow, 0f, 1f, true);
                r.Poly(new (float, float)[] { (60, 153), (65, 160), (60, 167), (55, 160) }, 26f, 0f, OxideCore, 0f, 1f, true);
                r.Glow(60, 160, 30, OxideGlow, 0.65f);
                r.Rim = PCol.Hex(0x78ffaa);
                r.RimStrength = 0.5f;

                after.Add(c => ArtKit.Crack(c, 64, 50, 70, 30, 95, 0.9f));
                after.Add(c => ArtKit.Crack(c, 96, 60, 110, 22, 96, 0.9f));
                after.Add(c => ArtKit.Crack(c, 40, 132, 20, 26, 97, 0.8f));
                after.Add(c => ArtKit.Crack(c, 74, 46, -80, 14, 98, 1f));
            }
            else
            {
                // So o comeco: a rachadura verde no peito.
                after.Add(c => ArtKit.Crack(c, 42, 130, 60, 10, 99, 0.6f));
                r.Rim = PCol.Hex(0xd8ecff);
            }

            return after;
        }

        // ------------------------------------------------------------------
        // Reserva
        // ------------------------------------------------------------------

        private static void Generic(PortraitRelief r, bool blink)
        {
            Shoulders(r, 12, 124, 134, Brass, 0.6f);
            r.Rivets(20, 132, 126, 132, 9, 1.5f, Brass);
            r.Patina(12, 124, 134, 168, 0.2f, 101, Verdigris);
            Neck(r, 76, 104, 126, DarkIron);
            r.Box(46, 44, 112, 112, 20f, 18f, 8f, Brass, 0.65f, 9f, 5f);
            r.Seam(48, 62, 110, 62);
            r.Rivets(52, 56, 106, 56, 5, 1.6f, Brass);
            r.Patina(46, 44, 112, 112, 0.18f, 102, Verdigris);
            VisorEyes(r, 76, 70, 114, 86, 30f, blink, EyeCore, EyeGlow);
            r.Rim = PCol.Hex(0xffd8a0);
        }
    }
}
