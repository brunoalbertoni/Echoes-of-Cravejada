using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte de tudo o que fica caido no mundo pra pegar: os fragmentos, o
    /// que os chefes largam (o motor do Coletor, o modulo do pulo de pressao,
    /// a ponta ressonante da broca, a safira, a chave antiga, a coroa), os
    /// reforcos de chassi, as gemas e cofres escondidos, as ferramentas do
    /// ourives, os reagentes do Alambique e a old scrap.
    ///
    /// Tudo a 32 px por tile (PixelsPerUnit), no traco do Scrap e dos chefes:
    /// rampas de quatro tons do ArtKit, rebites, contorno escuro e brilho,
    /// pra ler de longe mesmo pequeno. Nao depende de UnityEngine.
    /// </summary>
    public static class LootArt
    {
        public const int PixelsPerUnit = 32;

        private static readonly Rgba Ink = ArtKit.Outline;
        private static readonly Rgba White = new Rgba(255, 255, 255);

        private static readonly Rgba[] ShardBlue =
            { new Rgba(30, 48, 110), new Rgba(52, 96, 190), new Rgba(96, 156, 240), new Rgba(190, 228, 255) };

        private static readonly Rgba[] Ruby =
            { new Rgba(90, 14, 30), new Rgba(160, 30, 50), new Rgba(222, 70, 80), new Rgba(255, 176, 170) };

        private static readonly Rgba[] Sapphire =
            { new Rgba(22, 34, 104), new Rgba(40, 72, 176), new Rgba(76, 128, 236), new Rgba(170, 206, 255) };

        private static readonly Rgba[] Violet =
            { new Rgba(56, 40, 104), new Rgba(104, 80, 176), new Rgba(160, 140, 232), new Rgba(230, 222, 255) };

        private static readonly Rgba[] Rock =
            { new Rgba(40, 38, 46), new Rgba(70, 66, 76), new Rgba(104, 98, 108), new Rgba(140, 134, 142) };

        private static readonly Rgba[] Mercury =
            { new Rgba(46, 52, 72), new Rgba(98, 108, 134), new Rgba(170, 182, 206), new Rgba(246, 250, 255) };

        private static readonly Rgba[] Quartz =
            { new Rgba(130, 150, 186), new Rgba(186, 206, 232), new Rgba(230, 240, 252), new Rgba(255, 255, 255) };

        private static readonly Rgba[] Wood =
            { new Rgba(72, 42, 26), new Rgba(118, 74, 42), new Rgba(160, 108, 62), new Rgba(204, 152, 96) };

        private static readonly Rgba[] Paper =
            { new Rgba(150, 132, 100), new Rgba(196, 178, 140), new Rgba(226, 212, 176), new Rgba(246, 238, 214) };

        private static readonly Rgba Glass = new Rgba(196, 232, 246, 150);
        private static readonly Rgba GlassEdge = new Rgba(232, 248, 255);

        // ------------------------------------------------------------------
        // Fragmentos e gemas
        // ------------------------------------------------------------------

        /// <summary>
        /// O Opaque Shard, 16x20: um cristal azul lapidado pela metade, a
        /// parte de cima em facetas, a de baixo ainda rocha opaca (e por isso
        /// "opaco"), com um brilho na quina.
        /// </summary>
        public static PixelCanvas Shard()
        {
            var c = new PixelCanvas(16, 20);

            // As facetas: cada uma com seu tom, a luz vindo da esquerda.
            Facet(c, new[] { (8f, 1f), (3f, 7f), (8f, 9f) }, ShardBlue[3]);
            Facet(c, new[] { (8f, 1f), (8f, 9f), (13f, 7f) }, ShardBlue[2]);
            Facet(c, new[] { (3f, 7f), (2f, 12f), (8f, 15f), (8f, 9f) }, ShardBlue[2]);
            Facet(c, new[] { (8f, 9f), (8f, 15f), (14f, 12f), (13f, 7f) }, ShardBlue[1]);

            // A base de rocha opaca, onde o cristal ainda nao foi lapidado.
            ArtKit.Polygon(c, new[] { (2f, 12f), (8f, 15f), (14f, 12f), (13f, 17f), (8f, 19f), (3f, 17f) }, (x, y) =>
                ArtKit.Noise(x * 0.5f, y * 0.5f, 31) > 0.6f ? Rock[2] : (x < 8f ? Rock[1] : Rock[0]));

            ArtKit.Line(c, 8, 2, 8, 14, ShardBlue[3]);
            c.Set(5, 6, White);
            c.Set(6, 5, White);
            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>A gema lapidada escondida, 24x22: um rubi em brilhante, coroa facetada, pavilhao em ponta, estrela de brilho.</summary>
        public static PixelCanvas CutGem() => Brilliant(24, 22, Ruby);

        /// <summary>
        /// A safira bruta que o Capataz guarda, 22x22: um cacho de prismas
        /// azuis saindo de um pedaco de rocha, as pontas brilhando.
        /// </summary>
        public static PixelCanvas SapphireRaw()
        {
            var c = new PixelCanvas(22, 22);

            ArtKit.Ball(c, 11f, 17f, 9f, 4.5f, ArtKit.Corroded(Rock, 0.1f, 77, 0.4f, Rock), -0.1f);
            Prism(c, 7f, 16f, -20f, 9f, 2.6f, Sapphire);
            Prism(c, 14f, 16f, 22f, 8f, 2.4f, Sapphire);
            Prism(c, 11f, 16f, 0f, 13f, 3.2f, Sapphire);
            Prism(c, 17f, 17f, 50f, 5f, 1.8f, Sapphire);

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 10, 4, 1);
            return c;
        }

        // ------------------------------------------------------------------
        // O que os chefes largam
        // ------------------------------------------------------------------

        /// <summary>
        /// O motor de vapor das costas do Coletor, 40x34: a caldeirinha de
        /// cobre enferrujada com cintas, o pistao duplo, o volante dentado ao
        /// lado, a valvula de alivio em cima, os canos arrancados e os
        /// suportes torcidos de onde foi solto. Ainda morno: a fornalha brilha.
        /// </summary>
        public static PixelCanvas ColetorMotor()
        {
            var c = new PixelCanvas(40, 34);

            // Os suportes torcidos atras, presos no corpo dele.
            ArtKit.Limb(c, 6f, 28f, 2f, 20f, 1.4f, ArtKit.Iron);
            ArtKit.Limb(c, 30f, 30f, 36f, 26f, 1.4f, ArtKit.Iron);

            // O volante, atras a direita.
            WreckKit.Gear(c, 30f, 15f, 7f, ArtKit.Iron, 10, 0.2f);
            ArtKit.Ball(c, 30f, 15f, 2f, 2f, ArtKit.Brass, 0.3f);

            // A caldeira deitada: cobre com manchas de ferrugem e azinhavre.
            ArtKit.Drum(c, 16f, 20f, 12f, 9f, 3f, ArtKit.Corroded(ArtKit.Copper, 0.18f, 401, 0.25f), 0.1f);
            foreach (int x in new[] { 8, 16, 24 })
            {
                ArtKit.Line(c, x, 12, x, 28, ArtKit.Iron[1]);
                ArtKit.Line(c, x + 1, 12, x + 1, 28, ArtKit.Iron[3]);
                ArtKit.Rivet(c, x, 14, ArtKit.Brass);
                ArtKit.Rivet(c, x, 25, ArtKit.Brass);
            }

            // A porta da fornalha, com a brasa pela grade.
            c.Fill(10, 19, 8, 6, ArtKit.Iron[0]);
            for (int x = 11; x < 17; x += 2) c.Fill(x, 20, 1, 4, WreckKit.Ember);
            c.Set(13, 22, WreckKit.EmberHot);
            c.Fill(10, 19, 8, 1, ArtKit.Iron[3]);

            // Os pistoes em cima, inclinados, e a biela ate o volante.
            ArtKit.Limb(c, 12f, 11f, 15f, 3f, 2f, ArtKit.Brass);
            ArtKit.Limb(c, 19f, 11f, 22f, 4f, 2f, ArtKit.Brass);
            ArtKit.Limb(c, 22f, 5f, 30f, 15f, 1f, ArtKit.Iron);
            c.Fill(13, 2, 4, 2, ArtKit.Iron[2]);
            c.Fill(20, 3, 4, 2, ArtKit.Iron[2]);

            // A valvula de alivio e o manometro pequeno.
            ArtKit.Limb(c, 6f, 12f, 6f, 6f, 1.2f, ArtKit.Brass);
            ArtKit.Ball(c, 6f, 5f, 2.4f, 1.6f, ArtKit.Copper, 0.2f);
            ArtKit.Ball(c, 25f, 21f, 3f, 3f, ArtKit.Brass, 0.2f);
            ArtKit.Ball(c, 25f, 21f, 1.8f, 1.8f, ArtKit.Plain(Paper), 0.3f);
            c.Set(25, 21, Ink);
            c.Set(26, 20, new Rgba(190, 50, 40));

            // Os canos arrancados, com a boca escura.
            WreckKit.Pipe(c, 4f, 22f, 0.5f, 25f, 1.6f, ArtKit.Copper);
            WreckKit.Pipe(c, 28f, 26f, 33f, 31f, 1.4f, ArtKit.Iron);

            ArtKit.AddOutline(c);
            ArtKit.Glow(c, 14f, 22f, 6f, WreckKit.EmberHot, 0.35f);
            ArtKit.Puff(c, 7f, 2f, 2f, WreckKit.SmokeLight, WreckKit.SmokeDark, 120);
            return c;
        }

        /// <summary>
        /// Um reforco de chassi, 30x30: uma placa de peito de latao abaulada,
        /// emoldurada de ferro, com rebites e um nucleo de cobre no meio (o
        /// "coracao" que segura mais uma pancada).
        /// </summary>
        public static PixelCanvas Chassis()
        {
            var c = new PixelCanvas(30, 30);

            // A moldura de ferro e o escudo de latao por dentro.
            var frame = new[] { (3f, 3f), (27f, 3f), (27f, 17f), (15f, 28f), (3f, 17f) };
            var plate = new[] { (5.5f, 5.5f), (24.5f, 5.5f), (24.5f, 16f), (15f, 25f), (5.5f, 16f) };
            ArtKit.Polygon(c, frame, (x, y) => ArtKit.Tone(ArtKit.Iron, 0.5f - (x + y) / 40f));
            ArtKit.Polygon(c, plate, (x, y) =>
            {
                // Abaulada: clara no alto a esquerda, escura embaixo a direita.
                float light = 0.9f - ((x - 6f) + (y - 6f) * 1.2f) / 24f;
                return ArtKit.Tone(ArtKit.Brass, light);
            });

            // A costura do meio e as linhas de forca.
            ArtKit.Line(c, 15, 6, 15, 24, ArtKit.Brass[1]);
            ArtKit.Line(c, 16, 6, 16, 23, ArtKit.Brass[3]);

            // O nucleo de cobre e o brilho dele.
            ArtKit.Ball(c, 15.5f, 13f, 4.5f, 4.5f, ArtKit.Copper, 0.2f);
            ArtKit.Ball(c, 15.5f, 13f, 2.5f, 2.5f, ArtKit.Plain(new[] { new Rgba(150, 40, 20), new Rgba(220, 90, 40), new Rgba(255, 170, 80), new Rgba(255, 236, 180) }), 0.4f);

            foreach ((int x, int y) in new[] { (5, 5), (23, 5), (5, 15), (23, 15), (14, 25), (10, 21), (19, 21) })
            {
                ArtKit.Rivet(c, x, y, ArtKit.Iron);
            }

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 9, 8, 1);
            return c;
        }

        /// <summary>
        /// O modulo do pulo de pressao que o Mestre das Caldeiras deixa, 30x32:
        /// um cilindro de pressao de latao com cintas, o manometro na frente
        /// (ponteiro no vermelho), a valvula-borboleta em cima e o bocal de
        /// vapor embaixo, soltando um fio branco.
        /// </summary>
        public static PixelCanvas PressureJump()
        {
            var c = new PixelCanvas(30, 32);

            // O bocal embaixo e o vapor saindo.
            ArtKit.Taper(c, 15f, 23f, 15f, 28f, 3f, 5f, ArtKit.Iron);
            ArtKit.Puff(c, 15f, 30.5f, 3f, new Rgba(240, 240, 244), new Rgba(190, 190, 200), 150);

            // O cilindro.
            ArtKit.Drum(c, 15f, 14f, 9f, 10f, 3f, ArtKit.Plain(ArtKit.Brass), 0.05f);
            foreach (int y in new[] { 7, 21 })
            {
                c.Fill(6, y, 18, 2, ArtKit.Iron[1]);
                c.Fill(6, y, 18, 1, ArtKit.Iron[3]);
                ArtKit.Rivet(c, 8, y, ArtKit.Iron);
                ArtKit.Rivet(c, 20, y, ArtKit.Iron);
            }

            // O manometro: aro de cobre, mostrador creme, faixa vermelha e ponteiro no talo.
            ArtKit.Ball(c, 15f, 14.5f, 5f, 5f, ArtKit.Copper, 0.2f);
            ArtKit.Ball(c, 15f, 14.5f, 3.6f, 3.6f, ArtKit.Plain(Paper), 0.4f);
            c.Set(17, 12, new Rgba(200, 40, 36));
            c.Set(18, 13, new Rgba(200, 40, 36));
            c.Set(18, 14, new Rgba(200, 40, 36));
            ArtKit.Line(c, 15, 15, 17, 12, Ink);
            c.Set(15, 15, ArtKit.Brass[0]);

            // A valvula de cima: a haste e o volantinho vermelho.
            ArtKit.Limb(c, 15f, 4f, 15f, 1f, 1f, ArtKit.Brass);
            c.Fill(10, 0, 11, 2, new Rgba(170, 40, 36));
            c.Fill(10, 0, 11, 1, new Rgba(230, 90, 70));

            // Os encaixes laterais (onde prende no tornozelo).
            c.Fill(2, 11, 4, 6, ArtKit.Iron[1]);
            c.Fill(24, 11, 4, 6, ArtKit.Iron[1]);
            c.Fill(2, 11, 4, 1, ArtKit.Iron[3]);
            c.Fill(24, 11, 4, 1, ArtKit.Iron[3]);

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 9, 9, 1);
            return c;
        }

        /// <summary>
        /// A ponta ressonante que o Geode Drill deixa, 32x30: a cabeca de
        /// cinzel de aco cravada de cristais violeta, presa numa virola de
        /// latao, com as ondas do zumbido em volta.
        /// </summary>
        public static PixelCanvas ResonanceStrike()
        {
            var c = new PixelCanvas(32, 30);

            // As ondas de ressonancia, translucidas, em volta da lamina.
            for (int r = 0; r < 2; r++)
            {
                float radius = 10f + r * 3.5f;
                for (int a = 200; a < 340; a += 6)
                {
                    (float x, float y) = ArtKit.Along(19f, 16f, a, radius);
                    c.Blend((int)x, (int)y, Violet[3].WithAlpha(r == 0 ? 170 : 100));
                }
            }

            // A espiga e a virola de latao, embaixo a esquerda.
            ArtKit.Limb(c, 4f, 27f, 10f, 21f, 2.6f, Wood);
            ArtKit.Limb(c, 9f, 22f, 12f, 19f, 3.4f, ArtKit.Brass);

            // A lamina de aco, larga, com o fio claro.
            ArtKit.Polygon(c, new[] { (11f, 18f), (24f, 3f), (29f, 6f), (15f, 22f) }, (x, y) =>
                x + y * 0.2f < 20f ? ArtKit.Tin[3] : ArtKit.Tin[2]);
            ArtKit.Line(c, 24, 3, 29, 6, White);
            ArtKit.Line(c, 13, 20, 26, 6, ArtKit.Tin[1]);

            // Os cristais cravados na lamina.
            foreach ((float x, float y) in new[] { (17f, 14f), (21f, 10f), (25f, 6.5f) })
            {
                Facet(c, new[] { (x, y - 2.5f), (x - 2f, y), (x, y + 2.5f), (x + 2f, y) }, Violet[2]);
                c.Set((int)x - 1, (int)y - 1, Violet[3]);
                c.Set((int)x + 1, (int)y + 1, Violet[1]);
            }

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 28, 4, 2);
            return c;
        }

        /// <summary>
        /// O modulo do Steam Dash, 30x24: dois pistoes de latao lado a lado
        /// presos numa cinta de couro, com as bocas de escape atras.
        /// </summary>
        public static PixelCanvas DashModule()
        {
            var c = new PixelCanvas(30, 24);

            ArtKit.Limb(c, 3f, 12f, 27f, 12f, 3f, ArtKit.Leather);
            foreach (float x in new[] { 9f, 20f })
            {
                ArtKit.Drum(c, x, 10f, 4.5f, 8f, 4f, ArtKit.Plain(ArtKit.Brass), 0.05f);
                c.Fill((int)x - 4, 4, 9, 1, ArtKit.Iron[1]);
                c.Fill((int)x - 4, 15, 9, 1, ArtKit.Iron[1]);
                ArtKit.Taper(c, x, 18f, x, 22f, 2.4f, 3.4f, ArtKit.Iron);
                ArtKit.Ball(c, x, 22.5f, 2f, 0.9f, ArtKit.Plain(new[] { WreckKit.Soot, WreckKit.Soot, ArtKit.Iron[0], ArtKit.Iron[0] }));
                ArtKit.Rivet(c, (int)x - 1, 9, ArtKit.Iron);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// A chave antiga do Capataz, 32x18: ferro velho com a argola
        /// trabalhada (um cristal azul no meio), haste com aneis e o palhetao
        /// de dentes.
        /// </summary>
        public static PixelCanvas AncientKey()
        {
            var c = new PixelCanvas(32, 18);

            // A argola: aro grosso, com quatro lobulos.
            for (int i = 0; i < 4; i++)
            {
                (float x, float y) = ArtKit.Along(8f, 9f, i * 90f + 45f, 4.5f);
                ArtKit.Ball(c, x, y, 2.6f, 2.6f, ArtKit.Corroded(ArtKit.Iron, 0.12f, 11 + i, 0.5f, ArtKit.Rust), 0.2f);
            }

            ArtKit.Ball(c, 8f, 9f, 5f, 5f, ArtKit.Corroded(ArtKit.Iron, 0.1f, 15, 0.5f, ArtKit.Rust), 0.2f);
            ArtKit.Ball(c, 8f, 9f, 2.6f, 2.6f, ArtKit.Plain(Sapphire), 0.4f);

            // A haste, com dois aneis.
            ArtKit.Limb(c, 12f, 9f, 27f, 9f, 1.6f, ArtKit.Corroded(ArtKit.Iron, 0.1f, 19, 0.5f, ArtKit.Rust));
            foreach (int x in new[] { 15, 18 })
            {
                c.Fill(x, 6, 2, 7, ArtKit.Brass[2]);
                c.Fill(x, 6, 1, 7, ArtKit.Brass[3]);
            }

            // O palhetao: os dentes pra baixo.
            c.Fill(23, 10, 7, 5, ArtKit.Iron[2]);
            c.Fill(23, 10, 7, 1, ArtKit.Iron[3]);
            c.Set(25, 14, Rgba.Clear);
            c.Set(25, 13, Rgba.Clear);
            c.Set(28, 14, Rgba.Clear);
            c.Fill(23, 14, 1, 2, ArtKit.Iron[1]);
            c.Fill(29, 14, 1, 3, ArtKit.Iron[1]);

            ArtKit.AddOutline(c);
            c.Set(7, 8, White);
            return c;
        }

        /// <summary>
        /// A coroa de quartzo do Rei, caida, 48x30: o aro de ouro rachado e
        /// meio tombado, as pontas de quartzo (uma partida, caida do lado), as
        /// pedras do aro e um brilho.
        /// </summary>
        public static PixelCanvas FallenCrown()
        {
            var c = new PixelCanvas(48, 30);

            // As pontas de quartzo, de tamanhos diferentes, inclinadas com a coroa.
            float[] heights = { 10f, 14f, 18f, 13f, 9f };
            for (int i = 0; i < heights.Length; i++)
            {
                float bx = 9f + i * 7f;
                float lean = -8f + i * 2f;
                if (i == 3) continue; // a partida
                (float tx, float ty) = ArtKit.Along(bx, 19f, -90f + lean, heights[i]);
                ArtKit.Polygon(c, new[] { (bx - 3.2f, 19.5f), (tx, ty), (bx + 3.2f, 19.5f) }, (x, y) =>
                    x < tx ? Quartz[2] : (x < bx + 1.5f ? Quartz[1] : Quartz[0]));
                ArtKit.Line(c, (int)tx, (int)ty + 1, (int)bx, 19, Quartz[3]);
            }

            // O toco da ponta partida e o pedaco dela caido ao lado.
            ArtKit.Polygon(c, new[] { (27.8f, 19.5f), (30f, 14f), (32f, 15.5f), (34.2f, 19.5f) }, (x, y) => Quartz[1]);
            ArtKit.Polygon(c, new[] { (38f, 28f), (46f, 23f), (46f, 27f) }, (x, y) => y < 25.5f ? Quartz[2] : Quartz[0]);

            // O aro de ouro, com a rachadura e as pedras.
            ArtKit.Limb(c, 4f, 22f, 42f, 21f, 3.6f, ArtKit.Brass);
            ArtKit.Line(c, 4, 19, 42, 18, ArtKit.Brass[3]);
            foreach (float x in new[] { 10f, 23f, 36f })
            {
                ArtKit.Ball(c, x, 21.5f, 2.2f, 2.2f, ArtKit.Plain(Ruby), 0.4f);
            }

            ArtKit.Line(c, 30, 18, 28, 25, Ink);
            ArtKit.Line(c, 31, 18, 29, 25, ArtKit.Brass[0]);

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 22, 5, 2);
            c.Set(10, 20, White);
            return c;
        }

        // ------------------------------------------------------------------
        // Esconderijos
        // ------------------------------------------------------------------

        /// <summary>
        /// Um cofre de fragmentos, 28x24: uma arca pequena de latao com cintas
        /// de ferro e fechadura, a tampa entreaberta e os fragmentos azuis
        /// transbordando pela fresta, brilhando.
        /// </summary>
        public static PixelCanvas ShardCache()
        {
            var c = new PixelCanvas(28, 24);

            // A caixa.
            ArtKit.Polygon(c, new[] { (2f, 11f), (26f, 11f), (25f, 22f), (3f, 22f) }, (x, y) =>
                ArtKit.Tone(ArtKit.Brass, 0.6f - (x - 2f) / 30f - (y - 11f) / 20f));
            c.Fill(3, 21, 22, 1, ArtKit.Brass[0]);
            foreach (int x in new[] { 7, 19 })
            {
                c.Fill(x, 11, 3, 11, ArtKit.Iron[1]);
                c.Fill(x, 11, 1, 11, ArtKit.Iron[3]);
                ArtKit.Rivet(c, x + 1, 14, ArtKit.Iron);
                ArtKit.Rivet(c, x + 1, 19, ArtKit.Iron);
            }

            // A fechadura.
            c.Fill(12, 13, 4, 5, ArtKit.Iron[2]);
            c.Fill(13, 15, 2, 2, Ink);

            // Os fragmentos saindo pela fresta (atras da tampa).
            foreach ((float x, float y) in new[] { (7f, 9f), (12f, 8f), (17f, 9.5f), (21f, 8.5f), (14.5f, 10f) })
            {
                Facet(c, new[] { (x, y - 3f), (x - 2.4f, y), (x, y + 2f), (x + 2.4f, y) }, ShardBlue[2]);
                c.Set((int)x - 1, (int)y - 1, ShardBlue[3]);
                c.Set((int)x + 1, (int)y + 1, ShardBlue[1]);
            }

            // A tampa entreaberta, inclinada pra tras.
            ArtKit.Polygon(c, new[] { (2f, 7f), (25f, 3f), (27f, 6f), (3f, 10f) }, (x, y) =>
                y < 6f ? ArtKit.Brass[3] : ArtKit.Brass[2]);
            ArtKit.Line(c, 3, 10, 27, 6, ArtKit.Brass[0]);
            ArtKit.Line(c, 8, 6, 9, 9, ArtKit.Iron[2]);
            ArtKit.Line(c, 20, 4, 21, 7, ArtKit.Iron[2]);

            ArtKit.AddOutline(c);
            ArtKit.Glow(c, 14f, 9f, 9f, ShardBlue[3], 0.4f, true);
            ArtKit.Spark(c, 12, 4, 1);
            return c;
        }

        // ------------------------------------------------------------------
        // As ferramentas do ourives
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma ferramenta do mestre do Rebite, 24x24, desenhada grande o
        /// bastante pra se reconhecer no chao: 0 o buril (cabo de cogumelo de
        /// madeira, virola de latao e a lamina em bisel), 1 a lupa (aro de
        /// latao gravado, vidro com reflexo e cabo torneado), 2 a pinca (duas
        /// hastes de aco unidas em cima, ponta fina e a mola).
        /// </summary>
        public static PixelCanvas MasterTool(int kind)
        {
            var c = new PixelCanvas(24, 24);

            switch (kind)
            {
                case 0:
                    // O cabo de cogumelo, embaixo a esquerda.
                    ArtKit.Ball(c, 6f, 18f, 5f, 4.2f, ArtKit.Plain(Wood), 0.2f);
                    for (int i = 0; i < 3; i++) c.Set(4 + i * 2, 20, Wood[0]);
                    ArtKit.Limb(c, 8f, 16f, 11f, 13f, 2.2f, ArtKit.Brass);
                    ArtKit.Line(c, 9, 13, 11, 15, ArtKit.Brass[0]);

                    // A lamina em diagonal, com a face do bisel na ponta.
                    ArtKit.Limb(c, 11f, 13f, 20f, 4f, 1.4f, ArtKit.Tin);
                    ArtKit.Polygon(c, new[] { (18f, 4f), (21f, 1f), (22f, 4f), (20f, 6f) }, (x, y) => ArtKit.Tin[3]);
                    ArtKit.Line(c, 12, 12, 20, 4, White.WithAlpha(200));
                    break;

                case 1:
                    // O aro gravado e o vidro.
                    for (int py = 0; py < 24; py++)
                    {
                        for (int px = 0; px < 24; px++)
                        {
                            float dx = px + 0.5f - 10f;
                            float dy = py + 0.5f - 10f;
                            float r = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (r <= 5.6f) c.Blend(px, py, (dx + dy < -3f) ? new Rgba(220, 244, 252) : new Rgba(160, 210, 230));
                            else if (r <= 8f)
                            {
                                bool notch = (int)(Math.Atan2(dy, dx) * 6f) % 2 == 0 && r > 7f;
                                c.Set(px, py, notch ? ArtKit.Brass[1] : (dx + dy < 0f ? ArtKit.Brass[3] : ArtKit.Brass[2]));
                            }
                        }
                    }

                    // O reflexo no vidro, em arco.
                    for (int a = 200; a < 260; a += 12)
                    {
                        (float x, float y) = ArtKit.Along(10f, 10f, a, 4f);
                        c.Set((int)x, (int)y, White);
                    }

                    // O cabo torneado.
                    ArtKit.Limb(c, 15f, 15f, 17f, 17f, 2f, ArtKit.Brass);
                    ArtKit.Limb(c, 17f, 17f, 22f, 22f, 2.2f, Wood);
                    c.Set(19, 19, Wood[0]);
                    c.Set(20, 21, Wood[3]);
                    break;

                default:
                    // As duas hastes, unidas em cima e se fechando na ponta.
                    ArtKit.Taper(c, 11f, 2f, 6f, 12f, 1.6f, 1.3f, ArtKit.Tin);
                    ArtKit.Taper(c, 6f, 12f, 10f, 22f, 1.3f, 0.6f, ArtKit.Tin);
                    ArtKit.Taper(c, 13f, 2f, 18f, 12f, 1.6f, 1.3f, ArtKit.Iron);
                    ArtKit.Taper(c, 18f, 12f, 12f, 22f, 1.3f, 0.6f, ArtKit.Iron);
                    ArtKit.Ball(c, 12f, 2.5f, 3f, 2.2f, ArtKit.Brass, 0.3f);

                    // As ranhuras de pegada.
                    for (int i = 0; i < 3; i++)
                    {
                        c.Set(7 + i / 2, 9 + i, ArtKit.Tin[0]);
                        c.Set(17 - i / 2, 9 + i, ArtKit.Iron[0]);
                    }

                    // Um grao de gema preso na ponta.
                    c.Set(11, 22, Ruby[2]);
                    c.Set(11, 21, Ruby[3]);
                    break;
            }

            ArtKit.AddOutline(c);
            return c;
        }

        // ------------------------------------------------------------------
        // Os reagentes do Alambique
        // ------------------------------------------------------------------

        /// <summary>
        /// Um reagente, 24x32: 0 o mercurio destilado (balao redondo, metal
        /// liquido com reflexo, rolha lacrada e etiqueta), 1 a luz engarrafada
        /// (garrafa alta acesa por dentro, raios em volta), 2 o sal de quartzo
        /// (pote largo de boca larga com cristais rosados).
        /// </summary>
        public static PixelCanvas Reagent(int index)
        {
            var c = new PixelCanvas(24, 32);

            switch (index)
            {
                case 0:
                {
                    // O balao, com o mercurio pesado no fundo.
                    Flask(c, 12f, 21f, 8.5f, 8.5f, 9, 4, 6);
                    for (int y = 19; y < 30; y++)
                    {
                        for (int x = 4; x < 21; x++)
                        {
                            float dx = (x + 0.5f - 12f) / 7.5f;
                            float dy = (y + 0.5f - 21f) / 7.5f;
                            if (dx * dx + dy * dy > 1f) continue;
                            float light = -0.8f * dx - 0.4f * dy + (y == 19 ? 0.6f : 0f);
                            c.Set(x, y, ArtKit.Tone(Mercury, light));
                        }
                    }

                    c.Fill(7, 19, 10, 1, Mercury[3]);
                    Label(c, 9, 13, 6, 4);
                    break;
                }

                case 1:
                {
                    // A garrafa alta, acesa: o brilho por dentro e os raios.
                    var lit = new Rgba(255, 238, 150);
                    var core = new Rgba(255, 252, 226);
                    ArtKit.Glow(c, 12f, 19f, 11f, lit, 0.9f, true);
                    for (int i = 0; i < 8; i++)
                    {
                        (float x, float y) = ArtKit.Along(12f, 19f, i * 45f + 22f, 10f);
                        (float x2, float y2) = ArtKit.Along(12f, 19f, i * 45f + 22f, 12f);
                        ArtKit.Line(c, (int)x, (int)y, (int)x2, (int)y2, lit.WithAlpha(200));
                    }

                    ArtKit.Drum(c, 12f, 20f, 6f, 9f, 3f, ArtKit.Plain(new[] { new Rgba(230, 170, 60), new Rgba(255, 210, 100), lit, core }), 0.3f);
                    ArtKit.Ball(c, 12f, 20f, 2.6f, 3.4f, ArtKit.Plain(new[] { lit, core, core, White }), 0.5f);
                    c.Fill(10, 6, 4, 6, new Rgba(240, 200, 110));
                    c.Fill(10, 6, 1, 6, core);
                    c.Fill(9, 3, 6, 3, ArtKit.Brass[2]);
                    c.Fill(9, 3, 6, 1, ArtKit.Brass[3]);
                    c.Fill(9, 5, 6, 1, ArtKit.Brass[0]);
                    break;
                }

                default:
                {
                    // O pote largo, de vidro, e os cristais de sal dentro.
                    Flask(c, 12f, 22f, 9f, 7f, 7, 10, 3);
                    foreach ((float x, float h, float lean) in new[] { (7f, 7f, -15f), (11f, 10f, -3f), (15f, 8f, 10f), (18f, 5f, 25f), (9f, 5f, 30f) })
                    {
                        (float tx, float ty) = ArtKit.Along(x, 28f, -90f + lean, h);
                        ArtKit.Polygon(c, new[] { (x - 2f, 28f), (tx, ty), (x + 2f, 28f) }, (px, py) =>
                            px < tx ? new Rgba(255, 236, 248) : new Rgba(226, 170, 214));
                    }

                    c.Fill(5, 27, 14, 2, new Rgba(240, 210, 236));
                    c.Fill(5, 13, 14, 2, Wood[2]);
                    c.Fill(5, 13, 14, 1, Wood[3]);
                    Label(c, 8, 20, 8, 3);
                    break;
                }
            }

            ArtKit.AddOutline(c);
            c.Set(7, 16, White);
            return c;
        }

        // ------------------------------------------------------------------
        // A old scrap
        // ------------------------------------------------------------------

        /// <summary>
        /// A old scrap, 32x24: o que fica onde o Scrap morreu — um montinho
        /// das placas enferrujadas dele (o capacete de cima, com o olho
        /// apagado), parafusos e, espetado no meio, o fragmento que guarda o
        /// que ele carregava, brilhando pra ser achado.
        /// </summary>
        public static PixelCanvas OldScrap()
        {
            var c = new PixelCanvas(32, 24);
            Rgba[][] ramps = { ArtKit.Rust, ArtKit.Iron, ArtKit.Rust, ArtKit.Verdigris };

            WreckKit.Rubble(c, 2, 29, 22, 7, ramps, 1201, 0.8f);

            // O capacete do Scrap, de lado, com o olho apagado.
            ArtKit.Ball(c, 21f, 16f, 6f, 5f, ArtKit.Corroded(ArtKit.Rust, 0.22f, 1202, 0.35f), 0.1f);
            c.Fill(15, 18, 12, 2, ArtKit.Rust[0]);
            ArtKit.Ball(c, 23f, 15.5f, 1.8f, 1.8f, ArtKit.Plain(new[] { new Rgba(30, 30, 34), new Rgba(46, 46, 52), new Rgba(70, 70, 78), new Rgba(96, 96, 104) }));
            ArtKit.Rivet(c, 18, 13, ArtKit.Rust);

            // O fragmento espetado.
            PixelCanvas shard = Shard();
            c.Blit(shard, 6, 0);

            ArtKit.AddOutline(c);
            ArtKit.Glow(c, 14f, 7f, 8f, ShardBlue[3], 0.35f, true);
            return c;
        }

        // ------------------------------------------------------------------
        // Pecas
        // ------------------------------------------------------------------

        private static void Facet(PixelCanvas c, (float X, float Y)[] points, Rgba color)
        {
            ArtKit.Polygon(c, points, (x, y) => color);
        }

        /// <summary>Uma gema em brilhante: a mesa no alto, a coroa em facetas, o pavilhao afinando ate a ponta.</summary>
        private static PixelCanvas Brilliant(int w, int h, Rgba[] ramp)
        {
            var c = new PixelCanvas(w, h);
            float cx = w / 2f;
            float girdle = h * 0.38f;

            // A coroa (em cima): mesa clara e facetas laterais.
            Facet(c, new[] { (2f, girdle), (6f, 2f), (cx, 2f), (cx - 2f, girdle) }, ramp[2]);
            Facet(c, new[] { (cx - 2f, girdle), (cx, 2f), (w - 6f, 2f), (cx + 2f, girdle) }, ramp[3]);
            Facet(c, new[] { (cx + 2f, girdle), (w - 6f, 2f), (w - 2f, girdle) }, ramp[1]);

            // O pavilhao (embaixo), em tres faixas que se juntam na ponta.
            Facet(c, new[] { (2f, girdle), (cx - 2f, girdle), (cx, h - 1f) }, ramp[2]);
            Facet(c, new[] { (cx - 2f, girdle), (cx + 2f, girdle), (cx, h - 1f) }, ramp[1]);
            Facet(c, new[] { (cx + 2f, girdle), (w - 2f, girdle), (cx, h - 1f) }, ramp[0]);

            // A cintura e as arestas claras.
            ArtKit.Line(c, 2, (int)girdle, w - 3, (int)girdle, ramp[3]);
            ArtKit.Line(c, 6, 2, (int)cx - 2, (int)girdle, ramp[3]);

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, (int)cx + 3, 4, 2);
            return c;
        }

        /// <summary>Um prisma de cristal: corpo de duas faces e a ponta chanfrada.</summary>
        private static void Prism(PixelCanvas c, float x, float y, float lean, float length, float half, Rgba[] ramp)
        {
            (float tx, float ty) = ArtKit.Along(x, y, -90f + lean, length);
            (float nx, float ny) = ArtKit.Along(0f, 0f, lean, half);
            (float sx, float sy) = ArtKit.Along(x, y, -90f + lean, length - half * 1.5f);

            Facet(c, new[] { (x - nx, y - ny), (sx - nx, sy - ny), (tx, ty), (sx, sy), (x, y) }, ramp[2]);
            Facet(c, new[] { (x, y), (sx, sy), (tx, ty), (sx + nx, sy + ny), (x + nx, y + ny) }, ramp[1]);
            ArtKit.Line(c, (int)(x - nx * 0.5f), (int)(y - ny * 0.5f), (int)tx, (int)ty, ramp[3]);
        }

        /// <summary>Um frasco de vidro: corpo redondo (rx, ry), gargalo e a rolha lacrada.</summary>
        private static void Flask(PixelCanvas c, float cx, float cy, float rx, float ry, int neckTop, int neckWidth, int neckHeight)
        {
            for (int y = (int)(cy - ry) - 1; y <= (int)(cy + ry) + 1; y++)
            {
                for (int x = (int)(cx - rx) - 1; x <= (int)(cx + rx) + 1; x++)
                {
                    float dx = (x + 0.5f - cx) / rx;
                    float dy = (y + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    c.Blend(x, y, d > 0.8f ? GlassEdge : Glass);
                }
            }

            int nx = (int)(cx - neckWidth / 2f);
            c.Fill(nx, neckTop, neckWidth, neckHeight, Glass);
            c.Fill(nx, neckTop, 1, neckHeight, GlassEdge);
            c.Fill(nx + neckWidth - 1, neckTop, 1, neckHeight, GlassEdge);

            // A rolha de cortica e o lacre vermelho.
            c.Fill(nx, neckTop - 3, neckWidth, 3, Wood[2]);
            c.Fill(nx, neckTop - 3, neckWidth, 1, Wood[3]);
            c.Fill(nx - 1, neckTop - 1, neckWidth + 2, 1, new Rgba(170, 40, 40));

            // O reflexo de janela no vidro.
            c.Set((int)(cx - rx * 0.5f), (int)(cy - ry * 0.4f), White);
            c.Set((int)(cx - rx * 0.5f), (int)(cy - ry * 0.4f) + 1, GlassEdge);
        }

        /// <summary>A etiqueta de papel, com riscos de letra.</summary>
        private static void Label(PixelCanvas c, int x, int y, int w, int h)
        {
            c.Fill(x, y, w, h, Paper[2]);
            c.Fill(x, y + h - 1, w, 1, Paper[0]);
            for (int i = 0; i < w - 2; i += 2) c.Set(x + 1 + i, y + 1, Paper[0]);
            if (h > 3) for (int i = 0; i < w - 3; i += 3) c.Set(x + 1 + i, y + 2, Paper[0]);
        }
    }
}
