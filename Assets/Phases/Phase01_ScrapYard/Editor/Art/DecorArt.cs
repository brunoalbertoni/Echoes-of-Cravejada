using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A cenografia do District 1, desenhada por codigo: o que conta, so de
    /// olhar, que isto e o Poco de Sucata da Coroa.
    ///
    ///   Na parede — vigas rebitadas, canos com flange, correntes com gancho,
    ///   engrenagens gigantes presas na rocha (que giram), a Calha de Descarte
    ///   N.º 1 saindo do teto, cartazes da Coroa, a esteira e os cestos da
    ///   Triagem, cabos caidos, respiros com o verde do Oxide atras.
    ///
    ///   No chao — montes de sucata feitos de pecas de autômato (cabecas,
    ///   bracos, engrenagens, canos, latas), carcacas de descartados que nao
    ///   levantaram mais, carrinhos, rodas, barris, caldeiras furadas, os
    ///   postes de lampiao de cristal da Avenida das Facetas (que acendem com a
    ///   purificacao) e pocas de Oxide (que somem com ela).
    ///
    /// Tudo em 16 pixels por tile, como o tileset da caverna, pra conversar
    /// com o terreno. Os nomes e os pivos de cada peca estao em Specs.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class DecorArt
    {
        public const int PixelsPerUnit = 16;

        private static readonly Rgba Ink = new Rgba(24, 18, 26);

        /// <summary>Como uma peca e importada: o pivo (fracao do tamanho) e se repete em modo Tiled.</summary>
        public readonly struct Spec
        {
            public readonly float PivotX;
            public readonly float PivotY;
            public readonly bool Tiled;

            public Spec(float pivotX, float pivotY, bool tiled = false)
            {
                PivotX = pivotX;
                PivotY = pivotY;
                Tiled = tiled;
            }
        }

        // ------------------------------------------------------------------
        // Os nomes
        // ------------------------------------------------------------------

        public const string Chute = "d1_chute";
        public const string GirderV = "d1_girder_v";
        public const string GirderH = "d1_girder_h";
        public const string PipeV = "d1_pipe_v";
        public const string PipeH = "d1_pipe_h";
        public const string PipeFlange = "d1_pipe_flange";

        // As curvas do cano, pelos dois lados por onde ele entra e sai.
        public const string PipeElbowDownRight = "d1_pipe_elbow_down_right";
        public const string PipeElbowDownLeft = "d1_pipe_elbow_down_left";
        public const string PipeElbowUpLeft = "d1_pipe_elbow_up_left";
        public const string PipeElbowUpRight = "d1_pipe_elbow_up_right";

        public const string GearBig = "d1_gear_big";
        public const string GearMid = "d1_gear_mid";
        public const string Chain = "d1_chain";
        public const string Hook = "d1_hook";
        public const string Cage = "d1_cage";
        public const string PosterCrown = "d1_poster_crown";
        public const string PosterDecree = "d1_poster_decree";
        public const string Hazard = "d1_hazard";
        public const string ArrowUp = "d1_arrow_up";
        public const string Vent = "d1_vent";
        public const string Conveyor = "d1_conveyor";
        public const string ConveyorMotor = "d1_conveyor_motor";
        public const string SortingBins = "d1_bins";
        public const string Scaffold = "d1_scaffold";
        public const string ClawMarks = "d1_claw_marks";
        public const string Tally = "d1_tally";
        public const string Cable = "d1_cable";
        public const string WallLamp = "d1_wall_lamp";

        public static string Heap(int size, int variant) => $"d1_heap_{size}_{variant}";
        public const int HeapSizes = 3;
        public const int HeapVariants = 3;

        public static string Carcass(int variant) => "d1_carcass_" + variant;
        public const int CarcassVariants = 3;

        public const string Barrel = "d1_barrel";
        public const string BarrelTipped = "d1_barrel_tipped";
        public const string Cart = "d1_cart";
        public const string Wheel = "d1_wheel";
        public const string LampPost = "d1_lamp_post";
        public const string LampPostGlow = "d1_lamp_post_glow";
        public const string Puddle = "d1_puddle";
        public const string PuddleSmall = "d1_puddle_small";
        public const string Bolts = "d1_bolts";
        public const string BoilerWreck = "d1_boiler_wreck";
        public const string PipePile = "d1_pipe_pile";

        /// <summary>Todas as pecas, desenhadas, pelo nome.</summary>
        public static Dictionary<string, PixelCanvas> All()
        {
            var all = new Dictionary<string, PixelCanvas>
            {
                [Chute] = DrawChute(),
                [GirderV] = DrawGirder(true),
                [GirderH] = DrawGirder(false),
                [PipeV] = DrawPipe(true),
                [PipeH] = DrawPipe(false),
                [PipeFlange] = DrawFlange(),
                [PipeElbowDownRight] = DrawElbow(),
                // Cada quarto de volta no sentido horario leva o lado de baixo
                // pra esquerda e o da direita pra baixo.
                [PipeElbowDownLeft] = DrawElbow().RotatedClockwise(),
                [PipeElbowUpLeft] = DrawElbow().RotatedClockwise().RotatedClockwise(),
                [PipeElbowUpRight] = DrawElbow().RotatedClockwise().RotatedClockwise().RotatedClockwise(),
                [GearBig] = DrawGear(64, 14, 31f, 26f, 3),
                [GearMid] = DrawGear(40, 10, 19f, 15.5f, 7),
                [Chain] = DrawChain(),
                [Hook] = DrawHook(),
                [Cage] = DrawCage(),
                [PosterCrown] = DrawPoster(false),
                [PosterDecree] = DrawPoster(true),
                [Hazard] = DrawHazard(),
                [ArrowUp] = DrawArrow(),
                [Vent] = DrawVent(),
                [Conveyor] = DrawConveyor(),
                [ConveyorMotor] = DrawConveyorMotor(),
                [SortingBins] = DrawBins(),
                [Scaffold] = DrawScaffold(),
                [ClawMarks] = DrawClawMarks(),
                [Tally] = DrawTally(),
                [Cable] = DrawCable(),
                [WallLamp] = DrawWallLamp(),
                [Barrel] = DrawBarrel(),
                [BarrelTipped] = DrawBarrelTipped(),
                [Cart] = DrawCart(),
                [Wheel] = DrawWheel(),
                [LampPost] = DrawLampPost(false),
                [LampPostGlow] = DrawLampPost(true),
                [Puddle] = DrawPuddle(32, 8, 1),
                [PuddleSmall] = DrawPuddle(18, 6, 2),
                [Bolts] = DrawBolts(),
                [BoilerWreck] = DrawBoilerWreck(),
                [PipePile] = DrawPipePile(),
            };

            int[] heapW = { 32, 52, 84 };
            int[] heapH = { 18, 28, 42 };
            for (int size = 0; size < HeapSizes; size++)
            {
                for (int variant = 0; variant < HeapVariants; variant++)
                {
                    all[Heap(size, variant)] = DrawHeap(heapW[size], heapH[size], 100 + size * 10 + variant);
                }
            }

            for (int variant = 0; variant < CarcassVariants; variant++) all[Carcass(variant)] = DrawCarcass(variant);

            return all;
        }

        /// <summary>O pivo e o modo de cada peca. O que nao esta aqui tem o pivo no canto de baixo, a esquerda.</summary>
        public static Spec SpecOf(string name)
        {
            switch (name)
            {
                case GirderV:
                case GirderH:
                case PipeV:
                case PipeH:
                case Chain:
                case Conveyor:
                    return new Spec(0f, 0f, true);

                // As engrenagens giram em volta do eixo.
                case GearBig:
                case GearMid:
                    return new Spec(0.5f, 0.5f);

                // O que pendura balanca em volta do ponto de cima.
                case Hook:
                case Cage:
                    return new Spec(0.5f, 1f);

                default:
                    return new Spec(0f, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Na parede
        // ------------------------------------------------------------------

        /// <summary>
        /// A Calha de Descarte N.º 1, 64x64: um tubo grosso de ferro rebitado
        /// que desce do teto e abre num funil amassado, com a faixa de perigo,
        /// a plaquinha do numero e o verde escorrendo da boca.
        /// </summary>
        private static PixelCanvas DrawChute()
        {
            var c = new PixelCanvas(64, 64);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.22f, 301, 0.18f, ArtKit.Rust);

            // O tubo, descendo do teto.
            for (int py = 0; py < 38; py++)
            {
                for (int px = 20; px < 44; px++)
                {
                    float u = (px + 0.5f - 32f) / 12f;
                    float light = -0.9f * u + 0.35f * (float)Math.Sqrt(Math.Max(0f, 1f - u * u)) - 0.1f;
                    c.Set(px, py, iron(px - 32f, py, light));
                }
            }

            // As cintas rebitadas do tubo.
            foreach (int band in new[] { 6, 22 })
            {
                for (int px = 19; px < 45; px++)
                {
                    float u = (px + 0.5f - 32f) / 13f;
                    c.Set(px, band, ArtKit.Tone(ArtKit.Iron, 0.6f - u));
                    c.Set(px, band + 1, ArtKit.Tone(ArtKit.Iron, 0.1f - u));
                    c.Set(px, band + 2, ArtKit.Tone(ArtKit.Iron, -0.5f - u));
                    if (px % 4 == 1) c.Set(px, band + 1, ArtKit.Iron[3]);
                }
            }

            // O funil: abre ate 60 de largura, amassado de um lado.
            for (int py = 38; py < 56; py++)
            {
                float t = (py - 38f) / 17f;
                float half = 12f + t * t * 18f;
                float dent = py > 48 ? (py - 48) * 0.6f : 0f;

                for (int px = (int)(32f - half); px < (int)(32f + half - dent); px++)
                {
                    float u = (px + 0.5f - 32f) / half;
                    float light = -0.8f * u + 0.3f - t * 0.25f;
                    c.Set(px, py, iron(px - 32f, py, light));
                }
            }

            // A boca: o escuro de dentro e a borda.
            for (int px = 4; px < 56; px++)
            {
                float u = (px + 0.5f - 30f) / 26f;
                if (Math.Abs(u) > 1f) continue;
                int lip = 55 + (int)(Math.Sqrt(1f - u * u) * 4f);
                for (int py = 55; py <= lip; py++) c.Set(px, py, py == lip ? ArtKit.Iron[2] : ArtKit.Iron[0]);
                c.Set(px, 54, ArtKit.Tone(ArtKit.Iron, 0.7f - u));
            }

            // A faixa de perigo, em diagonais amarelas e pretas, ja descascando.
            var yellow = new Rgba(196, 160, 60);
            var black = new Rgba(40, 34, 36);
            for (int py = 28; py < 34; py++)
            {
                for (int px = 21; px < 43; px++)
                {
                    bool stripe = ((px + py) / 4) % 2 == 0;
                    if (ArtKit.Hash(px, py, 307) % 9 == 0) continue;
                    c.Set(px, py, stripe ? yellow : black);
                }
            }

            // A plaquinha do numero 1.
            c.Fill(27, 12, 10, 8, ArtKit.Brass[1]);
            c.Fill(27, 12, 10, 1, ArtKit.Brass[3]);
            c.Fill(27, 19, 10, 1, ArtKit.Brass[0]);
            c.Fill(31, 13, 2, 6, Ink);
            c.Set(30, 14, Ink);
            c.Fill(30, 18, 4, 1, Ink);

            // O verde escorrendo pela boca.
            foreach ((int x, int length) in new[] { (12, 6), (19, 9), (37, 5), (46, 7) })
            {
                for (int i = 0; i < length; i++) c.Set(x, 56 + i, i == length - 1 ? ArtKit.OxideCore : ArtKit.OxideGlow);
                c.Set(x + 1, 56, ArtKit.OxideDeep);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma viga em I rebitada, 16x16, em pe ou deitada. Repete.</summary>
        private static PixelCanvas DrawGirder(bool vertical)
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Rust, 0.18f, vertical ? 311 : 312, 0.3f);

            for (int a = 0; a < 16; a++)
            {
                for (int b = 2; b < 14; b++)
                {
                    // b atravessa a viga; a corre ao longo dela.
                    bool flange = b < 5 || b > 10;
                    float light = flange ? (b < 5 ? 0.5f - b * 0.15f : -0.3f - (b - 11) * 0.15f) : -0.35f;
                    if (b == 2) light = 0.8f;
                    if (b == 13) light = -0.9f;

                    Rgba color = iron(a, b, light);

                    // Os rebites das abas, a cada 4.
                    if (flange && (b == 3 || b == 12) && a % 4 == 1) color = ArtKit.Rust[3];

                    // A alma vazada em losango, a cada 8.
                    if (!flange && Math.Abs((a % 8) - 4) + Math.Abs(b - 7.5f) < 2.6f) color = Rgba.Clear;

                    if (vertical) c.Set(b, a, color);
                    else c.Set(a, b, color);
                }
            }

            return c;
        }

        /// <summary>Um trecho de cano grosso, 16x16, em pe ou deitado. Repete.</summary>
        private static PixelCanvas DrawPipe(bool vertical)
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Skin skin = ArtKit.Corroded(ArtKit.Copper, 0.35f, vertical ? 321 : 322, 0.35f);

            for (int a = 0; a < 16; a++)
            {
                for (int b = 3; b < 13; b++)
                {
                    float u = (b + 0.5f - 8f) / 5f;
                    float light = -0.9f * u + 0.3f * (float)Math.Sqrt(Math.Max(0f, 1f - u * u));
                    if (b == 3 || b == 12) light = -0.9f;
                    Rgba color = skin(a, b, light);
                    if (vertical) c.Set(b, a, color);
                    else c.Set(a, b, color);
                }
            }

            return c;
        }

        /// <summary>A junta de dois canos: um flange de parafusos e uma valvula de volante.</summary>
        private static PixelCanvas DrawFlange()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Drum(c, 8f, 8f, 7f, 4f, 3f, ArtKit.Plain(ArtKit.Iron), 0.1f);
            for (int i = 0; i < 4; i++) ArtKit.Rivet(c, 3 + i * 3, 7, ArtKit.Brass);

            // O volante da valvula, por cima.
            ArtKit.Limb(c, 8f, 4f, 8f, 1f, 0.8f, ArtKit.Iron, 0.2f);
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI / 8.0;
                c.Set((int)Math.Round(8 + Math.Cos(a) * 4), (int)Math.Round(1 + Math.Sin(a) * 1.3), ArtKit.Brass[i < 8 ? 1 : 3]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A curva do cano, 16x16: entra por baixo e sai pra direita.</summary>
        private static PixelCanvas DrawElbow()
        {
            var c = new PixelCanvas(16, 16);
            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    float dx = px + 0.5f - 16f;
                    float dy = py + 0.5f - 16f;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 3f || r > 13f) continue;
                    float u = (r - 8f) / 5f;
                    c.Set(px, py, ArtKit.Tone(ArtKit.Copper, u * 0.9f + 0.1f));
                }
            }

            return c;
        }

        /// <summary>Uma engrenagem enorme, presa na rocha, com raios vazados e o cubo rebitado.</summary>
        private static PixelCanvas DrawGear(int size, int teeth, float tip, float root, int seed)
        {
            var c = new PixelCanvas(size, size);
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            float rim = root - size * 0.12f;
            float hub = size * 0.12f;
            ArtKit.Skin metal = ArtKit.Corroded(ArtKit.Iron, 0.25f, seed, 0.2f, ArtKit.Rust);

            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx);
                    bool tooth = Math.Cos(angle * teeth) > 0.25;
                    if (r > (tooth ? tip : root)) continue;

                    // Os raios: seis, com o vazio entre eles.
                    bool spoke = Math.Abs(Math.IEEERemainder(angle, Math.PI / 3.0)) * r < size * 0.06f;
                    if (r < rim && r > hub && !spoke) continue;

                    float light = -(dx * 0.55f + dy * 0.65f) / Math.Max(1f, r);
                    float bevel = r > rim - 1.5f && r < rim + 1.5f ? -light : light * 0.6f;
                    c.Set(px, py, metal(dx, dy, bevel));
                }
            }

            // Os rebites do cubo.
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3.0 + Math.PI / 6.0;
                ArtKit.Rivet(c, (int)(cx + Math.Cos(a) * hub * 0.6f) - 1, (int)(cy + Math.Sin(a) * hub * 0.6f) - 1, ArtKit.Brass);
            }

            c.Set((int)cx, (int)cy, Ink);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um trecho de corrente, 16x16, com os elos no meio. Repete na vertical.</summary>
        private static PixelCanvas DrawChain()
        {
            var c = new PixelCanvas(16, 16);
            for (int link = 0; link < 2; link++)
            {
                int y = link * 8;
                if (link == 0)
                {
                    // Elo de frente: oval vazado.
                    for (int py = 0; py < 8; py++)
                    {
                        for (int px = 5; px < 11; px++)
                        {
                            float dx = (px + 0.5f - 8f) / 3f;
                            float dy = (py + 0.5f - 4f) / 4f;
                            float d = dx * dx + dy * dy;
                            if (d > 1f || d < 0.35f) continue;
                            c.Set(px, y + py, ArtKit.Tone(ArtKit.Iron, -dx * 0.8f + 0.2f));
                        }
                    }
                }
                else
                {
                    // Elo de lado: uma barra.
                    for (int py = -1; py < 9; py++) c.Set(7, y + py, ArtKit.Iron[3]);
                    for (int py = -1; py < 9; py++) c.Set(8, y + py, ArtKit.Iron[1]);
                }
            }

            return c;
        }

        /// <summary>O gancho de guindaste na ponta da corrente, 16x16, pendurado pelo alto.</summary>
        private static PixelCanvas DrawHook()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Limb(c, 8f, 0f, 8f, 6f, 1.3f, ArtKit.Iron, 0.2f);
            ArtKit.Drum(c, 8f, 4f, 3f, 2f, 3f, ArtKit.Plain(ArtKit.Rust), 0.2f);

            for (int i = 0; i <= 12; i++)
            {
                double a = Math.PI * 0.1 + i * Math.PI * 1.15 / 12.0;
                float x = 8f - (float)Math.Cos(a) * 3.6f;
                float y = 10f + (float)Math.Sin(a) * 3.6f;
                ArtKit.Ball(c, x, y, 1.3f, 1.3f, ArtKit.Iron, 0.1f);
            }

            c.Set(12, 8, ArtKit.Tin[3]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma gaiola pendurada, 16x24: barras de ferro, o fundo amassado e uma cabeca de autômato la dentro.</summary>
        private static PixelCanvas DrawCage()
        {
            var c = new PixelCanvas(16, 24);

            // A cabeca la dentro.
            ArtKit.Ball(c, 8f, 18f, 3.5f, 3f, ArtKit.Corroded(ArtKit.Copper, 0.3f, 331, 0.5f), 0.1f);
            c.Set(7, 18, Ink);
            c.Set(10, 18, Ink);

            // O argolao, o teto em cupula e as barras.
            ArtKit.Limb(c, 8f, 0f, 8f, 3f, 0.8f, ArtKit.Iron, 0.2f);
            for (int px = 2; px <= 13; px++)
            {
                float u = (px - 7.5f) / 6f;
                int top = 3 + (int)(Math.Abs(u) * Math.Abs(u) * 3f);
                c.Set(px, top, ArtKit.Tone(ArtKit.Iron, 0.5f - u));
                c.Set(px, top + 1, ArtKit.Iron[1]);
            }

            foreach (int x in new[] { 2, 5, 8, 11, 13 })
            {
                for (int py = 6; py < 22; py++) c.Set(x, py, x < 8 ? ArtKit.Iron[3] : ArtKit.Iron[2]);
            }

            c.Fill(2, 21, 12, 2, ArtKit.Rust[1]);
            c.Fill(2, 21, 12, 1, ArtKit.Rust[2]);
            c.Fill(1, 13, 14, 1, ArtKit.Iron[1]);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Um cartaz da Coroa, 24x32, rasgado e manchado. O da coroa mostra a
        /// coroa de prata e o "925"; o do decreto, o selo e linhas de texto.
        /// </summary>
        private static PixelCanvas DrawPoster(bool decree)
        {
            var c = new PixelCanvas(24, 32);
            var paper = decree ? new Rgba(196, 184, 150) : new Rgba(70, 70, 96);
            var paperDark = decree ? new Rgba(150, 136, 104) : new Rgba(46, 46, 68);
            var silver = new Rgba(214, 220, 228);
            var silverDark = new Rgba(150, 156, 170);

            for (int py = 0; py < 32; py++)
            {
                for (int px = 0; px < 24; px++)
                {
                    // Rasgado: a ponta de baixo a direita arrancada, a borda irregular.
                    if (px + (31 - py) * 0.9f < 6f - ArtKit.Hash(px, py, 341) % 3) continue;
                    if (py > 25 && px > 15 && (px - 15) + (py - 25) > 6) continue;
                    if ((px == 0 || px == 23 || py == 0 || py == 31) && ArtKit.Hash(px, py, 342) % 3 == 0) continue;

                    Rgba color = (px + py) % 11 == 0 ? paperDark : paper;
                    if (ArtKit.Noise(px * 0.3f, py * 0.3f, 343) > 0.78f) color = Rgba.Lerp(color, ArtKit.Verdigris[1], 0.5f);
                    c.Set(px, py, color);
                }
            }

            // A borda de prata.
            for (int px = 2; px < 22; px++)
            {
                if (c.Get(px, 2).A > 0) c.Set(px, 2, silverDark);
                if (c.Get(px, 29).A > 0) c.Set(px, 29, silverDark);
            }

            if (!decree)
            {
                // A coroa: tres pontas sobre a faixa.
                c.Fill(6, 11, 12, 3, silver);
                c.Fill(6, 13, 12, 1, silverDark);
                foreach (int x in new[] { 6, 11, 16 })
                {
                    c.Fill(x, 7, 2, 4, silver);
                    c.Set(x, 6, silver);
                }

                c.Set(11, 12, new Rgba(120, 180, 230));
                c.Set(7, 12, new Rgba(120, 180, 230));
                c.Set(16, 12, new Rgba(120, 180, 230));

                // 925, em algarismos de 3x5.
                DrawDigits(c, "925", 6, 17, silver);

                // Duas linhas de letra miuda.
                for (int x = 4; x < 20; x++)
                {
                    if (x % 4 != 3) c.Set(x, 24, silverDark);
                    if (x % 5 != 4 && x < 17) c.Set(x, 26, silverDark);
                }
            }
            else
            {
                // O selo de cera prateada e as linhas do texto.
                ArtKit.Ball(c, 12f, 8f, 4f, 4f, new[] { silverDark, new Rgba(176, 182, 194), silver, new Rgba(240, 244, 250) }, 0.2f);
                c.Set(12, 8, silverDark);
                var ink = new Rgba(60, 50, 44);
                for (int line = 0; line < 6; line++)
                {
                    int y = 15 + line * 2;
                    int length = line == 5 ? 9 : 16;
                    for (int x = 4; x < 4 + length; x++)
                    {
                        if ((x + line * 2) % 6 == 5) continue;
                        if (c.Get(x, y).A > 0) c.Set(x, y, ink);
                    }
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
        {
            ['9'] = new[] { "###", "#.#", "###", "..#", "###" },
            ['2'] = new[] { "###", "..#", "###", "#..", "###" },
            ['5'] = new[] { "###", "#..", "###", "..#", "###" },
            ['1'] = new[] { ".#.", "##.", ".#.", ".#.", "###" },
        };

        private static void DrawDigits(PixelCanvas c, string text, int x, int y, Rgba color)
        {
            foreach (char ch in text)
            {
                string[] glyph = Font[ch];
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (glyph[row][col] == '#') c.Set(x + col, y + row, color);
                    }
                }

                x += 4;
            }
        }

        /// <summary>Uma chapa de perigo, 32x8: diagonais amarelas e pretas, descascadas.</summary>
        private static PixelCanvas DrawHazard()
        {
            var c = new PixelCanvas(32, 8);
            var yellow = new Rgba(196, 160, 60);
            var black = new Rgba(40, 34, 36);

            for (int py = 1; py < 7; py++)
            {
                for (int px = 0; px < 32; px++)
                {
                    bool stripe = ((px + py) / 4) % 2 == 0;
                    Rgba color = stripe ? yellow : black;
                    if (ArtKit.Noise(px * 0.4f, py * 0.4f, 351) > 0.72f) color = ArtKit.Rust[1];
                    c.Set(px, py, color);
                }
            }

            for (int px = 0; px < 32; px++)
            {
                c.Set(px, 0, ArtKit.Iron[2]);
                c.Set(px, 7, ArtKit.Iron[0]);
            }

            ArtKit.Rivet(c, 1, 3, ArtKit.Iron);
            ArtKit.Rivet(c, 29, 3, ArtKit.Iron);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma seta pintada a mao na rocha, 16x16, apontando pra cima: a saida.</summary>
        private static PixelCanvas DrawArrow()
        {
            var c = new PixelCanvas(16, 16);
            var paint = new Rgba(220, 210, 180, 220);
            var faint = new Rgba(220, 210, 180, 120);

            for (int py = 2; py < 15; py++)
            {
                int w = py < 8 ? py - 1 : 2;
                for (int px = 8 - w; px < 8 + w; px++)
                {
                    bool drip = ArtKit.Hash(px, py, 361) % 7 == 0;
                    c.Set(px, py, drip ? faint : paint);
                }
            }

            c.Set(6, 15, faint);
            return c;
        }

        /// <summary>Um respiro de parede, 32x16: grade de ferro com o verde do Oxide pulsando atras.</summary>
        private static PixelCanvas DrawVent()
        {
            var c = new PixelCanvas(32, 16);
            for (int py = 1; py < 15; py++)
            {
                for (int px = 1; px < 31; px++)
                {
                    bool frame = px < 3 || px > 28 || py < 3 || py > 12;
                    if (frame)
                    {
                        c.Set(px, py, ArtKit.Tone(ArtKit.Iron, py < 3 ? 0.6f : (px < 3 ? 0.2f : -0.5f)));
                        continue;
                    }

                    bool slat = (px - 3) % 4 == 0;
                    float depth = (py - 3) / 9f;
                    Rgba inside = depth > 0.75f ? ArtKit.OxideGlow : (depth > 0.4f ? ArtKit.OxideDeep : ArtKit.Iron[0]);
                    c.Set(px, py, slat ? ArtKit.Iron[2] : inside);
                }
            }

            ArtKit.Rivet(c, 1, 1, ArtKit.Iron);
            ArtKit.Rivet(c, 29, 1, ArtKit.Iron);
            ArtKit.Rivet(c, 1, 13, ArtKit.Iron);
            ArtKit.Rivet(c, 29, 13, ArtKit.Iron);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um trecho da esteira da Triagem, 16x16: lona remendada em cima dos roletes. Repete na horizontal.</summary>
        private static PixelCanvas DrawConveyor()
        {
            var c = new PixelCanvas(16, 16);

            // A lona.
            for (int px = 0; px < 16; px++)
            {
                c.Set(px, 5, ArtKit.Rubber[3]);
                c.Set(px, 6, ArtKit.Rubber[2]);
                c.Set(px, 7, px % 4 == 0 ? ArtKit.Rubber[0] : ArtKit.Rubber[1]);
            }

            // Os roletes e a armacao.
            ArtKit.Ball(c, 4f, 9.5f, 2.2f, 2.2f, ArtKit.Iron, 0.2f);
            ArtKit.Ball(c, 12f, 9.5f, 2.2f, 2.2f, ArtKit.Iron, 0.2f);
            for (int px = 0; px < 16; px++)
            {
                c.Set(px, 12, ArtKit.Rust[2]);
                c.Set(px, 13, ArtKit.Rust[1]);
            }

            // As pernas da armacao.
            for (int py = 13; py < 16; py++)
            {
                c.Set(1, py, ArtKit.Rust[1]);
                c.Set(2, py, ArtKit.Rust[0]);
            }

            // Uma pecinha esquecida em cima da lona.
            c.Set(10, 4, ArtKit.Brass[2]);
            c.Set(11, 4, ArtKit.Brass[1]);
            return c;
        }

        /// <summary>O motor da ponta da esteira, 24x24: caixa de engrenagens com a manivela.</summary>
        private static PixelCanvas DrawConveyorMotor()
        {
            var c = new PixelCanvas(24, 24);
            ArtKit.Drum(c, 12f, 15f, 9f, 7f, 4f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 371, 0.3f, ArtKit.Rust), 0.1f);
            ArtKit.Ball(c, 12f, 13f, 4.5f, 4.5f, ArtKit.Brass, 0.1f);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0;
                c.Set((int)(12 + Math.Cos(a) * 5), (int)(13 + Math.Sin(a) * 5), ArtKit.Brass[0]);
            }

            ArtKit.Limb(c, 12f, 13f, 19f, 6f, 1f, ArtKit.Iron, 0.2f);
            ArtKit.Ball(c, 19f, 5.5f, 1.6f, 1.6f, ArtKit.Leather, 0.2f);
            c.Fill(3, 22, 18, 2, ArtKit.Iron[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Tres cestos de triagem, 48x24, com marcas de giz: o bom, o ruim e o que ninguem escreveu.</summary>
        private static PixelCanvas DrawBins()
        {
            var c = new PixelCanvas(48, 24);
            var chalk = new Rgba(226, 220, 204);

            for (int bin = 0; bin < 3; bin++)
            {
                int x0 = bin * 16;
                ArtKit.Skin wood = ArtKit.Corroded(ArtKit.Leather, 0.12f, 381 + bin, 0.4f);

                for (int py = 8; py < 24; py++)
                {
                    for (int px = x0 + 1; px < x0 + 15; px++)
                    {
                        bool plank = (py - 8) % 4 == 3;
                        float light = px == x0 + 1 ? 0.6f : (px == x0 + 14 ? -0.6f : (plank ? -0.4f : 0.1f));
                        c.Set(px, py, wood(px, py, light));
                    }
                }

                // O que tem dentro, saindo pela boca.
                if (bin == 0)
                {
                    ArtKit.Ball(c, x0 + 5f, 7f, 3f, 3f, ArtKit.Brass, 0.2f);
                    ArtKit.Ball(c, x0 + 10f, 6f, 2.5f, 2.5f, ArtKit.Brass, 0.1f);
                }
                else if (bin == 1)
                {
                    ArtKit.Limb(c, x0 + 3f, 8f, x0 + 11f, 3f, 1.3f, ArtKit.Iron, 0.1f);
                    ArtKit.Ball(c, x0 + 9f, 7f, 3f, 2f, ArtKit.Rust, 0f);
                }

                // As marcas de giz: um circulo, um X, e um ponto de interrogacao borrado.
                int mx = x0 + 6;
                if (bin == 0)
                {
                    foreach ((int dx, int dy) in new[] { (1, 0), (2, 0), (0, 1), (3, 1), (0, 2), (3, 2), (1, 3), (2, 3) }) c.Set(mx + dx, 14 + dy, chalk);
                }
                else if (bin == 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        c.Set(mx + i, 14 + i, chalk);
                        c.Set(mx + 3 - i, 14 + i, chalk);
                    }
                }
                else
                {
                    foreach ((int dx, int dy) in new[] { (0, 0), (1, 0), (2, 0), (2, 1), (1, 2), (1, 4) }) c.Set(mx + dx, 13 + dy, chalk.WithAlpha(150));
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um andaime de ferro, 32x48: dois montantes, travessas e um X de reforco, um pedaco caido.</summary>
        private static PixelCanvas DrawScaffold()
        {
            var c = new PixelCanvas(32, 48);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Rust, 0.2f, 391, 0.3f);

            ArtKit.Limb(c, 3f, 0f, 3f, 48f, 1.6f, iron, 0.2f);
            ArtKit.Limb(c, 28f, 4f, 28f, 48f, 1.6f, iron, 0f);

            foreach (int y in new[] { 6, 22, 38 })
            {
                ArtKit.Limb(c, 3f, y, 28f, y, 1.2f, iron, 0.1f);
                ArtKit.Rivet(c, 2, y - 1, ArtKit.Iron);
                ArtKit.Rivet(c, 27, y - 1, ArtKit.Iron);
            }

            ArtKit.Line(c, 4, 7, 27, 21, ArtKit.Rust[1]);
            ArtKit.Line(c, 27, 7, 4, 21, ArtKit.Rust[2]);
            ArtKit.Line(c, 4, 23, 27, 37, ArtKit.Rust[1]);

            // A tabua do patamar de cima, partida.
            c.Fill(4, 4, 14, 2, ArtKit.Leather[2]);
            c.Fill(4, 5, 14, 1, ArtKit.Leather[0]);
            ArtKit.Line(c, 18, 5, 24, 12, ArtKit.Leather[1]);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Riscos de garra na rocha, 32x24: tres sulcos fundos, em diagonal.</summary>
        private static PixelCanvas DrawClawMarks()
        {
            var c = new PixelCanvas(32, 24);
            var deep = new Rgba(20, 16, 22, 230);
            var lip = new Rgba(160, 170, 160, 140);

            for (int claw = 0; claw < 3; claw++)
            {
                for (int i = 0; i < 20; i++)
                {
                    int x = 4 + claw * 8 + i / 2;
                    int y = 2 + i;
                    int w = i < 3 || i > 16 ? 1 : 2;
                    for (int k = 0; k < w; k++) c.Set(x + k, y, deep);
                    c.Set(x - 1, y, lip);
                }
            }

            return c;
        }

        /// <summary>Marcas de contagem riscadas na parede, 32x16: grupos de cinco, centenas deles.</summary>
        private static PixelCanvas DrawTally()
        {
            var c = new PixelCanvas(32, 16);
            var chalk = new Rgba(210, 206, 190, 130);

            for (int row = 0; row < 2; row++)
            {
                for (int group = 0; group < 5; group++)
                {
                    int x0 = 1 + group * 6 + row;
                    int y0 = 1 + row * 8 + (group % 2);
                    if (row == 1 && group == 3) break;

                    // Quatro riscos tortos, de tamanhos diferentes, e o quinto cortando.
                    for (int i = 0; i < 4; i++)
                    {
                        int length = 4 + ArtKit.Hash(group, row * 4 + i, 501) % 3;
                        int lean = ArtKit.Hash(group, row * 4 + i, 502) % 2;
                        ArtKit.Line(c, x0 + i, y0, x0 + i + lean, y0 + length, chalk);
                    }

                    ArtKit.Line(c, x0 - 1, y0 + 4, x0 + 4, y0 + 1, chalk);
                }
            }

            return c;
        }

        /// <summary>Um cabo caido entre dois isoladores, 64x24, fazendo barriga.</summary>
        private static PixelCanvas DrawCable()
        {
            var c = new PixelCanvas(64, 24);

            for (int px = 2; px < 62; px++)
            {
                float t = (px - 2f) / 59f;
                float sag = 4f + 14f * 4f * t * (1f - t);
                int y = (int)sag;
                c.Set(px, y, ArtKit.Rubber[3]);
                c.Set(px, y + 1, ArtKit.Rubber[1]);
            }

            foreach (int x in new[] { 2, 61 })
            {
                ArtKit.Ball(c, x, 3.5f, 2f, 2.5f, new[] { new Rgba(110, 80, 60), new Rgba(160, 120, 90), new Rgba(200, 160, 120), new Rgba(230, 200, 160) }, 0.2f);
            }

            // Uma ponta partida pendurada, soltando faisca de vez em quando.
            ArtKit.Line(c, 40, 16, 42, 21, ArtKit.Rubber[2]);
            c.Set(42, 22, ArtKit.Copper[3]);
            c.Set(43, 22, ArtKit.Copper[2]);
            return c;
        }

        /// <summary>Uma arandela de cristal quebrada, 16x16, presa na rocha.</summary>
        private static PixelCanvas DrawWallLamp()
        {
            var c = new PixelCanvas(16, 16);
            c.Fill(2, 6, 3, 6, ArtKit.Brass[1]);
            c.Fill(2, 6, 1, 6, ArtKit.Brass[3]);
            ArtKit.Limb(c, 4f, 9f, 9f, 9f, 0.9f, ArtKit.Brass, 0.2f);

            // O vidro rachado, apagado, com um caco faltando.
            for (int py = 3; py < 13; py++)
            {
                for (int px = 8; px < 14; px++)
                {
                    if (px > 11 && py < 6) continue;
                    bool frame = px == 8 || px == 13 || py == 3 || py == 12;
                    c.Set(px, py, frame ? ArtKit.Brass[1] : new Rgba(60, 80, 84));
                }
            }

            ArtKit.Line(c, 9, 4, 12, 11, new Rgba(30, 40, 44));
            c.Set(10, 5, new Rgba(170, 200, 200));
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // No chao
        // ------------------------------------------------------------------

        /// <summary>
        /// Um monte de sucata: um morro escuro coberto de pecas de autômato
        /// jogadas umas por cima das outras — cabecas, bracos, engrenagens,
        /// canos, chapas, latas e molas. Cada semente e um monte diferente.
        /// </summary>
        private static PixelCanvas DrawHeap(int w, int h, int seed)
        {
            var c = new PixelCanvas(w, h);
            var rng = new Random(seed);

            // O perfil do morro, com calombos.
            var top = new float[w];
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f - w * 0.5f) / (w * 0.5f);
                float hill = (float)Math.Pow(Math.Max(0f, 1f - u * u), 0.7);
                float bumps = ArtKit.Noise(x * 0.25f, seed, seed) * 4f - 2f;
                top[x] = h - (h - 3f) * hill - bumps;
            }

            // O miolo: escuro, ferro e ferrugem misturados, mais escuro embaixo.
            for (int x = 0; x < w; x++)
            {
                for (int y = (int)Math.Ceiling(top[x]); y < h; y++)
                {
                    if (y < 0) continue;
                    float depth = (y - top[x]) / Math.Max(1f, h - top[x]);
                    float n = ArtKit.Noise(x * 0.35f, y * 0.35f, seed + 1);
                    Rgba[] ramp = n > 0.6f ? ArtKit.Rust : ArtKit.Iron;
                    c.Set(x, y, ArtKit.Tone(ramp, 0.2f - depth * 1.4f + (n - 0.5f) * 0.6f));
                }
            }

            // As pecas, das de baixo pras de cima.
            int count = w * h / 45;
            var parts = new List<(float X, float Y, int Kind, float Size, float Angle)>();
            for (int i = 0; i < count; i++)
            {
                float x = (float)rng.NextDouble() * (w - 6) + 3f;
                int ix = Math.Max(0, Math.Min(w - 1, (int)x));
                float room = h - top[ix];
                if (room < 3f) continue;

                // Mais pecas perto da superficie.
                float y = top[ix] + (float)Math.Pow(rng.NextDouble(), 1.8) * room * 0.9f + 1f;
                parts.Add((x, y, rng.Next(8), 0.7f + (float)rng.NextDouble() * 0.6f, (float)rng.NextDouble() * 360f));
            }

            parts.Sort((a, b) => b.Y.CompareTo(a.Y));
            foreach ((float X, float Y, int Kind, float Size, float Angle) part in parts)
            {
                DrawPart(c, part.X, part.Y, part.Kind, part.Size, part.Angle, seed + (int)part.X * 7 + (int)part.Y);
            }

            // Uma ou duas pecas grandes espetadas no alto: e o que da a silhueta.
            int peak = w / 2 + rng.Next(-w / 6, w / 6 + 1);
            float peakY = top[Math.Max(0, Math.Min(w - 1, peak))];
            ArtKit.Taper(c, peak, peakY + 4f, peak + (rng.Next(2) == 0 ? -1 : 1) * h * 0.25f, peakY - h * 0.22f, 1.8f, 1.2f,
                ArtKit.Corroded(ArtKit.Tin, 0.2f, seed + 5, 0.4f), 0.1f);
            if (w > 40) DrawPart(c, w * 0.3f, top[(int)(w * 0.3f)] + 1f, 1, 1.4f, 20f, seed + 9);

            // Sombra rente ao chao.
            for (int x = 0; x < w; x++)
            {
                for (int y = h - 3; y < h; y++)
                {
                    Rgba p = c.Get(x, y);
                    if (p.A > 0) c.Set(x, y, Rgba.Lerp(p, ArtKit.Iron[0], 0.5f + (y - (h - 3)) * 0.2f));
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma peca de sucata: 0 engrenagem, 1 cabeca, 2 cano, 3 chapa, 4 lata, 5 braco, 6 mola, 7 roda.</summary>
        private static void DrawPart(PixelCanvas c, float x, float y, int kind, float size, float angle, int seed)
        {
            Rgba[][] metals = { ArtKit.Brass, ArtKit.Copper, ArtKit.Tin, ArtKit.Iron, ArtKit.Rust };
            Rgba[] ramp = metals[ArtKit.Hash(seed, kind, 401) % metals.Length];

            switch (kind)
            {
                case 0:
                {
                    float r = 3f + size * 2.5f;
                    for (int py = (int)(y - r) - 2; py <= (int)(y + r) + 2; py++)
                    {
                        for (int px = (int)(x - r) - 2; px <= (int)(x + r) + 2; px++)
                        {
                            float dx = px + 0.5f - x;
                            float dy = py + 0.5f - y;
                            float d = (float)Math.Sqrt(dx * dx + dy * dy);
                            bool tooth = Math.Cos(Math.Atan2(dy, dx) * 8 + angle) > 0.3;
                            if (d > (tooth ? r + 1.3f : r) || d < r * 0.3f) continue;
                            c.Set(px, py, ArtKit.Tone(ramp, -(dx * 0.55f + dy * 0.65f) / Math.Max(d, 0.1f) * 0.8f));
                        }
                    }

                    break;
                }

                case 1:
                {
                    // Uma cabeca de autômato, de olhos vazios.
                    float r = 2.6f + size * 1.6f;
                    ArtKit.Ball(c, x, y, r, r * 0.9f, ArtKit.Corroded(ramp, 0.25f, seed, 0.5f), 0.1f);
                    int ex = (int)(x - r * 0.4f);
                    int ey = (int)y;
                    c.Set(ex, ey, Ink);
                    c.Set(ex + (int)Math.Max(2f, r * 0.8f), ey, Ink);
                    if (size > 1f) c.Fill(ex, ey + 2, (int)Math.Max(2f, r * 0.8f) + 1, 1, ramp[0]);
                    break;
                }

                case 2:
                {
                    (float tx, float ty) = ArtKit.Along(x, y, angle, 5f + size * 6f);
                    ArtKit.Limb(c, x, y, tx, ty, 1f + size * 0.7f, ramp, 0.1f);
                    c.Set((int)tx, (int)ty, Ink);
                    break;
                }

                case 3:
                {
                    // Uma chapa torta, com dois rebites.
                    float hw = 3f + size * 2.5f;
                    float hh = 2f + size * 1.2f;
                    double a = angle * Math.PI / 180.0 * 0.25;
                    float ca = (float)Math.Cos(a);
                    float sa = (float)Math.Sin(a);
                    var corners = new[]
                    {
                        (x - hw * ca + hh * sa, y - hw * sa - hh * ca), (x + hw * ca + hh * sa, y + hw * sa - hh * ca),
                        (x + hw * ca - hh * sa, y + hw * sa + hh * ca), (x - hw * ca - hh * sa, y - hw * sa + hh * ca),
                    };
                    ArtKit.Polygon(c, corners, (px, py) => ArtKit.Tone(ramp, 0.5f - (py - y + hh) / (hh * 2f) * 1.2f));
                    c.Set((int)(x - hw * 0.6f), (int)y, ramp[3]);
                    c.Set((int)(x + hw * 0.6f), (int)y, ramp[3]);
                    break;
                }

                case 4:
                    ArtKit.Drum(c, x, y, 2f + size, 2.5f + size * 1.2f, 3f, ArtKit.Plain(ArtKit.Tin), 0.1f);
                    c.Set((int)x, (int)(y - 1f), new Rgba(160, 60, 50));
                    break;

                case 5:
                {
                    // Um braco: antebraco e mao de tres dedos.
                    (float ex, float ey) = ArtKit.Along(x, y, angle, 4f + size * 3f);
                    (float hx, float hy) = ArtKit.Along(ex, ey, angle + 35f, 3f + size * 2f);
                    ArtKit.Limb(c, x, y, ex, ey, 1.2f, ramp, 0.1f);
                    ArtKit.Limb(c, ex, ey, hx, hy, 1f, ramp, 0f);
                    ArtKit.Ball(c, ex, ey, 1.4f, 1.4f, ArtKit.Iron, 0.2f);
                    for (int f = -1; f <= 1; f++)
                    {
                        (float fx, float fy) = ArtKit.Along(hx, hy, angle + 35f + f * 30f, 2f);
                        c.Set((int)fx, (int)fy, ramp[2]);
                    }

                    break;
                }

                case 6:
                {
                    // Uma mola esticada.
                    for (int i = 0; i < 10; i++)
                    {
                        (float sx, float sy) = ArtKit.Along(x, y, angle, i * (0.8f + size * 0.4f));
                        c.Set((int)sx, (int)(sy + (i % 2 == 0 ? -1 : 1)), ArtKit.Brass[i % 2 == 0 ? 3 : 1]);
                    }

                    break;
                }

                default:
                {
                    // Uma roda: aro e cubo.
                    float r = 3f + size * 2f;
                    for (int i = 0; i < 32; i++)
                    {
                        double a = i * Math.PI / 16.0;
                        int px = (int)Math.Round(x + Math.Cos(a) * r);
                        int py = (int)Math.Round(y + Math.Sin(a) * r);
                        c.Set(px, py, ArtKit.Tone(ArtKit.Rubber, (float)-Math.Sin(a) * 0.8f));
                        c.Set(px + 1, py, ArtKit.Rubber[1]);
                    }

                    ArtKit.Ball(c, x, y, 1.5f, 1.5f, ArtKit.Iron, 0.2f);
                    break;
                }
            }
        }

        /// <summary>
        /// Um descartado que nao levantou mais, 24x24: sentado contra a
        /// parede, as pernas esticadas, a cabeca caida no peito, os olhos
        /// apagados. 0 sem um braco, 1 com o peito aberto, 2 so o tronco e a
        /// cabeca, deitado de lado.
        /// </summary>
        private static PixelCanvas DrawCarcass(int variant)
        {
            var c = new PixelCanvas(24, 24);
            Rgba[] body = variant == 1 ? ArtKit.Copper : ArtKit.Tin;
            ArtKit.Skin skin = ArtKit.Corroded(body, 0.35f, 411 + variant, 0.45f);

            if (variant == 2)
            {
                // Deitado de lado, sem pernas, o braco estendido.
                ArtKit.Drum(c, 11f, 19f, 7f, 4.5f, 3f, skin, 0f);
                ArtKit.Ball(c, 20f, 19f, 3.6f, 3.4f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 421, 0.5f, ArtKit.Rust), 0.1f);
                c.Set(21, 18, Ink);
                ArtKit.Limb(c, 9f, 16f, 3f, 21f, 1.2f, ArtKit.Iron, -0.1f);
                ArtKit.Ball(c, 2.5f, 21.5f, 1.6f, 1.4f, ArtKit.Iron, 0f);
                ArtKit.Line(c, 4, 18, 2, 15, new Rgba(160, 60, 40));
                c.Fill(4, 23, 20, 1, ArtKit.Iron[0]);
                ArtKit.AddOutline(c, Ink);
                return c;
            }

            // As pernas esticadas no chao.
            ArtKit.Limb(c, 10f, 19f, 20f, 21.5f, 1.8f, ArtKit.Iron, -0.3f);
            ArtKit.Limb(c, 9f, 20f, 19f, 22f, 2f, body, 0f);
            ArtKit.Ball(c, 21f, 21.5f, 2.2f, 1.8f, ArtKit.Rubber, 0f);

            // O tronco encostado na parede, meio torto.
            ArtKit.Drum(c, 8f, 15f, 5.5f, 6f, 3f, skin, 0.05f);
            if (variant == 1)
            {
                // O peito aberto: uma portinhola caida e o escuro de dentro com molas.
                c.Fill(7, 13, 4, 4, ArtKit.Iron[0]);
                c.Set(8, 14, ArtKit.Brass[2]);
                c.Set(9, 15, ArtKit.Brass[1]);
                ArtKit.Line(c, 11, 17, 14, 19, body[2]);
            }

            for (int i = 0; i < 3; i++) ArtKit.Rivet(c, 4 + i * 3, 11, ArtKit.Iron);

            // A cabeca, caida pro peito.
            ArtKit.Ball(c, 11f, 8.5f, 4f, 3.6f, ArtKit.Corroded(body, 0.3f, 431 + variant, 0.5f), 0.15f);
            c.Fill(11, 8, 2, 1, Ink);
            c.Set(14, 8, Ink);
            c.Set(13, 11, body[0]);

            if (variant == 0)
            {
                // O ombro sem braco: o coto e os fios.
                ArtKit.Ball(c, 12f, 13f, 1.6f, 1.6f, ArtKit.Iron, 0f);
                ArtKit.Line(c, 13, 14, 14, 17, new Rgba(170, 60, 40));
                ArtKit.Line(c, 12, 14, 12, 18, new Rgba(60, 90, 140));
            }
            else
            {
                // O braco caido no colo.
                ArtKit.Limb(c, 11f, 13f, 15f, 18f, 1.3f, body, 0.1f);
                ArtKit.Ball(c, 15.5f, 18.5f, 1.6f, 1.6f, body, 0.2f);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um barril de chapa, 16x20, amassado e verde embaixo.</summary>
        private static PixelCanvas DrawBarrel()
        {
            var c = new PixelCanvas(16, 20);
            ArtKit.Drum(c, 8f, 10.5f, 6.5f, 9f, 3.5f, ArtKit.Corroded(ArtKit.Rust, 0.3f, 441, 0.35f), 0.05f);
            foreach (int y in new[] { 4, 16 })
            {
                for (int px = 2; px < 15; px++) c.Set(px, y, ArtKit.Tone(ArtKit.Iron, 0.5f - (px - 8) * 0.15f));
            }

            // O simbolo de perigo, meio apagado.
            c.Fill(6, 9, 4, 3, new Rgba(190, 160, 70));
            c.Set(7, 10, Ink);
            c.Set(8, 10, Ink);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um barril tombado, 26x16, com a tampa solta e o Oxide vazando.</summary>
        private static PixelCanvas DrawBarrelTipped()
        {
            var c = new PixelCanvas(26, 16);
            for (int py = 2; py < 15; py++)
            {
                for (int px = 2; px < 20; px++)
                {
                    float v = (py + 0.5f - 8.5f) / 6.5f;
                    if (Math.Abs(v) > 1f) continue;
                    c.Set(px, py, ArtKit.Tone(ArtKit.Rust, -0.9f * v + 0.1f));
                }
            }

            foreach (int x in new[] { 6, 15 })
            {
                for (int py = 2; py < 15; py++) c.Set(x, py, ArtKit.Iron[1]);
            }

            // A boca, aberta pra direita.
            ArtKit.Ball(c, 20f, 8.5f, 2.5f, 6.5f, ArtKit.Iron, -0.4f);
            ArtKit.Ball(c, 20.5f, 8.5f, 1.5f, 5f, new[] { ArtKit.Iron[0], ArtKit.Iron[0], ArtKit.Iron[1], ArtKit.Iron[1] }, 0f);
            ArtKit.AddOutline(c, Ink);

            // O verde escorrendo da boca pro chao.
            for (int px = 19; px < 26; px++) c.Set(px, 15, px < 24 ? ArtKit.OxideGlow : ArtKit.OxideDeep);
            c.Set(21, 14, ArtKit.OxideGlow);
            c.Set(22, 15, ArtKit.OxideCore);
            return c;
        }

        /// <summary>Um carrinho de sucata, 40x28, com uma roda solta e o caixote cheio.</summary>
        private static PixelCanvas DrawCart()
        {
            var c = new PixelCanvas(40, 28);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.25f, 451, 0.3f, ArtKit.Rust);

            // A sucata dentro, saindo por cima.
            DrawPart(c, 10f, 9f, 1, 1.1f, 0f, 452);
            DrawPart(c, 20f, 8f, 0, 1f, 10f, 453);
            DrawPart(c, 28f, 10f, 2, 1.2f, -60f, 454);
            DrawPart(c, 15f, 11f, 3, 0.9f, 30f, 455);

            // O caixote, mais largo em cima.
            var box = new[] { (3f, 10f), (37f, 10f), (34f, 22f), (6f, 22f) };
            ArtKit.Polygon(c, box, (px, py) => iron(px, py, 0.35f - (py - 10f) * 0.08f - (px - 20f) * 0.02f));
            for (int px = 4; px < 36; px++) c.Set(px, 10, ArtKit.Iron[3]);
            for (int i = 0; i < 6; i++) ArtKit.Rivet(c, 7 + i * 5, 12, ArtKit.Iron);

            // As rodas: uma no lugar, a outra caida, encostada.
            DrawPart(c, 11f, 23.5f, 7, 0.8f, 0f, 456);
            ArtKit.Drum(c, 31f, 25.5f, 5f, 2f, 2f, ArtKit.Plain(ArtKit.Rubber), 0.1f);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma roda grande de carroca, 24x24, encostada.</summary>
        private static PixelCanvas DrawWheel()
        {
            var c = new PixelCanvas(24, 24);
            for (int py = 0; py < 24; py++)
            {
                for (int px = 0; px < 24; px++)
                {
                    float dx = px + 0.5f - 12f;
                    float dy = py + 0.5f - 12f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    double a = Math.Atan2(dy, dx);
                    float light = -(dx * 0.55f + dy * 0.65f) / Math.Max(d, 0.1f);

                    if (d <= 11f && d > 8.6f) c.Set(px, py, ArtKit.Tone(ArtKit.Leather, light * 0.8f));
                    else if (d <= 8.6f && d > 7.6f) c.Set(px, py, ArtKit.Iron[1]);
                    else if (d < 2.6f) c.Set(px, py, ArtKit.Tone(ArtKit.Iron, light));
                    else if (d < 8f && Math.Abs(Math.IEEERemainder(a, Math.PI / 4.0)) * d < 0.9f) c.Set(px, py, ArtKit.Tone(ArtKit.Leather, 0.2f));
                }
            }

            // Um raio quebrado.
            c.Set(17, 7, Rgba.Clear);
            c.Set(16, 8, Rgba.Clear);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Um poste de lampiao de cristal da Avenida das Facetas, 16x48,
        /// entortado na queda. Apagado: o vidro escuro e rachado. Aceso (so a
        /// luz, por cima do apagado): o cristal quente e o halo em volta.
        /// </summary>
        private static PixelCanvas DrawLampPost(bool glow)
        {
            var c = new PixelCanvas(16, 48);

            if (glow)
            {
                for (int py = 6; py < 16; py++)
                {
                    for (int px = 6; px < 13; px++)
                    {
                        bool frame = px == 6 || px == 12 || py == 6 || py == 15;
                        if (frame) continue;
                        float d = Math.Abs(px - 9f) / 3f + Math.Abs(py - 11f) / 5f;
                        c.Set(px, py, d < 0.5f ? new Rgba(255, 250, 225) : new Rgba(255, 210, 130));
                    }
                }

                ArtKit.Glow(c, 9.5f, 11f, 7.5f, new Rgba(255, 210, 130), 0.9f, true);
                return c;
            }

            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.2f, 461, 0.35f, ArtKit.Rust);

            // A base, o poste e a curva do alto.
            ArtKit.Drum(c, 6f, 45f, 4.5f, 2.5f, 3f, iron, 0.1f);
            ArtKit.Limb(c, 6f, 44f, 6.5f, 20f, 1.2f, iron, 0.1f);
            ArtKit.Limb(c, 6.5f, 20f, 9f, 16f, 1.1f, iron, 0.1f);
            ArtKit.Ball(c, 6.3f, 30f, 1.8f, 1f, ArtKit.Brass, 0.2f);

            // A gaiola do lampiao com o cristal apagado e rachado.
            for (int py = 5; py < 17; py++)
            {
                for (int px = 5; px < 14; px++)
                {
                    bool frame = px == 6 || px == 12 || py == 6 || py == 15;
                    if (px == 5 || px == 13 || py == 5 || py == 16) continue;
                    c.Set(px, py, frame ? ArtKit.Tone(ArtKit.Brass, px == 6 ? 0.6f : -0.2f) : new Rgba(56, 74, 80));
                }
            }

            ArtKit.Line(c, 8, 7, 10, 14, new Rgba(28, 36, 40));
            ArtKit.Line(c, 10, 9, 11, 12, new Rgba(28, 36, 40));
            c.Set(8, 8, new Rgba(150, 180, 186));
            c.Set(10, 12, new Rgba(90, 120, 130));

            // O chapeu e a ponta.
            ArtKit.Drum(c, 9.5f, 4.5f, 5f, 1.6f, 3f, ArtKit.Plain(ArtKit.Brass), 0.2f);
            ArtKit.Limb(c, 9.5f, 3f, 9.5f, 0.5f, 0.6f, ArtKit.Brass, 0.3f);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma poca de Oxide no chao, rasa e acesa.</summary>
        private static PixelCanvas DrawPuddle(int w, int h, int seed)
        {
            var c = new PixelCanvas(w, h);
            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    float dx = (px + 0.5f - w * 0.5f) / (w * 0.5f);
                    float dy = (py + 0.5f - h * 0.75f) / (h * 0.45f);
                    float wobble = ArtKit.Noise(px * 0.3f, seed, seed + 471) * 0.3f;
                    float d = dx * dx + dy * dy - wobble;
                    if (d > 1f) continue;

                    Rgba color = d < 0.25f ? ArtKit.OxideCore : (d < 0.6f ? ArtKit.OxideGlow : ArtKit.OxideDeep);
                    if (py < h * 0.45f && d > 0.25f) color = ArtKit.OxideDeep;
                    c.Set(px, py, color.WithAlpha(d > 0.85f ? 170 : 235));
                }
            }

            // Bolhinhas.
            c.Set(w / 3, h / 2, ArtKit.OxideCore);
            c.Set(w * 2 / 3, h / 2 + 1, ArtKit.OxideCore);
            return c;
        }

        /// <summary>Parafusos, porcas, molinhas e engrenagenzinhas espalhados no chao, 32x8.</summary>
        private static PixelCanvas DrawBolts()
        {
            var c = new PixelCanvas(32, 8);
            var rng = new Random(481);

            for (int i = 0; i < 14; i++)
            {
                int x = rng.Next(1, 31);
                int y = rng.Next(4, 8);
                Rgba[] ramp = i % 3 == 0 ? ArtKit.Brass : (i % 3 == 1 ? ArtKit.Iron : ArtKit.Copper);

                switch (i % 4)
                {
                    case 0:
                        c.Set(x, y, ramp[3]);
                        c.Set(x + 1, y, ramp[1]);
                        c.Set(x, y - 1, ramp[2]);
                        break;
                    case 1:
                        c.Set(x, y, ramp[2]);
                        c.Set(x + 1, y, ramp[2]);
                        c.Set(x + 2, y, ramp[1]);
                        break;
                    case 2:
                        c.Set(x, y, ramp[1]);
                        c.Set(x, y - 1, ramp[3]);
                        c.Set(x + 1, y - 1, ramp[1]);
                        c.Set(x - 1, y - 1, ramp[2]);
                        break;
                    default:
                        c.Set(x, y, ramp[2]);
                        break;
                }
            }

            return c;
        }

        /// <summary>Uma caldeira furada, 40x32, deitada de lado: o rombo na chapa, os tubos de dentro aparecendo.</summary>
        private static PixelCanvas DrawBoilerWreck()
        {
            var c = new PixelCanvas(40, 32);
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.4f, 491, 0.25f);

            for (int py = 6; py < 31; py++)
            {
                for (int px = 2; px < 36; px++)
                {
                    float v = (py + 0.5f - 18.5f) / 12.5f;
                    float u = (px + 0.5f - 19f) / 17f;
                    if (Math.Abs(v) > 1f || Math.Pow(Math.Abs(u), 4) + v * v > 1.05f) continue;
                    c.Set(px, py, brass(px, py, -0.9f * v + 0.15f - Math.Abs(u) * 0.3f));
                }
            }

            foreach (int x in new[] { 9, 19, 29 })
            {
                for (int py = 6; py < 31; py++)
                {
                    if (c.Get(x, py).A == 0) continue;
                    c.Set(x, py, ArtKit.Iron[1]);
                    if (py % 3 == 0) c.Set(x, py, ArtKit.Iron[3]);
                }
            }

            // O rombo: o escuro de dentro, os tubos e a chapa virada pra fora.
            for (int py = 11; py < 22; py++)
            {
                for (int px = 18; px < 30; px++)
                {
                    float d = Math.Abs(px - 23.5f) / 6f + Math.Abs(py - 16.5f) / 5.5f + ArtKit.Noise(px * 0.6f, py * 0.6f, 492) * 0.3f;
                    if (d > 1f) continue;
                    c.Set(px, py, (py - 11) % 3 == 1 ? ArtKit.Copper[1] : ArtKit.Iron[0]);
                }
            }

            ArtKit.Line(c, 29, 12, 34, 8, ArtKit.Brass[3]);
            ArtKit.Line(c, 29, 13, 33, 10, ArtKit.Brass[1]);

            // O domo de cima e a valvula quebrada.
            ArtKit.Ball(c, 12f, 6f, 4f, 3f, ArtKit.Brass, 0.2f);
            ArtKit.Limb(c, 12f, 4f, 13f, 0.5f, 0.9f, ArtKit.Iron, 0.2f);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma pilha de canos, 40x16, uns por cima dos outros, vistos de ponta.</summary>
        private static PixelCanvas DrawPipePile()
        {
            var c = new PixelCanvas(40, 16);
            (float X, float Y, float R)[] pipes =
            {
                (5f, 11.5f, 4f), (13f, 11.5f, 4f), (21f, 11.5f, 4f), (29f, 11.5f, 4f), (36f, 12.5f, 3f),
                (9f, 5f, 4f), (17f, 5f, 4f), (25f, 5.5f, 3.5f),
            };

            foreach ((float X, float Y, float R) pipe in pipes)
            {
                Rgba[] ramp = ((int)pipe.X % 3) switch { 0 => ArtKit.Copper, 1 => ArtKit.Tin, _ => ArtKit.Iron };
                ArtKit.Ball(c, pipe.X, pipe.Y, pipe.R, pipe.R, ramp, 0.2f);
                ArtKit.Ball(c, pipe.X + 0.3f, pipe.Y + 0.3f, pipe.R * 0.55f, pipe.R * 0.55f,
                    new[] { ArtKit.Iron[0], ArtKit.Iron[0], ArtKit.Iron[1], ArtKit.Iron[1] }, -0.5f);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
