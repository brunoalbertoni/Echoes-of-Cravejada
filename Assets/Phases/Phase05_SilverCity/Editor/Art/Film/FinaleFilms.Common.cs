using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.FilmPaint;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os filmes do fim do jogo (tocados pelo FilmCutscene):
    ///
    ///   King()    A Transformacao: a coroa racha, o Oxide sobe do Coracao
    ///             pela Coluna ate o palacio, toma a prata do Rei e ele se
    ///             ergue corrompido.
    ///   Column()  O Nucleo: o Scrap entrega o nucleo a Coluna, a luz desce
    ///             limpa ate o Coracao, a cidade acende, o Scrap se apaga e a
    ///             Pequena risca o nome dele no muro.
    ///   Crown()   A Coroa: o Scrap quebra a coroa, a luz presa volta pra
    ///             cidade (ainda verde), o atelie monta corpos brutos e os
    ///             descartados sobem do poco.
    ///
    /// Aqui ficam as pecas que os tres usam: a paleta, a coroa de perto e o
    /// corte da cidade inteira (do palacio ao Coracao).
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static partial class FinaleFilms
    {
        // ------------------------------------------------------------------
        // Paleta
        // ------------------------------------------------------------------

        private static readonly Rgba Ink = C(10, 8, 16);
        private static readonly Rgba Night = C(16, 14, 28);

        private static readonly Rgba[] Gold = { C(70, 40, 18), C(130, 82, 30), C(196, 140, 50), C(240, 198, 96), C(255, 244, 196) };
        private static readonly Rgba[] Quartz = { C(64, 70, 104), C(112, 126, 170), C(170, 188, 226), C(222, 232, 252), C(255, 255, 255) };
        private static readonly Rgba[] SickQuartz = { C(14, 52, 40), C(30, 104, 74), C(70, 170, 120), C(150, 236, 180), C(236, 255, 240) };
        private static readonly Rgba[] Silver = { C(40, 44, 62), C(84, 92, 118), C(146, 156, 184), C(206, 214, 232), C(250, 252, 255) };
        private static readonly Rgba[] Patina = { C(14, 30, 26), C(28, 62, 50), C(48, 104, 80), C(86, 158, 116), C(150, 220, 170) };
        private static readonly Rgba[] Velvet = { C(30, 6, 14), C(66, 14, 28), C(110, 28, 44), C(160, 50, 66), C(204, 96, 104) };
        private static readonly Rgba[] Marble = { C(30, 32, 48), C(58, 62, 84), C(96, 102, 128), C(146, 152, 176), C(200, 206, 224) };
        private static readonly Rgba[] Rock = { C(14, 12, 18), C(30, 26, 34), C(48, 42, 50), C(70, 62, 68), C(98, 88, 90) };
        private static readonly Rgba[] Brass = { C(46, 26, 12), C(98, 58, 22), C(160, 106, 40), C(214, 164, 78), C(250, 226, 150) };
        private static readonly Rgba[] Amethyst = { C(26, 14, 44), C(58, 32, 96), C(104, 64, 162), C(164, 120, 222), C(226, 204, 255) };

        private static readonly Rgba Oxide = C(120, 255, 170);
        private static readonly Rgba OxideHot = C(220, 255, 230);
        private static readonly Rgba OxideDeep = C(24, 120, 76);
        private static readonly Rgba Pure = C(236, 244, 255);
        private static readonly Rgba Warm = C(255, 214, 140);
        private static readonly Rgba Candle = C(255, 180, 90);
        private static readonly Rgba Sky = C(150, 200, 255);

        // ------------------------------------------------------------------
        // A coroa de perto
        // ------------------------------------------------------------------

        /// <summary>
        /// A coroa de 925 facetas vista de frente e um pouco de baixo: o aro de
        /// ouro com as gemas e as pontas de quartzo (as de tras menores e
        /// escuras). crack de 0 a 1 abre a rachadura verde; sick tinge o
        /// quartzo de Oxide; explode separa as pecas pra fora (0 inteira).
        /// Vem num quadro transparente de 320x210 (s = 1) e ja com contorno.
        /// </summary>
        private static PixelCanvas Crown(float s, float crack, float sick, float explode, bool head, int seed = 9001)
        {
            int w = (int)(320 * s), h = (int)((head ? 260 : 190) * s);
            var c = new PixelCanvas(w, h);
            float cx = w * 0.5f;
            float cy = 150f * s;
            float rx = 128f * s;
            float ry = 20f * s;
            float band = 28f * s;

            float Bottom(float x)
            {
                float n = (x - cx) / rx;
                return cy + ry * (float)Math.Sqrt(Math.Max(0f, 1f - n * n));
            }

            Rgba[] quartz = Mix(Quartz, SickQuartz, sick);

            // O rosto embaixo do aro: a testa de prata esculpida, as sobrancelhas
            // fundas e os olhos acesos (dourados; verdes quando adoece).
            if (head)
            {
                Rgba[] skin = Mix(Silver, Patina, sick * 0.8f);
                Rgba[] hair = Mix(new[] { C(26, 28, 40), C(52, 56, 76), C(96, 102, 128), C(146, 152, 178), C(196, 202, 222) }, Patina, sick * 0.6f);
                float faceTop = cy - band + 4f * s;
                float faceW = rx * 0.8f;

                for (int y = (int)faceTop; y < h; y++)
                {
                    float v = (y - faceTop) / (h - faceTop);
                    float half = faceW * (1f - v * v * 0.35f);
                    for (int x = (int)(cx - rx * 1.05f); x <= (int)(cx + rx * 1.05f); x++)
                    {
                        float nx = (x - cx) / half;

                        // O cabelo cai dos lados, em mechas largas.
                        if (Math.Abs(nx) > 0.78f && Math.Abs(nx) < 1.2f)
                        {
                            float strand = (float)Math.Sin((x - cx) * 0.3f / s + y * 0.09f / s) * 0.5f + 0.5f;
                            float light = 0.3f - nx * 0.2f - v * 0.3f + strand * 0.2f;
                            c.Set(x, y, Ramp(hair, light, x, y));
                            continue;
                        }

                        if (Math.Abs(nx) > 0.78f) continue;
                        // Na sombra da coroa: escuro em cima, clareando com a luz dos olhos.
                        float shade = 0.18f - nx * 0.18f + v * 0.25f - Math.Abs(nx) * Math.Abs(nx) * 0.25f;
                        c.Set(x, y, Ramp(skin, shade, x, y));
                    }
                }

                // As sobrancelhas fundas e os olhos.
                float browY = faceTop + 44f * s;
                Rgba eye = Rgba.Lerp(C(255, 236, 170), Oxide, sick);
                Rgba eyeHot = Rgba.Lerp(C(255, 255, 236), OxideHot, sick);
                for (int k = -1; k <= 1; k += 2)
                {
                    float ex = cx + k * 34f * s;
                    Poly(c, (x, y) => Ramp(skin, 0.12f, x, y),
                        (ex - 26f * s * k, browY - 4f * s), (ex + 20f * s * k, browY - 12f * s), (ex + 24f * s * k, browY + 2f * s), (ex - 22f * s * k, browY + 10f * s));
                    Poly(c, (x, y) => Ramp(skin, 0.86f, x, y),
                        (ex - 26f * s * k, browY - 7f * s), (ex + 20f * s * k, browY - 15f * s), (ex + 20f * s * k, browY - 12f * s), (ex - 26f * s * k, browY - 4f * s));
                    Oval(c, ex, browY + 6f * s, 13f * s, 4f * s, (nx, ny, px, py) => Math.Abs(ny) < 0.5f && Math.Abs(nx) < 0.75f ? eyeHot : eye);
                    Light(c, ex, browY + 6f * s, 30f * s, eye, 0.45f, false, 5f);
                }

                // A ponte do nariz, saindo entre os olhos, pegando a luz deles.
                Poly(c, (x, y) => Ramp(skin, x < cx ? 0.5f : 0.22f, x, y),
                    (cx - 4f * s, browY + 2f * s), (cx + 4f * s, browY + 2f * s), (cx + 10f * s, h), (cx - 10f * s, h));
            }

            // As pontas de tras: menores, escuras, entre as da frente.
            var back = new List<(float X, float Base, float H, float W)>();
            for (int i = -3; i <= 4; i++)
            {
                float n = (i - 0.5f) * 0.2f;
                float x = cx + n * rx * 0.92f;
                float height = (64f - Math.Abs(i - 0.5f) * 9f) * s;
                back.Add((x, Bottom(x) - band - 2f * s - 8f * s, height, 7f * s * (float)Math.Sqrt(1f - n * n)));
            }

            foreach (var b in back) Prism(c, b.X, b.Base, b.H, b.W, Darker(quartz, 0.62f), 0f, explode, cx, cy, seed + (int)b.X);

            // O aro.
            for (int y = (int)(cy - ry - band - 2); y <= (int)(cy + ry + 2); y++)
            {
                for (int x = (int)(cx - rx); x <= (int)(cx + rx); x++)
                {
                    float bottom = Bottom(x);
                    float top = bottom - band;
                    if (y > bottom || y < top) continue;
                    float n = (x - cx) / rx;
                    float v = (y - top) / band;
                    float light = 0.62f - n * 0.38f + (v < 0.12f ? 0.35f : 0f) - (v > 0.85f ? 0.3f : 0f) + (Math.Abs(v - 0.5f) < 0.06f ? -0.18f : 0f);
                    int ex = (int)(x + ExplodeShift(x, cx, explode, s) * 0.3f);
                    c.Set(ex, y, Ramp(Gold, light, x, y));
                }
            }

            // Filigrana: arcos gravados entre as gemas.
            for (float n = -0.95f; n <= 0.95f; n += 0.035f)
            {
                float x = cx + n * rx;
                float mid = Bottom(x) - band * 0.5f;
                if (ArtKit.Hash((int)(n * 100), 3, seed) % 3 == 0) continue;
                c.Set((int)x, (int)(mid + (float)Math.Sin(n * 40f) * 4f * s), Gold[1]);
            }

            // As gemas do aro.
            for (int g = -4; g <= 4; g++)
            {
                float n = g * 0.215f;
                float x = cx + n * rx;
                float y = Bottom(x) - band * 0.5f;
                float gw = 6.5f * s * (float)Math.Sqrt(1f - n * n), gh = 8f * s;
                Rgba[] gem = g % 2 == 0
                    ? new[] { C(60, 6, 20), C(140, 20, 40), C(220, 60, 70), C(255, 180, 170) }
                    : new[] { C(10, 24, 70), C(30, 70, 160), C(80, 150, 230), C(200, 236, 255) };
                Oval(c, x, y, gw + 2f * s, gh + 2f * s, (nx, ny, px, py) => Ramp(Gold, 0.55f - nx * 0.3f - ny * 0.3f, px, py));
                Oval(c, x, y, gw, gh, (nx, ny, px, py) => Ramp(gem, 0.7f - ny * 0.45f - nx * 0.25f, px, py));
                c.Set((int)(x - gw * 0.35f), (int)(y - gh * 0.45f), C(255, 255, 255));
                c.Set((int)(x - gw * 0.35f) + 1, (int)(y - gh * 0.45f), gem[3]);
            }

            // As pontas da frente, das de fora pras do meio.
            var order = new[] { -4, 4, -3, 3, -2, 2, -1, 1, 0 };
            foreach (int i in order)
            {
                float n = i * 0.205f;
                float x = cx + n * rx;
                float baseY = Bottom(x) - band + 3f * s;
                float height = (98f - Math.Abs(i) * 13f + (ArtKit.Hash(i, 1, seed) % 7)) * s;
                float width = (12.5f - Math.Abs(i) * 0.8f) * s * (float)Math.Sqrt(1f - n * n * 0.8f);
                Prism(c, x, baseY, height, width, quartz, n * 7f * s, explode, cx, cy, seed + i * 7);

                // A garra de ouro que segura a ponta.
                for (int k = -1; k <= 1; k += 2)
                {
                    Thick(c, x + k * width * 0.9f, baseY + 2f * s, x + k * width * 0.55f, baseY - 9f * s, 1.6f * s, Gold[3]);
                }
            }

            if (crack > 0f)
            {
                // A rachadura: nasce no pe da ponta do meio, sobe por ela e desce pelo aro.
                var paths = new List<List<(float X, float Y)>>
                {
                    CrackPath(cx + 2f * s, Bottom(cx) - band, -88f, (int)(100 * s), seed + 1, 0.45f),
                    CrackPath(cx, Bottom(cx) - band + 4f * s, 170f, (int)(110 * s), seed + 2, 0.35f),
                    CrackPath(cx, Bottom(cx) - band + 6f * s, 12f, (int)(120 * s), seed + 3, 0.35f),
                    CrackPath(cx - 20f * s, Bottom(cx) - band - 30f * s, -125f, (int)(60 * s), seed + 4, 0.5f),
                    CrackPath(cx + 24f * s, Bottom(cx) - band - 20f * s, -60f, (int)(70 * s), seed + 5, 0.5f),
                };

                for (int p = 0; p < paths.Count; p++)
                {
                    float local = Math.Max(0f, Math.Min(1f, crack * 1.5f - p * 0.12f));
                    DrawCrack(c, paths[p], local, Ink, OxideHot, Oxide);
                    int lit = (int)(paths[p].Count * local);
                    for (int i = 0; i < lit; i += 6) Light(c, paths[p][i].X, paths[p][i].Y, 9f * s, Oxide, 0.22f * crack, true, 3f);
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static float ExplodeShift(float x, float cx, float explode, float s) => (x - cx) * explode * 0.9f;

        /// <summary>Uma ponta de quartzo: prisma de seis faces visto de frente, com a ponta facetada.</summary>
        private static void Prism(PixelCanvas c, float x, float baseY, float h, float w, Rgba[] ramp, float lean,
            float explode, float cx, float cy, int seed)
        {
            // Estourando, cada ponta voa pra fora e pra cima.
            float dx = (x - cx) * explode * 1.4f + (ArtKit.Hash(seed, 0, 1) % 21 - 10) * explode * 2f;
            float dy = -explode * (60f + ArtKit.Hash(seed, 1, 1) % 50);
            x += dx;
            baseY += dy;

            float shoulder = baseY - h * 0.74f;
            float tipX = x + lean, tipY = baseY - h;

            var body = new[] { (x - w, baseY), (x + w, baseY), (x + w, shoulder), (tipX, tipY), (x - w, shoulder) };
            ArtKit.Polygon(c, body, (px, py) =>
            {
                float u = (px - (x - w)) / (2f * w);
                bool tip = py < shoulder + (Math.Abs(px - tipX) / w) * (shoulder - tipY) * 0.0f && py < shoulder;
                float light;
                if (u < 0.3f) light = 0.8f;
                else if (u < 0.55f) light = 0.98f;
                else if (u < 0.8f) light = 0.55f;
                else light = 0.3f;
                if (py < shoulder) light += px < tipX ? 0.12f : -0.15f;
                float depth = (baseY - py) / h;
                light += depth * 0.1f - 0.05f;

                // Reflexos internos: linhas finas inclinadas.
                if (((int)(px * 0.7f + py * 0.35f) + seed) % 13 == 0) light += 0.25f;
                return Ramp(ramp, light, (int)px, (int)py);
            });

            // Arestas claras entre as faces.
            ArtKit.Line(c, (int)(x - w * 0.4f), (int)baseY - 1, (int)(x - w * 0.4f), (int)shoulder, ramp[ramp.Length - 1]);
            ArtKit.Line(c, (int)(x - w * 0.4f), (int)shoulder, (int)tipX, (int)tipY + 1, ramp[ramp.Length - 1]);
            ArtKit.Line(c, (int)(x + w * 0.1f), (int)baseY - 1, (int)(x + w * 0.1f), (int)shoulder + 2, ramp[ramp.Length - 2]);

            // Brilho na ponta.
            c.Set((int)tipX, (int)tipY + 2, ramp[ramp.Length - 1]);
        }

        private static Rgba[] Darker(Rgba[] ramp, float k)
        {
            var r = new Rgba[ramp.Length];
            for (int i = 0; i < ramp.Length; i++) r[i] = ramp[i].Scaled(k);
            return r;
        }

        private static Rgba[] Mix(Rgba[] a, Rgba[] b, float k)
        {
            if (k <= 0f) return a;
            var r = new Rgba[a.Length];
            for (int i = 0; i < a.Length; i++) r[i] = Rgba.Lerp(a[i], b[Math.Min(i, b.Length - 1)], k);
            return r;
        }

        // ------------------------------------------------------------------
        // O corte da cidade: do palacio ao Coracao
        // ------------------------------------------------------------------

        public const int SectionW = 320;
        public const int SectionH = 760;
        private const float ColumnX = 160f;
        private const float ColumnTop = 150f;
        private const float HeartY = 690f;

        /// <summary>
        /// A cidade cortada ao meio, de cima a baixo: o salao do trono (prata),
        /// os Ateliers (vidro, lilas), as Tubulacoes (latao, vapor), as Geodes
        /// (ametista) e, no fundo, a caverna do Coracao de Quartzo. A Coluna
        /// atravessa tudo pelo meio. lit: as janelas da cidade acesas.
        /// heart: 0 verde (doente), 1 limpo.
        /// </summary>
        private static PixelCanvas Section(float lit, float heart)
        {
            var c = new PixelCanvas(SectionW, SectionH);

            // Rocha em tudo, depois os andares escavados.
            for (int y = 0; y < SectionH; y++)
                for (int x = 0; x < SectionW; x++)
                {
                    float n = ArtKit.Noise(x * 0.06f, y * 0.09f, 811) * 0.6f + ArtKit.Noise(x * 0.2f, y * 0.25f, 812) * 0.4f;
                    float strata = (float)Math.Sin(y * 0.21f + n * 4f) * 0.08f;
                    c.Set(x, y, Ramp(Rock, 0.3f + n * 0.35f + strata, x, y));
                }

            PalaceFloor(c, 0, 168, lit);
            Ateliers(c, 186, 318, lit);
            Pipes(c, 338, 470, lit);
            Geodes(c, 490, 600, lit);
            HeartCave(c, 616, SectionH, heart);

            // A Coluna, do chao do trono ate o Coracao.
            for (int y = (int)ColumnTop; y < HeartY - 40; y++)
            {
                float half = 8f;
                for (int x = (int)(ColumnX - half - 3); x <= (int)(ColumnX + half + 3); x++)
                {
                    float u = (x - ColumnX) / half;
                    if (Math.Abs(u) > 1.25f) continue;
                    if (Math.Abs(u) > 1f)
                    {
                        c.Set(x, y, Ink);
                        continue;
                    }

                    float light = 0.42f - u * 0.3f + (Math.Abs(u + 0.5f) < 0.14f ? 0.3f : 0f) + (((y / 3) + (int)(u * 6)) % 17 == 0 ? 0.15f : 0f);
                    c.Set(x, y, Ramp(Quartz, light * 0.62f, x, y));
                }

                // O veio do meio, apagado.
                c.Set((int)ColumnX, y, Quartz[1]);
            }

            Throne(c, 168);

            // As cintas de ouro, onde a Coluna fura cada andar.
            foreach (int y in new[] { 168, 176, 318, 326, 470, 478, 600, 608 })
            {
                for (int x = (int)ColumnX - 15; x <= (int)ColumnX + 15; x++)
                    for (int k = 0; k < 4; k++)
                        c.Set(x, y + k, Ramp(Gold, 0.7f - (x - ColumnX) / 30f - k * 0.15f, x, y + k));
                ArtKit.Line(c, (int)ColumnX - 16, y + 4, (int)ColumnX + 16, y + 4, Ink);
            }

            return c;
        }

        private static void Carve(PixelCanvas c, int y0, int y1, Rgba[] wall, float tone, int seed)
        {
            for (int y = y0; y < y1; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    float edge = Math.Min(x, c.Width - 1 - x) / 24f;
                    float n = ArtKit.Noise(x * 0.04f, y * 0.04f, seed);
                    float v = (y - y0) / (float)(y1 - y0);
                    c.Set(x, y, Ramp(wall, tone + n * 0.18f - v * 0.12f - Math.Max(0f, 1f - edge) * 0.25f, x, y));
                }

            for (int x = 0; x < c.Width; x++)
            {
                c.Set(x, y0, Ink);
                c.Set(x, y1, Ink);
            }
        }

        private static void Window(PixelCanvas c, int x, int y, int w, int h, float lit, Rgba glow, int seed)
        {
            bool on = (ArtKit.Hash(x, y, seed) % 100) / 100f < lit;
            for (int py = y; py < y + h; py++)
                for (int px = x; px < x + w; px++)
                    c.Set(px, py, on ? (py == y ? glow.Scaled(1.1f).WithAlpha(255) : glow) : C(18, 18, 28));
            if (on) Light(c, x + w * 0.5f, y + h * 0.5f, Math.Max(w, h) * 2.2f, glow, 0.18f, false, 3f);
        }

        private static void PalaceFloor(PixelCanvas c, int y0, int y1, float lit)
        {
            // O salao: marmore claro, arcadas e o janelao da Coluna no meio.
            for (int y = y0; y < y1; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    float v = (y - y0) / (float)(y1 - y0);
                    c.Set(x, y, Ramp(Marble, 0.38f + v * 0.18f + ArtKit.Noise(x * 0.05f, y * 0.05f, 821) * 0.1f, x, y));
                }

            for (int i = 0; i < 6; i++)
            {
                int ax = 10 + i * 54;
                if (Math.Abs(ax + 22 - ColumnX) < 40) continue;
                for (int y = 40; y < 150; y++)
                    for (int x = ax; x < ax + 44; x++)
                    {
                        float u = (x - ax - 22f) / 22f;
                        float top = 70f - (float)Math.Sqrt(Math.Max(0f, 1f - u * u)) * 26f;
                        if (y < top) continue;
                        c.Set(x, y, Ramp(Marble, 0.12f + (1f - Math.Abs(u)) * 0.12f, x, y));
                    }

                Window(c, ax + 14, 82, 16, 30, lit, Warm, 830 + i);
                for (int y = 30; y < 150; y++) for (int x = ax - 5; x < ax; x++) c.Set(x, y, Ramp(Marble, 0.75f - (x - ax + 5) * 0.08f, x, y));
            }

            // Os candelabros e as faixas vermelhas.
            for (int i = 0; i < 5; i++)
            {
                int bx = 34 + i * 64;
                if (Math.Abs(bx - ColumnX) < 30) continue;
                for (int y = 20; y < 60; y++) for (int x = bx - 6; x < bx + 6; x++) c.Set(x, y, Ramp(Velvet, 0.55f - Math.Abs(x - bx) / 12f, x, y));
                Poly(c, (x, y) => Ramp(Velvet, 0.4f, x, y), (bx - 6f, 60f), (bx + 6f, 60f), (bx, 68f));
            }

        }

        private static void Throne(PixelCanvas c, int y1)
        {
            for (int y = 150; y < y1; y++) for (int x = 0; x < c.Width; x++) c.Set(x, y, Ramp(Marble, 0.72f - (y - 150) * 0.03f, x, y));
            ArtKit.Line(c, 0, 150, c.Width, 150, Marble[4]);

            Poly(c, (x, y) => Ramp(Velvet, 0.5f + (ColumnX - x) / 60f, x, y), (ColumnX - 26f, 150f), (ColumnX + 26f, 150f), (ColumnX + 22f, 96f), (ColumnX - 22f, 96f));
            for (int k = -5; k <= 5; k++)
            {
                float a = -90f + k * 15f;
                (float tx, float ty) = ArtKit.Along(ColumnX, 100f, a, 46f - Math.Abs(k) * 2f);
                Thick(c, ColumnX, 100f, tx, ty, 1.3f, Silver[3]);
                c.Set((int)tx, (int)ty, Gold[4]);
            }
            for (int s = 0; s < 3; s++) c.Fill((int)ColumnX - 34 + s * 4, 150 - s * 4 - 4, 68 - s * 8, 4, Ramp(Marble, 0.8f - s * 0.1f, 0, 0));
        }

    }
}
