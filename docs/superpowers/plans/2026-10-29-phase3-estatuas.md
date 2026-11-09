# Phase 3 — Estátuas, placa de palavras e escombros: plano de implementação

> Execução: nativa, nesta sessão (o usuário pediu "pode implementar"). Sem commits: o usuário sobe as alterações.

**Goal:** implementar o design em `docs/superpowers/specs/2026-10-29-phase3-estatuas-design.md`.

**Architecture:** layout puro (`Phase03Layout`, sem UnityEngine) ganha estátuas, rachaduras e a porta "oculta";
o gerador (`Phase03Generator`) monta os objetos; dois scripts de jogo novos (`StatueButton`, `StatueHall`);
arte procedural nova em `StatueArt` + `RubbleArt` refeito.

**Tech Stack:** Unity 6 / C#; verificação por compilação Roslyn fora do Editor e `Phase03Layout.Validate()` num console net9.0.

**Spec:** `docs/superpowers/specs/2026-10-29-phase3-estatuas-design.md`

## Global Constraints
- Sem commit/push.
- Layout sem dependência de UnityEngine.
- Ordem das estátuas = ordem das gravuras: Mineiro(1), Rei(2), Broca(3), Coração(4). Na sala, da esquerda pra direita: Broca, Mineiro, Coração, Rei.
- Sequência da porta do mini boss: 3,1,2,1,3. Placa: "GELO · RUBI · MEL · SANGUE · CÉU".
- Botão: E/K/botão de cima, de pé. Ruído do botão: raio 12, força 0.9.
- Mites nascidos: acerto 1, erro 2, no máximo 4 vivos.
- Dash: `dashRadius` 14 → 16.

## Review Focus
1. Morrer com mites nascidos vivos: `Damageable.Revive` reativa todos — o salão precisa desligá-los de novo e zerar a sequência.
2. Apertar a mesma estátua duas vezes seguidas: conta como erro (ela já foi).
3. Apertar depois da porta aberta, ou depois da purificação: não faz nada (nem nasce bicho).
4. Duas interações no mesmo lugar (lore/E perto da estátua): ninguém de lore fica a menos de 2 tiles de uma estátua.
5. Porta oculta não pode tocar prévia nem pintar as gemas com a cor da nota.

---

### Task 1: Layout
**Files:** Modify `Assets/Editor/Phase03Layout.cs`
- `StatueSpot { Name, Kind (0 Mineiro..3 Coração), Order (1..4), X, Y }`, lista `Statues`; `SpawnCracks` (lista de (X,Y)); `StatueGate = "Porta_Campanario"`; `MainDoor = "Statues_Belfry"`.
- Campanário: sai Resonant_High_*/Door_High; entram 4 estátuas em (144,100) Broca, (148,104) Mineiro, (156,108) Coração, (162,104) Rei; recompensa vai pra (165,104); rachaduras em (146,100) e (164,100).
- `DoorSpec.Hidden`; `Door_East` com Sequence {3,1,2,1,3}, Hidden, Glyphs sobre o portão; placa `Lore_WordPlaque` (kind "silverplaque") em (195,52).
- Chegada: rocha de apoio `Solid(10,14,11,21)` à direita dos escombros; placa de enfeite (10,16) → (13,16).
- `Play()`: abre `StatueGate` quando todas as estátuas se alcançam (respeitando `allowDoor(MainDoor)`); `Validate()`: estátuas alcançáveis, sem elas a broca fica fora de alcance.
- Verificar: console net9.0 rodando `Validate()` → 0 problemas.

### Task 2: Scripts
**Files:** Create `Assets/Scripts/StatueButton.cs`, `Assets/Scripts/StatueHall.cs`; Modify `HarmonicDoor.cs`, `Gate.cs`, `GameHud.cs`, `ScrapNoise.cs`, `NoiseSystem.cs`
- `StatueButton.Configure(int order, SpriteRenderer button, SpriteRenderer glow, Light2D light)`; `static StatueButton Nearby`; `event Action<StatueButton> Pressed`; `SetLit(bool)`; `Press()` afunda o botão.
- `StatueHall.Configure(StatueButton[] statues, Gate gate, Transform[] cracks, SpriteRenderer[] crackArt, Sprite[] crackFrames, GameObject[] pool)`; constantes 1/2/4; zera em `ScrapHealth.Died` (no frame seguinte).
- `HarmonicDoor.Configure(..., bool hidden)`: sem prévia quando oculta.
- `Gate.Open()` emite ruído (raio 8, força 0.6).
- `GameHud`: sai o medidor; entra o aviso "Aperte E para acionar a estátua".
- `ScrapNoise.dashRadius = 16`. `NoiseSystem.LastLoudness/LastTime` saem (só o HUD usava).

### Task 3: Arte
**Files:** Create `Assets/Editor/StatueArt.cs`; Modify `Assets/Editor/RubbleArt.cs`
- `StatueArt.Statue(int kind)` 32x48, `StatueArt.StatueGlow(int kind)`, `StatueArt.Button(bool pressed)`, `StatueArt.Crack(int frame)` (0..2), `StatueArt.SilverPlaque()`, `RubbleArt.Pebbles()`.
- `RubbleArt.ElevatorRubble` refeito: viga torta entalada embaixo, pilha em cima, trilho dobrado, cabos pendurados.
- Verificar: despejar PNGs num console e olhar.

### Task 4: Gerador
**Files:** Modify `Assets/Editor/Phase03Generator.cs`
- `EnsureArt` grava as novas; `BuildStatues` (estátuas, rachaduras, pool de 4 mites desligados dentro de Enemies, `StatueHall`); `BuildDoors` passa `Hidden` e pinta gemas ocultas de branco; `BuildLore` desenha "silverplaque"; pedrinhas no chão da estação.
- Verificar: compilar `Assets/Scripts` e `Assets/Editor` com Roslyn sem erros.
