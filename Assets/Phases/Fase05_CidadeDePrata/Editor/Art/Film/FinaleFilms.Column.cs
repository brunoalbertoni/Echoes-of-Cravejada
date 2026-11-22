using System;
using static EchoesOfCravejada.EditorTools.FilmPaint;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O fim do trono (o Nucleo), em seis planos:
    ///
    ///   1 O golpe      o Scrap quebra o trono; o trono racha e puxa o nucleo
    ///                  dele, uma bola de luz que sai do peito e entra na fenda.
    ///   2 A descida    a luz branca desce pela Coluna, andar por andar, do
    ///                  palacio ate o Coracao (o caminho do Oxide, ao contrario).
    ///   3 O Coracao    de perto: a crosta verde estoura e o quartzo fica limpo.
    ///   4 A cidade     de ponta a ponta: a fumaca verde vai embora e as janelas
    ///                  acendem numa onda.
    ///   5 O Scrap      ajoelhado diante do trono aberto, contra a luz: os olhos
    ///                  dele se apagam.
    ///   6 O muro       no Poco, a Pequena risca SCRAP embaixo dos outros nomes.
    /// </summary>
    public static partial class FinaleFilms
    {
        public static FilmDraft Column()
        {
            var film = new FilmDraft();
            ColumnStrike(film);
            ColumnDescends(film);
            HeartCleansed(film);
            CityWakes(film);
            ScrapDims(film);
            NameOnWall(film);
            return film;
        }

        // ------------------------------------------------------------------
        // O Scrap, nos quadros dele
        // ------------------------------------------------------------------

        /// <summary>O visual polido com os olhos de 1 (acesos) a 0 (apagados).</summary>
        private static HeroArt.Look ScrapLook(float eyes)
        {
            HeroArt.Look look = HeroArt.Polished();
            var off = C(34, 40, 54);
            look.EyeCore = Rgba.Lerp(off, look.EyeCore, eyes);
            look.EyeGlow = Rgba.Lerp(off, look.EyeGlow, eyes);
            look.EyeHalo = Rgba.Lerp(off, look.EyeHalo, eyes);
            return look;
        }

        private static PixelCanvas ScrapFrame(int index, float eyes = 1f) => HeroArt.Frames(ScrapLook(eyes))[index];

        // ------------------------------------------------------------------
        // A sala do trono, de lado
        // ------------------------------------------------------------------

        /// <summary>A sala do trono em plano medio: o piso, as colunas, o janelao verde ao fundo.</summary>
        private static PixelCanvas ThroneHall(float green, float white)
        {
            var c = new PixelCanvas(360, 200);
            VGradient(c, 0, 0, 360, 200, C(18, 18, 30), C(34, 34, 52), C(24, 22, 36));

            // O janelao atras do trono.
            float wx = 214f;
            for (int y = 20; y < 150; y++)
                for (int x = (int)wx - 46; x <= wx + 46; x++)
                {
                    float u = (x - wx) / 46f;
                    float arch = 20f + 34f - (float)Math.Sqrt(Math.Max(0f, 1f - u * u)) * 34f;
                    if (y < arch) continue;
                    bool frame = Math.Abs(u) > 0.9f || (x - (int)wx) % 23 == 0 || y % 34 == 0;
                    Rgba sky = Rgba.Lerp(Rgba.Lerp(C(30, 34, 70), C(40, 120, 80), green), C(220, 236, 255), white);
                    c.Set(x, y, frame ? Ramp(Marble, 0.45f, x, y) : Rgba.Lerp(sky, sky.Scaled(1.3f).WithAlpha(255), Bayer(x, y) * 0.3f));
                }

            if (green > 0f) Light(c, wx, 90f, 120f, Oxide, 0.25f * green, false, 6f);
            if (white > 0f) Light(c, wx, 90f, 150f, Pure, 0.4f * white, false, 6f);

            // Colunas de marmore a esquerda e a direita.
            foreach (int px in new[] { 20, 96, 320 })
            {
                for (int y = 0; y < 170; y++)
                    for (int x = px; x < px + 22; x++)
                    {
                        float u = (x - px) / 22f;
                        c.Set(x, y, Ramp(Marble, 0.25f + (u < 0.35f ? 0.3f : u > 0.8f ? -0.1f : 0.12f), x, y));
                    }

                for (int x = px - 3; x < px + 25; x++) for (int k = 0; k < 5; k++) { c.Set(x, k + 12, Ramp(Gold, 0.6f - k * 0.1f, x, k)); c.Set(x, 165 + k, Ramp(Marble, 0.5f - k * 0.08f, x, k)); }
            }

            // O piso de marmore em xadrez, com reflexo.
            for (int y = 170; y < 200; y++)
                for (int x = 0; x < 360; x++)
                {
                    int tile = ((x + (y - 170) * 2) / 26 + (y - 170) / 8) % 2;
                    float refl = Math.Max(0f, 1f - Math.Abs(x - wx) / 80f) * 0.25f;
                    c.Set(x, y, Ramp(Marble, (tile == 0 ? 0.42f : 0.3f) - (y - 170) * 0.006f + refl, x, y));
                }

            ArtKit.Line(c, 0, 170, 360, 170, Marble[4]);

            // O tapete vermelho ate o trono.
            for (int y = 172; y < 200; y++)
                for (int x = 150 + (200 - y) / 3; x < 278 - (200 - y) / 3; x++)
                    c.Set(x, y, Ramp(Velvet, 0.45f + (x % 30 < 2 ? 0.3f : 0f), x, y));

            return c;
        }

        // ------------------------------------------------------------------
        // 1. O golpe
        // ------------------------------------------------------------------

        private static void ColumnStrike(FilmDraft film)
        {
            ShotDraft shot = film.Shot("column_strike", 5.4f, Ink).Fades(0.5f, 0.15f).Vignette(0.45f);
            shot.Layer("hall", ThroneHall(0.8f, 0f), 160f, 90f, 0.8f);
            shot.Layer("hall_white", ThroneHall(0f, 1f), 160f, 90f, 0.8f).Keys(
                new FilmKey(0f, 160f, 90f, 1f, 0f), new FilmKey(3.4f, 160f, 90f, 1f, 0f), new FilmKey(4.6f, 160f, 90f, 1f, 1f));

            // O trono: inteiro ate o golpe, rachado depois, com a luz por dentro.
            PixelCanvas throne = PalaceArt.Throne(false);
            PixelCanvas broken = PalaceArt.Throne(true);
            shot.Anim("throne", new[] { throne, broken }, 1f, 0.6f, false, 214f, 172f, 1f, 0.5f, 1f).Keys(new FilmKey(0f, 214f, 172f, 0.82f));

            var crackLight = new PixelCanvas(168, 184);
            foreach (var path in new[] { CrackPath(84f, 180f, -90f, 120, 9601, 0.4f), CrackPath(84f, 130f, -60f, 60, 9602, 0.5f), CrackPath(84f, 110f, -120f, 60, 9603, 0.5f) })
            {
                foreach ((float x, float y) in path)
                {
                    Halo(crackLight, x, y, 4f, Pure, 0.5f, 3f);
                    crackLight.Set((int)x, (int)y, C(255, 255, 255));
                }
            }

            Halo(crackLight, 84f, 170f, 40f, Pure, 0.7f, 5f);
            shot.Layer("crack", crackLight, 214f, 172f, 1f, 0.5f, 1f, true).Keys(
                new FilmKey(0f, 214f, 172f, 0.82f, 0f), new FilmKey(1.6f, 214f, 172f, 0.82f, 0f),
                new FilmKey(1.7f, 214f, 172f, 0.82f, 1f, 2), new FilmKey(2.4f, 214f, 172f, 0.82f, 0.6f), new FilmKey(4.6f, 214f, 172f, 0.82f, 1f));

            // O Scrap: prepara, golpeia, e cambaleia quando o nucleo sai.
            PixelCanvas[] scrap =
            {
                ScrapFrame(HeroArt.Idle), ScrapFrame(HeroArt.Windup), ScrapFrame(HeroArt.Windup + 1),
                ScrapFrame(HeroArt.Strike), ScrapFrame(HeroArt.Strike + 1), ScrapFrame(HeroArt.Strike + 2),
                ScrapFrame(HeroArt.Strike + 2), ScrapFrame(HeroArt.Hurt, 0.6f), ScrapFrame(HeroArt.Hurt, 0.4f), ScrapFrame(HeroArt.Idle, 0.3f),
            };
            for (int i = 0; i < scrap.Length; i++) scrap[i] = Scaled(Relight(scrap[i], C(200, 200, 220), C(210, 255, 220), 1f, -0.4f, 1, 0.8f), 3);
            shot.Anim("scrap", scrap, 6f, 0.5f, false, 96f, 172f, 1f, 0.5f, 1f);

            // O nucleo: sai do peito e voa pra fenda do trono.
            var core = new PixelCanvas(40, 40);
            Halo(core, 20f, 20f, 19f, Pure, 0.9f, 5f);
            Disc(core, 20f, 20f, 4f, C(255, 255, 255));
            shot.Layer("core", core, 100f, 140f, 1f, 0.5f, 0.5f, true).Keys(
                new FilmKey(0f, 100f, 140f, 0.5f, 0f),
                new FilmKey(2.1f, 100f, 140f, 0.5f, 0f),
                new FilmKey(2.4f, 104f, 136f, 1.2f, 1f, 2),
                new FilmKey(3.4f, 214f, 150f, 0.9f, 1f, 3),
                new FilmKey(3.6f, 214f, 152f, 0.3f, 0f));

            shot.Cam(new FilmCam(0f, 172f, 100f, 1.08f), new FilmCam(1.6f, 170f, 98f, 1.1f), new FilmCam(5.4f, 184f, 94f, 1.0f, 1));
            shot.Shake(1.6f, 0.6f, 4f).Shake(3.4f, 0.8f, 2.5f);
            shot.Flash(1.6f, 0.35f, Pure, 0.8f).Flash(3.4f, 0.5f, Pure, 0.5f);
            shot.Cue(0.9f, "Swing").Cue(1.6f, "Hit", 0.7f).Cue(1.65f, "KingShatter", 1.2f, 0.8f).Cue(2.2f, "HeartGive").Cue(3.4f, "FinaleLight");

            // Lascas de marmore e faiscas no golpe; o rastro do nucleo.
            shot.Spray(0, 196f, 120f, 30f, 40f, 1.6f, 1.75f, 220f, Marble[4], Marble[2].WithAlpha(0), 0.9f, 9610)
                .Move(-20f, -60f, 60f, 50f, 260f).Look(2);
            shot.Spray(0, 196f, 120f, 30f, 30f, 1.6f, 1.75f, 160f, C(255, 250, 220), Warm.WithAlpha(0), 0.5f, 9611)
                .Move(0f, -40f, 90f, 60f, 120f).Look(1, 0f, true);
            shot.Spray(1, 100f, 132f, 110f, 16f, 2.3f, 3.5f, 70f, Pure, Sky.WithAlpha(0), 0.7f, 9612)
                .Move(20f, -6f, 10f, 10f).Look(2, 0f, true);
        }

        // ------------------------------------------------------------------
        // 2. A descida
        // ------------------------------------------------------------------

        private static void ColumnDescends(FilmDraft film)
        {
            ShotDraft shot = film.Shot("column_descends", 4.8f, Ink).Fades(0.15f, 0.15f).Vignette(0.4f);
            shot.Layer("section", Section(0.25f, 0f), 0f, 0f, 1f, 0f, 0f);

            PixelCanvas[] energy = Frames(16, (k, i) => ColumnEnergy(k, false, C(170, 210, 255), C(255, 255, 255)));
            shot.Anim("energy", energy, 16f / 3.4f, 0.3f, false, ColumnX, SectionH * 0.5f, 1f, 0.5f, 0.5f, true);

            shot.Cam(
                new FilmCam(0f, 160f, 96f, 1f),
                new FilmCam(0.4f, 160f, 100f, 1f),
                new FilmCam(3.8f, 160f, 668f, 1.04f, 1),
                new FilmCam(4.8f, 160f, 676f, 1.12f));

            shot.Shake(4.0f, 0.6f, 2f);
            shot.Cue(0.3f, "FinaleLight", 1.2f, 0.6f).Cue(3.9f, "HeartGive", 1.3f);

            shot.Spray(1, ColumnX - 24f, 150f, 48f, 540f, 0.4f, 4.8f, 80f, C(255, 255, 255), Sky.WithAlpha(0), 0.9f, 9701)
                .Move(0f, 120f, 20f, 30f).Look(2, 0f, true);
        }

        // ------------------------------------------------------------------
        // 3. O Coracao
        // ------------------------------------------------------------------

        private static PixelCanvas HeartClose(float clean)
        {
            var c = new PixelCanvas(320, 180);
            Rgba[] wall = Mix(new[] { C(4, 12, 10), C(10, 30, 22), C(20, 52, 38), C(36, 84, 60) },
                new[] { C(8, 10, 24), C(20, 26, 52), C(40, 50, 92), C(80, 96, 150) }, clean);
            for (int y = 0; y < 180; y++)
                for (int x = 0; x < 320; x++)
                {
                    float d = (float)Math.Sqrt((x - 160) * (x - 160) * 0.5f + (y - 92) * (y - 92)) / 150f;
                    float n = ArtKit.Noise(x * 0.05f, y * 0.05f, 9801);
                    c.Set(x, y, Ramp(wall, 0.8f - d * 0.7f + n * 0.25f, x, y));
                }

            // Os cristais da caverna em volta, apontando pro Coracao.
            for (int i = 0; i < 18; i++)
            {
                float a = i * 20f + ArtKit.Hash(i, 0, 9802) % 12;
                (float bx, float by) = ArtKit.Along(160f, 92f, a, 170f);
                (float tx, float ty) = ArtKit.Along(160f, 92f, a, 110f + ArtKit.Hash(i, 1, 9802) % 30);
                (float lx, float ly) = ArtKit.Along(bx, by, a + 90f, 9f);
                (float rx, float ry) = ArtKit.Along(bx, by, a - 90f, 9f);
                Rgba[] ramp = Mix(SickQuartz, Quartz, clean);
                Poly(c, (x, y) => Ramp(ramp, 0.3f + ((x - tx) * (ly - ry) - (y - ty) * (lx - rx) > 0 ? 0.35f : 0f), x, y), (lx, ly), (rx, ry), (tx, ty));
            }

            HeartGem(c, 160f, 92f, 50f, clean, 1.2f, 9803);

            // A crosta estourando: lascas soltas em volta enquanto limpa.
            if (clean > 0.05f && clean < 0.95f)
            {
                for (int i = 0; i < 30; i++)
                {
                    float a = ArtKit.Hash(i, 0, 9804) % 360;
                    (float px, float py) = ArtKit.Along(160f, 92f, a, 30f + clean * 90f + ArtKit.Hash(i, 1, 9804) % 20);
                    Poly(c, (x, y) => OxideDeep, (px, py - 3f), (px + 3f, py + 2f), (px - 3f, py + 2f));
                }
            }

            return c;
        }

        private static void HeartCleansed(FilmDraft film)
        {
            ShotDraft shot = film.Shot("heart_cleansed", 4.6f, Ink).Fades(0.15f, 0.25f).Vignette(0.5f);
            PixelCanvas[] heart = Frames(12, (k, i) => HeartClose(k * k * (3f - 2f * k)));
            shot.Anim("heart", heart, 12f / 2.2f, 0.7f, false, 160f, 90f);

            var rays = new PixelCanvas(320, 180);
            Rays(rays, 160f, 92f, 18, 220f, 9f, Pure, 0.8f, 9805, 0f, 360f, true);
            shot.Layer("rays", rays, 160f, 90f, 1f, 0.5f, 0.5f, true).Keys(
                new FilmKey(0f, 160f, 90f, 0.6f, 0f), new FilmKey(0.8f, 160f, 90f, 0.6f, 0f),
                new FilmKey(2.4f, 160f, 90f, 1.2f, 0.9f, 2), new FilmKey(4.6f, 160f, 90f, 1.4f, 0.6f));

            shot.Cam(new FilmCam(0f, 160f, 82f, 1.2f), new FilmCam(4.6f, 160f, 90f, 1.0f, 2));
            shot.Shake(0.8f, 0.8f, 3f);
            shot.Flash(0.8f, 0.5f, Pure, 0.8f);
            shot.Cue(0.8f, "KingShatter", 1.4f, 0.8f).Cue(1.0f, "PurifyWave");

            // A crosta voando pra fora e o vapor verde indo embora.
            shot.Spray(0, 150f, 82f, 20f, 20f, 0.8f, 1.6f, 120f, C(40, 100, 70), OxideDeep.WithAlpha(0), 1.2f, 9806)
                .Move(0f, 0f, 10f, 10f, 60f).Burst(120f).Look(2);
            shot.Spray(1, 100f, 40f, 120f, 110f, 0.8f, 2.6f, 50f, Oxide.WithAlpha(180), OxideDeep.WithAlpha(0), 1.4f, 9807)
                .Move(0f, -30f, 30f, 20f).Look(4, 3f);
            shot.Spray(1, 120f, 50f, 80f, 80f, 2.0f, 4.6f, 30f, C(255, 255, 255), Sky.WithAlpha(0), 1.5f, 9808)
                .Move(0f, -16f, 16f, 10f).Look(2, 0f, true);
        }

        // ------------------------------------------------------------------
        // 4. A cidade
        // ------------------------------------------------------------------

        public const int CityW = 680;

        /// <summary>O fundo da geode: a parede de ametista do alto da caverna.</summary>
        private static PixelCanvas CityDome(float clean)
        {
            var c = new PixelCanvas(CityW, 200);
            VGradient(c, 0, 0, CityW, 200, Rgba.Lerp(C(10, 26, 20), C(26, 20, 52), clean), Rgba.Lerp(C(20, 50, 36), C(60, 44, 100), clean), Rgba.Lerp(C(14, 30, 24), C(30, 26, 60), clean));

            // Cristais pendurados no teto da geode, nas bordas.
            for (int i = 0; i < 60; i++)
            {
                float x = ArtKit.Hash(i, 0, 9901) % CityW;
                float len = 10 + ArtKit.Hash(i, 1, 9901) % 40;
                float w = 3 + len * 0.15f;
                Poly(c, (px, py) => Ramp(Amethyst, px < x ? 0.55f : 0.3f, px, py), (x - w, -2f), (x + w, -2f), (x + (ArtKit.Hash(i, 2, 9901) % 9 - 4), len));
            }

            Bokeh(c, 40, 0, 20, CityW, 80, C(200, 160, 255), 1f, 2.5f, 9902, 0.5f);
            return c;
        }

        /// <summary>
        /// A cidade de ponta a ponta: a esquerda as Tubulacoes (latao e
        /// chamines), no meio o palacio de prata e a Coluna, a direita as
        /// cupulas de vidro dos Ateliers. lit: ate onde (de 0 a 1, da
        /// esquerda) as janelas ja acenderam.
        /// </summary>
        private static PixelCanvas CityLine(float lit, int seed)
        {
            var c = new PixelCanvas(CityW, 200);
            var rng = new Random(seed);

            void Building(float x, float w, float top, Rgba[] ramp, int roof)
            {
                for (int y = (int)top; y < 200; y++)
                    for (int px = (int)x; px < x + w; px++)
                    {
                        float u = (px - x) / w;
                        c.Set(px, y, Ramp(ramp, 0.3f + (u < 0.2f ? 0.18f : u > 0.85f ? -0.12f : 0f) - (y - top) / 600f, px, y));
                    }

                if (roof == 1) Poly(c, (px, py) => Ramp(ramp, px < x + w / 2 ? 0.55f : 0.25f, px, py), (x - 2f, top), (x + w / 2f, top - w * 0.9f), (x + w + 2f, top));
                if (roof == 2) Oval(c, x + w / 2f, top, w / 2f + 1f, w / 2.4f, (nx, ny, px, py) => ny > 0f ? Rgba.Clear : Ramp(new[] { C(60, 80, 110), C(120, 170, 210), C(200, 236, 255) }, 0.6f - nx * 0.4f - ny * 0.3f, px, py));
                if (roof == 3)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        float cx = x + w * (0.3f + k * 0.4f);
                        for (int y = (int)top - 22; y < top; y++) for (int px = (int)cx - 3; px < cx + 3; px++) c.Set(px, y, Ramp(Brass, 0.35f + (px < cx ? 0.2f : 0f), px, y));
                    }
                }

                // As janelas: acendem da esquerda pra direita.
                for (float wy = top + 6; wy < 196; wy += 9)
                    for (float wx = x + 3; wx < x + w - 4; wx += 6)
                    {
                        if (ArtKit.Hash((int)wx, (int)wy, seed) % 4 == 0) continue;
                        bool on = wx / CityW < lit;
                        Rgba glow = roof == 2 ? C(220, 190, 255) : roof == 3 ? Candle : Warm;
                        c.Fill((int)wx, (int)wy, 2, 3, on ? glow : C(20, 20, 30));
                        if (on) Light(c, wx + 1, wy + 1, 6f, glow, 0.25f, false, 2f);
                    }
            }

            // Tubulacoes a esquerda.
            for (float x = 0; x < 230; x += 22 + (float)rng.NextDouble() * 12)
                Building(x, 18 + (float)rng.NextDouble() * 10, 90 + (float)rng.NextDouble() * 50, Brass, 3);

            // Cupulas a direita.
            for (float x = 450; x < CityW; x += 26 + (float)rng.NextDouble() * 12)
                Building(x, 22 + (float)rng.NextDouble() * 10, 96 + (float)rng.NextDouble() * 40, new[] { C(30, 26, 44), C(60, 50, 84), C(100, 86, 130), C(150, 136, 180) }, 2);

            // Prata no meio, subindo pro palacio.
            for (float x = 230; x < 450; x += 20 + (float)rng.NextDouble() * 10)
            {
                float mid = 1f - Math.Abs(x - 340f) / 120f;
                Building(x, 16 + (float)rng.NextDouble() * 8, 110 - mid * 40 + (float)rng.NextDouble() * 20, Marble, 1);
            }

            // O palacio e a Coluna saindo dele, de alto a baixo.
            Building(312f, 56f, 46f, Marble, 1);
            for (int y = 0; y < 200; y++) for (int x = 337; x < 343; x++) c.Set(x, y, Ramp(Quartz, 0.5f - (x - 337) * 0.08f, x, y));
            return c;
        }

        /// <summary>A fumaca verde por cima da cidade (camada que some).</summary>
        private static PixelCanvas Smog()
        {
            var c = new PixelCanvas(CityW, 200);
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < CityW; x++)
                {
                    float n = ArtKit.Noise(x * 0.02f, y * 0.04f, 9910) * 0.7f + ArtKit.Noise(x * 0.07f, y * 0.1f, 9911) * 0.3f;
                    float k = (n - 0.3f) * 1.4f * Math.Min(1f, y / 80f);
                    if (k < Bayer(x, y) * 0.9f) continue;
                    c.Set(x, y, Rgba.Lerp(C(40, 120, 80), C(110, 220, 150), n).WithAlpha((int)(Math.Min(0.8f, k) * 200f)));
                }

            return c;
        }

        /// <summary>O telhado mais perto, com os descartados olhando a cidade acender.</summary>
        private static PixelCanvas Rooftop(bool lit)
        {
            var c = new PixelCanvas(400, 80);
            for (int y = 30; y < 80; y++)
                for (int x = 0; x < 400; x++)
                {
                    float top = 34f + (float)Math.Sin(x * 0.02f) * 6f;
                    if (y < top) continue;
                    c.Set(x, y, Ramp(Rock, 0.2f + (y - top < 2 ? 0.25f : 0f), x, y));
                }

            for (int v = 0; v < 3; v++)
            {
                PixelCanvas body = DiscardedArt.Frames(v)[0];
                PixelCanvas sil = Relight(body, lit ? C(120, 110, 100) : C(40, 40, 50), lit ? Warm : C(90, 110, 100), 0f, -1f, 2, 0.9f);
                Stamp(c, sil, 60 + v * 110, 30 - 14 + (int)(Math.Sin((60 + v * 110) * 0.02f) * 6f));
            }

            return c;
        }

        private static void CityWakes(FilmDraft film)
        {
            ShotDraft shot = film.Shot("city_wakes", 6.4f, Ink).Fades(0.25f, 0.3f).Vignette(0.35f);
            shot.Layer("dome", CityDome(0.6f), 0f, -10f, 0.35f, 0f, 0f);

            PixelCanvas[] city = Frames(12, (k, i) => CityLine(k * 1.05f, 9920));
            shot.Anim("city", city, 12f / 4.2f, 0.8f, false, 0f, 0f, 0.7f, 0f, 0f);

            shot.Layer("smog", Smog(), 0f, 0f, 0.8f, 0f, 0f).Keys(
                new FilmKey(0f, 0f, 0f, 1f, 1f), new FilmKey(0.8f, 0f, 0f, 1f, 1f), new FilmKey(4.4f, 0f, -20f, 1f, 0f, 1));

            // A onda de luz passando pela cidade.
            var wave = new PixelCanvas(120, 200);
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 120; x++)
                {
                    float k = 1f - Math.Abs(x - 60f) / 60f;
                    k = k * k * 0.6f;
                    if (k > Bayer(x, y) * 0.5f) wave.Set(x, y, Pure.WithAlpha((int)(k * 255f)));
                }
            shot.Layer("wave", wave, -60f, 0f, 0.7f, 0f, 0f, true).Keys(
                new FilmKey(0f, -120f, 0f, 1f, 0f), new FilmKey(0.8f, -120f, 0f, 1f, 1f), new FilmKey(5.0f, CityW, 0f, 1f, 1f, 0));

            shot.Anim("roof", new[] { Rooftop(false), Rooftop(true) }, 1f / 2.6f, 0.4f, false, 140f, 200f, 1.15f, 0f, 1f);

            shot.Cam(new FilmCam(0f, 160f, 100f, 1.0f), new FilmCam(6.4f, 470f, 92f, 1.0f, 1));
            shot.Cue(0.8f, "PurifyWave", 0.8f).Cue(2.6f, "FinaleLight", 1.1f, 0.5f);

            shot.Spray(1, 0f, 40f, 520f, 140f, 1.0f, 6.4f, 50f, C(255, 240, 200), Warm.WithAlpha(0), 1.6f, 9930)
                .Move(0f, -14f, 8f, 8f).Look(1, 0f, true, 0.85f);
        }

        // ------------------------------------------------------------------
        // 5. O Scrap se apaga
        // ------------------------------------------------------------------

        private static void ScrapDims(FilmDraft film)
        {
            ShotDraft shot = film.Shot("scrap_dims", 5.8f, Ink).Fades(0.3f, 0.5f).Vignette(0.6f);
            shot.Layer("hall", ThroneHall(0f, 1f), 160f, 90f, 0.8f);
            shot.Layer("throne", PalaceArt.Throne(true), 214f, 172f, 1f, 0.5f, 1f).Keys(new FilmKey(0f, 214f, 172f, 0.82f));

            // A luz saindo do trono aberto, pra cima.
            var column = new PixelCanvas(120, 200);
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 120; x++)
                {
                    float k = Math.Max(0f, 1f - Math.Abs(x - 60f) / (14f + (200 - y) * 0.2f)) * (0.4f + y / 400f);
                    if (k > Bayer(x, y) * 0.6f) column.Set(x, y, Pure.WithAlpha((int)(Math.Min(1f, k) * 230f)));
                }
            shot.Layer("light", column, 214f, 160f, 1f, 0.5f, 1f, true);

            // O Scrap de costas pra luz, os olhos se apagando, a cabeca caindo.
            var frames = new PixelCanvas[8];
            for (int i = 0; i < frames.Length; i++)
            {
                float k = i / 7f;
                var pose = new HeroArt.Pose { BodyY = k * 3f, HeadY = k * 3f, HeadX = k * 1.5f, Squash = 1f - k * 0.08f, EyesShut = i == 7, Angle = -62f + k * 50f };
                PixelCanvas body = HeroArt.DrawPose(ScrapLook(1f - k), pose);
                frames[i] = Scaled(Relight(body, C(110, 110, 130), Pure, 1f, -0.3f, 2, 1f), 3);
            }

            shot.Anim("scrap", frames, 2.2f, 1.0f, false, 104f, 172f, 1f, 0.5f, 1f);

            shot.Cam(new FilmCam(0f, 176f, 96f, 1.0f), new FilmCam(5.8f, 130f, 116f, 1.25f, 0));
            shot.Cue(1.0f, "HeartGive", 0.6f, 0.5f).Cue(4.2f, "KingLieDown", 1.8f, 0.3f);

            // Poeira acesa subindo na luz.
            shot.Spray(1, 180f, 40f, 70f, 140f, 0f, 5.8f, 24f, C(255, 255, 255), Sky.WithAlpha(0), 2.5f, 9950)
                .Move(0f, -10f, 4f, 4f).Look(1, 0f, true);
        }

        // ------------------------------------------------------------------
        // 6. O muro dos nomes
        // ------------------------------------------------------------------

        private const int LetterX = 120;
        private const int LetterY = 104;

        private static PixelCanvas NameWall(int letters)
        {
            var c = new PixelCanvas(340, 200);
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 340; x++)
                {
                    float n = ArtKit.Noise(x * 0.04f, y * 0.06f, 9960) * 0.6f + ArtKit.Noise(x * 0.15f, y * 0.15f, 9961) * 0.4f;
                    c.Set(x, y, Ramp(Rock, 0.28f + n * 0.4f, x, y));
                }

            var scratch = C(170, 160, 146);
            var fresh = C(250, 244, 230);
            PixelFont.Draw(c, "LAMPIÃO-12. AINDA ACENDO.", 26, 22, scratch, default, PixelFont.Style.Engraved, 1, 9962);
            PixelFont.Draw(c, "BIGORNA. TRABALHEI 40 ANOS.", 150, 42, scratch, default, PixelFont.Style.Engraved, 1, 9963);
            PixelFont.Draw(c, "PEQUENA. AINDA AQUI.", 40, 62, scratch, default, PixelFont.Style.Engraved, 1, 9964);
            PixelFont.Draw(c, "COLETOR N.º 1. TAMBÉM ERA UM DE NÓS.", 70, 84, scratch, default, PixelFont.Style.Engraved, 1, 9965);
            PixelFont.DrawCentered(c, "QUEM LER ISTO, LEMBRE-SE DE NÓS.", 170, 176, scratch, default, PixelFont.Style.Engraved, 1, 9966);

            const string name = "SCRAP";
            string part = name.Substring(0, Math.Min(name.Length, letters));
            if (part.Length > 0)
            {
                PixelFont.Draw(c, part, LetterX, LetterY, fresh, default, PixelFont.Style.Engraved, 3, 9967);
                Light(c, LetterX + PixelFont.Measure(part, 3) * 0.5f, LetterY + 10f, 60f, Warm, 0.18f, false, 4f);
            }

            return c;
        }

        private static void NameOnWall(FilmDraft film)
        {
            ShotDraft shot = film.Shot("name_on_wall", 7.0f, Ink).Fades(0.5f, 1.2f).Vignette(0.55f);

            var walls = new PixelCanvas[6];
            for (int i = 0; i < 6; i++) walls[i] = NameWall(i);
            shot.Anim("wall", walls, 1f / 0.6f, 1.0f, false, 160f, 90f);

            // A luz do lampiao dos descartados.
            var lamp = new PixelCanvas(340, 200);
            Halo(lamp, 120f, 150f, 150f, Warm, 0.35f, 6f);
            shot.Layer("lamp", lamp, 160f, 90f, 1f, 0.5f, 0.5f, true);

            // A Pequena, com o giz, de frente pro muro.
            PixelCanvas[] pequena = DiscardedArt.Frames(3);
            var small = new PixelCanvas[pequena.Length];
            for (int i = 0; i < small.Length; i++) small[i] = Scaled(Relight(pequena[i], C(190, 170, 150), Warm, -1f, -0.2f, 2, 0.8f), 2);
            shot.Anim("pequena", small, 8f, 0f, true, 96f, 182f, 1.05f, 0.5f, 1f).Keys(
                new FilmKey(0f, 84f, 182f), new FilmKey(1.0f, 88f, 182f), new FilmKey(4.0f, 100f, 182f, 1f, 1f, 0));

            // Os outros descartados, mais perto, de costas, olhando.
            for (int v = 0; v < 3; v++)
            {
                if (v == 1) continue;
                PixelCanvas sil = Scaled(Relight(DiscardedArt.Frames(v)[0], C(40, 34, 32), Warm, v == 0 ? 1f : -1f, -0.4f, 2, 0.8f), 2);
                shot.Layer("friend" + v, v == 2 ? Flipped(sil) : sil, v == 0 ? 22f : 300f, 196f, 1.3f, 0.5f, 1f);
            }

            shot.Cam(new FilmCam(0f, 160f, 84f, 1.12f), new FilmCam(7.0f, 170f, 96f, 1.0f, 2));
            for (int i = 0; i < 5; i++) shot.Cue(1.0f + i * 0.6f, "BenchStrike", 1.6f + i * 0.05f, 0.35f);

            // O po do giz caindo de cada letra.
            for (int i = 0; i < 5; i++)
            {
                float lx = LetterX + 9 + i * 18f;
                shot.Spray(0, lx, LetterY + 4, 12f, 18f, 1.0f + i * 0.6f, 1.5f + i * 0.6f, 40f, C(250, 246, 236), C(200, 190, 170, 0), 0.8f, 9970 + i)
                    .Move(0f, 10f, 12f, 8f, 60f).Look(1);
            }

            shot.Spray(1, 0f, 60f, 320f, 120f, 0f, 7.0f, 10f, Warm, Candle.WithAlpha(0), 2.5f, 9980)
                .Move(0f, -8f, 4f, 4f).Look(1, 0f, true, 1.2f);
        }
    }
}
