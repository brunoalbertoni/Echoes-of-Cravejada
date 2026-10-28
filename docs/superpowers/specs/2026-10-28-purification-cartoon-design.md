# Cutscene desenhada da purificação do Scrap (Phase 1)

## Intenção

Quando o Scrap é purificado pela primeira vez (fim do Rusty Scrapper, Phase 1),
o jogo sai da câmera normal e passa um curta "de desenho animado": quadros
ilustrados em tela cheia mostrando ele se transformando parte por parte e o
ambiente em volta voltando à vida. É o momento mais marcante da fase. Hoje a
troca de visual é só um brilho e um inchaço no sprite (`RustyScrapper.Polish`).

## O que o usuário pediu (palavras dele)

- "Literalmente uma cutscene, que saia da câmera convencional, como se fosse um desenho animado."
- Sequência: "primeiro vê ele de costas, começando a se purificar; depois mostra
  o braço, as armas, o ambiente em volta, o tubo de ar perdendo a ferrugem e
  começando a soltar ar; depois uma arte MUITO FODA dele purificado."
- "Por enquanto só nessa" (só na Phase 1).
- A cerimônia em jogo que já existe (coração, onda, câmera) ele aprovou: "adorei a cutscene, mto boa".

## Objetivos

1. Cinco quadros, em tela cheia, com faixas pretas, na ordem pedida:
   1. **De costas:** Scrap por trás, diante da luz do coração da área. A ferrugem
      começa a se soltar em lascas que sobem; contorno de luz crescendo.
   2. **O braço:** close do antebraço e da mão. A crosta racha (rachaduras
      crescendo), pedaços caem, o bronze limpo aparece por baixo, do ombro pra mão.
   3. **As armas:** close do cinzel. A ferrugem descasca do cabo até a ponta,
      a lâmina fica prateada e um brilho corre pelo fio.
   4. **O ambiente:** um tubo de ar da caverna (o mesmo tipo dos respiros da
      Phase 1). A ferrugem recua pelo tubo, as juntas brilham, e ele volta a
      soltar ar em jatos. Em volta, o musgo/cristais acendem.
   5. **A pose final:** ilustração grande e caprichada do Scrap polido, olhos
      azuis acesos, capa balançando, raios de luz atrás. Fica um instante, um
      clarão, e corta de volta pro jogo.
2. Cada quadro tem movimento próprio (não é slide): a transformação "varre" o
   desenho parte por parte, partículas (lascas, faíscas, ar), leve movimento de
   câmera dentro do quadro (aproximar/deslizar), capa animada na pose final.
3. Ao voltar pro jogo, o Scrap já está polido (a troca acontece escondida
   durante o curta) e os respiros já estão soprando.
4. Visual coerente com o resto: pixel art, mesmas paletas do `HeroArt`
   (`Look.Scrap()` → `Look.Polished()`): pedra/ferrugem/pano → cristal/bronze/couro/prata.

## Fora do escopo

- Phases 2 e 3 (continuam só com a cerimônia em jogo).
- Som/música (o projeto não tem áudio dessa parte).
- Mudar a cerimônia em jogo que já existe.

## Anti-objetivos (falharia mesmo funcionando)

- Parecer uma apresentação de slides parada.
- Arte "genérica" que não lembre o Scrap das imagens de referência (cabeça de
  pedra/cristal facetada, olhos brilhando, mola, capa rasgada, corpo redondo, cinzel grande).
- Travar o jogador pra sempre ou deixar o jogo em estado de cena depois do fim.

## Como vai ser feito (decisões minhas, podem ser trocadas)

- **Arte desenhada por código** no Editor (novo `Assets/Editor/PurificationCartoonArt.cs`,
  sem Unity, em cima do `PixelCanvas`), em resolução 320×180 por quadro, salva
  como PNG em `Assets/Generated/Cutscene/`. Para cada quadro: a versão "antes",
  a versão "depois" e um mapa de ordem (em tons de cinza: o que é mais escuro
  se transforma primeiro). Assim a transformação acontece parte por parte, na
  ordem que eu pintar no mapa (ex.: ombro → mão → dedos).
  Os PNGs podem ser trocados por arte feita à mão depois, mantendo o tamanho.
- **Tocador em jogo** (novo `Assets/Scripts/PurificationCartoon.cs`): desenha
  por cima de tudo (OnGUI, como o HUD), com filtro de pixel nítido. A cada
  quadro mistura "antes"/"depois" pelo mapa de ordem, com uma faixa de brilho
  na borda da transformação, e desenha as partículas em pixels na própria imagem.
- **Ligação:** em `RustyScrapper.Fall`, a etapa `Polish()` vira o curta:
  cerimônia em jogo → corte pro curta → no clarão final `appearance.Polish()` → volta.
  O gerador da Phase 1 passa as texturas pro chefe.
- **Duração:** uns 14 s no total (≈3 + 2,5 + 2,5 + 3 + 3,5). Dá pra pular
  apertando pular/atacar depois do primeiro segundo.
- **Antes de ligar no jogo**, eu mostro prévias (PNG/GIF de cada quadro) pra
  aprovação da arte, principalmente da pose final.

## Para quem for construir decidir

- Composição exata de cada desenho, cores de brilho, velocidade das partículas.
- Se o movimento de câmera de cada quadro é aproximar ou deslizar.
