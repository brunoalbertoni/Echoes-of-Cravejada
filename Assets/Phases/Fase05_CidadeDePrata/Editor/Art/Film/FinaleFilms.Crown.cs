using System;
using static EchoesOfCravejada.EditorTools.FilmPaint;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O fim da coroa, em quatro planos:
    ///
    ///   1 O golpe      a coroa no chao, longe do trono; o Scrap desce o cinzel
    ///                  e as 925 facetas estouram, soltando a luz presa.
    ///   2 A luz volta  da janela do palacio: a luz cai em riscos sobre a
    ///                  cidade e as janelas acendem — mas a fumaca verde fica.
    ///   3 O atelie     o Crisol e a Assistente montam corpos brutos copiando
    ///                  a planta do prototipo n.º 7; os olhos acendem um a um.
    ///   4 O poco       de baixo: os descartados sobem pela rampa de luz ate a
    ///                  borda, onde os corpos novos estendem a mao.
    /// </summary>
    public static partial class FinaleFilms
    {
        public static FilmDraft Crown()
        {
            var film = new FilmDraft();
            CrownStrike(film);
            LightReturns(film);
            Workshop(film);
            PitRise(film);
            return film;
        }

        // ------------------------------------------------------------------
        // 1. O golpe na coroa
        // ------------------------------------------------------------------

        private static void CrownStrike(FilmDraft film)
        {
            ShotDraft shot = film.Shot("crown_strike", 5.0f, Ink).Fades(0.5f, 0.2f).Vignette(0.5f);

            // A sala, com o trono pequeno la atras: a coroa esta longe dele.
            PixelCanvas hall = ThroneHall(0.8f, 0f);
            shot.Layer("hall", hall, 160f, 90f, 0.7f).Keys(new FilmKey(0f, 260f, 90f, 1f));

            PixelCanvas[] crown = Frames(8, (k, i) => i == 0 ? Crown(0.55f, 0f, 0.15f, 0f, false) : Crown(0.55f, 0.9f, 0.15f, k * 1.2f, false));
            shot.Anim("crown", crown, 10f, 1.55f, false, 176f, 174f, 1f, 0.5f, 1f);

            var burst = new PixelCanvas(320, 200);
            Rays(burst, 160f, 150f, 22, 230f, 8f, Warm, 0.9f, 10010, 180f, 180f, true);
            Halo(burst, 160f, 150f, 70f, C(255, 250, 230), 0.9f, 6f);
            shot.Layer("burst", burst, 176f, 168f, 1f, 0.5f, 0.75f, true).Keys(
                new FilmKey(0f, 176f, 168f, 0.4f, 0f), new FilmKey(1.55f, 176f, 168f, 0.4f, 0f),
                new FilmKey(1.8f, 176f, 168f, 1.1f, 1f, 2), new FilmKey(5.0f, 176f, 168f, 1.5f, 0.5f));

            // O Scrap: prepara e desce o cinzel.
            PixelCanvas[] scrap =
            {
                ScrapFrame(HeroArt.Idle), ScrapFrame(HeroArt.Windup), ScrapFrame(HeroArt.Windup + 1), ScrapFrame(HeroArt.StrikeDown),
                ScrapFrame(HeroArt.StrikeDown + 1), ScrapFrame(HeroArt.StrikeDown + 1), ScrapFrame(HeroArt.Idle),
            };
            for (int i = 0; i < scrap.Length; i++) scrap[i] = Scaled(Relight(scrap[i], C(190, 186, 200), Warm, 1f, -0.3f, 1, 0.8f), 3);
            shot.Anim("scrap", scrap, 5f, 0.7f, false, 112f, 174f, 1f, 0.5f, 1f);

            shot.Cam(new FilmCam(0f, 150f, 100f, 1.08f), new FilmCam(1.55f, 160f, 108f, 1.14f, 3), new FilmCam(5.0f, 168f, 86f, 1.0f, 2));
            shot.Shake(1.55f, 0.9f, 5f);
            shot.Flash(1.55f, 0.45f, C(255, 246, 220), 0.9f);
            shot.Cue(1.1f, "Swing").Cue(1.55f, "Hit", 0.6f).Cue(1.6f, "KingShatter", 1.1f).Cue(1.9f, "FinaleLight", 0.9f);

            // As facetas voando e a luz presa subindo em riscos.
            shot.Spray(0, 170f, 140f, 12f, 12f, 1.55f, 1.7f, 300f, C(240, 246, 255), Quartz[2].WithAlpha(0), 1.3f, 10011)
                .Move(0f, -60f, 30f, 30f, 200f).Burst(140f).Look(2);
            shot.Spray(2, 140f, 120f, 70f, 40f, 1.6f, 5.0f, 60f, C(255, 250, 220), Warm.WithAlpha(0), 0.9f, 10012)
                .Move(0f, -220f, 60f, 40f).Look(1, 0f, true);
        }

        // ------------------------------------------------------------------
        // 2. A luz volta pra cidade
        // ------------------------------------------------------------------

        /// <summary>A moldura escura da janela do palacio (camada da frente).</summary>
        private static PixelCanvas PalaceWindowFrame()
        {
            var c = new PixelCanvas(420, 240);
            float cx = 210f, half = 150f, top = 20f;
            for (int y = 0; y < 240; y++)
                for (int x = 0; x < 420; x++)
                {
                    float u = (x - cx) / half;
                    float arch = top + 70f - (float)Math.Sqrt(Math.Max(0f, 1f - u * u)) * 70f;
                    bool open = Math.Abs(u) < 1f && y > arch && y < 214;
                    bool mullion = open && (Math.Abs(x - cx) < 3 || Math.Abs(y - 120) < 2);
                    if (open && !mullion) continue;
                    float rim = open ? 0.4f : Math.Abs(u) < 1.06f && y > arch - 6 ? 0.3f : 0.08f;
                    c.Set(x, y, Ramp(Marble, rim, x, y));
                }

            return c;
        }

        private static void LightReturns(FilmDraft film)
        {
            ShotDraft shot = film.Shot("light_returns", 6.0f, Ink).Fades(0.2f, 0.3f).Vignette(0.4f);
            shot.Layer("dome", CityDome(0.3f), 0f, -10f, 0.4f, 0f, 0f);

            // As janelas acendem do palacio pra fora (meio pras bordas).
            PixelCanvas[] city = Frames(10, (k, i) => CityRadial(k, 10020));
            shot.Anim("city", city, 10f / 3.4f, 1.4f, false, 0f, 0f, 0.7f, 0f, 0f);

            // A fumaca verde continua, e grossa: o Coracao segue doente e o
            // Oxide ainda cobre a cidade inteira, mesmo com as luzes acesas.
            var sick = new PixelCanvas(CityW, 200);
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < CityW; x++)
                {
                    float k = y / 200f * 0.55f;
                    if (k > Bayer(x, y) * 0.8f) sick.Set(x, y, OxideDeep.WithAlpha((int)(k * 255f)));
                }
            shot.Layer("sick", sick, 0f, 0f, 0.75f, 0f, 0f);
            shot.Layer("smog", Smog(), 0f, 0f, 0.8f, 0f, 0f).Keys(new FilmKey(0f, 0f, 0f, 1f, 1f), new FilmKey(6f, -30f, -6f, 1f, 1f, 0));
            shot.Layer("smog2", Flipped(Smog()), 0f, 30f, 0.9f, 0f, 0f).Keys(new FilmKey(0f, 0f, 40f, 1f, 0.8f), new FilmKey(6f, 30f, 30f, 1f, 0.9f, 0));

            shot.Layer("frame", PalaceWindowFrame(), 340f, 100f, 1.4f);

            shot.Cam(new FilmCam(0f, 340f, 92f, 1.35f), new FilmCam(6.0f, 340f, 96f, 1.0f, 2));
            shot.Cue(0.6f, "FinaleLight", 1.1f).Cue(1.4f, "PurifyWave", 1.2f, 0.6f);

            // Os riscos de luz saindo do palacio e caindo sobre os distritos.
            shot.Spray(2, 330f, 40f, 20f, 20f, 0.6f, 4.0f, 40f, C(255, 250, 220), Warm.WithAlpha(0), 1.6f, 10021)
                .Move(0f, -60f, 160f, 30f, 120f).Look(1, 0f, true, 0.7f);
            shot.Spray(1, 100f, 60f, 480f, 120f, 1.5f, 6.0f, 50f, Warm, Candle.WithAlpha(0), 1.2f, 10022)
                .Move(0f, -10f, 6f, 6f).Look(1, 0f, true, 0.7f);

            // Esporos de Oxide boiando por cima de tudo.
            shot.Spray(1, 0f, 20f, CityW, 180f, 0f, 6.0f, 70f, Oxide, OxideDeep.WithAlpha(0), 2.4f, 10023)
                .Move(6f, -6f, 8f, 6f).Look(2, 0f, true, 0.85f);
        }

        /// <summary>A mesma cidade, com as janelas acendendo do meio (o palacio) pras bordas.</summary>
        private static PixelCanvas CityRadial(float lit, int seed)
        {
            PixelCanvas full = CityLine(1.1f, seed);
            PixelCanvas dark = CityLine(0f, seed);
            var c = new PixelCanvas(full.Width, full.Height);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    float d = Math.Abs(x - 340f) / 340f;
                    c.Pixels[x + y * c.Width] = d < lit ? full.Pixels[x + y * c.Width] : dark.Pixels[x + y * c.Width];
                }

            return c;
        }

        // ------------------------------------------------------------------
        // 3. O atelie
        // ------------------------------------------------------------------

        /// <summary>O atelie do Crisol: madeira, ferramentas na parede, a planta do n.º 7 e a lampada.</summary>
        private static PixelCanvas WorkshopRoom()
        {
            var c = new PixelCanvas(340, 200);

            // Paredes de tabuas.
            for (int y = 0; y < 200; y++)
                for (int x = 0; x < 340; x++)
                {
                    bool seam = x % 28 == 0;
                    float grain = ArtKit.Noise(x * 0.3f, y * 0.02f, 10030) * 0.25f;
                    c.Set(x, y, seam ? C(30, 18, 14) : Ramp(LabArt.Walnut, 0.25f + grain - y / 900f, x, y));
                }

            // A planta azul do prototipo n.º 7, pregada na parede.
            int bx = 120, by = 20, bw = 100, bh = 70;
            for (int y = by; y < by + bh; y++)
                for (int x = bx; x < bx + bw; x++)
                    c.Set(x, y, ((x - bx) % 10 == 0 || (y - by) % 10 == 0) ? C(70, 110, 170) : C(40, 74, 130));

            PixelCanvas plan = Scaled(HeroArt.Frames(HeroArt.Scrap())[HeroArt.Idle], 1);
            for (int y = 0; y < plan.Height; y++)
                for (int x = 0; x < plan.Width; x++)
                {
                    bool edge = plan.Get(x, y).A > 0 && (plan.Get(x - 1, y).A == 0 || plan.Get(x + 1, y).A == 0 || plan.Get(x, y - 1).A == 0 || plan.Get(x, y + 1).A == 0);
                    if (edge) c.Set(bx + 26 + x, by + 14 + y, C(220, 236, 255));
                }

            PixelFont.Draw(c, "N.º 7", bx + 6, by + 4, C(220, 236, 255), default, PixelFont.Style.Engraved, 1, 10031);
            foreach ((int x, int y) in new[] { (bx + 2, by + 2), (bx + bw - 3, by + 2) }) { c.Set(x, y, Gold[3]); c.Set(x + 1, y, Gold[1]); }

            // Ferramentas penduradas.
            for (int i = 0; i < 5; i++)
            {
                int tx = 20 + i * 18;
                ArtKit.Line(c, tx, 30, tx, 70 - i % 2 * 10, C(150, 150, 160));
                c.Fill(tx - 2, 28, 5, 4, LabArt.Walnut[1]);
            }

            // A bancada comprida.
            for (int y = 150; y < 200; y++)
                for (int x = 0; x < 340; x++)
                    c.Set(x, y, y < 156 ? Ramp(LabArt.Walnut, 0.75f - (y - 150) * 0.08f, x, y) : Ramp(LabArt.Walnut, 0.2f, x, y));

            // A lampada pendurada.
            ArtKit.Line(c, 170, 0, 170, 12, C(40, 30, 26));
            Oval(c, 170f, 16f, 14f, 6f, (nx, ny, px, py) => ny < 0.2f ? Ramp(Brass, 0.6f - nx * 0.3f, px, py) : Warm);
            Light(c, 170f, 80f, 190f, Warm, 0.35f, false, 7f);
            return c;
        }

        private static void Workshop(FilmDraft film)
        {
            ShotDraft shot = film.Shot("workshop", 6.4f, Ink).Fades(0.3f, 0.3f).Vignette(0.5f);
            shot.Layer("room", WorkshopRoom(), 160f, 90f, 0.85f);

            // Os corpos brutos na bancada (o mesmo corpo do n.º 7): os olhos acendem um a um.
            for (int b = 0; b < 4; b++)
            {
                PixelCanvas off = Scaled(Relight(HeroArt.Frames(BruteLook(0f))[HeroArt.Idle], C(170, 150, 130), Warm, 0f, -1f, 1, 0.7f), 2);
                PixelCanvas on = Scaled(Relight(HeroArt.Frames(BruteLook(1f))[HeroArt.Idle], C(200, 180, 160), Warm, 0f, -1f, 1, 0.7f), 2);
                float bxp = 108f + b * 50f;
                var layer = shot.Anim("brute" + b, new[] { off, on }, 1f, 1.4f + b * 0.9f - 1f, false, bxp, 152f, 1f, 0.5f, 1f);
                shot.Cue(1.4f + b * 0.9f, "RunePolished", 0.9f + b * 0.1f, 0.6f);
            }

            // O Crisol, soldando, e a Assistente com a prancheta.
            PixelCanvas[] crisol = District2NpcArt.Frames(District2NpcArt.Crisol);
            var crisolBig = new PixelCanvas[crisol.Length];
            for (int i = 0; i < crisol.Length; i++) crisolBig[i] = Scaled(Relight(crisol[i], C(210, 190, 170), Warm, 0.3f, -1f, 1, 0.6f), 2);
            shot.Anim("crisol", crisolBig, 6f, 0f, true, 66f, 190f, 1.1f, 0.5f, 1f);

            PixelCanvas[] assistant = District4NpcArt.Frames();
            var assistantBig = new PixelCanvas[assistant.Length];
            for (int i = 0; i < assistant.Length; i++) assistantBig[i] = Scaled(Relight(Flipped(assistant[i]), C(210, 190, 170), Warm, -0.3f, -1f, 1, 0.6f), 2);
            shot.Anim("assistant", assistantBig, 6f, 0f, true, 290f, 190f, 1.1f, 0.5f, 1f);

            shot.Cam(new FilmCam(0f, 150f, 96f, 1.12f), new FilmCam(6.4f, 172f, 88f, 1.0f, 2));

            // Faiscas da solda saindo da mao do Crisol.
            shot.Spray(0, 86f, 140f, 8f, 8f, 0.2f, 6.4f, 40f, C(255, 250, 200), Candle.WithAlpha(0), 0.5f, 10040)
                .Move(20f, -50f, 50f, 30f, 260f).Look(1, 0f, true);
            for (int b = 0; b < 4; b++) shot.Cue(0.4f + b * 1.4f, "BenchStrike", 1.3f, 0.3f);
        }

        /// <summary>O corpo bruto: a pedra bruta do n.º 7, com os olhos de 0 (apagados) a 1 (acesos, ambar).</summary>
        private static HeroArt.Look BruteLook(float eyes)
        {
            HeroArt.Look look = HeroArt.Scrap();
            var off = C(30, 28, 30);
            look.EyeCore = Rgba.Lerp(off, C(255, 246, 210), eyes);
            look.EyeGlow = Rgba.Lerp(off, C(255, 190, 90), eyes);
            look.EyeHalo = Rgba.Lerp(off, C(240, 200, 140), eyes);
            return look;
        }

        // ------------------------------------------------------------------
        // 4. O poco
        // ------------------------------------------------------------------

        private const int PitH = 420;

        private static PixelCanvas PitShaft()
        {
            var c = new PixelCanvas(320, PitH);

            // A luz la em cima; a sucata e o verde la embaixo.
            VGradient(c, 0, 0, 320, PitH, C(255, 236, 190), C(170, 130, 90), C(60, 44, 40), C(16, 22, 20));
            for (int y = 0; y < PitH; y++)
            {
                int wall = 70 - (int)(y * 0.08f) + (int)(Math.Sin(y * 0.05f) * 6f);
                for (int x = 0; x < 320; x++)
                {
                    if (x > wall && x < 320 - wall) continue;
                    float n = ArtKit.Noise(x * 0.08f, y * 0.05f, 10050);
                    float light = 0.15f + n * 0.3f + (1f - y / (float)PitH) * 0.45f;
                    c.Set(x, y, Ramp(Rock, light, x, y));
                }
            }

            // A montanha de sucata no fundo.
            for (int i = 0; i < 40; i++)
            {
                float x = 40 + ArtKit.Hash(i, 0, 10051) % 240;
                float y = PitH - 10 - ArtKit.Hash(i, 1, 10051) % 40;
                Oval(c, x, y, 14f, 8f, (nx, ny, px, py) => Ramp(ArtKit.Rust, 0.5f - ny * 0.3f - nx * 0.2f, px, py));
            }

            for (int i = 0; i < 10; i++) ArtKit.Puff(c, 30f + i * 28f, PitH - 50f - i % 3 * 10f, 16f, C(90, 190, 120), OxideDeep, 90);

            // O chao do poco, por onde eles andam, e a saida iluminada a direita.
            for (int y = PitH - 30; y < PitH; y++)
                for (int x = 0; x < 320; x++)
                    c.Set(x, y, Ramp(Rock, 0.35f - (y - PitH + 30) * 0.01f, x, y));
            ArtKit.Line(c, 0, PitH - 30, 320, PitH - 30, Rock[4]);
            Light(c, 320f, PitH - 60f, 120f, Warm, 0.5f, false, 6f);

            return c;
        }

        private static void PitRise(FilmDraft film)
        {
            ShotDraft shot = film.Shot("pit_rise", 8.0f, Ink).Fades(0.3f, 1.4f).Vignette(0.45f);
            shot.Layer("pit", PitShaft(), 0f, 0f, 1f, 0f, 0f);

            // Os descartados saindo do poco, devagarinho, um atras do outro, pra luz.
            float floor = PitH - 30f;
            for (int v = 0; v < 4; v++)
            {
                PixelCanvas[] body = DiscardedArt.Frames(v);
                var lit = new PixelCanvas[body.Length];
                for (int i = 0; i < body.Length; i++) lit[i] = Scaled(Relight(body[i], C(200, 180, 160), Warm, 1f, -0.5f, 2, 0.8f), 2);
                float start = 20f - v * 50f;
                shot.Anim("walker" + v, lit, 5f, 0f, true, start, floor, 1f, 0.5f, 1f).Keys(
                    new FilmKey(0f, start, floor),
                    new FilmKey(8.0f, start + 230f, floor, 1f, 1f, 0));
            }

            // O verde ainda la, no fundo do poco.
            shot.Spray(1, 0f, floor - 60f, 320f, 60f, 0f, 8.0f, 30f, Oxide, OxideDeep.WithAlpha(0), 2.4f, 10061)
                .Move(0f, -8f, 6f, 4f).Look(2, 0f, true);

            shot.Cam(new FilmCam(0f, 150f, floor - 64f, 1.05f), new FilmCam(8.0f, 180f, floor - 70f, 1.1f, 0));
            shot.Cue(0.5f, "PurifyWave", 0.7f, 0.6f);

            shot.Spray(1, 200f, floor - 120f, 120f, 120f, 0f, 8.0f, 20f, C(255, 240, 200), Warm.WithAlpha(0), 2.0f, 10060)
                .Move(-6f, -10f, 6f, 6f).Look(1, 0f, true);
        }
    }
}
