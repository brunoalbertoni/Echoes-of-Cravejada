# Phase 2 — The Brass Pipelines: redesenho planejado

Data: 2026-10-27
Arquivo afetado: `Assets/Editor/Phase02Layout.cs` (o mapa é gerado por código;
`Phase02Mechanics.cs` e `Phase02Generator.cs` só mudam se precisarem).

## Por que

Jogando a Phase 2, a sensação é de fase "jogada": salas espalhadas sem lógica
de ligação, NPCs em lugares sem sentido, áreas grandes que não pagam a
travessia. Diagnóstico do layout atual:

1. A missão do velho encanador está invertida: ele pede as 4 páginas do livro
   de turnos, mas fica num cofre que é uma das últimas coisas abertas. O
   jogador junta páginas sem saber por quê.
2. Tudo o que aparece depois da purificação (Solda, Galeria Alta, falas novas
   do Crisol) fica longe de onde a purificação acontece (na saída), sem atalho.
3. A história do Boiler Master está espalhada sem ordem (diário no extremo
   leste, relatório no extremo oeste, ficha no depósito). Dá para lutar com
   ele sem saber quem era.
4. Áreas grandes que pagam pouco (Torre: 4 andares e 7 inimigos por um
   chassi; Esgoto: 9 pulos iguais por 30 fragmentos).
5. As combinações difíceis de inimigos estão nas áreas opcionais; o caminho
   principal é ralo.

## Objetivo

Remodelar a fase inteira, **com os mesmos elementos**, de forma planejada:

- O jogador entende a lore em ordem e chega ao Boiler Master sabendo que ele
  era o chefe de manutenção que escondeu as máscaras e se trancou com as
  caldeiras para o distrito não congelar.
- Exploração livre: depois do centro, o jogador escolhe a ordem das alas.
- Toda área opcional termina em algo que vale a travessia.
- Salas próximas e ligadas com lógica; atalhos de volta ao centro.

## Não-objetivos

- Não mudar mecânicas (vapor, válvulas, caldeiras de pressão, ácido, boss).
- Não criar inimigos, NPCs ou tipos de recompensa novos.
- Não mudar os atributos dos inimigos nem do boss.
- Não mexer na Phase 1, 3 ou 4, nem nos nomes de cena e de spawn que as ligam.

## Antiobjetivos (seria fracasso mesmo funcionando)

- **Banalizar gemas.** Gemas são raras: a fase tem exatamente 4 e continua com
  4. Nenhuma gema fácil.
- Dificuldade que muda ao longo da fase (escala por ala, por trava aberta etc.).
- Uma ala que só dá para fazer depois de outra (a ordem tem que ser livre).
- Área opcional sem recompensa no fim.
- Ter que atravessar o mapa inteiro de volta para ver o conteúdo pós-boss.

## Dificuldade

Uniforme do começo ao fim, calibrada para o Scrap que sai da Phase 1: Steam
Dash, Wall Grip, 5 de vida (6 com o chassi da Phase 1), dano 1, alguns
fragmentos, e o que puder comprar no Crisol (Engaste de ouro dá dano 2).

- Atributos dos inimigos ficam como estão (sanguessuga 5 de vida, sentinela 6,
  welder 7, bloater 8; todos tiram 1).
- As três alas têm densidade e mistura de inimigos parecidas (cerca de 10 a 12
  cada, com os quatro tipos).
- Antes do centro, cada tipo novo aparece uma vez, sozinho, para o jogador
  aprender (isso é apresentação, não rampa): sentinela, sanguessuga e bloater
  na entrada; welder na Praça.

## Estrutura

```
                 [11 Saída D3]──[topo da fuga: PURIFICA]──[F Galeria Alta]
                                   │   │ (atalho da purificação)
                              [10 Fuga]│
                                   │   │
                            [09 Caldeiras]
                                   │
                            [08 Pré-boss + bancada]
                                   │
                         [Escadaria das 3 Travas]
                                   │
  [C Sótão]──[B Casa de Máquinas]  │
        ALA ALTA     └──(escotilha)┤
                                   │
 [01 Galeria]─[02 Canos]─[03 Válvula]─[05 PRAÇA (Welders)]─[06 Cadeia de Vapor]─[07 Labirinto]
                                   │                         ALA LESTE
                          [04 Cisterna + Crisol]
                          [cofre do encanador]
                                   │ (alçapão)
                          [A Esgoto]──[D Depósito]──[E Torre]──(volta à Praça)
                                  ALA BAIXA
```

### Entrada (linear e curta)

01 Galeria de Transição → 02 Primeiros Canos → 03 Válvula Emperrada → Praça.

- Inimigos: só apresentação (1 sentinela, 1 sanguessuga, 1 bloater, cada um
  sozinho).
- Lore: **página 1** do livro de turnos (a pressão subindo, "disseram que é o
  tempo"). O jogador acha a página antes de conhecer o dono, e o encanador
  logo depois diz "você já tem uma!".

### O centro: Praça (05 Salão dos Welders) e Cisterna (04)

A Praça é o salão dos operários, por onde se passa sempre. A Cisterna, embaixo
dela, é a sala segura.

- **Cisterna:** Crisol, a loja e a bancada. Do lado, o **cofre do velho
  encanador**, aberto pela primeira caldeira de pressão (ensina a mecânica
  cedo). Ele conta por que sobrou operário (as máscaras de solda) e pede as 4
  páginas.
- **Praça:** pichações 1 e 2 ("A prata bebe, o latão sua"; a máscara a giz). É
  aqui que se apresenta o welder (2 welders e 1 sanguessuga; não voltam,
  a menos que o Scrap morra).
- **A Escadaria das 3 Travas:** sai da Praça para cima, rumo à sala pré-boss.
  São três portões de grade, um por ala, cada um com um manômetro grande do
  lado. O jogador vê o objetivo da fase logo de cara. Lore: foram os operários
  que trancaram a porta por fora a pedido do chefe ("tranquem a porta e não a
  abram, ouçam o que ouvirem").
- Da Praça saem as três alas: **para cima** (Alta), **pelo alçapão da
  Cisterna** (Baixa) e **para leste** (Leste).

### As três alas (qualquer ordem)

Cada ala:
- conta **um** pedaço da história;
- guarda **uma** página do livro de turnos;
- termina numa **válvula que abre uma das três travas** e numa recompensa
  grande;
- tem um **atalho de mão única de volta para a Praça** no fim.

#### Ala Alta: a culpa da Coroa (B Casa de Máquinas, C Sótão dos Manômetros)

- Jogo: plataformas verticais em andares de grade, nuvem de vapor no vão,
  coluna pulsante, válvulas que ligam as colunas. Sentinelas e welders em
  destaque.
- Lore: **relatório de inspeção** ("NEGADO. A CIDADE NÃO PODE ESCURECER — O
  REI DE PRATA"), **pichação 3** ("ELES NUNCA DESCERAM ATÉ AQUI") e a
  **página 2** (a água com gosto de cobre, o manômetro no vermelho).
- Recompensas: **gema** (no fundo do Sótão), cofre de 30 fragmentos,
  **bancada** da ala.
- Válvulas: Hall_1, Attic e a última (Hall_2), que abre a trava Alta e a
  escotilha de volta para a Praça.

#### Ala Baixa: os operários e as máscaras (A Esgoto, D Depósito, E Torre)

Desce pelo alçapão, atravessa por baixo para leste e sobe de volta pela Torre:
um laço.

- Jogo: ilhas sobre o ácido com dash (menos e mais variadas do que as 9 de
  hoje), caldeira de pressão que derruba a parede do Depósito, e a Torre de
  andares com válvula e nuvem. Bloaters e sanguessugas em destaque.
- Lore: **placa do coletor** (os 12 nomes), **ficha do depósito** (o chefe
  escondeu as máscaras da Coroa), **página 4** ("As máscaras funcionam") e, no
  topo da Torre, o **diário do chefe de manutenção** ("vou ficar com as
  caldeiras"), virado para a sala das caldeiras.
- Recompensas: cofre de 50 fragmentos e o **reforço de chassi** (no topo da
  Torre).
- Válvulas: as três da Torre; a do topo abre a trava Baixa.

#### Ala Leste: quem ficou para trás (06 Cadeia de Vapor, 07 Labirinto de Válvulas)

- Jogo: a travessia do lago (coluna, jato, coluna pulsante) e o labirinto de
  três válvulas em sequência. Mistura de todos os tipos.
- Lore: a **carta** nunca enviada ("eu desço os canos atrás de você") e a
  **página 3** (os colegas que não voltaram, ferramentas verdes).
- Recompensas: **gema** no nicho secreto da Cadeia de Vapor e o cofre de 40
  fragmentos do mirante.
- Válvulas: Maze_A, Maze_B e Maze_C; a última abre a trava Leste.

### O boss e depois

Escadaria das Travas → 08 Pré-boss (bancada) → 09 Sala de Caldeiras (Boiler
Master, com as 3 válvulas da arena, sem mudanças na luta) → 10 Fuga com o
ácido subindo → topo da fuga, onde o distrito é purificado.

No topo da fuga:
- a oeste, 11 Saída para os Silent Geodes (placa do cano-mestre);
- a leste, a **Galeria Alta**, fechada pela névoa até a purificação e que só se
  completa com o Salto de Pressão (o mirante, a placa "agora é de quem
  subir" e a **gema** pós-boss);
- um **atalho aberto pela purificação** que desce até a Praça.

Depois da purificação, na Praça e na Cisterna: a **Solda** (sem máscara,
conta quem era o Mestre das Caldeiras), as falas purificadas do Crisol e do
encanador. Tudo a um atalho de distância.

## Gemas (exatamente 4, como hoje)

| Gema | Onde | O que custa |
|---|---|---|
| 1 | fundo do Sótão (Ala Alta) | atravessar a ala Alta inteira |
| 2 | nicho secreto da Cadeia de Vapor (Ala Leste) | coluna escondida, só para quem procura |
| 3 | mirante da Galeria Alta | pós-boss, exige o Salto de Pressão |
| 4 | velho encanador | as 4 páginas (uma em cada ala e uma na entrada) |

Antes do boss o jogador tem no máximo 3 gemas. O Engaste de ouro (2 gemas)
pede pelo menos duas alas bem exploradas.

## Inventário (tudo continua)

- 17 salas: 01 a 11 e A a F. A Escadaria das Travas é uma passagem nova entre
  a Praça e o pré-boss, não uma sala de conteúdo.
- 13 válvulas (Válvula Emperrada, 3 do Labirinto, 2 da Casa de Máquinas, a do
  Sótão, 3 da Torre e 3 da arena). Algumas mudam o que ligam.
- 2 caldeiras de pressão (cofre do encanador; Depósito) e 2 paredes rachadas.
- Portões: o do Labirinto, a escotilha da Casa de Máquinas, a grade da saída
  da arena, **mais as 3 travas** (o mesmo portão de grade de hoje, aberto por
  válvula) e o atalho da purificação (o tipo `Purification` já existe).
- Vapor: colunas, a coluna pulsante, o jato e as 2 nuvens.
- 1 névoa de Óxido (Galeria Alta).
- Cerca de 41 inimigos (17 sanguessugas, 9 sentinelas, 9 welders, 6 bloaters),
  redistribuídos conforme "Dificuldade".
- 16 peças de lore e Crisol: as 4 páginas, 3 pichações, Solda, carta, placa do
  coletor, encanador, relatório, ficha, diário, placa do mirante, placa do
  cano-mestre.
- Recompensas: 4 gemas, 3 cofres (30, 40, 50) e 1 chassi.
- 3 bancadas: Cisterna, Ala Alta e Pré-boss.

## Restrições técnicas

- O mapa continua em `Phase02Layout.cs`, uma função por sala, sem depender de
  UnityEngine.
- `Validate()` continua passando e ganha checagens novas:
  - cada ala se completa partindo da Praça sem nenhuma outra ala feita
    (ordem livre de verdade);
  - o boss é inalcançável com qualquer trava fechada;
  - todos os elementos que pedem a purificação ficam a um atalho da Praça;
  - a gema da Galeria Alta continua exigindo o Salto de Pressão.
- Mantêm-se `SceneName`, `FromDistrict1SpawnId`, a saída para
  `Phase03Layout.FromDistrict2SpawnId` e os nomes que as mecânicas procuram
  (`BossGate`, `RisingAcidPool` etc.).
- Validar fora do Editor (projeto `net9.0` apontando para os `.cs`), e gerar
  uma prévia do mapa antes de pedir ao usuário que rode o gerador.
- Sem commit nem push: o usuário sobe as alterações.

## Decisões minhas (para conferir)

- A Torre das Válvulas passou da ala Leste para a ala Baixa, para fechar o laço
  "desce, atravessa, sobe" e equilibrar o tamanho das alas.
- Página 4 ("As máscaras funcionam") na ala das máscaras, em vez do pré-boss.
- A gema do encanador sai da Casa de Máquinas: ele vai para o lado da Cisterna.
- Pequenos ajustes de texto só onde a fala aponta para um lugar que mudou
  (por exemplo, as dicas do encanador sobre onde caíram as páginas). O resto
  das falas fica igual.
- O tamanho do mapa pode encolher (hoje 290 × 110) para aproximar as salas.
- Como a trava abre longe de quem gira a válvula, girar uma válvula de trava
  mostra um aviso na tela (o `HudNotice` que já existe), algo como "Uma trava
  da Escadaria se abriu (2/3)". É a única mudança fora do layout.

## Deixado para quem constrói

Coordenadas exatas, formato de cada sala, posição fina de inimigos e enfeites
(canos, luzes, névoas), desde que respeitem o que está acima.

## Revisão 2 — mapa refeito do zero (2026-10-27)

Depois de jogar, o usuário pediu a fase inteira refeita sem seguir o layout
antigo, com mais decoração e fluxo melhor. Mantido: centro + três alas livres +
Escadaria das Travas, toda a lore e os NPCs. Mudanças:

- **Gemas: 2** (antes 4): a do encanador (as 4 páginas) e a do nicho secreto da
  Cadeia de Vapor (duas nuvens no alto). Os outros prêmios viraram cofres de
  fragmentos (30, 40, 50, 60) e o chassi.
- **Mapa compacto**: 216×116 (antes 290×110). Cada sala tem uma função clara e
  as válvulas ficam à vista do que ligam.
- **Decoração nova** (arte gerada em `BrassDecorArt.cs`): lampiões, armários de
  operário, cabide de máscaras (uma só sobrou), prateleiras vazias do depósito,
  tambores, guarda-corpos, cartazes da Coroa, placas de sala e correntes.
- **Bancadas**: Cisterna, Casa de Máquinas (andar 3), patamar da Cadeia de
  Vapor, Depósito e pré-boss.
- **Regras novas na validação**: inimigo longe de válvula, lore, recompensa e
  bancada (e fora da linha de tiro); toda sentinela pode ser alcançada; coluna
  desligada tem a válvula na mesma sala (ou avisa no HUD); toda sala que pede
  esforço tem algo para achar; decoração não flutua; exatamente 2 gemas.
