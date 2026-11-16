using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A Bancada de Ourives: o checkpoint. Um altar de pedra e latao com a
    /// bigorna, o cadinho e uma gema presa numa engrenagem.
    ///
    /// Apagada, a gema e cinza e respira devagar. Perto dela, E salva: tres
    /// marteladas na bigorna (com fagulhas), a gema esquenta ate o branco, a
    /// vida enche, as gemas brutas sao cravadas e o momento vira uma gravura
    /// numa placa de latao (ver <see cref="SaveEngraving"/>). Dali pra frente
    /// ela fica acesa — a engrenagem gira, o cadinho tem brasa — e e onde o
    /// Scrap renasce. So uma fica acesa por vez.
    ///
    /// Os quadros vem de Resources/Checkpoint (ver CheckpointArt, no editor):
    /// a bancada troca de cara sozinha em todas as cenas.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Workbench : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do CheckpointArt, no editor.
        private const int Dormant = 0;
        private const int DormantFrames = 4;
        private const int Lit = 4;
        private const int LitFrames = 6;
        private const int Forge = 10;
        private const int ForgeFrames = 6;

        /// <summary>Onde fica o meio da gema e a bigorna, em tiles a partir do canto de baixo a esquerda.</summary>
        private static readonly Vector2 GemOffset = new Vector2(1.5f, 1.94f);
        private static readonly Vector2 AnvilOffset = new Vector2(0.72f, 1.08f);

        private static readonly Color DormantLight = new Color(0.55f, 0.68f, 0.95f);
        private static readonly Color LitLight = new Color(0.55f, 0.92f, 1f);

        [Tooltip("Onde ele renasce. Vazio usa a posicao da propria bancada.")]
        [SerializeField] private Transform restPoint;

        /// <summary>A ultima vez em que alguem salvou numa bancada.</summary>
        public static float LastRestTime { get; private set; } = -100f;

        /// <summary>A bancada em que o Scrap esta encostado agora (o HUD pede o E).</summary>
        public static Workbench Nearby { get; private set; }

        /// <summary>A bancada acesa: a ultima em que se salvou nesta cena.</summary>
        public static Workbench Current { get; private set; }

        /// <summary>Se ela esta no meio de salvar (o HUD nao pede o E de novo).</summary>
        public bool Busy => _busy;

        private static Sprite[] _frames;

        private SpriteRenderer _sprite;
        private Light2D _light;
        private ScrapController _controller;
        private ScrapHealth _health;
        private bool _busy;
        private float _forgeStart = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            LastRestTime = -100f;
            Nearby = null;
            Current = null;
            _frames = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Transform newRestPoint)
        {
            restPoint = newRestPoint;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            if (Frames() != null && _sprite != null) _sprite.sprite = _frames[Dormant];

            // A luz da gema: fraca apagada, forte acesa, um estouro ao salvar.
            var lightObject = new GameObject("GemLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = GemOffset;
            _light = lightObject.AddComponent<Light2D>();
            _light.lightType = Light2D.LightType.Point;
            _light.pointLightInnerRadius = 0f;
            _light.pointLightOuterRadius = 2f;
            _light.color = DormantLight;
            _light.intensity = 0.2f;
        }

        private void Start()
        {
            // O jogo foi carregado de um save feito aqui: ela ja abre acesa.
            Vector3 rest = restPoint != null ? restPoint.position : transform.position;
            if (SaveSystem.LoadedBench.HasValue && Vector2.Distance(SaveSystem.LoadedBench.Value, rest) < 0.5f) Current = this;
        }

        private void OnDestroy()
        {
            if (Nearby == this) Nearby = null;
            if (Current == this) Current = null;
        }

        /// <summary>Os quadros, carregados uma vez. Null se nao estiverem la (fica a arte antiga da cena).</summary>
        private static Sprite[] Frames()
        {
            if (_frames != null) return _frames;

            Sprite[] loaded = Resources.LoadAll<Sprite>("Checkpoint");
            if (loaded == null || loaded.Length < Forge + ForgeFrames)
            {
                Debug.LogWarning("[Bancada] Quadros nao encontrados em Resources/Checkpoint.");
                return null;
            }

            System.Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
            _frames = loaded;
            return _frames;
        }

        // ------------------------------------------------------------------
        // Chegar perto e apertar E
        // ------------------------------------------------------------------

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _controller = controller;
            _health = controller.GetComponent<ScrapHealth>();
            Nearby = this;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            if (Nearby == this) Nearby = null;
            _controller = null;
        }

        /// <summary>Se da pra salvar agora: perto, no chao, sem conversa, sem cena, sem cartao aberto.</summary>
        public bool CanSave =>
            !_busy && _controller != null && _controller.IsGrounded && !_controller.ControlLocked
            && !Cinematic.Active && !AbilityCard.Showing && Npc.Talking == null && Npc.Nearby == null;

        private void Update()
        {
            Animate();

            if (Nearby == this && CanSave && SavePressed()) StartCoroutine(Save());
        }

        private static bool SavePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.wasPressedThisFrame;
        }

        // ------------------------------------------------------------------
        // Salvar
        // ------------------------------------------------------------------

        /// <summary>
        /// As tres marteladas, a gema esquentando, e no pico do clarao: salva,
        /// enche a vida, crava as gemas e grava a placa.
        /// </summary>
        private IEnumerator Save()
        {
            _busy = true;
            ScrapController controller = _controller;
            ScrapHealth health = _health;

            if (controller != null)
            {
                controller.ControlLocked = true;
                var body = controller.GetComponent<Rigidbody2D>();
                if (body != null) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }

            // Tres marteladas, cada uma mais aguda: tin, tin, TAN.
            Vector2 anvil = (Vector2)transform.position + AnvilOffset;
            float[] pitch = { 0.94f, 1f, 1.12f };
            for (int i = 0; i < 3; i++)
            {
                Sfx.BenchStrike.Play(anvil, volume: i == 2 ? 1f : 0.8f, pitch: pitch[i]);
                BenchSpark.Burst(anvil, i == 2 ? 10 : 5, new Color(1f, 0.95f, 0.6f), new Color(1f, 0.4f, 0.1f), i == 2 ? 5f : 3.5f);
                if (i == 2) ScrapCameraFollow.Shake(0.06f, 0.15f);
                yield return new WaitForSeconds(0.2f);
            }

            // A gema acende.
            Current = this;
            _forgeStart = Time.time;
            Sfx.BenchIgnite.Play((Vector2)transform.position + GemOffset);
            BenchSpark.Burst((Vector2)transform.position + GemOffset, 14, Color.white, new Color(0.4f, 0.85f, 1f), 6f, 180f);
            yield return new WaitForSeconds(0.2f);

            // O pico: salvo (no jogo e no arquivo).
            Vector3 rest = restPoint != null ? restPoint.position : transform.position;
            if (health != null) health.RestAt(rest);
            LastRestTime = Time.time;
            ScrapGems.SocketAtWorkbench();
            SaveEngraving.Engrave();
            SaveSystem.Save(rest);

            yield return new WaitForSeconds(0.45f);

            if (controller != null) controller.ControlLocked = false;
            _busy = false;
        }

        // ------------------------------------------------------------------
        // Animacao e luz
        // ------------------------------------------------------------------

        private void Animate()
        {
            float forge = (Time.time - _forgeStart) / 0.6f;
            bool lit = Current == this;

            if (_frames != null && _sprite != null)
            {
                int frame;
                if (forge >= 0f && forge < 1f) frame = Forge + Mathf.Min(ForgeFrames - 1, (int)(forge * ForgeFrames));
                else if (lit) frame = Lit + (int)(Time.time * 8f) % LitFrames;
                else frame = Dormant + (int)(Time.time * 2.5f) % DormantFrames;

                _sprite.sprite = _frames[frame];
            }

            if (_light == null) return;

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * (lit ? 3f : 1.5f));
            float intensity = lit ? 0.9f + 0.25f * pulse : 0.18f + 0.08f * pulse;
            float radius = lit ? 3.6f : 2f;

            // O estouro do salvamento: sobe na hora e volta em meio segundo e pouco.
            if (forge >= 0f && forge < 1.6f)
            {
                float burst = Mathf.Sin(Mathf.Clamp01(forge / 1.6f) * Mathf.PI);
                intensity += 2.6f * burst;
                radius += 5f * burst;
            }

            _light.color = Color.Lerp(_light.color, lit ? LitLight : DormantLight, Time.deltaTime * 4f);
            _light.intensity = intensity;
            _light.pointLightOuterRadius = radius;
        }
    }
}
