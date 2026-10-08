using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O HUD provisorio: vida, Opaque Shards e os avisos da alavanca e da
    /// bancada. E desenhado com IMGUI, sem asset nenhum, pra as mecanicas
    /// poderem ser testadas ja — troque por uma interface de verdade depois.
    /// </summary>
    public class GameHud : MonoBehaviour
    {
        private ScrapHealth _health;
        private GUIStyle _label;
        private GUIStyle _speech;
        private bool _checkedForEcho;
        private bool _showNoise;

        private static readonly Color HeartFull = new Color(0.95f, 0.80f, 0.45f);
        private static readonly Color HeartEmpty = new Color(0.25f, 0.20f, 0.18f);
        private static readonly Color ShardColor = new Color(0.60f, 0.72f, 0.78f);

        private void OnGUI()
        {
            if (_health == null) _health = FindAnyObjectByType<ScrapHealth>();

            if (_label == null)
            {
                _label = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
                _label.normal.textColor = Color.white;
            }

            const float margin = 16f;
            const float size = 18f;

            if (_health != null)
            {
                for (int i = 0; i < _health.MaxHealth; i++)
                {
                    var rect = new Rect(margin + i * (size + 6f), margin, size, size);
                    Fill(new Rect(rect.x - 2f, rect.y - 2f, size + 4f, size + 4f), Color.black);
                    Fill(rect, i < _health.Health ? HeartFull : HeartEmpty);
                }
            }

            Fill(new Rect(margin, margin + 32f, 12f, 16f), ShardColor);
            GUI.Label(new Rect(margin + 20f, margin + 27f, 200f, 28f), ScrapWallet.Shards.ToString(), _label);

            RustyLever lever = RustyLever.Nearby;
            if (lever != null)
            {
                float width = 240f;
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height * 0.72f;

                GUI.Label(new Rect(x, y - 28f, width + 80f, 28f), "Segure E para raspar a ferrugem", _label);
                Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
                Fill(new Rect(x, y, width, 12f), HeartEmpty);
                Fill(new Rect(x, y, width * lever.Progress, 12f), HeartFull);
            }

            DrawDialogue();
            DrawDistrict2();
            DrawDistrict3();

            RustyScrapper boss = RustyScrapper.Fighting;
            if (boss != null)
            {
                float width = Screen.width * 0.5f;
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height - 46f;

                GUI.Label(new Rect(x, y - 26f, 320f, 28f), "The Rusty Scrapper", _label);
                Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
                Fill(new Rect(x, y, width, 12f), HeartEmpty);
                Fill(new Rect(x, y, width * boss.HealthFraction, 12f), new Color(0.45f, 0.75f, 0.55f));
            }

            if (Time.time - ScrapDash.UnlockTime < 5f)
            {
                GUI.Label(new Rect((Screen.width - 380f) * 0.5f, Screen.height * 0.32f, 420f, 28f),
                    "Steam Dash conquistado: Shift esquerdo", _label);
            }

            if (Time.time - AreaPurifier.PurifiedTime < 5f)
            {
                GUI.Label(new Rect((Screen.width - 220f) * 0.5f, Screen.height * 0.18f, 320f, 28f),
                    "Area purificada", _label);
            }

            if (Time.time - Workbench.LastRestTime < 2.5f)
            {
                GUI.Label(new Rect((Screen.width - 220f) * 0.5f, Screen.height * 0.25f, 320f, 28f),
                    "Bancada: progresso salvo", _label);
            }
        }

        /// <summary>O que o District 2 trouxe: valvulas, o Boiler Master, a loja de Crisol e os avisos.</summary>
        private void DrawDistrict2()
        {
            Valve valve = Valve.Nearby;
            if (valve != null)
            {
                float width = 240f;
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height * 0.72f;

                GUI.Label(new Rect(x, y - 28f, width + 120f, 28f), "Segure E para raspar e girar a valvula", _label);
                Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
                Fill(new Rect(x, y, width, 12f), HeartEmpty);
                Fill(new Rect(x, y, width * valve.Progress, 12f), HeartFull);
            }

            BoilerMaster master = BoilerMaster.Fighting;
            if (master != null)
            {
                float width = Screen.width * 0.5f;
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height - 46f;

                string title = master.IsExposed ? "The Boiler Master  -  FORNALHA EXPOSTA" : "The Boiler Master";
                GUI.Label(new Rect(x, y - 26f, 520f, 28f), title, _label);
                Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
                Fill(new Rect(x, y, width, 12f), HeartEmpty);
                Fill(new Rect(x, y, width * master.HealthFraction, 12f),
                    master.IsExposed ? new Color(1f, 0.55f, 0.25f) : new Color(0.55f, 0.6f, 0.7f));
            }

            Crisol shop = Crisol.ShopOpen;
            if (shop != null)
            {
                float width = 520f;
                float height = 170f;
                float x = (Screen.width - width) * 0.5f;
                float y = (Screen.height - height) * 0.5f;

                Fill(new Rect(x - 3f, y - 3f, width + 6f, height + 6f), HeartFull);
                Fill(new Rect(x, y, width, height), new Color(0.07f, 0.06f, 0.08f, 0.97f));

                GUI.Label(new Rect(x + 18f, y + 10f, width, 28f), "Loja de Crisol", _label);
                GUI.Label(new Rect(x + 18f, y + 48f, width, 28f),
                    "[1]  Reforco de chassi (+1 de vida)  -  " + shop.ChassisPrice + " shards", _label);
                GUI.Label(new Rect(x + 18f, y + 86f, width - 36f, 28f), shop.LastMessage, _label);
                GUI.Label(new Rect(x + 18f, y + height - 34f, width, 28f), "E ou Esc: sair", _label);
            }

            float centerX = (Screen.width - 460f) * 0.5f;

            if (Time.time - ScrapGems.RubyGivenTime < 6f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.14f, 560f, 28f),
                    "Rubi bruto recebido: crave numa Workbench", _label);
            }

            if (Time.time - ScrapGems.RubySocketedTime < 6f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.30f, 560f, 28f),
                    "Rubi cravado: o cinzel bate mais forte", _label);
            }

            if (Time.time - ScrapController.PressureJumpTime < 6f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.32f, 560f, 28f),
                    "Pressure Jump conquistado: pule de novo no ar", _label);
            }

            if (Time.time - RisingAcidTrigger.StartTime < 5f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.22f, 560f, 28f),
                    "O acido esta subindo. Suba!", _label);
            }
        }

        /// <summary>O que o District 3 trouxe: o aviso de raspar um cristal e o medidor de ruido.</summary>
        private void DrawDistrict3()
        {
            if (HoldAction.Owner != null)
            {
                float width = 240f;
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height * 0.72f;

                GUI.Label(new Rect(x, y - 28f, width + 160f, 28f), HoldAction.Text, _label);
                Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
                Fill(new Rect(x, y, width, 12f), HeartEmpty);
                Fill(new Rect(x, y, width * Mathf.Clamp01(HoldAction.Progress), 12f), HeartFull);
            }

            TheResonant elite = TheResonant.Fighting;
            if (elite != null)
            {
                BossBar("The Resonant", elite.HealthFraction, new Color(0.60f, 0.80f, 1f));
            }

            GeodeDrill drill = GeodeDrill.Fighting;
            if (drill != null)
            {
                BossBar(drill.IsExposed ? "The Geode Drill  -  MOTOR EXPOSTO" : "The Geode Drill",
                    drill.HealthFraction,
                    drill.IsExposed ? new Color(1f, 0.55f, 0.25f) : new Color(0.62f, 0.55f, 0.80f));
            }

            float centerX = (Screen.width - 520f) * 0.5f;

            if (Time.time - ScrapGems.SapphireGivenTime < 6f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.14f, 620f, 28f),
                    "Safira bruta recebida: crave numa Workbench", _label);
            }

            if (Time.time - ScrapGems.SapphireSocketedTime < 6f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.34f, 620f, 28f),
                    "Safira cravada: passos e quedas fazem menos ruido", _label);
            }

            if (Time.time - ScrapChisel.ResonanceTime < 8f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.32f, 720f, 28f),
                    "Resonance Strike conquistado: segure o golpe e solte", _label);
            }

            if (Time.time < ScrapController.SlowedUntil)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.80f, 620f, 28f),
                    "Mercurio grudado no corpo: voce esta lento", _label);
            }

            if (Time.time - RewardPickup.ChassisTime < 5f)
            {
                GUI.Label(new Rect(centerX, Screen.height * 0.32f, 620f, 28f),
                    "Reforco de chassi encontrado: +1 de vida", _label);
            }

            // O medidor de ruido: sobe na hora do som e desce em um segundo.
            // So aparece em cena que tenha cristal de eco, pra nao poluir as outras.
            if (!_checkedForEcho)
            {
                _checkedForEcho = true;
                _showNoise = FindAnyObjectByType<EchoCrystal>() != null;
            }

            if (!_showNoise) return;

            float age = Time.time - NoiseSystem.LastTime;
            float level = NoiseSystem.LastLoudness * Mathf.Clamp01(1f - age);

            const float barWidth = 120f;
            float barX = 16f;
            float barY = 86f;

            GUI.Label(new Rect(barX, barY - 24f, 200f, 26f), "Ruido", _label);
            Fill(new Rect(barX - 2f, barY - 2f, barWidth + 4f, 14f), Color.black);
            Fill(new Rect(barX, barY, barWidth, 10f), HeartEmpty);
            Fill(new Rect(barX, barY, barWidth * level, 10f),
                level > 0.3f ? new Color(0.95f, 0.45f, 0.75f) : new Color(0.7f, 0.6f, 0.95f));
        }

        private void BossBar(string title, float fraction, Color color)
        {
            float width = Screen.width * 0.5f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - 46f;

            GUI.Label(new Rect(x, y - 26f, 520f, 28f), title, _label);
            Fill(new Rect(x - 2f, y - 2f, width + 4f, 16f), Color.black);
            Fill(new Rect(x, y, width, 12f), HeartEmpty);
            Fill(new Rect(x, y, width * fraction, 12f), color);
        }

        /// <summary>A caixa de fala de um NPC, ou o aviso de que da pra conversar.</summary>
        private void DrawDialogue()
        {
            Npc talking = Npc.Talking;

            if (talking == null)
            {
                Npc nearby = Npc.Nearby;
                if (nearby != null)
                {
                    GUI.Label(new Rect((Screen.width - 260f) * 0.5f, Screen.height * 0.72f, 360f, 28f),
                        "E: " + nearby.Prompt + " " + nearby.DisplayName, _label);
                }

                return;
            }

            string line = talking.CurrentLine;
            if (line == null) return;

            float width = Mathf.Min(Screen.width - 80f, 900f);
            float height = 130f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 70f;

            Fill(new Rect(x - 3f, y - 3f, width + 6f, height + 6f), HeartFull);
            Fill(new Rect(x, y, width, height), new Color(0.07f, 0.06f, 0.08f, 0.96f));

            if (_speech == null)
            {
                _speech = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, alignment = TextAnchor.UpperLeft };
                _speech.normal.textColor = new Color(0.93f, 0.90f, 0.82f);
            }

            Color previous = GUI.contentColor;
            GUI.contentColor = HeartFull;
            GUI.Label(new Rect(x + 18f, y + 10f, width - 36f, 28f), talking.DisplayName, _label);
            GUI.contentColor = previous;

            GUI.Label(new Rect(x + 18f, y + 42f, width - 36f, height - 52f), line, _speech);
            GUI.Label(new Rect(x + width - 150f, y + height - 30f, 140f, 26f), "E: continuar", _label);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
