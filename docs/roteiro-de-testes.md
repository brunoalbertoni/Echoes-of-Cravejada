# Roteiro de testes do Echoes of Cravejada

Cobre tudo o que mudou na reorganização: arte, Animator, módulos, nomes em português, GameManager, prefabs, Cinemachine, Canvas, TextMeshPro, LeanTween, áudio e save.

**Como usar:** marque cada item. Se algo falhar, anote a fase, o que você estava fazendo e o erro do Console.

**Já verificado sem jogar:** o projeto compila na Unity e as 6 cenas abrem em Play sem erros. Isso não cobre controles, combate, visual nem som.

## 0. Preparação

- [ ] Fazer um commit ou cópia do projeto antes de testar.
- [ ] Guardar uma cópia do save atual: `%USERPROFILE%\AppData\LocalLow\<empresa>\<jogo>\save.json`.
- [ ] Abrir o projeto e confirmar que o Console não tem erros vermelhos.
- [ ] Abrir `Assets/Common/Scenes/MainMenu.unity` e dar Play.
- [ ] Deixar o Console visível durante todo o teste.

## 1. Teste prioritário (15 minutos)

Se algo aqui falhar, pare e me avise antes de continuar.

- [ ] O menu aparece com a arte correta e os botões respondem.
- [ ] **Continuar** carrega o save antigo na bancada certa.
- [ ] Habilidades, fragmentos, gemas, itens e chefes derrotados continuam iguais.
- [ ] O Scrap anda, pula, golpeia e usa o dash com a mesma sensação de antes.
- [ ] A câmera segue o Scrap sem tremer e não mostra fora do mapa.
- [ ] Um inimigo anda, ataca, apanha e morre com a animação certa.
- [ ] A pausa abre, fecha e muda os volumes.
- [ ] A troca de fase fecha e abre a tela normalmente.

## 2. Menu e fluxo do jogo

- [ ] Navegar pelo menu com teclado, mouse e controle.
- [ ] **Novo jogo** começa na Fase 1, sem progresso anterior.
- [ ] **Continuar** fica indisponível quando não há save.
- [ ] **Sair** encerra o Play.
- [ ] Pausa: **Continuar** retoma o jogo no ponto exato.
- [ ] Pausa: **Sair** mostra o aviso, e as duas opções funcionam.
- [ ] Morrer abre a tela de morte, e voltar à bancada funciona.
- [ ] Voltar ao menu e iniciar de novo não deixa o jogo congelado.
- [ ] Nenhuma tela fica presa aparecendo depois de fechar.

## 3. Scrap

- [ ] Parado: respira e pisca.
- [ ] Andar, pular, subir, cair e aterrissar mostram os quadros certos.
- [ ] Pulo de parede e deslize na parede.
- [ ] Golpe para o lado, para cima e para baixo.
- [ ] O golpe para baixo quica nos inimigos e perigos.
- [ ] Dash, com recarga e som.
- [ ] Salto de Pressão, depois de liberado.
- [ ] Golpe Ressonante, depois de liberado.
- [ ] Levar dano: piscar, recuo e invulnerabilidade.
- [ ] Morrer deixa a sucata com fragmentos, e dá para recuperar.
- [ ] Depois do polimento, todas as animações usam o visual polido.

## 4. Câmera (Cinemachine)

- [ ] Sem tremedeira ao andar, pular e cair.
- [ ] O parallax do fundo acompanha sem atraso.
- [ ] A câmera respeita as bordas de todas as fases.
- [ ] Tremor de tela em golpes de chefe e explosões.
- [ ] Zoom e foco nas cutscenes, voltando ao normal depois.
- [ ] Ao chegar de outra cena, ela já começa enquadrada no Scrap.
- [ ] O elevador não faz a câmera pular.

## 5. Interface (Canvas e TextMeshPro)

Atenção especial: os textos agora usam TextMeshPro, então a fonte pode ter mudado.

- [ ] HUD: vida, fragmentos, gemas e avisos.
- [ ] Barra de vida de cada chefe.
- [ ] Caixa de diálogo: nome, retrato, texto e quebra de linha.
- [ ] Inventário e dicas dos itens.
- [ ] Loja do Crisol: lista, preços, compra, recusa e mouse.
- [ ] Documentos de lore abrem com a arte grande.
- [ ] Cartão de habilidade, incluindo a demonstração animada.
- [ ] Placas de tutorial.
- [ ] Aviso de item ganho e encaixe de gema.
- [ ] Gravura de jogo salvo.
- [ ] "Carregando" e a engrenagem na troca de fase.
- [ ] Testar em 1280x720, 1920x1080 e tela cheia.
- [ ] Nenhum texto cortado, fora do lugar ou atrás de outro elemento.

## 6. Som

- [ ] Música certa no menu e em cada fase.
- [ ] Cada chefe troca para a própria música.
- [ ] Silêncio depois da queda do chefe, sem cortar a cena.
- [ ] Música da purificação e da fuga do ácido.
- [ ] A música abaixa nos diálogos e volta depois.
- [ ] Volume de música muda só a música.
- [ ] Volume de efeitos muda efeitos, ambiente e sons do mapa.
- [ ] Os volumes continuam iguais depois de fechar e abrir o jogo.
- [ ] Som de vapor, caldeira, ácido e cristais varia com a distância.

## 7. Save

- [ ] Salvar com **E** na bancada mostra a gravura.
- [ ] Fechar e continuar volta para a mesma bancada.
- [ ] Mantém habilidades, vida extra, fragmentos e gemas.
- [ ] Mantém rubi, safira e encaixes.
- [ ] Mantém itens, entregas e compras.
- [ ] Portões, barreiras e recompensas pegas continuam como estavam.
- [ ] Chefes derrotados não voltam.
- [ ] Áreas purificadas continuam purificadas.
- [ ] A cena de abertura não repete em um save existente.

## 8. Fase 1: Scrap Yard

- [ ] Cena de abertura, queda e despertar.
- [ ] Rastejantes, com bote e morte.
- [ ] Alavanca enferrujada e portão.
- [ ] NPCs descartados animados e diálogos.
- [ ] Chefe **O Coletor**: entrada, ataques, fase rachada, queda.
- [ ] Purificação e polimento do Scrap.
- [ ] Saída para a Fase 2.

## 9. Fase 2: Brass Pipelines

- [ ] Sanguessuga, sentinela de válvula, soldador e inflado.
- [ ] Projéteis de vapor e poças de ácido.
- [ ] Válvulas, vapor e caldeiras.
- [ ] Missão do encanador e páginas.
- [ ] Fuga do ácido subindo.
- [ ] Chefe **Mestre das Caldeiras**: martelo, investida, rajada, gêiser, salto e fase quente.
- [ ] Purificação e elevador.

## 10. Fase 3: Silent Geodes

- [ ] Ácaro, caçador e sentinela de cristal.
- [ ] Sistema de ruído: inimigos reagem ao barulho.
- [ ] Cristais de eco, plataformas frágeis e barreiras.
- [ ] Porta Harmônica e Salão das Estátuas.
- [ ] Mini chefe **O Capataz**: picareta, investida, grito e crias.
- [ ] Chefe **Broca**: erupção, pilares, destroços e queda.
- [ ] Purificação e saída.

## 11. Fase 4: Alchemical Ateliers

- [ ] Gosma de mercúrio, arremessador, experimento e sentinelas.
- [ ] Frascos, gotas e poças.
- [ ] Raios de luz e espelhos.
- [ ] Reagentes e Grande Alambique.
- [ ] Chefe **O Alquimista**: chicote, gêiseres, inundação, redoma, círculos e forma de prata.
- [ ] Purificação e Grande Escadaria.

## 12. Fase 5: Silver City

- [ ] Corredor, bancada e Crisol.
- [ ] Entrada do **Rei de Prata**.
- [ ] Fase 1: cetro, varrida, raio, facetas e lança do céu.
- [ ] Transformação sem música, como planejado.
- [ ] Fase corrompida: sopro de óxido e novos ataques.
- [ ] Morrer no chefe e tentar de novo reinicia a luta corretamente.
- [ ] Queda do Rei e caminho para o Altar.
- [ ] **Final do trono**, do começo à tela de fim.
- [ ] **Final da coroa**, do começo à tela de fim.
- [ ] Os créditos aparecem na tela final.

## 13. Prefabs e objetos criados em jogo

- [ ] Fragmentos caem e são coletados.
- [ ] Recompensas e módulos aparecem no lugar certo.
- [ ] Projéteis acertam, somem e não atravessam o que não deveriam.
- [ ] Efeitos não ficam parados na tela.
- [ ] Depois de uma luta longa, o jogo não fica mais lento.

## 14. Ferramentas do Editor

Teste por último e com commit feito, porque gerar uma fase substitui a cena.

- [ ] O menu **Echoes** mostra os comandos novos.
- [ ] **Gerar Prefabs** roda sem erro.
- [ ] **Gerar Fase 1** valida, monta a cena e recria os Animators.
- [ ] Depois de gerar, a fase continua jogável.
- [ ] **Redesenhar Interface** roda sem erro.

## 15. Build final

- [ ] Gerar um build em **File > Build Profiles**.
- [ ] O build abre fora da Unity.
- [ ] Menu, save, som e troca de fase funcionam no build.
- [ ] Jogar uma fase inteira no build.
- [ ] **Sair** fecha o jogo.
