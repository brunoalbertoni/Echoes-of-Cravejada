# Purification Cartoon Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** When the Rusty Scrapper (Phase 1 boss) dies, after the existing in-game purification ceremony, play a full-screen illustrated "cartoon" cutscene (5 shots) in which Scrap transforms part by part and a cave air pipe comes back to life, then return to gameplay with Scrap already polished.

**Architecture:** Art is drawn by code in the Editor (Unity-free, on `PixelCanvas`), one 320×180 "before", "after" and "order map" per shot, saved as readable PNGs. A runtime player (`PurificationCartoon`) composites each frame on the CPU (before/after chosen per pixel by the order map against a moving threshold, a glowing band at the edge, pixel particles drawn on top) into one 320×180 texture shown full-screen with OnGUI. `RustyScrapper.Fall` calls it in place of the old `Polish()` blink.

**Tech Stack:** Unity 6000.5.7f1, C# 9, `PixelCanvas`/`Rgba` (Assets/Editor), OnGUI, Input System.

**Spec:** `docs/superpowers/specs/2026-10-28-purification-cartoon-design.md`

## Global Constraints

- Only Phase 1 (Rusty Scrapper). Phases 2/3 untouched.
- Five shots, in this order: back view → arm → chisel → air pipe → final pose.
- Panel size 320×180, pixel art, point filtering, no stretching (letterbox/pillarbox to 16:9).
- Colors come from `HeroArt.Scrap()` (before) and `HeroArt.Polished()` (after); outline `Rgba(22,18,22)`.
- Total ≈14 s: shot durations 3.0, 2.5, 2.5, 3.0, 3.5 s.
- Skippable with jump or attack after the first 1.0 s; skipping still polishes Scrap.
- When gameplay resumes, Scrap is polished (`ScrapAppearance.Polish()` called during the final flash) and controls are unlocked.
- No sound. The existing in-game ceremony (`AreaPurifier.Purify`) stays as is and plays before the cartoon.
- All in-game text in correct Portuguese. Code comments in Portuguese without accents, like the rest of the project.
- Do not commit or push (the user does it). Verify compilation with `scratchpad/unitybuild.sh` (prints `SCRIPTS OK` / `EDITOR OK`), run from the project root.

## Review Focus

1. **Skip pressed mid-shot** → jumps to the final 0.8 s of shot 5, `onPolish` still fires exactly once, controls come back.
2. **Scene generated before this change / textures missing or not readable** → `RustyScrapper` falls back to the old `Polish()` blink; no exception, no frozen player.
3. **Non-16:9 window (4:3, ultrawide, resized mid-play)** → panel keeps 16:9 with black bars, recomputed every OnGUI.
4. **Leaving the scene mid-cutscene (death/reload)** → `PurificationCartoon.IsPlaying` is false in the next scene; `Cinematic` state not stuck.
5. **Same frame skip + natural end** → `onPolish` invoked once (guard flag), coroutine ends once.

---

### Task 1: Art framework, preview harness, shots 1–2 (back view, arm)

**Files:**
- Create: `Assets/Editor/PurificationCartoonArt.cs`
- Create (scratchpad, not in project): `scratchpad/cartoon/cartoon.csproj`, `scratchpad/cartoon/Program.cs`, `scratchpad/cartoon/preview.py`

**Interfaces:**
- Produces (namespace `EchoesOfCravejada.EditorTools`, no `UnityEngine`):
  ```csharp
  public static class PurificationCartoonArt
  {
      public const int Width = 320, Height = 180;
      public const byte Static = 255;              // order value: pixel never changes
      public enum EmitterKind { Flakes, Sparks, Steam, Glint }
      public readonly struct Emitter { public readonly EmitterKind Kind; public readonly int X, Y, W, H; public readonly float StartAt; /* 0..1 of the reveal */ }
      public sealed class Shot
      {
          public PixelCanvas Before, After, Order;   // Order: R channel = 0..255
          public PixelCanvas[] Loop;                 // optional frames cycled after the reveal (final pose cape), else null
          public Emitter[] Emitters;
          public float Duration;                     // seconds
          public float PanX, PanY, ZoomFrom, ZoomTo; // in-shot camera: pixel offset over the shot, zoom 1.0..1.15
      }
      public static Shot Back();   // shot 1
      public static Shot Arm();    // shot 2
      public static Shot Chisel(); // shot 3 (Task 2)
      public static Shot Vent();   // shot 4 (Task 2)
      public static Shot Hero();   // shot 5 (Task 3)
      public static Shot[] All();  // the five, in order
  }
  ```
- Reveal rule (shared by preview and runtime, must match): at reveal progress `p` in 0..1, `threshold = p * 270`; for order `o != Static`: `o < threshold - 14` → After; `threshold - 14 <= o < threshold` → edge glow (After lerped 60% toward `Rgba(255,240,190)`); else Before. Order `Static` → Before (Before and After are identical there).

- [ ] **Step 1: Build the preview harness.** `cartoon.csproj` (net9.0, same style as `scratchpad/hero/hero.csproj`) compiles `PixelCanvas.cs`, `HeroArt.cs`, `PurificationCartoonArt.cs`. `Program.cs` writes, per shot `i`, `out/shot{i}_before.raw`, `_after.raw`, `_order.raw` (RGBA bytes via `ToBytes()`), and `_loop{k}.raw`. `preview.py` (Pillow) turns each shot into `preview_shot{i}.png` (before | after | order side by side, ×3 scale) and `cartoon.gif` simulating the whole cutscene at 12 fps with the reveal rule above, the pan/zoom, and simple particle dots per emitter.

- [ ] **Step 2: Draw shot 1 `Back()`.** Scrap seen from behind, centered low (~90 px tall): round body, cape with tattered hem covering the back, rock head with the coil spring on top, chisel handle over the shoulder. In front of him (background), the heart's light: a big soft radial glow with rays, cave walls dark around. After = polished look (crystal head, leather cape with gold trim, bronze). Order: from the spring/head outward and down to the feet (radial from head center + noise of ±20 so it breaks in "chunks", not a smooth circle). Emitters: `Flakes` over the body (StartAt 0.1), `Sparks` around the head (StartAt 0.3). Duration 3.0, ZoomFrom 1.0 → ZoomTo 1.08.

- [ ] **Step 3: Draw shot 2 `Arm()`.** Close-up filling the frame: forearm entering from the left, round glove gripping the chisel handle. Before: rusty metal with a crust (lumpy rust shapes, cracks). After: brushed bronze with rivets and a highlight. Order: shoulder side (left) → fingers (right), quantized in plates of ~12 px so whole plates flip at once; crack lines get lower order values than their surroundings so cracks appear first. Emitters: `Flakes` falling from the edge region (StartAt 0.0). Duration 2.5, PanX −12.

- [ ] **Step 4: Run the harness and look at the previews.**
  Run: `dotnet run --project scratchpad/cartoon -- scratchpad/cartoon/out && python scratchpad/cartoon/preview.py`
  Expected: `preview_shot0.png`, `preview_shot1.png`, `cartoon.gif` exist; read the PNGs with the Read tool and check the silhouette reads as Scrap (head, spring, cape, chisel).

### Task 2: Shots 3–4 (chisel, air pipe)

**Files:**
- Modify: `Assets/Editor/PurificationCartoonArt.cs`

**Interfaces:**
- Consumes: `Shot`, `Emitter`, reveal rule from Task 1.

- [ ] **Step 1: Draw shot 3 `Chisel()`.** Diagonal close-up of the chisel, handle bottom-left → blade top-right, blade large (~170 px). Before: `Look.Scrap().Blade` + rust patches `BladeSpot`, nicks on the edge. After: `Look.Polished().Blade`, clean bevel, sharp edge highlight. Order: along the handle→tip axis. Emitters: `Flakes` (StartAt 0.0), `Glint` running along the edge (StartAt 0.85). Duration 2.5, ZoomFrom 1.0 → 1.12.

- [ ] **Step 2: Draw shot 4 `Vent()`.** Cave wall (dark greenish rock like the Phase 1 tileset), a thick air pipe coming out of the rock with two flanged joints and a nozzle at the top. Before: rusted, dented, a crack. After: clean metal, glowing joints, moss and small crystals on the rock lit up (those rock pixels also get order values). Order: from the pipe base up to the nozzle, then the surrounding moss/crystals last. Emitters: `Flakes` along the pipe (StartAt 0.05), `Steam` at the nozzle (StartAt 0.75, strong upward jets). Duration 3.0, PanY −10.

- [ ] **Step 3: Rerun the harness and check previews 2–3.** Same command as Task 1 Step 4.

### Task 3: Shot 5 (final pose) — user approval checkpoint

**Files:**
- Modify: `Assets/Editor/PurificationCartoonArt.cs`

- [ ] **Step 1: Draw shot 5 `Hero()`.** The showpiece: polished Scrap, front three-quarter view, ~150 px tall, chisel planted beside him or held across, crystal head with facets and highlights, two round glowing blue eyes with halo, coil spring, leather cape with gold trim and brooch blowing to one side, bronze body with rivets. Background: light rays from behind, floating particles, cave silhouette. Before = same drawing as a dark silhouette (outline color + rim light). Order: from the eyes outward (eyes light first, then head, body, cape, background rays last). `Loop`: 4 frames with the cape hem and rays shifting (cycled at 6 fps after the reveal). Emitters: `Sparks` around (StartAt 0.4). Duration 3.5, ZoomFrom 1.12 → 1.0 (pull back).

- [ ] **Step 2: Render previews and the full GIF, show the user.** Run the harness; send the user the paths of `preview_shot0..4.png` and `cartoon.gif` (full absolute paths). **Wait for approval of the art before Task 4.** Apply requested changes and rerun until approved.

### Task 4: Runtime player `PurificationCartoon`

**Files:**
- Create: `Assets/Scripts/PurificationCartoon.cs`

**Interfaces:**
- Produces (namespace `EchoesOfCravejada`):
  ```csharp
  [System.Serializable] public class CartoonEmitter { public CartoonEmitter.Kind kind; public RectInt area; public float startAt; public enum Kind { Flakes, Sparks, Steam, Glint } }
  [System.Serializable] public class CartoonShot { public Texture2D before, after, order; public Texture2D[] loop; public CartoonEmitter[] emitters; public float duration; public Vector2 pan; public float zoomFrom = 1f, zoomTo = 1f; }
  public class PurificationCartoon : MonoBehaviour
  {
      public static bool IsPlaying { get; }
      public void Configure(CartoonShot[] shots);
      public bool IsReady { get; }                                  // shots present, textures readable, sizes 320x180
      public IEnumerator Play(System.Action onPolish);              // runs all shots; onPolish fires once, at the final flash
  }
  ```
- `area` and pixel coordinates are y-down panel pixels (same as `PixelCanvas`).

- [ ] **Step 1: Implement `PurificationCartoon`.**
  - On `Play`: read each shot's textures once with `GetPixels32()`; create one `Texture2D(320,180, RGBA32, false)` with `filterMode = Point`; `IsPlaying = true`.
  - Per shot: reveal progress `p = clamp01(t / (duration * 0.75))` (reveal fills the first 75 % of the shot, the rest holds); composite with the reveal rule from Task 1 (flip y when reading textures: Unity textures are y-up); after the reveal, if `loop` is set, use `loop[(int)(t*6) % loop.Length]` as the image; draw particles as 1–2 px pixels into the buffer (Flakes: rust-colored, fall with gravity and drift; Sparks: warm white, rise and fade; Steam: pale gray puffs growing upward from the area; Glint: a bright 3-px streak sweeping along the area's diagonal); `SetPixels32` + `Apply(false)`.
  - Cuts: 2 frames of solid `Rgba(255,247,224)` between shots. Final shot: at `t >= duration - 0.6` a 0.35 s white flash; `onPolish` fires at the flash peak; then fade to black 0.25 s and end.
  - OnGUI (`GUI.depth = -1000`, only while playing): fill screen black, compute the largest 16:9 rect centered, apply in-shot zoom/pan via `GUI.DrawTextureWithTexCoords` (uv rect shrinks by `1/zoom`, offset by `pan * progress / 320|180`, clamped inside 0..1), then black letterbox bars 9 % of the rect height top and bottom.
  - Skip: after 1.0 s total, a press of jump (Space / gamepad south) or attack (J / X key / gamepad west, same keys `ScrapChisel` uses — check there) jumps to `t = duration - 0.8` of the last shot.
  - Guards: `onPolish` behind a bool; `IsPlaying = false` in `OnDisable`/`OnDestroy` and at the end; `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` resets the static.
  - `IsReady`: false if any shot or texture is null, `!isReadable`, or size ≠ 320×180.

- [ ] **Step 2: Compile.**
  Run (from project root): `sh "$SCRATCH/unitybuild.sh"`
  Expected: `SCRIPTS OK` and `EDITOR OK`.

### Task 5: Assets, generator and boss hookup

**Files:**
- Create: `Assets/Editor/CartoonAssets.cs`
- Modify: `Assets/Editor/Phase01Generator.cs` (art step near line 97–130; boss build near line 965–1033)
- Modify: `Assets/Scripts/RustyScrapper.cs` (`Configure` line 68; `Fall` lines 392–393)

**Interfaces:**
- Consumes: `PurificationCartoonArt.All()` (Task 1–3), `CartoonShot`/`PurificationCartoon` (Task 4).
- Produces:
  ```csharp
  public static class CartoonAssets   // namespace EchoesOfCravejada.EditorTools
  {
      public const string Folder = Phase01Assets.GeneratedRoot + "/Cutscene";
      public static void EnsureArt();            // writes shot{i}_before/after/order[_loop{k}].png and imports them
      public static CartoonShot[] LoadShots();   // loads the textures and converts Emitters/pan/zoom/duration
  }
  ```

- [ ] **Step 1: Implement `CartoonAssets`.** PNGs written like the other generated art (reuse the existing PNG writer in `Phase01Assets`/`Phase02Assets`). Import settings: `TextureImporterType.Default`, `isReadable = true`, `filterMode = Point`, `mipmapEnabled = false`, `textureCompression = Uncompressed`, `npotScale = None`, `wrapMode = Clamp`, `alphaIsTransparency = false`.

- [ ] **Step 2: Hook into the generator.** Call `CartoonAssets.EnsureArt()` in `Build()` after the slicing steps. In the boss build, add `PurificationCartoon` to the `RustyScrapper` object, `Configure(CartoonAssets.LoadShots())`, and pass it to `RustyScrapper.Configure` as a new last parameter `PurificationCartoon newCartoon` (serialized field `cartoon`).

- [ ] **Step 3: Use it in `RustyScrapper.Fall`.** Replace `yield return Polish(controller);` with: if `cartoon != null && cartoon.IsReady`, `yield return cartoon.Play(() => appearance.Polish())` while keeping `ControlLocked` true (call `Hold()` each frame of a wrapper loop or lock before and after), then `HudNotice.Show("A purificação arrancou a ferrugem do seu corpo")`; else keep the old `Polish(controller)`. Keep `Cinematic.End()` and the unlock after it.

- [ ] **Step 4: Compile.**
  Run (from project root): `sh "$SCRATCH/unitybuild.sh"`
  Expected: `SCRIPTS OK` and `EDITOR OK`.

- [ ] **Step 5: Tell the user how to see it.** Regenerate Phase 1 (menu Echoes > Gerar Phase 1), play to the Rusty Scrapper and kill it. No commit (the user pushes).
