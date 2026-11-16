using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Bancada de Ourives, o checkpoint: um altar de pedra e latao com a
    /// bigorna de um lado, o cadinho do outro e, no meio, uma coluna que
    /// segura uma engrenagem com a gema do salvamento flutuando dentro.
    ///
    /// 96x96 (3x3 tiles a 32 px), pivo no canto de baixo a esquerda. Uma folha
    /// so, na ordem das constantes:
    ///   Dormant  apagada: gema cinza-azulada respirando devagar, cadinho frio.
    ///   Lit      acesa (a ultima em que se salvou): gema ciano, engrenagem
    ///            girando, brasas no cadinho, brilhos em volta.
    ///   Forge    o salvamento: a gema esquenta ate o branco e solta raios,
    ///            e volta a acesa.
    ///
    /// Nao depende de UnityEngine: da pra renderizar pelo console.
    /// </summary>
    public static class CheckpointArt
    {
        public const int Size = 96;
        public const int PixelsPerUnit = 32;

        public const int Dormant = 0;
        public const int DormantFrames = 4;
        public const int Lit = 4;
        public const int LitFrames = 6;
        public const int Forge = 10;
        public const int ForgeFrames = 6;
        public const int FrameCount = 16;

        private const float GemX = 48f;
        private const float GemY = 34f;

        private static readonly Rgba[] Stone =
            { new Rgba(38, 36, 46), new Rgba(62, 58, 72), new Rgba(90, 86, 100), new Rgba(124, 120, 132) };

        private static readonly Rgba[] GemCold =
            { new Rgba(34, 44, 62), new Rgba(52, 70, 94), new Rgba(78, 100, 126), new Rgba(112, 136, 162) };

        private static readonly Rgba[] GemLit =
            { new Rgba(24, 104, 150), new Rgba(60, 182, 228), new Rgba(146, 232, 255), new Rgba(236, 252, 255) };

        private static readonly Rgba White = new Rgba(255, 253, 240);
        private static readonly Rgba Ember = new Rgba(255, 150, 60);
        private static readonly Rgba EmberHot = new Rgba(255, 226, 140);
        private static readonly Rgba Cyan = new Rgba(120, 230, 255);

        /// <summary>Todos os quadros, na ordem das constantes.</summary>
        public static PixelCanvas[] Frames()
        {
            var frames = new List<PixelCanvas>();

            for (int i = 0; i < DormantFrames; i++)
            {
                float breath = 0.5f - 0.5f * (float)Math.Cos(i / (float)DormantFrames * Math.PI * 2);
                frames.Add(Draw(0f, breath, 0f, i));
            }

            for (int i = 0; i < LitFrames; i++)
            {
                frames.Add(Draw(1f, 0.5f + 0.5f * (float)Math.Sin(i / (float)LitFrames * Math.PI * 2), 0f, i));
            }

            // O salvamento: sobe rapido ate o branco e desce devagar.
            float[] heat = { 0.35f, 0.75f, 1f, 0.8f, 0.5f, 0.2f };
            for (int i = 0; i < ForgeFrames; i++)
            {
                frames.Add(Draw(1f, 1f, heat[i], i));
            }

            return frames.ToArray();
        }

        /// <param name="lit">0 apagada, 1 acesa.</param>
        /// <param name="pulse">0 a 1: o respiro da gema.</param>
        /// <param name="heat">0 a 1: o quanto ela esta no clarao do salvamento.</param>
        /// <param name="step">O quadro dentro da animacao (gira a engrenagem, mexe as brasas).</param>
        private static PixelCanvas Draw(float lit, float pulse, float heat, int step)
        {
            var canvas = new PixelCanvas(Size, Size);
            bool on = lit > 0.5f;

            // A luz por tras de tudo: o halo da gema.
            if (on) Halo(canvas, GemX, GemY, 30f + 10f * heat, Cyan, 0.22f + 0.08f * pulse + 0.35f * heat);
            else Halo(canvas, GemX, GemY, 20f, new Rgba(120, 150, 190), 0.05f + 0.05f * pulse);

            // O vidro dentro da engrenagem: escuro apagada, banhado de luz acesa.
            Rgba glass = on
                ? Rgba.Lerp(new Rgba(40, 120, 160, 200), new Rgba(200, 245, 255, 230), 0.15f * pulse + 0.7f * heat)
                : new Rgba(26, 30, 44, 210);
            Disc(canvas, GemX, GemY, 16.5f, glass, on ? new Rgba(150, 230, 255, 120) : new Rgba(70, 84, 110, 120));

            Plinth(canvas);
            Column(canvas);
            Anvil(canvas);
            Crucible(canvas, on, step, heat);

            // A engrenagem gira acesa: 5 graus por quadro, 6 quadros = um dente.
            float turn = on ? step * 5f : 0f;
            Gear(canvas, GemX, GemY, turn);
            Finial(canvas, on, heat);

            Rgba[] gem = on ? Heat(GemLit, heat) : Dim(GemCold, 0.86f + 0.14f * pulse);
            Gem(canvas, GemX, GemY, 9.5f, 14f, gem);

            Outline(canvas);

            // O que brilha por cima do contorno.
            if (on)
            {
                Sparkles(canvas, step, heat);
                if (heat > 0.3f) Rays(canvas, GemX, GemY, heat);
            }
            else
            {
                // Apagada, um fio de luz ainda corre na gema: e um lugar pra voltar.
                canvas.Blend((int)GemX - 3, (int)GemY - 8 + step, new Rgba(170, 200, 230, (int)(90 + 80 * pulse)));
            }

            return canvas;
        }

        // ------------------------------------------------------------------
        // As pecas
        // ------------------------------------------------------------------

        /// <summary>O altar: dois degraus de pedra, frisos de latao e a placa com a gema gravada.</summary>
        private static void Plinth(PixelCanvas canvas)
        {
            // Degrau de baixo, blocos de pedra.
            for (int y = 88; y < 96; y++)
            {
                for (int x = 2; x < 94; x++)
                {
                    bool mortar = y == 91 || (x + (y > 91 ? 8 : 0)) % 16 == 0;
                    float light = 0.35f - (y - 88) * 0.12f - (x - 48) * 0.004f;
                    canvas.Set(x, y, mortar ? Stone[0] : ArtKit.Tone(Stone, light));
                }
            }

            // Degrau de cima.
            for (int y = 78; y < 88; y++)
            {
                for (int x = 8; x < 88; x++)
                {
                    bool mortar = y == 83 || (x + (y > 83 ? 6 : 0)) % 20 == 0;
                    float light = 0.5f - (y - 78) * 0.09f - (x - 48) * 0.005f;
                    canvas.Set(x, y, mortar ? Stone[0] : ArtKit.Tone(Stone, light));
                }
            }

            // Frisos de latao com rebites.
            canvas.Fill(6, 76, 84, 2, ArtKit.Brass[2]);
            canvas.Fill(6, 76, 84, 1, ArtKit.Brass[3]);
            canvas.Fill(6, 78, 84, 1, ArtKit.Brass[0]);
            canvas.Fill(1, 87, 94, 1, ArtKit.Brass[1]);
            canvas.Fill(1, 86, 94, 1, ArtKit.Brass[2]);
            for (int x = 10; x < 88; x += 9) ArtKit.Rivet(canvas, x, 80, ArtKit.Brass);

            // A placa no meio, com a gema gravada.
            canvas.Fill(38, 79, 20, 8, ArtKit.Brass[1]);
            canvas.Fill(38, 79, 20, 1, ArtKit.Brass[3]);
            canvas.Fill(38, 86, 20, 1, ArtKit.Brass[0]);
            canvas.Fill(39, 80, 1, 6, ArtKit.Brass[2]);
            int[] gemRows = { 1, 3, 5, 3, 1 };
            for (int i = 0; i < gemRows.Length; i++)
            {
                canvas.Fill(48 - gemRows[i] / 2, 80 + i, gemRows[i], 1, i < 2 ? ArtKit.Brass[3] : ArtKit.Brass[0]);
            }
        }

        /// <summary>A coluna de latao canelada que segura a engrenagem.</summary>
        private static void Column(PixelCanvas canvas)
        {
            ArtKit.Drum(canvas, 48f, 64f, 6f, 13f, 6f, ArtKit.Plain(ArtKit.Brass));

            // Caneluras.
            for (int x = 44; x <= 52; x += 3)
            {
                for (int y = 53; y < 75; y++) canvas.Set(x, y, ArtKit.Shift(ArtKit.Brass, canvas.Get(x, y), -1));
            }

            // Base e capitel.
            ArtKit.Drum(canvas, 48f, 75f, 9f, 2f, 4f, ArtKit.Plain(ArtKit.Brass));
            ArtKit.Drum(canvas, 48f, 52f, 8f, 2f, 4f, ArtKit.Plain(ArtKit.Brass), 0.2f);

            // Os bracos que sobem ate a engrenagem.
            ArtKit.Limb(canvas, 42f, 52f, 31f, 44f, 1.6f, ArtKit.Brass);
            ArtKit.Limb(canvas, 54f, 52f, 65f, 44f, 1.6f, ArtKit.Brass);
        }

        /// <summary>A bigorna, com o martelo apoiado.</summary>
        private static void Anvil(PixelCanvas canvas)
        {
            // Pe.
            ArtKit.Drum(canvas, 22f, 72f, 5f, 4f, 3f, ArtKit.Plain(ArtKit.Iron));
            // Corpo e chifre.
            ArtKit.Drum(canvas, 23f, 65f, 9f, 3.2f, 5f, ArtKit.Plain(ArtKit.Iron), 0.15f);
            ArtKit.Taper(canvas, 14f, 64f, 6f, 65.5f, 2.6f, 0.6f, ArtKit.Plain(ArtKit.Iron), 0.2f);
            canvas.Fill(15, 62, 17, 1, ArtKit.Iron[3]);

            // O martelo encostado, cabo de madeira.
            ArtKit.Limb(canvas, 34f, 75f, 30f, 57f, 0.9f, ArtKit.Leather);
            ArtKit.Drum(canvas, 30f, 56f, 3.5f, 2f, 4f, ArtKit.Plain(ArtKit.Iron), 0.2f);
        }

        /// <summary>O cadinho no tripe: frio apagado, com brasas acesa.</summary>
        private static void Crucible(PixelCanvas canvas, bool on, int step, float heat)
        {
            ArtKit.Line(canvas, 68, 76, 72, 66, ArtKit.Iron[2]);
            ArtKit.Line(canvas, 80, 76, 76, 66, ArtKit.Iron[2]);
            ArtKit.Line(canvas, 74, 76, 74, 67, ArtKit.Iron[1]);

            ArtKit.Ball(canvas, 74f, 63f, 8f, 5f, ArtKit.Copper);
            canvas.Fill(66, 59, 17, 2, ArtKit.Copper[3]);
            canvas.Fill(67, 61, 15, 1, ArtKit.Copper[0]);

            // A boca: brasa ou cinza.
            for (int x = 68; x < 81; x++)
            {
                if (on)
                {
                    bool hot = ((x * 7 + step * 3) % 5) < 2;
                    canvas.Set(x, 60, hot ? EmberHot : Ember);
                }
                else
                {
                    canvas.Set(x, 60, (x % 3 == 0) ? ArtKit.Iron[2] : ArtKit.Iron[1]);
                }
            }

            if (!on) return;

            // Fagulhas subindo, em lugares diferentes a cada quadro.
            for (int i = 0; i < 3; i++)
            {
                int x = 70 + (i * 5 + step * 3) % 10;
                int y = 57 - (step * 2 + i * 4) % 10;
                canvas.Blend(x, y, (i + step) % 2 == 0 ? EmberHot : Ember.WithAlpha(200));
            }

            if (heat > 0.5f) Halo(canvas, 74f, 59f, 8f, Ember, 0.4f * heat);
        }

        /// <summary>A engrenagem que segura a gema, com os dentes por fora.</summary>
        private static void Gear(PixelCanvas canvas, float cx, float cy, float turn)
        {
            const float outer = 20f;
            const float inner = 16f;
            const int teeth = 12;

            for (int y = (int)(cy - outer - 4); y <= (int)(cy + outer + 4); y++)
            {
                for (int x = (int)(cx - outer - 4); x <= (int)(cx + outer + 4); x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI - turn;
                    double phase = ((angle % (360.0 / teeth)) + 360.0) % (360.0 / teeth);
                    bool tooth = r >= outer && r < outer + 3.2f && phase < 360.0 / teeth * 0.45;

                    if (!(r >= inner && r < outer) && !tooth) continue;

                    // Luz de cima a esquerda na borda de fora; a de dentro, ao contrario.
                    float nx = dx / Math.Max(0.01f, r);
                    float ny = dy / Math.Max(0.01f, r);
                    float light = -0.7f * nx - 0.7f * ny;
                    if (r < inner + 1.2f) light = -light - 0.3f;
                    if (tooth) light -= 0.15f;

                    canvas.Set(x, y, ArtKit.Tone(ArtKit.Brass, light));
                }
            }

            // Os parafusos que prendem a gema, nos quatro pontos.
            for (int i = 0; i < 4; i++)
            {
                double a = (45 + i * 90 + turn) * Math.PI / 180.0;
                int bx = (int)Math.Round(cx + Math.Cos(a) * (outer - 2f)) - 1;
                int by = (int)Math.Round(cy + Math.Sin(a) * (outer - 2f)) - 1;
                ArtKit.Rivet(canvas, bx, by, ArtKit.Iron);
            }

            // As garras que seguram a gema: de cima e de baixo.
            canvas.Fill((int)cx - 1, (int)(cy - inner), 2, 3, ArtKit.Brass[2]);
            canvas.Fill((int)cx - 1, (int)(cy + inner) - 3, 2, 3, ArtKit.Brass[1]);
        }

        /// <summary>A pontinha em cima da engrenagem: uma coroa pequena de latao.</summary>
        private static void Finial(PixelCanvas canvas, bool on, float heat)
        {
            ArtKit.Taper(canvas, 48f, 13f, 48f, 4f, 2.4f, 0.5f, ArtKit.Plain(ArtKit.Brass), 0.2f);
            ArtKit.Ball(canvas, 48f, 3.5f, 1.8f, 1.8f, on ? Heat(GemLit, heat) : GemCold);
            canvas.Fill(42, 12, 13, 2, ArtKit.Brass[2]);
            canvas.Fill(42, 12, 13, 1, ArtKit.Brass[3]);
            canvas.Set(42, 11, ArtKit.Brass[3]);
            canvas.Set(54, 11, ArtKit.Brass[3]);
        }

        /// <summary>A gema lapidada: quatro faces, a cintura e o brilho na face de cima.</summary>
        private static void Gem(PixelCanvas canvas, float cx, float cy, float hw, float hh, Rgba[] ramp)
        {
            float girdle = cy - hh * 0.3f;

            for (int y = (int)(cy - hh) - 1; y <= (int)(cy + hh) + 1; y++)
            {
                for (int x = (int)(cx - hw) - 1; x <= (int)(cx + hw) + 1; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float dx = px - cx;

                    // Em cima do cinto a gema e mais curta: a coroa da lapidacao.
                    float top = py < girdle ? (girdle - py) / (girdle - (cy - hh)) : 0f;
                    float bottom = py >= girdle ? (py - girdle) / (cy + hh - girdle) : 0f;
                    float halfWidth = py < girdle ? hw * (1f - 0.55f * top) : hw * (1f - bottom);
                    if (Math.Abs(dx) > halfWidth || py < cy - hh || py > cy + hh) continue;

                    int tone;
                    if (py < girdle) tone = dx < -halfWidth * 0.3f ? 3 : dx < halfWidth * 0.35f ? 2 : 1;
                    else tone = dx < 0 ? (dx < -halfWidth * 0.5f ? 2 : 1) : (dx > halfWidth * 0.4f ? 0 : 1);

                    // A cintura: uma linha clara.
                    if (Math.Abs(py - girdle) < 0.6f) tone = 3;
                    // A quilha do meio, embaixo.
                    if (py > girdle && Math.Abs(dx) < 0.6f) tone = 2;

                    canvas.Set(x, y, ramp[tone]);
                }
            }

            // O brilho.
            canvas.Set((int)(cx - hw * 0.45f), (int)(girdle - 2), ramp[3].Scaled(1.15f));
            canvas.Set((int)(cx - hw * 0.45f) + 1, (int)(girdle - 3), ramp[3].Scaled(1.15f));
        }

        // ------------------------------------------------------------------
        // Luz
        // ------------------------------------------------------------------

        /// <summary>Um disco de vidro translucido, com um reflexo curvo em cima a esquerda.</summary>
        private static void Disc(PixelCanvas canvas, float cx, float cy, float radius, Rgba fill, Rgba shine)
        {
            for (int y = (int)(cy - radius) - 1; y <= (int)(cy + radius) + 1; y++)
            {
                for (int x = (int)(cx - radius) - 1; x <= (int)(cx + radius) + 1; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r > radius) continue;

                    canvas.Set(x, y, fill);

                    // O reflexo: um arco fino perto da borda, em cima a esquerda.
                    double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                    if (r > radius - 3f && r < radius - 1.6f && angle > -165 && angle < -110) canvas.Set(x, y, shine);
                }
            }
        }

        /// <summary>Um halo que so pinta onde ainda nao tem nada (fica atras do desenho).</summary>
        private static void Halo(PixelCanvas canvas, float cx, float cy, float radius, Rgba color, float strength)
        {
            for (int y = (int)(cy - radius); y <= (int)(cy + radius); y++)
            {
                for (int x = (int)(cx - radius); x <= (int)(cx + radius); x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy) / radius;
                    if (d >= 1f) continue;

                    // Em degraus, pra ficar pixelado como o resto.
                    float k = (float)Math.Pow(1f - d, 1.6f) * strength;
                    k = (float)Math.Round(k * 6f) / 6f;
                    if (k <= 0f) continue;

                    canvas.Blend(x, y, color.WithAlpha((int)(Math.Min(1f, k) * 255)));
                }
            }
        }

        /// <summary>Estrelinhas de quatro pontas em volta da gema, trocando de lugar a cada quadro.</summary>
        private static void Sparkles(PixelCanvas canvas, int step, float heat)
        {
            (int X, int Y)[] spots = { (20, 22), (76, 18), (70, 50), (26, 46), (60, 8), (34, 10) };

            for (int i = 0; i < spots.Length; i++)
            {
                int phase = (step + i * 2) % 6;
                if (phase > 2) continue;

                int arm = phase == 1 ? 2 : 1;
                Rgba color = phase == 1 ? White : Cyan;
                var (x, y) = spots[i];

                canvas.Blend(x, y, color);
                for (int a = 1; a <= arm; a++)
                {
                    Rgba fade = color.WithAlpha(a == arm ? 150 : 230);
                    canvas.Blend(x + a, y, fade);
                    canvas.Blend(x - a, y, fade);
                    canvas.Blend(x, y + a, fade);
                    canvas.Blend(x, y - a, fade);
                }
            }
        }

        /// <summary>Os raios do clarao, saindo da gema em oito direcoes.</summary>
        private static void Rays(PixelCanvas canvas, float cx, float cy, float heat)
        {
            int length = (int)(10 + 24 * heat);

            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4 + Math.PI / 8;
                for (int s = 12; s < 12 + length; s++)
                {
                    int x = (int)Math.Round(cx + Math.Cos(a) * s);
                    int y = (int)Math.Round(cy + Math.Sin(a) * s);
                    float fade = 1f - (s - 12f) / length;
                    if ((s & 1) == 0 || fade > 0.6f) canvas.Blend(x, y, White.WithAlpha((int)(fade * 220 * heat)));
                }
            }
        }

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        /// <summary>O contorno escuro em volta de tudo que e solido (o halo nao conta).</summary>
        private static void Outline(PixelCanvas canvas)
        {
            var solid = new bool[canvas.Pixels.Length];
            for (int i = 0; i < solid.Length; i++) solid[i] = canvas.Pixels[i].A == 255;

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    if (solid[x + y * canvas.Width]) continue;

                    bool edge = false;
                    for (int oy = -1; oy <= 1 && !edge; oy++)
                    {
                        for (int ox = -1; ox <= 1 && !edge; ox++)
                        {
                            if ((ox == 0) == (oy == 0)) continue;
                            int nx = x + ox, ny = y + oy;
                            edge = canvas.Inside(nx, ny) && solid[nx + ny * canvas.Width];
                        }
                    }

                    if (edge) canvas.Set(x, y, ArtKit.Outline);
                }
            }
        }

        /// <summary>A rampa acesa esquentando ate o branco.</summary>
        private static Rgba[] Heat(Rgba[] ramp, float heat)
        {
            var hot = new Rgba[ramp.Length];
            for (int i = 0; i < ramp.Length; i++) hot[i] = Rgba.Lerp(ramp[i], White, heat * (0.5f + 0.15f * i));
            return hot;
        }

        private static Rgba[] Dim(Rgba[] ramp, float factor)
        {
            var dim = new Rgba[ramp.Length];
            for (int i = 0; i < ramp.Length; i++) dim[i] = ramp[i].Scaled(factor);
            return dim;
        }
    }
}
