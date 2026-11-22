using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Um filme sendo montado no editor: os quadros desenhados (PixelCanvas)
    /// e os planos (FilmShot) que apontam pra eles. O FilmAssets grava os
    /// quadros como PNG e entrega o filme pro FilmCutscene; a previa fora do
    /// Unity toca o mesmo filme pelo FilmCompositor.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public sealed class FilmDraft
    {
        public readonly List<PixelCanvas> Frames = new List<PixelCanvas>();
        public readonly List<FilmShot> Shots = new List<FilmShot>();

        public ShotDraft Shot(string name, float duration, Rgba background)
        {
            var shot = new FilmShot
            {
                name = name,
                duration = duration,
                background = 0xFF000000u | (uint)background.R << 16 | (uint)background.G << 8 | background.B,
            };
            Shots.Add(shot);
            return new ShotDraft(this, shot);
        }

        public int Add(PixelCanvas frame)
        {
            Frames.Add(frame);
            return Frames.Count - 1;
        }

        /// <summary>Os quadros como o compositor le (pra previa fora do Unity).</summary>
        public List<FilmFrame> ToFrames()
        {
            var list = new List<FilmFrame>(Frames.Count);
            foreach (PixelCanvas c in Frames) list.Add(ToFrame(c));
            return list;
        }

        public static FilmFrame ToFrame(PixelCanvas c)
        {
            var pixels = new uint[c.Width * c.Height];
            for (int i = 0; i < pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                pixels[i] = (uint)p.A << 24 | (uint)p.R << 16 | (uint)p.G << 8 | p.B;
            }

            return new FilmFrame(c.Width, c.Height, pixels);
        }

        public float Duration
        {
            get
            {
                float total = 0f;
                foreach (FilmShot s in Shots) total += s.duration;
                return total;
            }
        }
    }

    /// <summary>Monta um plano: camadas, camera, tremores, claroes, particulas e sons.</summary>
    public sealed class ShotDraft
    {
        private readonly FilmDraft _film;
        private readonly FilmShot _shot;
        private readonly List<FilmLayer> _layers = new List<FilmLayer>();
        private readonly List<FilmCam> _cams = new List<FilmCam>();
        private readonly List<FilmShake> _shakes = new List<FilmShake>();
        private readonly List<FilmFlash> _flashes = new List<FilmFlash>();
        private readonly List<FilmSpray> _sprays = new List<FilmSpray>();
        private readonly List<FilmCue> _cues = new List<FilmCue>();

        public ShotDraft(FilmDraft film, FilmShot shot)
        {
            _film = film;
            _shot = shot;
            Sync();
        }

        public FilmShot Shot => _shot;
        public float Duration => _shot.duration;

        private void Sync()
        {
            _shot.layers = _layers.ToArray();
            _shot.camera = _cams.Count > 0 ? _cams.ToArray() : new[] { new FilmCam(0f, 160f, 90f) };
            _shot.shakes = _shakes.ToArray();
            _shot.flashes = _flashes.ToArray();
            _shot.sprays = _sprays.ToArray();
            _shot.cues = _cues.ToArray();
        }

        /// <summary>Uma camada parada (um quadro) com o pivo em (x, y) do palco.</summary>
        public FilmLayer Layer(string name, PixelCanvas art, float x, float y, float parallax = 1f,
            float pivotX = 0.5f, float pivotY = 0.5f, bool additive = false)
        {
            return Anim(name, new[] { art }, 1f, 0f, false, x, y, parallax, pivotX, pivotY, additive);
        }

        /// <summary>Uma camada animada.</summary>
        public FilmLayer Anim(string name, PixelCanvas[] frames, float fps, float start, bool loop, float x, float y,
            float parallax = 1f, float pivotX = 0.5f, float pivotY = 0.5f, bool additive = false)
        {
            int first = -1;
            foreach (PixelCanvas frame in frames)
            {
                int index = _film.Add(frame);
                if (first < 0) first = index;
            }

            var layer = new FilmLayer
            {
                name = name, firstFrame = first, frameCount = frames.Length, fps = fps, animStart = start, loop = loop,
                parallax = parallax, pivotX = pivotX, pivotY = pivotY, additive = additive,
                keys = new[] { new FilmKey(0f, x, y) },
            };
            _layers.Add(layer);
            Sync();
            return layer;
        }

        /// <summary>A mesma arte de outra camada (sem gravar o quadro de novo).</summary>
        public FilmLayer Reuse(FilmLayer source, string name, float x, float y, float parallax = 1f, bool additive = false)
        {
            var layer = new FilmLayer
            {
                name = name, firstFrame = source.firstFrame, frameCount = source.frameCount, fps = source.fps,
                animStart = source.animStart, loop = source.loop, parallax = parallax, pivotX = source.pivotX,
                pivotY = source.pivotY, additive = additive, keys = new[] { new FilmKey(0f, x, y) },
            };
            _layers.Add(layer);
            Sync();
            return layer;
        }

        public ShotDraft Cam(params FilmCam[] keys)
        {
            _cams.AddRange(keys);
            Sync();
            return this;
        }

        public ShotDraft Shake(float t, float duration, float amount)
        {
            _shakes.Add(new FilmShake(t, duration, amount));
            Sync();
            return this;
        }

        public ShotDraft Flash(float t, float duration, Rgba color, float strength = 1f)
        {
            _flashes.Add(new FilmFlash(t, duration, 0xFF000000u | (uint)color.R << 16 | (uint)color.G << 8 | color.B, strength));
            Sync();
            return this;
        }

        public FilmSpray Spray(int shape, float x, float y, float w, float h, float from, float to, float rate,
            Rgba color, Rgba color2, float life, int seed)
        {
            var spray = new FilmSpray
            {
                shape = shape, x = x, y = y, w = w, h = h, from = from, to = to, rate = rate, life = life, seed = seed,
                color = (uint)color.A << 24 | (uint)color.R << 16 | (uint)color.G << 8 | color.B,
                color2 = (uint)color2.A << 24 | (uint)color2.R << 16 | (uint)color2.G << 8 | color2.B,
            };
            _sprays.Add(spray);
            Sync();
            return spray;
        }

        public ShotDraft Cue(float t, string sound, float pitch = 1f, float volume = 1f)
        {
            _cues.Add(new FilmCue(t, sound, pitch, volume));
            Sync();
            return this;
        }

        public ShotDraft Fades(float fadeIn, float fadeOut)
        {
            _shot.fadeIn = fadeIn;
            _shot.fadeOut = fadeOut;
            return this;
        }

        public ShotDraft Vignette(float amount)
        {
            _shot.vignette = amount;
            return this;
        }
    }

    public static class FilmLayerKeys
    {
        /// <summary>Troca as chaves da camada (a primeira vale ate o primeiro instante).</summary>
        public static FilmLayer Keys(this FilmLayer layer, params FilmKey[] keys)
        {
            layer.keys = keys;
            return layer;
        }
    }

    public static class FilmSprayTweaks
    {
        public static FilmSpray Move(this FilmSpray s, float vx, float vy, float spreadX, float spreadY, float gravity = 0f)
        {
            s.vx = vx;
            s.vy = vy;
            s.spreadX = spreadX;
            s.spreadY = spreadY;
            s.gravity = gravity;
            return s;
        }

        public static FilmSpray Look(this FilmSpray s, int size, float grow = 0f, bool additive = false, float parallax = 1f)
        {
            s.size = size;
            s.grow = grow;
            s.additive = additive;
            s.parallax = parallax;
            return s;
        }

        public static FilmSpray Burst(this FilmSpray s, float power)
        {
            s.burst = power;
            return s;
        }
    }
}
