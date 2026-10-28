# Phase 2 Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reorganizar a Phase 2 em entrada → Praça/Cisterna (centro) → três alas livres → Escadaria das 3 Travas → boss → fuga → purificação com atalho de volta, mantendo os mesmos elementos.

**Architecture:** Tudo vive em `Assets/Editor/Phase02Layout.cs` (uma função por sala, sem UnityEngine). O bloco do boss (Sala de Caldeiras, Fuga, Saída) é espelhado em `x' = 359 - x` para que a entrada fique a oeste, junto da Escadaria; a Galeria Alta sobe `+20` e vai `-17` em x. Um script novo de runtime avisa no HUD quando uma trava abre.

**Tech Stack:** C# (Unity 2D, sem dependências novas), validação fora do Editor com um projeto `net9.0` que compila os `.cs` de layout, prévia em Python/PIL.

**Spec:** `docs/superpowers/specs/2026-10-27-phase02-redesign-design.md`

## Global Constraints

- Exatamente 4 gemas: `D2_Attic_Gem`, `D2_Chain_Gem`, `D2_Gallery_Gem` e a do encanador (`GemReward = 1`).
- Atributos de inimigos e boss inalterados (`Phase02Mechanics.cs` não muda os `Configure`).
- `SceneName`, `FromDistrict1SpawnId`, `SpawnTile (15,6)`, saída para `Phase03Layout.FromDistrict2SpawnId`, `BossGate`, `RisingAcidPool` mantêm o nome.
- Sem commit nem push (o usuário sobe).

## Review Focus

- Jogador que faz uma ala só e tenta o boss: tem que bater na trava, não achar um caminho por fora → checagem "boss inalcançável sem cada ala".
- Jogador que volta à Praça depois da fuga com o ácido ainda alto no poço → checagem de volta com o poço cheio de ácido.
- Jogador que desce pela Escadaria (travas abertas) ou pelo atalho da purificação → coberto pela mesma checagem de volta.
- Plataformas de patrulha mexidas (buracos novos no chão da Praça e da Casa de Máquinas) → checagem existente "anda por chão que não é chão".
- Ordem livre: ala que dependa de válvula de outra → checagem "cada ala completa sozinha a partir do centro".

---

### Task 1: Validação nova (falha antes do redesenho)

**Files:** Modify `Assets/Editor/Phase02Layout.cs` (tipos, `Play`, `Validate`, `CheckArena`)

- [ ] `ValveSpec.Lock` (bool) e `BoilerSpec.Group` (string). `Wings`: `Alta = {B_MachineHall, C_GaugeAttic}`, `Baixa = {A_Sewer, D_Depot, E_ValveTower}`, `Leste = {06_SteamChain, 07_ValveMaze}`.
- [ ] `Play(..., Func<string,bool> allowGroup = null)`: válvula/caldeira só gira/explode se `allowGroup(group)`.
- [ ] Checagens: (a) para cada ala, sem as outras duas, todas as válvulas dela giram; (b) para cada ala, sem ela, o boss não é alcançado; (c) exatamente 3 válvulas `Lock`, uma por ala; (d) com o poço da fuga cheio de ácido até `50 + RisingAcidHeight`, do topo (`District3Tile`) se chega na Solda e em Crisol; (e) 4 gemas no total.
- [ ] Rodar o projeto de validação: esperado FALHA (ainda não há travas).

### Task 2: Centro — Cisterna, cofre do encanador, Praça, Escadaria, Pré-boss

- Cisterna: bancada x100, alçapão x104–105, Crisol (109,19), caldeira `Boiler_PlumberVault` em (115,19) parada em (113,19), parede rachada x121 y19–21, cofre x122–128 y19–23, encanador (125,19).
- Praça: elevador da ala Alta `Lift_ToMachineHall` x101–103 y31–50 (sempre ligado); `Gate_SewerHatch` x147–149 y30; mirante x150–152 removido; inimigos 2 welders + 1 sanguessuga.
- Escadaria: carve x139–143 y44–55; `Lift_LockStair` x139–140 y31–58; `Trava_Alta` y46, `Trava_Baixa` y50, `Trava_Leste` y54 (x139–143, MoveX 1).
- Pré-boss: sala x139–157 y56–63, bancada x150, passagem para a arena x158–160 y56–60.

### Task 3: Bloco do boss espelhado + Galeria Alta + atalho

- Arena x161–207, `BossTile (178,56)`, `BossRange (164,205)`, `ArenaSeal x160`, `BossGate x209`, válvulas da arena em 164/204/184.
- Fuga x211–221 y50–99, `RisingAcidTrigger (208..217, 56..60)`, cano x211–217 y95 no topo.
- Saída x222–249 y96–104, toco x250–252, `District3Tile (251,96)`, `PurifyArea (223..231, 96..100)`.
- Galeria Alta x133–209 y96–110, névoa x210 y96–99, `Gate_PurifiedChute` (146..147, 95) e poço x146–147 y64–94 até o pré-boss. `Height = 113`.

### Task 4: Ala Alta

- `Valve_Hall_1` → `Lift_Hall_2`; `Valve_Attic` → `Lift_Hall_3` (agora começa desligado) e `Lift_ToAttic`; `Valve_Hall_2` (Lock) → `Gate_HallHatch` e `Trava_Alta`.
- Cofre de 30 no nicho oeste do andar 3; furo do sótão para a Galeria removido; `Lift_ToMachineHall` sai da Válvula Emperrada.

### Task 5: Ala Baixa

- Esgoto com ilhas variadas; túnel x151–209 y6–9 até o Depósito; túnel Depósito↔lago, furo e elevador para o Labirinto removidos.
- Torre: só a entrada da base; `Lift_Tower_3` sempre ligado; `Valve_Tower_3` (Lock) no topo → `Trava_Baixa`, `Gate_SewerHatch`, `Lift_Sewer_Up` (agora começa desligado).
- Página 4 no Depósito; cofre de 30 sai do Esgoto.

### Task 6: Ala Leste

- Elevador do Labirinto encurtado (y35–46) até um nicho x227–232 y44–47 com o cofre de 40; `Valve_Maze_C` (Lock) → elevador e `Trava_Leste`; túnel para a Torre removido.
- Inimigos extras: welder, sentinela e sanguessuga no Labirinto; sentinela e bloater na Cadeia.

### Task 7: Aviso de trava no HUD

**Files:** Create `Assets/Scripts/LockValveNotice.cs`; Modify `Assets/Editor/Phase02Mechanics.cs` (`BuildValves`)

- [ ] `LockValveNotice : MonoBehaviour` escuta `Valve.Turned` e chama `HudNotice.Show($"Uma trava da Escadaria se abriu ({n}/3)")`; contador estático zerado em `SubsystemRegistration`.
- [ ] Em `BuildValves`, `if (spec.Lock) valveObject.AddComponent<LockValveNotice>()`.
- [ ] Compilar `Assets/Scripts` + `Assets/Editor` com o Roslyn do Unity (ver memória): 0 erros.

### Task 8: Verificação

- [ ] `Validate()` com 0 problemas; `Walkthrough()` e `UnreachableFloors()` revisados.
- [ ] Prévia do mapa renderizada e conferida.
