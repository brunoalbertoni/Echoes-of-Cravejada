# Trilha sonora e sons de ambiente

Data: 2026-11-04

> **[usuário]** = decidido pelo usuário. **[minha decisão]** = escolha minha, que o usuário pode revisar.
> O usuário deixou a escolha das músicas comigo **[usuário: "deixo você escolher as músicas"]**.

## 1. Intenção

Mais imersão: cada mapa soa como o lugar que ele é **[usuário]**. Música que combina com o ambiente, música
própria em cada luta de chefe **[usuário]**, sons do próprio lugar tocando por baixo **[usuário]**. A última fase
tem que ser caprichada, "algo lindo" **[usuário]**.

O jogo não tinha nenhum som.

## 2. Metas, não-metas e anti-metas

**Metas**
- Músicas gratuitas prontas **[usuário]**, liberadas pra uso em jogo.
- Um tema de exploração por fase; um tema por chefe; troca suave entre eles.
- Sons de ambiente por fase (uma "cama" contínua) e alguns sons presos a coisas do mapa (vapor, caldeira, ácido,
  cristal), que crescem quando o Scrap chega perto.
- Fase 5: o Rei tem duas músicas, uma por fase da luta; a transformação acontece em silêncio de música; o final
  (O Grande Polimento) tem a música mais bonita do jogo, que segue no cartão de FIM.

**Não-metas (nesta etapa)** **[minha decisão, usuário concordou]**
- Efeitos de ação (pulo, golpe, dano, ataques de chefe, interface).
- Menu de volume.

**Anti-metas**
- A música atrapalhar o barulho da fase 3, que é mecânica do jogo: lá tudo fica mais baixo.
- Silêncio quebrado por cortes secos: toda troca é em fade.

**Mudança no que foi combinado:** o Final B (O Grande Silêncio) não existe mais no jogo; só o Final A.

## 3. As músicas **[minha decisão]**

Quase tudo de um compositor só, pra trilha ter uma voz: **Scott Buckley** (scottbuckley.com.au), orquestral de
cinema, licença **CC BY 4.0** (pede crédito). Crédito no cartão de FIM e em `Assets/Common/Audio/CREDITOS.md`.
Convertidas de MP3 pra OGG (metade do tamanho), sem outra alteração.

| Momento | Música | Por quê |
|---|---|---|
| Fase 1, ferro-velho | Signal to Noise | cello e piano, sombria mas com esperança; abre o jogo |
| Fase 2, tubulações | Horizons | piano e cordas sobre um arpejo que empurra, como máquina |
| Fase 3, geodos | Decoherence | ambiente assombrado, cordas graves; quase fundo do mar |
| Fase 4, ateliês | Hiraeth | "saudade de um lar que não se pode voltar": é de onde o Scrap veio |
| Fase 5, palácio | Passage of Time | solene, cresce até um final régio |
| Fase 5, depois do Rei | Monomyth – The Restoration | piano contido, a calma voltando |
| Coletor (fase 1) | Juggernaut | cordas pesadas e metais, "forjando aço" |
| Mestre das Caldeiras | Catalyst | sintetizador escuro e orquestra crescendo |
| Fuga do ácido (fase 2) | Escape Velocity | alta energia |
| O Capataz (fase 3) | Monomyth – The Encounter | ameaça que cresce, termina sem resolver |
| A Broca (fase 3) | Simulacra | ação híbrida, guitarras "como máquina" |
| Alquimista de Mercúrio | Hour of the Witch | "fermentando poções" |
| Rei de Prata, fase 1 | Goliath | batalha épica com coro |
| Rei de Prata, fase 2 | Eyes in the Void | sombria, caótica, marcha gótica: o Óxido |
| Purificação (todas) | Hymn to the Dawn | sol nascendo num mundo novo |
| Final A | Echoes | cello solo, clímax emocionado; e tem o nome do jogo |

Pontos de início e de volta do loop de cada faixa foram escolhidos pelo envelope de volume (as de chefe começam
onde a energia já está alta).

## 4. Regras de troca

Prioridade, da mais alta pra mais baixa:
1. **Final** começou → Echoes, uma vez, sem loop; o ambiente some devagar.
2. **Purificação** começou na cena → Hymn to the Dawn até sair da cena (ou até ela acabar).
3. **Chefe em luta** → o tema dele. Começa quando ele acorda (no Rei, quando a cena de entrada acaba). Se o Scrap
   morre, volta a exploração; ao acordar de novo, o tema recomeça do início. Quando o chefe cai, a música some
   (a cena de queda fica com o ambiente).
   - Rei: na transformação a música da fase 1 some; a da fase 2 entra quando ele se ergue.
4. **Fuga do ácido** (fase 2) → Escape Velocity, até purificar ou morrer.
5. **Exploração** → tema da fase; na fase 5, depois do Rei, The Restoration.

Toda troca é com fade. Ao abrir uma cena com cutscene rodando (o despertar da fase 1), a exploração espera a
cutscene acabar.

## 5. Sons de ambiente **[minha decisão]**

Fontes CC0 do OpenGameArt (sem crédito obrigatório, mas listadas nos créditos), misturadas e emendadas em loops por
fase, mais detalhes sintetizados onde não havia gravação (batidas de metal, chiado, brilho de cristal).

- Fase 1: vento entre o ferro, metal batendo longe.
- Fase 2: caldeira, motor a vapor, canos batendo, chiado.
- Fase 3: vento grave de caverna e gotas, brilho de cristal raro (baixo).
- Fase 4: borbulhas, fervura, vidro tilintando.
- Fase 5: ar de salão grande, zumbido grave, ressonância de prata distante.

Sons presos a objetos (volume pela distância até a câmera): `SteamVent` (chiado, só com vapor saindo),
`PressureBoiler` (ronco), `AcidPool` (borbulha), `ResonantCrystal` (zumbido de cristal).

## 6. Como fica no código **[minha decisão]**

- `Assets/Common/Audio/`: `SoundDirector` (criado sozinho ao dar Play, atravessa as cenas), `SoundtrackCatalog`
  (a tabela: cena → tema, chefe → tema, volumes, pontos de loop), `AmbientEmitter`.
- Áudio em `Assets/Common/Audio/Resources/Soundtrack/{Music,Ambience}` (carregado por nome; música em streaming).
- `Assets/Common/Editor/SoundtrackImport.cs` acerta a importação (streaming pra música, loop pra ambiente).
- Mudanças mínimas fora dali: cada chefe ganha `IsDown`; `FinaleDirector` ganha `Running` e o crédito no cartão.
- Nenhuma cena precisa ser regerada.

## 7. Restrições

Sem commit (o usuário sobe). Compilar pelo terminal antes de entregar. Músicas cabem no limite de arquivo do
GitHub (todas abaixo de 20 MB em OGG).
