using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Cria um Animator Controller por personagem, com um clipe por sequencia
    /// (Idle, Walk, Windup...), a partir das listas de quadros das cenas.
    /// Rodar de novo depois de gerar uma fase.
    /// </summary>
    public static class AnimatorBaker
    {
        private const string Root = "Assets/Animations";
        private const float SampleRate = 12f;

        private static readonly Dictionary<Type, string[]> Sources = new Dictionary<Type, string[]>
        {
            { typeof(ScrapAnimator), new[] { "quadrosSucata", "quadrosPolido" } },
            { typeof(MotionFrames), new[] { "parado", "mover" } },
            { typeof(DeathFrames), new[] { "quadros" } },
            { typeof(SpriteLoop), new[] { "quadros" } },
            { typeof(CrawlerEnemy), new[] { "quadros" } },
            { typeof(RustyScrapper), new[] { "quadros", "quadrosRachado" } },
            { typeof(BoilerMaster), new[] { "quadros", "quadrosQuente" } },
            { typeof(AcidBloater), new[] { "quadros" } },
            { typeof(BrassWelder), new[] { "quadros" } },
            { typeof(PipeLeech), new[] { "quadros" } },
            { typeof(ValveSentry), new[] { "quadros" } },
            { typeof(TheResonant), new[] { "quadros" } },
            { typeof(CrystalMite), new[] { "quadros" } },
            { typeof(EchoHunter), new[] { "quadros" } },
            { typeof(ShardSentinel), new[] { "quadros" } },
            { typeof(MercuryAlchemist), new[] { "quadros" } },
            { typeof(FailedExperiment), new[] { "quadros" } },
            { typeof(FlaskThrower), new[] { "quadros" } },
            { typeof(QuicksilverSlime), new[] { "quadros" } },
            { typeof(TransmutedSentinel), new[] { "quadrosVidro", "quadrosPrata" } },
            { typeof(SilverKing), new[] { "quadrosReal", "quadrosCorrompido" } },
        };

        private sealed class Sequence
        {
            public string Name;
            public Sprite[] Frames;
        }

        private sealed class Baked
        {
            public RuntimeAnimatorController Controller;
            public Sprite[] Sprites;
            public int[] States;
            public float[] Times;
        }

        public static void BakeAll()
        {
            if (AssetDatabase.IsValidFolder(Root)) AssetDatabase.DeleteAsset(Root);
            AssetDatabase.CreateFolder("Assets", "Animations");

            var cache = new Dictionary<string, Baked>();
            int renderers = 0;

            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes.Where(s => s.enabled))
            {
                var scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                int here = BakeScene(cache);
                renderers += here;

                if (here > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }

                Debug.Log($"[Animators] {scene.name}: {here} personagens");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Animators] PRONTO controllers={cache.Count} personagens={renderers} clipes={cache.Values.Sum(b => b.States.Distinct().Count())}");
        }

        private static int BakeScene(Dictionary<string, Baked> cache)
        {
            var groups = new Dictionary<SpriteRenderer, List<(MonoBehaviour owner, string field, Sprite[] frames)>>();

            foreach (MonoBehaviour owner in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (owner == null || !Sources.TryGetValue(owner.GetType(), out string[] fields)) continue;

                SpriteRenderer renderer = owner.GetComponentInChildren<SpriteRenderer>(true);
                if (renderer == null) continue;

                var serialized = new SerializedObject(owner);
                foreach (string field in fields)
                {
                    SerializedProperty list = serialized.FindProperty(field);
                    if (list == null || !list.isArray || list.arraySize == 0) continue;

                    var frames = new Sprite[list.arraySize];
                    for (int i = 0; i < frames.Length; i++) frames[i] = list.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;

                    if (!groups.TryGetValue(renderer, out var group)) groups[renderer] = group = new List<(MonoBehaviour, string, Sprite[])>();
                    group.Add((owner, field, frames));
                }
            }

            foreach (var pair in groups)
            {
                var ordered = pair.Value.OrderBy(g => g.owner.GetType().Name).ThenBy(g => g.field, StringComparer.Ordinal).ToList();
                string signature = string.Join("|", ordered.Select(g => g.owner.GetType().Name + ":" + g.field + ":" + string.Join(",", g.frames.Select(f => f != null ? f.name : "-"))));
                string key = Hash(signature);

                if (!cache.TryGetValue(key, out Baked baked))
                {
                    baked = Bake(ordered, key);
                    cache[key] = baked;
                }

                GameObject target = pair.Key.gameObject;
                Animator animator = target.GetComponent<Animator>();
                if (animator == null) animator = target.AddComponent<Animator>();
                animator.runtimeAnimatorController = baked.Controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                FrameAnimator frames = target.GetComponent<FrameAnimator>();
                if (frames == null) frames = target.AddComponent<FrameAnimator>();
                frames.Configurar(baked.Sprites, baked.States, baked.Times);
                EditorUtility.SetDirty(frames);
                EditorUtility.SetDirty(animator);
            }

            return groups.Count;
        }

        private static Baked Bake(List<(MonoBehaviour owner, string field, Sprite[] frames)> sources, string key)
        {
            Type main = sources.Select(s => s.owner.GetType()).OrderBy(t => t == typeof(MotionFrames) || t == typeof(DeathFrames) ? 1 : 0).First();
            Sprite first = sources.SelectMany(s => s.frames).FirstOrDefault(f => f != null);
            string art = first != null && first.texture != null ? first.texture.name : "sem_arte";
            string folderName = Clean($"{main.Name}_{art}_{key.Substring(0, 6)}");
            AssetDatabase.CreateFolder(Root, folderName);
            string folder = $"{Root}/{folderName}";

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/{folderName}.controller");
            AnimatorStateMachine machine = controller.layers[0].stateMachine;

            var sprites = new List<Sprite>();
            var states = new List<int>();
            var times = new List<float>();
            var used = new HashSet<Sprite>();
            var names = new HashSet<string>();

            foreach (var source in sources)
            {
                string prefix = sources.Count > 1 ? Clean($"{source.owner.GetType().Name}_{source.field}") + "_" : "";

                foreach (Sequence sequence in Split(source.owner.GetType(), source.field, source.frames))
                {
                    Sprite[] frames = sequence.Frames.Where(f => f != null).ToArray();
                    if (frames.Length == 0) continue;

                    string state = prefix + sequence.Name;
                    for (int n = 2; !names.Add(state); n++) state = prefix + sequence.Name + "_" + n;

                    AnimationClip clip = MakeClip(state, frames);
                    AssetDatabase.CreateAsset(clip, $"{folder}/{state}.anim");

                    AnimatorState node = machine.AddState(state);
                    node.motion = clip;
                    node.writeDefaultValues = false;

                    int hash = Animator.StringToHash(state);
                    for (int i = 0; i < frames.Length; i++)
                    {
                        if (!used.Add(frames[i])) continue;
                        sprites.Add(frames[i]);
                        states.Add(hash);
                        times.Add((i + 0.5f) / frames.Length);
                    }
                }
            }

            EditorUtility.SetDirty(controller);
            return new Baked { Controller = controller, Sprites = sprites.ToArray(), States = states.ToArray(), Times = times.ToArray() };
        }

        private static AnimationClip MakeClip(string name, Sprite[] frames)
        {
            var clip = new AnimationClip { name = name, frameRate = SampleRate };
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];

            for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / SampleRate, value = frames[i] };
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / SampleRate, value = frames[frames.Length - 1] };

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            return clip;
        }

        /// <summary>Corta a lista nas sequencias nomeadas pelas constantes do script.</summary>
        private static IEnumerable<Sequence> Split(Type type, string field, Sprite[] frames)
        {
            if (type == typeof(MotionFrames) || type == typeof(DeathFrames) || type == typeof(SpriteLoop))
            {
                string whole = type == typeof(DeathFrames) ? "Morte" : type == typeof(SpriteLoop) ? "Repeticao" : field == "parado" ? "Parado" : "Andando";
                yield return new Sequence { Name = whole, Frames = frames };
                yield break;
            }

            Dictionary<string, int> constants = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(int))
                .ToDictionary(f => f.Name, f => (int)f.GetRawConstantValue());

            var starts = constants.Where(c => !c.Key.StartsWith("Quadros", StringComparison.Ordinal) && c.Value >= 0 && c.Value < frames.Length)
                .GroupBy(c => c.Value).Select(g => g.First()).OrderBy(c => c.Value).ToList();

            int cursor = 0;
            for (int i = 0; i < starts.Count; i++)
            {
                if (starts[i].Value > cursor) yield return Slice("Extra" + cursor, frames, cursor, starts[i].Value - cursor);

                int limit = (i + 1 < starts.Count ? starts[i + 1].Value : frames.Length) - starts[i].Value;
                int count = constants.TryGetValue(starts[i].Key.Replace("Quadro", "Quadros"), out int declared) ? Mathf.Clamp(declared, 1, limit) : limit;
                yield return Slice(starts[i].Key, frames, starts[i].Value, count);
                cursor = starts[i].Value + count;
            }

            if (cursor < frames.Length) yield return Slice("Extra" + cursor, frames, cursor, frames.Length - cursor);
        }

        private static Sequence Slice(string name, Sprite[] frames, int start, int count)
        {
            return new Sequence { Name = name, Frames = frames.Skip(start).Take(count).ToArray() };
        }

        private static string Clean(string text)
        {
            return new string(text.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
        }

        private static string Hash(string text)
        {
            uint hash = 2166136261;
            foreach (char c in text) hash = (hash ^ c) * 16777619;
            return hash.ToString("x8");
        }
    }
}
