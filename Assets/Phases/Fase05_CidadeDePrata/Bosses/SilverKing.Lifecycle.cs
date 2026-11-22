using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    public partial class SilverKing
    {
        // ------------------------------------------------------------------
        // Apanhar
        // ------------------------------------------------------------------

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_morto || _cena) return;

            if (!_acordado)
            {
                // Sentado, de cabeca baixa, ele nem sente: o cinzel tine na prata.
                NoiseSystem.Emitir(de, 6f, 0.3f);
                Sfx.QuadroBloqueio.Tocar(transform.position + Vector3.up * 2f);
                return;
            }

            Sfx.PrataFeridoChefe.Tocar(transform.position + Vector3.up * 2f, tom2: _doisFase ? 0.75f : 0.9f);
            _hp -= EstaExposto ? quantia * 2 : quantia;
            _feridoAte = Time.time + 0.14f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.55f, 0.55f);

            if (_hp > 0) return;

            StopAllCoroutines();
            PararAtaques();
            _carregando = false;
            _recuavel = false;
            _expostoAte = 0f;

            if (!_doisFase) StartCoroutine(Transformacao());
            else Morrer();
        }

        // ------------------------------------------------------------------
        // A transformacao
        // ------------------------------------------------------------------

        private IEnumerator Transformacao()
        {
            _cena = true;

            var controlador = ScrapRef.Controlador;
            bool comecou = !Cinematic.Ativo;
            if (comecou) Cinematic.Iniciar();
            if (controlador != null) controlador.ControleTravado = true;

            // Ele cai de joelhos; a coroa racha. A sala treme.
            ScrapCameraFollow.Direcionar((Vector2)transform.position + Vector2.up * 3f, 0.9f, 0.6f);
            Tocar(QuadroQueda, QuadrosQueda, QuadrosQueda / 1f, false);
            ScrapCameraFollow.Tremer(0.3f, 1.4f);
            Sfx.RachaduraCoroaRei.Tocar(transform.position + Vector3.up * 2f);
            yield return new WaitForSeconds(1.5f);

            yield return ScriptedDialogue.Tocar(Nome, _sprite, LinhasTransicao);

            // A cutscene desenhada (inteira so na primeira vez): no pico do clarao, ele ja e o outro.
            if (!_vistoTransformacao && desenho2 != null && desenho2.EstaProntoUso)
            {
                _vistoTransformacao = true;
                yield return desenho2.Tocar(TornarCorrompido);
            }
            else
            {
                Cinematic.Clarao2 = 1f;
                TornarCorrompido();
                for (float t = 0f; t < 0.8f; t += Time.deltaTime)
                {
                    Cinematic.Clarao2 = 1f - t / 0.8f;
                    yield return null;
                }

                Cinematic.Clarao2 = 0f;
            }

            // Ele se ergue, inteiro em Oxide, e a coroa acende verde.
            ScrapCameraFollow.Direcionar((Vector2)transform.position + Vector2.up * 3f, 1f, 0.5f);
            Sfx.SubidaRei.Tocar(transform.position + Vector3.up * 2f);
            for (int i = QuadrosQueda - 1; i >= 0; i--)
            {
                Tocar(QuadroQueda + i, 1, 1f, false);
                yield return new WaitForSeconds(0.22f);
            }

            Tocar(QuadroLancamento, QuadrosLancamento, QuadrosLancamento / 0.8f, false);
            ScrapCameraFollow.Tremer(0.4f, 1.2f);
            yield return new WaitForSeconds(0.9f);

            yield return ScriptedDialogue.TocarComo("king_corrupt", Nome, _sprite, LinhasErguido);

            ScrapCameraFollow.Liberar();
            if (controlador != null) controlador.ControleTravado = false;
            if (comecou) Cinematic.Encerrar();

            _cena = false;
            _prontoParaCima = Time.time + 0.8f;
            yield return Lutar();
        }

        private void TornarCorrompido()
        {
            // A metade de luz estilhaca (e os espelhos da sala junto).
            Sfx.EstilhacoRei.Tocar(transform.position + Vector3.up * 2f);
            _hp = vidaDoisFase;
            AplicarVisualFaseDois();
            Tocar(QuadroQueda + QuadrosQueda - 1, 1, 1f, false);
        }

        // ------------------------------------------------------------------
        // Cair e recomecar
        // ------------------------------------------------------------------

        private void Morrer()
        {
            _morto = true;
            _derrotado = true;
            if (_colisor != null) _colisor.enabled = false;
            StartCoroutine(Caido());
        }

        private IEnumerator Caido()
        {
            _cena = true;
            var controlador = ScrapRef.Controlador;
            bool comecou = !Cinematic.Ativo;
            if (comecou) Cinematic.Iniciar();
            if (controlador != null) controlador.ControleTravado = true;

            if (_sprite != null) _sprite.color = Color.white;
            ScrapCameraFollow.Direcionar((Vector2)transform.position + Vector2.up * 2.5f, 0.85f, 0.7f);
            ScrapCameraFollow.Tremer(0.45f, 2f);

            // Cambaleia, larga o cetro, ajoelha, a coroa cai: o som inteiro, casado com os quadros.
            Sfx.QuedaRei.Tocar(transform.position + Vector3.up * 2f);
            float[] segurar = { 0.3f, 0.45f, 0.6f, 0.7f, 0.9f, 0.7f };
            for (int i = 0; i < segurar.Length; i++)
            {
                Tocar(QuadroMorte + i, 1, 1f, false);

                // No quadro em que ela sai da cabeca, a coroa de verdade voa.
                if (i == 3) StartCoroutine(VooDaCoroa());
                yield return new WaitForSeconds(segurar[i]);
            }

            // A camera vai ate a coroa, la longe do trono, e volta.
            if (coroaCaido != null)
            {
                ScrapCameraFollow.Direcionar((Vector2)coroaCaido.transform.position + Vector2.up * 1.5f, 0.85f, 0.7f);
                yield return new WaitForSeconds(1.4f);
                ScrapCameraFollow.Direcionar((Vector2)transform.position + Vector2.up * 2.5f, 0.85f, 0.7f);
                yield return new WaitForSeconds(0.6f);
            }

            yield return ScriptedDialogue.TocarComo("king_corrupt", Nome, _sprite, UltimoLinhas);

            // E deita.
            Tocar(QuadroMorte + 6, 1, 1f, false);
            Sfx.BaixoDeitarRei.Tocar(transform.position);
            yield return new WaitForSeconds(0.8f);
            Tocar(QuadroMorte + 7, 1, 1f, false);
            yield return new WaitForSeconds(0.3f);
            DeixarDestrocos();

            if (Lutando == this) Lutando = null;
            if (portaoAltar != null) portaoAltar.Abrir();

            ScrapCameraFollow.Liberar();
            if (controlador != null) controlador.ControleTravado = false;
            if (comecou) Cinematic.Encerrar();
            _cena = false;
        }

        /// <summary>
        /// A coroa sai da cabeca dele, sobe girando, cai longe do trono, quica
        /// duas vezes e para no lugar dela (onde o gerador a deixou).
        /// </summary>
        private IEnumerator VooDaCoroa()
        {
            if (coroaCaido == null) yield break;

            Transform coroa = coroaCaido.transform;
            Vector3 alvo = coroa.position;
            Vector3 inicio = transform.position + new Vector3(pontoCoroa.x * _direcaoOlhar, pontoCoroa.y * 0.8f, 0f);
            coroa.position = inicio;
            coroa.rotation = Quaternion.identity;
            coroaCaido.SetActive(true);
            Sfx.RachaduraCoroaRei.Tocar(inicio, tom2: 1.3f, volume: 0.6f);

            // O arco: sobe acima da cabeca e desce no lugar, girando.
            const float tempo = 1.1f;
            float apice = Mathf.Max(inicio.y, alvo.y) + 3f;
            float giro = Mathf.Sign(alvo.x - inicio.x) * -720f;
            for (float t = 0f; t < tempo; t += Time.deltaTime)
            {
                float k = t / tempo;
                float x = Mathf.Lerp(inicio.x, alvo.x, k);
                float y = k < 0.5f
                    ? Mathf.Lerp(inicio.y, apice, 1f - (1f - k * 2f) * (1f - k * 2f))
                    : Mathf.Lerp(apice, alvo.y, (k - 0.5f) * 2f * ((k - 0.5f) * 2f));
                coroa.position = new Vector3(x, y, alvo.z);
                coroa.rotation = Quaternion.Euler(0f, 0f, giro * k);
                yield return null;
            }

            // Quica duas vezes, cada vez mais baixo, e assenta reta.
            float[] saltos = { 0.7f, 0.25f };
            foreach (float salto in saltos)
            {
                coroa.position = alvo;
                Sfx.QuadroBloqueio.Tocar(alvo, tom2: 0.8f + salto * 0.3f);
                ScrapCameraFollow.Tremer(0.06f * salto, 0.2f);
                float extensao = 0.18f + salto * 0.25f;
                float de = coroa.eulerAngles.z;
                for (float t = 0f; t < extensao; t += Time.deltaTime)
                {
                    float k = t / extensao;
                    coroa.position = alvo + Vector3.up * (salto * 4f * k * (1f - k));
                    coroa.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(de, 0f, k));
                    yield return null;
                }
            }

            coroa.position = alvo;
            coroa.rotation = Quaternion.identity;
            Sfx.QuadroBloqueio.Tocar(alvo, tom2: 0.7f, volume: 0.6f);
        }

        /// <summary>Deitado, troca pelo loop da carcaca: a armadura rachada e o Oxide saindo em fiapos.</summary>
        private void DeixarDestrocos()
        {
            if (destrocos == null || _sprite == null) return;
            bool inverter = _sprite.flipX;
            _sprite.enabled = false;
            BossWreck.Mostrar(destrocos, transform.position, inverter);
        }

        private void PararAtaques()
        {
            if (aRaio != null) aRaio.Esconder();
            if (bRaio != null) bRaio.Esconder();
            if (sopro != null) sopro.Dissipar();
        }

        /// <summary>O Scrap morreu no meio da luta: tudo volta ao comeco — o Rei sentado no trono, na fase 1.</summary>
        private void ReiniciarLuta()
        {
            if (_morto || _cena) return;

            StopAllCoroutines();
            PararAtaques();
            _acordado = false;
            _carregando = false;
            _recuavel = false;
            _expostoAte = 0f;
            _ultimoAtaque = -1;
            if (_sprite != null) _sprite.color = Color.white;
            if (Lutando == this) Lutando = null;

            EntrarFaseUm();
        }

        // ------------------------------------------------------------------
        // Encostar
        // ------------------------------------------------------------------

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void Encostar(Collider2D outro)
        {
            // Exposto ele nao machuca: e a hora de chegar perto.
            if (_morto || !_acordado || _cena || EstaExposto) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(_carregando ? 3 : 2, transform.position);
        }
    }
}
