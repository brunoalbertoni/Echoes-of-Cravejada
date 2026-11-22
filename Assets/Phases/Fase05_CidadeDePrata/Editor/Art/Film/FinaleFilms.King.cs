using System;
using static EchoesOfCravejada.EditorTools.FilmPaint;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Transformacao, em cinco planos:
    ///
    ///   1 A coroa      de perto, de baixo: a rachadura abre no quartzo e o
    ///                  verde vaza; os olhos dele esverdeiam.
    ///   2 A Coluna     a camera parte do Coracao, la no fundo, e sobe junto
    ///                  com o Oxide pela Coluna, andar por andar, ate o palacio.
    ///   3 O janelao    por tras dele, ajoelhado: o janelao explode em verde e
    ///                  a luz doente vem buscar o Rei.
    ///   4 A armadura   o peito de prata: o azinhavre come a prata, as placas
    ///                  descascam.
    ///   5 Ele se ergue de baixo, enorme, contra a abobada verde; o Scrap,
    ///                  pequeno, na frente. Sobe ao branco (volta pro jogo).
    /// </summary>
    public static partial class FinaleFilms
    {
        public static FilmDraft King()
        {
            var film = new FilmDraft();
            KingCrown(film);
            KingColumn(film);
            KingWindow(film);
            KingArmor(film);
            KingRises(film);
            return film;
        }

        // ------------------------------------------------------------------
        // 1. A coroa
        // ------------------------------------------------------------------

        private static PixelCanvas HallBokeh(Rgba top, Rgba bottom, Rgba lights, int seed)
        {
            var c = new PixelCanvas(320, 180);
            VGradient(c, 0, 0, 320, 180, top, Rgba.Lerp(top, bottom, 0.5f), bottom);

            // Colunas fora de foco.
            for (int i = 0; i < 5; i++)
            {
                int x0 = 10 + i * 72 + ArtKit.Hash(i, 0, seed) % 14;
                for (int y = 0; y < 180; y++)
                    for (int x = x0; x < x0 + 26; x++)
                    {
                        float u = (x - x0) / 26f;
                        AddLight(c, x, y, lights, (0.05f + (u < 0.3f ? 0.04f : 0f)) * (y / 180f));
                    }
            }

            Bokeh(c, 26, 0, 20, 320, 140, lights, 3f, 9f, seed, 0.45f);
            Bokeh(c, 12, 0, 40, 320, 120, Rgba.Lerp(lights, C(255, 255, 255), 0.4f), 1.5f, 3f, seed + 1, 0.6f);
            return c;
        }

        private static void KingCrown(FilmDraft film)
        {
            ShotDraft shot = film.Shot("king_crown", 4.4f, Night).Fades(0.5f, 0.15f).Vignette(0.55f);
            shot.Layer("bg", HallBokeh(C(18, 16, 30), C(34, 22, 30), Candle, 9101), 160f, 90f, 0.4f);

            PixelCanvas[] crown = Frames(10, (k, i) => Crown(1f, k, k * k * 0.55f, 0f, true));
            shot.Anim("crown", crown, 7f, 1.0f, false, 160f, 94f, 1f, 0.5f, 0.45f).Keys(
                new FilmKey(0f, 160f, 100f, 0.8f),
                new FilmKey(4.4f, 160f, 94f, 0.84f, 1f, 0));

            // O verde vazando da rachadura.
            var leak = new PixelCanvas(320, 260);
            Halo(leak, 160f, 120f, 120f, Oxide, 0.55f, 7f);
            shot.Layer("leak", leak, 160f, 94f, 1f, 0.5f, 0.45f, true).Keys(
                new FilmKey(0f, 160f, 94f, 0.84f, 0f),
                new FilmKey(1.0f, 160f, 94f, 0.84f, 0f),
                new FilmKey(1.3f, 160f, 94f, 0.84f, 0.8f, 2),
                new FilmKey(2.4f, 160f, 94f, 0.84f, 0.45f),
                new FilmKey(4.4f, 160f, 94f, 0.84f, 0.7f));

            shot.Cam(new FilmCam(0f, 160f, 96f, 1.0f), new FilmCam(1.0f, 160f, 92f, 1.04f), new FilmCam(4.4f, 160f, 74f, 1.16f, 0));
            shot.Shake(1.0f, 0.7f, 3f).Shake(1.9f, 0.4f, 1.5f);
            shot.Flash(1.0f, 0.35f, Oxide, 0.45f);
            shot.Cue(1.0f, "KingCrownCrack").Cue(1.9f, "KingCrownCrack", 0.8f, 0.6f);

            // Lascas de quartzo caindo e fagulhas verdes subindo.
            shot.Spray(0, 120f, 20f, 80f, 60f, 1.0f, 2.8f, 26f, C(230, 240, 255), C(140, 160, 200, 0), 1.1f, 9102)
                .Move(0f, -30f, 50f, 30f, 260f).Look(1);
            shot.Spray(1, 110f, 30f, 100f, 70f, 1.1f, 4.4f, 18f, OxideHot, Oxide.WithAlpha(0), 1.6f, 9103)
                .Move(0f, -24f, 10f, 8f).Look(2, 0f, true);
        }

        // ------------------------------------------------------------------
        // 2. A Coluna
        // ------------------------------------------------------------------

        private static void KingColumn(FilmDraft film)
        {
            ShotDraft shot = film.Shot("king_column", 4.8f, Ink).Fades(0.15f, 0.15f).Vignette(0.4f);
            shot.Layer("section", Section(0.55f, 0f), 0f, 0f, 1f, 0f, 0f);

            PixelCanvas[] energy = Frames(16, (k, i) => ColumnEnergy(k, true, Oxide, OxideHot));
            shot.Anim("energy", energy, 16f / 3.3f, 0.5f, false, ColumnX, SectionH * 0.5f, 1f, 0.5f, 0.5f, true).Keys(
                new FilmKey(0f, ColumnX, SectionH * 0.5f, 1f, 0f),
                new FilmKey(0.45f, ColumnX, SectionH * 0.5f, 1f, 1f));

            // O Coracao pulsa antes de soltar o Oxide.
            var pulse = new PixelCanvas(200, 200);
            Halo(pulse, 100f, 100f, 96f, Oxide, 0.8f, 7f);
            shot.Layer("pulse", pulse, ColumnX, HeartY, 1f, 0.5f, 0.5f, true).Keys(
                new FilmKey(0f, ColumnX, HeartY, 0.6f, 0f),
                new FilmKey(0.35f, ColumnX, HeartY, 1.1f, 1f, 2),
                new FilmKey(1.2f, ColumnX, HeartY, 0.9f, 0.4f),
                new FilmKey(4.8f, ColumnX, HeartY, 1f, 0.5f));

            shot.Cam(
                new FilmCam(0f, 160f, 676f, 1.12f),
                new FilmCam(0.6f, 160f, 668f, 1.05f),
                new FilmCam(3.9f, 160f, 96f, 1f, 1),
                new FilmCam(4.8f, 160f, 90f, 1f));

            shot.Shake(0.35f, 0.5f, 2.5f).Shake(3.8f, 0.8f, 2f);
            shot.Cue(0.3f, "HeartGive", 0.5f).Cue(0.6f, "KingSkyCharge", 0.55f).Cue(3.7f, "KingShatter", 0.7f, 0.6f);

            // O Oxide subindo em fiapos ao redor da Coluna.
            shot.Spray(1, ColumnX - 22f, 150f, 44f, 560f, 0.6f, 4.8f, 70f, OxideHot, Oxide.WithAlpha(0), 1.2f, 9201)
                .Move(0f, -90f, 10f, 30f).Look(2, 0f, true);
        }

        // ------------------------------------------------------------------
        // 3. O janelao
        // ------------------------------------------------------------------

        private static PixelCanvas WindowWall(bool green)
        {
            var c = new PixelCanvas(360, 200);
            VGradient(c, 0, 0, 360, 200, C(14, 12, 24), C(26, 24, 40), C(20, 18, 30));

            float wx = 196f, top = 18f, bottom = 176f, half = 70f;
            Rgba[] sky = green
                ? new[] { C(10, 40, 30), C(30, 110, 70), C(90, 220, 150), C(200, 255, 220) }
                : new[] { C(14, 16, 36), C(30, 34, 70), C(60, 70, 120), C(110, 130, 190) };

            for (int y = (int)top; y < (int)bottom; y++)
                for (int x = (int)(wx - half - 10); x <= (int)(wx + half + 10); x++)
                {
                    float u = (x - wx) / half;
                    float arch = top + 50f - (float)Math.Sqrt(Math.Max(0f, 1f - u * u)) * 50f;
                    if (Math.Abs(u) > 1.14f || y < arch - 8f) continue;
                    bool frame = Math.Abs(u) > 1f || y < arch;
                    if (frame)
                    {
                        c.Set(x, y, Ramp(Marble, 0.55f - u * 0.2f, x, y));
                        continue;
                    }

                    // O vazio la fora, com a Coluna de luz no meio.
                    float beam = Math.Abs(x - wx) / 18f;
                    float t = 0.15f + (1f - Math.Min(1f, beam)) * 0.75f + (y - top) / (bottom - top) * 0.1f;
                    c.Set(x, y, Ramp(sky, t, x, y));
                }

            // Os montantes e as travessas do vitral.
            for (int k = -2; k <= 2; k++)
            {
                int mx = (int)(wx + k * half * 0.5f);
                for (int y = (int)top + 20; y < (int)bottom; y++) { c.Set(mx, y, Ink); c.Set(mx + 1, y, Marble[1]); }
            }

            foreach (float ty in new[] { 70f, 118f, 150f })
                for (int x = (int)(wx - half); x <= (int)(wx + half); x++) { c.Set(x, (int)ty, Ink); c.Set(x, (int)ty + 1, Marble[1]); }

            // O piso de marmore, refletindo a janela.
            for (int y = 176; y < 200; y++)
                for (int x = 0; x < 360; x++)
                {
                    float refl = Math.Max(0f, 1f - Math.Abs(x - wx) / (half * 1.2f)) * (1f - (y - 176) / 30f);
                    c.Set(x, y, Ramp(Marble, 0.18f + refl * 0.4f, x, y));
                    if (green && refl > 0.2f) AddLight(c, x, y, Oxide, refl * 0.25f);
                }

            if (green) Light(c, wx, 100f, 170f, Oxide, 0.35f, false, 7f);
            else Light(c, wx, 100f, 140f, Sky, 0.12f, false, 5f);
            return c;
        }

        /// <summary>O Rei ajoelhado, visto de costas: o manto aberto no chao, as ombreiras, a cabeca e a coroa.</summary>
        private static PixelCanvas KingFromBehind()
        {
            var c = new PixelCanvas(220, 200);
            float cx = 110f;

            // O manto, aberto no chao.
            Poly(c, (x, y) =>
            {
                float fold = (float)Math.Sin((x - cx) * 0.22f) * 0.5f + 0.5f;
                return Ramp(Velvet, 0.25f + fold * 0.35f - (y - 60) / 400f, x, y);
            }, (cx - 52f, 62f), (cx + 52f, 62f), (cx + 98f, 196f), (cx - 104f, 196f));

            // A barra de ouro rasgada do manto.
            for (int x = (int)cx - 104; x < cx + 98; x++)
            {
                int y = 192 + (int)(Math.Sin(x * 0.4f) * 2f);
                for (int k = 0; k < 4; k++) c.Set(x, y + k - 2, Ramp(Gold, 0.6f - k * 0.12f, x, y));
            }

            // A gola de arminho.
            Oval(c, cx, 66f, 56f, 14f, (nx, ny, px, py) =>
                (ArtKit.Hash(px / 3, py / 3, 9301) % 11 == 0) ? C(26, 24, 30) : Ramp(new[] { C(150, 146, 140), C(214, 210, 200), C(246, 244, 236) }, 0.7f - ny * 0.4f, px, py));

            // As ombreiras de prata, em tres laminas.
            for (int side = -1; side <= 1; side += 2)
            {
                for (int k = 0; k < 3; k++)
                {
                    float ox = cx + side * (44f - k * 4f);
                    float oy = 58f + k * 9f;
                    Oval(c, ox, oy, 24f - k * 2f, 12f, (nx, ny, px, py) => Ramp(Silver, 0.55f - ny * 0.4f + nx * side * 0.1f, px, py));
                    for (int x = (int)(ox - 22f); x <= ox + 22f; x++) c.Set(x, (int)(oy + 10f - Math.Abs(x - ox) * 0.2f), Gold[2]);
                }
            }

            // A cabeca, o cabelo e a coroa.
            Oval(c, cx, 40f, 20f, 22f, (nx, ny, px, py) => Ramp(new[] { C(40, 44, 60), C(80, 86, 110), C(130, 136, 160), C(180, 186, 206) },
                0.45f - ny * 0.2f + (float)Math.Sin(px * 0.8f) * 0.1f, px, py));
            for (int x = (int)cx - 21; x <= cx + 21; x++) for (int k = 0; k < 6; k++) c.Set(x, 22 + k, Ramp(Gold, 0.7f - k * 0.1f, x, 22 + k));
            for (int k = -4; k <= 4; k++)
            {
                float x = cx + k * 4.6f;
                float hgt = 26f - Math.Abs(k) * 3.5f;
                Poly(c, (px, py) => Ramp(Quartz, px < x ? 0.85f : 0.45f, px, py), (x - 2.4f, 23f), (x + 2.4f, 23f), (x + k * 0.6f, 23f - hgt));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static void KingWindow(FilmDraft film)
        {
            ShotDraft shot = film.Shot("king_window", 4.6f, Ink).Fades(0.15f, 0.2f).Vignette(0.5f);
            shot.Layer("wall", WindowWall(false), 160f, 90f, 0.6f);
            shot.Layer("wall_green", WindowWall(true), 160f, 90f, 0.6f).Keys(
                new FilmKey(0f, 160f, 90f, 1f, 0f),
                new FilmKey(0.8f, 160f, 90f, 1f, 0f),
                new FilmKey(1.6f, 160f, 90f, 1f, 1f, 2));

            // A luz doente vindo buscar o Rei: fitas verdes que crescem da janela.
            PixelCanvas[] tendrils = Frames(9, (k, i) => Tendrils(k));
            shot.Anim("tendrils", tendrils, 6f, 1.3f, false, 160f, 90f, 0.8f, 0.5f, 0.5f, true).Keys(
                new FilmKey(0f, 160f, 90f, 1f, 0f), new FilmKey(1.3f, 160f, 90f, 1f, 0f), new FilmKey(1.5f, 160f, 90f, 1f, 1f));

            PixelCanvas king = KingFromBehind();
            shot.Layer("king_cold", Relight(king, C(70, 66, 96), C(170, 190, 240), 0.55f, -0.8f, 3, 0.9f), 96f, 190f, 1.1f, 0.5f, 1f).Keys(
                new FilmKey(0f, 96f, 190f, 0.66f));
            shot.Layer("king_green", Relight(king, C(50, 74, 64), OxideHot, 0.55f, -0.8f, 4, 1f), 96f, 190f, 1.1f, 0.5f, 1f).Keys(
                new FilmKey(0f, 96f, 190f, 0.66f, 0f),
                new FilmKey(1.2f, 96f, 190f, 0.66f, 0f),
                new FilmKey(2.2f, 96f, 190f, 0.66f, 1f));

            shot.Cam(new FilmCam(0f, 176f, 92f, 1.0f), new FilmCam(4.6f, 150f, 100f, 1.12f, 0));
            shot.Shake(1.5f, 0.5f, 2f).Shake(2.3f, 1.2f, 3.5f);
            shot.Flash(2.3f, 0.45f, Oxide, 0.55f);
            shot.Cue(1.4f, "KingBeamFire", 0.6f).Cue(2.3f, "KingShatter");

            // Motas verdes correndo da janela pro Rei.
            shot.Spray(1, 150f, 30f, 90f, 120f, 1.4f, 4.6f, 60f, OxideHot, Oxide.WithAlpha(0), 1.3f, 9301)
                .Move(-70f, 20f, 25f, 25f).Look(2, 0f, true, 0.9f);
        }

        private static PixelCanvas Tendrils(float k)
        {
            // Fitas de luz doente: saem do janelao e vem, ondulando, ate o Rei.
            var c = new PixelCanvas(320, 180);
            (float sx, float sy, float ex, float ey, float bend)[] ribbons =
            {
                (190f, 60f, 104f, 92f, -26f), (200f, 100f, 110f, 112f, 18f), (186f, 140f, 96f, 128f, 22f), (214f, 40f, 120f, 100f, -40f),
            };

            for (int r = 0; r < ribbons.Length; r++)
            {
                var b = ribbons[r];
                float reach = Math.Max(0f, Math.Min(1f, k * 1.3f - r * 0.1f));
                int steps = 160;
                for (int s = 0; s < steps * reach; s++)
                {
                    float t = s / (float)steps;
                    float x = b.sx + (b.ex - b.sx) * t;
                    float y = b.sy + (b.ey - b.sy) * t + (float)Math.Sin(t * Math.PI) * b.bend + (float)Math.Sin(t * 9f + r * 2f) * 4f * (1f - t);
                    float width = 3.2f * (1f - t * 0.55f);
                    Halo(c, x, y, width * 2.2f, Oxide, 0.22f, 3f);
                    Disc(c, x, y, Math.Max(0.6f, width * 0.35f), OxideHot);
                }
            }

            return c;
        }

        // ------------------------------------------------------------------
        // 4. A armadura
        // ------------------------------------------------------------------

        private static PixelCanvas ChestPlate(float rot)
        {
            var c = new PixelCanvas(340, 200);

            // O manto vermelho atras, em dobras.
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 340; x++)
                {
                    float fold = (float)Math.Sin(x * 0.09f + y * 0.01f) * 0.5f + 0.5f;
                    c.Set(x, y, Ramp(Velvet, 0.12f + fold * 0.3f, x, y));
                }

            // O peitoral: largo nos ombros, com o recorte do pescoco, afinando
            // pra cintura; a quilha no meio pega a luz.
            float cx = 150f, cy = 150f;
            var plate = new[] { (34f, 40f), (112f, 30f), (130f, 52f), (150f, 58f), (170f, 52f), (188f, 30f), (262f, 40f), (250f, 140f), (222f, 210f), (78f, 210f), (50f, 140f) };
            ArtKit.Polygon(c, plate, (px, py) =>
            {
                float nx = (px - cx) / 110f;
                float ridge = Math.Abs(px - cx) < 3f ? 0.3f : Math.Abs(px - cx) < 5f ? -0.12f : 0f;
                float light = 0.72f - nx * 0.5f - nx * nx * 0.35f + ridge - (py - 40f) / 500f;
                return Ramp(Silver, light, (int)px, (int)py);
            });

            // O cinturao de ouro embaixo e a barra na borda do peitoral.
            for (int x = 70; x < 232; x++) for (int k = 0; k < 9; k++) c.Set(x, 186 + k, Ramp(Gold, 0.8f - k * 0.09f - Math.Abs(x - cx) / 300f, x, 186 + k));
            for (int k = 0; k < plate.Length; k++)
            {
                var a = plate[k];
                var b = plate[(k + 1) % plate.Length];
                if (a.Item2 > 180f && b.Item2 > 180f) continue;
                Thick(c, a.Item1, a.Item2, b.Item1, b.Item2, 1.6f, Gold[3]);
                ArtKit.Line(c, (int)a.Item1, (int)a.Item2 + 2, (int)b.Item1, (int)b.Item2 + 2, Gold[1]);
                ArtKit.Rivet(c, (int)a.Item1, (int)a.Item2 + 4, Gold);
            }

            // A marca 925 gravada.
            PixelFont.DrawCentered(c, "925", (int)cx - 30, 100, Silver[1], Silver[4], PixelFont.Style.Engraved, 4, 9401);

            // A ombreira, em tres laminas, por cima a direita.
            for (int k = 0; k < 3; k++)
            {
                float ox = 280f - k * 10f, oy = 40f + k * 26f;
                Oval(c, ox, oy, 70f - k * 6f, 30f, (nx, ny, px, py) => Ramp(Silver, 0.62f - ny * 0.45f - nx * 0.15f, px, py));
                for (int x = (int)(ox - 66f + k * 6); x <= ox + 66f - k * 6; x++)
                {
                    float u = (x - ox) / (70f - k * 6f);
                    int y = (int)(oy + 28f * (float)Math.Sqrt(Math.Max(0f, 1f - u * u)));
                    for (int d = 0; d < 4; d++) c.Set(x, y - d, Ramp(Gold, 0.75f - d * 0.15f, x, y));
                }
            }

            // A gola de arminho, em cima a esquerda.
            Poly(c, (x, y) =>
            {
                bool tail = (x / 14 + y / 12) % 2 == 0 && x % 14 > 5 && x % 14 < 8 && y % 12 > 3 && y % 12 < 9;
                return tail ? C(26, 24, 30) : Ramp(new[] { C(160, 156, 150), C(214, 210, 200), C(246, 244, 236) }, 0.85f - y / 90f + (float)Math.Sin(x * 0.5f) * 0.08f, x, y);
            }, (0f, 0f), (128f, 0f), (104f, 26f), (0f, 44f));

            // A rachadura verde que ele ja tinha no peito.
            var crackPath = CrackPath(cx - 8f, 120f, 70f, 70, 9403, 0.5f);
            DrawCrack(c, crackPath, 1f, Ink, OxideHot, Oxide);
            Light(c, cx - 4f, 150f, 40f, Oxide, 0.3f, true, 4f);

            // O azinhavre comendo a prata, a partir da rachadura.
            if (rot > 0f)
            {
                for (int y = 0; y < c.Height; y++)
                    for (int x = 0; x < c.Width; x++)
                    {
                        Rgba p = c.Get(x, y);
                        float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - 150f) * (y - 150f)) / 300f;
                        float n = ArtKit.Noise(x * 0.06f, y * 0.06f, 9404) * 0.6f + ArtKit.Noise(x * 0.2f, y * 0.2f, 9405) * 0.4f;
                        float front = rot * 1.3f - d - n * 0.35f;
                        if (front < 0f) continue;
                        if (front < 0.04f)
                        {
                            c.Set(x, y, Bayer(x, y) < 0.5f ? Oxide : OxideDeep);
                            continue;
                        }

                        bool silver = p.B > p.R + 10 && p.G > 60;
                        if (silver) c.Set(x, y, Remap(p, Patina, x, y));
                        else if (p.R > p.B + 40) c.Set(x, y, Rgba.Lerp(p, C(60, 40, 30), 0.5f));
                    }
            }

            return c;
        }

        private static void KingArmor(FilmDraft film)
        {
            ShotDraft shot = film.Shot("king_armor", 4.2f, Ink).Fades(0.15f, 0.2f).Vignette(0.45f);
            PixelCanvas[] plate = Frames(12, (k, i) => ChestPlate(k));
            shot.Anim("plate", plate, 12f / 2.8f, 0.4f, false, 170f, 100f).Keys(
                new FilmKey(0f, 176f, 102f, 1f),
                new FilmKey(4.2f, 164f, 96f, 1f, 1f, 0));

            var glow = new PixelCanvas(340, 200);
            Halo(glow, 150f, 150f, 150f, Oxide, 0.45f, 6f);
            shot.Layer("glow", glow, 170f, 100f, 1f, 0.5f, 0.5f, true).Keys(
                new FilmKey(0f, 176f, 102f, 1f, 0.2f), new FilmKey(3.2f, 164f, 96f, 1f, 0.9f));

            shot.Cam(new FilmCam(0f, 160f, 90f, 1.04f), new FilmCam(4.2f, 160f, 96f, 1.16f, 0));
            shot.Shake(0.5f, 0.4f, 1.5f).Shake(1.6f, 0.4f, 1.5f).Shake(2.7f, 0.6f, 2.5f);
            shot.Cue(0.5f, "KingArmor", 0.8f).Cue(1.6f, "KingArmor", 0.7f).Cue(2.7f, "KingArmor", 0.6f).Cue(2.9f, "KingCrownCrack", 0.6f, 0.5f);

            // Lascas de prata descascando e caindo.
            shot.Spray(0, 60f, 40f, 220f, 140f, 0.6f, 4.0f, 36f, Silver[4], Silver[2].WithAlpha(0), 1.1f, 9410)
                .Move(-10f, -40f, 40f, 30f, 240f).Look(2);
            shot.Spray(1, 80f, 60f, 180f, 120f, 0.8f, 4.2f, 22f, OxideHot, Oxide.WithAlpha(0), 1.4f, 9411)
                .Move(0f, -30f, 12f, 10f).Look(2, 0f, true);
        }

        // ------------------------------------------------------------------
        // 5. Ele se ergue
        // ------------------------------------------------------------------

        private static PixelCanvas Vault()
        {
            var c = new PixelCanvas(380, 220);
            float vx = 190f, vy = -30f;

            // A abobada vista de baixo: nervuras que convergem pro alto.
            for (int y = 0; y < 220; y++)
                for (int x = 0; x < 380; x++)
                {
                    float dx = x - vx, dy = y - vy;
                    float ang = (float)Math.Atan2(dy, dx);
                    float rib = (float)Math.Abs(Math.Sin(ang * 9f));
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float light = 0.12f + (y / 220f) * 0.35f + (rib < 0.1f ? 0.25f : 0f) - (d < 60f ? 0.1f : 0f);
                    c.Set(x, y, Ramp(Mix(Marble, Patina, 0.6f), light, x, y));
                }

            // O lustre, pendurado, em silhueta.
            ArtKit.Line(c, (int)vx, 0, (int)vx, 46, Ink);
            Oval(c, vx, 54f, 40f, 9f, (nx, ny, px, py) => ny < 0.2f ? Ink : C(30, 40, 36));
            for (int k = -4; k <= 4; k++)
            {
                c.Fill((int)(vx + k * 9f), 42, 2, 8, Ink);
                Light(c, vx + k * 9f, 40f, 7f, Oxide, 0.5f, false, 3f);
            }

            // O verde vindo de baixo.
            for (int y = 120; y < 220; y++)
                for (int x = 0; x < 380; x++)
                    AddLight(c, x, y, Oxide, (y - 120) / 100f * 0.35f * (Bayer(x, y) < 0.7f ? 1f : 0.6f));

            return c;
        }

        private static void KingRises(FilmDraft film)
        {
            ShotDraft shot = film.Shot("king_rises", 5.2f, Ink).Fades(0.15f, 0f).Vignette(0.5f);
            shot.Layer("vault", Vault(), 160f, 90f, 0.5f);

            // Ele, de joelhos, levanta e abre os bracos (os quadros do proprio Rei, em contraluz verde).
            PixelCanvas[] all = KingArt.Frames(true);
            int[] order = { KingArt.Fall + 3, KingArt.Fall + 3, KingArt.Fall + 2, KingArt.Fall + 1, KingArt.Fall, KingArt.Idle, KingArt.Cast, KingArt.Cast + 1, KingArt.Cast + 2 };
            var frames = new PixelCanvas[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                PixelCanvas lit = Relight(all[order[i]], C(120, 150, 136), OxideHot, -0.2f, 1f, 3, 1f);
                frames[i] = Scaled(Relight(lit, C(255, 255, 255), C(200, 255, 220), 0.9f, -0.3f, 1, 0.6f), 2);
            }

            shot.Anim("king", frames, 3.2f, 0.4f, false, 176f, 262f, 1f, KingArt.PivotX, 1f - KingArt.PivotY).Keys(
                new FilmKey(0f, 150f, 420f, 0.88f),
                new FilmKey(0.4f, 150f, 420f, 0.88f),
                new FilmKey(2.6f, 150f, 356f, 0.88f, 1f, 2),
                new FilmKey(5.2f, 150f, 350f, 0.9f, 1f, 0));

            // O Scrap, pequeno, na frente, olhando pra cima.
            PixelCanvas scrap = Scaled(HeroArt.Frames(HeroArt.Polished())[HeroArt.Idle], 2);
            shot.Layer("scrap", Relight(Flipped(scrap), C(70, 96, 90), Oxide, -0.7f, -0.6f, 2, 1f), 284f, 186f, 1.25f, 0.5f, 1f);

            shot.Cam(new FilmCam(0f, 160f, 96f, 1.0f), new FilmCam(5.2f, 164f, 86f, 1.08f, 0));
            shot.Shake(0.4f, 0.6f, 1.5f).Shake(2.5f, 1.6f, 4f);
            shot.Flash(2.5f, 0.5f, Oxide, 0.5f);
            shot.Cue(0.4f, "KingRise").Cue(2.5f, "KingCast", 0.8f).Cue(2.6f, "KingSkyLance", 0.7f, 0.7f);

            // Brasas verdes subindo e poeira caindo da abobada.
            shot.Spray(1, 0f, 120f, 320f, 70f, 0.4f, 5.2f, 40f, OxideHot, Oxide.WithAlpha(0), 1.8f, 9501)
                .Move(0f, -50f, 15f, 20f).Look(2, 0f, true);
            shot.Spray(0, 0f, -10f, 320f, 20f, 2.5f, 5.2f, 30f, C(170, 180, 170), C(90, 100, 96, 0), 1.6f, 9502)
                .Move(0f, 30f, 10f, 10f, 80f).Look(1, 0f, false, 0.7f);
        }

        private static PixelCanvas FlipIfNeeded(PixelCanvas c, bool flip) => flip ? Flipped(c) : c;
    }
}
