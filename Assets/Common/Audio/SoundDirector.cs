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
        private const string AcidEscapeScene = "Phase02_BrassPipelines";
        private const string PalaceScene = "Phase05_SilverCity";

        /// <summary>Quanto tempo sem cena depois de um chefe cair ate a exploracao voltar.</summary>
        private const float HushAfterBoss = 3f;

        private static SoundDirector _instance;

        /// <summary>O volume dos sons presos a objetos agora (abaixa na luta e no final).</summary>
        public static float EmitterLevel { get; private set; } = SoundtrackCatalog.EmitterVolume;

        private MusicPlayer _music;
        private readonly AudioSource[] _beds = new AudioSource[2];
        private readonly float[] _bedLevels = new float[2];
        private int _bed;

        private string _scene;
        private SoundPlace _place;
        private float _sceneStart;
        private float _lastDeath = -100f;

        private string _purifiedScene;
        private bool _purificationDone;

        private MonoBehaviour _boss;
        private bool _bossMusicStarted;
        private bool _hush;
        private float _quietFor;

        private bool _battle;
        private bool _finale;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
            EmitterLevel = SoundtrackCatalog.EmitterVolume;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (_instance != null) return;

            var host = new GameObject("SoundDirector");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<SoundDirector>();
        }

        private void Awake()
        {
            _music = new MusicPlayer(gameObject);

            for (int i = 0; i < _beds.Length; i++)
            {
                var bed = gameObject.AddComponent<AudioSource>();
                bed.loop = true;
                bed.playOnAwake = false;
                bed.spatialBlend = 0f;
                bed.priority = 8;
                bed.volume = 0f;
                _beds[i] = bed;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            ScrapHealth.Died += OnScrapDied;
            AreaPurifier.Purifying += OnPurifying;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ScrapHealth.Died -= OnScrapDied;
            AreaPurifier.Purifying -= OnPurifying;
            if (_instance == this) _instance = null;
        }

        // ------------------------------------------------------------------
        // O que acontece no jogo
        // ------------------------------------------------------------------

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;

            _scene = scene.name;
            _place = SoundtrackCatalog.ForScene(_scene);
            _sceneStart = Time.time;
            _boss = null;
            _bossMusicStarted = false;
            _hush = false;

            if (_purifiedScene != _scene)
            {
                _purifiedScene = null;
                _purificationDone = false;
            }

            SwitchBed(_place != null ? LoadAmbience(_place.Ambience) : null);
            AttachEmitters();
        }

        private void OnScrapDied()
        {
            _lastDeath = Time.time;
        }

        private void OnPurifying()
        {
            _purifiedScene = SceneManager.GetActiveScene().name;
            _purificationDone = false;
            _hush = false;
        }

        // ------------------------------------------------------------------
        // Cada quadro
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            MusicTrack want = Decide(dt);
            _music.Volume = SoundtrackCatalog.MusicVolume * (_place != null ? _place.MusicScale : 1f);

            // A luta entra rapido; o final chega devagar; o resto, no tempo normal.
            float fade = _finale ? 5f : _battle ? 1.2f : SoundtrackCatalog.CrossfadeTime;
            _music.Want(want, fade);
            _music.Update(dt);

            if (_purifiedScene == _scene && !_purificationDone && _music.HasFinished(SoundtrackCatalog.Purification))
            {
                _purificationDone = true;
            }

            UpdateBeds(dt);

            float emitters = SoundtrackCatalog.EmitterVolume * (_battle ? 0.6f : 1f) * (_finale ? 0f : 1f);
            EmitterLevel = Mathf.MoveTowards(EmitterLevel, emitters, dt * (_finale ? 0.12f : 0.5f));
        }

        private MusicTrack Decide(float dt)
        {
            _battle = false;
            _finale = FinaleDirector.Running;

            // 1. O final.
            if (_finale) return SoundtrackCatalog.Finale;

            // 2. A purificacao deste distrito.
            if (_purifiedScene != null && _purifiedScene == _scene && !_purificationDone)
            {
                return SoundtrackCatalog.Purification;
            }

            // 3. Chefe em luta.
            MonoBehaviour boss = FightingBoss();
            if (boss != _boss)
            {
                // O que estava lutando saiu: caiu de vez (silencio) ou o Scrap morreu (volta a explorar).
                if (_boss != null && boss == null && IsDown(_boss)) StartHush();
                _boss = boss;
                _bossMusicStarted = false;
            }

            if (boss != null)
            {
                if (IsDown(boss))
                {
                    StartHush();
                    return null;
                }

                if (boss is SilverKing king)
                {
                    // A entrada e a transformacao ficam sem musica; cada fase tem a sua.
                    if (king.IsTransforming) return null;
                    _battle = true;
                    return king.IsPhaseTwo ? SoundtrackCatalog.SilverKingCorrupted : SoundtrackCatalog.SilverKingRadiant;
                }

                // Os outros: se ele acordou dentro de uma cena, a musica espera ela acabar.
                if (!_bossMusicStarted && Cinematic.Active) return null;

                _bossMusicStarted = true;
                _battle = true;
                return BossTrack(boss);
            }

            // 4. Um chefe acabou de cair: silencio ate a cena de queda acabar.
            if (_hush)
            {
                _quietFor = Cinematic.Active ? 0f : _quietFor + dt;
                if (_quietFor < HushAfterBoss) return null;
                _hush = false;
            }

            // 5. A fuga do acido.
            if (_scene == AcidEscapeScene && RisingAcidTrigger.StartTime >= _sceneStart
                && RisingAcidTrigger.StartTime > _lastDeath && !AreaPurifier.IsPurified)
            {
                _battle = true;
                return SoundtrackCatalog.AcidEscape;
            }

            // 6. Explorar.
            if (_place == null) return null;

            // Cena abrindo: um respiro antes da musica, e se abriu com cutscene (o despertar), ela entra quando a cena acaba.
            if (_music.Current == null && (Cinematic.Active || Time.time - _sceneStart < 0.8f)) return null;

            if (_scene == PalaceScene && SilverKing.Defeated) return SoundtrackCatalog.PalaceAfterKing;
            return _place.Explore;
        }

        private void StartHush()
        {
            _hush = true;
            _quietFor = 0f;
        }

        // ------------------------------------------------------------------
        // Chefes
        // ------------------------------------------------------------------

        private static MonoBehaviour FightingBoss()
        {
            if (SilverKing.Fighting != null) return SilverKing.Fighting;
            if (RustyScrapper.Fighting != null) return RustyScrapper.Fighting;
            if (BoilerMaster.Fighting != null) return BoilerMaster.Fighting;
            if (TheResonant.Fighting != null) return TheResonant.Fighting;
            if (GeodeDrill.Fighting != null) return GeodeDrill.Fighting;
            if (MercuryAlchemist.Fighting != null) return MercuryAlchemist.Fighting;
            return null;
        }

        private static MusicTrack BossTrack(MonoBehaviour boss)
        {
            switch (boss)
            {
                case RustyScrapper _: return SoundtrackCatalog.RustyScrapper;
                case BoilerMaster _: return SoundtrackCatalog.BoilerMaster;
                case TheResonant _: return SoundtrackCatalog.TheResonant;
                case GeodeDrill _: return SoundtrackCatalog.GeodeDrill;
                case MercuryAlchemist _: return SoundtrackCatalog.MercuryAlchemist;
                default: return null;
            }
        }

        private static bool IsDown(MonoBehaviour boss)
        {
            switch (boss)
            {
                case RustyScrapper b: return b.IsDown;
                case BoilerMaster b: return b.IsDown;
                case TheResonant b: return b.IsDown;
                case GeodeDrill b: return b.IsDown;
                case MercuryAlchemist b: return b.IsDown;
                case SilverKing b: return b.IsDown;
                default: return true;
            }
        }

        // ------------------------------------------------------------------
        // Ambiente
        // ------------------------------------------------------------------

        private static AudioClip LoadAmbience(string clip)
        {
            if (string.IsNullOrEmpty(clip)) return null;

            AudioClip loaded = Resources.Load<AudioClip>("Soundtrack/Ambience/" + clip);
            if (loaded == null) Debug.LogWarning("[Som] Ambiente nao encontrado: Resources/Soundtrack/Ambience/" + clip);
            return loaded;
        }

        /// <summary>Troca a cama de ambiente: a nova entra numa fonte enquanto a velha sai da outra.</summary>
        private void SwitchBed(AudioClip clip)
        {
            if (_beds[_bed].clip == clip && clip != null) return;

            _bed = 1 - _bed;
            AudioSource bed = _beds[_bed];
            bed.Stop();
            bed.clip = clip;
            _bedLevels[_bed] = 0f;

            if (clip != null)
            {
                bed.volume = 0f;
                bed.Play();
                bed.time = Random.Range(0f, Mathf.Max(0f, clip.length - 1f));
            }
        }

        private void UpdateBeds(float dt)
        {
            float level = _place != null ? SoundtrackCatalog.AmbienceVolume * _place.AmbienceScale : 0f;
            if (_battle) level *= SoundtrackCatalog.AmbienceUnderBattle;
            if (_finale) level = 0f;

            for (int i = 0; i < _beds.Length; i++)
            {
                float target = i == _bed ? level : 0f;
                // Some devagar no final; nas trocas, uns dois segundos.
                float speed = _finale ? 0.08f : 0.5f;
                _bedLevels[i] = Mathf.MoveTowards(_bedLevels[i], target, dt * speed);
                _beds[i].volume = Mathf.Min(1f, _bedLevels[i]);

                if (i != _bed && _bedLevels[i] <= 0f && _beds[i].isPlaying) _beds[i].Stop();
            }
        }

        /// <summary>Prende os sons as coisas do mapa que tem som: vapor, caldeira, acido, cristal.</summary>
        private static void AttachEmitters()
        {
            AudioClip hiss = null, rumble = null, bubbles = null, hum = null, beam = null;

            foreach (SteamVent vent in FindObjectsByType<SteamVent>())
            {
                if (vent.GetComponent<AmbientEmitter>() != null) continue;
                hiss = hiss != null ? hiss : LoadAmbience(SoundtrackCatalog.SteamHiss);
                SteamVent v = vent;
                AmbientEmitter.Attach(vent.gameObject, hiss, 0.7f, 1.5f, 9f, () => v != null && v.IsOn ? 1f : 0f);
            }

            foreach (PressureBoiler boiler in FindObjectsByType<PressureBoiler>())
            {
                if (boiler.GetComponent<AmbientEmitter>() != null) continue;
                rumble = rumble != null ? rumble : LoadAmbience(SoundtrackCatalog.BoilerRumble);
                PressureBoiler b = boiler;
                AmbientEmitter.Attach(boiler.gameObject, rumble, 0.9f, 2f, 12f,
                    () => b != null && b.Pressure < 0.999f ? 0.7f + 0.3f * b.Pressure : 0f);
            }

            foreach (AcidPool pool in FindObjectsByType<AcidPool>())
            {
                if (pool.GetComponent<AmbientEmitter>() != null) continue;
                bubbles = bubbles != null ? bubbles : LoadAmbience(SoundtrackCatalog.AcidBubbles);
                AcidPool p = pool;
                AmbientEmitter.Attach(pool.gameObject, bubbles, 0.6f, 2f, 10f, () => p != null ? Mathf.Clamp01(p.Level / 0.4f) : 0f);
            }

            foreach (ResonantCrystal crystal in FindObjectsByType<ResonantCrystal>())
            {
                if (crystal.GetComponent<AmbientEmitter>() != null) continue;
                hum = hum != null ? hum : LoadAmbience(SoundtrackCatalog.CrystalHum);
                ResonantCrystal c = crystal;
                AmbientEmitter.Attach(crystal.gameObject, hum, 0.45f, 1f, 6f, () => c != null && c.IsLit ? 1f : 0.4f);
            }

            // Os raios de luz da fase 4 zumbem no emissor, so enquanto estao acesos.
            foreach (LightBeam emitter in FindObjectsByType<LightBeam>())
            {
                if (emitter.GetComponent<AmbientEmitter>() != null) continue;
                beam = beam != null ? beam : SfxPlayer.Load(Sfx.BeamHum.Clip);
                LightBeam b = emitter;
                AmbientEmitter.Attach(emitter.gameObject, beam, Sfx.BeamHum.Volume, 1.5f, 8f,
                    () => b != null && b.isActiveAndEnabled && b.IsOn ? 1f : 0f);
            }
        }
    }
}
