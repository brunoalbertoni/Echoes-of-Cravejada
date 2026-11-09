using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Scrap, desenhado por codigo a partir da arte de referencia: um robozinho
    /// redondo, de cabeca de pedra facetada com dois olhos acesos, uma mola no
    /// topo, capa amarrada no pescoco com a barra esfarrapada, corpo de barril
    /// rebitado, bracos de luva redonda, pernas curtas — e um cinzel de cabo
    /// longo, grande demais pra ele, segurado com as duas maos.
    ///
    /// Cada quadro tem 48x48 e olha pra direita, e e importado com o dobro de
    /// pixels por tile do mundo (ver PixelsPerUnit): sai menor que o desenho
    /// antigo e com mais detalhe. Os pes ficam na ultima linha, no meio — e o
    /// pivo.
    ///
    /// Ha dois visuais com os mesmos quadros: a sucata (The Cute Scrap — pedra
    /// bruta, ferrugem e pano cru) e o polido (The Respected Artisan — quartzo,
    /// olhos azuis, bronze e capa de couro).
    ///
    /// Nao depende de UnityEngine, como o resto da arte gerada.
    /// </summary>
    public static class HeroArt
    {
        public const int Size = 48;

        /// <summary>Pixels por tile do Scrap: o dobro do mundo, pra caber detalhe num corpo menor.</summary>
        public const int PixelsPerUnit = 32;

        // A ordem dos quadros. O ScrapAnimator, no jogo, usa os mesmos numeros.
        public const int Idle = 0;
        public const int IdleFrames = 8;
        public const int Walk = 8;
        public const int WalkFrames = 8;
        public const int Jump = 16;
        public const int Apex = 17;
        public const int Fall = 18;
        public const int FallFrames = 2;
        public const int Land = 20;
        public const int Wall = 21;
        public const int Windup = 22;
        public const int WindupFrames = 2;
        public const int Strike = 24;
        public const int StrikeFrames = 3;
        public const int StrikeUp = 27;
        public const int StrikeUpFrames = 2;
        public const int StrikeDown = 29;
        public const int StrikeDownFrames = 2;
        public const int Dash = 31;
        public const int DashFrames = 2;
        public const int Hurt = 33;
        public const int FrameCount = 34;

        /// <summary>As cores de um visual. Cada material tem quatro tons, do escuro pro brilho.</summary>
        public sealed class Look
        {
            public Rgba[] Stone;
            public Rgba Fleck;
            public bool Crystal;

            public Rgba EyeCore;
            public Rgba EyeGlow;
            public Rgba EyeHalo;

            public Rgba[] Metal;
            public Rgba[] Cloth;
            public Rgba Trim;
            public bool Brooch;

            public Rgba[] Spring;
            public Rgba[] Wood;
            public Rgba[] Blade;
            public Rgba BladeSpot;
            public bool RoundEyes;
        }

        /// <summary>The Cute Scrap: pedra bruta, ferrugem e pano cru.</summary>
        public static Look Scrap()
        {
            return new Look
            {
                Stone = new[] { new Rgba(44, 40, 44), new Rgba(78, 72, 72), new Rgba(116, 108, 104), new Rgba(156, 148, 140) },
                Fleck = new Rgba(194, 188, 176),
                EyeCore = new Rgba(255, 252, 232),
                EyeGlow = new Rgba(255, 218, 146),
                EyeHalo = new Rgba(150, 136, 112),
                Metal = new[] { new Rgba(64, 38, 30), new Rgba(112, 66, 44), new Rgba(160, 100, 62), new Rgba(204, 144, 92) },
                Cloth = new[] { new Rgba(74, 70, 62), new Rgba(118, 112, 96), new Rgba(160, 152, 128), new Rgba(196, 188, 160) },
                Trim = new Rgba(96, 90, 76),
                Spring = new[] { new Rgba(96, 56, 38), new Rgba(150, 92, 56), new Rgba(198, 132, 78), new Rgba(226, 170, 110) },
                Wood = new[] { new Rgba(70, 44, 32), new Rgba(110, 72, 46), new Rgba(146, 98, 62), new Rgba(176, 126, 82) },
                Blade = new[] { new Rgba(74, 66, 64), new Rgba(118, 106, 98), new Rgba(160, 148, 136), new Rgba(204, 194, 180) },
                BladeSpot = new Rgba(150, 82, 48),
            };
        }

        /// <summary>The Respected Artisan: quartzo claro, olhos azuis, bronze escovado, couro e prata.</summary>
        public static Look Polished()
        {
            return new Look
            {
                Stone = new[] { new Rgba(112, 132, 160), new Rgba(168, 188, 210), new Rgba(212, 226, 240), new Rgba(244, 250, 255) },
                Fleck = new Rgba(255, 255, 255),
                Crystal = true,
                EyeCore = new Rgba(226, 250, 255),
                EyeGlow = new Rgba(86, 186, 255),
                EyeHalo = new Rgba(150, 204, 246),
                Metal = new[] { new Rgba(88, 60, 38), new Rgba(146, 104, 62), new Rgba(192, 148, 92), new Rgba(234, 198, 136) },
                Cloth = new[] { new Rgba(62, 40, 28), new Rgba(104, 68, 44), new Rgba(140, 96, 62), new Rgba(170, 124, 84) },
                Trim = new Rgba(196, 156, 92),
                Brooch = true,
                Spring = new[] { new Rgba(110, 76, 46), new Rgba(170, 124, 72), new Rgba(214, 168, 100), new Rgba(242, 206, 140) },
                Wood = new[] { new Rgba(70, 44, 32), new Rgba(110, 72, 46), new Rgba(146, 98, 62), new Rgba(176, 126, 82) },
                Blade = new[] { new Rgba(106, 118, 138), new Rgba(164, 176, 194), new Rgba(214, 224, 236), new Rgba(250, 252, 255) },
                BladeSpot = new Rgba(150, 162, 180),
                RoundEyes = true,
            };
        }

        private static readonly Rgba Outline = new Rgba(22, 18, 22);

        /// <summary>Tudo o que muda de um quadro pra outro.</summary>
        private sealed class Pose
        {
            /// <summary>Deslocamento do tronco (positivo: pra frente e pra baixo).</summary>
            public float BodyX;
            public float BodyY;

            /// <summary>Quanto a cabeca vai alem do tronco (inclinacao).</summary>
            public float HeadX;
            public float HeadY;

            /// <summary>Achatamento: menor que 1 achata (aterrissagem), maior estica (pulo).</summary>
            public float Squash = 1f;

            /// <summary>Pes: avanco e quanto sobem do chao.</summary>
            public float BackFootX = -3f;
            public float BackFootLift;
            public float FrontFootX = 3f;
            public float FrontFootLift;

            /// <summary>O cinzel: onde fica a mao da frente (relativo ao centro do tronco) e o angulo da lamina, em graus (0 = pra frente, -90 = pra cima).</summary>
            public float GripX = 6f;
            public float GripY = 2f;
            public float Angle = -62f;

            /// <summary>Se o cinzel passa por tras do corpo.</summary>
            public bool WeaponBehind;

            /// <summary>Rastro do golpe: do angulo inicial ao final (0 = sem rastro).</summary>
            public float SmearFrom;
            public float SmearTo;

            /// <summary>A capa: 0 caida; 1 levantada pelo vento; o sinal diz o lado.</summary>
            public float Flutter;
            public int CapeWave;

            public float SpringSway;
            public float SpringStretch = 1f;

            public bool EyesShut;
            public bool Squint;
        }

        /// <summary>Todos os quadros de um visual, na ordem das constantes la de cima.</summary>
        public static PixelCanvas[] Frames(Look look)
        {
            var frames = new PixelCanvas[FrameCount];

            // Parado: respira, a capa balanca de leve, a mola tambem. Pisca uma vez.
            float[] breath = { 0f, 0f, 0.5f, 1f, 1f, 1f, 0.5f, 0f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(look, new Pose
                {
                    BodyY = breath[i],
                    HeadY = breath[i] * 0.5f,
                    SpringSway = (float)Math.Sin(i * Math.PI / 4.0) * 1.2f,
                    CapeWave = i / 2,
                    GripY = 2f + breath[i],
                    EyesShut = i == 5,
                });
            }

            // Andando: oito quadros de passada, com o tronco descendo no apoio.
            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));

                frames[Walk + i] = Draw(look, new Pose
                {
                    BodyX = 1f,
                    BodyY = 1f - bob,
                    HeadX = 1f,
                    HeadY = 0.5f - bob * 0.5f,
                    BackFootX = -swing * 4.5f,
                    FrontFootX = swing * 4.5f,
                    BackFootLift = swing > 0.2f ? swing * 2.5f : 0f,
                    FrontFootLift = swing < -0.2f ? -swing * 2.5f : 0f,
                    GripY = 2f + (1f - bob),
                    Angle = -60f + swing * 3f,
                    Flutter = 0.35f,
                    CapeWave = i,
                    SpringSway = -1.5f - swing * 0.8f,
                });
            }

            // No ar: subindo encolhe as pernas e a capa cai pra baixo; no alto
            // tudo flutua; caindo a capa sobe e as pernas esticam.
            frames[Jump] = Draw(look, new Pose
            {
                BodyY = -1f, Squash = 1.06f, BackFootX = -2f, FrontFootX = 3f, BackFootLift = 3f, FrontFootLift = 2f,
                Angle = -75f, GripY = 1f, Flutter = -0.4f, SpringSway = 0f, SpringStretch = 0.7f,
            });
            frames[Apex] = Draw(look, new Pose
            {
                BodyY = -1f, BackFootX = -3f, FrontFootX = 3f, BackFootLift = 2f, FrontFootLift = 1f,
                Angle = -55f, GripY = 1f, Flutter = 0.3f, SpringSway = 1f, SpringStretch = 1.1f,
            });
            for (int i = 0; i < FallFrames; i++)
            {
                frames[Fall + i] = Draw(look, new Pose
                {
                    BodyY = -1f, BackFootX = -2.5f, FrontFootX = 3.5f, Angle = -48f, GripY = 0f,
                    Flutter = 0.85f + i * 0.15f, CapeWave = i * 3, SpringSway = 2f - i, SpringStretch = 1.35f,
                });
            }

            frames[Land] = Draw(look, new Pose
            {
                BodyY = 2.5f, HeadY = 1.5f, Squash = 0.86f, BackFootX = -4.5f, FrontFootX = 4.5f,
                Angle = -40f, GripY = 4f, Flutter = -0.2f, SpringSway = 0f, SpringStretch = 0.6f, Squint = true,
            });

            // Na parede: encostado pra frente, os pes no muro, a capa caida pra tras.
            frames[Wall] = Draw(look, new Pose
            {
                BodyX = 2f, HeadX = 2f, BackFootX = 2f, BackFootLift = 3f, FrontFootX = 5f, FrontFootLift = 6f,
                Angle = -100f, GripX = 2f, GripY = 0f, WeaponBehind = true, Flutter = 0.5f, SpringSway = -2f, Squint = true,
            });

            // O golpe de lado: ergue o cinzel por cima da cabeca, desce num arco
            // com rastro e acompanha o movimento.
            frames[Windup + 0] = Draw(look, new Pose
            {
                BodyX = -1f, HeadX = -1f, Angle = -110f, GripX = 3f, GripY = -1f, WeaponBehind = true,
                BackFootX = -4f, FrontFootX = 3f, SpringSway = 1.5f, Flutter = 0.2f,
            });
            frames[Windup + 1] = Draw(look, new Pose
            {
                BodyX = -2f, HeadX = -1.5f, BodyY = 0.5f, Angle = -145f, GripX = 1f, GripY = -2f, WeaponBehind = true,
                BackFootX = -5f, FrontFootX = 3f, SpringSway = 2.5f, Flutter = 0.3f, Squint = true,
            });
            frames[Strike + 0] = Draw(look, new Pose
            {
                BodyX = 1f, HeadX = 1f, Angle = -75f, GripX = 6f, GripY = -2f,
                BackFootX = -5f, FrontFootX = 4f, SmearFrom = -150f, SmearTo = -75f, SpringSway = 0f, Flutter = 0.4f, Squint = true,
            });
            frames[Strike + 1] = Draw(look, new Pose
            {
                BodyX = 3f, HeadX = 2f, BodyY = 1f, Angle = -5f, GripX = 8f, GripY = 1f,
                BackFootX = -6f, FrontFootX = 5f, SmearFrom = -120f, SmearTo = -5f, SpringSway = -2.5f, Flutter = 0.6f, Squint = true,
            });
            frames[Strike + 2] = Draw(look, new Pose
            {
                BodyX = 3f, HeadX = 2f, BodyY = 1.5f, Angle = 25f, GripX = 8f, GripY = 3f,
                BackFootX = -6f, FrontFootX = 5f, SmearFrom = -30f, SmearTo = 25f, SpringSway = -3f, Flutter = 0.7f,
            });

            // Pra cima: o cinzel sobe reto, por cima da cabeca.
            frames[StrikeUp + 0] = Draw(look, new Pose
            {
                BodyY = 0.5f, Squash = 0.95f, Angle = -70f, GripX = 4f, GripY = 0f, BackFootX = -3f, FrontFootX = 3f,
                SpringSway = 1f, Squint = true,
            });
            frames[StrikeUp + 1] = Draw(look, new Pose
            {
                BodyY = -1f, Squash = 1.06f, Angle = -92f, GripX = 3f, GripY = -6f, BackFootX = -3f, FrontFootX = 3f,
                SmearFrom = -60f, SmearTo = -92f, SpringSway = -1.5f, SpringStretch = 0.7f, Squint = true,
            });

            // Pra baixo, no ar: o cinzel desce entre os pes, pra quicar.
            frames[StrikeDown + 0] = Draw(look, new Pose
            {
                BodyY = -1f, Angle = 60f, GripX = 4f, GripY = 4f, BackFootX = -4f, FrontFootX = 4f, BackFootLift = 2f, FrontFootLift = 2f,
                Flutter = 0.8f, SpringSway = 1f, SpringStretch = 1.3f, Squint = true,
            });
            frames[StrikeDown + 1] = Draw(look, new Pose
            {
                BodyY = -2f, Angle = 90f, GripX = 1f, GripY = 6f, BackFootX = -5f, FrontFootX = 5f, BackFootLift = 3f, FrontFootLift = 3f,
                SmearFrom = 50f, SmearTo = 90f, Flutter = 1f, SpringSway = 0f, SpringStretch = 1.4f, Squint = true,
            });

            // O dash: deitado pra frente, a capa esticada pra tras e o cinzel arrastando.
            for (int i = 0; i < DashFrames; i++)
            {
                frames[Dash + i] = Draw(look, new Pose
                {
                    BodyX = 3f, HeadX = 3f, BodyY = 1f, Squash = 0.94f, BackFootX = -6f, BackFootLift = 2f, FrontFootX = 4f, FrontFootLift = 1f,
                    Angle = 170f, GripX = 0f, GripY = 3f, WeaponBehind = true, Flutter = 1.3f, CapeWave = i * 2,
                    SpringSway = -3.5f, SpringStretch = 0.9f, Squint = true,
                });
            }

            frames[Hurt] = Draw(look, new Pose
            {
                BodyX = -2f, HeadX = -2f, BodyY = 0.5f, BackFootX = -4f, FrontFootX = 2f, FrontFootLift = 2f,
                Angle = -20f, GripX = 5f, GripY = 2f, Flutter = 0.6f, SpringSway = 3f, EyesShut = true,
            });

            return frames;
        }

        // ------------------------------------------------------------------
        // Montagem de um quadro
        // ------------------------------------------------------------------

        // Medidas, antes da pose. O centro do tronco e da cabeca, e os raios.
        private const float CenterX = 23.5f;
        private const float BodyCY = 37.5f;
        private const float BodyRX = 7.4f;
        private const float BodyRY = 6.4f;
        private const float HeadCY = 23.5f;
        private const float HeadRX = 10.2f;
        private const float HeadRY = 9.2f;
        private const float FootY = 46f;

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Size, Size);
            public Look Look;
            public Pose Pose;

            public float BodyX => CenterX + Pose.BodyX;
            public float BodyY => BodyCY + Pose.BodyY;
            public float HeadX => CenterX + Pose.BodyX + Pose.HeadX + 0.5f;
            public float HeadY => HeadCY + Pose.BodyY + Pose.HeadY + (1f - Pose.Squash) * 6f;

            public float GripX => BodyX + Pose.GripX;
            public float GripY => BodyY + Pose.GripY;
        }

        private static PixelCanvas Draw(Look look, Pose pose)
        {
            var f = new Frame { Look = look, Pose = pose };

            DrawCapeBack(f);
            if (pose.WeaponBehind) DrawWeapon(f);
            DrawBackArm(f);
            DrawLeg(f, pose.BackFootX, pose.BackFootLift, true);
            DrawLeg(f, pose.FrontFootX, pose.FrontFootLift, false);
            DrawBody(f);
            DrawCapeFront(f);
            DrawHead(f);
            DrawSpring(f);
            DrawScarf(f);
            if (!pose.WeaponBehind) DrawWeapon(f);
            DrawFrontArm(f);
            AddOutline(f.Canvas);
            DrawSmear(f);

            return f.Canvas;
        }

        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        /// <summary>Um tom de uma rampa de quatro, pela luz (de -1 a 1; a luz vem de cima e da esquerda).</summary>
        private static Rgba Tone(Rgba[] ramp, float light)
        {
            if (light > 0.62f) return ramp[3];
            if (light > 0.18f) return ramp[2];
            if (light > -0.32f) return ramp[1];
            return ramp[0];
        }

        /// <summary>Uma elipse sombreada como uma esfera, com a luz de cima e da esquerda.</summary>
        private static void Ball(PixelCanvas canvas, float cx, float cy, float rx, float ry, Rgba[] ramp, float bias = 0f)
        {
            for (int py = (int)(cy - ry) - 1; py <= (int)(cy + ry) + 1; py++)
            {
                for (int px = (int)(cx - rx) - 1; px <= (int)(cx + rx) + 1; px++)
                {
                    float dx = (px + 0.5f - cx) / rx;
                    float dy = (py + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    float nz = (float)Math.Sqrt(1f - d);
                    float light = -0.55f * dx - 0.65f * dy + 0.5f * nz - 0.25f + bias;
                    canvas.Set(px, py, Tone(ramp, light));
                }
            }
        }

        /// <summary>Uma faixa grossa de um ponto a outro, sombreada como um cilindro.</summary>
        private static void Limb(PixelCanvas canvas, float x0, float y0, float x1, float y1, float half, Rgba[] ramp, float bias = 0f)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.01f)
            {
                Ball(canvas, x0, y0, half, half, ramp, bias);
                return;
            }

            float nx = -dy / length;
            float ny = dx / length;

            int minX = (int)Math.Floor(Math.Min(x0, x1) - half - 1);
            int maxX = (int)Math.Ceiling(Math.Max(x0, x1) + half + 1);
            int minY = (int)Math.Floor(Math.Min(y0, y1) - half - 1);
            int maxY = (int)Math.Ceiling(Math.Max(y0, y1) + half + 1);

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float rx = px + 0.5f - x0;
                    float ry = py + 0.5f - y0;
                    float along = (rx * dx + ry * dy) / (length * length);
                    if (along < -0.02f || along > 1.02f) continue;

                    float across = (rx * nx + ry * ny) / half;
                    if (Math.Abs(across) > 1f) continue;

                    // O lado virado pra cima/esquerda pega luz.
                    float facing = -(nx * 0.55f + ny * 0.65f);
                    float light = across * facing * 1.2f + 0.1f * (1f - Math.Abs(across)) + bias;
                    canvas.Set(px, py, Tone(ramp, light));
                }
            }
        }

        // ------------------------------------------------------------------
        // A capa
        // ------------------------------------------------------------------

        /// <summary>
        /// A parte de tras da capa: um sino de pano que desce do pescoco e
        /// passa por tras do corpo. Com vento (Flutter) a barra vai pra tras e
        /// sobe; a barra tem dentes esfarrapados que mudam com CapeWave.
        /// </summary>
        private static void DrawCapeBack(Frame f)
        {
            Pose pose = f.Pose;
            Rgba[] cloth = f.Look.Cloth;

            float neckX = f.BodyX - 0.5f;
            float neckY = f.BodyY - 6.5f;
            float hemY = FootY - 3f + Math.Min(0f, -pose.Flutter * 5f);
            float back = 8f + Math.Max(0f, pose.Flutter) * 8f;

            for (int py = (int)neckY; py <= (int)hemY + 2; py++)
            {
                float t = Math.Max(0f, Math.Min(1f, (py - neckY) / Math.Max(1f, hemY - neckY)));

                // A capa abre como um sino, e com vento vai mais pra tras embaixo.
                float left = neckX - 3f - back * (float)Math.Pow(t, 0.75);
                float right = neckX + 2f - t * 3f - Math.Max(0f, pose.Flutter) * t * 3f;

                for (int px = (int)left; px <= (int)right; px++)
                {
                    // Barra esfarrapada.
                    int tooth = Hash(px + pose.CapeWave * 3, 1, 9) % 4;
                    if (py > hemY - tooth + 1) continue;

                    float u = (px + 0.5f - left) / Math.Max(1f, right - left);
                    float fold = (float)Math.Sin((px + pose.CapeWave) * 0.9f) * 0.25f;
                    float light = (u - 0.6f) * 0.6f - t * 0.4f + fold - 0.1f;
                    f.Canvas.Set(px, py, Tone(cloth, light));
                }
            }
        }

        /// <summary>As abas da capa por cima dos ombros, caindo dos lados do corpo, e o broche.</summary>
        private static void DrawCapeFront(Frame f)
        {
            Rgba[] cloth = f.Look.Cloth;
            float cx = f.BodyX;
            float top = f.BodyY - 6.5f;

            // Aba de tras (a do ombro esquerdo): larga, cobre o lado do corpo.
            for (int py = (int)top; py <= (int)(f.BodyY + 1f); py++)
            {
                float t = (py - top) / 8f;
                int left = (int)(cx - 8f - t * 1.0f);
                int right = (int)(cx - 4f - t * 2.5f);

                for (int px = left; px <= right; px++)
                {
                    if (py > f.BodyY + 2f && Hash(px, py, 4) % 3 == 0) continue;
                    float light = (px - left) / 5f * 0.5f - 0.3f + (py == (int)top ? 0.5f : 0f);
                    f.Canvas.Set(px, py, Tone(cloth, light));
                }
            }

            // Aba da frente: so a beirada, no ombro direito.
            for (int py = (int)top; py <= (int)(f.BodyY - 1f); py++)
            {
                float t = (py - top) / 6f;
                int left = (int)(cx + 3f + t * 2f);
                int right = (int)(cx + 6f + t * 1.2f);

                for (int px = left; px <= right; px++)
                {
                    float light = 0.35f - (px - left) * 0.2f;
                    f.Canvas.Set(px, py, Tone(cloth, light));
                }
            }

            // A barra decorada do couro (so no polido).
            if (f.Look.Brooch)
            {
                for (int px = (int)(cx - 8f); px <= (int)(cx - 4f); px++) f.Canvas.Set(px, (int)(f.BodyY + 2f), f.Look.Trim);
            }
        }

        /// <summary>O no do lenco no pescoco, com as duas pontas caindo — ou o broche de engrenagem.</summary>
        private static void DrawScarf(Frame f)
        {
            Rgba[] cloth = f.Look.Cloth;
            int x = (int)(f.BodyX + 2f);
            int y = (int)(f.BodyY - 6f);

            // A faixa do pescoco.
            for (int px = x - 6; px <= x + 3; px++)
            {
                f.Canvas.Set(px, y, Tone(cloth, 0.4f - (x - px) * 0.05f));
                f.Canvas.Set(px, y + 1, Tone(cloth, -0.1f - (x - px) * 0.05f));
            }

            if (f.Look.Brooch)
            {
                // O broche redondo de metal, como uma engrenagem pequena.
                Ball(f.Canvas, x + 1.5f, y + 1.5f, 2.2f, 2.2f, f.Look.Metal, 0.2f);
                f.Canvas.Set(x + 1, y + 1, f.Look.Metal[0]);
                f.Canvas.Set(x + 1, y - 1, f.Look.Metal[2]);
                f.Canvas.Set(x + 4, y + 1, f.Look.Metal[1]);
                f.Canvas.Set(x - 1, y + 2, f.Look.Metal[1]);
                return;
            }

            // O no e as pontas.
            f.Canvas.Fill(x, y, 3, 3, cloth[2]);
            f.Canvas.Set(x + 1, y + 1, cloth[3]);
            f.Canvas.Set(x, y + 2, cloth[1]);
            for (int i = 0; i < 4; i++)
            {
                f.Canvas.Set(x - 1 + i / 2, y + 3 + i, i < 3 ? cloth[2] : cloth[1]);
                f.Canvas.Set(x + 2 + i / 3, y + 3 + i, i < 2 ? cloth[2] : cloth[1]);
            }

            f.Canvas.Set(x - 1, y + 7, cloth[0]);
            f.Canvas.Set(x + 4, y + 6, cloth[0]);
        }

        // ------------------------------------------------------------------
        // O corpo
        // ------------------------------------------------------------------

        private static void DrawLeg(Frame f, float footX, float lift, bool back)
        {
            Rgba[] metal = f.Look.Metal;
            float hipX = f.BodyX + (back ? -2.5f : 2.5f);
            float hipY = f.BodyY + 4f;
            float fx = CenterX + footX + f.Pose.BodyX * 0.3f;
            float fy = FootY - 1f - lift;

            float bias = back ? -0.45f : 0f;
            Limb(f.Canvas, hipX, hipY, fx, fy - 1f, 1.7f, metal, bias);

            // O pe: uma bota redonda, virada pra frente.
            Ball(f.Canvas, fx + 1f, fy, 3f, 1.9f, metal, bias - 0.05f);
            f.Canvas.Fill((int)(fx - 1f), (int)(fy + 1f), 5, 1, metal[0]);
        }

        /// <summary>O tronco: um barril de chapa, com cinta rebitada e uma placa no peito.</summary>
        private static void DrawBody(Frame f)
        {
            Rgba[] metal = f.Look.Metal;
            float cx = f.BodyX;
            float cy = f.BodyY;
            float rx = BodyRX / (float)Math.Sqrt(f.Pose.Squash);
            float ry = BodyRY * f.Pose.Squash;

            Ball(f.Canvas, cx, cy, rx, ry, metal, 0.05f);

            // A cinta, com rebites.
            int belt = (int)(cy + 2f);
            for (int px = (int)(cx - rx) + 1; px <= (int)(cx + rx) - 1; px++)
            {
                float dx = (px + 0.5f - cx) / rx;
                if (dx * dx > 0.9f) continue;

                f.Canvas.Set(px, belt, metal[0]);
                f.Canvas.Set(px, belt + 1, (px & 1) == 0 ? metal[2] : metal[1]);
            }

            // A placa do peito e o rebite do meio.
            for (int py = (int)(cy - 3f); py <= (int)(cy); py++)
            {
                f.Canvas.Set((int)(cx + 2f), py, metal[0]);
            }

            f.Canvas.Set((int)(cx + 4f), (int)(cy - 2f), metal[3]);
            f.Canvas.Set((int)(cx - 1f), (int)(cy - 3f), metal[3]);
            f.Canvas.Set((int)(cx + 4f), (int)(cy - 1f), metal[1]);

            // Ferrugem ou risco de uso.
            if (!f.Look.Crystal)
            {
                f.Canvas.Set((int)(cx - 3f), (int)(cy + 4f), new Rgba(76, 112, 92));
                f.Canvas.Set((int)(cx - 2f), (int)(cy + 4f), new Rgba(64, 96, 80));
            }
        }

        // ------------------------------------------------------------------
        // A cabeca
        // ------------------------------------------------------------------

        /// <summary>
        /// A cabeca: uma pedra bruta (ou um quartzo), facetada. Cada pixel pega
        /// o tom da faceta mais proxima, conforme ela olha pra luz; onde duas
        /// facetas se encontram fica um vinco. No quartzo, um brilho interno e
        /// faiscas.
        /// </summary>
        private static void DrawHead(Frame f)
        {
            Look look = f.Look;
            float cx = f.HeadX;
            float cy = f.HeadY;
            float rx = HeadRX;
            float ry = HeadRY * (0.5f + 0.5f * f.Pose.Squash);

            // Centros das facetas (relativos ao centro) e o "lado" que cada uma olha.
            float[,] facets =
            {
                { -5.5f, -5.0f, 0.9f }, { 0.5f, -6.5f, 0.75f }, { 6.0f, -4.0f, 0.35f },
                { -8.0f, 0.5f, 0.3f }, { -2.5f, -1.0f, 0.55f }, { 3.5f, -0.5f, 0.2f },
                { 8.0f, 1.5f, -0.05f }, { -5.0f, 5.5f, -0.1f }, { 1.0f, 5.0f, -0.15f }, { 6.5f, 6.0f, -0.35f },
            };

            for (int py = (int)(cy - ry) - 2; py <= (int)(cy + ry) + 2; py++)
            {
                for (int px = (int)(cx - rx) - 2; px <= (int)(cx + rx) + 2; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;

                    // Contorno de pedra: o raio varia com o angulo, achatado embaixo.
                    double angle = Math.Atan2(dy, dx);
                    float bump = 1f + 0.06f * (float)Math.Sin(angle * 5.0 + 1.1) + 0.04f * (float)Math.Sin(angle * 9.0 + 0.4);
                    float ex = dx / (rx * bump);
                    float ey = dy / (ry * bump * (dy > 0 ? 0.94f : 1f));
                    float d = ex * ex + ey * ey;
                    if (d > 1f) continue;

                    float nearest = 999f;
                    float second = 999f;
                    float facing = 0f;

                    for (int i = 0; i < facets.GetLength(0); i++)
                    {
                        float fx = dx - facets[i, 0];
                        float fy = dy - facets[i, 1];
                        float distance = fx * fx + fy * fy;

                        if (distance < nearest)
                        {
                            second = nearest;
                            nearest = distance;
                            facing = facets[i, 2];
                        }
                        else if (distance < second)
                        {
                            second = distance;
                        }
                    }

                    // A borda escurece (volume), as facetas dao o tom.
                    float light = facing * 1.25f - d * 0.4f + 0.05f;
                    Rgba color = Tone(look.Stone, light);

                    // Vinco entre facetas: um tom abaixo, nao preto.
                    bool crease = Math.Sqrt(second) - Math.Sqrt(nearest) < 0.6;
                    if (crease)
                    {
                        int index = Array.IndexOf(look.Stone, color);
                        color = look.Stone[Math.Max(0, index - 1)];
                    }

                    // A borda de baixo e de tras fica na sombra.
                    if (d > 0.9f && (dy > 4f || dx < -7f)) color = look.Stone[0];

                    int hx = (int)Math.Floor(dx + 16f);
                    int hy = (int)Math.Floor(dy + 16f);

                    if (look.Crystal)
                    {
                        // Brilho interno: o miolo do quartzo e mais claro.
                        if (d < 0.35f && !crease && Hash(hx, hy, 3) % 5 == 0) color = look.Stone[3];
                        if (Hash(hx, hy, 21) % 41 == 0) color = look.Fleck;
                    }
                    else
                    {
                        // Lasquinhas e grao da pedra.
                        if (Hash(hx, hy, 21) % 19 == 0) color = look.Fleck;
                        else if (Hash(hx, hy, 22) % 11 == 0) color = look.Stone[Math.Max(0, Array.IndexOf(look.Stone, color) - 1)];
                    }

                    f.Canvas.Set(px, py, color);
                }
            }

            // O brilho grande do quartzo, no alto a esquerda.
            if (look.Crystal)
            {
                f.Canvas.Set((int)(cx - 5f), (int)(cy - 6f), look.Fleck);
                f.Canvas.Set((int)(cx - 4f), (int)(cy - 6f), look.Fleck);
                f.Canvas.Set((int)(cx - 5f), (int)(cy - 5f), look.Fleck);
                f.Canvas.Set((int)(cx - 6f), (int)(cy - 4f), look.Stone[3]);
            }

            DrawEyes(f, cx, cy);
        }

        /// <summary>
        /// Os olhos, virados pra frente (pra direita). Na sucata, um redondo e
        /// um meio fechado — a cara de teimoso da referencia. No polido, os dois
        /// redondos e azuis. Em volta, um halo aceso na pedra.
        /// </summary>
        private static void DrawEyes(Frame f, float cx, float cy)
        {
            Look look = f.Look;
            Pose pose = f.Pose;

            int y = (int)(cy + 0.5f);
            int x0 = (int)(cx - 2.5f);
            int x1 = (int)(cx + 4.5f);

            // O de tras e redondo; o da frente, na sucata, meio fechado (a cara
            // de teimoso). No polido, os dois redondos.
            int open0 = pose.EyesShut ? 0 : (pose.Squint ? 3 : 5);
            int open1 = pose.EyesShut ? 0 : (pose.Squint || !look.RoundEyes ? 3 : 5);

            DrawEye(f, x0, y, open0, false);
            DrawEye(f, x1, y, open1, !look.RoundEyes || pose.Squint);
        }

        /// <summary>
        /// Um olho aceso: um oval de 4 de largura e h de altura, com miolo
        /// claro, borda de brilho e um halo na pedra em volta. Lidded corta o
        /// alto em diagonal, como uma palpebra caida. h 0 = fechado.
        /// </summary>
        private static void DrawEye(Frame f, int x, int y, int h, bool lidded)
        {
            Look look = f.Look;
            const int w = 4;

            // O halo, na pedra em volta.
            for (int py = y - 5; py <= y + 5; py++)
            {
                for (int px = x - 3; px <= x + w + 2; px++)
                {
                    float dx = (px + 0.5f - (x + w * 0.5f)) / 4.2f;
                    float dy = (py + 0.5f - (y + 0.5f)) / 4.4f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    Rgba under = f.Canvas.Get(px, py);
                    if (under.A == 0) continue;
                    f.Canvas.Set(px, py, Rgba.Lerp(under, look.EyeHalo, 0.5f * (1f - d)));
                }
            }

            if (h == 0)
            {
                f.Canvas.Fill(x - 1, y, w + 2, 1, look.Stone[0]);
                f.Canvas.Fill(x, y, w, 1, look.EyeGlow);
                return;
            }

            float cx = x + w * 0.5f;
            float cy = y + 0.5f + (lidded ? 0.5f : 0f);
            float ry = h * 0.5f;

            // Um contorno escuro fino, so em volta do oval.
            for (int py = y - 4; py <= y + 4; py++)
            {
                for (int px = x - 1; px <= x + w; px++)
                {
                    float dx = (px + 0.5f - cx) / (w * 0.5f + 0.9f);
                    float dy = (py + 0.5f - cy) / (ry + 0.9f);
                    if (dx * dx + dy * dy <= 1f) f.Canvas.Set(px, py, look.Stone[0]);
                }
            }

            for (int py = y - 4; py <= y + 4; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    float dx = (px + 0.5f - cx) / (w * 0.5f);
                    float dy = (py + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1.05f) continue;

                    // A palpebra: corta o alto do olho, mais do lado de dentro.
                    if (lidded && py < cy - ry * 0.1f + (px - x) * -0.35f) continue;

                    f.Canvas.Set(px, py, d < 0.45f ? look.EyeCore : look.EyeGlow);
                }
            }

            // O reflexo.
            if (!lidded) f.Canvas.Set(x + 1, (int)(cy - ry + 1f), look.EyeCore);
        }

        /// <summary>A mola: um espiral de quatro voltas, saindo do alto da cabeca, a direita.</summary>
        private static void DrawSpring(Frame f)
        {
            Rgba[] ramp = f.Look.Spring;
            float baseX = f.HeadX + 3.5f;
            float baseY = f.HeadY - HeadRY + 1.5f;
            float height = 9f * f.Pose.SpringStretch;
            const int turns = 4;

            int steps = 60;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                double a = t * turns * Math.PI * 2.0;

                // A ponta balanca mais que a base.
                float sway = f.Pose.SpringSway * t * t;
                float px = baseX + (float)Math.Cos(a) * 1.8f + sway;
                float py = baseY - t * height;

                bool front = Math.Sin(a) > 0;
                Rgba color = front ? (Math.Cos(a) < 0 ? ramp[3] : ramp[2]) : ramp[0];
                f.Canvas.Set((int)px, (int)py, color);
            }

            // O pino que prende a mola na pedra.
            f.Canvas.Set((int)baseX, (int)baseY + 1, ramp[1]);
            f.Canvas.Set((int)baseX + 1, (int)baseY + 1, ramp[0]);
        }

        // ------------------------------------------------------------------
        // Os bracos e o cinzel
        // ------------------------------------------------------------------

        private static (float X, float Y) Along(float x, float y, float angle, float distance)
        {
            double a = angle * Math.PI / 180.0;
            return (x + (float)Math.Cos(a) * distance, y + (float)Math.Sin(a) * distance);
        }

        /// <summary>
        /// O cinzel: o cabo de madeira, com a mao da frente em Grip e a de tras
        /// mais abaixo no cabo; a lamina larga, de ponta reta e chanfrada.
        /// </summary>
        private static void DrawWeapon(Frame f)
        {
            Look look = f.Look;
            float angle = f.Pose.Angle;

            (float bx, float by) = Along(f.GripX, f.GripY, angle, -9f);
            (float nx, float ny) = Along(f.GripX, f.GripY, angle, 11f);
            (float tx, float ty) = Along(f.GripX, f.GripY, angle, 21.5f);

            Limb(f.Canvas, bx, by, nx, ny, 1.05f, look.Wood);

            // A virola de metal entre o cabo e a lamina.
            (float fx, float fy) = Along(f.GripX, f.GripY, angle, 11.5f);
            Ball(f.Canvas, fx, fy, 1.8f, 1.8f, look.Metal, 0.1f);

            // A lamina: alarga da virola ate a ponta.
            float dx = tx - nx;
            float dy = ty - ny;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / length;
            float uy = dy / length;
            float perpX = -uy;
            float perpY = ux;

            for (int py = (int)Math.Min(ny, ty) - 6; py <= (int)Math.Max(ny, ty) + 6; py++)
            {
                for (int px = (int)Math.Min(nx, tx) - 6; px <= (int)Math.Max(nx, tx) + 6; px++)
                {
                    float rx = px + 0.5f - nx;
                    float ry = py + 0.5f - ny;
                    float along = (rx * ux + ry * uy) / length;
                    if (along < 0f || along > 1f) continue;

                    float across = rx * perpX + ry * perpY;
                    float half = 2.6f + along * 1.0f;
                    if (Math.Abs(across) > half) continue;

                    // Fio da lamina claro, costas escuras, chanfro na ponta.
                    float u = across / half;
                    float light = (perpY < 0 ? u : -u) * 0.7f + 0.1f;
                    if (along > 0.84f) light += 0.5f;

                    // A lombada: uma linha escura no meio da lamina.
                    if (Math.Abs(u) < 0.18f && along < 0.8f) light -= 0.6f;
                    Rgba color = Tone(look.Blade, light);

                    int hx = (int)Math.Floor(along * 10f);
                    int hy = (int)Math.Floor(u * 3f + 3f);
                    if (Hash(hx, hy, 31) % 7 == 0) color = look.BladeSpot;

                    f.Canvas.Set(px, py, color);
                }
            }

            // A mao de tras, no cabo.
            (float hx2, float hy2) = Along(f.GripX, f.GripY, angle, -5.5f);
            Ball(f.Canvas, hx2, hy2, 2.1f, 2.1f, look.Metal, -0.35f);
        }

        /// <summary>O braco de tras: do ombro ate a mao de tras, no cabo.</summary>
        private static void DrawBackArm(Frame f)
        {
            (float hx, float hy) = Along(f.GripX, f.GripY, f.Pose.Angle, -5.5f);
            Limb(f.Canvas, f.BodyX - 3f, f.BodyY - 3f, hx, hy, 1.4f, f.Look.Metal, -0.45f);
        }

        /// <summary>O braco da frente e a luva, por cima do cabo.</summary>
        private static void DrawFrontArm(Frame f)
        {
            Rgba[] metal = f.Look.Metal;
            float sx = f.BodyX + 4.5f;
            float sy = f.BodyY - 3f;

            Ball(f.Canvas, sx, sy, 2.2f, 2.2f, metal, 0.1f);
            Limb(f.Canvas, sx, sy, f.GripX, f.GripY, 1.5f, metal);
            Ball(f.Canvas, f.GripX, f.GripY, 2.4f, 2.4f, metal, 0.15f);

            // Os dedos fechados no cabo.
            f.Canvas.Set((int)(f.GripX + 1f), (int)(f.GripY + 1f), metal[0]);
            f.Canvas.Set((int)(f.GripX), (int)(f.GripY + 1f), metal[1]);
        }

        /// <summary>O rastro do golpe: um arco claro e translucido, do angulo inicial ao final.</summary>
        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;

            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);
            bool newerAtHigh = pose.SmearTo > pose.SmearFrom;

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    float dx = px + 0.5f - f.GripX;
                    float dy = py + 0.5f - f.GripY;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 10f || r > 21f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    // Mais forte perto da lamina e na ponta; some no comeco do arco.
                    float k = (a - from) / Math.Max(1f, to - from);
                    if (!newerAtHigh) k = 1f - k;
                    float edge = (r - 10f) / 11f;
                    int alpha = (int)(40 + 150 * k * edge);
                    if (alpha < 30) continue;

                    Rgba glow = Rgba.Lerp(f.Look.Blade[3], new Rgba(255, 255, 255), 0.5f).WithAlpha(alpha);
                    f.Canvas.Blend(px, py, glow);
                }
            }
        }

        /// <summary>
        /// Contorna tudo de escuro. E o que faz o sprite ler bem em cima de
        /// qualquer fundo — e o fundo deste jogo e quase preto.
        /// </summary>
        private static void AddOutline(PixelCanvas canvas)
        {
            var filled = new bool[Size * Size];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A > 128;

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    if (filled[px + py * Size]) continue;

                    bool touches = (px > 0 && filled[px - 1 + py * Size])
                        || (px < Size - 1 && filled[px + 1 + py * Size])
                        || (py > 0 && filled[px + (py - 1) * Size])
                        || (py < Size - 1 && filled[px + (py + 1) * Size]);

                    if (touches) canvas.Set(px, py, Outline);
                }
            }
        }
    }
}
