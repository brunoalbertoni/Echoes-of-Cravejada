using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Onde fica cada peca da cenografia do District 1 (ver DecorArt).
    ///
    /// Coordenadas em tiles, do canto de baixo a esquerda do sprite (menos as
    /// engrenagens, que contam do eixo). Cada trecho do mapa conta uma parte
    /// da historia:
    ///
    ///   O POCO        — a Calha N.º 1 saindo do teto em cima do monte onde o
    ///                   Scrap acorda, as vigas e canos da obra da Coroa, os
    ///                   postes de lampiao da Avenida que desceram junto.
    ///   O ANDAR 1     — a oficina do Rebite: o cano do teto, os ganchos em
    ///                   cima dos espinhos.
    ///   O ANDAR 2 E A TRIAGEM — o alto do distrito, quase deserto: so a
    ///                   esteira parada e o
    ///                   que restou dos ganchos do Coletor.
    ///   A ANTESSALA   — o canto quente da bancada e o corredor dos que nao
    ///                   chegaram a ela.
    ///   A ARENA       — a toca do Coletor: as gaiolas, a contagem riscada na
    ///                   parede, as marcas de garra. Nada em volta das
    ///                   plataformas, pra luta ficar legivel.
    ///   O ABISMO      — os cabos partidos por cima do vao.
    ///   A PISTA       — a obra da Coroa, abandonada: nada alem da placa.
    ///
    /// Os canos sempre vao de rocha a rocha (ou ate uma curva que segue em
    /// frente): as pontas ficam enterradas no terreno, que e desenhado por
    /// cima. O Validate confere.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class Phase01Decor
    {
        public enum Layer
        {
            /// <summary>Presa na parede do fundo: tingida junto com ela.</summary>
            Wall,

            /// <summary>No chao, atras do terreno e do Scrap, um pouco esmaecida.</summary>
            Back,

            /// <summary>No chao, na frente do terreno, atras do Scrap.</summary>
            Middle,

            /// <summary>Na frente do Scrap, quase em silhueta.</summary>
            Front,
        }

        public enum Motion
        {
            None,

            /// <summary>Gira devagar em volta do eixo (as engrenagens da parede). Speed em graus por segundo.</summary>
            Spin,
        }

        public readonly struct Piece
        {
            public readonly string Sprite;
            public readonly float X;
            public readonly float Y;
            public readonly Layer Layer;
            public readonly bool Flip;

            /// <summary>Pras pecas que repetem: o tamanho, em tiles. Zero: o sprite do tamanho dele.</summary>
            public readonly float Width;
            public readonly float Height;

            public readonly Motion Motion;
            public readonly float Speed;

            /// <summary>E Oxide (poca, respiro): acende verde e some com a purificacao.</summary>
            public readonly bool Oxide;

            public Piece(string sprite, float x, float y, Layer layer, bool flip = false, float width = 0f, float height = 0f,
                Motion motion = Motion.None, float speed = 0f, bool oxide = false)
            {
                Sprite = sprite;
                X = x;
                Y = y;
                Layer = layer;
                Flip = flip;
                Width = width;
                Height = height;
                Motion = motion;
                Speed = speed;
                Oxide = oxide;
            }
        }

        /// <summary>
        /// Uma corrente pendurada do teto, com o que tiver na ponta (gancho,
        /// gaiola ou nada). Balanca devagar em volta do ponto de cima.
        /// </summary>
        public readonly struct Hanging
        {
            public readonly float X;
            public readonly float Top;
            public readonly float Length;
            public readonly string End;
            public readonly bool Front;

            public Hanging(float x, float top, float length, string end, bool front = false)
            {
                X = x;
                Top = top;
                Length = length;
                End = end;
                Front = front;
            }
        }

        /// <summary>
        /// Um poste de lampiao de cristal. Apagado, acende com a purificacao;
        /// os das bancadas ja estao acesos (sao a luz quente delas).
        /// </summary>
        public readonly struct Lamp
        {
            public readonly float X;
            public readonly float Y;
            public readonly bool AlwaysLit;

            public Lamp(float x, float y, bool alwaysLit = false)
            {
                X = x;
                Y = y;
                AlwaysLit = alwaysLit;
            }

            /// <summary>Onde fica o cristal, pra luz.</summary>
            public (float X, float Y) Glass => (X + 0.6f, Y + 2.3f);
        }

        public static readonly Piece[] Pieces = BuildPieces();

        public static readonly Hanging[] Hangings =
        {
            // Poco: um gancho perto da calha e uma gaiola em cima da subida.
            new Hanging(16.5f, 28f, 4f, DecorArt.Hook),
            new Hanging(28.8f, 28f, 4.5f, DecorArt.Cage),

            // Andar 1: ganchos em cima dos dois fossos de espinhos.
            new Hanging(18.5f, 46f, 2.5f, DecorArt.Hook),
            new Hanging(27.5f, 46f, 1.5f, DecorArt.Hook),

            // A Triagem: o que restou da fileira de ganchos do Coletor. O alto
            // do distrito e quase deserto de proposito.
            new Hanging(61.5f, 56f, 3f, DecorArt.Hook),
            new Hanging(66.5f, 56f, 2f, DecorArt.Cage),

            // A Antessala: em cima do fosso.
            new Hanging(67.5f, 43f, 2f, DecorArt.Hook),

            // A arena: as gaiolas da toca, de corrente curta, coladas no teto —
            // mais baixas elas passavam por cima das plataformas.
            new Hanging(57.5f, 32f, 1.2f, DecorArt.Cage),
            new Hanging(70.5f, 32f, 1.2f, DecorArt.Cage),

            // O abismo.
            new Hanging(90.5f, 31f, 3f, DecorArt.Hook),
        };

        public static readonly Lamp[] Lamps =
        {
            // Poco: os postes da Avenida que desceram pela calha.
            new Lamp(16.0f, 5f),
            new Lamp(26.3f, 5f),
            new Lamp(8.6f, 4f),
            new Lamp(36.2f, 22f),

            // A bancada do Andar 1: aceso, e a luz dela.
            new Lamp(3.9f, 40f, true),

            // Os andares e a Triagem.
            new Lamp(16.1f, 40f),
            new Lamp(87.8f, 50f),
            new Lamp(70.2f, 37f),

            // A arena: acendem quando ele cai. Ficam nos vaos entre as
            // plataformas (e fora da coluna do vapor da volta, 53 a 55), e nao
            // embaixo delas.
            new Lamp(49.0f, 20f),
            new Lamp(71.4f, 20f),
        };

        private static Piece[] BuildPieces()
        {
            var p = new List<Piece>();

            // Na parede. Os montes de sucata postos aqui viram fundo apagado (ver
            // Phase01DecorBuilder): e onde ficam os que estao no caminho de um
            // inimigo, pra ele nao sumir na frente do monte.
            void Wall(string sprite, float x, float y, bool flip = false) => p.Add(new Piece(sprite, x, y, Layer.Wall, flip));
            void Back(string sprite, float x, float y, bool flip = false) => p.Add(new Piece(sprite, x, y, Layer.Back, flip));
            void Mid(string sprite, float x, float y, bool flip = false) => p.Add(new Piece(sprite, x, y, Layer.Middle, flip));

            void GirderV(float x, float y0, float y1) => p.Add(new Piece(DecorArt.GirderV, x, y0, Layer.Wall, width: 1f, height: y1 - y0));
            void PipeV(float x, float y0, float y1) => p.Add(new Piece(DecorArt.PipeV, x, y0, Layer.Wall, width: 1f, height: y1 - y0));
            void PipeH(float x0, float x1, float y) => p.Add(new Piece(DecorArt.PipeH, x0, y, Layer.Wall, width: x1 - x0, height: 1f));
            void Conveyor(float x0, float x1, float y) => p.Add(new Piece(DecorArt.Conveyor, x0, y, Layer.Wall, width: x1 - x0, height: 1f));
            void Gear(string sprite, float cx, float cy, float speed) =>
                p.Add(new Piece(sprite, cx, cy, Layer.Wall, motion: Motion.Spin, speed: speed));

            // As pocas ficam um quarto de tile pra dentro do chao, pra assentar na beirada.
            void Puddle(float x, float y, bool small = false) =>
                p.Add(new Piece(small ? DecorArt.PuddleSmall : DecorArt.Puddle, x, y - 0.25f, Layer.Middle, oxide: true));
            void Vent(float x, float y) => p.Add(new Piece(DecorArt.Vent, x, y, Layer.Wall, oxide: true));

            // ------------------------------------------------------------------
            // O poco
            // ------------------------------------------------------------------

            // A Calha N.º 1, saindo do teto bem em cima do monte onde o Scrap acorda.
            Wall(DecorArt.Chute, 19.5f, 25.2f);

            // A obra: uma coluna de viga e o cano que corre pela parede oeste,
            // do chao da saliencia ate o teto.
            GirderV(14.2f, 16f, 26f);
            PipeV(4f, 13.5f, 19.5f);

            // As engrenagens presas na rocha.
            Gear(DecorArt.GearBig, 32f, 25f, 5f);
            Gear(DecorArt.GearMid, 6.5f, 25.5f, -9f);
            Gear(DecorArt.GearMid, 37.5f, 13.5f, 7f);

            // Um cartaz, a seta pra chamine, o cabo caido, os respiros.
            Wall(DecorArt.PosterCrown, 9.5f, 17.5f);
            Wall(DecorArt.ArrowUp, 35.0f, 23.5f);
            Wall(DecorArt.Cable, 10.9f, 23.9f);
            Vent(18f, 13.5f);
            Vent(34.5f, 18f);

            // Montanhas de sucata no fundo, apagadas na parede: o poco e mais fundo do que parece.
            Wall(DecorArt.Heap(2, 1), 27.6f, 4f);

            // O monte de sucata embaixo da calha, e mais lixo no chao.
            Back(DecorArt.Heap(2, 0), 15.6f, 5f);
            Back(DecorArt.Heap(1, 1), 22.6f, 6f, true);
            Back(DecorArt.Heap(0, 2), 9.6f, 4f);
            Back(DecorArt.Wheel, 28.1f, 4f);
            Back(DecorArt.PipePile, 29.5f, 4f);
            Back(DecorArt.Carcass(1), 4.05f, 14f);
            Back(DecorArt.Carcass(0), 35.1f, 6f);
            Back(DecorArt.Heap(0, 0), 20.2f, 18f);
            Back(DecorArt.Barrel, 12.2f, 12f);
            Mid(DecorArt.Bolts, 18.4f, 6f);
            Mid(DecorArt.Bolts, 9.5f, 14f);
            Mid(DecorArt.Bolts, 28.5f, 8f);
            Puddle(11.8f, 4f, true);
            Puddle(33.0f, 4f, true);
            Puddle(21.2f, 10f, true);
            Puddle(42.3f, 6f);

            // ------------------------------------------------------------------
            // Andar 1
            // ------------------------------------------------------------------

            // O cano do teto: sai da parede oeste, corre rente ao teto e, na
            // ponta leste, faz a curva e desce pela chamine ate o patamar do topo.
            PipeH(8.5f, 37.2f, 44.8f);
            Wall(DecorArt.PipeFlange, 16.5f, 44.8f);
            Wall(DecorArt.PipeFlange, 21.5f, 44.8f);
            Wall(DecorArt.PipeFlange, 31.5f, 44.8f);
            Wall(DecorArt.PipeElbowDownLeft, 37.2f, 44.8f);
            PipeV(37.2f, 21.5f, 44.8f);
            Vent(21.5f, 42.5f);
            Wall(DecorArt.Heap(1, 0), 20.3f, 40f);
            Back(DecorArt.BarrelTipped, 29.0f, 40f);
            Mid(DecorArt.Bolts, 13.5f, 40f);
            Puddle(23.8f, 40f, true);

            // ------------------------------------------------------------------
            // Andar 2 e a Triagem: o alto do distrito, quase deserto. So a
            // esteira parada da Triagem.
            // ------------------------------------------------------------------

            Conveyor(46f, 58.4f, 50f);
            Wall(DecorArt.ConveyorMotor, 58.2f, 50f);

            // ------------------------------------------------------------------
            // A Antessala
            // ------------------------------------------------------------------

            // O cano sai da parede oeste, corre rente ao teto e, antes do poco
            // que desce da Triagem, faz a curva e entra no teto.
            PipeH(48.5f, 88.8f, 41.8f);
            Wall(DecorArt.PipeFlange, 63.5f, 41.8f);
            Wall(DecorArt.PipeFlange, 78.5f, 41.8f);
            Wall(DecorArt.PipeElbowUpLeft, 88.8f, 41.8f);
            PipeV(88.8f, 42.8f, 43.5f);
            Wall(DecorArt.PosterDecree, 88.6f, 38.2f);
            Vent(74f, 39.3f);
            Wall(DecorArt.Heap(1, 2), 75.8f, 37f);
            Back(DecorArt.BarrelTipped, 81.6f, 37f);
            Back(DecorArt.Carcass(0), 93.3f, 37f, true);
            Mid(DecorArt.Bolts, 71.5f, 37f);
            Puddle(83.4f, 37f, true);
            Puddle(78.6f, 37f);

            // ------------------------------------------------------------------
            // A arena: a toca do Coletor
            // ------------------------------------------------------------------

            // Pouca coisa, e nada em volta das plataformas: os montes grandes que
            // ficavam embaixo e na frente delas sairam. Sobra um monte baixo no
            // canto leste, longe da beirada do degrau, e nada na frente do Scrap.
            Gear(DecorArt.GearBig, 49.6f, 30.2f, -3f);
            Gear(DecorArt.GearMid, 77.2f, 28.6f, 6f);
            Wall(DecorArt.ClawMarks, 53.5f, 26.3f);
            Wall(DecorArt.Tally, 48.3f, 24.0f);
            Wall(DecorArt.Tally, 48.5f, 25.1f);
            Back(DecorArt.Heap(0, 0), 75.8f, 20f, true);
            Back(DecorArt.Carcass(2), 58.4f, 20f);
            Mid(DecorArt.Bolts, 65.8f, 20f);
            Puddle(60.2f, 20f);
            Puddle(66.4f, 20f, true);

            // ------------------------------------------------------------------
            // O abismo
            // ------------------------------------------------------------------

            Wall(DecorArt.Cable, 85.4f, 29.55f);
            Wall(DecorArt.Cable, 91.4f, 29.55f, true);
            Wall(DecorArt.ArrowUp, 102.2f, 24.5f);
            Gear(DecorArt.GearMid, 102.6f, 28f, -8f);
            Back(DecorArt.Carcass(2), 83.2f, 22f);

            // O poco que sobe pro District 2: a viga e o cano da obra, do chao
            // da beirada de la ate o teto do poco.
            GirderV(98f, 22f, 49f);
            PipeV(100.6f, 21.5f, 49.5f);

            // A Pista de Provas, abandonada faz tempo, fica sem cenografia: so a
            // placa e a ferrugem. Quanto menos coisa, mais esquecida ela parece.

            return p.ToArray();
        }

        /// <summary>
        /// Confere que nenhum cano termina no ar: cada ponta tem que estar
        /// enterrada na rocha ou encostada numa curva, e cada curva tem que
        /// ligar dois canos.
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();
            var elbows = new List<Piece>();
            foreach (Piece piece in Pieces)
            {
                if (piece.Sprite.StartsWith("d1_pipe_elbow")) elbows.Add(piece);
            }

            var linked = new int[elbows.Count];

            // Um quarto de tile pra dentro da ponta tem que ser rocha: e o que
            // garante que o terreno, desenhado por cima, esconde o corte do cano.
            bool Buried(float x, float y) => Phase01Layout.IsRock((int)Math.Floor(x), (int)Math.Floor(y));

            bool AtElbow(float x, float y)
            {
                for (int i = 0; i < elbows.Count; i++)
                {
                    Piece elbow = elbows[i];
                    if (x >= elbow.X - 0.05f && x <= elbow.X + 1.05f && y >= elbow.Y - 0.05f && y <= elbow.Y + 1.05f)
                    {
                        linked[i]++;
                        return true;
                    }
                }

                return false;
            }

            void CheckEnd(Piece pipe, float x, float y, float inwardX, float inwardY)
            {
                if (AtElbow(x, y) || Buried(x + inwardX * 0.25f, y + inwardY * 0.25f)) return;
                problems.Add($"O cano em ({pipe.X}, {pipe.Y}) termina no ar em ({x}, {y}): enterre a ponta na rocha ou ligue numa curva.");
            }

            foreach (Piece piece in Pieces)
            {
                if (piece.Sprite == DecorArt.PipeV)
                {
                    float x = piece.X + 0.5f;
                    CheckEnd(piece, x, piece.Y, 0f, 1f);
                    CheckEnd(piece, x, piece.Y + piece.Height, 0f, -1f);
                }
                else if (piece.Sprite == DecorArt.PipeH)
                {
                    float y = piece.Y + 0.5f;
                    CheckEnd(piece, piece.X, y, 1f, 0f);
                    CheckEnd(piece, piece.X + piece.Width, y, -1f, 0f);
                }
            }

            for (int i = 0; i < elbows.Count; i++)
            {
                if (linked[i] < 2)
                {
                    problems.Add($"A curva de cano em ({elbows[i].X}, {elbows[i].Y}) so liga {linked[i]} cano(s): sobra uma boca no ar.");
                }
            }

            return problems;
        }
    }
}
