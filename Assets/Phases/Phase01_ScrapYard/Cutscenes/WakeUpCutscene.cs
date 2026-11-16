using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A abertura do jogo: o Scrap, rejeitado pela Coroa, despencando pela
    /// Calha de Descarte N.º 1 ate o fundo do Poco de Sucata.
    ///
    /// A tela comeca preta e abre na boca da calha, que treme e chacoalha com
    /// algo descendo por dentro. Ele sai rolando, apagado, cai no monte de
    /// sucata num estrondo (poeira, parafusos e chapas voando, a tela
    /// tremendo), quica e fica largado de costas. A etiqueta de remessa vem
    /// atras, planando, e pousa do lado. Os olhos piscam ate pegar, ele se
    /// apoia no cinzel, fica de joelhos, de pe — e o controle e do jogador.
    /// Sem nenhuma frase na tela. E/Espaco/Enter pulam a cena.
    ///
    /// So roda no comeco de um jogo novo: nunca no Continuar (o save guarda
    /// _played) nem chegando de outra cena por uma SceneExit.
    ///
    /// Os quadros (IntroFallArt) e os pontos da calha vem do gerador da fase.
    /// Sem eles (cena antiga), roda o despertar simples de antes.
    /// </summary>
    // Antes do SceneArrival, que consome o recado de quem veio de outra cena.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(ScrapController))]
    public class WakeUpCutscene : MonoBehaviour
    {
        // A ordem dos quadros na folha. E a mesma do IntroFallArt, no editor.
        private const int Tumble = 0;
        private const int TumbleFrames = 8;
        private const int Slam = 8;
        private const int Bounce = 9;
        private const int LieOff = 10;
        private const int LieDim = 11;
        private const int LieOn = 12;
        private const int LiftHead = 13;
        private const int Prop = 14;
        private const int Kneel = 15;
        private const int Crouch = 16;
        private const int FrameCount = 17;

        private const int Dust = 0;
        private const int DustFrames = 6;
        private const int Debris = 6;
        private const int DebrisFrames = 6;
        private const int Spark = 12;
        private const int FxCount = 14;

        [SerializeField] private float darkTime = 0.6f;
        [SerializeField] private float fadeTime = 1.6f;
        [SerializeField] private float flickerTime = 1.4f;

        [Tooltip("A cor dele apagado, no despertar simples (sem os quadros da queda).")]
        [SerializeField] private Color offColor = new Color(0.35f, 0.38f, 0.40f);

        [Header("A queda pela calha")]
        [Tooltip("Os quadros do Scrap caindo e se levantando (IntroFallArt).")]
        [SerializeField] private Sprite[] fallFrames;

        [Tooltip("Poeira, pecas de sucata e faisca (IntroFallArt.Fx).")]
        [SerializeField] private Sprite[] fxFrames;

        [Tooltip("A boca da Calha N.º 1, em unidades de mundo.")]
        [SerializeField] private Vector2 chuteMouth = new Vector2(21.5f, 25.6f);

        [Tooltip("Onde ele bate no monte (x). A coluna da calha ate ali nao passa por plataforma.")]
        [SerializeField] private float impactX = 18.2f;

        [Tooltip("Onde ele para depois do quique (x), e onde se levanta.")]
        [SerializeField] private float restX = 20.6f;

        [Tooltip("O nome do legivel da etiqueta de remessa, que cai junto.")]
        [SerializeField] private string tagName = "Lore_TornTag";

        private const float Gravity = 38f;

        private static bool _played;

        private ScrapController _controller;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;
        private ScrapAnimator _animator;
        private bool _skip;

        private readonly List<GameObject> _fx = new List<GameObject>();
        private Transform _tagArt;
        private Vector3 _tagHome;
        private Quaternion _tagHomeRotation;
        private Renderer[] _tagRenderers;
        private Collider2D _tagArea;
        private bool _tagLanded = true;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _played = false;
        }

        /// <summary>Usado pelo gerador da fase 1.</summary>
        public void ConfigureFall(Sprite[] frames, Sprite[] fx, Vector2 mouth, float impact, float rest)
        {
            fallFrames = frames;
            fxFrames = fx;
            chuteMouth = mouth;
            impactX = impact;
            restX = rest;
        }

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<ScrapAnimator>();
        }

        private void Start()
        {
            bool arriving = !string.IsNullOrEmpty(SceneTravel.PendingSpawnId);

            // Continuar: o save ja traz _played, mas a bancada carregada tambem diz.
            if (_played || arriving || SaveSystem.LoadedBench.HasValue) return;

            _played = true;

            // Ja comeca no preto, antes do primeiro quadro aparecer.
            Cinematic.Begin();
            Cinematic.Fade = 1f;

            bool hasArt = fallFrames != null && fallFrames.Length >= FrameCount
                && fxFrames != null && fxFrames.Length >= FxCount;
            StartCoroutine(hasArt ? PlayFall() : PlayWake());
        }

        // ------------------------------------------------------------------
        // A queda
        // ------------------------------------------------------------------

        private IEnumerator PlayFall()
        {
            yield return WaitForArrival();

            Vector3 spawn = transform.position;
            float floor = GroundAt(restX, spawn.y);
            float impactFloor = GroundAt(impactX, spawn.y);

            // Some com ele ate sair da calha; a etiqueta tambem espera a vez.
            _controller.ControlLocked = true;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            if (_animator != null) _animator.enabled = false;
            _sprite.enabled = false;
            _sprite.flipX = false;
            HideTag();

            Vector2 look = chuteMouth + new Vector2(-1f, -3f);
            ScrapCameraFollow.Direct(look, 0.85f, 0.02f);

            yield return Wait(darkTime);

            // Abre na calha. Algo desce chacoalhando la dentro, cada vez mais perto.
            ScrapCameraFollow.Direct(look, 0.85f, 1.2f);
            Sfx.IntroChute.Play(chuteMouth);
            const float rattle = 2.1f;
            float nextKnock = 0.5f;
            for (float t = 0f; t < rattle && !_skip; t += Time.deltaTime)
            {
                Cinematic.Fade = 1f - Mathf.Clamp01(t / fadeTime);

                if (t >= nextKnock)
                {
                    float k = t / rattle;
                    ScrapCameraFollow.Shake(0.02f + 0.05f * k, 0.18f);
                    SpawnPuff(chuteMouth + new Vector2(Random.Range(-1.2f, 1.2f), -0.2f), new Vector2(0f, -1.2f), 0.7f, 0.5f);
                    nextKnock += Mathf.Lerp(0.45f, 0.14f, k);
                }

                Hold();
                yield return null;
            }

            Cinematic.Fade = 0f;

            // Sai rolando pela boca, puxado pro canto do funil amassado.
            if (!_skip)
            {
                _sprite.enabled = true;
                SpawnPuff(chuteMouth + new Vector2(0f, -0.3f), new Vector2(0f, -2f), 1.1f, 0.7f);
                SpawnPuff(chuteMouth + new Vector2(-0.8f, -0.1f), new Vector2(-1.5f, -1f), 0.9f, 0.6f);
                ScrapCameraFollow.Shake(0.1f, 0.25f);
                StartCoroutine(TagFall(chuteMouth, floor));
            }

            // Sai pelo lado esquerdo do funil e escorrega pra coluna livre de plataformas.
            float exitX = chuteMouth.x - 0.9f;
            float drift = exitX - impactX;
            float vy0 = -2.5f;
            float landY = impactFloor + 0.5f;
            Vector2 center = chuteMouth;
            for (float t = 0f; !_skip; t += Time.deltaTime)
            {
                center.x = exitX - drift * (1f - Mathf.Exp(-7f * t));
                center.y = chuteMouth.y - 0.9f + vy0 * t - 0.5f * Gravity * t * t;

                if (center.y <= landY) break;

                ShowFrame(Tumble + (int)(t * 18f) % TumbleFrames);
                transform.position = new Vector3(center.x, center.y - 1f, spawn.z);
                ScrapCameraFollow.Direct(new Vector2(center.x + 0.6f, center.y - 0.6f), 0.85f, 0.1f);

                Hold();
                yield return null;
            }

            // O tombo no monte.
            if (!_skip)
            {
                var impact = new Vector3(impactX, impactFloor, spawn.z);
                transform.position = impact;
                ShowFrame(Slam);

                ScrapCameraFollow.Shake(0.4f, 0.6f);
                Cinematic.Flash = 0.18f;
                Sfx.IntroCrash.Play(impact);
                NoiseSystem.Emit(impact, 10f, 0.8f);
                Burst(impact, impactFloor);
                ScrapCameraFollow.Direct(new Vector2(impactX + 0.8f, impactFloor + 1.6f), 0.8f, 0.25f);

                yield return Wait(0.14f);
                Cinematic.Flash = 0f;
            }

            // Quica e escorrega pro lado, ate parar de costas.
            const float hop = 0.42f;
            for (float t = 0f; t < hop && !_skip; t += Time.deltaTime)
            {
                float k = t / hop;
                float x = Mathf.Lerp(impactX, restX, k);
                float y = Mathf.Lerp(impactFloor, floor, k) + 0.9f * 4f * k * (1f - k);
                ShowFrame(Bounce);
                transform.position = new Vector3(x, y - 0.3f * (1f - 4f * k * (1f - k)), spawn.z);
                Hold();
                yield return null;
            }

            var rest = new Vector3(restX, floor, spawn.z);
            transform.position = rest;
            ShowFrame(LieOff);

            if (!_skip)
            {
                Sfx.ChunkLand.Play(rest, pitch: 0.85f);
                ScrapCameraFollow.Shake(0.1f, 0.2f);
                SpawnPuff(rest + new Vector3(-0.6f, 0.3f), new Vector2(-0.8f, 0.4f), 0.8f, 0.6f);
                SpawnPuff(rest + new Vector3(0.7f, 0.3f), new Vector2(0.8f, 0.4f), 0.8f, 0.6f);
            }

            // Largado. A camera chega perto devagar; a etiqueta acaba de pousar.
            ScrapCameraFollow.Direct(new Vector2(restX + 0.5f, floor + 1.3f), 0.62f, 1.1f);
            yield return Wait(1.7f);

            // Os olhos pegando: brasa, apaga, brasa, acende, apaga, acende de vez.
            int[] eyes = { LieDim, LieOff, LieDim, LieOn, LieOff, LieOn };
            float[] holds = { 0.18f, 0.32f, 0.14f, 0.12f, 0.2f, 0.5f };
            for (int i = 0; i < eyes.Length && !_skip; i++)
            {
                ShowFrame(eyes[i]);
                if (eyes[i] != LieOff) Sfx.EyeFlicker.Play(rest, pitch: 0.85f + 0.06f * i);
                if (i == eyes.Length - 1)
                {
                    NoiseSystem.Emit(rest, 6f, 0.3f);
                    Sfx.ScrapBoot.Play(rest);
                }

                yield return Wait(Mathf.Max(holds[i], flickerTime / eyes.Length * 0.5f));
            }

            // Levanta: ergue a cabeca, finca o cinzel, joelhos, agacha, de pe.
            (int frame, float hold)[] rise = { (LiftHead, 0.4f), (Prop, 0.45f), (Kneel, 0.4f), (Crouch, 0.22f) };
            foreach ((int frame, float hold) in rise)
            {
                if (_skip) break;
                ShowFrame(frame);
                if (frame == Prop) Sfx.ChunkLand.Play(rest, volume: 0.5f, pitch: 1.4f);
                if (frame == Kneel) Sfx.Step.Play(rest);
                if (frame == Crouch)
                {
                    Sfx.Land.Play(rest);
                    SpawnPuff(rest + new Vector3(0f, 0.2f), new Vector2(0f, 0.3f), 0.6f, 0.45f);
                }

                yield return Wait(hold);
            }

            Finish(rest);
        }

        /// <summary>Devolve tudo ao jogo: o Scrap de pe, a fisica, a camera, a etiqueta.</summary>
        private void Finish(Vector3 standing)
        {
            if (_skip) Cinematic.Fade = 0f;

            transform.rotation = Quaternion.identity;
            transform.position = standing;
            _sprite.enabled = true;
            _sprite.color = Color.white;
            if (_animator != null) _animator.enabled = true;

            _body.position = standing;
            _body.simulated = true;
            _body.linearVelocity = Vector2.zero;

            var respawn = GetComponent<ScrapRespawn>();
            if (respawn != null) respawn.SetSafePosition(standing);

            if (_skip)
            {
                foreach (GameObject piece in _fx) if (piece != null) Destroy(piece);
                _fx.Clear();
            }

            LandTag();

            if (!_skip) ScrapCameraFollow.Shake(0.05f, 0.15f);
            ScrapCameraFollow.Release();

            _controller.ControlLocked = false;
            Cinematic.End();
        }

        private void ShowFrame(int frame)
        {
            Sprite sprite = fallFrames[frame];
            if (sprite != null) _sprite.sprite = sprite;
        }

        /// <summary>A altura do chao em x, olhando de logo acima do ponto de chegada pra baixo (por baixo das plataformas finas).</summary>
        private static float GroundAt(float x, float spawnY)
        {
            int ground = LayerMask.GetMask("Ground");
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, spawnY + 1.2f), Vector2.down, 6f, ground == 0 ? ~0 : ground);
            return hit.collider != null ? hit.point.y : spawnY;
        }

        // ------------------------------------------------------------------
        // Poeira e sucata
        // ------------------------------------------------------------------

        /// <summary>O estrondo: nuvens pros lados, pecas de sucata voando, faiscas.</summary>
        private void Burst(Vector3 at, float floor)
        {
            for (int i = 0; i < 6; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float spread = 0.4f + 0.35f * (i / 2);
                SpawnPuff(at + new Vector3(side * spread * 0.6f, 0.25f + 0.1f * i), new Vector2(side * (1.2f + i * 0.4f), 0.5f + 0.15f * i),
                    0.7f + 0.08f * i, 0.55f + 0.05f * i);
            }

            for (int i = 0; i < 7; i++)
            {
                float angle = Mathf.Lerp(35f, 145f, (i + Random.Range(-0.3f, 0.3f)) / 6f) * Mathf.Deg2Rad;
                float speed = Random.Range(5.5f, 9f);
                var velocity = new Vector2(Mathf.Cos(angle) * speed * 0.7f, Mathf.Sin(angle) * speed);
                StartCoroutine(Fly(Debris + i % DebrisFrames, at + new Vector3(0f, 0.4f), velocity, floor));
            }

            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(Flare(at + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(0.1f, 0.7f)), 0.05f * i));
            }
        }

        private SpriteRenderer MakeFx(string name, int frame, Vector3 at, int orderOffset)
        {
            var piece = new GameObject(name);
            piece.transform.position = at;
            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = fxFrames[frame];
            renderer.sortingLayerID = _sprite.sortingLayerID;
            renderer.sortingOrder = _sprite.sortingOrder + orderOffset;
            _fx.Add(piece);
            return renderer;
        }

        private void SpawnPuff(Vector2 at, Vector2 velocity, float life, float scale)
        {
            StartCoroutine(Puff(at, velocity, life, scale));
        }

        /// <summary>Uma nuvem que incha, desliza, freia e some.</summary>
        private IEnumerator Puff(Vector2 at, Vector2 velocity, float life, float scale)
        {
            SpriteRenderer renderer = MakeFx("IntroDust", Dust, at, 2);
            Transform piece = renderer.transform;
            piece.localScale = Vector3.one * scale;
            Vector3 position = at;

            for (float t = 0f; t < life && renderer != null; t += Time.deltaTime)
            {
                float k = t / life;
                renderer.sprite = fxFrames[Dust + Mathf.Min(DustFrames - 1, (int)(k * DustFrames))];
                renderer.color = new Color(1f, 1f, 1f, 1f - k * k);
                velocity *= Mathf.Exp(-3f * Time.deltaTime);
                position += (Vector3)(velocity * Time.deltaTime);
                piece.position = position;
                yield return null;
            }

            if (renderer != null)
            {
                _fx.Remove(renderer.gameObject);
                Destroy(renderer.gameObject);
            }
        }

        /// <summary>Uma peca de sucata: sobe girando, cai, quica uma vez com um tinido, assenta e some.</summary>
        private IEnumerator Fly(int frame, Vector3 at, Vector2 velocity, float floor)
        {
            SpriteRenderer renderer = MakeFx("IntroDebris", frame, at, 1);
            Transform piece = renderer.transform;
            float spin = Random.Range(-720f, 720f);
            Vector3 position = at;
            int bounces = 0;

            for (float t = 0f; t < 4f && renderer != null; t += Time.deltaTime)
            {
                velocity.y -= Gravity * 0.8f * Time.deltaTime;
                position += (Vector3)(velocity * Time.deltaTime);

                float ground = floor + 0.15f;
                if (position.y < ground && velocity.y < 0f)
                {
                    position.y = ground;
                    if (bounces++ < 1 && velocity.y < -3f)
                    {
                        velocity = new Vector2(velocity.x * 0.5f, -velocity.y * 0.35f);
                        spin *= 0.5f;
                        Sfx.ChunkLand.Play(position, volume: 0.35f, pitch: Random.Range(1.4f, 1.9f));
                    }
                    else
                    {
                        velocity = Vector2.zero;
                        spin = 0f;
                    }
                }

                piece.position = position;
                piece.Rotate(0f, 0f, spin * Time.deltaTime);
                if (t > 3f) renderer.color = new Color(1f, 1f, 1f, 4f - t);
                yield return null;
            }

            if (renderer != null)
            {
                _fx.Remove(renderer.gameObject);
                Destroy(renderer.gameObject);
            }
        }

        private IEnumerator Flare(Vector3 at, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpriteRenderer renderer = MakeFx("IntroSpark", Spark, at, 3);
            yield return new WaitForSeconds(0.05f);
            if (renderer != null) renderer.sprite = fxFrames[Spark + 1];
            yield return new WaitForSeconds(0.06f);
            if (renderer != null)
            {
                _fx.Remove(renderer.gameObject);
                Destroy(renderer.gameObject);
            }
        }

        // ------------------------------------------------------------------
        // A etiqueta de remessa
        // ------------------------------------------------------------------

        private void HideTag()
        {
            GameObject tag = GameObject.Find(tagName);
            if (tag == null) return;

            Transform art = tag.transform.Find("Art");
            _tagArt = art != null ? art : tag.transform;
            _tagHome = _tagArt.position;
            _tagHomeRotation = _tagArt.rotation;
            _tagRenderers = _tagArt.GetComponentsInChildren<Renderer>();
            _tagArea = tag.GetComponent<Collider2D>();

            foreach (Renderer renderer in _tagRenderers) renderer.enabled = false;
            if (_tagArea != null) _tagArea.enabled = false;
            _tagLanded = false;
        }

        /// <summary>Plana da boca da calha ate o lugar dela, balancando como papel.</summary>
        private IEnumerator TagFall(Vector2 from, float floor)
        {
            if (_tagArt == null) yield break;

            yield return new WaitForSeconds(0.3f);
            if (_tagLanded) yield break;

            foreach (Renderer renderer in _tagRenderers) renderer.enabled = true;

            const float fall = 3.4f;
            Vector3 start = new Vector3(from.x, from.y - 0.4f, _tagHome.z);
            for (float t = 0f; t < fall && !_tagLanded; t += Time.deltaTime)
            {
                float k = t / fall;
                float eased = 1f - (1f - k) * (1f - k * 0.4f);
                float sway = Mathf.Sin(t * 4.2f) * 0.9f * (1f - k);
                Vector3 position = Vector3.Lerp(start, _tagHome, eased);
                position.x += sway;
                _tagArt.position = position;
                _tagArt.rotation = _tagHomeRotation * Quaternion.Euler(0f, 0f, Mathf.Cos(t * 4.2f) * 32f * (1f - k));
                yield return null;
            }

            LandTag();
        }

        private void LandTag()
        {
            if (_tagLanded || _tagArt == null) return;
            _tagLanded = true;

            _tagArt.position = _tagHome;
            _tagArt.rotation = _tagHomeRotation;
            foreach (Renderer renderer in _tagRenderers) if (renderer != null) renderer.enabled = true;
            if (_tagArea != null) _tagArea.enabled = true;
        }

        // ------------------------------------------------------------------
        // O despertar simples (cena gerada antes dos quadros da queda)
        // ------------------------------------------------------------------

        private IEnumerator PlayWake()
        {
            yield return WaitForArrival();

            Vector3 standing = transform.position;
            Quaternion upright = transform.rotation;

            // Caido de lado: sem fisica, deitado e erguido o bastante pra nao
            // afundar no monte (o pivo e nos pes).
            _controller.ControlLocked = true;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            transform.position = standing + Vector3.up * Mathf.Max(0f, standing.y - _sprite.bounds.min.y);
            _sprite.color = offColor;

            yield return Wait(darkTime);

            for (float t = 0f; t < fadeTime && !_skip; t += Time.deltaTime)
            {
                Cinematic.Fade = 1f - t / fadeTime;
                Hold();
                yield return null;
            }

            Cinematic.Fade = 0f;

            // Os olhos pegando: acende, apaga, acende de vez.
            float[] blinks = { 0.15f, 0.25f, 0.12f, 0.15f, 0.08f, 0.4f };
            float step = flickerTime / blinks.Length;
            for (int i = 0; i < blinks.Length && !_skip; i++)
            {
                _sprite.color = i % 2 == 0 ? Color.white : offColor;
                if (i % 2 == 0) Sfx.EyeFlicker.Play(transform.position, pitch: 0.9f + 0.05f * i);
                if (i == blinks.Length - 1)
                {
                    NoiseSystem.Emit(transform.position, 6f, 0.3f);
                    Sfx.ScrapBoot.Play(transform.position);
                }
                yield return Wait(Mathf.Max(blinks[i], step * 0.5f));
            }

            // Levanta: de volta em pe, com um tranco pra cima.
            const float riseTime = 0.25f;
            Vector3 lying = transform.position;
            Quaternion side = transform.rotation;
            for (float t = 0f; t < riseTime && !_skip; t += Time.deltaTime)
            {
                float k = t / riseTime;
                transform.rotation = Quaternion.Slerp(side, upright, k);
                transform.position = Vector3.Lerp(lying, standing, k);
                Hold();
                yield return null;
            }

            transform.rotation = upright;
            transform.position = standing;
            _sprite.color = Color.white;
            _body.position = standing;
            _body.simulated = true;
            _body.linearVelocity = _skip ? Vector2.zero : new Vector2(0f, 6f);
            if (!_skip) Sfx.Jump.Play();

            ScrapCameraFollow.Shake(0.08f, 0.25f);

            // Deixa ele cair de volta no monte antes de soltar o controle.
            yield return null;
            for (float t = 0f; t < 1f && !_controller.IsGrounded; t += Time.deltaTime)
            {
                _controller.ControlLocked = true;
                yield return null;
            }

            _controller.ControlLocked = false;
            Cinematic.End();
        }

        // ------------------------------------------------------------------
        // Comum
        // ------------------------------------------------------------------

        /// <summary>Espera o chao existir (o SceneArrival segura o Scrap ate la) e ele assentar.</summary>
        private IEnumerator WaitForArrival()
        {
            for (float t = 0f; t < 3f && !(_body.simulated && _controller.IsGrounded); t += Time.deltaTime)
            {
                _controller.ControlLocked = true;
                yield return null;
            }
        }

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds && !_skip; t += Time.deltaTime)
            {
                Hold();
                yield return null;
            }
        }

        /// <summary>Mantem o controle travado e escuta o botao de pular a cena.</summary>
        private void Hold()
        {
            _controller.ControlLocked = true;

            // Meio segundo de tolerancia: o botao que comecou o jogo nao pula a cena.
            if (Time.time - Cinematic.StartTime > 0.5f && SkipPressed()) _skip = true;
        }

        private static bool SkipPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame
                || keyboard.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad pad = Gamepad.current;
            return pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame);
        }
    }
}
