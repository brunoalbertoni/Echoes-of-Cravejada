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
        [UnityEngine.Serialization.FormerlySerializedAs("spawnId")] [Tooltip("O SpawnPoint de quem chega por este elevador.")]
        [SerializeField] private string idNascimento;

        [UnityEngine.Serialization.FormerlySerializedAs("startY")] [Tooltip("Altura (y de mundo) do fundo do poco, de onde a cabine parte.")]
        [SerializeField] private float inicioY;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [Tooltip("Tiles por segundo, na velocidade de cruzeiro.")]
        [SerializeField] private float velocidade = 5f;

        [UnityEngine.Serialization.FormerlySerializedAs("playWithoutTravel")] [Tooltip("Se a chegada toca tambem quando se da Play direto na cena (sem vir de outra).")]
        [SerializeField] private bool tocarSemViagem = true;

        private bool _tocar;
        private float _paradaY;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoIdNascimento, float novoInicioY, float novoVelocidade)
        {
            idNascimento = novoIdNascimento;
            inicioY = novoInicioY;
            velocidade = novoVelocidade;
        }

        private void Awake()
        {
            // Lido antes do Start da SceneArrival, que consome o recado.
            string pendente = SceneTravel.IdNascimentoPendente;

            if (SaveSystem.TemBancadaPendente) _tocar = false;
            else if (!string.IsNullOrEmpty(pendente)) _tocar = pendente == idNascimento;
            else _tocar = tocarSemViagem;

            // A cabine fica no andar ate a SceneArrival achar chao embaixo do
            // Scrap (o chao dele e o da cabine); so depois desce pro fundo. A
            // tela ainda esta fechada pelo loading nesses quadros.
            _paradaY = transform.position.y;
        }

        private void Start()
        {
            if (_tocar) StartCoroutine(Chegar());
        }

        private void DefinirAltura(float y)
        {
            Vector3 posicao = transform.position;
            posicao.y = y;
            transform.position = posicao;
        }

        private IEnumerator Chegar()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador == null)
            {
                DefinirAltura(_paradaY);
                yield break;
            }

            Transform jogador = controlador.transform;
            var corpo = jogador.GetComponent<Rigidbody2D>();

            // A SceneArrival poe o Scrap no SpawnPoint e espera o chao. Deixa
            // ela terminar: a posicao em que ela o larga e a de chegada.
            float esperou = 0f;
            while (corpo != null && !corpo.simulated && esperou < 6f)
            {
                esperou += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return null;

            Cinematic.Iniciar();
            controlador.ControleTravado = true;
            if (corpo != null)
            {
                corpo.linearVelocity = Vector2.zero;
                corpo.simulated = false;
            }

            // O Scrap vai no meio da cabine, na mesma altura relativa de quando ela para.
            Vector3 parada = new Vector3(transform.position.x, _paradaY, 0f);
            Vector3 deslocamentoPassageiro = new Vector3(transform.position.x + 1.5f, jogador.position.y, 0f) - parada;

            DefinirAltura(inicioY);
            jogador.position = transform.position + deslocamentoPassageiro;
            if (corpo != null) corpo.position = jogador.position;

            Camera principal = Camera.main;
            var seguir = principal != null ? principal.GetComponent<ScrapCameraFollow>() : null;
            if (seguir != null) seguir.IrDiretoAoAlvo();

            // O motor ja vem andando: a cabine chega embalada e freia no andar.
            var motor = gameObject.AddComponent<AudioSource>();
            motor.clip = SfxPlayer.Carregar(Sfx.RepeticaoElevador.Clipe);
            motor.loop = true;
            motor.spatialBlend = 0f;
            motor.volume = 0f;
            if (motor.clip != null) motor.Play();

            float freando = velocidade * 0.9f;
            float atual = velocidade;

            while (transform.position.y < _paradaY)
            {
                float esquerda = _paradaY - transform.position.y;

                // Freia nos ultimos tiles: v = raiz(2 a d), sem passar da de cruzeiro.
                float alvo = Mathf.Min(velocidade, Mathf.Sqrt(2f * freando * esquerda) + 0.4f);
                atual = Mathf.MoveTowards(atual, alvo, freando * 2f * Time.deltaTime);

                DefinirAltura(Mathf.Min(_paradaY, transform.position.y + atual * Time.deltaTime));
                jogador.position = transform.position + deslocamentoPassageiro;

                float ritmo = atual / Mathf.Max(0.1f, velocidade);
                motor.pitch = 0.8f + 0.25f * ritmo;
                motor.volume = Sfx.Volume * Sfx.RepeticaoElevador.Volume * Mathf.Lerp(0.35f, 1f, ritmo);

                yield return null;
            }

            DefinirAltura(_paradaY);
            jogador.position = transform.position + deslocamentoPassageiro;
            Destroy(motor);

            // O tranco da parada.
            ScrapCameraFollow.Tremer(0.1f, 0.3f);
            Sfx.InicioElevador.Tocar(transform.position, tom2: 0.8f);
            yield return new WaitForSeconds(0.45f);

            if (corpo != null)
            {
                corpo.position = jogador.position;
                corpo.linearVelocity = Vector2.zero;
                corpo.simulated = true;
            }

            var renascimento = jogador.GetComponent<ScrapRespawn>();
            if (renascimento != null) renascimento.DefinirPosicaoSegura(jogador.position);

            controlador.ControleTravado = false;
            Cinematic.Encerrar();
        }
    }
}
