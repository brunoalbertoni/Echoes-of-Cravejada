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
    public static partial class HeroArt
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

        /// <summary>Tudo o que muda de um quadro pra outro. Publico pra cenas desenharem poses proprias (IntroFallArt).</summary>
        public sealed class Pose
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

        /// <summary>Um quadro avulso numa pose qualquer, 48x48, pes embaixo no meio.</summary>
        public static PixelCanvas DrawPose(Look look, Pose pose) => Draw(look, pose);

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
    }
}
