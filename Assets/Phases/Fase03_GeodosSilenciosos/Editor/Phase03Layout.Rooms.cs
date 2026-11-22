using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase03Layout
    {
        // ------------------------------------------------------------------
        // 1. A Estacao de Chegada
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde o elevador do District 2 para: a ultima chapa de latao e uma
        /// lanterna fraca, mais nada — e um lugar que ninguem mais habita. O
        /// poco segue pra cima, ate o alto do mapa e dali aos Ateliers, mas
        /// desabou logo acima da cabine: os escombros sao a
        /// primeira coisa que se ve, e o que a fase inteira vai buscar. Logo
        /// atras da cabine, um nicho fechado por cristal macico — tambem so
        /// abre com o que a broca deixa.
        /// </summary>
        private static void BuildArrival()
        {
            const string room = "01_Arrival";
            Room(room, 5, 11, 28, 21);

            Carve(5, 11, 28, 21);
            Solid(16, 19, 22, 21);

            // O poco, subindo acima da cabine ate onde a tela escurece. Os
            // trilhos vao ate o alto; a cabine em si e o objeto do elevador.
            (int carX, int carY, int carWidth) = ElevatorCar;
            Carve(carX, 22, carX + carWidth - 1, (int)ElevatorTopY + 4);
            Decor.Add(new DecorSpot("elevator_rail", carX, carY + 3, 4, carWidth, ElevatorTopY + 5 - (carY + 3)));

            // O poco tambem desce: a cabine veio subindo do District 2 e parou
            // aqui. O chao dela (celula logo abaixo do andar) e o objeto da
            // cabine, nao tile; pro mapa continua sendo chao.
            Carve(carX, ElevatorShaftBottom, carX + carWidth - 1, carY - 2);
            for (int x = carX; x < carX + carWidth; x++) FragileCells.Add((x, carY - 1));

            // O ombro de rocha a direita do poco: e nele (e na parede oeste)
            // que a viga dos escombros ficou entalada. Sem ele, a pilha
            // parecia pendurada no ar.
            (int rx, int ry, int rw, int _) = ElevatorRubble;
            Solid(rx + rw, ry, rx + rw + 1, 21);

            // O que escorregou da pilha: pedrinhas no chao, perto da cabine.
            Pebbles.Add((10.2f, 11f));
            Pebbles.Add((13.6f, 11f));
            Pebbles.Add((9.1f, 11f));

            // Ninguem mais passa por aqui: nem placa, nem aviso. So um tambor
            // largado e a luz que sobrou.
            Barrel(12, 11);

            Lamps.Add((9f, 12.5f, 5f));

            // Depois da broca, quem sobrou espera aqui, na estacao: e o caminho
            // de quem vai subir. O Crisol fica do lado do elevador.
            Purified.Add(new LoreSpot
            {
                Kind = "miner", Name = "Tarnished_Miner_1", Title = "Mineiro", X = 17, Y = 11,
                Lines = new[]
                {
                    "Escuta. ...Ouviu? Ela afinou. A pedra inteira afinou.",
                    "A gente passou anos aqui embaixo sem poder dar um pio. Agora vamos fazer barulho de propósito.",
                    "Vai ter festa, pequeno. Você é convidado. Foi você que desligou aquela coisa.",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "miner", Name = "Tarnished_Miner_2", Title = "Mineira", X = 24, Y = 11,
                Lines = new[]
                {
                    "Café? É de borra de caldeira, mas tá quente.",
                    "Eu operava a broca, sabia? No começo, a gente era seis lá dentro. Depois, ela não precisou mais da gente.",
                    "Ninguém contou o que tinha lá embaixo. Só mandavam: mais fundo.",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "crisol", Name = "Npc_Crisol_Purified", Title = "Crisol", X = 13, Y = 11,
                Lines = new[]
                {
                    "Você viu a placa. Então agora você sabe o que eu sabia.",
                    "Eu avisei, e me riscaram. Você desceu lá, com esse corpo de sucata, e fez a máquina parar.",
                    "...Obrigado, Scrap.",
                    "Talvez você não seja só sucata, afinal. Não me faz repetir.",
                    "E agora essa onda é sua. Usa nos escombros do elevador: eles não aguentam. Lá em cima fica o Laboratório.",
                },
            });

            Scatter(18, 11, 28, 21, 4, 11);
        }

        // ------------------------------------------------------------------
        // 2. O Tunel Escuro
        // ------------------------------------------------------------------

        /// <summary>
        /// Escuridao total e tres fossos de espinhos. Fazer barulho mostra
        /// onde pisar — e acorda os mites que dormem em cada trecho de chao.
        /// O primeiro fosso e largo: pede o segundo pulo, que e silencioso.
        /// </summary>
        private static void BuildDarkTunnel()
        {
            const string room = "02_DarkTunnel";
            Room(room, 29, 6, 78, 21);

            Carve(29, 6, 78, 21);

            Solid(29, 6, 36, 10);
            Spikes(37, 42, 6);
            Solid(43, 6, 50, 8);
            Spikes(51, 55, 6);
            Solid(56, 6, 64, 7);
            Spikes(65, 68, 6);
            Solid(69, 6, 78, 10);

            // Teto irregular.
            Solid(40, 18, 48, 21);
            Solid(58, 16, 62, 21);

            // Um ou dois mites em cada trecho de chao entre os fossos.
            Foe(FoeKind.CrystalMite, "Mite_Tunnel_1", 10, 36, 11, 32);
            Foe(FoeKind.CrystalMite, "Mite_Tunnel_2", 43, 50, 9, 46);
            Foe(FoeKind.CrystalMite, "Mite_Tunnel_3", 56, 64, 8, 59);
            Foe(FoeKind.CrystalMite, "Mite_Tunnel_4", 56, 64, 8, 62);
            Foe(FoeKind.CrystalMite, "Mite_Tunnel_5", 69, 111, 11, 73);

            Scatter(29, 6, 78, 21, 5, 21);
        }

        // ------------------------------------------------------------------
        // 3. O Primeiro Cacador
        // ------------------------------------------------------------------

        /// <summary>
        /// Um Echo Hunter parado no meio do corredor. Passa-se por ele so
        /// andando, ou manda-se ele pra uma ponta golpeando o cristal de la.
        /// Ha tambem uma passarela por cima, e nela o capacete de quem correu.
        /// </summary>
        private static void BuildFirstHunter()
        {
            const string room = "03_FirstHunter";
            Room(room, 79, 11, 108, 20);

            Carve(79, 11, 108, 20);

            Ledge(85, 90, 14);

            Foe(FoeKind.EchoHunter, "EchoHunter_First", 69, 111, 11, 95);
            Foe(FoeKind.CrystalMite, "Mite_First_1", 69, 111, 11, 88);
            Foe(FoeKind.CrystalMite, "Mite_First_2", 69, 111, 11, 101);
            Foe(FoeKind.EchoHunter, "EchoHunter_First_2", 69, 111, 11, 76);

            Resonant(room, "Resonant_Hunter_West", 82, 11, 0);
            Resonant(room, "Resonant_Hunter_East", 107, 11, 0);

            Tell("helmet", room, "Lore_Helmet", "um capacete amassado", 87, 15,
                "Um capacete de mineiro, amassado, esquecido em cima da passarela.",
                "Por dentro, riscado a prego: \"SHH!\"");

            Scatter(79, 11, 108, 20, 5, 31);
        }

        // ------------------------------------------------------------------
        // 4. O Poco Fragil
        // ------------------------------------------------------------------

        /// <summary>
        /// O caminho pro centro e pra cima, por laminas de cristal fino
        /// penduradas sobre espinhos. Pular e silencioso e elas aguentam; o
        /// dash, o golpe e o tombo as quebram. Dois patamares de rocha firme
        /// no meio, pra respirar.
        /// </summary>
        private static void BuildFragileShaft()
        {
            const string room = "04_FragileShaft";
            Room(room, 109, 7, 120, 43);

            // A entrada, na altura do corredor do cacador.
            Carve(109, 11, 111, 13);

            // A chamine: quatro de largura, espinhos no fundo. Sobe-se quicando
            // de parede em parede.
            Carve(112, 8, 115, 43);
            Spikes(112, 115, 8);

            // A saida pro tunel da Fenda, na parede leste.
            Carve(116, 34, 120, 37);

            // A unica lamina: um descanso no meio da subida, que quebra com dash.
            Fragile(112, 113, 24);

            Scatter(109, 8, 120, 43, 4, 41);
        }

        // ------------------------------------------------------------------
        // 4b. A Fenda
        // ------------------------------------------------------------------

        /// <summary>
        /// Da lamina alta do Poco Fragil sai um tunel pro leste, que acaba num
        /// mirante na beira da Fenda — o abismo gigantesco embaixo de tudo. Na
        /// beirada, uma placa da Coroa. Nao se desce: la embaixo e espinho, e o
        /// verde do Coracao de Quartzo pulsando.
        /// </summary>
        private static void BuildRift()
        {
            const string room = "04b_Rift";
            Room(room, 121, RiftBottom, 207, RiftTop);

            // O tunel e o mirante.
            Carve(121, 34, RiftLeft(34) - 1, 37);

            for (int y = RiftBottom; y <= RiftTop; y++) Carve(RiftLeft(y), y, RiftRight(y), y);
            Spikes(RiftLeft(RiftBottom), RiftRight(RiftBottom), RiftBottom);

            Tell("crownsign", room, "Lore_RiftSign", "a placa da beirada", RiftLeft(34) - 3, 34,
                "\"" + RiftSignText + "\"");

            OxideGlows.Add((140f, 3f, 12f));
            OxideGlows.Add((188f, 3f, 12f));

            Scatter(121, RiftBottom + 1, 207, RiftTop, 6, 45);
        }

        // ------------------------------------------------------------------
        // 5. A Gruta de Crisol
        // ------------------------------------------------------------------

        /// <summary>
        /// O centro do distrito, e a unica luz parada dele. A oeste, o patamar
        /// do Poco Fragil e a porta da Galeria das Gravuras; a leste, a
        /// Galeria dos Cacadores; no teto, a chamine da Abobada Fragil e o Poco
        /// das Notas, que sobe pra broca.
        /// </summary>
        private static void BuildGrotto()
        {
            const string room = "05_Grotto";
            Room(room, 109, 44, 152, 54);

            // O patamar do poco e a gruta.
            Carve(109, 44, 121, 48);
            Carve(122, 44, 152, 54);
            Solid(147, 52, 152, 54);

            Bench(room, "Workbench_Grotto", 124, 44);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            Lamps.Add((126f, 49f, 7f));
            Lamps.Add((140.5f, 49f, 7f));
            Lamps.Add((115f, 47f, 5f));

            Barrel(144, 44);
            Barrel(145, 44);
            Shelf(147, 44);
            Sign(113, 46);

            // A porta da Galeria dos Cacadores.
            Carve(153, 44, 153, 48);

            Scatter(122, 44, 152, 54, 6, 51);
        }

        /// <summary>
        /// O atalho de volta: um poco largo, liso, que cai da antessala da
        /// broca direto no teto da gruta. Largo demais pra subir quicando nas
        /// paredes, e sem ressalto nenhum — so serve pra descer.
        /// </summary>
        private static void BuildNoteShaft()
        {
            Carve(128, 55, 134, 91);
            Scatter(128, 55, 134, 91, 3, 55);
        }

        // ------------------------------------------------------------------
        // 6. A Galeria das Gravuras (Ala Oeste)
        // ------------------------------------------------------------------

        /// <summary>
        /// O corredor da Ala Oeste, tomado de bichos. Eles sao cegos: dormem
        /// no chao e acordam com o barulho. A Shard Sentinel, no pedestal do
        /// meio, atira no barulho; o pedestal parte o chao em dois trechos, e
        /// cada trecho tem os seus. O trecho oeste continua pela Camara do Eco
        /// adentro, entao quem anda ali anda pelos dois.
        /// </summary>
        private static void BuildEngravingGallery()
        {
            const string room = "06_EngravingGallery";
            Room(room, 36, 44, 108, 56);

            Carve(36, 44, 108, 56);
            Solid(36, 54, 44, 56);
            Solid(70, 44, 74, 46);

            Foe(FoeKind.ShardSentinel, "Sentinel_Gallery", 72, 72, 47);

            // O trecho leste, da gruta ate o pedestal.
            Foe(FoeKind.CrystalMite, "Mite_Gallery_1", 75, 108, 44, 80);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_2", 75, 108, 44, 88);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_3", 75, 108, 44, 96);
            Foe(FoeKind.EchoHunter, "EchoHunter_GalleryEast", 75, 108, 44, 92);

            // O trecho oeste, do pedestal ate o fundo da Camara do Eco.
            Foe(FoeKind.CrystalMite, "Mite_Gallery_4", 10, 69, 44, 48);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_5", 10, 69, 44, 62);
            Foe(FoeKind.EchoHunter, "EchoHunter_GalleryWest", 10, 69, 44, 56);

            // Nenhum ressalto pra fugir por cima: passa-se por eles no chao.
            Foe(FoeKind.EchoHunter, "EchoHunter_GalleryEast_2", 75, 108, 44, 102);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_6", 10, 69, 44, 38);

            Scatter(36, 44, 108, 56, 5, 61);
        }

        // ------------------------------------------------------------------
        // 7. A Camara do Eco (fim da Ala Oeste)
        // ------------------------------------------------------------------

        /// <summary>
        /// O fundo da Ala Oeste: um salao baixo, colado no poco do elevador,
        /// e no canto leste uma chamine estreita que sobe ate o buraco do chao
        /// das Ruinas. Sobe-se quicando de parede em parede. Um golpe no
        /// cristal de baixo serve de isca pra tirar os cacadores do caminho.
        /// </summary>
        private static void BuildEchoChamber()
        {
            const string room = "07_EchoChamber";
            Room(room, 10, 44, 35, 62);

            Carve(10, 44, 35, 50);

            // A chamine, de quatro de largura, ate as Ruinas.
            Carve(30, 51, 33, 62);
            Solid(29, 48, 29, 50);
            Solid(34, 48, 34, 50);

            Resonant(room, "Resonant_Echo_Lure", 12, 44, 0);

            Foe(FoeKind.EchoHunter, "EchoHunter_Echo", 10, 69, 44, 20);
            Foe(FoeKind.CrystalMite, "Mite_Echo_1", 10, 69, 44, 26);
            Foe(FoeKind.CrystalMite, "Mite_Echo_2", 10, 69, 44, 15);

            Scatter(10, 44, 35, 50, 5, 71);
        }

        // ------------------------------------------------------------------
        // 7b. As Ruinas (o fundo da Ala Oeste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da Camara do Eco, pelo buraco do teto: uma sala meio
        /// desabada, o teto caindo aos pedacos, onde ficou o que restou da
        /// historia da escavacao — quatro gravuras lado a lado, da esquerda pra
        /// direita, na ordem em que aconteceu. E a mesma ordem das estatuas.
        /// </summary>
        private static void BuildRuins()
        {
            const string room = "07b_Ruins";
            Room(room, 12, 61, 32, 72);

            Carve(12, 63, 32, 72);

            // O teto desabando: blocos pendurados e uma rampa de pedra caida
            // no canto oeste.
            Solid(12, 69, 14, 72);
            Solid(19, 71, 22, 72);
            Solid(27, 70, 30, 72);
            Solid(12, 63, 12, 64);

            Tell("engraving0", room, "Lore_Engraving_1", "a primeira gravura", 14, 63,
                "Uma laje rachada, encostada na parede. Riscada nela: uma fila de mineiros de picareta no ombro.",
                "Estão descendo. Todos olham para baixo.");

            Tell("engraving1", room, "Lore_Engraving_2", "a segunda gravura", 18, 63,
                "Ao lado, outra laje, partida no meio: uma figura maior que as outras, de coroa. O Rei de Prata.",
                "Ele não segura ferramenta nenhuma. Só aponta. Para baixo.");

            Tell("engraving2", room, "Lore_Engraving_3", "a terceira gravura", 22, 63,
                "Uma broca em espiral, descendo pela rocha. Em volta dela, riscos de impacto.",
                "Os mineiros agora são menores. Estão atrás dela, não na frente.");

            Tell("engraving3", room, "Lore_Engraving_4", "a última gravura", 26, 63,
                "Um losango enorme no fundo da terra: o Coração de Quartzo.",
                "Há uma fenda nele, de cima a baixo. Quem gravou pintou a fenda de verde.",
                "Depois desta, só pedra caída. Ninguém voltou para gravar o resto.");

            // Ate aqui ha bichos: dormem entre as lajes.
            Foe(FoeKind.CrystalMite, "Mite_Ruins_1", 13, 29, 63, 16);
            Foe(FoeKind.CrystalMite, "Mite_Ruins_2", 13, 29, 63, 24);

            // O que caiu do teto, espalhado pelo chao.
            Pebbles.Add((16.2f, 63f));
            Pebbles.Add((20.4f, 63f));
            Pebbles.Add((24.1f, 63f));
            Pebbles.Add((29.5f, 63f));

            Scatter(12, 63, 32, 72, 5, 76);
        }

        // ------------------------------------------------------------------
        // 8. A Galeria dos Cacadores (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Um corredor longo com tres cacadores dormindo no chao e uma
        /// passarela de cristal fino por cima. Por baixo, o silencio; por cima,
        /// o peso. As notas da trava estao espalhadas pelo corredor: cada uma
        /// tocada chama quem estiver perto. Uma delas tem veio de Oxide e
        /// precisa ser raspada antes — de costas pros cacadores. A sequencia
        /// certa abre a grade do teto, no fim do corredor, que leva ao elite.
        /// </summary>
        private static void BuildHunterGallery()
        {
            const string room = "08_HunterGallery";
            Room(room, 154, 44, 202, 56);

            Carve(154, 44, 202, 56);

            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_1", 154, 202, 44, 163);
            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_2", 154, 202, 44, 180);
            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_3", 154, 202, 44, 195);
            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_4", 154, 202, 44, 172);

            Resonant(room, "Resonant_East_1", 158, 44, 1, false, -1, "Door_East");
            Resonant(room, "Resonant_East_2", 188, 44, 2, true, -1, "Door_East");
            Resonant(room, "Resonant_East_3", 172, 44, 3, false, -1, "Door_East");

            // Esta porta nao toca a sequencia nem pinta as gemas: quem diz a
            // ordem e a placa de prata, embaixo da grade, em palavras. Cada
            // palavra lembra a cor de um cristal (1 vermelho, 2 ambar, 3 ciano).
            Doors.Add(new DoorSpec
            {
                Name = "Door_East", Gate = "Porta_Ressonante", Sequence = new[] { 3, 1, 2, 1, 3 },
                Preview = (154, 44, 4, 5), Glyphs = (190f, 54.5f), Hidden = true,
            });

            // A placa so tem as palavras. Embaixo, um bloco de notas pequeno,
            // de algum mineiro sem paciencia.
            Tell("silverplaque", room, "Lore_WordPlaque", "a placa de prata", 161, 44,
                "GELO · RUBI · MEL · SANGUE · CÉU",
                "Embaixo, num bloco de notas pequeno: \"muito chato ter que ficar andando de um lado pro outro pra abrir uma porta\"");

            Tell("note", room, "Lore_HunterRoster", "uma folha de caderno", 156, 44,
                "Uma folha arrancada de um caderno de turno, presa na rocha. A letra é de um colega:",
                "\"O capataz foi o primeiro a descer. Ouviu a pedra antes de todo mundo. Dizia que ela cantava pra ele.\"",
                "\"Primeiro ele perdeu os olhos. Depois a voz. Depois parou de caber no capacete.\"",
                "\"Desceu sozinho pra toca do fim do corredor e nunca mais subiu. Às vezes ainda dá pra ouvir ele chamando o turno.\"");

            // A subida pra toca do Capataz, no comeco do corredor: uma chamine
            // estreita, quicando de parede em parede, ate a grade do teto que
            // a sequencia abre. Ela da no lado oeste da toca — o Capataz espera
            // ajoelhado no fundo leste.
            Solid(164, 48, 164, 56);
            Solid(169, 48, 169, 56);
            Carve(165, 57, 168, 59);
            Gate("Porta_Ressonante", 165, 57, 4, 1, 1, 0);

            Scatter(154, 44, 202, 56, 5, 81);
        }

        // ------------------------------------------------------------------
        // 9. A Camara do Ressonante (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da Galeria dos Cacadores: a camara larga e alta onde mora
        /// o Capataz, o mini chefe que guarda a safira e a CHAVE ANTIGA. Depois
        /// da purificacao vira o salao de concertos dos mineiros. Na entrada, a
        /// bancada da ala.
        /// </summary>
        private static void BuildResonantChamber()
        {
            const string room = "09_ResonantChamber";
            Room(room, 164, 60, 202, 74);

            Carve(164, 60, 202, 74);

            // A saleta da chamine, a oeste: sobe-se pela grade do chao e
            // entra-se na toca pela porta lateral, que tranca na luta. Assim
            // o Capataz tem o chao inteiro da toca, sem buraco nenhum.
            Solid(164, 66, 169, 74);
            Solid(170, 64, 170, 74);
            Ledge(172, 177, 63);

            Mark(room, "Elite_TheResonant", EliteTile.X, EliteTile.Y);

            Foe(FoeKind.CrystalMite, "Mite_Chamber_1", 171, 202, 60, 171);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_2", 171, 202, 60, 172);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_3", 171, 202, 60, 187);

            Scatter(171, 60, 202, 74, 5, 91);
        }

        // ------------------------------------------------------------------
        // 10. A Abobada Fragil (Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da gruta, pela chamine: uma abobada escura, de laminas
        /// finas sobre espinhos. Pra enxergar e preciso barulho; o barulho
        /// quebra as laminas. Os cristais ressonantes nos patamares de rocha
        /// firme sao a resposta: golpeia-se um de la, a abobada inteira acende,
        /// e sobe-se no escuro que vem depois, lembrando o caminho. Sobe-se
        /// pouco: no alto, um patamar firme a leste abre a Volta Leste.
        /// </summary>
        private static void BuildFragileVault()
        {
            const string room = "10_FragileVault";
            Room(room, 137, 58, 162, 88);

            // A chamine, da gruta ate o alto da abobada: quatro de largura,
            // entre duas paredes de rocha. Sobe-se quicando de uma na outra.
            Solid(142, 48, 142, 54);
            Solid(147, 48, 147, 54);
            Carve(143, 55, 146, 59);

            Carve(138, 60, 162, 88);
            Solid(137, 58, 152, 59);
            Carve(143, 58, 146, 59);
            Spikes(148, 162, 60);

            // A parede oeste da chamine e um bloco macico; a leste, uma coluna
            // fina que acaba antes do topo, pra deixar sair.
            Solid(137, 60, 142, 84);
            Solid(147, 61, 147, 80);

            // No alto: a unica lamina da abobada, uma ponte comprida sobre o
            // escuro ate o patamar da saida, a leste. Atravessa-se andando.
            Fragile(147, 155, 84);
            Solid(156, 81, 162, 84);

            // A lanterna de som e o bilhete, no topo do bloco oeste.
            Resonant(room, "Resonant_Vault_1", 142, 85, 0);

            Tell("note", room, "Lore_VaultPage", "uma folha dobrada", 138, 85,
                "Uma folha dobrada, presa numa fresta da rocha:",
                "\"A abóbada canta quando a gente pisa errado. Aprendemos a pisar como quem reza.\"");

            Foe(FoeKind.CrystalMite, "Mite_Vault_1", 156, 169, 85, 158);

            Scatter(137, 60, 162, 88, 4, 101);
        }

        // ------------------------------------------------------------------
        // 10b. A Volta Leste
        // ------------------------------------------------------------------

        /// <summary>
        /// O caminho da abobada ate o Salao das Estatuas, dando a volta pela
        /// direita, por cima da Camara do Ressonante:
        ///
        ///   o corredor de baixo   pro leste, com um trecho de chao de cristal
        ///                         fino sobre espinhos (passa-se andando; dash
        ///                         ou tombo quebram), mites e um cacador
        ///   o poco do fundo       uma chamine: sobe-se quicando nas paredes
        ///   o corredor de cima    de volta pro oeste, com outro cacador, ate a
        ///                         porta do salao — trancada. Quem abre e a
        ///                         CHAVE ANTIGA, que o Capataz guarda.
        /// </summary>
        private static void BuildEastLoop()
        {
            const string room = "10b_EastLoop";
            Room(room, 163, 82, 201, 106);

            // O corredor de baixo.
            Carve(163, 85, 201, 90);

            Carve(170, 82, 174, 84);
            Spikes(170, 174, 82);
            Fragile(170, 174, 84);

            Foe(FoeKind.CrystalMite, "Mite_Loop_1", 175, 201, 85, 179);
            Foe(FoeKind.CrystalMite, "Mite_Loop_2", 175, 201, 85, 190);
            Foe(FoeKind.CrystalMite, "Mite_Loop_3", 156, 169, 85, 160);
            Foe(FoeKind.EchoHunter, "EchoHunter_LoopLow", 175, 201, 85, 186);

            // Uma isca pros que acordarem: golpeada, chama tudo pra longe.
            Resonant(room, "Resonant_Loop", 167, 85, 0);

            // O poco do fundo: uma chamine de quatro, entre duas paredes.
            Solid(196, 89, 196, 90);
            Solid(201, 89, 201, 90);
            Carve(197, 91, 200, 106);
            Carve(195, 101, 196, 106);

            // O corredor de cima, de volta pro oeste, ate a porta do salao.
            Carve(168, 101, 194, 106);
            Foe(FoeKind.EchoHunter, "EchoHunter_Loop", 168, 196, 101, 184);
            Foe(FoeKind.EchoHunter, "EchoHunter_Loop_2", 168, 196, 101, 174);
            Foe(FoeKind.CrystalMite, "Mite_Upper_1", 168, 196, 101, 171);
            Foe(FoeKind.CrystalMite, "Mite_Upper_2", 168, 196, 101, 179);
            Foe(FoeKind.CrystalMite, "Mite_Upper_3", 168, 196, 101, 188);
            Foe(FoeKind.CrystalMite, "Mite_Upper_4", 168, 196, 101, 193);

            // A porta do salao: uma grade com fechadura, no vao da parede leste.
            Carve(167, 101, 167, 104);
            Gate(StatueEntranceGate, 167, 101, 1, 4);

            Tell("note", room, "Lore_LoopNote", "um bilhete amassado", 191, 85,
                "Um bilhete amassado, preso numa lasca de cristal:",
                "\"Dei a volta pela direita, como o capataz mandou. Que portão é aquele?\"");

            Scatter(163, 85, 201, 90, 5, 105);
            Scatter(195, 91, 201, 106, 4, 106);
            Scatter(167, 101, 194, 106, 5, 107);
        }

        // ------------------------------------------------------------------
        // 11. O Campanario (fim da Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// O Salao das Estatuas, o enigma principal do distrito: uma galeria
        /// comprida e baixa, que se entra pela direita, vindo da Volta Leste.
        /// Quatro estatuas no chao, em fila e fora de ordem — o Mineiro, o Rei,
        /// a Broca e o Coracao. Apertadas na ordem da historia das Ruinas,
        /// abrem o segredo da galeria: a porta da parede oeste, que da na
        /// antessala da broca. Cada botao e um clangor que chama quem estiver
        /// perto — mas aqui nao nasce bicho nenhum.
        /// </summary>
        private static void BuildBelfry()
        {
            const string room = "11_Belfry";
            Room(room, 137, 100, 166, 106);

            Carve(137, 100, 166, 106);

            // As quatro estatuas, fora de ordem. A ordem certa e a das gravuras
            // das Ruinas: o Mineiro, o Rei, a Broca e o Coracao. Nada aqui diz
            // isso — quem viu as Ruinas sabe.
            Statue(room, "Statue_Drill", 2, 3, 141, 100);
            Statue(room, "Statue_Miner", 0, 1, 148, 100);
            Statue(room, "Statue_Heart", 3, 4, 154, 100);
            Statue(room, "Statue_King", 1, 2, 160, 100);

            // Cada botao apertado e um clangor que chama quem estiver perto —
            // mas aqui nao nasce bicho nenhum.

            // O segredo: a porta, na parede oeste. Atras dela, um poco curto
            // que cai na antessala da broca.
            Carve(135, 92, 136, 103);
            Gate("Porta_Campanario", 136, 100, 1, 4);

            // Um templo, nao uma caverna: parede de tijolos no fundo, piso de
            // lajes, uma viga no teto e colunas entre as estatuas.
            Decor.Add(new DecorSpot("temple_bricks", 137, 100, 1, 30, 7));
            Decor.Add(new DecorSpot("temple_floor", 137, 99, 11, 30, 1));
            Decor.Add(new DecorSpot("temple_lintel", 137, 107, 11, 30, 1));
            foreach (int x in new[] { 138, 144, 151, 157, 164 }) Decor.Add(new DecorSpot("temple_pillar", x, 100, 3, 1, 7));

            // Os cristais de eco: sem eles o salao nunca aparece no escuro.
            Scatter(137, 100, 166, 106, 5, 111);
        }

        // ------------------------------------------------------------------
        // 12. A antessala da broca
        // ------------------------------------------------------------------

        private static void BuildPreBoss()
        {
            const string room = "12_PreBoss";
            Room(room, 80, 92, 134, 98);

            Carve(80, 92, 134, 98);

            Bench(room, "Workbench_PreBoss", 84, 92);
            Lamps.Add((86f, 96f, 7f));

            Tell("note", room, "Lore_MinerNote", "um bilhete de mineiro", 96, 92,
                "Um papel dobrado, preso debaixo de um capacete.",
                "\"Ela não para mais. Desligamos a chave, e ela não parou. Não tem mais ninguém lá dentro, e ela não parou.\"",
                "\"Se você está lendo isto: não faça barulho perto dela. Ela não enxerga. Ela escuta.\"",
                "\"E tem a luz. Os pilares de cristal lá dentro acendem quando cantam, e ela corre atrás da luz como bicho atrás de lampião.\"",
                "\"Mas a luz apaga rápido. Se o cristal ainda estiver brilhando forte quando ela chega, ela perde o controle e se crava nele. Foi a única vez que vi aquele motor parado.\"");

            Barrel(90, 92);
            Barrel(91, 92);
            Shelf(104, 92);
            Sign(110, 95);

            Scatter(92, 92, 126, 98, 5, 121);
        }

        // ------------------------------------------------------------------
        // 13. A Camara de Perfuracao
        // ------------------------------------------------------------------

        /// <summary>
        /// A arena do Geode Drill, em tres camadas empilhadas. A luta comeca na
        /// de cima; a cada terco de vida a broca fura o chao e tudo desce uma.
        ///
        ///   camada 1 (chao 92)   quatro pilares ressonantes
        ///   camada 2 (chao 80)   dois
        ///   camada 3 (chao 68)   um so, e o brilho verde logo embaixo
        ///
        /// Na parede leste ha prateleiras em zigue-zague: e por onde se sobe de
        /// volta depois que o chao se foi — e dali, pelo atalho, ate a gruta e
        /// a estacao, onde a onda que a broca deixa abre o elevador.
        /// </summary>
        private static void BuildDrillChamber()
        {
            const string room = "13_DrillChamber";
            Room(room, 34, 68, 76, 112);

            Carve(77, 92, 79, 96);
            Carve(34, 68, 76, 112);

            Solid(34, 109, 38, 112);
            Solid(72, 109, 76, 112);

            int top = Slab("Slab_Drill_1", 34, 76, 90, 91);

            Solid(73, 78, 76, 79);
            int middle = Slab("Slab_Drill_2", 34, 72, 78, 79);

            // A volta, depois que o chao se foi: as prateleiras da parede
            // leste. E a unica sala com mais de uma plataforma — o chao dela
            // desaba inteiro na luta, e sem elas nao se sai do fundo.
            Ledge(73, 76, 71);
            Ledge(66, 69, 75);
            Ledge(66, 69, 83);
            Ledge(72, 75, 87);

            Resonant(room, "Resonant_Pillar_1", 44, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_2", 52, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_3", 60, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_4", 68, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_5", 48, 80, 0, false, middle);
            Resonant(room, "Resonant_Pillar_6", 62, 80, 0, false, middle);
            Resonant(room, "Resonant_Pillar_7", 55, 68, 0);

            Mark(room, "BossSpawn_GeodeDrill", BossTile.X, BossTile.Y);

            OxideGlows.Add((42f, 68.5f, 8f));
            OxideGlows.Add((55f, 68.5f, 9f));
            OxideGlows.Add((68f, 68.5f, 8f));

            Scatter(34, 92, 76, 112, 6, 131);
            Scatter(34, 80, 72, 89, 7, 132);
            Scatter(34, 68, 72, 77, 7, 133);
        }
    }
}
