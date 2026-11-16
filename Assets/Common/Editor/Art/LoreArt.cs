using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte propria de cada coisa que se le no District 1 (cartas, placas,
    /// riscos na parede). Cada uma tem dois desenhos:
    ///
    ///   o do MAPA    — pequeno, na escala do cenario, pra achar de longe;
    ///   o da LEITURA — grande, mostrado no meio da tela enquanto o Scrap le,
    ///                  desenhado com o kit de documentos (pasta Docs: papel,
    ///                  metal e pedra com relevo, cursiva de pena, sulco de
    ///                  prego e de cinzel). Cada documento tem o seu proprio.
    ///
    /// O que muda com a purificacao (o muro dos nomes) tem um terceiro desenho,
    /// o de depois. O desenho do mapa e um so pra cada tipo de objeto
    /// (MapName): duas placas iguais usam o mesmo sprite.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static partial class LoreArt
    {
        public sealed class Piece
        {
            /// <summary>O nome do LoreSpot (Lore_...).</summary>
            public string Lore;

            /// <summary>O nome do sprite do mapa. Objetos iguais dividem o mesmo.</summary>
            public string MapName;

            /// <summary>O desenho no mapa, com o pivo no meio da base.</summary>
            public PixelCanvas Map;

            /// <summary>Quantos tiles acima do chao da celula o desenho do mapa fica.</summary>
            public float Lift;

            /// <summary>Preso na parede (atras do terreno) ou no chao (na frente).</summary>
            public bool OnWall;

            /// <summary>O desenho grande, da leitura.</summary>
            public PixelCanvas View;

            /// <summary>O de depois da purificacao, se muda. Nulo repete View.</summary>
            public PixelCanvas PurifiedView;
        }

        public static string ViewSprite(string lore, bool purified = false) =>
            "lore_" + lore.ToLowerInvariant() + (purified ? "_view_purified" : "_view");

        // Os desenhos do mapa, um por tipo de objeto, desenhados uma vez so.
        private static readonly Dictionary<string, PixelCanvas> Maps = new Dictionary<string, PixelCanvas>();

        private static Piece Make(string lore, string map, Func<PixelCanvas> drawMap, float lift, bool onWall,
            PixelCanvas view, PixelCanvas purified = null)
        {
            string mapName = "lore_map_" + map;
            if (!Maps.TryGetValue(mapName, out PixelCanvas canvas))
            {
                canvas = drawMap();
                Maps[mapName] = canvas;
            }

            return new Piece
            {
                Lore = lore, MapName = mapName, Map = canvas, Lift = lift, OnWall = onWall,
                View = view, PurifiedView = purified,
            };
        }

        private static Dictionary<string, Piece> Index(IEnumerable<Piece> pieces)
        {
            var all = new Dictionary<string, Piece>();
            foreach (Piece piece in pieces) all[piece.Lore] = piece;
            return all;
        }

        private static Dictionary<string, Piece> _phase01;

        /// <summary>As do District 1, pelo nome do LoreSpot.</summary>
        public static IReadOnlyDictionary<string, Piece> Phase01 => _phase01 ?? (_phase01 = BuildPhase01());

        private static Dictionary<string, Piece> BuildPhase01()
        {
            return Index(new[]
            {
                Make("Lore_TornTag", "torn_tag", TornTagMap, 0f, false, TornTagDoc()),
                Make("Lore_ChutePlaque", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, ChutePlaqueDoc()),
                Make("Lore_WallOfNames", "scratches_stone", WallOfNamesMap, 1.3f, true,
                    WallOfNamesDoc(false), WallOfNamesDoc(true)),
                Make("Lore_TrialPlaque", "plaque_polished", () => PlaqueMap(false), 1.9f, true, TrialPlaqueDoc()),
                Make("Lore_Decree", "noble_paper", () => NoblePaperMap(false), 0f, false, DecreeDoc()),
                Make("Lore_SortingList", "tin_sheet", SortingListMap, 1.4f, true, SortingListDoc()),
                Make("Lore_CollectorPlate", "collector_plate", CollectorPlateMap, 0f, false, CollectorPlateDoc()),
            });
        }

        // ------------------------------------------------------------------
        // Cores
        // ------------------------------------------------------------------

        private static readonly Rgba Ink = ArtKit.Outline;
        private static readonly Rgba PenInk = new Rgba(58, 44, 40);
        private static readonly Rgba Charcoal = new Rgba(64, 60, 62);
        private static readonly Rgba Chalk = new Rgba(222, 220, 206);
        private static readonly Rgba StampRed = new Rgba(168, 42, 38, 210);

        private static readonly Rgba[] Stone =
            { new Rgba(30, 28, 34), new Rgba(46, 44, 50), new Rgba(64, 62, 68), new Rgba(88, 86, 90) };

        // O risco na rocha: a pedra clara de dentro, e a borda escura do sulco.
        private static readonly Rgba ScratchOld = new Rgba(150, 146, 136);
        private static readonly Rgba ScratchFresh = new Rgba(222, 218, 204);
        private static readonly Rgba ScratchShadow = new Rgba(16, 14, 18);

        // ------------------------------------------------------------------
        // A etiqueta rasgada (no chao, onde o Scrap acorda)
        // ------------------------------------------------------------------


        private static PixelCanvas TornTagMap()
        {
            var c = new PixelCanvas(14, 7);
            var card = new Rgba(196, 176, 130);
            c.Fill(4, 2, 8, 5, card);
            c.Fill(4, 2, 1, 1, Rgba.Clear);
            c.Set(11, 2, Rgba.Clear);
            c.Set(6, 4, ArtKit.Iron[0]);
            c.Fill(8, 3, 3, 1, PenInk);
            c.Set(9, 5, StampRed);
            c.Set(10, 5, StampRed);
            ArtKit.Line(c, 6, 4, 1, 2, ArtKit.Iron[2]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // As placas de latao
        // ------------------------------------------------------------------



        /// <summary>A placa no mapa: a da calha tomada de azinhavre, a da Pista polida.</summary>
        private static PixelCanvas PlaqueMap(bool corroded)
        {
            var c = new PixelCanvas(18, 11);
            Plate(c, 1, 1, 16, 9, ArtKit.Brass, corroded ? 0.4f : 0f, corroded ? 931 : 932);

            for (int x = 4; x < 14; x++)
            {
                if (x % 3 != 0) c.Set(x, 4, corroded ? ArtKit.Brass[3] : ArtKit.Brass[0]);
                if (x % 4 != 1 && x < 12) c.Set(x, 6, corroded ? ArtKit.Brass[3] : ArtKit.Brass[0]);
            }

            if (!corroded)
            {
                c.Set(3, 2, new Rgba(255, 250, 230));
                c.Set(4, 2, new Rgba(255, 250, 230));
                c.Set(3, 3, new Rgba(255, 250, 230));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // O muro dos nomes
        // ------------------------------------------------------------------


        /// <summary>No mapa, so os riscos: a parede de fundo e a propria pedra.</summary>
        private static PixelCanvas WallOfNamesMap()
        {
            var c = new PixelCanvas(40, 22);
            Rgba mark = new Rgba(176, 170, 156, 210);

            // Linhas de letra miuda, cada uma de um tamanho.
            int[] starts = { 2, 5, 3, 1, 4 };
            int[] ends = { 30, 36, 27, 34, 22 };
            for (int row = 0; row < starts.Length; row++)
            {
                int y = 2 + row * 4;
                for (int x = starts[row]; x < ends[row]; x++)
                {
                    if (ArtKit.Hash(x, y, 961) % 4 == 0) continue;
                    c.Set(x, y + (ArtKit.Hash(x, y, 962) % 5 == 0 ? 1 : 0), mark);
                }
            }

            ScratchGear(c, 35, 16, 2, mark);
            ScratchGear(c, 29, 18, 1, mark);
            return c;
        }

        // ------------------------------------------------------------------
        // O decreto dos Nobres
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // A lista da triagem
        // ------------------------------------------------------------------


        private static PixelCanvas SortingListMap()
        {
            var c = new PixelCanvas(12, 15);
            for (int y = 2; y < 14; y++)
            {
                for (int x = 1; x < 11; x++) c.Set(x, y, ArtKit.Iron[(x + y) % 5 == 0 ? 2 : 1]);
            }

            c.Set(10, 13, Rgba.Clear);
            c.Set(5, 1, ArtKit.Iron[3]);
            c.Set(6, 1, ArtKit.Iron[2]);
            for (int row = 0; row < 4; row++)
            {
                int y = 4 + row * 2;
                for (int x = 2; x < 10; x++) if (x != 5 && ArtKit.Hash(x, y, 991) % 3 != 0) c.Set(x, y, Chalk);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa do Coletor (o pedaco de chassi que sobra dele)
        // ------------------------------------------------------------------


        private static PixelCanvas CollectorPlateMap()
        {
            var c = new PixelCanvas(24, 11);
            for (int y = 2; y < 11; y++)
            {
                for (int x = 1; x < 23; x++)
                {
                    if ((x < 3 && y < 4) || (x > 20 && y > 8)) continue;
                    c.Set(x, y, (x * 7 + y * 3) % 11 == 0 ? ArtKit.Rust[1] : ArtKit.Iron[(x + y) % 4 == 0 ? 2 : 1]);
                }
            }

            c.Fill(6, 4, 11, 4, ArtKit.Brass[2]);
            c.Fill(6, 4, 11, 1, ArtKit.Brass[3]);
            c.Fill(8, 6, 7, 1, ArtKit.Brass[0]);
            c.Set(19, 8, new Rgba(240, 244, 250));
            c.Set(20, 8, new Rgba(196, 202, 214));
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>O papel dos 925 no mapa: no chao, ou pregado na parede.</summary>
        private static PixelCanvas NoblePaperMap(bool pinned)
        {
            if (!pinned)
            {
                var floor = new PixelCanvas(14, 7);
                floor.Fill(1, 2, 12, 5, new Rgba(214, 204, 176));
                floor.Fill(6, 2, 3, 2, Silver[2]);
                floor.Set(7, 3, Silver[0]);
                for (int x = 3; x < 11; x++) if (x % 3 != 0) floor.Set(x, 5, PenInk);
                ArtKit.AddOutline(floor, Ink);
                return floor;
            }

            var c = new PixelCanvas(12, 15);
            c.Fill(1, 1, 10, 13, new Rgba(214, 204, 176));
            c.Fill(5, 2, 2, 2, Silver[2]);
            for (int row = 0; row < 4; row++)
            {
                for (int x = 2; x < 10; x++) if ((x + row) % 4 != 0) c.Set(x, 6 + row * 2, PenInk);
            }

            c.Set(2, 2, ArtKit.Iron[0]);
            c.Set(9, 2, ArtKit.Iron[0]);
            c.Set(10, 13, Rgba.Clear);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static readonly Rgba[] Silver =
            { new Rgba(110, 114, 128), new Rgba(150, 156, 170), new Rgba(196, 202, 214), new Rgba(240, 244, 250) };

        // ------------------------------------------------------------------
        // Pecas de desenho
        // ------------------------------------------------------------------


        /// <summary>
        /// Uma chapa de metal com bisel (claro em cima e a esquerda), riscos de
        /// polimento, azinhavre pela quantidade e um rebite em cada canto.
        /// </summary>
        private static void Plate(PixelCanvas c, int x0, int y0, int w, int h, Rgba[] ramp, float patina, int seed)
        {
            for (int y = y0; y < y0 + h; y++)
            {
                for (int x = x0; x < x0 + w; x++)
                {
                    int left = x - x0;
                    int right = x0 + w - 1 - x;
                    int top = y - y0;
                    int bottom = y0 + h - 1 - y;

                    Rgba color;
                    if (top == 0 || left == 0) color = ramp[3];
                    else if (bottom == 0 || right == 0) color = ramp[0];
                    else if (bottom == 1 || right == 1) color = ramp[1];
                    else
                    {
                        // Riscos finos de polimento, na horizontal, quase sem contraste.
                        float n = ArtKit.Noise(x * 0.03f, y * 0.6f, seed);
                        color = n > 0.7f
                            ? Rgba.Lerp(ramp[2], ramp[3], 0.18f)
                            : (n < 0.25f ? Rgba.Lerp(ramp[2], ramp[1], 0.18f) : ramp[2]);
                    }

                    if (patina > 0f && top > 0 && left > 0)
                    {
                        float p = ArtKit.Noise(x * 0.07f, y * 0.09f, seed + 7) * 0.7f + ArtKit.Noise(x * 0.25f, y * 0.25f, seed + 8) * 0.3f;
                        if (p > 1f - patina) color = Rgba.Lerp(color, ArtKit.Verdigris[p > 1f - patina * 0.45f ? 3 : 2], 0.8f);
                    }

                    c.Set(x, y, color);
                }
            }

            foreach ((int rx, int ry) in new[] { (x0 + 5, y0 + 5), (x0 + w - 6, y0 + 5), (x0 + 5, y0 + h - 6), (x0 + w - 6, y0 + h - 6) })
            {
                if (w < 24 || h < 16)
                {
                    c.Set(rx - 3, ry - 3, ramp[3]);
                    c.Set(rx - 2, ry - 2, ramp[0]);
                    continue;
                }

                ArtKit.Ball(c, rx + 0.5f, ry + 0.5f, 2.2f, 2.2f, ramp, 0.3f);
            }
        }


        /// <summary>Uma engrenagem riscada: o aro, os dentes e o eixo.</summary>
        private static void ScratchGear(PixelCanvas c, int cx, int cy, int r, Rgba color)
        {
            for (int i = 0; i < 48; i++)
            {
                double a = i * Math.PI * 2.0 / 48.0;
                c.Set(cx + (int)Math.Round(Math.Cos(a) * r), cy + (int)Math.Round(Math.Sin(a) * r), color);
            }

            for (int t = 0; t < 8; t++)
            {
                double a = t * Math.PI / 4.0;
                for (int d = r + 1; d <= r + Math.Max(1, r / 2); d++)
                {
                    c.Set(cx + (int)Math.Round(Math.Cos(a) * d), cy + (int)Math.Round(Math.Sin(a) * d), color);
                }
            }

            c.Set(cx, cy, color);
        }

        /// <summary>Contas de presidiario: grupos de quatro riscos cortados por um.</summary>
        private static void Tally(PixelCanvas c, int x, int y, int groups, Rgba color)
        {
            for (int g = 0; g < groups; g++)
            {
                int gx = x + g * 10;
                for (int i = 0; i < 4; i++) ArtKit.Line(c, gx + i * 2, y, gx + i * 2, y + 7, color);
                ArtKit.Line(c, gx - 1, y + 6, gx + 7, y + 1, color);
            }
        }


        private static void Box(PixelCanvas c, int x, int y, int w, int h, Rgba color)
        {
            for (int i = x; i < x + w; i++)
            {
                c.Set(i, y, color);
                c.Set(i, y + h - 1, color);
            }

            for (int j = y; j < y + h; j++)
            {
                c.Set(x, j, color);
                c.Set(x + w - 1, j, color);
            }
        }


    }
}
