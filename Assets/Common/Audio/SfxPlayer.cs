using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Quem toca os efeitos sonoros. Nasce sozinho ao dar Play, atravessa as
    /// cenas e guarda um punhado de vozes: cada <see cref="SfxCue"/> tocado
    /// pega uma livre (ou a mais velha, se todas estiverem ocupadas).
    ///
    /// Ao abrir uma cena, prende o <see cref="ScrapSounds"/> no Scrap (passos,
    /// quedas, a lixa). Nenhuma cena precisa ser regerada.
    ///
    /// Os sons tocam mesmo com o tempo parado (o card de habilidade para o jogo).
    /// </summary>
    public class SfxPlayer : MonoBehaviour
    {
        private const int QuantidadeVoz = 24;

        /// <summary>Ate esta distancia da camera (em tiles) um som do mapa soa cheio.</summary>
        private const float Perto = 9f;

        /// <summary>A partir desta, silencio.</summary>
        private const float Longe = 22f;

        private static SfxPlayer _instancia;
        private static readonly Dictionary<string, AudioClip> Clipes = new Dictionary<string, AudioClip>();

        private readonly AudioSource[] _vozes = new AudioSource[QuantidadeVoz];
        private readonly float[] _iniciadoEm = new float[QuantidadeVoz];
        private readonly Dictionary<SfxCue, float> _ultimoTocado = new Dictionary<SfxCue, float>();
        private readonly Dictionary<SfxCue, int> _ultimoVariacao = new Dictionary<SfxCue, int>();

        private Camera _camera;
        private float _ultimoInterface = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
            Clipes.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Inicializar()
        {
            if (_instancia != null) return;

            var hospedeiro = new GameObject("SfxPlayer");
            DontDestroyOnLoad(hospedeiro);
            _instancia = hospedeiro.AddComponent<SfxPlayer>();
        }

        private void Awake()
        {
            for (int i = 0; i < QuantidadeVoz; i++)
            {
                var voz = gameObject.AddComponent<AudioSource>();
                voz.playOnAwake = false;
                voz.loop = false;
                voz.spatialBlend = 0f;
                voz.priority = 64;
                _vozes[i] = voz;
                _iniciadoEm[i] = -100f;
            }

            SceneManager.sceneLoaded += AoCarregarCena;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            if (_instancia == this) _instancia = null;
        }

        private static void AoCarregarCena(Scene cenaAtual, LoadSceneMode modo)
        {
            foreach (ScrapController sucata in FindObjectsByType<ScrapController>())
            {
                if (sucata.GetComponent<ScrapSounds>() == null) sucata.gameObject.AddComponent<ScrapSounds>();
            }
        }

        /// <summary>Se tocou um som de interface ou de coleta ha menos de <paramref name="segundos"/>.</summary>
        public static bool InterfaceSoouHa(float segundos)
        {
            return _instancia != null && Time.unscaledTime - _instancia._ultimoInterface < segundos;
        }

        /// <summary>Carrega (uma vez so) um arquivo de Resources/Sfx.</summary>
        public static AudioClip Carregar(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return null;
            if (Clipes.TryGetValue(nome, out AudioClip guardado)) return guardado;

            AudioClip clipe = Resources.Load<AudioClip>("Sfx/" + nome);
            if (clipe == null) Debug.LogWarning("[Som] Efeito nao encontrado: Resources/Sfx/" + nome);
            Clipes[nome] = clipe;
            return clipe;
        }

        public static void Tocar(SfxCue deixa, Vector2? at, float volume, float tom2, float atraso)
        {
            if (_instancia != null && deixa != null) _instancia.TocarEfeito(deixa, at, volume, tom2, atraso);
        }

        private void TocarEfeito(SfxCue deixa, Vector2? at, float volume, float tom2, float atraso)
        {
            float agora = Time.unscaledTime;
            if (_ultimoTocado.TryGetValue(deixa, out float ultimo) && agora - ultimo < deixa.Recarga) return;

            float ganho = 1f;
            float panorama = 0f;
            if (at.HasValue) ganho = Distancia(at.Value, out panorama);

            if (ganho <= 0.01f) return;

            AudioClip clipe = Carregar(EscolherClipe(deixa));
            if (clipe == null) return;

            _ultimoTocado[deixa] = agora;
            if (deixa.Interface) _ultimoInterface = agora;

            int indice = VozLivre();
            AudioSource voz = _vozes[indice];
            voz.Stop();
            voz.clip = clipe;
            voz.volume = Mathf.Clamp01(Sfx.Volume * deixa.Volume * volume * ganho * GameAudio.NivelEfeitos);
            voz.pitch = deixa.Tom * tom2 * (1f + Random.Range(-deixa.VariacaoTom, deixa.VariacaoTom));
            voz.panStereo = panorama;
            _iniciadoEm[indice] = agora + atraso;

            if (atraso > 0f) voz.PlayDelayed(atraso);
            else voz.Play();
        }

        /// <summary>
        /// Quanto um som vindo de <paramref name="at"/> se ouve agora (0 a 1,
        /// pela distancia da camera) e de que lado ele vem.
        /// </summary>
        public static float Distancia(Vector2 at, out float panorama)
        {
            panorama = 0f;

            Camera camera = _instancia != null ? _instancia._camera : null;
            if (camera == null || !camera.isActiveAndEnabled)
            {
                camera = Camera.main;
                if (_instancia != null) _instancia._camera = camera;
            }

            if (camera == null) return 1f;

            Vector2 deslocamento = at - (Vector2)camera.transform.position;
            float perto = 1f - Mathf.Clamp01((deslocamento.magnitude - Perto) / (Longe - Perto));
            panorama = Mathf.Clamp(deslocamento.x / 12f, -1f, 1f) * 0.5f;
            return perto * perto;
        }

        /// <summary>O arquivo desta vez: com variacoes, uma qualquer menos a ultima.</summary>
        private string EscolherClipe(SfxCue deixa)
        {
            if (deixa.Variacoes <= 1) return deixa.Clipe;

            _ultimoVariacao.TryGetValue(deixa, out int valorAnterior);
            int picareta = Random.Range(1, deixa.Variacoes + 1);
            if (picareta == valorAnterior) picareta = picareta % deixa.Variacoes + 1;

            _ultimoVariacao[deixa] = picareta;
            return deixa.Clipe + "_" + picareta;
        }

        /// <summary>Uma voz parada; se nao houver, a que comecou ha mais tempo.</summary>
        private int VozLivre()
        {
            int maisAntigo = 0;
            for (int i = 0; i < QuantidadeVoz; i++)
            {
                if (!_vozes[i].isPlaying && Time.unscaledTime >= _iniciadoEm[i]) return i;
                if (_iniciadoEm[i] < _iniciadoEm[maisAntigo]) maisAntigo = i;
            }

            return maisAntigo;
        }
    }
}
