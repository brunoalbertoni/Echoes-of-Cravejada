# Echoes of Cravejada

Jogo de plataforma e ação em 2D, em pixel art, feito na Unity. Você controla **Scrap**, um pequeno autômato descartado, e atravessa cinco distritos até enfrentar o Rei de Prata. A história termina em um de dois finais: o trono ou a coroa.

![Scrap](docs/imagens/conceitos/scrap_conceito.png)

## Fases

| Fase | Distrito | Chefe |
|---|---|---|
| 1 | Scrap Yard | O Coletor |
| 2 | Brass Pipelines | Mestre das Caldeiras |
| 3 | Silent Geodes | O Capataz e a Broca |
| 4 | Alchemical Ateliers | O Alquimista |
| 5 | Silver City | O Rei de Prata |

## Controles

| Ação | Teclado |
|---|---|
| Andar | `A` `D` ou setas |
| Pular | `Espaço`, `W` ou seta para cima |
| Golpear | `J` ou `X` |
| Mirar o golpe | `W` `S` ou setas, junto com o golpe |
| Dash | `Shift esquerdo` ou `L` |
| Interagir e salvar na bancada | `E` |

## Como abrir

1. Instale a Unity **6000.5.7f1**.
2. Abra esta pasta pelo Unity Hub.
3. Abra `Assets/Common/Scenes/MainMenu.unity` e aperte Play.

## Organização

```text
Assets/
  Common/       sistemas gerais: jogador, combate, interface, áudio, cutscenes
  Phases/       cada fase com cena, inimigos, chefes, mecânicas e ferramentas
  Animations/   clipes e Animator Controllers dos personagens
  Generated/    sprites, tiles e quadros usados pelo jogo
  LeanTween/    biblioteca das transições de interface
docs/           planos do projeto e imagens de referência
Tools/          ferramentas externas, como o sintetizador de efeitos sonoros
```

A lógica de cada personagem escolhe o quadro, e o `FrameAnimator` o exibe pelo Animator. O código fica em dois módulos: `Echoes.Runtime` (o jogo) e `Echoes.Editor` (as ferramentas).

O fluxo do jogo passa pelo `GameManager`. Projéteis, efeitos e coletáveis nascem de prefabs, a câmera usa Cinemachine, e a interface usa Canvas, TextMeshPro e LeanTween. Métodos e variáveis do jogo estão em português.

Para refazer uma cena, use o menu **Echoes > Gerar Fase N**. Ele valida o mapa, monta a cena e recria os Animators. O que testar está em [docs/roteiro-de-testes.md](docs/roteiro-de-testes.md).

Os PNGs em `Assets/Generated` são a versão oficial da arte. Os geradores só desenham uma imagem quando ela não existe.

## Créditos de terceiros

- Música: Scott Buckley (scottbuckley.com.au), licença CC BY 4.0
- Ambiente: fontes CC0 do OpenGameArt. Detalhes em `Assets/Common/Audio/CREDITOS.md`
- Cave Platformer Tileset
- 2D Dungeon Tilemap
