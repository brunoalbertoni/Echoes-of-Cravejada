using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O jeito discreto de mostrar um item ganho, sem frase e sem parar o
    /// jogo: o desenho do item salta de dentro do Scrap pra cima da cabeca
    /// dele, solta um anel de brilho, fica um instante cintilando
    /// (acompanhando o Scrap, que continua andando) e some subindo.
    ///
    /// O rubi cravado na bancada tem o seu proprio (<see cref="Encaixar"/>): o
    /// rubi aparece em cima, desce pra dentro do peito e o corpo acende em
    /// vermelho, com o estouro de faiscas.
    ///
    /// Itens contados (ferramentas, paginas, reagentes) nao chamam isto.
    /// </summary>
    public class ItemToast : MonoBehaviour
    {
        private const float Acima = 2.1f;
        private const float Tamanho = 0.85f;

        private static Sprite _anel;
        private static Sprite _faisca;
        private static Sprite _rubi;
        private static Sprite _fragmento;

        private Transform _alvo;
        private SpriteRenderer _item;
        private SpriteRenderer _halo;
        private SpriteRenderer _rotulo;
        private SpriteRenderer[] _faiscas;
        private SpriteRenderer[] _corpo;
        private Color[] _coresCorpo;
        private Color _brilho;
        private float _escalaBase;
        private float _idade;
        private float _atraso;
        private bool _encaixe;
        private bool _rajada;

        /// <summary>O item ganho salta em cima do Scrap. glow: a cor do brilho.</summary>
        public static void Mostrar(Sprite sprite, Color brilho)
        {
            Criar(sprite, brilho, false, 0f);
        }

        /// <summary>O mesmo, com "+N" ao lado do desenho (o cofre de fragmentos).</summary>
        public static void Mostrar(Sprite sprite, Color brilho, int quantia)
        {
            var aviso = Criar(sprite, brilho, false, 0f);
            if (aviso != null && quantia > 0) aviso.SomarQuantidade(quantia);
        }

        /// <summary>O icone de fragmento do HUD.</summary>
        public static Sprite SpriteDoFragmento()
        {
            if (_fragmento != null) return _fragmento;
            var textura = Resources.Load<Texture2D>("HudIcons/shard");
            if (textura == null) return null;
            _fragmento = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0.5f, 0.5f), 32f);
            return _fragmento;
        }

        /// <summary>O mesmo, desenhando um icone do HUD (o rubi bruto).</summary>
        public static void MostrarRubi()
        {
            Criar(SpriteDoRubi(), new Color(1f, 0.45f, 0.4f), false, 0f);
        }

        /// <summary>
        /// O rubi sendo cravado: espera a gravura da bancada sair do meio da
        /// tela (e nao entra na foto) e entao desce pro peito do Scrap.
        /// </summary>
        public static void Encaixar(float atraso)
        {
            Criar(SpriteDoRubi(), new Color(1f, 0.3f, 0.28f), true, atraso);
        }

        private static ItemToast Criar(Sprite sprite, Color brilho, bool encaixe, float atraso)
        {
            if (sprite == null) return null;

            ScrapController sucata = ScrapRef.Controlador;
            if (sucata == null) return null;

            var raiz = new GameObject(encaixe ? "RubySocketFx" : "ItemToast");
            var aviso = raiz.AddComponent<ItemToast>();
            aviso.Montar(sucata.transform, sprite, brilho, encaixe, atraso);
            return aviso;
        }

        /// <summary>
        /// Poe o "+N" em pixel a esquerda do desenho. Fica filho do item, entao
        /// salta, cresce e some junto com ele.
        /// </summary>
        private void SomarQuantidade(int quantia)
        {
            Sprite quantidade = SpriteDaQuantidade("+" + quantia);
            var filho = new GameObject("Count");
            filho.transform.SetParent(_item.transform, false);
            _rotulo = filho.AddComponent<SpriteRenderer>();
            _rotulo.sprite = quantidade;
            _rotulo.sortingLayerName = "Actors";
            _rotulo.sortingOrder = 32;

            const float espaco = 2f / 32f;
            float itemW = _item.sprite.rect.width / _item.sprite.pixelsPerUnit;
            float rotuloW = quantidade.rect.width / 32f;
            filho.transform.localPosition = new Vector3(-(itemW + rotuloW) * 0.5f - espaco, 0f, 0f);
            // O conjunto todo fica centrado sobre a cabeca.
            _item.transform.localPosition = new Vector3((rotuloW + espaco) * 0.5f * _escalaBase, 0f, 0f);
        }

        // Algarismos 3x5, uma linha por string, de cima pra baixo.
        private static readonly string[] Glifos =
        {
            "111101101101111", "010110010010111", "111001111100111", "111001111001111", "101101111001001",
            "111100111001111", "111100111101111", "111001001001001", "111101111101111", "111101111001111",
        };
        private const string Mais = "000010111010000";

        /// <summary>O texto em pixel, claro com contorno escuro, no padrao do HUD.</summary>
        private static Sprite SpriteDaQuantidade(string texto)
        {
            int w = texto.Length * 4 - 1 + 2, h = 5 + 2;
            var textura = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var preenchimento = new bool[w, h];
            for (int c = 0; c < texto.Length; c++)
            {
                string g = texto[c] == '+' ? Mais : Glifos[texto[c] - '0'];
                for (int gy = 0; gy < 5; gy++)
                    for (int gx = 0; gx < 3; gx++)
                        if (g[gy * 3 + gx] == '1') preenchimento[1 + c * 4 + gx, h - 2 - gy] = true;
            }

            var tinta = new Color(1f, 0.93f, 0.7f);
            var beirada = new Color(0.12f, 0.08f, 0.06f, 1f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool perto = false;
                    for (int dy = -1; dy <= 1 && !perto; dy++)
                        for (int dx = -1; dx <= 1 && !perto; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            perto = nx >= 0 && ny >= 0 && nx < w && ny < h && preenchimento[nx, ny];
                        }
                    textura.SetPixel(x, y, preenchimento[x, y] ? tinta : perto ? beirada : Color.clear);
                }

            textura.Apply();
            return Sprite.Create(textura, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        }

        private void Montar(Transform alvo, Sprite sprite, Color brilho, bool encaixe, float atraso)
        {
            _alvo = alvo;
            _brilho = brilho;
            _encaixe = encaixe;
            _atraso = atraso;
            transform.position = alvo.position + Vector3.up * Acima;

            _halo = Camada("Halo", Anel(), 30);
            _halo.color = Color.clear;

            _item = Camada("Item", sprite, 32);
            // Cresce em passos inteiros de pixel, pra nao entortar o desenho.
            float pixels = Mathf.Max(1f, Mathf.Max(sprite.rect.width, sprite.rect.height));
            _escalaBase = Mathf.Max(1f, Mathf.Floor(Tamanho * 32f / pixels)) * sprite.pixelsPerUnit / 32f;
            _item.transform.localScale = Vector3.zero;

            _faiscas = new SpriteRenderer[encaixe ? 10 : 4];
            for (int i = 0; i < _faiscas.Length; i++)
            {
                _faiscas[i] = Camada("Spark" + i, Faisca(), 33);
                _faiscas[i].color = Color.clear;
            }

            if (encaixe)
            {
                _corpo = alvo.GetComponentsInChildren<SpriteRenderer>();
                _coresCorpo = new Color[_corpo.Length];
                for (int i = 0; i < _corpo.Length; i++) _coresCorpo[i] = _corpo[i].color;
            }
        }

        private SpriteRenderer Camada(string nome, Sprite sprite, int ordem)
        {
            var filho = new GameObject(nome);
            filho.transform.SetParent(transform, false);
            var renderizador = filho.AddComponent<SpriteRenderer>();
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = ordem;
            return renderizador;
        }

        private void LateUpdate()
        {
            if (_alvo == null)
            {
                Destroy(gameObject);
                return;
            }

            // Com conversa ou loja na tela, espera: o efeito e pra ser visto.
            if (Npc.Conversando != null || Crisol.AbertoLoja != null || ScriptedDialogue.Ativo) return;

            if (_atraso > 0f)
            {
                _atraso -= Time.deltaTime;
                if (_atraso > 0f) return;
            }

            _idade += Time.deltaTime;
            if (_encaixe) PassoDoEncaixe();
            else PassoDoAviso();
        }

        // ------------------------------------------------------------------
        // O item ganho: salta, brilha, cintila e some subindo.
        // ------------------------------------------------------------------

        private void PassoDoAviso()
        {
            const float estalo = 0.32f, sair = 1.55f, sumido = 1.95f;

            float subida = SuavizarSaida(Mathf.Clamp01(_idade / estalo));
            float y = Mathf.Lerp(0.7f, Acima, subida);
            float escalaAtual = _idade < estalo ? VoltarSuave(_idade / estalo) : 1f;

            float esmaecimento = 1f;
            if (_idade > sair)
            {
                float t = Mathf.Clamp01((_idade - sair) / (sumido - sair));
                y += t * 0.5f;
                escalaAtual *= 1f - t * 0.6f;
                esmaecimento = 1f - t;
            }

            // Bobeia meio pixel pra cima e pra baixo, em degrau de pixel.
            if (_idade > estalo && _idade < sair) y += Mathf.Round(Mathf.Sin((_idade - estalo) * 5f) * 1.5f) / 32f;

            transform.position = _alvo.position + Vector3.up * y;
            _item.transform.localScale = Vector3.one * _escalaBase * escalaAtual;
            _item.color = new Color(1f, 1f, 1f, esmaecimento);
            if (_rotulo != null) _rotulo.color = new Color(1f, 1f, 1f, esmaecimento);

            Aureola(_idade - estalo * 0.7f, 0.45f, 0.6f, 1.7f);
            Reluzir(esmaecimento, 0.55f);

            if (_idade >= sumido) Destroy(gameObject);
        }

        // ------------------------------------------------------------------
        // O rubi cravado: aparece, desce pro peito, o corpo acende.
        // ------------------------------------------------------------------

        private void PassoDoEncaixe()
        {
            const float estalo = 0.3f, segurar = 0.75f, afundar = 1.05f, fimClarao = 1.5f, sumido = 1.7f;

            float y;
            float escalaAtual;
            float alfa = 1f;

            if (_idade < estalo)
            {
                y = Mathf.Lerp(Acima + 0.5f, Acima, SuavizarSaida(_idade / estalo));
                escalaAtual = VoltarSuave(_idade / estalo);
            }
            else if (_idade < segurar)
            {
                y = Acima + Mathf.Round(Mathf.Sin((_idade - estalo) * 6f) * 1.5f) / 32f;
                escalaAtual = 1f;
            }
            else if (_idade < afundar)
            {
                float t = (_idade - segurar) / (afundar - segurar);
                y = Mathf.Lerp(Acima, 0.75f, t * t);
                escalaAtual = 1f - t * 0.7f;
            }
            else
            {
                y = 0.75f;
                escalaAtual = 0f;
                alfa = 0f;
            }

            transform.position = _alvo.position + Vector3.up * y;
            _item.transform.localScale = Vector3.one * _escalaBase * escalaAtual;
            _item.color = new Color(1f, 1f, 1f, alfa);

            if (_idade >= afundar && !_rajada)
            {
                _rajada = true;
                Sfx.Gema.Tocar();
                ScrapCameraFollow.Tremer(0.05f, 0.12f);
            }

            // O estouro: o anel grande, as faiscas pra fora e o corpo em vermelho.
            Aureola(_idade - afundar, 0.5f, 0.4f, 3.2f);
            if (_idade >= afundar) Estourar(_idade - afundar, 0.6f);

            float clarao = _idade >= afundar ? 1f - Mathf.Clamp01((_idade - afundar) / (fimClarao - afundar)) : 0f;
            for (int i = 0; i < _corpo.Length; i++)
            {
                if (_corpo[i] == null) continue;
                _corpo[i].color = Color.Lerp(_coresCorpo[i], new Color(1f, 0.35f, 0.32f, _coresCorpo[i].a), clarao * 0.85f);
            }

            if (_idade >= sumido)
            {
                for (int i = 0; i < _corpo.Length; i++) if (_corpo[i] != null) _corpo[i].color = _coresCorpo[i];
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_corpo == null) return;
            for (int i = 0; i < _corpo.Length; i++) if (_corpo[i] != null) _corpo[i].color = _coresCorpo[i];
        }

        // ------------------------------------------------------------------
        // Pecas do brilho
        // ------------------------------------------------------------------

        /// <summary>O anel que abre uma vez em volta do item e some.</summary>
        private void Aureola(float t, float comprimento, float de, float to)
        {
            if (t < 0f || t > comprimento)
            {
                _halo.color = Color.clear;
                return;
            }

            float k = t / comprimento;
            _halo.transform.localScale = Vector3.one * Mathf.Lerp(de, to, SuavizarSaida(k));
            _halo.color = new Color(_brilho.r, _brilho.g, _brilho.b, 1f - k);
        }

        /// <summary>As cruzinhas que piscam em volta do item, cada uma na sua vez.</summary>
        private void Reluzir(float esmaecimento, float raioArea)
        {
            for (int i = 0; i < _faiscas.Length; i++)
            {
                float fase = _idade * 2.2f + i * 0.25f;
                float on = Mathf.Repeat(fase, 1f);
                float a = on < 0.35f ? Mathf.Sin(on / 0.35f * Mathf.PI) : 0f;
                float angulo = i * Mathf.PI * 0.5f + 0.6f + Mathf.Floor(fase) * 1.3f;
                Vector3 deslocamento = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0f) * raioArea;
                _faiscas[i].transform.localPosition = AlinharAoPixel(deslocamento);
                _faiscas[i].color = Color.Lerp(Color.white, _brilho, 0.4f) * new Color(1f, 1f, 1f, a * esmaecimento * (_idade > 0.2f ? 1f : 0f));
            }
        }

        /// <summary>As faiscas voando pra fora do peito.</summary>
        private void Estourar(float t, float comprimento)
        {
            float k = Mathf.Clamp01(t / comprimento);
            for (int i = 0; i < _faiscas.Length; i++)
            {
                float angulo = i * Mathf.PI * 2f / _faiscas.Length + 0.3f;
                float alcanceGolpe = Mathf.Lerp(0.2f, 1.3f + (i % 3) * 0.2f, SuavizarSaida(k));
                _faiscas[i].transform.localPosition = AlinharAoPixel(new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0f) * alcanceGolpe);
                _faiscas[i].color = Color.Lerp(new Color(1f, 0.9f, 0.8f), _brilho, k) * new Color(1f, 1f, 1f, 1f - k);
            }
        }

        private static Vector3 AlinharAoPixel(Vector3 v) => new Vector3(Mathf.Round(v.x * 32f) / 32f, Mathf.Round(v.y * 32f) / 32f, 0f);
        private static float SuavizarSaida(float t) => 1f - (1f - t) * (1f - t);

        private static float VoltarSuave(float t)
        {
            t = Mathf.Clamp01(t);
            const float s = 1.9f;
            t -= 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }

        // ------------------------------------------------------------------
        // As texturas do brilho, desenhadas uma vez em codigo
        // ------------------------------------------------------------------

        private static Sprite Anel()
        {
            if (_anel != null) return _anel;

            const int tamanho = 32;
            var textura = NovaTextura(tamanho);
            for (int y = 0; y < tamanho; y++)
                for (int x = 0; x < tamanho; x++)
                {
                    float dx = x + 0.5f - tamanho * 0.5f, dy = y + 0.5f - tamanho * 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    textura.SetPixel(x, y, d > 13f && d < 15f ? Color.white : Color.clear);
                }

            textura.Apply();
            _anel = Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), 32f);
            return _anel;
        }

        private static Sprite Faisca()
        {
            if (_faisca != null) return _faisca;

            var textura = NovaTextura(5);
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                    textura.SetPixel(x, y, (x == 2 || y == 2) ? Color.white : Color.clear);
            textura.SetPixel(0, 2, new Color(1f, 1f, 1f, 0.5f));
            textura.SetPixel(4, 2, new Color(1f, 1f, 1f, 0.5f));
            textura.SetPixel(2, 0, new Color(1f, 1f, 1f, 0.5f));
            textura.SetPixel(2, 4, new Color(1f, 1f, 1f, 0.5f));
            textura.Apply();
            _faisca = Sprite.Create(textura, new Rect(0, 0, 5, 5), new Vector2(0.5f, 0.5f), 32f);
            return _faisca;
        }

        private static Sprite SpriteDoRubi()
        {
            if (_rubi != null) return _rubi;

            var textura = Resources.Load<Texture2D>("HudIcons/gem");
            if (textura == null) return null;
            _rubi = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0.5f, 0.5f), 32f);
            return _rubi;
        }

        private static Texture2D NovaTextura(int tamanho)
        {
            return new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
        }
    }
}
