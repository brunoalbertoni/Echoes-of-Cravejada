# Phase 4 (The Alchemical Ateliers) — a região dos laboratórios nobres

Data: 2026-10-29
Arquivos principais: `Assets/Editor/Phase04Layout.cs`, `Assets/Editor/Phase04Generator.cs`,
arte nova em `Assets/Editor/*Art.cs` (+ `LoreArt.Phase04.cs`), scripts novos em `Assets/Scripts/`.

> Marcação: **[usuário]** = decidido pelo usuário. **[minha decisão]** = decisão minha,
> que o usuário pode revisar. Sem marcação = consequência direta das duas.

## 1. Por quê

A Phase 4 ficou na "etapa A": mapa escavado, mas sem chefe; o Grande Laboratório, a Forja, a
Cerimônia e o elevador são salas vazias atrás de uma grade. Os inimigos são sprites parados, a
arte das leituras é genérica (página solta, placa de latão) e o cenário é mármore com um ou outro
tanque. Não parece um laboratório.

O usuário pediu a mesma reformulação que as fases 1, 2 e 3 tiveram: fase refeita inteira, com
chefe, inimigos animados, arte própria para tudo e ambientação do distrito.

## 2. Intenção

É a região dos **forjadores e laboratórios nobres** **[usuário]**, e é **de onde o Scrap veio**
**[usuário]**: ele é o Protótipo Bruto n.º 7, desenhado aqui e jogado fora por ordem do Rei.
O jogador tem que **sentir que está num laboratório**, cheio de experimentos **[usuário]**:
vidraria, alambiques, cobaias, tanques, anotações, fórmulas, mesas de trabalho. Mármore e prata
são a casca rica; o que se vê em toda sala é o trabalho do laboratório.

## 3. Metas, não-metas e anti-metas

**Metas**
- Fase refeita inteira, mapa novo **[usuário]**.
- Cara de laboratório em toda sala **[usuário]**.
- A ficha do Protótipo n.º 7 é **um arquivo comum, lido do jeito normal** (com E), sem cena
  especial **[usuário]**.
- Chefe: **o Alquimista de Mercúrio**, com a luta descrita na seção 6 **[usuário aprovou minha proposta]**.
  A luta não copia nada dos chefes anteriores **[usuário]**.
- Depois do chefe: a **fuga do Guarda de Prata Perfeita** e a **Cerimônia** **[usuário]**.
- O **reforço de chassi** secreto continua **[usuário]**.
- Mesmos padrões de arte das fases anteriores (chefe em folha com `Pose`, inimigos animados com
  morte, legíveis com desenho grande, props próprios do distrito).

**Não-metas**
- **Sem a Forja Modular** **[usuário]**.
- **Sem recompensas novas** além do chassi **[usuário]**: nada de cofres, gemas ou sacos de fragmentos.
- Não mexer nas fases 1, 2 e 3 (só o necessário para a ligação com o elevador continuar igual).
- A próxima fase (District 5, Silver City) não existe e não é feita agora.

**Anti-metas (falharia mesmo funcionando)**
- Parecer um palácio de mármore com tanques espalhados, e não um laboratório.
- Copiar golpe ou estrutura de luta de um chefe anterior (piscar amarelo antes do golpe, investida,
  onda de choque pelo chão, gêiser, chão desabando, caçar pelo som, teto caindo, bichos brotando).
- Chefe ou inimigo com sprite parado trocado na mão.
- Fuga injusta: morrer para o Guarda sem ter visto por onde ir.

## 4. Estrutura **[usuário aprovou minha proposta]**

```
 [Entrada] → [CENTRO: Ateliê de Crisol] ─┬─ Ala da Destilaria  (mercúrio, círculos)  → reagente 1
                     │                    ├─ Ala de Óptica      (espelhos, raio)      → reagente 2
                     │                    └─ Ala de Incubação   (tanques, protótipos) → reagente 3
                     │
                [Porta do Grande Alambique: pede os 3 reagentes]
                     │
            [Pré-boss + bancada] → [Grande Laboratório: o Alquimista]
                     │
            [Fuga: Corredor do Guarda] → [Cerimônia: Anfiteatro de Demonstrações, purificação]
                     │
            [Elevador para o District 5]
```

### 4.1 Entrada
O Scrap desce da cabine do District 3 (`FromDistrict3SpawnId` continua o mesmo) num saguão de
mármore. A placa do portão continua ("ENTRADA VEDADA A METAIS COMUNS" / "e a quem mais?"). É curta
e linear, com no máximo um inimigo de apresentação.

### 4.2 O centro: Ateliê de Crisol
Bancada, Crisol e a loja (como nas outras fases). A porta do **Grande Alambique** fica à vista
daqui ou logo ao lado: o jogador vê o objetivo cedo. Dela saem as três alas. As alas podem ser
feitas **em qualquer ordem**.

### 4.3 As três alas
Cada ala é uma parte do laboratório, tem o seu tipo de enigma e termina num **reagente** (item-chave)
que vai no alambique. Cada uma conta um pedaço da história.

**Ala da Destilaria (mercúrio).** Poças de mercúrio (o Scrap flutua e escorrega; a corrompida
tem correnteza), goteiras, alambiques e serpentinas. Enigmas com os círculos de transmutação
(vidro em prata, mercúrio em vapor, mármore em areia). Aqui fica o **nicho do chassi**, alcançado
por uma coluna de vapor de um círculo opcional. Reagente: **Mercúrio Destilado**.

**Ala de Óptica (luz).** Bancadas de lentes, prismas, o emissor e os espelhos girando; o raio
precisa chegar numa fechadura de cristal. Pelo menos dois enigmas de espelho, o segundo mais
pensado que o primeiro (o atual da Câmara dos Espelhos pode virar o primeiro). Reagente:
**Luz Engarrafada** (ou nome parecido).

**Ala de Incubação (cobaias e protótipos).** Corredores de tanques com cobaias de cobre dentro,
gaiolas, mesas de dissecação. No fim, a **Oficina de Protótipos**: mesas com os protótipos 1 a 6
desmontados, e a **ficha do n.º 7** (o Scrap), lida como arquivo comum. A **Assistente** fica
escondida no armário de reagentes desta ala. Reagente: **Sal de Quartzo** (ou nome parecido).

### 4.4 Porta do Grande Alambique
Um aparelho grande com três encaixes. Cada reagente entregue acende um encaixe; com os três a
porta abre. Leva ao pré-boss (bancada) e à arena.

### 4.5 Depois do chefe
1. O Alquimista escorre pelo ralo da arena. **Ainda não purifica**: o coração do distrito é o
   círculo do palco da Cerimônia.
2. O **Guarda de Prata Perfeita** acorda (ver 7) e o Scrap foge pelo Corredor do Guarda.
3. A fuga termina na **Cerimônia: o Anfiteatro de Demonstrações**, onde a nobreza assistia aos
   experimentos e onde o Rei recusou o protótipo. O Scrap pole o grande círculo do palco; o
   distrito é purificado (`AreaPurifier`, partindo do palco) e o Guarda se desfaz.
4. Do anfiteatro, o **elevador** para o District 5.

Depois da purificação: a Assistente sai do armário (fala nova, em pé, perto do centro ou do
anfiteatro), Crisol ganha falas purificadas.

## 5. História e leituras

Ordem que o jogador pode montar, sem obrigação de ordem:
- **Diários de laboratório** (folhas numeradas por dia): o mercúrio retarda a corrosão → testes em
  "voluntários" de cobre → tanques que acordam errado → o Mestre entra no tanque grande e vira
  mercúrio → protótipos brutos são imunes → o Rei recusa: "a prata não será salva por sucata".
  Os textos atuais servem de base; cada ala recebe as folhas que combinam com ela.
- **Retratos** dos nobres riscados (o de Crisol intacto).
- **Ficha do Protótipo n.º 7** na Oficina de Protótipos (texto atual).
- **Fichas dos protótipos 1 a 6** (curtas, ao lado de cada mesa ou numa só prancheta): o que deu
  errado em cada um. **[minha decisão]**
- **Quadros-negros** com fórmulas e a última anotação de alguém.
- A **Assistente** e **Crisol** contam o resto.

Tudo que se lê com E tem arte própria em `LoreArt.Phase04`: desenho pequeno no mapa e desenho
grande da leitura com o texto escrito nele. Objetos iguais usam o mesmo modelo (toda folha de
diário é o mesmo papel, todo retrato a mesma moldura).

## 6. O chefe: o Alquimista de Mercúrio **[usuário aprovou minha proposta]**

**Quem é:** alto, jaleco, máscara de bico, educado. Da cintura para baixo já é mercúrio
escorrendo. Para ele a luta é um experimento: quer uma "amostra" do protótipo que descartou.
Tamanho e traço como os chefes anteriores (~100 px a 32 px/tile, cheio de peças pequenas).

**Regra principal:**
- **Líquido, não leva dano**: o cinzel atravessa e só espirra.
- A arena tem **círculos de transmutação** no chão (os mesmos da fase, mas de polir mais rápido).
  Terminar de polir um círculo com o Alquimista perto o transforma em **prata maciça** por alguns
  segundos: só aí ele apanha.
- Depois ele se solta e **apaga aquele círculo com mercúrio** por um tempo: o Scrap tem que ir a
  outro. A tensão é ficar parado polindo enquanto ele ataca.

**Golpes:**
- **Campânula:** uma redoma de vidro cai do teto em cima do Scrap. Pego, ele fica preso e precisa
  quebrar o vidro a golpes, e leva dano.
- **Destilação:** ele evapora numa nuvem prateada que anda pelo teto pingando gotas; depois se
  condensa em outro lugar.
- **Pipeta:** o braço vira um chicote de mercúrio que varre a arena numa altura; passa-se por cima
  ou por baixo.

**Metade da vida:** os tanques da arena racham e o chão vira poças de mercúrio (o Scrap flutua e
escorrega); os círculos ficam em ilhas mais altas.

**Morte:** escorre pelo ralo central (animação própria). Deixa o caminho para o Corredor do Guarda.

Avisos de golpe: cada golpe tem pose de preparo própria e legível, **sem** o piscar amarelo dos
chefes anteriores (ex.: a redoma faz sombra no chão e range no teto; a pipeta enche e brilha;
a destilação ferve antes).

## 7. O Guarda de Prata Perfeita e a fuga

- Armadura de prata perfeita, sem Óxido à vista. **Não morre e não leva dano.**
- Acorda quando o Alquimista cai, com a ordem de "descartar o protótipo defeituoso".
- Persegue o Scrap pelo Corredor do Guarda até o anfiteatro. Encostar dá dano e empurra.
- O corredor é desenhado para a fuga: obstáculos que pedem o que o Scrap já tem (dash, pulo de
  parede, Salto de Pressão), com o caminho legível de longe.
- Morrer na fuga volta à bancada do pré-boss, e a fuga recomeça desde o começo (o Alquimista
  continua derrotado).
- Na purificação, ele para e se desfaz.

## 8. Inimigos

Os quatro tipos atuais continuam, com comportamento igual ou ajustado, **redesenhados e animados**
(parado, andando, ataque quando houver e **morte**):
- **Gosma de Mercúrio** (`QuicksilverSlime`): se parte em duas menores.
- **Arremessador de Frascos** (`FlaskThrower`): assistente corrompido que joga frascos.
- **Experimento Costurado** (`FailedExperiment`): meio prata, meio cobre.
- **Sentinela de Vidro** (`TransmutedSentinel`): vidro/prata.

Patrulhas usam o chão inteiro do trecho. Alas com densidade e mistura parecidas.

## 9. Ambientação

Props de laboratório próprios do distrito, espalhados por todas as salas: alambiques, retortas,
bicos de gás, redomas, vidros de espécime, quadros-negros com fórmulas, mesas de dissecação, tubos
com líquidos coloridos, estantes de reagentes, gaiolas, tanques de cultura, lustres. Os tiles de
mármore continuam como base.

## 10. Restrições técnicas

- Mapa por código em `Phase04Layout.cs`, sem depender de UnityEngine. `Validate()` passa e confere:
  cada ala se completa partindo do centro sem outra feita; a arena só se alcança com os três
  reagentes; a fuga tem caminho do pré-boss ao anfiteatro; o chassi exige o círculo do vapor;
  marcadores, inimigos e leituras alcançáveis; o raio de cada enigma de luz não bate em terreno.
- Mantêm-se `SceneName` e `FromDistrict3SpawnId`.
- Validar fora do Editor (projeto `net9.0` apontando para os `.cs`) e gerar prévia do mapa e da arte
  em PNG antes de pedir ao usuário para rodar o gerador.
- Sem commit nem push: o usuário sobe as alterações.

## 11. Decisões minhas (para conferir)

- Os nomes dos reagentes (Mercúrio Destilado, Luz Engarrafada, Sal de Quartzo).
- A purificação vem no palco da Cerimônia, não na morte do Alquimista.
- Fichas curtas dos protótipos 1 a 6.
- Morrer na fuga recomeça a fuga desde o pré-boss.
- O elevador para o District 5 leva a uma cena que não existe: até ela existir, ele mostra um aviso
  em vez de carregar a cena.
- Avisos de golpe sem o piscar amarelo.

## 12. Fica para quem constrói

Coordenadas, formato exato das salas, número exato de inimigos, posição dos props, texto fino das
leituras e das falas, tempos e números da luta (vida, duração da prata, velocidade do Guarda),
desde que respeitem o que está acima.

## 13. Revisão do usuário (durante a implementação)

- **Sem enigmas nesta fase** **[usuário]**: "acredito que essa phase não precisa de enigma, a última já foi o suficiente".
  - Saem os espelhos giratórios, as fechaduras de luz e os círculos de vidro e de mármore.
  - Os raios de luz da Óptica viram **perigo**: ligam e desligam em ritmo e só queimam o Scrap.
  - O fosso da Destilaria ganha um pilar no meio, para atravessar pulando.
  - Ficam o que não é enigma: os três reagentes no fim de cada ala, o círculo do vapor (segurar E) que leva ao
    chassi, e os círculos da luta contra o Alquimista.
- **O chassi volta** **[usuário]**, no nicho em cima da Destilaria.
