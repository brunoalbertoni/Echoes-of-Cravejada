using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O regente do som. Nasce sozinho ao dar Play, atravessa as cenas e, a
    /// cada quadro, decide que musica toca, quao alto o ambiente fica e prende
    /// os sons aos objetos do mapa quando uma cena abre.
    ///
    /// O que toca, do mais importante pro menos:
    /// 1. o final (O Grande Polimento);
    /// 2. a purificacao do distrito, ate sair da cena ou ela acabar;
    /// 3. a luta de chefe (no Rei, uma musica por fase e silencio na transformacao);
    /// 4. o silencio depois que um chefe cai, ate a cena de queda acabar;
    /// 5. a fuga do acido, na fase 2;
    /// 6. a musica de explorar a fase.
    ///
    /// A tabela de musicas e volumes fica em <see cref="SoundtrackCatalog"/>.
    /// </summary>
    public class SoundDirector : MonoBehaviour
    {
        private const string CenaFugaAcido = "Phase02_BrassPipelines";
        private const string CenaPalacio = "Phase05_SilverCity";

        /// <summary>Quanto tempo sem cena depois de um chefe cair ate a exploracao voltar.</summary>
        private const float SilencioDepoisChefe = 3f;

        /// <summary>O volume da musica durante os dialogos.</summary>
        private const float AbaixarDialogo = 0.7f;

        private static SoundDirector _instancia;

        /// <summary>O volume dos sons presos a objetos agora (abaixa na luta e no final).</summary>
        public static float NivelEmissor { get; private set; } = SoundtrackCatalog.VolumeEmissor;

        private MusicPlayer _musica;
        private readonly AudioSource[] _camadas = new AudioSource[2];
        private readonly float[] _niveisCamada = new float[2];
        private int _camada;

        private string _cena;
        private SoundPlace _lugar;
        private float _inicioCena;
        private float _ultimoMorte = -100f;

        private string _cenaPurificado;
        private bool _prontoPurificacao;

        private MonoBehaviour _chefe;
        private bool _iniciadoMusicaChefe;
        private bool _silencio;
        private float _quietoPor;

        private bool _batalha;
        private bool _finalJogo;
        private float _abaixar = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
            NivelEmissor = SoundtrackCatalog.VolumeEmissor;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Inicializar()
        {
            if (_instancia != null) return;

            var hospedeiro = new GameObject("SoundDirector");
            DontDestroyOnLoad(hospedeiro);
            _instancia = hospedeiro.AddComponent<SoundDirector>();
        }

        private void Awake()
        {
            _musica = new MusicPlayer(gameObject);

            for (int i = 0; i < _camadas.Length; i++)
            {
                var camada = gameObject.AddComponent<AudioSource>();
                camada.loop = true;
                camada.playOnAwake = false;
                camada.spatialBlend = 0f;
                camada.priority = 8;
                camada.volume = 0f;
                _camadas[i] = camada;
            }

            SceneManager.sceneLoaded += AoCarregarCena;
            ScrapHealth.Morreu += AoScrapMorrer;
            AreaPurifier.Purificando += AoPurificar;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            ScrapHealth.Morreu -= AoScrapMorrer;
            AreaPurifier.Purificando -= AoPurificar;
            if (_instancia == this) _instancia = null;
        }

        // ------------------------------------------------------------------
        // O que acontece no jogo
        // ------------------------------------------------------------------

        private void AoCarregarCena(Scene cenaAtual, LoadSceneMode modo)
        {
            if (modo == LoadSceneMode.Additive) return;

            _cena = cenaAtual.name;
            _lugar = SoundtrackCatalog.ParaCena(_cena);
            _inicioCena = Time.time;
            _chefe = null;
            _iniciadoMusicaChefe = false;
            _silencio = false;

            if (_cenaPurificado != _cena)
            {
                _cenaPurificado = null;
                _prontoPurificacao = false;
            }

            TrocarAmbiente(_lugar != null ? CarregarAmbiente(_lugar.Ambiente) : null);
            PrenderEmissores();
        }

        private void AoScrapMorrer()
        {
            _ultimoMorte = Time.time;
        }

        private void AoPurificar()
        {
            _cenaPurificado = SceneManager.GetActiveScene().name;
            _prontoPurificacao = false;
            _silencio = false;
        }

        // ------------------------------------------------------------------
        // Cada quadro
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            MusicTrack desejo = Decidir(dt);

            // A musica abaixa um pouco enquanto alguem fala.
            bool conversando = Npc.Conversando != null || ScriptedDialogue.Ativo;
            _abaixar = Mathf.MoveTowards(_abaixar, conversando ? AbaixarDialogo : 1f, dt * 1.5f);

            _musica.Volume = SoundtrackCatalog.VolumeMusica * (_lugar != null ? _lugar.EscalaMusica : 1f) * _abaixar;

            // A luta entra rapido; o final chega devagar; o resto, no tempo normal.
            float esmaecimento = _finalJogo ? 5f : _batalha ? 1.2f : SoundtrackCatalog.TempoTransicao;
            _musica.Pedir(desejo, esmaecimento);
            _musica.AtualizarMusica(dt);

            if (_cenaPurificado == _cena && !_prontoPurificacao && _musica.JaTerminou(SoundtrackCatalog.Purificacao))
            {
                _prontoPurificacao = true;
            }

            AtualizarAmbientes(dt);

            float emissores = SoundtrackCatalog.VolumeEmissor * (_batalha ? 0.6f : 1f) * (_finalJogo ? 0f : 1f);
            NivelEmissor = Mathf.MoveTowards(NivelEmissor, emissores, dt * (_finalJogo ? 0.12f : 0.5f));
        }

        private MusicTrack Decidir(float dt)
        {
            _batalha = false;
            _finalJogo = FinaleDirector.Correndo;

            // 1. O final.
            if (_finalJogo) return SoundtrackCatalog.FinalJogo;

            // 2. A purificacao deste distrito.
            if (_cenaPurificado != null && _cenaPurificado == _cena && !_prontoPurificacao)
            {
                return SoundtrackCatalog.Purificacao;
            }

            // 3. Chefe em luta.
            MonoBehaviour chefe = ChefeEmLuta();
            if (chefe != _chefe)
            {
                // O que estava lutando saiu: caiu de vez (silencio) ou o Scrap morreu (volta a explorar).
                if (_chefe != null && chefe == null && EstaCaido(_chefe)) IniciarSilencio();
                _chefe = chefe;
                _iniciadoMusicaChefe = false;
            }

            if (chefe != null)
            {
                if (EstaCaido(chefe))
                {
                    IniciarSilencio();
                    return null;
                }

                var luta = (IBossFight)chefe;

                // Quem cuida da propria musica (o Rei) nao espera a cena acabar.
                if (luta.CinematicasIgnoraMusica)
                {
                    MusicTrack faixa = luta.MusicaBatalha;
                    if (faixa == null) return null;
                    _batalha = true;
                    return faixa;
                }

                // Os outros: se ele acordou dentro de uma cena, a musica espera ela acabar.
                if (!_iniciadoMusicaChefe && Cinematic.Ativo) return null;

                _iniciadoMusicaChefe = true;
                _batalha = true;
                return luta.MusicaBatalha;
            }

            // 4. Um chefe acabou de cair: silencio ate a cena de queda acabar.
            if (_silencio)
            {
                _quietoPor = Cinematic.Ativo ? 0f : _quietoPor + Time.deltaTime;
                if (_quietoPor < SilencioDepoisChefe) return null;
                _silencio = false;
            }

            // 5. A fuga do acido.
            if (_cena == CenaFugaAcido && RisingAcidTrigger.TempoInicio >= _inicioCena
                && RisingAcidTrigger.TempoInicio > _ultimoMorte && !AreaPurifier.EstaPurificado)
            {
                _batalha = true;
                return SoundtrackCatalog.FugaAcido;
            }

            // 6. Explorar.
            if (_lugar == null) return null;

            // Cena abrindo: um respiro antes da musica, e se abriu com cutscene (o despertar), ela entra quando a cena acaba.
            if (_musica.Atual == null && (Cinematic.Ativo || Time.time - _inicioCena < 0.8f)) return null;

            if (_cena == CenaPalacio && SilverKing.Derrotado) return SoundtrackCatalog.PalacioDepoisRei;
            return _lugar.Explorar;
        }

        private void IniciarSilencio()
        {
            _silencio = true;
            _quietoPor = 0f;
        }

        // ------------------------------------------------------------------
        // Chefes
        // ------------------------------------------------------------------

        private static MonoBehaviour ChefeEmLuta() => BossRoster.Atual;

        private static bool EstaCaido(MonoBehaviour chefe) => !(chefe is IBossFight luta) || luta.EstaDerrubado;

        // ------------------------------------------------------------------
        // Ambiente
        // ------------------------------------------------------------------

        private static AudioClip CarregarAmbiente(string clipe)
        {
            if (string.IsNullOrEmpty(clipe)) return null;

            AudioClip carregado = Resources.Load<AudioClip>("Soundtrack/Ambience/" + clipe);
            if (carregado == null) Debug.LogWarning("[Som] Ambiente nao encontrado: Resources/Soundtrack/Ambience/" + clipe);
            return carregado;
        }

        /// <summary>Troca a cama de ambiente: a nova entra numa fonte enquanto a velha sai da outra.</summary>
        private void TrocarAmbiente(AudioClip clipe)
        {
            if (_camadas[_camada].clip == clipe && clipe != null) return;

            _camada = 1 - _camada;
            AudioSource camada = _camadas[_camada];
            camada.Stop();
            camada.clip = clipe;
            _niveisCamada[_camada] = 0f;

            if (clipe != null)
            {
                camada.volume = 0f;
                camada.Play();
                camada.time = Random.Range(0f, Mathf.Max(0f, clipe.length - 1f));
            }
        }

        private void AtualizarAmbientes(float dt)
        {
            float nivel = _lugar != null ? SoundtrackCatalog.VolumeAmbiente * _lugar.EscalaAmbiente : 0f;
            if (_batalha) nivel *= SoundtrackCatalog.AmbienteAbaixoBatalha;
            if (_finalJogo) nivel = 0f;

            for (int i = 0; i < _camadas.Length; i++)
            {
                float alvo = i == _camada ? nivel : 0f;
                // Some devagar no final; nas trocas, uns dois segundos.
                float velocidade = _finalJogo ? 0.08f : 0.5f;
                _niveisCamada[i] = Mathf.MoveTowards(_niveisCamada[i], alvo, dt * velocidade);
                _camadas[i].volume = Mathf.Min(1f, _niveisCamada[i]) * GameAudio.NivelEfeitos;

                if (i != _camada && _niveisCamada[i] <= 0f && _camadas[i].isPlaying) _camadas[i].Stop();
            }
        }

        /// <summary>Prende os sons as coisas do mapa que tem som: vapor, caldeira, acido, cristal.</summary>
        private static void PrenderEmissores()
        {
            AudioClip chiado = null, tremor2 = null, bolhas = null, zumbido = null, raio = null;

            foreach (SteamVent saida in FindObjectsByType<SteamVent>())
            {
                if (saida.GetComponent<AmbientEmitter>() != null) continue;
                chiado = chiado != null ? chiado : CarregarAmbiente(SoundtrackCatalog.ChiadoVapor);
                SteamVent v = saida;
                AmbientEmitter.Prender(saida.gameObject, chiado, 0.7f, 1.5f, 9f, () => v != null && v.EstaLigado ? 1f : 0f);
            }

            foreach (PressureBoiler caldeira in FindObjectsByType<PressureBoiler>())
            {
                if (caldeira.GetComponent<AmbientEmitter>() != null) continue;
                tremor2 = tremor2 != null ? tremor2 : CarregarAmbiente(SoundtrackCatalog.TremorCaldeira);
                PressureBoiler b = caldeira;
                AmbientEmitter.Prender(caldeira.gameObject, tremor2, 0.9f, 2f, 12f,
                    () => b != null && b.Pressao < 0.999f ? 0.7f + 0.3f * b.Pressao : 0f);
            }

            foreach (AcidPool poca in FindObjectsByType<AcidPool>())
            {
                if (poca.GetComponent<AmbientEmitter>() != null) continue;
                bolhas = bolhas != null ? bolhas : CarregarAmbiente(SoundtrackCatalog.BolhasAcido);
                AcidPool p = poca;
                AmbientEmitter.Prender(poca.gameObject, bolhas, 0.6f, 2f, 10f, () => p != null ? Mathf.Clamp01(p.Nivel / 0.4f) : 0f);
            }

            foreach (ResonantCrystal cristal in FindObjectsByType<ResonantCrystal>())
            {
                if (cristal.GetComponent<AmbientEmitter>() != null) continue;
                zumbido = zumbido != null ? zumbido : CarregarAmbiente(SoundtrackCatalog.ZumbidoCristal);
                ResonantCrystal c = cristal;
                AmbientEmitter.Prender(cristal.gameObject, zumbido, 0.45f, 1f, 6f, () => c != null && c.EstaAceso ? 1f : 0.4f);
            }

            // Os raios de luz da fase 4 zumbem no emissor, so enquanto estao acesos.
            foreach (LightBeam emissor in FindObjectsByType<LightBeam>())
            {
                if (emissor.GetComponent<AmbientEmitter>() != null) continue;
                raio = raio != null ? raio : SfxPlayer.Carregar(Sfx.ZumbidoRaio.Clipe);
                LightBeam b = emissor;
                AmbientEmitter.Prender(emissor.gameObject, raio, Sfx.ZumbidoRaio.Volume, 1.5f, 8f,
                    () => b != null && b.isActiveAndEnabled && b.EstaLigado ? 1f : 0f);
            }
        }
    }
}
