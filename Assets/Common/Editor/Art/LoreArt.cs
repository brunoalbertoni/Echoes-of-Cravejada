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
    ///                  com o texto escrito nele de verdade (ver PixelFont).
    ///
    /// O que muda com a purificacao (o muro dos nomes) tem um terceiro desenho,
    /// o de depois.
    ///
    /// Objetos iguais usam o mesmo modelo: todo papel oficial da Coroa e o
    /// mesmo documento com o selo dos 925 (NobleDocument), toda placa de latao
    /// e a mesma placa (BrassPlaque), toda pagina do livro de turnos e a mesma
    /// pagina (LogbookPage). O desenho do mapa tambem e um so pra cada tipo de
    /// objeto (MapName): duas placas iguais usam o mesmo sprite.
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
                Make("Lore_TornTag", "torn_tag", TornTagMap, 0f, false, TornTagView()),
                Make("Lore_ChutePlaque", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, ChutePlaqueView()),
                Make("Lore_WallOfNames", "scratches_stone", WallOfNamesMap, 1.3f, true,
                    WallOfNamesView(false), WallOfNamesView(true)),
                Make("Lore_TrialPlaque", "plaque_polished", () => PlaqueMap(false), 1.9f, true, TrialPlaqueView()),
                Make("Lore_Decree", "noble_paper", () => NoblePaperMap(false), 0f, false, DecreeView()),
                Make("Lore_SortingList", "tin_sheet", SortingListMap, 1.4f, true, SortingListView()),
                Make("Lore_CollectorPlate", "collector_plate", CollectorPlateMap, 0f, false, CollectorPlateView()),
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

        private static PixelCanvas TornTagView()
        {
            var c = new PixelCanvas(156, 92);
            var card = new Rgba(196, 176, 130);
            var cardDark = new Rgba(150, 128, 88);

            // O cartao: a ponta da esquerda chanfrada, a da direita arrancada.
            for (int y = 8; y < 86; y++)
            {
                int chamfer = Math.Max(0, 12 - Math.Min(y - 8, 85 - y));
                int tear = 132 + (int)(ArtKit.Noise(y * 0.45f, 0f, 901) * 14f) + ArtKit.Hash(y, 0, 902) % 3;
                for (int x = 4 + chamfer; x < tear; x++) c.Set(x, y, PaperTone(card, cardDark, x, y, 903));
            }

            // As fibras soltas do rasgo.
            for (int y = 8; y < 86; y += 3)
            {
                int x = RightEdge(c, y);
                if (x > 0 && ArtKit.Hash(x, y, 904) % 2 == 0) c.Set(x + 1, y, cardDark);
            }

            // O furo, com o ilhos de lata, e o arame partido.
            ArtKit.Ball(c, 17f, 47f, 6f, 6f, ArtKit.Tin, 0.2f);
            ClearDisc(c, 17f, 47f, 3f);
            ArtKit.Limb(c, 17f, 47f, 6f, 30f, 1.1f, ArtKit.Iron, 0.3f);
            ArtKit.Limb(c, 6f, 30f, 1f, 22f, 1.1f, ArtKit.Iron, 0.3f);

            // O texto: o comeco borrado, o resto ainda se le.
            PixelFont.Draw(c, "...TO Nº 7", 30, 18, PenInk);
            PixelFont.Draw(c, "DESTINO:", 30, 34, PenInk);
            PixelFont.Draw(c, "POÇO DE SUCATA.", 30, 50, PenInk);

            // A tinta borrada: manchas por cima das letras.
            for (int y = 12; y < 64; y++)
            {
                for (int x = 26; x < 120; x++)
                {
                    float n = ArtKit.Noise(x * 0.12f, y * 0.2f, 905);
                    float fade = x < 60 ? 1f : 0.45f;
                    if (n * fade > 0.6f && c.Get(x, y).A > 0) c.Blend(x, y, PenInk.WithAlpha((int)((n * fade - 0.6f) * 520f)));
                }
            }

            // O carimbo vermelho, pela metade: a outra metade foi com o rasgo.
            var stamp = new PixelCanvas(c.Width, c.Height);
            Box(stamp, 64, 64, 96, 22, StampRed);
            Box(stamp, 65, 65, 94, 20, StampRed);
            PixelFont.Draw(stamp, "REJEI", 72, 69, StampRed, scale: 2);
            for (int y = 60; y < 90; y++)
            {
                for (int x = 60; x < c.Width; x++)
                {
                    Rgba ink = stamp.Get(x, y);
                    if (ink.A == 0 || c.Get(x, y).A == 0) continue;
                    if (ArtKit.Hash(x, y, 906) % 5 == 0) continue;
                    c.Blend(x, y, ink);
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        private static PixelCanvas ChutePlaqueView()
        {
            return BrassPlaque(
                new[] { "CALHA DE DESCARTE Nº 1", "SERVIÇO DE LIMPEZA DA COROA", "\"O QUE NÃO BRILHA, DESCE.\"" },
                new[]
                {
                    Nail("e o que desce não sobe", 0, 912),
                    new Note("ainda", NailFresh, ArtKit.Brass[0], 136, 913),
                },
                0.26f, 911);
        }

        private static PixelCanvas TrialPlaqueView()
        {
            var c = new PixelCanvas(208, 118);
            Plate(c, 2, 2, 204, 114, ArtKit.Brass, 0f, 921);

            // Latao polido: o brilho em faixa, na diagonal.
            for (int y = 4; y < 114; y++)
            {
                for (int x = 4; x < 204; x++)
                {
                    int band = (x + y * 2) % 150;
                    if (band < 14 || (band > 20 && band < 24)) c.Blend(x, y, new Rgba(255, 246, 210, 70));
                }
            }

            // O filete gravado em volta.
            Box(c, 8, 8, 192, 102, ArtKit.Brass[1]);

            Rgba groove = ArtKit.Brass[0];
            Rgba edge = ArtKit.Brass[3];
            PixelFont.DrawCentered(c, "PISTA DE PROVAS DA COROA", 104, 16, groove, edge, PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "TODO AUTÔMATO RECÉM-MONTADO", 104, 32, groove, edge, PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "DEVE COMPLETÁ-LA.", 104, 44, groove, edge, PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "\"QUEM NÃO COMPLETA, DESCE.\"", 104, 61, groove, edge, PixelFont.Style.Engraved);

            for (int x = 40; x < 168; x++) c.Set(x, 75, ArtKit.Brass[1]);

            Rgba small = ArtKit.Brass[0];
            PixelFont.DrawCentered(c, "O PRÊMIO NO FIM DA PISTA", 104, 84, small, edge, PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "PERTENCE A QUEM O ALCANÇAR.", 104, 96, small, edge, PixelFont.Style.Engraved);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        private static PixelCanvas WallOfNamesView(bool purified)
        {
            var c = new PixelCanvas(240, 168);
            StoneSlab(c, 941);

            Rgba old = ScratchOld;
            Rgba shadow = ScratchShadow;

            // Os riscos miudos que enchem a pedra: engrenagens, contas, numeros.
            ScratchGear(c, 224, 16, 5, old);
            ScratchGear(c, 226, 98, 4, old);
            ScratchGear(c, 12, 98, 3, old);
            Tally(c, 170, 80, 3, old);
            Tally(c, 196, 64, 2, old);
            PixelFont.Draw(c, "A-77", 178, 50, old.Scaled(0.85f), shadow, PixelFont.Style.Scratched, seed: 942);
            PixelFont.Draw(c, "K9", 206, 84, old.Scaled(0.85f), shadow, PixelFont.Style.Scratched, seed: 943);

            // Os nomes, cada um na letra de quem riscou.
            PixelFont.Draw(c, "LAMPIÃO-12. AINDA ACENDO.", 12, 14, old, shadow, PixelFont.Style.Scratched, seed: 944);
            PixelFont.Draw(c, "BIGORNA. TRABALHEI QUARENTA ANOS.", 22, 30, old, shadow, PixelFont.Style.Scratched, seed: 945);
            PixelFont.Draw(c, "PEQUENA. NUNCA TIVE NÚMERO.", 14, 46, old, shadow, PixelFont.Style.Scratched, seed: 946);

            if (purified)
            {
                // Letra fresca: o risco ainda esta claro.
                PixelFont.Draw(c, "AINDA AQUI.", 40, 61, ScratchFresh, shadow, PixelFont.Style.Scratched, seed: 947);
                PixelFont.Draw(c, "COLETOR Nº 1.", 20, 80, ScratchFresh, shadow, PixelFont.Style.Scratched, seed: 948);
                PixelFont.Draw(c, "TAMBÉM ERA UM DE NÓS.", 20, 94, ScratchFresh, shadow, PixelFont.Style.Scratched, seed: 949);
            }
            else
            {
                // O espaco vazio no fim, e o prego pendurado num barbante, esperando.
                ArtKit.Line(c, 120, 70, 121, 84, ArtKit.Cloth[3]);
                ArtKit.Line(c, 121, 70, 122, 84, ArtKit.Cloth[1]);
                ArtKit.Limb(c, 121.5f, 86f, 121.5f, 101f, 1.3f, ArtKit.Tin, 0.4f);
                c.Fill(118, 84, 8, 2, ArtKit.Tin[3]);
                c.Fill(118, 86, 8, 1, ArtKit.Tin[1]);
                c.Set(121, 102, ArtKit.Tin[1]);
                c.Fill(119, 68, 3, 3, ArtKit.Tin[2]);
            }

            // Embaixo de todos, em letra grande.
            PixelFont.DrawCentered(c, "QUEM LER ISTO,", 120, 116, old, shadow, PixelFont.Style.Scratched, 2, 950);
            PixelFont.DrawCentered(c, "LEMBRE-SE DE NÓS.", 120, 144, old, shadow, PixelFont.Style.Scratched, 2, 951);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        private static PixelCanvas DecreeView()
        {
            return NobleDocument(
                new[] { "DECRETO DO CONSELHO", "DOS 925 NOBLES" },
                new[]
                {
                    "A CORROSÃO VERDE NÃO ATINGE A PRATA PURA.",
                    "TODO AUTÔMATO DE COBRE OU DE LATÃO QUE APRESENTAR MANCHAS, RANGIDOS OU DESOBEDIÊNCIA SERÁ RECOLHIDO E DESCARTADO.",
                    "NÃO HÁ CURA. HÁ DESCARTE.",
                    "POR ORDEM DE SUA MAJESTADE, O REI DE PRATA.",
                },
                null,
                new[] { Hand("a prata também mancha.", Charcoal, 0, 973), Hand("eu vi.", Charcoal, 8, 974) },
                972, false);
        }

        // ------------------------------------------------------------------
        // A lista da triagem
        // ------------------------------------------------------------------

        private static PixelCanvas SortingListView()
        {
            var c = new PixelCanvas(200, 156);

            // Uma folha de flandres escurecida, com a ponta de baixo dobrada.
            for (int y = 12; y < 152; y++)
            {
                for (int x = 4; x < 196; x++)
                {
                    if ((x - 180) + (y - 136) > 14) continue;
                    float n = ArtKit.Noise(x * 0.07f, y * 0.07f, 981);
                    Rgba color = n > 0.72f ? Rgba.Lerp(ArtKit.Iron[1], ArtKit.Iron[2], 0.4f) : ArtKit.Iron[1];
                    if (x < 6 || y < 14) color = ArtKit.Iron[3];
                    c.Set(x, y, color);
                }
            }

            // A ponta dobrada mostra o avesso, mais claro.
            for (int i = 0; i < 14; i++)
            {
                for (int j = 0; j <= i; j++) c.Set(195 - i + j, 151 - j, ArtKit.Tin[2]);
            }

            // O prego que a segura.
            ArtKit.Ball(c, 100f, 14f, 3.5f, 3.5f, ArtKit.Iron, 0.4f);
            c.Set(99, 12, ArtKit.Tin[3]);

            // Giz, em letra de maquina, toda igual.
            PixelFont.Draw(c, "PEÇAS BOAS:", 14, 26, Chalk);
            PixelFont.Draw(c, "PEÇAS RUINS:", 110, 26, Chalk);
            for (int x = 12; x < 190; x++) if (ArtKit.Hash(x, 0, 982) % 6 != 0) c.Set(x, 38, Chalk.WithAlpha(190));
            for (int y = 22; y < 112; y++) if (ArtKit.Hash(0, y, 983) % 6 != 0) c.Set(104, y, Chalk.WithAlpha(190));

            string[] good = { "ENGRENAGENS", "MOLAS", "REBITES", "LENTES" };
            for (int i = 0; i < good.Length; i++) PixelFont.Draw(c, good[i], 14, 46 + i * 12, Chalk);
            PixelFont.Draw(c, "SOBEM PELA", 14, 98 - 4, Chalk.WithAlpha(230));
            PixelFont.Draw(c, "CALHA.", 14, 98 + 8, Chalk.WithAlpha(230));

            PixelFont.Draw(c, "TODO O RESTO.", 110, 46, Chalk);
            PixelFont.Draw(c, "FICAM NO POÇO.", 110, 62, Chalk);

            for (int x = 12; x < 190; x++) if (ArtKit.Hash(x, 1, 984) % 6 != 0) c.Set(x, 120, Chalk.WithAlpha(190));

            // A terceira coluna, comecada depois, numa letra que treme. Em branco.
            PixelFont.Draw(c, "PEÇAS QUE SE MEXEM:", 14, 128, Chalk, default, PixelFont.Style.Scratched, seed: 985);
            for (int x = 16; x < 140; x += 4) c.Set(x, 146, Chalk.WithAlpha(110));

            // O giz borrado aqui e ali.
            for (int y = 20; y < 150; y++)
            {
                for (int x = 8; x < 192; x++)
                {
                    if (ArtKit.Noise(x * 0.1f, y * 0.1f, 986) > 0.8f && c.Get(x, y).A > 0) c.Blend(x, y, Chalk.WithAlpha(26));
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        private static PixelCanvas CollectorPlateView()
        {
            var c = new PixelCanvas(212, 126);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.2f, 1001, 0.12f, ArtKit.Rust);

            // O pedaco de chassi, de beirada rasgada.
            (float X, float Y)[] outline =
            {
                (8, 16), (40, 5), (92, 11), (150, 4), (206, 9), (203, 58), (209, 120),
                (140, 116), (96, 122), (44, 115), (3, 120), (9, 66),
            };
            ArtKit.Polygon(c, outline, (x, y) =>
            {
                float light = (ArtKit.Noise(x * 0.06f, y * 0.06f, 1002) - 0.5f) * 1.4f - 0.1f;
                return iron(x, y, light);
            });

            // Beirada rasgada: lascas no contorno.
            for (int y = 0; y < c.Height; y++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    if (c.Get(x, y).A == 0) continue;
                    bool edge = c.Get(x - 1, y).A == 0 || c.Get(x + 1, y).A == 0 || c.Get(x, y - 1).A == 0 || c.Get(x, y + 1).A == 0;
                    if (edge && ArtKit.Hash(x, y, 1003) % 3 == 0) c.Set(x, y, Rgba.Clear);
                }
            }

            // A fileira de rebites do chassi, nas beiradas.
            for (int x = 20; x < 196; x += 14)
            {
                ArtKit.Rivet(c, x, 15, ArtKit.Iron);
                ArtKit.Rivet(c, x + 4, 108, ArtKit.Iron);
            }

            // A placa de latao, rebitada no meio.
            Plate(c, 22, 24, 168, 70, ArtKit.Brass, 0.08f, 1004);
            Rgba face = ArtKit.Brass[3];
            Rgba shade = ArtKit.Brass[0];
            PixelFont.DrawCentered(c, "COLETOR DE REFUGO Nº 1", 106, 33, face, shade, PixelFont.Style.Embossed);
            PixelFont.DrawCentered(c, "SERVIÇO DE LIMPEZA DA COROA", 106, 48, face, shade, PixelFont.Style.Embossed);
            PixelFont.DrawCentered(c, "MANTER O POÇO LIMPO.", 106, 64, face, shade, PixelFont.Style.Embossed);
            PixelFont.DrawCentered(c, "NÃO PARAR.", 106, 78, face, shade, PixelFont.Style.Embossed);

            // O aro de prata preso entre as chapas, no canto.
            SilverRing(c, 184f, 100f, 8f, 4.5f);
            ArtKit.Spark(c, 179, 94, 2);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        // ------------------------------------------------------------------
        // Os modelos: o mesmo objeto, com outro texto
        // ------------------------------------------------------------------

        /// <summary>Uma linha escrita depois, por cima do objeto: a prego, a giz, a carvao, a mao.</summary>
        private readonly struct Note
        {
            public readonly string Text;
            public readonly Rgba Color;
            public readonly Rgba Shadow;
            public readonly int Indent;
            public readonly int Seed;

            public Note(string text, Rgba color, Rgba shadow, int indent, int seed)
            {
                Text = text;
                Color = color;
                Shadow = shadow;
                Indent = indent;
                Seed = seed;
            }
        }

        private static readonly Rgba NailScratch = new Rgba(240, 222, 168);
        private static readonly Rgba NailFresh = new Rgba(252, 246, 228);
        private static readonly Rgba Pencil = new Rgba(92, 92, 102);
        private static readonly Rgba BlueInk = new Rgba(44, 50, 86);

        /// <summary>Riscado a prego no latao: o risco claro, a borda escura.</summary>
        private static Note Nail(string text, int indent, int seed) => new Note(text, NailScratch, ArtKit.Brass[0], indent, seed);

        /// <summary>Escrito a mao (giz, carvao, lapis, tinta): sem sulco, so a cor.</summary>
        private static Note Hand(string text, Rgba color, int indent, int seed) => new Note(text, color, default, indent, seed);

        /// <summary>
        /// A placa de latao fundida, do tamanho do texto: as linhas em relevo,
        /// centradas, e embaixo de um filete o que foi riscado ou escrito
        /// depois. Toda placa de latao do jogo e esta.
        /// </summary>
        private static PixelCanvas BrassPlaque(string[] cast, Note[] notes, float patina, int seed, int castScale = 1,
            bool mud = false)
        {
            int castLine = castScale == 1 ? 15 : 24;
            int inner = 0;
            foreach (string line in cast) inner = Math.Max(inner, PixelFont.Measure(line, castScale));
            foreach (Note note in notes) inner = Math.Max(inner, note.Indent + PixelFont.Measure(note.Text));

            int w = Math.Max(140, inner + 40);
            int castTop = 10 + 3 * castScale;
            int h = castTop + cast.Length * castLine + (notes.Length > 0 ? 14 + notes.Length * 13 : 0) + 6 + (mud ? 12 : 0);

            var c = new PixelCanvas(w + 4, h + 4);
            Plate(c, 2, 2, w, h, ArtKit.Brass, patina, seed);

            int y = castTop;
            foreach (string line in cast)
            {
                PixelFont.DrawCentered(c, line, c.Width / 2, y, ArtKit.Brass[3], ArtKit.Brass[0], PixelFont.Style.Embossed, castScale);
                y += castLine;
            }

            if (notes.Length > 0)
            {
                // O filete que separa o fundido do que veio depois.
                for (int x = 14; x < c.Width - 14; x++)
                {
                    c.Set(x, y, ArtKit.Brass[0]);
                    c.Set(x, y + 1, ArtKit.Brass[3]);
                }

                y += 11;
                foreach (Note note in notes)
                {
                    PixelFont.Draw(c, note.Text, 18 + note.Indent, y, note.Color, note.Shadow, PixelFont.Style.Scratched, 1, note.Seed);
                    y += 13;
                }
            }

            // Afundada na lama: a parte de baixo coberta de barro.
            if (mud)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    int top = c.Height - 10 - (int)(ArtKit.Noise(x * 0.08f, 0f, seed + 9) * 8f);
                    for (int py = top; py < c.Height; py++)
                    {
                        if (c.Get(x, py).A == 0) continue;
                        Rgba mudColor = py == top ? new Rgba(96, 82, 56) : new Rgba(64, 54, 38);
                        c.Blend(x, py, mudColor.WithAlpha(py == top ? 255 : 235));
                    }
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Um papel oficial da Coroa: o selo de cera prateada dos 925 em cima,
        /// o titulo, o texto, o carimbo (se tem) e o que alguem anotou depois.
        /// Todo decreto, ordem e relatorio dos Nobres e este papel.
        /// </summary>
        private static PixelCanvas NobleDocument(string[] header, string[] paragraphs, string[] stamp, Note[] notes,
            int seed, bool pinned)
        {
            const int width = 196;
            const int wrap = 168;

            var body = new List<List<string>>();
            int bodyHeight = 0;
            foreach (string paragraph in paragraphs)
            {
                List<string> lines = PixelFont.Wrap(paragraph, wrap);
                body.Add(lines);
                bodyHeight += lines.Count * PixelFont.LineHeight + 4;
            }

            int stampHeight = stamp != null ? stamp.Length * PixelFont.LineHeight + 18 : 0;
            int notesHeight = notes.Length > 0 ? notes.Length * 14 + 10 : 0;
            int height = 48 + header.Length * 12 + 14 + bodyHeight + stampHeight + notesHeight + 22;

            var c = new PixelCanvas(width, height);
            var paper = new Rgba(214, 204, 176);
            var paperDark = new Rgba(172, 158, 124);

            for (int y = 4; y < height - 4; y++)
            {
                for (int x = 4; x < width - 4; x++)
                {
                    // O canto de baixo a direita arrancado.
                    if ((x - (width - 46)) + (y - (height - 42)) > 30 + ArtKit.Hash(x, y, seed + 1) % 3) continue;
                    c.Set(x, y, PaperTone(paper, paperDark, x, y, seed));
                }
            }

            // A dobra do meio, de quem guardou o papel.
            for (int y = 4; y < height - 4; y++)
            {
                if (c.Get(97, y).A == 0) continue;
                c.Set(97, y, Rgba.Lerp(c.Get(97, y), paperDark, 0.5f));
                c.Set(98, y, Rgba.Lerp(c.Get(98, y), new Rgba(236, 228, 204), 0.5f));
            }

            // Pregado: os furos dos pregos nos cantos de cima, com a ferrugem escorrida.
            if (pinned)
            {
                foreach (int nx in new[] { 14, width - 16 })
                {
                    ArtKit.Ball(c, nx + 0.5f, 12.5f, 2.2f, 2.2f, ArtKit.Iron, 0.3f);
                    for (int i = 0; i < 6; i++) c.Blend(nx, 15 + i, ArtKit.Rust[1].WithAlpha(150 - i * 22));
                }
            }

            // O selo de cera prateada, com o 925.
            for (int i = 0; i < 9; i++)
            {
                double a = i * Math.PI * 2.0 / 9.0;
                ArtKit.Ball(c, 98f + (float)Math.Cos(a) * 11f, 24f + (float)Math.Sin(a) * 11f, 4f, 4f, Silver, 0.1f);
            }

            ArtKit.Ball(c, 98f, 24f, 13f, 13f, Silver, 0.15f);
            PixelFont.DrawCentered(c, "925", 98, 21, Silver[0], Silver[3], PixelFont.Style.Engraved);

            int lineY = 48;
            foreach (string line in header)
            {
                PixelFont.DrawCentered(c, line, 98, lineY, PenInk);
                lineY += 12;
            }

            for (int x = 30; x < 166; x++) if (x % 7 != 6) c.Blend(x, lineY, PenInk.WithAlpha(200));
            lineY += 10;

            foreach (List<string> lines in body)
            {
                foreach (string line in lines)
                {
                    PixelFont.Draw(c, line, 14, lineY, PenInk);
                    lineY += PixelFont.LineHeight;
                }

                lineY += 4;
            }

            // O carimbo vermelho, por cima de tudo, gasto.
            if (stamp != null)
            {
                var ink = new PixelCanvas(width, height);
                int stampWidth = 0;
                foreach (string line in stamp) stampWidth = Math.Max(stampWidth, PixelFont.Measure(line));

                int top = lineY + 4;
                Box(ink, 98 - stampWidth / 2 - 7, top - 5, stampWidth + 14, stamp.Length * PixelFont.LineHeight + 8, StampRed);
                Box(ink, 98 - stampWidth / 2 - 6, top - 4, stampWidth + 12, stamp.Length * PixelFont.LineHeight + 6, StampRed);
                for (int i = 0; i < stamp.Length; i++) PixelFont.DrawCentered(ink, stamp[i], 98, top + i * PixelFont.LineHeight, StampRed);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rgba color = ink.Get(x, y);
                        if (color.A == 0 || c.Get(x, y).A == 0 || ArtKit.Hash(x, y, seed + 2) % 6 == 0) continue;
                        c.Blend(x, y, color);
                    }
                }

                lineY += stampHeight;
            }

            // O que alguem escreveu depois, na margem, torto.
            lineY += 6;
            foreach (Note note in notes)
            {
                PixelFont.Draw(c, note.Text, 12 + note.Indent, lineY, note.Color, note.Shadow, PixelFont.Style.Scratched, 1, note.Seed);
                lineY += 14;
            }

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

        /// <summary>O tom de papel num ponto: manchado em nuvem, com grao.</summary>
        private static Rgba PaperTone(Rgba light, Rgba dark, int x, int y, int seed)
        {
            float n = ArtKit.Noise(x * 0.08f, y * 0.08f, seed);
            float stain = Math.Max(0f, n - 0.58f) * 1.8f;
            float grain = ArtKit.Hash(x, y, seed + 1) % 100 / 100f * 0.1f;
            return Rgba.Lerp(light, dark, stain + grain);
        }

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

        /// <summary>Uma laje de rocha de beirada irregular, com veios e rachaduras.</summary>
        private static void StoneSlab(PixelCanvas c, int seed)
        {
            for (int y = 0; y < c.Height; y++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    int border = Math.Min(Math.Min(x, c.Width - 1 - x), Math.Min(y, c.Height - 1 - y));
                    if (border + 1 < ArtKit.Noise(x * 0.12f, y * 0.12f, seed) * 8f) continue;

                    float light = (ArtKit.Noise(x * 0.05f, y * 0.05f, seed + 1) * 0.6f
                                   + ArtKit.Noise(x * 0.2f, y * 0.2f, seed + 2) * 0.4f - 0.5f) * 1.8f;
                    if (border < 4) light -= 0.3f;
                    c.Set(x, y, ArtKit.Tone(Stone, light));
                }
            }

            // Umas rachaduras finas.
            for (int i = 0; i < 6; i++)
            {
                int x = ArtKit.Hash(i, 0, seed + 3) % c.Width;
                int y = ArtKit.Hash(i, 1, seed + 3) % c.Height;
                int length = 10 + ArtKit.Hash(i, 2, seed + 3) % 18;
                int dx = ArtKit.Hash(i, 3, seed + 3) % 2 == 0 ? 1 : -1;
                for (int s = 0; s < length; s++)
                {
                    if (c.Get(x, y).A > 0) c.Set(x, y, Stone[0]);
                    x += ArtKit.Hash(s, i, seed + 4) % 3 == 0 ? 0 : dx;
                    y += 1;
                }
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

        /// <summary>Um aro de prata polida.</summary>
        private static void SilverRing(PixelCanvas c, float cx, float cy, float outer, float inner)
        {
            var silver = new[] { new Rgba(110, 114, 128), new Rgba(150, 156, 170), new Rgba(204, 210, 222), new Rgba(248, 250, 255) };
            for (int y = (int)(cy - outer) - 1; y <= (int)(cy + outer) + 1; y++)
            {
                for (int x = (int)(cx - outer) - 1; x <= (int)(cx + outer) + 1; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d > outer || d < inner) continue;

                    // Luz de cima a esquerda: claro de um lado do aro, escuro do outro.
                    float light = -(dx + dy) / Math.Max(1f, d) * 0.8f + (d - inner < 1.2f ? -0.3f : 0.2f);
                    c.Set(x, y, ArtKit.Tone(silver, light));
                }
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

        private static void ClearDisc(PixelCanvas c, float cx, float cy, float r)
        {
            for (int y = (int)(cy - r) - 1; y <= (int)(cy + r) + 1; y++)
            {
                for (int x = (int)(cx - r) - 1; x <= (int)(cx + r) + 1; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r) c.Set(x, y, Rgba.Clear);
                }
            }
        }

        /// <summary>O ultimo pixel opaco de uma linha, ou -1.</summary>
        private static int RightEdge(PixelCanvas c, int y)
        {
            for (int x = c.Width - 1; x >= 0; x--) if (c.Get(x, y).A > 0) return x;
            return -1;
        }
    }
}
