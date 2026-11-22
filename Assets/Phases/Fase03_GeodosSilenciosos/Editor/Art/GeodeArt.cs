using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O resto da arte do District 3 — a segunda leva, no nivel das ultimas
    /// fases: a Shard Sentinel e a Geode Drill animadas, o cristal ressonante
    /// com a base de pedra, o portao de caverna, as placas (a tabua do turno,
    /// a placa da Coroa), o bilhete, o capacete e a ambientacao de mina
    /// (escoramento de madeira, trilho, vagonete).
    ///
    /// Os bichos olham pra direita, com os pes na ultima linha. Nao depende
    /// de UnityEngine.
    /// </summary>
    public static class GeodeArt
    {
        private static readonly Rgba[] Amethyst =
            { new Rgba(70, 40, 120), new Rgba(128, 82, 190), new Rgba(186, 142, 236), new Rgba(236, 216, 255) };

        private static readonly Rgba[] Milky =
            { new Rgba(130, 138, 168), new Rgba(190, 198, 224), new Rgba(228, 234, 248), new Rgba(255, 255, 255) };

        private static readonly Rgba[] Stone =
            { new Rgba(40, 38, 56), new Rgba(64, 60, 84), new Rgba(94, 88, 118), new Rgba(132, 126, 160) };

        private static readonly Rgba[] Wood =
            { new Rgba(46, 30, 24), new Rgba(76, 50, 34), new Rgba(108, 74, 48), new Rgba(140, 100, 66) };

        private static readonly Rgba[] Paper =
            { new Rgba(150, 138, 104), new Rgba(186, 174, 138), new Rgba(214, 204, 168), new Rgba(236, 228, 198) };

        private static readonly Rgba Ink = new Rgba(58, 52, 74);
        private static readonly Rgba Dark = new Rgba(18, 14, 24);

        // ------------------------------------------------------------------
        // The Geode Drill, 80x48
        // ------------------------------------------------------------------

        public const int DrillFrames = 4;

        /// <summary>
        /// A broca: uma maquina de mineracao sobre esteiras, de latao comido de
        /// Oxide. Na frente, a ponta em espiral de cristal (as roscas giram de
        /// quadro em quadro); no meio, a cabine vazia, de vidro rachado; atras,
        /// o motor exposto, com os veios verdes acesos e fumaca saindo. As
        /// esteiras andam.
        /// </summary>
        public static PixelCanvas Drill(int frame)
        {
            var c = new PixelCanvas(80, 48);
            Rgba[] metal = ArtKit.Brass;

            // As esteiras: um laco comprido, com as sapatas andando.
            for (int x = 6; x < 62; x++)
            {
                for (int y = 38; y < 47; y++)
                {
                    bool rim = y == 38 || y == 46;
                    bool shoe = (x + frame * 2) % 4 == 0;
                    c.Set(x, y, rim ? ArtKit.Iron[0] : (shoe ? ArtKit.Iron[1] : ArtKit.Iron[2]));
                }
            }

            for (int i = 0; i < 6; i++)
            {
                float wx = 11f + i * 9.5f;
                ArtKit.Ball(c, wx, 42.5f, 3.4f, 3.4f, ArtKit.Iron, 0.2f);
                int spoke = (frame + i) % 2;
                c.Set((int)wx, 41 + spoke, ArtKit.Iron[0]);
                c.Set((int)wx - 1 + spoke * 2, 42, ArtKit.Iron[0]);
            }

            // O chassi.
            ArtKit.Drum(c, 34f, 31f, 30f, 7f, 6f, ArtKit.Corroded(metal, 0.3f, 4101), 0f);
            for (int x = 8; x < 62; x += 6) ArtKit.Rivet(c, x, 27, metal);

            // O motor, atras: um bloco com grade e os veios de Oxide acesos.
            ArtKit.Drum(c, 14f, 20f, 10f, 9f, 5f, ArtKit.Corroded(ArtKit.Iron, 0.4f, 4102), 0f);
            for (int y = 14; y < 27; y += 3) ArtKit.Line(c, 6, y, 21, y, ArtKit.Iron[0]);
            ArtKit.Crack(c, 9f, 15f, 70f, 12, 4103 + frame, 0.9f);
            ArtKit.Crack(c, 17f, 14f, 100f, 10, 4104, 0.8f);

            // O escapamento e a fumaca (que sobe de quadro em quadro).
            c.Fill(8, 4, 3, 8, ArtKit.Iron[1]);
            c.Fill(8, 4, 1, 8, ArtKit.Iron[3]);
            var smokeLight = new Rgba(120, 116, 140);
            var smokeDark = new Rgba(70, 66, 86);
            ArtKit.Puff(c, 10f, 3f - frame * 0.7f, 2.6f + frame * 0.3f, smokeLight, smokeDark, 150 - frame * 25);

            // A cabine: vazia, o vidro rachado, o banco sem ninguem.
            ArtKit.Drum(c, 34f, 16f, 9f, 9f, 5f, ArtKit.Corroded(metal, 0.25f, 4105), 0.1f);
            c.Fill(29, 11, 11, 7, new Rgba(40, 56, 72));
            for (int i = 0; i < 4; i++) c.Set(30 + i * 2, 12 + i, new Rgba(120, 150, 170));
            ArtKit.Line(c, 33, 11, 37, 17, new Rgba(160, 190, 210));
            c.Fill(31, 15, 4, 2, ArtKit.Leather[1]);

            // O braco da broca e a ponta em espiral de cristal.
            ArtKit.Limb(c, 50f, 28f, 60f, 26f, 4f, ArtKit.Corroded(metal, 0.2f, 4106));
            ArtKit.Polygon(c, new[] { (58f, 17f), (79f, 26f), (58f, 35f) }, (x, y) =>
                ArtKit.Tone(Milky, (26f - y) / 9f * 0.6f - (x - 58f) / 21f * 0.3f));

            // As roscas, deslizando com o giro.
            for (int i = 0; i < 6; i++)
            {
                float t = ((i * 4 + frame) % 22) / 22f;
                float x = 58f + t * 21f;
                float half = 9f * (1f - t);
                ArtKit.Line(c, (int)x, (int)(26f - half), (int)(x + 2f), (int)(26f + half), Amethyst[0]);
            }

            ArtKit.AddOutline(c);

            // Os veios verdes brilham por cima do contorno.
            ArtKit.Glow(c, 13f, 20f, 8f, ArtKit.OxideGlow, 0.35f + (frame % 2) * 0.1f, true);
            return c;
        }

        // ------------------------------------------------------------------
        // O cristal ressonante, 20x32, e a base, 24x8
        // ------------------------------------------------------------------

        /// <summary>
        /// O cristal ressonante: um prisma alto, facetado, em tons de cinza
        /// claro — quem pinta ele da cor da nota e o script. Dois cristais
        /// menores encostados. A base de pedra e um desenho a parte, que nao
        /// leva a cor.
        /// </summary>
        public static PixelCanvas Resonant()
        {
            var c = new PixelCanvas(20, 32);
            var grey = new[] { new Rgba(120, 120, 130), new Rgba(176, 176, 186), new Rgba(220, 220, 228), new Rgba(255, 255, 255) };

            ArtKit.Taper(c, 6f, 31f, 4f, 16f, 2.6f, 0.4f, grey, 0f);
            ArtKit.Taper(c, 15f, 31f, 17f, 19f, 2.4f, 0.4f, grey, -0.2f);

            // O prisma grande, de faces marcadas.
            ArtKit.Polygon(c, new[] { (6f, 31f), (14f, 31f), (14f, 8f), (10f, 1f), (6f, 8f) }, (x, y) =>
            {
                if (x < 8f) return grey[3];
                if (x < 10f) return grey[2];
                if (x < 12f) return grey[1];
                return grey[0];
            });

            // Riscos de ressonancia: linhas finas atravessando.
            for (int y = 10; y < 30; y += 6) ArtKit.Line(c, 7, y, 13, y - 2, grey[1]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>A base de pedra lavrada onde o ressonante esta cravado, 24x8.</summary>
        public static PixelCanvas ResonantBase()
        {
            var c = new PixelCanvas(24, 8);

            for (int y = 0; y < 8; y++)
            {
                int inset = y < 2 ? 3 : 1;
                for (int x = inset; x < 24 - inset; x++)
                {
                    float light = y == 0 ? 0.8f : (x < 5 ? 0.3f : (x > 18 ? -0.6f : -0.1f));
                    c.Set(x, y, ArtKit.Tone(Stone, light));
                }
            }

            // As marcas de afinacao gravadas: tres riscos.
            for (int i = 0; i < 3; i++) ArtKit.Line(c, 8 + i * 3, 4, 9 + i * 3, 6, Stone[0]);

            ArtKit.AddOutline(c);
            return c;
        }

        // ------------------------------------------------------------------
        // O portao de caverna, 16x16 emendavel
        // ------------------------------------------------------------------

        /// <summary>Barras de ferro velho com uma travessa rebitada e cristal crescendo nelas.</summary>
        public static PixelCanvas CaveGate()
        {
            var c = new PixelCanvas(16, 16);

            for (int bar = 0; bar < 3; bar++)
            {
                int x = 2 + bar * 5;
                for (int y = 0; y < 16; y++)
                {
                    c.Set(x, y, ArtKit.Iron[3]);
                    c.Set(x + 1, y, ArtKit.Iron[2]);
                    c.Set(x + 2, y, ArtKit.Iron[0]);
                }
            }

            // A travessa.
            for (int x = 0; x < 16; x++)
            {
                c.Set(x, 7, ArtKit.Iron[3]);
                c.Set(x, 8, ArtKit.Iron[1]);
                c.Set(x, 9, ArtKit.Iron[0]);
            }

            ArtKit.Rivet(c, 3, 7, ArtKit.Iron);
            ArtKit.Rivet(c, 13, 7, ArtKit.Iron);

            // Ferrugem e um cristal miudo brotando na barra.
            c.Set(8, 12, ArtKit.Rust[1]);
            c.Set(2, 3, ArtKit.Rust[2]);
            c.Set(13, 13, Amethyst[2]);
            c.Set(13, 12, Amethyst[3]);
            c.Set(14, 13, Amethyst[1]);
            return c;
        }

        // ------------------------------------------------------------------
        // Placas, bilhetes e o capacete
        // ------------------------------------------------------------------

        /// <summary>A tabua do turno: tabuas pregadas, com doze riscos de nome (tres rasurados), 32x22.</summary>
        public static PixelCanvas RosterBoard()
        {
            var c = new PixelCanvas(32, 22);

            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool seam = y % 6 == 5;
                    float light = (y % 6 == 0 ? 0.6f : 0f) + (ArtKit.Noise(x * 0.3f, y * 1.4f, 4201) - 0.5f) * 0.6f;
                    c.Set(x, y, seam ? Wood[0] : ArtKit.Tone(Wood, light));
                }
            }

            // Os pregos e a rachadura.
            foreach (int x in new[] { 2, 29 })
            {
                for (int y = 2; y < 18; y += 6) c.Set(x, y, ArtKit.Iron[3]);
            }

            ArtKit.Line(c, 22, 0, 26, 17, Wood[0]);

            // Doze nomes, em duas colunas; tres riscados com forca.
            for (int i = 0; i < 12; i++)
            {
                int col = i / 6;
                int y = 2 + (i % 6) * 2 + (i % 6) / 3;
                int x0 = 5 + col * 13;
                for (int x = x0; x < x0 + 8; x++)
                {
                    if (ArtKit.Hash(x, i, 4202) % 4 != 0) c.Set(x, y, Paper[3]);
                }

                if (i == 3 || i == 8 || i == 10) ArtKit.Line(c, x0 - 1, y + 1, x0 + 9, y - 1, Dark);
                else c.Set(x0 + 9, y, Amethyst[2]);
            }

            // Os pes da tabua, fincados no chao.
            c.Fill(4, 18, 2, 4, Wood[1]);
            c.Fill(26, 18, 2, 4, Wood[1]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>A placa da Coroa na beira da Fenda: chapa esmaltada num poste, com a coroa e o aviso, 28x30.</summary>
        public static PixelCanvas CrownSign()
        {
            var c = new PixelCanvas(28, 30);
            var enamel = new[] { new Rgba(90, 20, 30), new Rgba(140, 34, 44), new Rgba(178, 52, 58), new Rgba(214, 90, 84) };

            // O poste.
            c.Fill(12, 16, 4, 14, Wood[1]);
            c.Fill(12, 16, 1, 14, Wood[3]);

            // A chapa, um pouco torta.
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 27; x++)
                {
                    bool frame = x < 3 || x > 24 || y < 3 || y > 14;
                    float light = frame ? 0.8f : 0.1f - y / 40f;
                    c.Set(x, y, frame ? ArtKit.Tone(ArtKit.Brass, light) : ArtKit.Tone(enamel, light));
                }
            }

            // A coroa, no meio, em cima.
            for (int x = 10; x <= 17; x++) c.Set(x, 7, ArtKit.Brass[3]);
            foreach (int x in new[] { 10, 13, 14, 17 }) c.Set(x, 6, ArtKit.Brass[3]);
            c.Set(10, 5, ArtKit.Brass[3]);
            c.Set(17, 5, ArtKit.Brass[3]);
            c.Set(13, 4, ArtKit.Brass[3]);

            // O aviso, em letras que nao se le de longe.
            for (int line = 0; line < 2; line++)
            {
                for (int x = 5; x < 23; x++)
                {
                    if (ArtKit.Hash(x, line, 4203) % 4 != 0) c.Set(x, 10 + line * 2, ArtKit.Brass[3]);
                }
            }

            // Lascas de esmalte caidas: a placa e velha.
            c.Set(21, 9, ArtKit.Iron[1]);
            c.Set(6, 13, ArtKit.Iron[1]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um bilhete de mineiro preso na rocha por uma lasca de cristal, 14x14.</summary>
        public static PixelCanvas MinerNote()
        {
            var c = new PixelCanvas(14, 14);

            ArtKit.Polygon(c, new[] { (1f, 3f), (12f, 2f), (13f, 13f), (2f, 13f) }, (x, y) =>
                ArtKit.Tone(Paper, 0.3f - (x + y) / 60f));

            for (int line = 0; line < 4; line++)
            {
                for (int x = 3; x < 11; x++)
                {
                    if (ArtKit.Hash(x, line, 4204) % 3 != 0) c.Set(x, 5 + line * 2, Ink);
                }
            }

            // A dobra e a lasca que prende.
            ArtKit.Line(c, 2, 8, 12, 8, Paper[0]);
            ArtKit.Taper(c, 7f, 4f, 9f, 0f, 1.4f, 0.3f, Amethyst, 0.3f);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>O capacete de mineiro, amassado, com a lanterna apagada e rachada, 18x12.</summary>
        public static PixelCanvas Helmet()
        {
            var c = new PixelCanvas(18, 12);

            // A cupula, amassada de um lado.
            ArtKit.Ball(c, 9f, 8f, 7.5f, 6f, ArtKit.Corroded(ArtKit.Brass, 0.25f, 4205), 0.1f);
            for (int x = 0; x < 18; x++)
            {
                for (int y = 9; y < 12; y++) c.Set(x, y, Rgba.Clear);
            }

            c.Fill(0, 8, 18, 2, ArtKit.Tone(ArtKit.Brass, -0.2f));
            ArtKit.Line(c, 12, 3, 14, 6, ArtKit.Brass[0]);

            // A lanterna, rachada e apagada.
            ArtKit.Ball(c, 9f, 4.5f, 2.2f, 2f, ArtKit.Iron, 0.2f);
            c.Set(9, 4, new Rgba(90, 96, 110));
            c.Set(8, 5, new Rgba(140, 150, 160));

            // O risco a prego, por dentro da aba.
            ArtKit.Line(c, 3, 9, 14, 9, ArtKit.Brass[0]);

            ArtKit.AddOutline(c);
            return c;
        }

        // ------------------------------------------------------------------
        // A mina: escoramento, trilho e vagonete
        // ------------------------------------------------------------------

        /// <summary>O poste do escoramento, 16x16, emendavel de cima pra baixo: um tronco lavrado a enxo.</summary>
        public static PixelCanvas MinePost()
        {
            var c = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 5; x < 11; x++)
                {
                    float light = (x < 7 ? 0.7f : (x > 9 ? -0.6f : 0f)) + (ArtKit.Hash(x, y, 4206) % 9 == 0 ? -0.5f : 0f);
                    c.Set(x, y, ArtKit.Tone(Wood, light));
                }

                c.Set(4, y, ArtKit.Outline);
                c.Set(11, y, ArtKit.Outline);
            }

            // Os veios da madeira e um nó.
            ArtKit.Line(c, 7, 2, 8, 9, Wood[0]);
            c.Set(8, 12, Wood[0]);
            c.Set(9, 12, Wood[0]);
            return c;
        }

        /// <summary>A viga do escoramento, 16x16, emendavel de lado: madeira grossa com uma cinta de ferro.</summary>
        public static PixelCanvas MineBeam()
        {
            var c = new PixelCanvas(16, 16);

            for (int x = 0; x < 16; x++)
            {
                for (int y = 4; y < 12; y++)
                {
                    float light = (y < 6 ? 0.7f : (y > 9 ? -0.6f : 0f)) + (ArtKit.Hash(x, y, 4207) % 11 == 0 ? -0.5f : 0f);
                    c.Set(x, y, ArtKit.Tone(Wood, light));
                }

                c.Set(x, 3, ArtKit.Outline);
                c.Set(x, 12, ArtKit.Outline);
            }

            // A cinta de ferro, com rebite.
            for (int y = 4; y < 12; y++) c.Set(13, y, ArtKit.Iron[2]);
            c.Set(13, 7, ArtKit.Iron[3]);
            ArtKit.Line(c, 2, 6, 9, 7, Wood[0]);
            return c;
        }

        /// <summary>O trilho da mina, 16x6, emendavel de lado: dormente de madeira e trilho de ferro.</summary>
        public static PixelCanvas MineRail()
        {
            var c = new PixelCanvas(16, 6);

            // Dormentes.
            foreach (int x in new[] { 2, 10 })
            {
                c.Fill(x, 3, 4, 3, Wood[1]);
                c.Fill(x, 3, 4, 1, Wood[2]);
            }

            // O trilho.
            for (int x = 0; x < 16; x++)
            {
                c.Set(x, 1, ArtKit.Iron[3]);
                c.Set(x, 2, ArtKit.Iron[1]);
            }

            c.Set(7, 2, ArtKit.Rust[1]);
            return c;
        }

        /// <summary>Um vagonete tombado de lado, com cristal bruto derramado, 36x22.</summary>
        public static PixelCanvas MineCart()
        {
            var c = new PixelCanvas(36, 22);

            // A caçamba, inclinada.
            ArtKit.Polygon(c, new[] { (3f, 6f), (29f, 3f), (27f, 17f), (6f, 18f) }, (x, y) =>
                ArtKit.Tone(ArtKit.Iron, 0.4f - (y - 3f) / 15f - (x > 24f ? 0.4f : 0f)));
            ArtKit.Line(c, 3, 6, 29, 3, ArtKit.Iron[3]);
            for (int x = 6; x < 28; x += 7) ArtKit.Rivet(c, x, 9, ArtKit.Iron);
            c.Set(15, 13, ArtKit.Rust[1]);
            c.Set(22, 10, ArtKit.Rust[2]);

            // As rodas.
            ArtKit.Ball(c, 9f, 19f, 3f, 3f, ArtKit.Iron, 0.2f);
            ArtKit.Ball(c, 24f, 18f, 3f, 3f, ArtKit.Iron, 0.2f);

            // O cristal derramado na frente.
            ArtKit.Taper(c, 31f, 21f, 33f, 13f, 2f, 0.4f, Amethyst, 0.2f);
            ArtKit.Taper(c, 34f, 21f, 35f, 16f, 1.4f, 0.3f, Milky, 0.2f);
            ArtKit.Taper(c, 28f, 21f, 29f, 17f, 1.2f, 0.3f, Amethyst, 0f);

            ArtKit.AddOutline(c);
            return c;
        }
    }
}
