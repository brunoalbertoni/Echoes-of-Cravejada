using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Toca as musicas da trilha com duas fontes, pra uma entrar enquanto a
    /// outra sai: troca de musica e a volta do loop sao sempre em fade.
    ///
    /// Quem usa pede a musica que quer agora (<see cref="Pedir"/>) todo quadro;
    /// pedir a mesma de novo nao reinicia.
    /// </summary>
    public sealed class MusicPlayer
    {
        private sealed class Voice
        {
            public AudioSource Fonte;
            public MusicTrack FaixaMusica;
            public float Nivel;
            public float Alvo;
            public float Velocidade;
            public float Desde;
        }

        private static readonly Dictionary<string, AudioClip> Clipes = new Dictionary<string, AudioClip>();

        private readonly Voice[] _vozes = new Voice[2];
        private Voice _atual;

        /// <summary>A musica tocando agora (a que entrou por ultimo), ou null.</summary>
        public MusicTrack Atual => _atual != null ? _atual.FaixaMusica : null;

        /// <summary>A ultima musica de tocar uma vez so que chegou ao fim (e nao foi pedida outra depois).</summary>
        private MusicTrack _terminado;

        /// <summary>Se esta musica (de tocar uma vez so) ja tocou ate o fim.</summary>
        public bool JaTerminou(MusicTrack faixa) => faixa != null && _terminado == faixa;

        /// <summary>Multiplica o volume de tudo (o volume geral e o da cena).</summary>
        public float Volume { get; set; } = 1f;

        public MusicPlayer(GameObject hospedeiro)
        {
            for (int i = 0; i < _vozes.Length; i++)
            {
                var fonte = hospedeiro.AddComponent<AudioSource>();
                fonte.playOnAwake = false;
                fonte.loop = false;
                fonte.spatialBlend = 0f;
                fonte.priority = 0;
                fonte.volume = 0f;
                _vozes[i] = new Voice { Fonte = fonte };
            }
        }

        /// <summary>Carrega uma musica pelo nome (cache). Null se o arquivo nao existe.</summary>
        public static AudioClip Carregar(string clipe)
        {
            if (string.IsNullOrEmpty(clipe)) return null;
            if (Clipes.TryGetValue(clipe, out AudioClip guardado) && guardado != null) return guardado;

            AudioClip carregado = Resources.Load<AudioClip>("Soundtrack/Music/" + clipe);
            if (carregado == null) Debug.LogWarning("[Som] Musica nao encontrada: Resources/Soundtrack/Music/" + clipe);
            Clipes[clipe] = carregado;
            return carregado;
        }

        /// <summary>
        /// A musica que deve tocar agora. Null: silencio (a atual sai em fade).
        /// Com <paramref name="reiniciar"/>, recomeca mesmo se ja for ela.
        /// </summary>
        public void Pedir(MusicTrack faixa, float esmaecimento = SoundtrackCatalog.TempoTransicao, bool reiniciar = false)
        {
            if (!reiniciar && _atual != null && _atual.FaixaMusica == faixa) return;
            // A que ja tocou ate o fim nao volta sozinha.
            if (!reiniciar && faixa != null && faixa == _terminado) return;
            if (faixa == null && _atual == null) return;

            if (_atual != null) DesaparecerAosPoucos(_atual, esmaecimento);
            _atual = null;
            _terminado = null;

            if (faixa == null) return;

            AudioClip clipe = Carregar(faixa.Clipe);
            if (clipe == null)
            {
                // Sem o arquivo, finge que tocou: quem pediu nao fica esperando pra sempre.
                if (!faixa.Repeticao) _terminado = faixa;
                return;
            }

            Voice voz = VozLivre();
            Iniciar(voz, faixa, clipe, faixa.Inicio, esmaecimento);
            _atual = voz;
        }

        /// <summary>Tira tudo, em fade.</summary>
        public void Parar(float esmaecimento)
        {
            foreach (Voice voz in _vozes) DesaparecerAosPoucos(voz, esmaecimento);
            _atual = null;
        }

        public void AtualizarMusica(float tempoDelta)
        {
            foreach (Voice voz in _vozes)
            {
                if (voz.FaixaMusica == null) continue;

                voz.Nivel = Mathf.MoveTowards(voz.Nivel, voz.Alvo, voz.Velocidade * tempoDelta);
                voz.Fonte.volume = voz.Nivel * voz.FaixaMusica.Volume * Volume * GameAudio.NivelMusica;

                if (voz.Alvo <= 0f && voz.Nivel <= 0f)
                {
                    voz.Fonte.Stop();
                    voz.FaixaMusica = null;
                    continue;
                }

                if (voz != _atual) continue;

                AudioClip clipe = voz.Fonte.clip;
                if (clipe == null) continue;

                // Logo depois do Play a fonte pode ainda nao ter comecado (streaming): espera um pouco antes de julgar.
                bool assentado = Time.unscaledTime - voz.Desde > 1f;

                if (voz.FaixaMusica.Repeticao)
                {
                    // Chegando no ponto de volta: a outra fonte entra no comeco do loop.
                    float fim = voz.FaixaMusica.FimRepeticao > 0f ? Mathf.Min(voz.FaixaMusica.FimRepeticao, clipe.length) : clipe.length;
                    if (voz.Fonte.time >= fim - SoundtrackCatalog.TempoEsmaecerRepeticao || (assentado && !voz.Fonte.isPlaying))
                    {
                        Voice proximo = OutraVoz(voz);
                        DesaparecerAosPoucos(voz, SoundtrackCatalog.TempoEsmaecerRepeticao);
                        Iniciar(proximo, voz.FaixaMusica, clipe, voz.FaixaMusica.RepeticaoDe, SoundtrackCatalog.TempoEsmaecerRepeticao);
                        _atual = proximo;
                    }
                }
                else if (assentado && !voz.Fonte.isPlaying)
                {
                    _terminado = voz.FaixaMusica;
                    voz.FaixaMusica = null;
                    _atual = null;
                }
            }
        }

        private void Iniciar(Voice voz, MusicTrack faixa, AudioClip clipe, float at, float esmaecimento)
        {
            voz.Fonte.Stop();
            voz.Fonte.clip = clipe;
            voz.Fonte.volume = 0f;
            voz.Fonte.Play();
            voz.Fonte.time = Mathf.Clamp(at, 0f, Mathf.Max(0f, clipe.length - 1f));
            voz.Desde = Time.unscaledTime;
            voz.FaixaMusica = faixa;
            voz.Nivel = 0f;
            voz.Alvo = 1f;
            voz.Velocidade = 1f / Mathf.Max(0.05f, esmaecimento);
            voz.Fonte.volume = 0f;
        }

        private static void DesaparecerAosPoucos(Voice voz, float esmaecimento)
        {
            if (voz == null || voz.FaixaMusica == null) return;

            voz.Alvo = 0f;
            voz.Velocidade = Mathf.Max(voz.Nivel, 0.01f) / Mathf.Max(0.05f, esmaecimento);
        }

        private Voice VozLivre()
        {
            // A que estiver mais baixa: a outra pode estar saindo em fade.
            return _vozes[0].FaixaMusica == null ? _vozes[0]
                : _vozes[1].FaixaMusica == null ? _vozes[1]
                : _vozes[0].Nivel <= _vozes[1].Nivel ? _vozes[0] : _vozes[1];
        }

        private Voice OutraVoz(Voice voz)
        {
            return voz == _vozes[0] ? _vozes[1] : _vozes[0];
        }
    }
}
