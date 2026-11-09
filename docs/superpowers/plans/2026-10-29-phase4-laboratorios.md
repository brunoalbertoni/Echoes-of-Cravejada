# Phase 4 — Laboratórios Nobres: plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refazer a Phase 4 inteira como região de laboratórios: mapa novo com centro e três alas, reagentes que abrem o Grande Laboratório, o Alquimista de Mercúrio, a fuga do Guarda, a Cerimônia com purificação e arte própria para tudo.

**Architecture:** Igual às fases 2 e 3: `Phase04Layout.cs` (sem UnityEngine) descreve o mapa e valida o progresso; `Phase04Generator.cs` monta a cena; classes `*Art` desenham a arte em `PixelCanvas`; comportamentos novos em `Assets/Scripts`.

**Tech Stack:** Unity 6 / C#, URP 2D, geradores de Editor. Verificação fora do Editor: Roslyn `csc` com as referências do `.csproj`, e um console `net9.0` que roda `Phase04Layout.Validate()` e despeja o mapa e a arte em PNG.

**Spec:** `docs/superpowers/specs/2026-10-29-phase4-laboratorios-design.md`

## Global Constraints

- `Phase04Layout.SceneName = "Phase04_AlchemicalAteliers"` e `FromDistrict3SpawnId = "FromDistrict3"` não mudam.
- `Phase04Layout.cs` e toda classe `*Art` não dependem de UnityEngine.
- Chefe e Guarda: folha única com `Pose`, 32 px/tile, gravada com `Phase02Assets.WriteSheet`, tocada por `Play(start, count, fps, loop)` + `LateUpdate`.
- Inimigos: `MotionFrames` (parado + andando) e `DeathFrames` (morte). Nenhum sprite parado.
- Legíveis: arte em `LoreArt.Phase04` ligada por `LoreArtAssets.Apply`.
- Recompensas: só o reforço de chassi (`RewardPickup.Kind.Chassis`, id `D4_Secret_Chassis`).
- Sem forja. Sem commit nem push.

## Review Focus

1. Morrer no meio da luta ou da fuga: o chefe volta inteiro e a fuga recomeça desde o pré-boss, sem o Scrap ficar travado (`ControlLocked`) nem preso na redoma.
2. Sair do círculo da arena no meio do polimento: o progresso zera e o controle volta.
3. Reagente pego e Scrap morre: o reagente continua no bolso (como a chave da fase 3).
4. Voltar à cena já purificada: Guarda e Alquimista não reaparecem; o círculo do palco fica aceso; Assistente e Crisol purificados estão lá.
5. Fazer as alas em qualquer ordem: o validador confere cada ala sozinha, a partir do centro.

---

### Task 1: Mapa novo e validação (`Phase04Layout.cs`)

**Files:**
- Rewrite: `Assets/Editor/Phase04Layout.cs`
- Create (scratchpad): console `net9.0` que compila `LevelMap.cs`, `PixelCanvas.cs`, `Phase01Layout.cs`, `BrassArt.cs`, `Phase04Layout.cs` e imprime `Validate()` + o mapa em PNG.

**Interfaces (Produces):**
- Tipos mantidos: `Cells`, `Pool`, `BlockKind`, `Block`, `Circle`, `Mirror`, `FoeKind`, `Foe`, `LoreSpot`, `Marker`.
- `LoreSpot.Kind` novos: `"diary"`, `"portrait0..2"`, `"sheet"`, `"protocard"`, `"chalkboard"`, `"sign"`, `"assistant"`.
- Listas: `Rooms, Pools, Blocks, Circles, Mirrors, Foes, Lore, Purified, Markers, VaporLifts, Drips, Props, Workbenches, Lamps`.
- `Props`: `List<(string Sprite, float X, float Y, int Order)>` — nomes de sprite do Task 2.
- Puzzles de luz: `LightPuzzles: List<LightPuzzle>` com `Name, Emitter (X,Y,DirX,DirY), Mirrors (nomes), Lock (X,Y), Gate (X,Y,Height)`.
- Reagentes: `Reagents: List<(string Id, string Name, int X, int Y)>`; prefixo `ReagentPrefix = "D4_Reagent_"`; ids `D4_Reagent_Mercury`, `D4_Reagent_Light`, `D4_Reagent_Salt`.
- `AlembicDoor => (X, Y, Height)` (portão) e `AlembicTile` (onde se entrega).
- Arena: `ArenaRoom (X0,Y0,X1,Y1)`, `BossTile`, `BossWake (X,Y,W,H)`, `ArenaFloorY`, `ArenaCeilingY`, `ArenaCircles: List<(string Name,int X,int Y)>`, `ArenaFloodPools: List<Cells>` (poças da segunda metade), `ArenaIslands` (ressaltos dos círculos na segunda metade), `DrainTile`.
- Fuga: `GuardWake (X,Y,W,H)`, `GuardPath: List<(float X, float Y)>`, `GuardStart`.
- Cerimônia: `StageCircle (X,Y)`, `ElevatorCar (X,Y,Width)`, `ElevatorTopY`.
- `SecretChassisTile`, `CrisolTile`, `CrisolLines`, `CrisolPurifiedLines`, `SpawnTile`.

- [ ] **Step 1:** Montar o console de validação no scratchpad (net9.0, `<Compile Include>` direto nos `.cs`). Rodar com o layout atual: deve compilar e imprimir os problemas atuais.
- [ ] **Step 2:** Reescrever o layout: entrada → centro (Ateliê de Crisol + porta do alambique) → Destilaria (poças, círculos, vapor até o chassi, reagente) / Óptica (dois enigmas de espelho, reagente) / Incubação (tanques, Oficina de Protótipos, Assistente, reagente) → pré-boss com bancada → arena → corredor da fuga → anfiteatro → elevador. Uma sala por função; props de laboratório em toda sala.
- [ ] **Step 3:** `Validate()` confere, em ordem: sem nada, só o centro e o começo das alas; cada ala sozinha a partir do centro chega ao seu reagente (círculos/espelhos da própria ala liberados); sem os três reagentes a arena é inalcançável; com eles, o chefe; com o chefe caído, o caminho `GuardPath` é andável e o palco é alcançável; o chassi só com `Circle_Vapor`; marcadores, inimigos e leituras em pé; raios sem terreno no meio. Também: patrulhas no chão inteiro (`patrulhas-usam-chao-inteiro`).
- [ ] **Step 4:** Rodar o console: `0 problema(s)`. Gerar `phase04_map.png` e conferir as salas a olho.

### Task 2: Ambientação de laboratório (`LabArt.cs`)

**Files:** Create `Assets/Editor/LabArt.cs`

**Interfaces (Produces):** métodos estáticos `PixelCanvas` sem parâmetros (ou com variante int), nomes de sprite `lab_*`:
`Alembic, Retort, Burner, BellJarProp, SpecimenJar(int), Chalkboard(int), DissectionTable, TubeRack, ReagentShelf, Cage(bool broken), CultureTank(int subject), Chandelier, PrototypeTable(int n 1..6), Lectern, Benches(amphitheater), AlembicDoor(int lit 0..3), Reagent(int 0..2), Drain, StageCircle`.
`LabArt.SpriteNames` lista os nomes + a função de desenho, para o gerador gravar tudo num laço.

- [ ] **Step 1:** Desenhar as peças (paleta mármore/prata/vidro/latão, líquidos coloridos, verde do Óxido nos corrompidos).
- [ ] **Step 2:** Despejar todas em PNG pelo console e conferir legibilidade lado a lado com `BrassDecorArt`/`GeodeArt`.

### Task 3: Inimigos animados (`LabFoeArt.cs` + scripts)

**Files:** Create `Assets/Editor/LabFoeArt.cs`; Modify `Assets/Scripts/QuicksilverSlime.cs`, `FlaskThrower.cs`, `FailedExperiment.cs`, `TransmutedSentinel.cs` (só para chamar `MotionFrames.Hold` na pose de ataque, quando houver).

**Interfaces (Produces):** `LabFoeArt.IdleFrames = 2`, `MoveFrames = 4`, `DeathFrames = 5`; `Slime(bool move, int frame, bool small)`, `SlimeDeath(int, bool small)`, `Thrower(bool move,int)`, `ThrowerThrow()`, `ThrowerDeath(int)`, `Failed(bool move,int)`, `FailedCharge()`, `FailedDeath(int)`, `Sentinel(bool move,int,bool silver)`, `SentinelDeath(int)`, `Assistant(int frame)`, `AssistantHiding()`.
Os scripts ganham `ConfigurePose(Sprite attackPose)` opcional.

- [ ] **Step 1:** Desenhar, despejar em PNG e conferir.
- [ ] **Step 2:** Ligar `Hold` nas poses de ataque; compilar `Assets/Scripts`.

### Task 4: Leituras (`LoreArt.Phase04.cs`)

**Files:** Create `Assets/Editor/LoreArt.Phase04.cs`

**Interfaces (Produces):** `LoreArt.Phase04 : IReadOnlyDictionary<string, Piece>` por nome de LoreSpot. Toda folha de diário = mesmo papel (`lab_diary`); todo retrato = mesma moldura; fichas 1–6 = mesmo cartão; ficha do n.º 7 = prancheta própria; quadros-negros = mesmo quadro.

- [ ] **Step 1:** Desenhos de mapa + desenho grande com o texto (`PixelFont`).
- [ ] **Step 2:** Despejar em PNG e conferir o texto legível.

### Task 5: O Alquimista de Mercúrio

**Files:** Create `Assets/Editor/AlchemistArt.cs`, `Assets/Scripts/MercuryAlchemist.cs`, `Assets/Scripts/ArenaCircle.cs`, `Assets/Scripts/BellJar.cs`.

**Interfaces (Produces):**
- `AlchemistArt`: `Width=192, Height=160, PixelsPerUnit=32`, constantes de sequência `Idle, Glide, BellCall, Distill (evapora), Condense, WhipWindup, Whip, Silver (estátua), BreakFree, Smother (apaga círculo), Hurt, Death (escorre pelo ralo)`, `FrameCount`, `Frames()`.
- `ArenaCircle : MonoBehaviour` — `Configure(SpriteRenderer rune, float polishTime)`, `bool Usable`, `void Smother(float seconds)`, `event Action<ArenaCircle> Polished`. Polir: segurar E de pé dentro; apanhar zera; sair zera e devolve o controle.
- `BellJar : MonoBehaviour, IDamageable` — `Configure(float ceilingY, float floorY, Sprite glass, Sprite cracked)`, `Drop(float x)`: sombra no chão 0,9 s, cai; se pegar o Scrap: 1 de dano, trava o controle, 3 golpes quebram.
- `MercuryAlchemist : MonoBehaviour, IDamageable` — `Configure(Rect wake, float minX, float maxX, float floorY, float ceilingY, ArenaCircle[] circles, BellJar jar, GameObject[] flood, GameObject[] islands, Sprite drop, Gate exit, GameObject guardWake)`, `ConfigureFrames(Sprite[] frames)`. Líquido: `Damage` só espirra. Prata (4 s): leva dano. Vida 30; metade → inundação. Morte: anima o ralo, abre `exit`, ativa `guardWake`. Morte do Scrap: volta inteiro e dorme.

- [ ] **Step 1:** Arte em folha; despejar e comparar com `BoilerMasterArt`/`CaveMinerArt` lado a lado (mesma escala e densidade).
- [ ] **Step 2:** Scripts; compilar.

### Task 6: O Guarda, a Cerimônia e a porta dos reagentes

**Files:** Create `Assets/Editor/GuardArt.cs`, `Assets/Scripts/SilverGuard.cs`, `Assets/Scripts/AlembicDoor.cs`, `Assets/Scripts/CeremonyStage.cs`; Modify `Assets/Scripts/TransmutationCircle.cs` (evento `Lit`).

**Interfaces (Produces):**
- `GuardArt`: folha `Dormant, Wake, March, Reach, Dissolve`.
- `SilverGuard` — `Configure(Vector2[] path, Rect wake, float speed)`, `ConfigureFrames(Sprite[])`, `Dissolve()`. Não leva dano. Segue o caminho atrás do Scrap; encostar = 1 de dano e empurrão. Morte do Scrap: volta para o início e dorme até o Scrap entrar em `wake` de novo.
- `AlembicDoor` — `Configure(string[] ids, string[] names, Gate gate, SpriteRenderer body, Sprite[] litStages)`: perto, entrega os reagentes que tiver, acende, abre com três.
- `TransmutationCircle.Lit` (`event Action`).
- `CeremonyStage` — `Configure(TransmutationCircle circle, AreaPurifier purifier, SilverGuard guard)`: círculo aceso → `purifier.Purify(pos)` e `guard.Dissolve()`.

- [ ] **Step 1:** Arte do Guarda; despejar.
- [ ] **Step 2:** Scripts; compilar.

### Task 7: Gerador (`Phase04Generator.cs`)

**Files:** Rewrite `Assets/Editor/Phase04Generator.cs`

- [ ] **Step 1:** Montar a cena a partir do layout: tilemaps, luz, props, poças, blocos, vapor, círculos, enigmas de luz, reagentes (`KeyItemPickup`), porta do alambique, chassi, inimigos animados, bancadas, Crisol, Assistente, leituras com `LoreArtAssets.Apply`, purificação (`AreaPurifier`), chefe, Guarda, palco, elevador (`ElevatorRide` para `Phase05`, que mostra aviso se a cena não existir).
- [ ] **Step 2:** Compilar `Assets/Scripts` e `Assets/Editor` com o `csc` (memória `verificar-unity-sem-abrir-editor`): 0 erros.

### Task 8: Verificação final

- [ ] Console: `Validate()` com 0 problemas; prévia do mapa com props sobrepostos.
- [ ] Folhas do chefe e do Guarda lado a lado com o Mestre das Caldeiras.
- [ ] Relatório ao usuário: o que rodar no Unity (`Echoes > Gerar Phase 4`) e o que conferir jogando.
