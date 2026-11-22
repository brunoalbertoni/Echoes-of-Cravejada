using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    public partial class SilverKing
    {
        // ------------------------------------------------------------------
        // A luta
        // ------------------------------------------------------------------

        private IEnumerator Lutar()
        {
            while (!_morto)
            {
                if (JogadorAcima() && Time.time >= _prontoParaCima)
                {
                    yield return LancaDoCeu();
                }
                else
                {
                    yield return Aproximar();
                    if (_morto) yield break;

                    if (JogadorAcima() && Time.time >= _prontoParaCima) yield return LancaDoCeu();
                    else yield return _doisFase ? AtaqueFaseDois() : AtaqueFaseUm();
                }

                // O respiro entre um golpe e outro — curto, e cortado se o Scrap sobe nele.
                Tocar(QuadroOcioso, QuadrosOcioso, 6f);
                _recuavel = true;
                float descanso = _doisFase ? 0.12f : 0.2f;
                for (float t = 0f; t < descanso; t += Time.deltaTime)
                {
                    if (JogadorAcima() && Time.time >= _prontoParaCima) break;
                    yield return null;
                }

                _recuavel = false;
            }
        }

        /// <summary>
        /// O Scrap esta em cima dele: numa sacada logo acima, ou pulando pra
        /// bater na cabeca dele.
        /// </summary>
        private bool JogadorAcima()
        {
            if (_jogador == null) return false;

            Vector2 deslocamento = _jogador.position - transform.position;
            return deslocamento.y > 2.2f && Mathf.Abs(deslocamento.x) < 4.5f;
        }

        /// <summary>Uns passos na direcao do Scrap.</summary>
        private IEnumerator Aproximar()
        {
            _recuavel = true;
            float tempo = Random.Range(0.4f, _doisFase ? 0.6f : 0.9f);
            float velocidade = _doisFase ? 3.9f : 2.9f;
            Tocar(QuadroAndar, QuadrosAndar, velocidade * 3.5f);

            for (float t = 0f; t < tempo; t += Time.deltaTime)
            {
                VirarParaJogador();
                if (Distancia() < 3f) break;
                if (JogadorAcima() && Time.time >= _prontoParaCima) break;
                if (!Mover(_direcaoOlhar * velocidade * Time.deltaTime)) break;

                // Corrompido ele flutua: so o Rei de pe faz barulho de passo.
                if (!_doisFase && Time.time >= _proximoPasso)
                {
                    _proximoPasso = Time.time + 4f / (velocidade * 3.5f);
                    Sfx.PassoRei.Tocar(transform.position);
                }

                yield return null;
            }

            _recuavel = false;
        }

        private int Sortear(int[] opcoes)
        {
            for (int tentativas = 0; tentativas < 6; tentativas++)
            {
                int picareta = opcoes[Random.Range(0, opcoes.Length)];
                if (picareta != _ultimoAtaque || opcoes.Length == 1) return _ultimoAtaque = picareta;
            }

            return _ultimoAtaque = opcoes[0];
        }

        private IEnumerator AtaqueFaseUm()
        {
            float distancia = Distancia();
            int ataque = distancia < 4f ? Sortear(new[] { 0, 0, 1 }) : (distancia < 10f ? Sortear(new[] { 1, 1, 2 }) : Sortear(new[] { 2, 1 }));

            switch (ataque)
            {
                case 0:
                    // Metade das vezes ele emenda: a cetrada e, sem respiro, o raio.
                    if (Random.value < 0.7f) yield return RaioDoCetro();
                    yield return VarridaDoCetro();
                    break;
                case 1:
                    yield return RaioDoCetro();
                    if (!_morto && Random.value < 0.65f) yield return FacetasDaCoroa(5, false);
                    break;
                default: yield return FacetasDaCoroa(9, false); break;
            }
        }

        private IEnumerator AtaqueFaseDois()
        {
            float distancia = Distancia();
            int ataque = distancia < 4f ? Sortear(new[] { 0, 0, 3, 4 }) : Sortear(new[] { 1, 2, 3, 4 });

            switch (ataque)
            {
                case 0: yield return VarridaDoCetro(); break;
                case 1: yield return TesouraDeLuz(); break;
                case 2: yield return ChuvaDeFacetas(); break;
                case 3: yield return AtaqueDeSoproOxido(); break;
                default: yield return Deslizar(); break;
            }
        }

        // ------------------------------------------------------------------
        // Os golpes
        // ------------------------------------------------------------------

        /// <summary>A cetrada: recolhe por cima do ombro e varre rente ao chao; depois fica exposto, apoiado.</summary>
        private IEnumerator VarridaDoCetro()
        {
            VirarParaJogador();
            float preparo = _doisFase ? 0.3f : 0.42f;
            Tocar(QuadroPreparoVarrida, QuadrosPreparoVarrida, QuadrosPreparoVarrida / preparo, false);
            Sfx.PreparoVarridaRei.Tocar(transform.position, tom2: 0.48f / preparo);
            yield return Alertar(preparo);

            Tocar(QuadroVarrida, QuadrosVarrida, QuadrosVarrida / 0.24f, false);
            Sfx.VarridaRei.Tocar(transform.position + Vector3.right * (_direcaoOlhar * 2f), tom2: _doisFase ? 0.85f : 1f);
            yield return new WaitForSeconds(0.08f);

            // A varrida rente ao chao: do corpo ate a ponta do cetro, como no desenho.
            float perto = -0.5f;
            float longe = alcanceVarrida + 0.4f;
            Acertar(new Vector2(transform.position.x + _direcaoOlhar * (perto + longe) * 0.5f, chaoY + 1.1f), new Vector2(longe - perto, 2.2f), 3);
            ScrapCameraFollow.Tremer(0.12f, 0.3f);
            yield return new WaitForSeconds(0.2f);

            // Apoiado no cetro, arfando: a hora de bater.
            _expostoAte = Time.time + (_doisFase ? 0.5f : 0.6f);
            Tocar(QuadroExposto, QuadrosExposto, 4f);
            while (EstaExposto && !_morto) yield return null;
        }

        /// <summary>
        /// A Coluna de Luz, contra quem fica em cima dele: agacha com o cetro
        /// baixo (o aviso), enfia o cetro pro alto — a coluna queima tudo
        /// acima dele ate o teto — e dois raios abrem em leque, com as
        /// facetas subindo.
        /// </summary>
        private IEnumerator LancaDoCeu()
        {
            _prontoParaCima = Time.time + (_doisFase ? 1.6f : 2.2f);
            VirarParaJogador();

            float preparo = _doisFase ? 0.26f : 0.36f;
            Tocar(QuadroParaCima, 1, 1f, false);
            Sfx.CargaCeuRei.Tocar(transform.position, tom2: 0.36f / preparo);
            yield return Alertar(preparo);

            Tocar(QuadroParaCima + 1, QuadrosParaCima - 1, 6f, false);
            Vector2 pontoOrigem = Apontar(pontaParaCima);
            Sfx.LancaCeuRei.Tocar(pontoOrigem, tom2: _doisFase ? 0.85f : 1f);
            Sfx.LancamentoFaceta.Tocar(pontoOrigem);

            // A coluna: tudo em cima dele, do peito ao teto.
            float parteInferior = chaoY + 2f;
            Acertar(new Vector2(transform.position.x, (parteInferior + tetoY) * 0.5f), new Vector2(3.6f, tetoY - parteInferior), 3);
            ScrapCameraFollow.Tremer(0.2f, 0.4f);

            for (int i = 0; i < 5; i++)
            {
                float angulo = Mathf.Lerp(55f, 125f, i / 4f);
                Vector2 velocidadeVetorial = new Vector2(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad)) * 11f;
                FacetShard.Lancar(pontoOrigem, velocidadeVetorial, _doisFase ? 2 : 1, _doisFase ? spriteCorrompidoFaceta : spriteFaceta);
            }

            // Os dois raios abrindo em leque, por cima dele.
            for (float t = 0f; t < 0.55f; t += Time.deltaTime)
            {
                float k = t / 0.55f;
                if (aRaio != null) aRaio.Disparar(pontoOrigem, Mathf.Lerp(92f, 48f, k));
                if (bRaio != null) bRaio.Disparar(pontoOrigem, Mathf.Lerp(88f, 132f, k));
                yield return null;
            }

            if (aRaio != null) aRaio.Esconder();
            if (bRaio != null) bRaio.Esconder();

            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.15f : 0.25f);
        }

        /// <summary>O raio: aponta, o fio fino mira, e o raio varre do alto ate o chao na frente.</summary>
        private IEnumerator RaioDoCetro()
        {
            VirarParaJogador();
            Tocar(QuadroErguerRaio, QuadrosErguerRaio, QuadrosErguerRaio / 0.4f, false);
            Sfx.CargaCeuRei.Tocar(transform.position, tom2: 0.75f);
            yield return new WaitForSeconds(0.4f);

            Tocar(QuadroDisparoRaio, 1, 1f, false);
            Vector2 pontoOrigem = Apontar(pontaCetro);

            // Do alto, na frente, ate o chao na frente (y pra cima: angulo positivo e pra cima).
            float de = Angulo(35f);
            float to = Angulo(-32f);

            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                if (aRaio != null) aRaio.Mirar(pontoOrigem, de);
                yield return null;
            }

            Tocar(QuadroDisparoRaio, QuadrosDisparoRaio, 8f);
            ScrapCameraFollow.Tremer(0.1f, 1.1f);

            for (float t = 0f; t < 1.1f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 1.1f);
                if (aRaio != null) aRaio.Disparar(pontoOrigem, Mathf.LerpAngle(de, to, k));
                yield return null;
            }

            if (aRaio != null) aRaio.Esconder();
        }

        /// <summary>As facetas: a coroa solta lascas de luz pra frente, que quicam nas paredes.</summary>
        private IEnumerator FacetasDaCoroa(int quantidade, bool corrompido)
        {
            VirarParaJogador();
            Tocar(QuadroLancamento, QuadrosLancamento, QuadrosLancamento / 0.45f, false);
            Sfx.LancamentoRei.Tocar(transform.position + Vector3.up * 4f, tom2: corrompido ? 0.8f : 1f);
            yield return Alertar(0.45f);

            Vector2 pontoOrigem = Apontar(pontoCoroa);
            Sfx.LancamentoFaceta.Tocar(pontoOrigem);
            for (int i = 0; i < quantidade; i++)
            {
                float angulo = Angulo(Mathf.Lerp(15f, 75f, quantidade == 1 ? 0.5f : i / (float)(quantidade - 1)));
                Vector2 velocidadeVetorial = new Vector2(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad)) * 10f;
                FacetShard.Lancar(pontoOrigem, velocidadeVetorial, 3, corrompido ? spriteCorrompidoFaceta : spriteFaceta);
            }

            ScrapCameraFollow.Tremer(0.08f, 0.3f);
            yield return new WaitForSeconds(0.9f);
        }

        /// <summary>A tesoura: o raio sai pros dois lados ao mesmo tempo e cruza a sala.</summary>
        private IEnumerator TesouraDeLuz()
        {
            Tocar(QuadroErguerRaio, QuadrosErguerRaio, QuadrosErguerRaio / 0.35f, false);
            Sfx.CargaCeuRei.Tocar(transform.position, tom2: 0.65f);
            yield return new WaitForSeconds(0.35f);

            Tocar(QuadroDisparoRaio, 1, 1f, false);
            Vector2 pontoOrigem = Apontar(pontaCetro);
            Vector2 coroa = Apontar(pontoCoroa);

            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                if (aRaio != null) aRaio.Mirar(pontoOrigem, 50f);
                if (bRaio != null) bRaio.Mirar(coroa, 130f);
                yield return null;
            }

            Tocar(QuadroDisparoRaio, QuadrosDisparoRaio, 8f);
            ScrapCameraFollow.Tremer(0.15f, 1.1f);

            for (float t = 0f; t < 1.05f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 1.05f);
                if (aRaio != null) aRaio.Disparar(pontoOrigem, Mathf.Lerp(50f, -28f, k));
                if (bRaio != null) bRaio.Disparar(coroa, Mathf.Lerp(130f, 208f, k));
                yield return null;
            }

            if (aRaio != null) aRaio.Esconder();
            if (bRaio != null) bRaio.Esconder();
        }

        /// <summary>A chuva de facetas: brilhos no teto avisam, as lascas caem com vaos.</summary>
        private IEnumerator ChuvaDeFacetas()
        {
            Tocar(QuadroLancamento, QuadrosLancamento, QuadrosLancamento / 0.4f, false);
            Sfx.LancamentoRei.Tocar(transform.position + Vector3.up * 4f, tom2: 0.75f);
            yield return new WaitForSeconds(0.35f);

            for (int onda = 0; onda < 4; onda++)
            {
                var brilhos = new List<GameObject>();
                var locais = new List<float>();
                float deslocamento = onda % 2 == 0 ? 0f : 2f;

                for (float x = minX + deslocamento; x <= maxX; x += 4f)
                {
                    locais.Add(x);
                    locais.Add(x + 1f);
                }

                foreach (float x in locais) brilhos.Add(Cintilar(new Vector2(x, tetoY - 1.5f)));

                // A sala inteira: o brilho e a chuva vem do teto todo, nao de um ponto.
                Sfx.BrilhoFaceta.Tocar();
                yield return new WaitForSeconds(0.55f);

                foreach (GameObject brilhoCurto in brilhos) if (brilhoCurto != null) Destroy(brilhoCurto);
                foreach (float x in locais) FacetShard.Lancar(new Vector2(x, tetoY - 1.5f), Vector2.down * 14f, 0, spriteCorrompidoFaceta);
                Sfx.ChuvaFaceta.Tocar();

                ScrapCameraFollow.Tremer(0.1f, 0.3f);
                yield return new WaitForSeconds(0.75f);
            }
        }

        private GameObject Cintilar(Vector2 at)
        {
            var brilhoCurto = Prefabs.Criar("FacetGlint");
            brilhoCurto.transform.position = at;
            var renderizador = Prefabs.Componente<SpriteRenderer>(brilhoCurto);
            renderizador.sprite = spriteBrilhoCurto;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 9;
            return brilhoCurto;
        }

        /// <summary>O sopro: inspira (o aviso), sopra uma onda rente ao chao, e fica arfando, exposto.</summary>
        private IEnumerator AtaqueDeSoproOxido()
        {
            VirarParaJogador();
            Tocar(QuadroInspirar, QuadrosInspirar, QuadrosInspirar / 1f, false);
            Sfx.InspirarRei.Tocar(transform.position + Vector3.up * 3f);
            yield return Alertar(1f);

            Tocar(QuadroSopro, QuadrosSopro, 7f);
            ScrapCameraFollow.Tremer(0.2f, 1.1f);
            if (sopro != null) yield return sopro.Soprar(transform.position.x);

            _expostoAte = Time.time + 0.75f;
            Tocar(QuadroExposto, QuadrosExposto, 4f);
            while (EstaExposto && !_morto) yield return null;
        }

        /// <summary>A investida: flutua rapido pela sala inteira, ate o outro lado.</summary>
        private IEnumerator Deslizar()
        {
            VirarParaJogador();
            Tocar(QuadroPreparoVarrida, 1, 1f, false);
            Sfx.PreparoVarridaRei.Tocar(transform.position, tom2: 0.8f);
            yield return Alertar(0.4f);

            float parede = _direcaoOlhar > 0 ? maxX : minX;
            _carregando = true;
            Tocar(QuadroAndar, QuadrosAndar, 16f);
            Sfx.PlanarRei.Tocar(transform.position);

            while (!Mathf.Approximately(transform.position.x, parede) && !_morto)
            {
                float x = Mathf.MoveTowards(transform.position.x, parede, 11f * Time.deltaTime);
                transform.position = new Vector3(x, chaoY, 0f);
                yield return null;
            }

            _carregando = false;
            Virar(-_direcaoOlhar);
        }

        /// <summary>O aviso antes do golpe: pisca em branco.</summary>
        private IEnumerator Alertar(float segundos)
        {
            _desenhoAte = Time.time + segundos + 0.15f;
            for (float t = 0f; t < segundos; t += Time.deltaTime)
            {
                if (_sprite != null && Time.time >= _feridoAte)
                {
                    _sprite.color = Mathf.FloorToInt(t * 14f) % 2 == 0 ? new Color(1f, 1f, 0.75f) : Color.white;
                }

                yield return null;
            }

            if (_sprite != null) _sprite.color = Color.white;
        }

        private void Acertar(Vector2 centro, Vector2 tamanho, int dano)
        {
            foreach (Collider2D golpe in Physics2D.OverlapBoxAll(centro, tamanho, 0f))
            {
                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null) vida.LevarDano(dano, transform.position);
            }
        }

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        private float Distancia() => _jogador == null ? 0f : Mathf.Abs(_jogador.position.x - transform.position.x);

        /// <summary>Um ponto do desenho (olhando pra direita), virado pro lado em que ele olha.</summary>
        private Vector2 Apontar(Vector2 local2) => (Vector2)transform.position + new Vector2(local2.x * _direcaoOlhar, local2.y);

        /// <summary>Um angulo de quem olha pra direita, virado pro lado em que ele olha.</summary>
        private float Angulo(float direita) => _direcaoOlhar > 0 ? direita : 180f - direita;

        private void VirarParaJogador()
        {
            if (_jogador != null) Virar(_jogador.position.x >= transform.position.x ? 1 : -1);
        }

        private void Virar(int direcao)
        {
            _direcaoOlhar = direcao >= 0 ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direcaoOlhar < 0;

            // O corpo do desenho: 1,2 atras, 1,45 na frente (o manto e o cetro), na altura de sempre.
            if (_corpo != null)
            {
                const float costas = 1.2f;
                const float frente = 1.45f;
                _corpo.size = new Vector2(costas + frente, _corpo.size.y);
                _corpo.offset = new Vector2((frente - costas) * 0.5f * _direcaoOlhar, _corpo.offset.y);
            }
        }


        private bool Mover(float dx)
        {
            Vector3 posicao = transform.position;
            float x = Mathf.Clamp(posicao.x + dx, minX, maxX);
            bool movido = !Mathf.Approximately(x, posicao.x);
            transform.position = new Vector3(x, chaoY, 0f);
            return movido;
        }
    }
}
