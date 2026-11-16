using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A chegada pelo elevador: o outro lado do ElevatorRide. Quando o Scrap
    /// chega na cena pelo SpawnPoint combinado (ou quando se da Play direto
    /// nela, sem save), a cabine comeca no fundo do poco e sobe com ele dentro
    /// ate parar no andar. So entao o controle volta.
    ///
    /// Fica na propria cabine. A posicao da cabine na cena e a do andar (onde
    /// ela para); o fundo do poco e startY. A cabine tem o proprio chao (um
    /// colisor na camada do terreno), entao ela tampa o poco quando para.
    /// </summary>
    public class ElevatorArrival : MonoBehaviour
    {
        [Tooltip("O SpawnPoint de quem chega por este elevador.")]
        [SerializeField] private string spawnId;

        [Tooltip("Altura (y de mundo) do fundo do poco, de onde a cabine parte.")]
        [SerializeField] private float startY;

        [Tooltip("Tiles por segundo, na velocidade de cruzeiro.")]
        [SerializeField] private float speed = 5f;

        [Tooltip("Se a chegada toca tambem quando se da Play direto na cena (sem vir de outra).")]
        [SerializeField] private bool playWithoutTravel = true;

        private bool _play;
        private float _stopY;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newSpawnId, float newStartY, float newSpeed)
        {
            spawnId = newSpawnId;
            startY = newStartY;
            speed = newSpeed;
        }

        private void Awake()
        {
            // Lido antes do Start da SceneArrival, que consome o recado.
            string pending = SceneTravel.PendingSpawnId;

            if (SaveSystem.HasPendingBench) _play = false;
            else if (!string.IsNullOrEmpty(pending)) _play = pending == spawnId;
            else _play = playWithoutTravel;

            // A cabine fica no andar ate a SceneArrival achar chao embaixo do
            // Scrap (o chao dele e o da cabine); so depois desce pro fundo. A
            // tela ainda esta fechada pelo loading nesses quadros.
            _stopY = transform.position.y;
        }

        private void Start()
        {
            if (_play) StartCoroutine(Arrive());
        }

        private void SetY(float y)
        {
            Vector3 position = transform.position;
            position.y = y;
            transform.position = position;
        }

        private IEnumerator Arrive()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller == null)
            {
                SetY(_stopY);
                yield break;
            }

            Transform player = controller.transform;
            var body = player.GetComponent<Rigidbody2D>();

            // A SceneArrival poe o Scrap no SpawnPoint e espera o chao. Deixa
            // ela terminar: a posicao em que ela o larga e a de chegada.
            float waited = 0f;
            while (body != null && !body.simulated && waited < 6f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return null;

            Cinematic.Begin();
            controller.ControlLocked = true;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            // O Scrap vai no meio da cabine, na mesma altura relativa de quando ela para.
            Vector3 stop = new Vector3(transform.position.x, _stopY, 0f);
            Vector3 riderOffset = new Vector3(transform.position.x + 1.5f, player.position.y, 0f) - stop;

            SetY(startY);
            player.position = transform.position + riderOffset;
            if (body != null) body.position = player.position;

            Camera main = Camera.main;
            var follow = main != null ? main.GetComponent<ScrapCameraFollow>() : null;
            if (follow != null) follow.SnapToTarget();

            // O motor ja vem andando: a cabine chega embalada e freia no andar.
            var motor = gameObject.AddComponent<AudioSource>();
            motor.clip = SfxPlayer.Load(Sfx.ElevatorLoop.Clip);
            motor.loop = true;
            motor.spatialBlend = 0f;
            motor.volume = 0f;
            if (motor.clip != null) motor.Play();

            float braking = speed * 0.9f;
            float current = speed;

            while (transform.position.y < _stopY)
            {
                float left = _stopY - transform.position.y;

                // Freia nos ultimos tiles: v = raiz(2 a d), sem passar da de cruzeiro.
                float target = Mathf.Min(speed, Mathf.Sqrt(2f * braking * left) + 0.4f);
                current = Mathf.MoveTowards(current, target, braking * 2f * Time.deltaTime);

                SetY(Mathf.Min(_stopY, transform.position.y + current * Time.deltaTime));
                player.position = transform.position + riderOffset;

                float pace = current / Mathf.Max(0.1f, speed);
                motor.pitch = 0.8f + 0.25f * pace;
                motor.volume = Sfx.Volume * Sfx.ElevatorLoop.Volume * Mathf.Lerp(0.35f, 1f, pace);

                yield return null;
            }

            SetY(_stopY);
            player.position = transform.position + riderOffset;
            Destroy(motor);

            // O tranco da parada.
            ScrapCameraFollow.Shake(0.1f, 0.3f);
            Sfx.ElevatorStart.Play(transform.position, pitch: 0.8f);
            yield return new WaitForSeconds(0.45f);

            if (body != null)
            {
                body.position = player.position;
                body.linearVelocity = Vector2.zero;
                body.simulated = true;
            }

            var respawn = player.GetComponent<ScrapRespawn>();
            if (respawn != null) respawn.SetSafePosition(player.position);

            controller.ControlLocked = false;
            Cinematic.End();
        }
    }
}
