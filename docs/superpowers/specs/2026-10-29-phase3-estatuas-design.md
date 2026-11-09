# Phase 3 (The Silent Geodes) — enigma das estátuas, placa de palavras e escombros

Data: 2026-10-29
Arquivos principais: `Assets/Editor/Phase03Layout.cs`, `Assets/Editor/Phase03Generator.cs`,
`Assets/Scripts/HarmonicDoor.cs`, `Assets/Scripts/GameHud.cs`, `Assets/Scripts/ScrapNoise.cs`,
`Assets/Editor/RubbleArt.cs` (+ arte nova).

> Marcação: **[usuário]** = decidido pelo usuário. **[minha decisão]** = decisão minha,
> que o usuário pode revisar. Sem marcação = consequência direta das duas.

## 1. Por quê

A fase 3 tem três quebra-cabeças que são o mesmo (golpear cristais na ordem que a porta
mostra com luzes). Isso cansa, e a resposta vem pronta. Os escombros do elevador são um
bloco solto no ar. O medidor de barulho no HUD transforma o som, que deveria ser
sentido, em uma barra para vigiar.

Objetivo: o som continua fundamental, mas como coisa do mundo (o que você faz atrai o
que mora no escuro), e os enigmas pedem **ler o mundo** em vez de decorar luzes.

## 2. Metas, não-metas e anti-metas

**Metas**
- O enigma das estátuas vira o enigma principal da fase **[usuário]**: abre o caminho até a broca.
- Apertar o botão de uma estátua faz barulho e atrai bichos, e alguns nascem na hora **[usuário]**.
- A ordem das estátuas vem da história gravada na Galeria das Gravuras, sem número nem luz **[usuário aprovou minha proposta]**.
- O enigma do mini boss (porta da Galeria dos Caçadores, que leva ao The Resonant) deixa de mostrar a ordem com luzes. No lugar, há uma placa com uma lista de palavras, e cada palavra remete a uma cor de cristal; as cores podem repetir **[usuário]**.
- O medidor de barulho sai do HUD **[usuário]**.
- O dash continua (ou fica ainda mais) barulhento; algumas interações específicas também são barulhentas **[usuário]**.
- Os escombros do elevador passam a fazer sentido físico: apoiados, entalados, com peso **[usuário]**.
- Arte nova para tudo o que for novo, no mesmo pipeline procedural das fases anteriores (classes `*Art` gerando PNG no Editor) **[usuário]**.

**Não-metas (nesta etapa)**
- Remodelar arte e animação dos inimigos (Echo Hunter, Crystal Mite, Shard Sentinel), do The Resonant e da Geode Drill. O usuário liberou, mas isso fica para uma **segunda etapa separada**, depois desta **[minha decisão]**.
- Mudar a luta da broca ou a do The Resonant.
- Mudar o layout fora das salas citadas abaixo.

**Anti-metas (falharia mesmo funcionando)**
- A fase ficar chata ou entediante **[usuário]**. Na prática: errar a ordem não pode custar uma caminhada longa de volta, e não pode haver espera parada.
- A ordem do enigma aparecer de graça (luz, número, gema colorida).
- Bichos nascendo sem limite até a sala virar impossível.

## 3. O enigma das estátuas (principal)

### Onde [minha decisão]
O **Campanário (sala 11_Belfry)** vira o **Salão das Estátuas**. Os quatro cristais
`Resonant_High_1..4` e a `Door_High` (sequência de cristais) saem. No lugar deles entram quatro
estátuas, em alturas diferentes nos ressaltos que já existem (chão em 100, ressaltos em 103,
107 e 103). A `Porta_Campanario` continua sendo o portão que abre o caminho para a antessala da
broca; quem a abre agora são as estátuas.

Motivo de ficarem todas numa sala só: errar não manda o jogador atravessar o mapa de novo, e o
barulho de cada aperto se acumula **na mesma sala**, então a tensão sobe a cada estátua
(primeiro aperto: um bicho; quarto aperto: a sala está cheia).

### As estátuas e a ordem
Quatro estátuas de prata/pedra, uma para cada gravura da Galeria das Gravuras (sala 06), que
já conta a escavação de leste para oeste:

| Ordem | Gravura (já existe)       | Estátua                                     |
|-------|---------------------------|---------------------------------------------|
| 1     | mineiros descendo         | **O Mineiro**: picareta no ombro, olhando para baixo |
| 2     | o Rei de Prata apontando  | **O Rei**: coroa, braço apontando para baixo |
| 3     | a broca em espiral        | **A Broca**: a máquina em espiral, sem mineiro na frente |
| 4     | o Coração de Quartzo      | **O Coração**: losango rachado, a fenda pintada de verde |

Na sala, as estátuas ficam **fora de ordem** no espaço **[minha decisão]**. Sugestão, da esquerda
para a direita: Broca, Mineiro, Coração, Rei.

**Pistas**:
- **Nenhuma placa no Salão das Estátuas apontando para a galeria [usuário]**. A ligação entre
  estátuas e gravuras fica para o jogador perceber sozinho.
- O texto das gravuras ganha ênfase leve de sequência ("A primeira...", "Depois...", "Então...",
  "Por fim..."). As linhas atuais já começam assim em parte; o ajuste fino é do builder.
- Quem não leu as gravuras ainda consegue resolver na tentativa e erro (são 24 ordens possíveis),
  mas cada erro custa bichos. É um incentivo a ler, não uma parede.

### Apertar o botão
- Cada estátua tem um botão (na base ou no peito). Aperta-se com **E** (interagir), de pé, perto,
  como as outras interações **[minha decisão]**. O cinzel não aperta o botão.
- Apertar faz um ruído forte via `NoiseSystem.Emit` (raio ~12 tiles, força ~0.9) **[minha decisão]**:
  a estátua range e trava com um clangor.
- A estátua apertada fica marcada (olhos ou fenda acendem numa luz fria e neutra, sem cor que
  denuncie ordem) enquanto a sequência estiver correta.
- **Acertou a próxima:** a estátua fica acesa e nasce uma leva de bichos (ver abaixo).
- **Errou:** todas as estátuas se apagam com um estalo seco, a sequência volta a zero e nasce
  uma leva maior **[minha decisão]**. Como em `HarmonicDoor`, se a estátua errada for a primeira
  da ordem, ela já conta como recomeço.
- **Completou as quatro:** a `Porta_Campanario` abre; os bichos que ainda estão vivos continuam (não somem).

### Os bichos que nascem [minha decisão]
- Nascem de **rachaduras** na parede/teto do Salão das Estátuas (2 a 3 pontos de nascimento, fora
  do alcance imediato do jogador e longe da bancada `Workbench_Belfry`).
- Tipo: **Crystal Mite** (já existe). Acerto: 1 mite. Erro: 2 mites.
- Teto de bichos nascidos vivos ao mesmo tempo: 4. Acima disso, o aperto ainda faz barulho, mas
  não nasce mais nada.
- O `Mite_Belfry_1` que já existe na sala pode ficar.
- Antes de nascer, a rachadura racha visivelmente e solta pó por ~0,6 s, para o jogador perceber.
- Ao morrer e voltar da bancada, os bichos nascidos somem e a sequência zera; a porta, se já
  aberta, continua aberta (salvo como as outras portas).

### Validação do mapa
O validador de `Phase03Layout` hoje trata `Door_High` como `MainDoor` e confere que sem ela não
se chega na broca. Isso continua valendo para o novo enigma: as quatro estátuas precisam ser
alcançáveis, e sem o enigma resolvido a broca fica fora de alcance. O lugar onde cada uma fica
precisa passar pelas regras de "lugar quieto" se tiver placa de lore ao lado.

## 4. O enigma do mini boss (placa de palavras)

Sala 08_HunterGallery, `Door_East`, que abre a `Porta_Ressonante` (caminho até o The Resonant).

**Muda** **[usuário]**:
- A porta não toca mais a sequência acendendo os cristais (`HarmonicDoor.Preview` desligado
  para esta porta).
- As gemas gravadas em cima da porta não mostram mais as cores da sequência **[minha decisão]**:
  ficam brancas/neutras e só acendem para mostrar **quantas** notas já foram acertadas.
- No lugar, uma **placa de prata** perto da porta, lida com E, com uma lista de palavras.

**Os cristais da sala** (já existem, ficam): nota 1 = vermelho-rosado, nota 2 = âmbar (corrompido,
precisa ser raspado), nota 3 = ciano. Hoje a sequência é 3-1-2.

**A placa** **[minha decisão, o texto final pode mudar]**: sequência nova de 5 notas, com
repetição: **3, 1, 2, 1, 3**.

> *"PARA ABRIR A CÂMARA, CANTE O TURNO:"*
> *GELO · RUBI · MEL · SANGUE · CÉU*

(gelo/céu = ciano, rubi/sangue = vermelho, mel = âmbar). Palavras diferentes para a mesma cor
fazem o jogador pensar sem ficar injusto.

A lista de turno que já existe na sala (`Lore_HunterRoster`) fica como está.

**A Câmara do Eco (07, `Door_West`) não muda** **[minha decisão]**: continua mostrando a ordem
com luzes. É opcional e é onde o jogador aprende a mecânica dos cristais; a placa de palavras
do mini boss é a evolução dela.

## 5. Os escombros do elevador

Hoje: um bloco 5x8 tiles (`ElevatorRubble`, de y=14 a 21) parado no ar acima da cabine.

**Novo [minha decisão sobre a forma]**: os escombros contam como o poço desabou.
- Uma **viga de latão torta**, atravessada de parede a parede no poço, entalada nas paredes.
  Ela é o que segura tudo.
- Em cima da viga, a pilha: blocos de rocha azulada, pedaços de cristal, um trilho do elevador
  arrancado e dobrado, cabos pendurados para baixo.
- Embaixo, sobre o teto da cabine e no chão da estação ao lado: pedras soltas e poeira caídas
  (só enfeite, sem colisão).
- Uma rachadura na parede do poço, de onde a pilha veio.
- A colisão continua sendo um bloco (a mesma área ou próxima), quebrado pelo Resonance Strike. Ao
  quebrar: a viga cede, a pilha cai em pedaços (efeito visual) e o poço fica livre.
- As regras de `CheckElevator` (os escombros tampam a largura toda; a onda alcança de dentro da
  cabine) continuam valendo.

## 6. Barulho

- Sai o medidor de ruído do HUD (`GameHud`, bloco de `_showNoise`) **[usuário]**.
  `NoiseSystem.LastLoudness/LastTime` podem ficar se outra coisa usar; se só o HUD usava, podem sair.
- **Dash:** continua o som mais forte; a distância até onde o barulho dele é ouvido (`dashRadius`
  em `ScrapNoise`) sobe de 14 para ~16 tiles **[minha decisão]**. A distância que o dash percorre
  não muda. Ganha um sopro de vapor visível (se já houver efeito, só reforça).
- **Interações barulhentas** **[minha decisão]**: botão das estátuas (acima), raspar veio de Óxido
  (já é), abrir portões de grade (um ruído médio ao abrir) e quebrar os escombros/barreiras com a onda.
- Os passos e pulos continuam silenciosos como hoje.

## 7. Arte

Tudo no pipeline atual: classes estáticas `*Art` no Editor desenhando em `PixelCanvas`, salvas
como sprite pelo gerador da fase. Paleta: a da fase (rocha azulada, cristais ametista/quartzo/fumê,
o verde do Óxido) **[usuário: "igual fez nas phases anteriores"]**.

Peças novas:
1. **Quatro estátuas** (Mineiro, Rei, Broca, Coração), ~2x3 tiles, com a mesma base. Dois estados:
   apagada e acesa (olhos/fenda brilhando). O botão visível na base, com um quadro afundado quando apertado.
2. **Rachaduras de nascimento** nas paredes: fechada, rachando (2–3 quadros), aberta.
3. **Placa de prata das palavras** (enfeite de parede, perto da `Porta_Ressonante`).
4. **Escombros novos**: viga, pilha, trilho dobrado, cabos, pedras soltas no chão; e os pedaços que caem ao quebrar.
5. **Gemas neutras** da porta do mini boss (variante sem cor).

## 8. O que fica para o builder
- Coordenadas exatas das estátuas, das rachaduras e da placa, desde que passem no validador.
- O tamanho exato em pixels e os detalhes de desenho, desde que leiam claramente como as quatro
  cenas das gravuras.
- Sons (se o projeto já tiver áudio para usar); senão, o ruído é só o do `NoiseSystem` + efeito visual.
- Como salvar o estado da porta das estátuas (seguir o que as outras portas já fazem).
- Ajustes finos de texto das gravuras e das placas.

## 8b. Revisão do usuário (depois da primeira implementação)
- A chegada é "menos habitada": saem o aviso do elevador e a placa do som. O capacete do mineiro fica.
- O Crisol não dá rubi na fase 3.
- A placa de prata tem **só** as palavras; embaixo, um bloco de notas pequeno com
  "muito chato ter que ficar andando de um lado pro outro pra abrir uma porta".
- As gravuras saem da galeria e ficam **lado a lado** numa sala nova, desabada: as Ruínas
  (07b), em cima da Câmara do Eco, no fim da ala oeste (escolha do usuário).
- Sem bancada na sala do mini boss nem no Salão das Estátuas.
- O Salão das Estátuas é **linear**: uma galeria baixa, estátuas no chão, que esconde o segredo (a porta oeste).
- O caminho principal sobe pouco pela abóbada, dá uma volta grande pela direita (corredor
  de baixo, poço do fundo, corredor de cima) e entra no salão **pela direita**.
- O poço do elevador sobe até o alto do mapa; a Câmara do Eco recuou pra não ficar no caminho
  dele (o nicho dela virou um alçapão no chão).

## 8c. Segunda revisão do usuário
- Sai o enigma das gemas da Câmara do Eco (a câmara vira só a subida pras Ruínas). Com isso,
  o único enigma de cristais que sobra é o do mini boss, sem gemas nem prévia.
- O distrito inteiro é infestado de bichos cegos (mites e caçadores), patrulhando o chão inteiro
  de cada trecho; só a gruta (bancada e Crisol) e a antessala ficam livres.
- Saem todos os sacos de fragmentos ("cofres") e o nicho secreto da chegada.
- Na placa de palavras, sem as gemas ("bolas") ao lado: o jogador descobre sozinho.
- O Salão das Estátuas ganha cara de templo: tijolos, piso de lajes, viga no teto, colunas.
- A fenda da galeria sai. No lugar, **a Fenda**: um abismo gigantesco no canto de baixo, a leste,
  com o verde do Coração no fundo; chega-se por um túnel do Poço Frágil até um mirante com a placa
  "Para trás, espaço reservado para mineração privada". Não se desce.

## 8d. Terceira revisão do usuário
- O mini boss vira **O Monstro da Caverna**: fera cega, corcunda, com crista de cristal e a
  **safira cravada na testa**. Ao morrer solta a safira e a **CHAVE ANTIGA**.
- A CHAVE ANTIGA abre a grade trancada na **entrada** do Salão das Estátuas (vindo da Volta Leste).
  Ordem do caminho principal: palavras → monstro → chave → estátuas → broca.
- **No máximo uma plataforma por sala**; as subidas viraram chaminés de pulo de parede. Exceção:
  a arena da broca, cujo chão desaba inteiro (as prateleiras são a única volta).
- Mite morrendo estilhaça com barulho (acorda os outros). Mais caçadores pelo distrito.
- Começo da segunda etapa: mites, caçadores e o monstro animados (parado/andando, poses de
  pancada); chave e fechadura com arte própria.

## 8e. Quarta revisão do usuário
- Mites: morrem em 2 golpes com o rubi (vida 4) e não soltam fragmentos.
- Sala das palavras: mais um caçador. Corredor de cima (antes do salão): mais um caçador e 4 mites.
- O pouso de um pulo comum já faz barulho que os bichos ouvem (com a safira, não).
- O Monstro da Caverna, refeito: enorme (12x8 tiles), **grudado na quina da parede leste**, parte da
  rocha; não anda. Barulho **perto** → morde onde veio o som e fica com os dentes presos no chão (a
  única hora em que leva dano). Barulho **longe** → grita e o teto despenca em estilhaços (com
  tremor de aviso, dão dano). Durante a luta, **mites brotam do chão** sem parar (no máximo 3).
  A chaminé da sala das palavras passou pro oeste, pra entrada da toca ficar longe dele.

## 8f. Segunda etapa (arte e ambientação), concluída
- Shard Sentinel: gigante de quartzo animado (núcleo pulsando) e pose de carga antes de atirar.
- Geode Drill: máquina de esteiras animada (esteiras, espiral girando, fumaça, veios de Óxido).
- Cristal ressonante novo, com base de pedra que não leva a cor da nota.
- Portões: grade de ferro velho com cristal brotando.
- Placas: tábua do turno (Galeria dos Caçadores), placa esmaltada da Coroa (Fenda), bilhetes
  presos por lasca de cristal, o capacete amassado. Gravuras das Ruínas rachadas e lascadas.
- Ambientação de mina: escoramentos de madeira nos corredores baixos, trilhos, vagonete tombado.

## 9. Segunda etapa (fora deste documento)
Remodelagem de arte e animação de Echo Hunter, Crystal Mite, Shard Sentinel, The Resonant e
Geode Drill, no nível das últimas fases. Vai ter documento próprio.
