using System;
using System.Collections.Generic;
using Shot = EchoesOfCravejada.EditorTools.PurificationCartoonArt.Shot;
using Emitter = EchoesOfCravejada.EditorTools.PurificationCartoonArt.Emitter;
using EmitterKind = EchoesOfCravejada.EditorTools.PurificationCartoonArt.EmitterKind;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// As cutscenes desenhadas do fim do jogo, no mesmo tocador da purificacao
    /// da fase 1 (cada quadro vira de "antes" pra "depois" na ordem do mapa, com
    /// a faixa de luz e as particulas):
    ///
    ///   A Transformacao   a coroa racha; a Coluna do Coracao sobe verde; a
    ///                     prata do Rei vira Oxide; ele se ergue.
    ///   A Coluna (fim 1)  o nucleo do Scrap acende; a luz desce pela Coluna;
    ///                     o Coracao fica limpo; a cidade acende; o Scrap se
    ///                     apaga; a Pequena risca SCRAP no muro dos nomes.
    ///   A Coroa (fim 2)   a coroa estoura; a luz volta pra cidade ainda verde;
    ///                     o atelie monta corpos brutos; os descartados sobem.
    ///
    /// Tudo 320x180, sem UnityEngine.
    /// </summary>
    public static class FinaleCartoonArt
    {
        private const int W = PurificationCartoonArt.Width;
        private const int H = PurificationCartoonArt.Height;
        private const byte Static = PurificationCartoonArt.Static;

        private static readonly Rgba Green = new Rgba(120, 255, 170);
        private static readonly Rgba GreenDeep = new Rgba(30, 110, 70);
        private static readonly Rgba White = new Rgba(250, 252, 255);
        private static readonly Rgba Warm = new Rgba(255, 220, 150);

        private static PixelCanvas[] _radiant;
        private static PixelCanvas[] _corrupt;
        private static PixelCanvas _hero;

        private static PixelCanvas[] Radiant => _radiant ?? (_radiant = KingArt.Frames(false));
        private static PixelCanvas[] Corrupt => _corrupt ?? (_corrupt = KingArt.Frames(true));
        private static PixelCanvas Hero => _hero ?? (_hero = PurificationCartoonArt.Hero().After);

        public static Shot[] Transformation() => new[] { CrownCracks(), ColumnRises(), LightShatters(), HeRises() };

        /// <summary>A Coluna: o nucleo desce, o Coracao fica limpo, a cidade acende, o Scrap se apaga e ganha o nome no muro.</summary>
        public static Shot[] EndingA() => new[] { CoreLights(), LightDescends(), HeartCleansed(), CityWakes(), ScrapDims(), NameOnWall() };

        /// <summary>A Coroa: as 925 facetas estouram, a luz volta pra cidade ainda verde, os corpos brutos saem do atelie e os descartados sobem.</summary>
        public static Shot[] EndingB() => new[] { CrownShatters(), LightReturns(), BruteWorkshop(), DiscardedRise() };

        // ------------------------------------------------------------------
        // Ferramentas
        // ------------------------------------------------------------------

        private static Shot Make(string name, PixelCanvas before, PixelCanvas after, Func<int, int, float> order, float duration,
            Emitter[] emitters, float zoomFrom = 1.06f, float zoomTo = 1f, float panX = 0f, float panY = 0f)
        {
            var map = new PixelCanvas(W, H);
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    Rgba a = before.Get(x, y);
                    Rgba b = after.Get(x, y);
                    byte v;
                    if (a.R == b.R && a.G == b.G && a.B == b.B) v = Static;
                    else
                    {
                        float k = Math.Max(0f, Math.Min(1f, order(x, y) + (ArtKit.Hash(x, y, 7001) % 100 - 50) / 2000f));
                        v = (byte)(k * 254f);
                    }

                    map.Set(x, y, new Rgba(v, v, v, 255));
                }
            }

            return new Shot
            {
                Name = name, Before = before, After = after, Order = map, Emitters = emitters, Duration = duration,
                ZoomFrom = zoomFrom, ZoomTo = zoomTo, PanX = panX, PanY = panY,
            };
        }

        private static Func<int, int, float> Radial(float cx, float cy, float reach) =>
            (x, y) => (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / reach;

        private static Func<int, int, float> BottomUp() => (x, y) => 1f - y / (float)H;
        private static Func<int, int, float> TopDown() => (x, y) => y / (float)H;
        private static Func<int, int, float> LeftRight() => (x, y) => x / (float)W;

        private static PixelCanvas Copy(PixelCanvas source)
        {
            var c = new PixelCanvas(source.Width, source.Height);
            Array.Copy(source.Pixels, c.Pixels, source.Pixels.Length);
            return c;
        }

        /// <summary>Cola um desenho escalado (vizinho mais proximo) com o canto em (x, y).</summary>
        private static void Paste(PixelCanvas c, PixelCanvas art, float scale, int x, int y, int sx = 0, int sy = 0, int sw = -1, int sh = -1)
        {
            if (sw < 0) sw = art.Width;
            if (sh < 0) sh = art.Height;
            int w = (int)(sw * scale);
            int h = (int)(sh * scale);

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    Rgba p = art.Get(sx + (int)(px / scale), sy + (int)(py / scale));
                    if (p.A > 0) c.Blend(x + px, y + py, p);
                }
            }
        }

        private static void Gradient(PixelCanvas c, Rgba top, Rgba bottom)
        {
            for (int y = 0; y < H; y++)
            {
                Rgba row = Rgba.Lerp(top, bottom, y / (float)H);
                for (int x = 0; x < W; x++) c.Set(x, y, row);
            }
        }

        private static void Darken(PixelCanvas c, float amount)
        {
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                c.Pixels[i] = new Rgba((int)(p.R * (1f - amount)), (int)(p.G * (1f - amount)), (int)(p.B * (1f - amount)), p.A);
            }
        }

        private static void Tint(PixelCanvas c, Rgba color, float amount)
        {
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                if (p.A == 0) continue;
                c.Pixels[i] = Rgba.Lerp(p, color.WithAlpha(p.A), amount);
            }
        }

        /// <summary>O salao do trono ao fundo: marmore escuro, colunas, o tapete.</summary>
        private static PixelCanvas Hall(bool green)
        {
            var c = new PixelCanvas(W, H);
            Gradient(c, green ? new Rgba(10, 26, 20) : new Rgba(18, 20, 32), green ? new Rgba(26, 46, 36) : new Rgba(40, 42, 58));

            for (int i = 0; i < 6; i++)
            {
                int x = 14 + i * 58;
                for (int y = 20; y < 160; y++)
                {
                    for (int dx = 0; dx < 14; dx++)
                    {
                        float light = dx < 4 ? 0.5f : dx > 10 ? -0.4f : 0f;
                        c.Set(x + dx, y, ArtKit.Tone(green ? new[] { new Rgba(20, 40, 32), new Rgba(36, 62, 50), new Rgba(56, 86, 70), new Rgba(84, 116, 96) } : LabArt.Marble, light - 0.4f));
                    }
                }
            }

            c.Fill(0, 158, W, 22, green ? new Rgba(20, 36, 28) : new Rgba(52, 54, 70));
            c.Fill(110, 158, 100, 22, LabArt.Velvet[1]);
            return c;
        }

        /// <summary>
        /// O corte da cidade: a Coluna de quartzo descendo do palacio (em cima)
        /// ate o Coracao (embaixo), atravessando as camadas — latao, geodes, rocha.
        /// </summary>
        private static PixelCanvas Column(Rgba vein, float veinGlow, bool cracked, bool dark)
        {
            var c = new PixelCanvas(W, H);
            Gradient(c, new Rgba(26, 24, 40), new Rgba(10, 8, 18));

            // As camadas da cidade de cada lado: o palacio, as caldeiras, as geodes.
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    if (Math.Abs(x - 160) < 26) continue;
                    float n = ArtKit.Noise(x * 0.05f, y * 0.08f, 7101);
                    Rgba layer = y < 40 ? new Rgba(70, 76, 96) : y < 90 ? new Rgba(78, 58, 34) : new Rgba(46, 34, 70);
                    c.Set(x, y, Rgba.Lerp(c.Get(x, y), layer, 0.35f + n * 0.3f));
                }
            }

            // As luzinhas dos distritos.
            for (int i = 0; i < 40; i++)
            {
                int x = ArtKit.Hash(i, 0, 7102) % W;
                if (Math.Abs(x - 160) < 30) continue;
                int y = 10 + ArtKit.Hash(i, 1, 7102) % 140;
                c.Set(x, y, dark ? new Rgba(30, 30, 40) : Warm);
            }

            // A Coluna.
            for (int y = 0; y < 150; y++)
            {
                float half = 14f + (float)Math.Sin(y * 0.09f) * 3f;
                for (int x = (int)(160 - half); x < (int)(160 + half); x++)
                {
                    float dx = (x - 160f) / half;
                    c.Set(x, y, ArtKit.Tone(new[] { new Rgba(100, 120, 150), new Rgba(160, 180, 210), new Rgba(210, 226, 244), White }, 0.3f - dx * 0.6f + ((x + y / 3) % 9 < 1 ? 0.5f : 0f)));
                }

                int vx = 160 + (int)(Math.Sin(y * 0.18f) * 5f);
                c.Set(vx, y, vein);
                c.Set(vx + 1, y, vein.Scaled(0.8f));
            }

            if (veinGlow > 0f) ArtKit.Glow(c, 160f, 75f, 70f, vein, veinGlow, false);

            // O Coracao, embaixo.
            Heart(c, 160f, 160f, 22f, vein, cracked ? 2 : 0, dark);

            if (cracked)
            {
                for (int k = 0; k < 4; k++) ArtKit.Crack(c, 160f, 80f, 40f + k * 90f, 40, 7103 + k, 0f);
            }

            return c;
        }

        /// <summary>O Coracao de Quartzo: um losango grande. Broken 0 inteiro, 1 rachado, 2 em pedacos.</summary>
        private static void Heart(PixelCanvas c, float cx, float cy, float r, Rgba core, int broken, bool dark)
        {
            if (broken == 2)
            {
                for (int i = 0; i < 9; i++)
                {
                    float a = i * 40f;
                    (float px, float py) = ArtKit.Along(cx, cy, a, r * 0.9f);
                    ArtKit.Polygon(c, new[] { (px, py - 6f), (px + 6f, py + 3f), (px - 5f, py + 5f) }, (x, y) => dark ? new Rgba(44, 46, 60) : core.Scaled(0.6f));
                }

                return;
            }

            ArtKit.Polygon(c, new[] { (cx, cy - r * 1.4f), (cx + r, cy), (cx, cy + r * 1.4f), (cx - r, cy) }, (x, y) =>
            {
                float d = Math.Abs(x - cx) / r + Math.Abs(y - cy) / (r * 1.4f);
                Rgba tone = d < 0.35f ? White : d < 0.7f ? core : core.Scaled(0.5f);
                return ((int)(x + y) % 11 == 0) ? tone.Scaled(0.8f) : tone;
            });
            ArtKit.Glow(c, cx, cy, r * 3f, core, 0.6f, true);

            if (broken == 1) ArtKit.Crack(c, cx, cy - r, 90f, (int)(r * 2.6f), 7111, 1f);
        }

        /// <summary>A Cidade de Prata dentro da geode: torres, pontes, janelas; a fumaca verde em cima ou nao.</summary>
        private static PixelCanvas City(bool lit, bool smog, bool people)
        {
            var c = new PixelCanvas(W, H);
            Gradient(c, smog ? new Rgba(20, 44, 32) : new Rgba(34, 30, 56), smog ? new Rgba(12, 22, 18) : new Rgba(16, 14, 28));

            // A boca da geode, com cristais nas bordas.
            for (int i = 0; i < 30; i++)
            {
                float x = i < 15 ? i * 4f : W - (i - 15) * 4f;
                float y = 6f + ArtKit.Hash(i, 0, 7201) % 30;
                ArtKit.Taper(c, x, 0f, x + (i < 15 ? 6f : -6f), y + 14f, 5f, 0.5f, ArtKit.Plain(new[] { new Rgba(60, 40, 100), new Rgba(110, 76, 170), new Rgba(170, 130, 230), new Rgba(230, 210, 255) }), 0.2f);
            }

            // As torres.
            for (int i = 0; i < 14; i++)
            {
                int x = 8 + i * 22 + ArtKit.Hash(i, 1, 7202) % 8;
                int top = 60 + ArtKit.Hash(i, 2, 7202) % 60;
                int w = 12 + ArtKit.Hash(i, 3, 7202) % 8;
                for (int y = top; y < H; y++) for (int dx = 0; dx < w; dx++) c.Set(x + dx, y, dx < 3 ? new Rgba(90, 96, 120) : new Rgba(56, 60, 80));
                ArtKit.Polygon(c, new[] { (x - 1f, top + 0f), (x + w / 2f, top - 14f), (x + w + 1f, top + 0f) }, (px, py) => new Rgba(110, 116, 140));

                for (int wy = top + 6; wy < H - 6; wy += 7)
                {
                    for (int wx = x + 3; wx < x + w - 2; wx += 4)
                    {
                        bool on = lit && ArtKit.Hash(wx, wy, 7203) % 3 != 0;
                        c.Set(wx, wy, on ? Warm : new Rgba(26, 28, 38));
                        c.Set(wx, wy + 1, on ? Warm.Scaled(0.8f) : new Rgba(26, 28, 38));
                    }
                }
            }

            // O palacio, no alto do meio.
            ArtKit.Polygon(c, new[] { (130f, 70f), (160f, 30f), (190f, 70f) }, (px, py) => new Rgba(170, 176, 200));
            c.Fill(130, 70, 60, 110, new Rgba(120, 126, 150));

            if (smog)
            {
                for (int i = 0; i < 40; i++)
                {
                    float px = ArtKit.Hash(i, 4, 7204) % W;
                    float py = 20f + ArtKit.Hash(i, 5, 7204) % 100;
                    ArtKit.Puff(c, px, py, 14f, new Rgba(90, 190, 120), GreenDeep, 120);
                }
            }

            if (people)
            {
                // Os Tarnished nos telhados, com as primeiras lampadas.
                for (int i = 0; i < 7; i++)
                {
                    float px = 20f + i * 44f;
                    float py = 150f - (i % 3) * 18f;
                    ArtKit.Ball(c, px, py - 6f, 3f, 3f, ArtKit.Plain(ArtKit.Copper), 0.2f);
                    ArtKit.Limb(c, px, py - 3f, px, py + 6f, 2.4f, ArtKit.Brass, 0.1f);
                    ArtKit.Glow(c, px + 4f, py - 2f, 10f, Warm, 0.9f, true);
                    c.Set((int)px + 4, (int)py - 2, White);
                }
            }

            return c;
        }

        // ------------------------------------------------------------------
        // A Transformacao
        // ------------------------------------------------------------------

        private static Shot CrownCracks()
        {
            PixelCanvas before = Hall(false);
            PixelCanvas after = Hall(true);

            // De perto: a cabeca e a coroa (o pedaco de cima do quadro do Rei, aumentado).
            Paste(before, Radiant[KingArt.Idle], 3f, 70, 0, 60, 28, 60, 60);
            Paste(after, Corrupt[KingArt.Idle], 3f, 70, 0, 60, 28, 60, 60);

            for (int k = 0; k < 5; k++) ArtKit.Crack(after, 160f, 30f, 60f + k * 30f, 50, 7301 + k, 1f);
            ArtKit.Glow(after, 160f, 40f, 60f, Green, 0.35f, false);

            return Make("crown_cracks", before, after, Radial(160f, 30f, 200f), 3.2f,
                new[] { new Emitter(EmitterKind.Sparks, 110, 10, 100, 60, 0.1f), new Emitter(EmitterKind.Flakes, 90, 20, 140, 120, 0.4f) },
                1.15f, 1.0f);
        }

        private static Shot ColumnRises()
        {
            PixelCanvas before = Column(new Rgba(180, 200, 230), 0f, false, false);
            PixelCanvas after = Column(Green, 0.45f, false, false);

            return Make("column_rises", before, after, BottomUp(), 3.0f,
                new[] { new Emitter(EmitterKind.Steam, 130, 0, 60, 170, 0.2f), new Emitter(EmitterKind.Glint, 140, 120, 40, 50, 0f) },
                1.0f, 1.08f, 0f, -20f);
        }

        private static Shot LightShatters()
        {
            PixelCanvas before = Hall(false);
            PixelCanvas after = Hall(true);

            // O tronco, de perto: a prata vira Oxide do meio pra fora.
            Paste(before, Radiant[KingArt.Hurt], 2f, 80, 0, 44, 62, 80, 90);
            Paste(after, Corrupt[KingArt.Hurt], 2f, 80, 0, 44, 62, 80, 90);

            return Make("light_shatters", before, after, (x, y) => Math.Abs(x - 160f) / 170f, 3.2f,
                new[] { new Emitter(EmitterKind.Glint, 20, 20, 140, 140, 0.1f), new Emitter(EmitterKind.Sparks, 40, 30, 120, 120, 0.3f) },
                1.05f, 1.12f);
        }

        private static Shot HeRises()
        {
            PixelCanvas before = Hall(false);
            PixelCanvas after = Hall(true);

            Paste(before, PalaceArt.Throne(false), 0.75f, 97, 42);
            Paste(after, PalaceArt.Throne(true), 0.75f, 97, 42);
            Paste(before, Radiant[KingArt.Fall + KingArt.FallFrames - 1], 1f, 76, 4);
            Paste(after, Corrupt[KingArt.Cast + 2], 1f, 76, 4);
            ArtKit.Glow(after, 160f, 70f, 120f, Green, 0.3f, false);

            return Make("he_rises", before, after, BottomUp(), 3.6f,
                new[] { new Emitter(EmitterKind.Steam, 40, 60, 240, 110, 0.2f), new Emitter(EmitterKind.Sparks, 100, 10, 120, 80, 0.5f) },
                1.12f, 0.98f);
        }

        // ------------------------------------------------------------------
        // O final: O Grande Polimento
        // ------------------------------------------------------------------

        private static Shot CoreLights()
        {
            PixelCanvas before = Copy(Hero);
            Tint(before, new Rgba(30, 60, 50), 0.25f);
            PixelCanvas after = Copy(Hero);

            // O nucleo no peito acende e a luz escorre pelas juntas.
            ArtKit.Glow(after, 160f, 108f, 80f, White, 0.75f, false);
            for (int i = 0; i < 12; i++)
            {
                (float tx, float ty) = ArtKit.Along(160f, 108f, i * 30f, 150f);
                ArtKit.Line(after, 160, 108, (int)tx, (int)ty, White.WithAlpha(110));
            }

            return Make("core_lights", before, after, Radial(160f, 108f, 190f), 3.2f,
                new[] { new Emitter(EmitterKind.Glint, 120, 80, 80, 60, 0.1f), new Emitter(EmitterKind.Sparks, 60, 20, 200, 150, 0.4f) },
                1.12f, 1.0f);
        }

        private static Shot LightDescends()
        {
            PixelCanvas before = Column(Green, 0.4f, false, false);
            PixelCanvas after = Column(White, 0.5f, false, false);
            return Make("light_descends", before, after, TopDown(), 3.0f,
                new[] { new Emitter(EmitterKind.Glint, 140, 0, 40, 160, 0f) }, 1.0f, 1.08f, 0f, 20f);
        }

        private static Shot HeartCleansed()
        {
            var before = new PixelCanvas(W, H);
            Gradient(before, new Rgba(16, 26, 22), new Rgba(4, 10, 8));
            var after = new PixelCanvas(W, H);
            Gradient(after, new Rgba(40, 44, 70), new Rgba(14, 14, 26));

            Heart(before, 160f, 90f, 52f, Green, 1, false);
            Heart(after, 160f, 90f, 52f, new Rgba(200, 226, 255), 0, false);
            for (int i = 0; i < 10; i++) ArtKit.Puff(before, 60f + i * 20f, 140f + i % 3 * 8f, 16f, new Rgba(90, 190, 120), GreenDeep, 130);

            return Make("heart_cleansed", before, after, Radial(160f, 90f, 180f), 3.4f,
                new[] { new Emitter(EmitterKind.Steam, 60, 100, 200, 70, 0.1f), new Emitter(EmitterKind.Glint, 110, 30, 100, 120, 0.3f) },
                1.15f, 1.0f);
        }

        private static Shot CityWakes()
        {
            PixelCanvas before = City(false, true, false);
            PixelCanvas after = City(true, false, true);
            return Make("city_wakes", before, after, BottomUp(), 4.2f,
                new[] { new Emitter(EmitterKind.Steam, 0, 20, 320, 120, 0f), new Emitter(EmitterKind.Glint, 0, 60, 320, 110, 0.4f) },
                1.0f, 1.1f, 0f, -12f);
        }

        // ------------------------------------------------------------------
        // O fim da Coluna: o Scrap se apaga e vira nome no muro
        // ------------------------------------------------------------------

        private static Shot ScrapDims()
        {
            PixelCanvas before = Copy(Hero);
            ArtKit.Glow(before, 160f, 108f, 70f, White, 0.4f, false);
            PixelCanvas after = Copy(Hero);
            Darken(after, 0.55f);
            Tint(after, new Rgba(70, 76, 96), 0.45f);

            return Make("scrap_dims", before, after, Radial(160f, 108f, 200f), 3.6f,
                new[] { new Emitter(EmitterKind.Glint, 130, 80, 60, 50, 0f), new Emitter(EmitterKind.Flakes, 60, 20, 200, 150, 0.5f) },
                1.0f, 1.1f);
        }

        /// <summary>O muro dos nomes do poco: rocha, os riscos de antes, e o nome novo.</summary>
        private static PixelCanvas NameWall(bool scrap)
        {
            var c = new PixelCanvas(W, H);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float n = ArtKit.Noise(x * 0.04f, y * 0.06f, 7401);
                    c.Set(x, y, Rgba.Lerp(new Rgba(40, 36, 40), new Rgba(84, 76, 72), n));
                }

            var scratch = new Rgba(196, 186, 170);
            var fresh = new Rgba(246, 240, 226);
            PixelFont.Draw(c, "LAMPIÃO-12. AINDA ACENDO.", 22, 18, scratch, default, PixelFont.Style.Engraved, 1, 7402);
            PixelFont.Draw(c, "BIGORNA. TRABALHEI 40 ANOS.", 120, 40, scratch, default, PixelFont.Style.Engraved, 1, 7403);
            PixelFont.Draw(c, "PEQUENA. AINDA AQUI.", 34, 62, scratch, default, PixelFont.Style.Engraved, 1, 7404);
            PixelFont.Draw(c, "COLETOR N.º 1. TAMBÉM ERA UM DE NÓS.", 40, 88, scratch, default, PixelFont.Style.Engraved, 1, 7405);
            PixelFont.DrawCentered(c, "QUEM LER ISTO, LEMBRE-SE DE NÓS.", 160, 160, scratch, default, PixelFont.Style.Engraved, 1, 7406);

            if (scrap)
            {
                PixelFont.DrawCentered(c, "SCRAP", 160, 112, fresh, default, PixelFont.Style.Engraved, 3, 7407);
                ArtKit.Glow(c, 160f, 122f, 50f, Warm, 0.25f, false);
            }

            // O prego pendurado no fim do muro.
            ArtKit.Line(c, 268, 120, 268, 134, new Rgba(150, 150, 160));
            c.Set(268, 119, new Rgba(220, 220, 230));
            return c;
        }

        private static Shot NameOnWall()
        {
            return Make("name_on_wall", NameWall(false), NameWall(true), LeftRight(), 4.4f,
                new[] { new Emitter(EmitterKind.Flakes, 110, 100, 100, 40, 0f) }, 1.08f, 1.0f);
        }

        // ------------------------------------------------------------------
        // O fim da Coroa: salvo por sucata
        // ------------------------------------------------------------------

        private static Shot CrownShatters()
        {
            PixelCanvas before = Hall(false);
            PixelCanvas after = Hall(false);
            PixelCanvas crown = LootArt.FallenCrown();
            float scale = 120f / crown.Width;
            int cx = 160 - (int)(crown.Width * scale / 2f);
            int cy = 150 - (int)(crown.Height * scale);
            Paste(before, crown, scale, cx, cy);
            ArtKit.Glow(before, 160f, 120f, 70f, Warm, 0.25f, false);

            // Estourada: os cacos voando e a luz presa saindo em raios.
            for (int i = 0; i < 24; i++)
            {
                (float px, float py) = ArtKit.Along(160f, 120f, i * 15f + ArtKit.Hash(i, 0, 7501) % 9, 40f + ArtKit.Hash(i, 1, 7501) % 90);
                Rgba shard = i % 3 == 0 ? White : new Rgba(200, 220, 250);
                ArtKit.Polygon(after, new[] { (px, py - 4f), (px + 4f, py + 2f), (px - 3f, py + 3f) }, (x, y) => shard);
            }

            for (int i = 0; i < 16; i++)
            {
                (float tx, float ty) = ArtKit.Along(160f, 120f, i * 22.5f, 220f);
                ArtKit.Line(after, 160, 120, (int)tx, (int)ty, Warm.WithAlpha(120));
            }

            ArtKit.Glow(after, 160f, 120f, 150f, White, 0.7f, false);

            return Make("crown_shatters", before, after, Radial(160f, 120f, 200f), 3.2f,
                new[] { new Emitter(EmitterKind.Sparks, 120, 80, 80, 80, 0f), new Emitter(EmitterKind.Glint, 20, 20, 280, 140, 0.3f) },
                1.12f, 1.0f);
        }

        private static Shot LightReturns()
        {
            PixelCanvas before = City(false, true, false);
            PixelCanvas after = City(true, true, false);
            return Make("light_returns", before, after, TopDown(), 4.0f,
                new[] { new Emitter(EmitterKind.Glint, 0, 40, 320, 120, 0.2f) }, 1.0f, 1.08f, 0f, -10f);
        }

        /// <summary>Um corpo bruto, igual a ficha do n.º 7: cabeca de quartzo sem lapidar, dois olhos e uma mola.</summary>
        private static void BruteBody(PixelCanvas c, float x, float y, float s, bool lit)
        {
            Rgba[] quartz = lit
                ? new[] { new Rgba(110, 120, 150), new Rgba(170, 182, 210), new Rgba(214, 224, 244), White }
                : new[] { new Rgba(34, 36, 46), new Rgba(52, 56, 70), new Rgba(70, 76, 94), new Rgba(90, 96, 116) };
            Rgba spring = lit ? new Rgba(200, 170, 110) : new Rgba(70, 62, 50);
            Rgba eye = lit ? Warm : new Rgba(20, 20, 26);

            // A mola.
            for (int i = 0; i < 6; i++)
            {
                float yy = y - i * 3f * s;
                ArtKit.Line(c, (int)(x - 4 * s), (int)yy, (int)(x + 4 * s), (int)(yy - 1.5f * s), spring);
            }

            // A cabeca, um bloco bruto de faces tortas.
            float hy = y - 30f * s;
            ArtKit.Polygon(c, new[]
            {
                (x - 11f * s, hy + 4f * s), (x - 6f * s, hy - 9f * s), (x + 7f * s, hy - 10f * s),
                (x + 12f * s, hy + 2f * s), (x + 8f * s, hy + 12f * s), (x - 8f * s, hy + 12f * s),
            }, (px, py) => ArtKit.Tone(quartz, 0.6f - (px - x) / (24f * s) - (py - hy) / (40f * s)));

            for (int e = -1; e <= 1; e += 2)
            {
                int ex = (int)(x + e * 4f * s);
                int ey = (int)hy;
                c.Set(ex, ey, eye);
                c.Set(ex + 1, ey, eye);
                c.Set(ex, ey + 1, eye);
                c.Set(ex + 1, ey + 1, eye);
                if (lit) ArtKit.Glow(c, ex, ey, 6f * s, Warm, 0.5f, true);
            }
        }

        private static PixelCanvas Workshop(bool built)
        {
            var c = new PixelCanvas(W, H);
            Gradient(c, new Rgba(30, 26, 34), new Rgba(14, 12, 18));

            // A bancada comprida e a lampada pendurada.
            c.Fill(20, 140, 280, 8, LabArt.Walnut[2]);
            c.Fill(20, 140, 280, 2, LabArt.Walnut[3]);
            c.Fill(30, 148, 6, 32, LabArt.Walnut[1]);
            c.Fill(284, 148, 6, 32, LabArt.Walnut[1]);
            ArtKit.Line(c, 160, 0, 160, 30, new Rgba(60, 60, 70));
            ArtKit.Glow(c, 160f, 34f, 120f, Warm, built ? 0.55f : 0.2f, false);

            // Os corpos em fila na bancada: esqueletos apagados, depois montados e acesos.
            for (int i = 0; i < 5; i++) BruteBody(c, 50f + i * 34f, 139f, 0.9f, built);

            // O Crisol, do lado, lustrando a primeira cabeca.
            PixelCanvas crisol = District2NpcArt.Frames(District2NpcArt.Crisol)[built ? 2 : 0];
            Paste(c, crisol, 2f, 216, 12);
            return c;
        }

        private static Shot BruteWorkshop()
        {
            return Make("brute_workshop", Workshop(false), Workshop(true), LeftRight(), 4.0f,
                new[] { new Emitter(EmitterKind.Sparks, 40, 90, 180, 50, 0.1f), new Emitter(EmitterKind.Glint, 40, 80, 180, 60, 0.4f) },
                1.06f, 1.0f);
        }

        private static PixelCanvas Pit(bool risen)
        {
            var c = new PixelCanvas(W, H);
            Gradient(c, risen ? new Rgba(60, 56, 80) : new Rgba(16, 30, 24), risen ? new Rgba(22, 20, 30) : new Rgba(6, 12, 10));

            // As paredes do poco e a montanha de sucata no fundo.
            for (int y = 0; y < H; y++)
            {
                int wall = 40 - y / 6;
                for (int x = 0; x < wall; x++) c.Set(x, y, new Rgba(44, 38, 40));
                for (int x = W - wall; x < W; x++) c.Set(x, y, new Rgba(44, 38, 40));
            }

            for (int i = 0; i < 30; i++)
            {
                float px = 30f + ArtKit.Hash(i, 0, 7601) % 260;
                ArtKit.Ball(c, px, 168f + ArtKit.Hash(i, 1, 7601) % 10, 10f, 6f, ArtKit.Plain(ArtKit.Rust), 0.1f);
            }

            if (!risen)
            {
                // Os descartados, de lataria amassada, no verde.
                for (int v = 0; v < DiscardedArt.Count; v++)
                {
                    PixelCanvas body = DiscardedArt.Frames(v)[0];
                    Paste(c, body, 1.6f, 50 + v * 60, 92);
                }

                for (int i = 0; i < 12; i++) ArtKit.Puff(c, 20f + i * 26f, 120f + i % 3 * 10f, 16f, new Rgba(90, 190, 120), GreenDeep, 110);
            }
            else
            {
                // Corpos brutos novos, acesos, subindo pela rampa de luz.
                ArtKit.Glow(c, 160f, 0f, 200f, White, 0.45f, false);
                for (int v = 0; v < 4; v++) BruteBody(c, 88f + v * 50f, 168f - v * 22f, 1.1f, true);
            }

            return c;
        }

        private static Shot DiscardedRise()
        {
            return Make("discarded_rise", Pit(false), Pit(true), BottomUp(), 4.4f,
                new[] { new Emitter(EmitterKind.Glint, 40, 40, 240, 120, 0.2f), new Emitter(EmitterKind.Steam, 0, 100, 320, 70, 0f) },
                1.0f, 1.1f, 0f, -14f);
        }
    }
}
