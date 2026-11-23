# Echoes of Cravejada

Jogo de plataforma e ação em 2D, em pixel art, feito na Unity. Você controla **Scrap**, um pequeno autômato descartado, e atravessa cinco distritos até enfrentar o Rei de Prata.

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
| Golpear | `J`, `X` ou botão esquerdo do mouse |
| Mirar o golpe | `W` `S` ou setas, junto com o golpe |
| Dash | `Shift esquerdo` ou `L` |
| Interagir e salvar na bancada | `E` ou `K` |
| Pausar | `Esc` |

No controle: botão de baixo pula, o da esquerda golpeia, o de cima interage, `RB` faz o dash e `Start` pausa. Os menus aceitam teclado, controle e mouse.

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

Para refazer uma cena, use o menu **Echoes > Gerar Fase N**. Ele valida o mapa, monta a cena e recria os Animators.

Os PNGs em `Assets/Generated` são a versão oficial da arte. Os geradores só desenham uma imagem quando ela não existe.

## Tecnologias

| Área | Tecnologia | Como é usada no jogo |
|---|---|---|
| Motor | Unity 6 (`6000.5.7f1`) | Cenas, componentes, física, áudio e build |
| Linguagem | C# | Todo o jogo e as ferramentas do Editor |
| Módulos | Assembly Definitions | `Echoes.Runtime` (jogo) e `Echoes.Editor` (ferramentas) |
| Renderização | URP 17.6 com Renderer 2D | Sprites com luz, usando `Light2D` em 29 scripts |
| Cenário | Tilemap e Tilemap Extras | Terreno, parede de fundo e tiles animados |
| Física | Physics 2D | `Rigidbody2D`, colisores, gatilhos, raycasts e áreas de golpe |
| Câmera | Cinemachine 3.1 | `ScrapCameraFollow` calcula o alvo e entrega a uma `CinemachineCamera` |
| Entrada | Input System 1.20 | Leitura direta de `Keyboard`, `Mouse` e `Gamepad` |
| Animação | Animator e clipes gerados | `FrameAnimator` liga o quadro escolhido pela lógica ao Animator |
| Interface | uGUI (Canvas) e TextMeshPro | Telas desenhadas por `TelaCanvas` |
| Transições | LeanTween | Abertura e fechamento da tela na troca de cena |
| Efeitos | Particle System e partículas próprias | Névoa, poeira, vapor, faíscas e explosões |
| Áudio | `AudioSource` e `AudioClip` | Mixagem por código, sem `AudioMixer` |
| Save | JSON, `JsonUtility` e Reflection | Arquivo `save.json` com o progresso |
| Preferências | `PlayerPrefs` | Volumes de música e efeitos |
| Carregamento | `Resources` e `LoadSceneAsync` | Sons, arte, prefabs e troca de fase |
| Efeitos sonoros | Python e NumPy | `Tools/Audio/sfx_synth.py` sintetiza os WAV |
| Arquivos grandes | Git LFS | Imagens, áudio, fontes e modelos |

Timeline, Visual Scripting, Sprite Shape e os pacotes de IA da Unity estão instalados, mas o código do jogo não os utiliza.

## Como os sistemas funcionam

### Inicialização

`GameManager`, `SoundDirector`, `SfxPlayer` e `TelaCanvas` nascem sozinhos antes da primeira cena, com `RuntimeInitializeOnLoadMethod`, e atravessam as fases com `DontDestroyOnLoad`.

O projeto entra em Play sem recarregar o domínio do C#. Por isso, as classes com dados estáticos têm um método `ReiniciarAoJogar`, que limpa esses dados a cada Play.

### Áudio

O áudio tem quatro camadas:

| Camada | Arquivos | Quem toca |
|---|---|---|
| Música | 16 OGG em `Resources/Soundtrack/Music` | `MusicPlayer` |
| Ambiente | 9 loops em `Resources/Soundtrack/Ambience` | `SoundDirector` |
| Sons presos ao mapa | Vapor, caldeira, ácido, cristal e raios | `AmbientEmitter` |
| Efeitos | 230 WAV em `Resources/Sfx` | `SfxPlayer` |

**Música.** A cada frame, o `SoundDirector` decide o que deve tocar, nesta prioridade:

1. Final do jogo.
2. Purificação do distrito.
3. Luta de chefe.
4. Silêncio depois da queda do chefe.
5. Fuga do ácido, na fase 2.
6. Música de exploração da fase.

O `MusicPlayer` usa duas fontes de áudio para fazer crossfade nas trocas e na volta do loop. As faixas e seus pontos de início e repetição ficam em `SoundtrackCatalog`. Durante os diálogos, a música cai para 70%.

**Efeitos.** Cada som é um `SfxCue` declarado na tabela `Sfx`, com arquivo, volume, variações, tom e tempo mínimo entre repetições. Para tocar, basta uma linha:

```csharp
Sfx.MordidaRastejante.Tocar(transform.position);
```

O `SfxPlayer` mantém 24 vozes reutilizáveis. Quando todas estão ocupadas, a mais antiga é substituída. Sons com posição diminuem com a distância da câmera, somem a partir de 22 tiles e usam panorama estéreo. O tom varia um pouco a cada toque, e sons com variações não repetem o último arquivo.

**Importação.** O `SoundtrackImport` configura os arquivos ao entrarem no projeto: música em streaming, ambiente comprimido na memória e efeitos prontos para tocar sem atraso.

**Volume.** O jogador ajusta música e efeitos de 0 a 10 na pausa. `GameAudio` guarda esses valores em `PlayerPrefs`, separados do save.

### Interface clicável

As telas não usam `Button` nem `EventSystem`. Cada uma funciona em três partes:

1. O `Update` lê teclado e controle e move a seleção.
2. O `OnGUI` calcula posição, cor e animação de cada peça e pede o desenho ao `TelaCanvas`.
3. O próprio `OnGUI` compara o mouse com o retângulo de cada botão.

```csharp
if (e.type == EventType.MouseMove) Selecionar(i);
if (e.type == EventType.MouseDown && e.button == 0) Escolher(i);
```

O `TelaCanvas` transforma esses pedidos em `RawImage` e `TextMeshProUGUI` dentro de Canvas. Ele reaproveita os objetos e esconde, no fim do frame, o que não foi pedido. Assim, uma tela fechada desaparece sem código extra. Cada `GUI.depth` vira um Canvas com sua ordem de desenho.

As telas em pixel art são montadas em uma grade de 320x180 e ampliadas por um número inteiro, o que mantém os pixels nítidos.

| Tela | Script | Mouse |
|---|---|---|
| Menu principal | `MainMenu` | Passar por cima seleciona, clicar confirma |
| Pausa e volumes | `PauseScreen` | Botões e barras de volume |
| Morte | `DeathScreen` | Botões |
| Loja do Crisol | `GameHud` e `Crisol` | Itens, comprar e sair |
| Fim de jogo | `EndScreen` | Botão |
| HUD e inventário | `GameHud` | Dica ao passar sobre o item |
| Carta de habilidade | `AbilityCard` | Fecha pelo teclado ou controle |
| Aviso de salvo | `SaveEngraving` | Sem interação |

### Diálogos e textos

Não há arquivo externo de diálogos. Os textos ficam em arrays de `string` no código:

- **NPCs e documentos:** nos dados de cada fase, como `Phase01Layout`. O gerador grava esses textos na cena, então **alterar uma fala exige gerar a fase de novo**.
- **Falas de chefes e cenas:** no script de quem fala, como `SilverKing`.

Há dois caminhos até a caixa de diálogo:

| Sistema | Quando acontece | Como funciona |
|---|---|---|
| `Npc` | O jogador aperta `E` perto de alguém | Escolhe as falas, trava o Scrap e avança a cada aperto |
| `ScriptedDialogue` | Uma cena inicia a fala | Corrotina aguardada com `yield return` |

`Npc.EscolherFalas()` é virtual, então cada personagem decide o que dizer conforme o progresso. O `FerrolhoNpc`, por exemplo, muda a conversa antes da purificação, com o motor na mão e depois da instalação. `AoTerminarConversa()` entrega recompensas ao final.

O `GameHud` desenha a caixa com nome, fala e retrato. `SpeakerPortraits` encontra o busto e a cor pelo nome de quem fala. Documentos usam o mesmo componente `Npc`, com o verbo "ler" e uma imagem grande na tela. Essas imagens são geradas pelo kit `Docs`, com letras baseadas nas fontes vetoriais Hershey.

### Salvamento

O jogo salva **somente nas Bancadas de Ourives**, ao apertar `E`. O arquivo é `save.json`, em `Application.persistentDataPath`.

```text
Workbench.Salvar()
  → recupera a vida e define o ponto de renascimento
  → SaveEngraving mostra a gravura
  → GameManager.SalvarJogo()
  → SaveSystem.Salvar()
  → grava save.json.tmp e o troca pelo save.json
```

O arquivo guarda versão, cena, posição da bancada, data e uma lista de entradas de progresso.

O progresso fica em 29 campos estáticos marcados com um atributo:

```csharp
[SaveProgress("_played")]
private static bool _tocado;
```

O `SaveSystem` encontra esses campos por Reflection. Um sistema novo só precisa marcar o campo para entrar no save. São aceitos `bool`, `int`, `HashSet<string>` e `Dictionary<string, int>`. A chave é `Classe.chaveDoAtributo`, e **a chave não deve mudar**, para os saves antigos continuarem válidos.

São salvos: habilidades, vida extra, fragmentos, gemas, itens, compras, portões abertos, barreiras quebradas, áreas purificadas, chefes derrotados e cenas já vistas.

- **Continuar:** zera a memória, aplica o save, abre a cena e o `SceneArrival` coloca o Scrap na bancada.
- **Novo jogo:** zera a memória, mas mantém o arquivo até o próximo salvamento.
- **Morrer:** não grava nada. O Scrap volta à última bancada, e inimigos e chefes em luta são reiniciados pelo evento `ScrapHealth.Morreu`.

### Troca de fase

A `SceneExit` pede a troca ao `GameManager`, que chama o `SceneLoader`. Ele fecha a tela com LeanTween, carrega a cena com `LoadSceneAsync` e abre a tela de novo. O `SceneArrival` mantém o Scrap parado até o chão existir, evitando que ele atravesse o terreno enquanto os colisores são montados.

### Geração de conteúdo

Boa parte do jogo é produzida por ferramentas do Editor, no menu **Echoes**:

- **Cenas:** `PhaseXXGenerator` monta cada fase a partir dos dados de layout e valida o mapa.
- **Arte:** classes como `CrawlerArt` e `ColetorArt` desenham pixel art por código e gravam PNGs.
- **Animators e prefabs:** são criados pelos comandos do menu.
- **Cutscenes:** `Film` e `FilmCutscene` compõem planos com camadas, câmera, parallax e partículas.

## Créditos de terceiros

- Música: Scott Buckley (scottbuckley.com.au), licença CC BY 4.0
- Ambiente: fontes CC0 do OpenGameArt. Detalhes em `Assets/Common/Audio/CREDITOS.md`
- Cave Platformer Tileset
- 2D Dungeon Tilemap
