"""
Sintetiza os efeitos sonoros do jogo (pacote 1: o Scrap, a coleta e a interface).

Tudo e feito do zero com numpy: sem gravacao de terceiros, sem licenca a creditar.
Gera WAV 16-bit mono em Assets/Common/Audio/Resources/Sfx/. Rodar da raiz do projeto:

    python Tools/Audio/sfx_synth.py            # gera tudo
    python Tools/Audio/sfx_synth.py Scrap_Hit  # gera so os que comecam com esse nome

As sementes sao fixas: rodar de novo da o mesmo som. O volume de cada um no jogo
fica na tabela do Sfx.cs; aqui todo arquivo sai normalizado no mesmo pico.

Ideias por tras dos sons:
- O Scrap e de ferro, pesado: passos e quedas tem um baque grave e um pouco de
  tinido de metal; o golpe e um assobio grosso e o acerto um clangor.
- O que se coleta e cristal e gema: sinos agudos (parciais inarmonicas de barra).
- A interface e discreta: bipes curtos e macios, pra nao cansar.
"""

import math
import os
import sys
import wave

import numpy as np

SR = 44100
OUT = os.path.join("Assets", "Common", "Audio", "Resources", "Sfx")


# ----------------------------------------------------------------------
# Blocos basicos
# ----------------------------------------------------------------------

def n_of(seconds):
    return max(1, int(round(seconds * SR)))


def times(seconds):
    return np.arange(n_of(seconds)) / SR


def silence(seconds):
    return np.zeros(n_of(seconds))


def noise(seconds, rng):
    return rng.uniform(-1.0, 1.0, n_of(seconds))


def env(seconds, attack, tau, hold=0.0):
    """Sobe em linha ate o pico em `attack`, segura `hold` e cai exponencial com constante `tau`."""
    t = times(seconds)
    e = np.ones_like(t)
    if attack > 0:
        rise = t < attack
        e[rise] = t[rise] / attack
    after = t >= attack + hold
    e[after] = np.exp(-(t[after] - attack - hold) / max(tau, 1e-4))
    return e


def bell_env(seconds, peak=0.5, sharp=1.0):
    """Um envelope em sino: sobe e desce, com o pico na fracao `peak` da duracao."""
    t = np.linspace(0.0, 1.0, n_of(seconds))
    up = np.clip(t / max(peak, 1e-3), 0, 1)
    down = np.clip((1 - t) / max(1 - peak, 1e-3), 0, 1)
    return (np.sin(np.minimum(up, down) * np.pi / 2) ** 2) ** sharp


def glide(f_from, f_to, seconds, curve="exp"):
    """Frequencia ao longo do tempo, de f_from a f_to."""
    t = np.linspace(0.0, 1.0, n_of(seconds))
    if curve == "exp":
        return f_from * (f_to / f_from) ** t
    return f_from + (f_to - f_from) * t


def osc(freq, seconds, shape="sine", phase=0.0):
    """Um oscilador; freq pode ser numero ou vetor (glissando)."""
    n = n_of(seconds)
    f = np.full(n, float(freq)) if np.isscalar(freq) else np.resize(freq, n)
    ph = 2 * np.pi * np.cumsum(f) / SR + phase
    if shape == "sine":
        return np.sin(ph)
    if shape == "tri":
        return 2 / np.pi * np.arcsin(np.sin(ph))
    if shape == "square":
        # Quadrada suave (soma de harmonicas impares ate ~8 kHz), sem serrilhado.
        out = np.zeros(n)
        for k in range(1, 40, 2):
            mask = f * k < 8000
            if not mask.any():
                break
            out += np.where(mask, np.sin(ph * k) / k, 0.0)
        return out * 4 / np.pi
    if shape == "saw":
        out = np.zeros(n)
        for k in range(1, 60):
            mask = f * k < 9000
            if not mask.any():
                break
            out += np.where(mask, np.sin(ph * k) / k, 0.0) * (-1) ** (k + 1)
        return out * 2 / np.pi
    raise ValueError(shape)


def modes(seconds, f0, ratios, decays, amps, rng=None, detune=0.0):
    """
    Sintese modal: um corpo que vibra em varias frequencias ao mesmo tempo,
    cada uma morrendo no seu ritmo. E o que faz metal e sino soarem como tal.
    """
    t = times(seconds)
    out = np.zeros_like(t)
    for r, d, a in zip(ratios, decays, amps):
        f = f0 * r
        if f > SR / 2 - 500:
            continue
        ph = rng.uniform(0, 2 * np.pi) if rng is not None else 0.0
        out += a * np.exp(-t / d) * np.sin(2 * np.pi * f * t + ph)
        if detune:
            out += a * 0.6 * np.exp(-t / d) * np.sin(2 * np.pi * (f + detune) * t + ph * 1.3)
    return out


def bell(freq, seconds, decay=0.6, rng=None, ratios=(1.0, 2.76, 5.4, 8.93), amps=(1.0, 0.45, 0.22, 0.1), detune=0.0):
    """Um sino de barra (glockenspiel): as parciais agudas morrem mais rapido."""
    decays = [decay / (1 + i * 1.6) for i in range(len(ratios))]
    return modes(seconds, freq, ratios, decays, amps, rng, detune)


# ----------------------------------------------------------------------
# Filtros
# ----------------------------------------------------------------------

def _biquad(x, b0, b1, b2, a0, a1, a2):
    b0, b1, b2, a1, a2 = b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0
    out = [0.0] * len(x)
    x1 = x2 = y1 = y2 = 0.0
    for i, xi in enumerate(x.tolist()):
        yi = b0 * xi + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2
        x2, x1, y2, y1 = x1, xi, y1, yi
        out[i] = yi
    return np.array(out)


def lowpass(x, fc, q=0.707):
    w = 2 * np.pi * min(fc, SR * 0.45) / SR
    alpha = np.sin(w) / (2 * q)
    c = np.cos(w)
    return _biquad(x, (1 - c) / 2, 1 - c, (1 - c) / 2, 1 + alpha, -2 * c, 1 - alpha)


def highpass(x, fc, q=0.707):
    w = 2 * np.pi * min(fc, SR * 0.45) / SR
    alpha = np.sin(w) / (2 * q)
    c = np.cos(w)
    return _biquad(x, (1 + c) / 2, -(1 + c), (1 + c) / 2, 1 + alpha, -2 * c, 1 - alpha)


def bandpass(x, fc, q=1.0):
    w = 2 * np.pi * min(fc, SR * 0.45) / SR
    alpha = np.sin(w) / (2 * q)
    c = np.cos(w)
    return _biquad(x, alpha, 0.0, -alpha, 1 + alpha, -2 * c, 1 - alpha)


def sweep(x, fc, q=1.0, mode="band"):
    """Filtro de estado variavel com a frequencia mudando no tempo (fc e um vetor)."""
    fc = np.clip(np.resize(fc, len(x)), 20, SR * 0.45)
    g = np.tan(np.pi * fc / SR).tolist()
    k = 1.0 / q
    out = [0.0] * len(x)
    ic1 = ic2 = 0.0
    for i, v0 in enumerate(x.tolist()):
        gi = g[i]
        a1 = 1.0 / (1.0 + gi * (gi + k))
        a2 = gi * a1
        a3 = gi * a2
        v3 = v0 - ic2
        v1 = a1 * ic1 + a2 * v3
        v2 = ic2 + a2 * ic1 + a3 * v3
        ic1 = 2 * v1 - ic1
        ic2 = 2 * v2 - ic2
        if mode == "band":
            out[i] = v1 * k
        elif mode == "low":
            out[i] = v2
        else:
            out[i] = v0 - k * v1 - v2
    return np.array(out)


# ----------------------------------------------------------------------
# Montagem
# ----------------------------------------------------------------------

def mix(length, *parts):
    """Soma pedacos (som, inicio em segundos, ganho) numa faixa de `length` segundos."""
    out = np.zeros(n_of(length))
    for sound, start, gain in parts:
        i = n_of(start) if start > 0 else 0
        if i >= len(out):
            continue
        piece = sound[: len(out) - i]
        out[i: i + len(piece)] += piece * gain
    return out


def finish(x, peak=0.89, fade_in=0.0015, fade_out=0.02, loop=False):
    """Tira o DC, suaviza as pontas (menos em loop) e normaliza o pico."""
    x = highpass(x, 25.0)
    if not loop:
        fi, fo = n_of(fade_in), n_of(fade_out)
        x[:fi] *= np.linspace(0, 1, fi)
        x[-fo:] *= np.linspace(1, 0, fo)
    top = np.max(np.abs(x))
    return x * (peak / top) if top > 0 else x


def write(name, x):
    os.makedirs(OUT, exist_ok=True)
    pcm = np.clip(np.round(x * 32767), -32768, 32767).astype("<i2")
    path = os.path.join(OUT, name + ".wav")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(pcm.tobytes())
    return path


# ----------------------------------------------------------------------
# Pecas que se repetem
# ----------------------------------------------------------------------

def thud(f_from, f_to, tau, seconds, attack=0.002):
    return osc(glide(f_from, f_to, seconds), seconds) * env(seconds, attack, tau)


def click(seconds, rng, fc=3000, tau=0.004, q=0.9):
    return bandpass(noise(seconds, rng), fc, q) * env(seconds, 0.0003, tau)


def grit(seconds, rng, density, fc=4000):
    """Estalinhos soltos: areia de ferrugem, faisca."""
    n = n_of(seconds)
    d = np.resize(density, n) if not np.isscalar(density) else np.full(n, density)
    hits = (rng.random(n) < d / SR) * rng.uniform(-1, 1, n)
    return highpass(hits, fc)


def clank(seconds, f0, rng, decay=0.06, bright=1.0):
    """Tinido curto de chapa de ferro."""
    ratios = [1.0, 1.52, 2.31, 2.95, 3.7, 4.6]
    decays = [decay, decay * 0.8, decay * 0.6, decay * 0.45, decay * 0.35, decay * 0.3]
    amps = [0.5, 0.42, 0.33 * bright, 0.25 * bright, 0.18 * bright, 0.12 * bright]
    return modes(seconds, f0, ratios, decays, amps, rng)


def whoosh(seconds, rng, f_from, f_to, q=1.2, peak=0.4):
    sig = sweep(noise(seconds, rng), glide(f_from, f_to, seconds), q)
    return sig * bell_env(seconds, peak)


# ----------------------------------------------------------------------
# O Scrap
# ----------------------------------------------------------------------

def scrap_step(seed):
    rng = np.random.default_rng(seed)
    d = 0.2
    body = thud(115 * rng.uniform(0.9, 1.1), 70, 0.035, d)
    tick = click(d, rng, fc=2400 * rng.uniform(0.8, 1.25), tau=0.01)
    metal = clank(d, rng.uniform(520, 700), rng, decay=0.05, bright=0.7)
    x = mix(d, (body, 0, 1.0), (tick, 0, 0.5), (metal, 0.002, 0.32))
    return finish(lowpass(x, 7000))


def scrap_jump(seed):
    rng = np.random.default_rng(seed)
    d = 0.3
    push = thud(150, 90, 0.03, d)
    hiss = highpass(noise(d, rng), 3000) * env(d, 0.001, 0.025)
    air = whoosh(0.2, rng, 450, 1700, q=1.4, peak=0.35)
    metal = clank(d, rng.uniform(850, 1000), rng, decay=0.04)
    return finish(mix(d, (push, 0, 0.8), (hiss, 0, 0.25), (air, 0.01, 0.7), (metal, 0, 0.25)))


def scrap_wall_jump():
    rng = np.random.default_rng(31)
    d = 0.32
    scrape = bandpass(noise(0.08, rng), 1900, 2.5) * env(0.08, 0.002, 0.03)
    scrape *= 0.6 + 0.4 * rng.random(len(scrape))
    kick = thud(170, 100, 0.025, d)
    metal = clank(d, 1100, rng, decay=0.05)
    air = whoosh(0.22, rng, 500, 1800, q=1.4, peak=0.35)
    return finish(mix(d, (scrape, 0, 0.7), (kick, 0.01, 0.7), (metal, 0.01, 0.35), (air, 0.03, 0.7)))


def scrap_wall_grab():
    rng = np.random.default_rng(32)
    d = 0.25
    scrape = bandpass(noise(0.11, rng), 2200, 2.0) * env(0.11, 0.003, 0.04)
    scrape *= 0.55 + 0.45 * rng.random(len(scrape))
    metal = clank(d, 1300, rng, decay=0.05)
    knock = thud(210, 140, 0.02, d)
    return finish(mix(d, (scrape, 0, 0.8), (metal, 0, 0.45), (knock, 0, 0.5)))


def scrap_pressure_jump():
    rng = np.random.default_rng(41)
    d = 0.55
    valve = clank(d, 1450, rng, decay=0.02) + click(d, rng, fc=5000, tau=0.003)
    steam = highpass(noise(d, rng), 1200) * env(d, 0.006, 0.12)
    steam += 0.6 * bandpass(noise(d, rng), 3500, 0.8) * env(d, 0.004, 0.08)
    puff = bandpass(noise(d, rng), 520, 1.0) * env(d, 0.002, 0.06)
    pump = thud(95, 55, 0.05, d)
    return finish(mix(d, (valve, 0, 0.35), (steam, 0.01, 0.75), (puff, 0.008, 0.6), (pump, 0.006, 0.8)))


def scrap_land(seed, heavy=False):
    rng = np.random.default_rng(seed)
    d = 0.6 if heavy else 0.35
    body = thud(70 if heavy else 85, 34 if heavy else 46, 0.16 if heavy else 0.09, d)
    dust = lowpass(noise(d, rng), 900 if heavy else 1100) * env(d, 0.001, 0.08 if heavy else 0.05)
    metal = clank(d, 230 if heavy else 320, rng, decay=0.22 if heavy else 0.11, bright=0.8)
    parts = [(body, 0, 1.0), (dust, 0, 0.7), (metal, 0.003, 0.45)]
    # As pecas do corpo chacoalham depois do baque.
    for _ in range(5 if heavy else 2):
        tink = clank(0.15, rng.uniform(1300, 2600), rng, decay=0.025)
        parts.append((tink, rng.uniform(0.03, 0.16 if heavy else 0.09), rng.uniform(0.08, 0.16)))
    if heavy:
        crunch = lowpass(grit(0.3, rng, 1800, fc=300), 3000) * env(0.3, 0.001, 0.09)
        parts.append((crunch, 0, 0.9))
    return finish(mix(d, *parts))


def scrap_swing(seed):
    rng = np.random.default_rng(seed)
    d = 0.26 * rng.uniform(0.92, 1.08)
    n = n_of(d)
    t = np.linspace(0, 1, n)
    centre = 380 + 900 * np.sin(np.pi * np.clip(t * 1.15, 0, 1)) ** 1.5 * rng.uniform(0.85, 1.15)
    air = sweep(noise(d, rng), centre, q=1.5)
    thick = lowpass(noise(d, rng), 380)
    shape = bell_env(d, peak=0.38, sharp=1.2)
    return finish((air + 0.55 * thick) * shape)


def scrap_hit(seed):
    rng = np.random.default_rng(seed)
    d = 0.55
    snap = highpass(noise(d, rng), 2000) * env(d, 0.0002, 0.004)
    punch = thud(180, 70, 0.05, d)
    ring = modes(d, rng.uniform(330, 430), [1, 1.48, 2.12, 2.63, 3.17, 3.88, 4.6],
                 [0.22, 0.18, 0.14, 0.11, 0.08, 0.06, 0.05], [0.6, 0.5, 0.45, 0.35, 0.3, 0.2, 0.15], rng)
    crunch = bandpass(noise(d, rng), 1300, 0.8) * env(d, 0.0005, 0.03)
    return finish(mix(d, (snap, 0, 0.6), (punch, 0, 0.9), (ring, 0, 0.55), (crunch, 0, 0.5)))


def scrap_charge_ready():
    rng = np.random.default_rng(51)
    d = 1.1
    rise = osc(glide(600, 1320, 0.14), 0.14) * bell_env(0.14, peak=0.85)
    vib = 1 + 0.003 * np.sin(2 * np.pi * 5.5 * times(0.95))
    ting = bell(1320, 0.95, decay=0.9, rng=rng, ratios=(1.0, 2.0, 2.76, 4.07, 5.4), amps=(1.0, 0.25, 0.4, 0.15, 0.1)) * vib
    shimmer = highpass(noise(d, rng), 7000) * env(d, 0.01, 0.1)
    return finish(mix(d, (rise, 0, 0.35), (ting, 0.12, 1.0), (shimmer, 0.12, 0.12)))


def scrap_resonance():
    rng = np.random.default_rng(52)
    d = 2.0
    boom = thud(75, 40, 0.35, d, attack=0.004)
    t = times(d)
    fork = np.zeros_like(t)
    for f, dec, a in ((220, 1.3, 0.5), (440, 1.1, 0.8), (443, 1.1, 0.5), (660, 0.7, 0.3), (880, 0.5, 0.25)):
        fork += a * np.exp(-t / dec) * np.sin(2 * np.pi * f * t)
    wave_ = sweep(noise(0.7, rng), glide(2600, 300, 0.7), q=1.2) * env(0.7, 0.008, 0.22)
    ring = bell(990, d, decay=0.8, rng=rng)
    return finish(mix(d, (boom, 0, 0.9), (fork, 0, 0.55), (wave_, 0, 0.8), (ring, 0, 0.3)))


def scrap_dash():
    rng = np.random.default_rng(61)
    d = 0.4
    jet = sweep(noise(d, rng), glide(3200, 850, d), q=0.9) * env(d, 0.008, 0.12)
    hiss = highpass(noise(d, rng), 4200) * env(d, 0.003, 0.06)
    low = bandpass(noise(d, rng), 280, 0.7) * bell_env(d, peak=0.3)
    start = clank(d, 1600, rng, decay=0.015)
    return finish(mix(d, (jet, 0, 1.0), (hiss, 0, 0.45), (low, 0, 0.6), (start, 0, 0.25)))


def zap(seconds, rng, base=120):
    """Curto-circuito: uma quadrada pulando de nota em nota, com estalos."""
    n = n_of(seconds)
    steps = np.repeat(rng.uniform(base * 0.7, base * 3.2, n // 350 + 1), 350)[:n]
    buzz = lowpass(osc(steps, seconds, "square"), 3200)
    crackle = grit(seconds, rng, 900, fc=3000)
    return (buzz * 0.7 + crackle) * env(seconds, 0.001, seconds * 0.35)


def scrap_hurt():
    rng = np.random.default_rng(71)
    d = 0.45
    clonk = modes(d, 240, [1, 1.6, 2.3, 3.1, 4.2], [0.15, 0.1, 0.08, 0.05, 0.04], [0.7, 0.5, 0.4, 0.25, 0.15], rng)
    punch = thud(140, 60, 0.04, d)
    sparks = zap(0.22, rng)
    return finish(mix(d, (clonk, 0, 0.8), (punch, 0, 0.8), (sparks, 0.01, 0.45)))


def scrap_death():
    rng = np.random.default_rng(72)
    d = 1.7
    crash = modes(d, 200, [1, 1.48, 2.12, 2.63, 3.17, 3.88], [0.4, 0.32, 0.25, 0.18, 0.12, 0.09], [0.6, 0.5, 0.45, 0.35, 0.3, 0.2], rng)
    crunch = lowpass(noise(d, rng), 2000) * env(d, 0.001, 0.12)
    dn = 1.05
    down = osc(glide(520, 48, dn), dn, "saw")
    down = sweep(down, glide(3000, 180, dn), q=0.9, mode="low") * env(dn, 0.02, 0.5, hold=0.25)
    parts = [(crash, 0, 0.75), (crunch, 0, 0.6), (down, 0.06, 0.5), (zap(0.3, rng, 90), 0.02, 0.3)]
    for i in range(5):
        parts.append((clank(0.2, rng.uniform(1800, 3200), rng, decay=0.03), rng.uniform(0.15, 0.8), 0.22 - i * 0.03))
    parts.append((thud(62, 38, 0.12, 0.5), 0.95, 0.8))
    return finish(mix(d, *parts))


def scrap_respawn():
    rng = np.random.default_rng(73)
    d = 1.4
    up_d = 0.6
    up = osc(glide(60, 330, up_d), up_d, "saw")
    up = sweep(up, glide(200, 2600, up_d), q=0.8, mode="low") * env(up_d, 0.06, 0.25, hold=0.4)
    hum = (osc(330, 0.6) + 0.6 * osc(495, 0.6)) * env(0.6, 0.02, 0.18)
    chime = bell(1320, 0.8, decay=0.5, rng=rng)
    sigh = lowpass(highpass(noise(0.55, rng), 500), 2500) * bell_env(0.55, peak=0.35)
    return finish(mix(d, (up, 0, 0.6), (hum, 0.55, 0.3), (chime, 0.56, 0.45), (sigh, 0.32, 0.22)))


def scrap_scrape_loop():
    """Lixa raspando a ferrugem, ida e volta. 2 s exatos, emenda sem pulo."""
    rng = np.random.default_rng(81)
    d = 2.0
    strokes = 6
    n = n_of(d)
    t = np.arange(n) / n
    phase = (t * strokes) % 1.0
    stroke = np.floor(t * strokes).astype(int)
    amp = 0.18 + 0.82 * np.sin(np.pi * phase) ** 0.8
    centre = np.where(stroke % 2 == 0, 2100, 1550) * (1 + 0.15 * np.sin(np.pi * phase))
    rasp = sweep(noise(d, rng), centre, q=1.3)
    rasp *= 0.65 + 0.35 * rng.random(n)
    sparkle = grit(d, rng, 500 + 2500 * amp, fc=4500)
    body = lowpass(noise(d, rng), 320)
    x = (rasp + 0.5 * sparkle + 0.35 * body) * amp
    return finish(lowpass(x, 9000), peak=0.8, loop=True)


def scrap_scrape_done():
    rng = np.random.default_rng(82)
    d = 1.1
    shing = highpass(noise(d, rng), 5000) * env(d, 0.004, 0.15)
    parts = [(shing, 0, 0.35)]
    for i, f in enumerate((1568, 2093, 2637, 3136)):
        parts.append((bell(f, 0.8, decay=0.4, rng=rng, ratios=(1.0, 2.76), amps=(1.0, 0.3)), 0.05 * i, 0.55 - 0.05 * i))
    warm = (osc(523.25, 0.8, "tri") + 0.6 * osc(784, 0.8, "tri")) * env(0.8, 0.01, 0.3)
    parts.append((warm, 0, 0.22))
    return finish(mix(d, *parts))


# ----------------------------------------------------------------------
# Coleta
# ----------------------------------------------------------------------

def pickup_shard():
    rng = np.random.default_rng(91)
    d = 0.4
    tink = modes(d, 2349, [1.0, 2.76, 5.4], [0.18, 0.08, 0.03], [1.0, 0.4, 0.2], rng)
    tick = highpass(noise(d, rng), 6000) * env(d, 0.0002, 0.0015)
    return finish(mix(d, (tink, 0, 1.0), (tick, 0, 0.3)))


def pickup_shards_recovered():
    rng = np.random.default_rng(92)
    d = 1.1
    notes = (1174.7, 1318.5, 1568.0, 1760.0, 1975.5, 2349.3, 2637.0, 3136.0)
    parts = []
    for i, f in enumerate(notes):
        tink = modes(0.5, f, [1.0, 2.76, 5.4], [0.2, 0.08, 0.03], [1.0, 0.35, 0.15], rng)
        parts.append((tink, 0.055 * i, 0.55 + 0.05 * i))
    parts.append((whoosh(0.45, rng, 800, 3200, q=1.0, peak=0.6), 0, 0.2))
    return finish(mix(d, *parts))


def pickup_gem():
    rng = np.random.default_rng(93)
    d = 1.7
    ratios = (1.0, 2.0, 3.0, 4.2)
    amps = (1.0, 0.35, 0.2, 0.1)
    a = bell(1318.5, 1.5, decay=1.1, rng=rng, ratios=ratios, amps=amps, detune=2.5)
    b = bell(1975.5, 1.5, decay=1.1, rng=rng, ratios=ratios, amps=amps, detune=2.5)
    shimmer = highpass(noise(d, rng), 8000) * env(d, 0.02, 0.4)
    return finish(mix(d, (a, 0, 0.8), (b, 0.12, 0.9), (shimmer, 0, 0.08)))


def mallet(freq, seconds, rng):
    return modes(seconds, freq, [1.0, 4.0, 9.2], [0.45, 0.12, 0.05], [1.0, 0.3, 0.1], rng)


def pickup_key_item():
    rng = np.random.default_rng(94)
    d = 1.3
    parts = [(mallet(f, 1.0, rng), 0.11 * i, 0.8) for i, f in enumerate((784.0, 1046.5, 1318.5))]
    pad = (osc(261.6, 1.0, "tri") + 0.7 * osc(392.0, 1.0, "tri")) * env(1.0, 0.1, 0.4, hold=0.15)
    parts.append((lowpass(pad, 1800), 0, 0.25))
    return finish(mix(d, *parts))


def pickup_chassis():
    rng = np.random.default_rng(95)
    d = 1.5
    anvil = clank(0.6, 450, rng, decay=0.3)
    parts = [(anvil, 0, 0.6), (thud(120, 70, 0.06, 0.3), 0, 0.5)]
    for i, f in enumerate((261.6, 329.6, 392.0, 523.25)):
        note = lowpass(osc(f, 0.14, "square"), 2500) * env(0.14, 0.003, 0.08)
        parts.append((note, 0.15 + 0.08 * i, 0.32))
    chord = (osc(523.25, 0.9, "tri") + osc(659.3, 0.9, "tri") + osc(784.0, 0.9, "tri")) * env(0.9, 0.01, 0.35)
    parts.append((chord, 0.47, 0.25))
    return finish(mix(d, *parts))


# ----------------------------------------------------------------------
# Interface
# ----------------------------------------------------------------------

def ui_ability_get():
    rng = np.random.default_rng(101)
    d = 3.2
    pad_d = 2.6
    pad = sum(osc(f, pad_d, "tri") for f in (130.8, 196.0, 261.6, 329.6))
    pad = lowpass(pad, 2000) * env(pad_d, 0.35, 0.7, hold=0.9)
    parts = [(pad, 0, 0.35), (thud(68, 40, 0.4, 1.0, attack=0.01), 0, 0.5)]
    for i, f in enumerate((523.25, 659.3, 784.0, 1046.5)):
        parts.append((bell(f, 2.0, decay=1.2, rng=rng, ratios=(1.0, 2.0, 3.0, 4.2), amps=(1.0, 0.3, 0.18, 0.08)), 0.05 + 0.1 * i, 0.5))
    parts.append((bell(2093, 2.4, decay=1.5, rng=rng), 0.5, 0.35))
    shimmer = highpass(noise(2.0, rng), 6000) * bell_env(2.0, peak=0.3)
    parts.append((shimmer, 0.2, 0.05))
    return finish(mix(d, *parts))


def ui_save():
    rng = np.random.default_rng(102)
    d = 1.9
    anvil = modes(1.2, 1760, [1.0, 1.42, 2.71, 3.9], [0.9, 0.6, 0.3, 0.2], [1.0, 0.5, 0.35, 0.2], rng)
    hum = (osc(220, 1.4, "tri") + 0.8 * osc(329.6, 1.4, "tri") + 0.5 * osc(440, 1.4, "tri")) * env(1.4, 0.08, 0.6)
    sigh = lowpass(highpass(noise(0.7, rng), 400), 2500) * bell_env(0.7, peak=0.3)
    return finish(mix(d, (anvil, 0, 0.45), (lowpass(hum, 2000), 0.05, 0.4), (sigh, 0.15, 0.18)))


def ui_gem_socket():
    rng = np.random.default_rng(103)
    d = 1.5
    c1 = clank(0.1, 2200, rng, decay=0.02) + click(0.1, rng, fc=4000, tau=0.002)
    c2 = clank(0.12, 1800, rng, decay=0.03) + click(0.12, rng, fc=3000, tau=0.003)
    parts = [(c1, 0, 0.4), (c2, 0.07, 0.6)]
    for i, f in enumerate((1318.5, 1760.0, 2637.0)):
        parts.append((bell(f, 1.1, decay=0.8, rng=rng, detune=2.0), 0.12 + 0.08 * i, 0.5))
    return finish(mix(d, *parts))


def blip(f_from, f_to, seconds, tau, glide_time=None):
    g = glide_time or seconds
    f = np.concatenate([glide(f_from, f_to, g), np.full(max(0, n_of(seconds) - n_of(g)), f_to)])
    tone = osc(f, seconds, "tri") + 0.15 * osc(f * 2, seconds)
    return tone * env(seconds, 0.003, tau)


def ui_dialogue_open():
    rng = np.random.default_rng(104)
    d = 0.18
    paper = bandpass(noise(d, rng), 3000, 0.7) * env(d, 0.001, 0.02)
    return finish(mix(d, (blip(620, 880, d, 0.05, 0.06), 0, 1.0), (paper, 0, 0.12)))


def ui_dialogue_next():
    return finish(blip(760, 820, 0.09, 0.03, 0.02))


def ui_dialogue_close():
    return finish(blip(760, 520, 0.15, 0.05, 0.07))


def ui_shop_move():
    rng = np.random.default_rng(105)
    d = 0.05
    return finish(osc(1800, d) * env(d, 0.0005, 0.008) + 0.3 * click(d, rng, fc=5000, tau=0.002))


def ui_shop_buy():
    rng = np.random.default_rng(106)
    d = 0.6
    coin_a = modes(0.4, 2637, [1.0, 2.4, 3.9], [0.2, 0.09, 0.04], [1.0, 0.4, 0.2], rng)
    coin_b = modes(0.4, 3520, [1.0, 2.4, 3.9], [0.22, 0.09, 0.04], [1.0, 0.4, 0.2], rng)
    drawer = thud(150, 90, 0.04, 0.2) + 0.5 * click(0.2, rng, fc=1500, tau=0.01)
    return finish(mix(d, (drawer, 0, 0.6), (coin_a, 0.0, 0.6), (coin_b, 0.06, 0.6)))


def ui_shop_deny():
    a = lowpass(osc(196, 0.09, "square"), 1200) * env(0.09, 0.003, 0.05, hold=0.03)
    b = lowpass(osc(165, 0.11, "square"), 1200) * env(0.11, 0.003, 0.06, hold=0.03)
    return finish(mix(0.26, (a, 0, 1.0), (b, 0.11, 1.0)))


def ui_notice():
    rng = np.random.default_rng(107)
    d = 0.75
    a = bell(880, 0.6, decay=0.35, rng=rng, ratios=(1.0, 2.0, 3.0), amps=(1.0, 0.2, 0.08))
    b = bell(1174.7, 0.6, decay=0.35, rng=rng, ratios=(1.0, 2.0, 3.0), amps=(1.0, 0.2, 0.08))
    return finish(mix(d, (a, 0, 0.8), (b, 0.09, 0.8)))


# ======================================================================
# Pacote 2: inimigos e mecanicas das fases
# ======================================================================

# ----------------------------------------------------------------------
# Pecas
# ----------------------------------------------------------------------

def hiss(seconds, rng, fc=2000, attack=0.005, tau=0.1, hold=0.0):
    return highpass(noise(seconds, rng), fc) * env(seconds, attack, tau, hold)


def bloop(f_from, f_to, seconds, tau=None):
    """Uma bolha: um tom curto que escorrega de nota."""
    return osc(glide(f_from, f_to, seconds), seconds) * env(seconds, 0.002, tau or seconds * 0.4)


def bubbles(seconds, rng, count, f_lo=300, f_hi=1200, rising=True, density_curve=None):
    """Varias bolhas soltas no tempo (fervura, gorgolejo)."""
    parts = []
    for i in range(count):
        if density_curve is not None:
            at = density_curve(rng.random()) * seconds * 0.92
        else:
            at = rng.uniform(0, seconds * 0.9)
        f = rng.uniform(f_lo, f_hi)
        d = rng.uniform(0.03, 0.08)
        parts.append((bloop(f, f * (1.8 if rising else 0.6), d), at, rng.uniform(0.3, 1.0)))
    return mix(seconds, *parts)


def splash(seconds, rng, fc=900, tau=0.08):
    wet = bandpass(noise(seconds, rng), fc, 0.7) * env(seconds, 0.002, tau)
    wet *= 0.5 + 0.5 * rng.random(n_of(seconds))
    low = lowpass(noise(seconds, rng), 400) * env(seconds, 0.001, tau * 0.6)
    return wet + 0.7 * low + 0.4 * bubbles(seconds, rng, 6, 400, 1400)


def glass_shatter(seconds, rng, f_lo=2200, f_hi=6500, shards=14, spread=0.25, decay=0.06):
    """Vidro ou cristal estourando: um estalo e uma chuva de cacos tinindo."""
    parts = [(hiss(seconds, rng, 3000, 0.0005, 0.03), 0, 0.8), (click(seconds, rng, fc=4500, tau=0.004), 0, 0.8)]
    for i in range(shards):
        f = rng.uniform(f_lo, f_hi)
        ping = modes(0.3, f, [1.0, 1.73, 2.6], [decay, decay * 0.6, decay * 0.4], [1.0, 0.5, 0.3], rng)
        at = spread * (rng.random() ** 1.6)
        parts.append((ping, at, rng.uniform(0.25, 0.7) * (1 - 0.5 * at / max(spread, 1e-3))))
    return mix(seconds, *parts)


def flame(seconds, rng):
    """Chama de macarico: um rugido grave tremulando e estalos."""
    n = n_of(seconds)
    flicker = lowpass(rng.uniform(0, 1, n), 18)
    flicker = 0.6 + 0.8 * (flicker - flicker.min()) / max(1e-6, np.ptp(flicker))
    roar = lowpass(noise(seconds, rng), 900) + 0.6 * bandpass(noise(seconds, rng), 320, 0.8)
    return roar * flicker + 0.35 * grit(seconds, rng, 120, fc=2500)


def grind(seconds, rng, fc=700, q=1.5, rattle=0.0):
    """Ferro ou pedra arrastando."""
    n = n_of(seconds)
    jitter = lowpass(rng.uniform(0, 1, n), 30)
    jitter = 0.4 + 0.6 * (jitter - jitter.min()) / max(1e-6, np.ptp(jitter))
    body = bandpass(noise(seconds, rng), fc, q) * jitter + 0.5 * lowpass(noise(seconds, rng), 160)
    if rattle:
        body += rattle * grit(seconds, rng, 40, fc=1200)
    return body


def squeak(f_from, f_to, seconds, vibrato=7.0, depth=0.02):
    """Metal (ou borracha) rangendo: um tom com harmonicas e tremido."""
    t = times(seconds)
    f = glide(f_from, f_to, seconds) * (1 + depth * np.sin(2 * np.pi * vibrato * t))
    tone = osc(f, seconds, "saw")
    return bandpass(tone, (f_from + f_to), 2.0) * bell_env(seconds, peak=0.3)


def thump(seconds, f_from=110, f_to=60, tau=0.05):
    return thud(f_from, f_to, tau, seconds)


# ----------------------------------------------------------------------
# Inimigos
# ----------------------------------------------------------------------

def enemy_block():
    rng = np.random.default_rng(201)
    d = 0.45
    ring = modes(d, 1250, [1, 1.47, 2.09, 2.56, 3.2], [0.12, 0.1, 0.08, 0.06, 0.05], [0.6, 0.5, 0.4, 0.3, 0.2], rng)
    sparks = grit(d, rng, 1800, fc=5000) * env(d, 0.001, 0.06)
    return finish(mix(d, (ring, 0, 0.8), (sparks, 0, 0.5), (click(d, rng, fc=6000, tau=0.002), 0, 0.6)))


def death_metal():
    rng = np.random.default_rng(202)
    d = 1.15
    parts = [(clank(0.5, 300, rng, decay=0.15), 0, 0.7), (thump(0.4, 120, 70, 0.06), 0, 0.7)]
    for i in range(7):
        at = 0.05 + 0.65 * rng.random()
        parts.append((clank(0.25, rng.uniform(500, 1800), rng, decay=rng.uniform(0.03, 0.07)), at, 0.45 - at * 0.35))
    parts.append((thump(0.3, 95, 60, 0.05), 0.32, 0.6))
    parts.append((thump(0.3, 85, 55, 0.05), 0.62, 0.45))
    parts.append((lowpass(noise(0.5, rng), 1200) * env(0.5, 0.01, 0.12), 0.3, 0.25))
    return finish(mix(d, *parts))


def death_steam():
    rng = np.random.default_rng(203)
    d = 1.3
    ping = clank(0.4, 1500, rng, decay=0.08)
    steam = hiss(1.2, rng, 1500, 0.01, 0.4) + 0.5 * bandpass(noise(1.2, rng), 3500, 0.9) * env(1.2, 0.01, 0.3)
    parts = [(ping, 0, 0.6), (steam, 0.02, 0.6), (clank(0.4, 350, rng, decay=0.12), 0.05, 0.5),
             (thump(0.4, 100, 55, 0.07), 0.5, 0.8), (clank(0.3, 900, rng, decay=0.05), 0.52, 0.3)]
    return finish(mix(d, *parts))


def death_crystal():
    rng = np.random.default_rng(204)
    d = 0.9
    shatter = glass_shatter(d, rng, 2000, 6000, shards=16, spread=0.25)
    tunk = thump(0.3, 220, 140, 0.04)
    return finish(mix(d, (shatter, 0, 1.0), (tunk, 0, 0.5)))


def death_glass():
    rng = np.random.default_rng(205)
    d = 1.2
    shatter = glass_shatter(d, rng, 2600, 7500, shards=22, spread=0.35, decay=0.08)
    flame_out = whoosh(0.6, rng, 400, 1600, q=1.0, peak=0.4)
    return finish(mix(d, (shatter, 0, 1.0), (flame_out, 0.08, 0.5), (thump(0.3, 180, 110, 0.04), 0, 0.4)))


def death_deflate():
    rng = np.random.default_rng(206)
    d = 1.0
    poke = splash(0.25, rng, 700, 0.05)
    n = n_of(0.85)
    flap = 0.55 + 0.45 * np.sign(np.sin(2 * np.pi * np.linspace(28, 14, n) * times(0.85)))
    air = sweep(noise(0.85, rng), glide(1300, 280, 0.85), q=2.0) * flap * env(0.85, 0.02, 0.4, hold=0.2)
    return finish(mix(d, (poke, 0, 0.6), (air, 0.05, 0.8)))


def death_goo():
    rng = np.random.default_rng(207)
    d = 0.65
    splat = splash(0.5, rng, 800, 0.09)
    ping = modes(0.4, 2400, [1.0, 2.3], [0.08, 0.04], [1.0, 0.4], rng)
    return finish(mix(d, (splat, 0, 1.0), (thump(0.3, 140, 80, 0.04), 0, 0.5), (ping, 0.01, 0.15),
                      (bloop(500, 220, 0.12), 0.18, 0.4)))


def crawler_windup():
    rng = np.random.default_rng(211)
    d = 0.5
    clack = lambda: clank(0.12, 1800, rng, decay=0.02) + click(0.12, rng, fc=3500, tau=0.003)
    creak = squeak(300, 420, 0.32)
    return finish(mix(d, (clack(), 0, 0.8), (clack(), 0.18, 0.8), (creak, 0.05, 0.35)))


def crawler_bite():
    rng = np.random.default_rng(212)
    d = 0.3
    snap = clank(0.2, 1400, rng, decay=0.03) + click(0.2, rng, fc=3000, tau=0.004)
    return finish(mix(d, (whoosh(0.14, rng, 600, 1500, q=1.4, peak=0.5), 0, 0.7), (snap, 0.08, 0.9)))


def bloater_windup():
    rng = np.random.default_rng(213)
    d = 0.5
    return finish(mix(d, (squeak(500, 700, 0.42), 0, 0.8), (hiss(0.3, rng, 3000, 0.02, 0.08), 0.25, 0.25)))


def bloater_spray():
    rng = np.random.default_rng(214)
    d = 0.75
    n = n_of(d)
    gurgle = 0.6 + 0.4 * np.sin(2 * np.pi * 15 * times(d)) * rng.uniform(0.7, 1.0, n)
    spray = (highpass(noise(d, rng), 1500) + 0.6 * bandpass(noise(d, rng), 3000, 1.2)) * gurgle
    spray *= env(d, 0.03, 0.15, hold=0.4)
    sizzle = grit(d, rng, 900, fc=4000) * env(d, 0.05, 0.2, hold=0.3)
    return finish(mix(d, (spray, 0, 1.0), (sizzle, 0, 0.4)))


def bloater_swell():
    rng = np.random.default_rng(215)
    d = 0.6
    whine = squeak(300, 900, 0.58, vibrato=11, depth=0.03) * np.linspace(0.4, 1.0, n_of(0.58))
    crackle = grit(d, rng, np.linspace(50, 900, n_of(d)), fc=3500)
    return finish(mix(d, (whine, 0, 0.8), (crackle, 0, 0.5)))


def bloater_burst():
    rng = np.random.default_rng(216)
    d = 1.1
    glass = glass_shatter(0.8, rng, 1800, 5000, shards=12, spread=0.15)
    wet = splash(0.7, rng, 700, 0.12)
    sizzle = hiss(1.0, rng, 3500, 0.05, 0.35) * (0.6 + 0.4 * rng.random(n_of(1.0)))
    return finish(mix(d, (glass, 0, 0.7), (wet, 0, 1.0), (thump(0.4, 90, 50, 0.08), 0, 0.8), (sizzle, 0.1, 0.35)))


def welder_windup():
    rng = np.random.default_rng(217)
    d = 0.5
    rise = np.linspace(0.1, 1.0, n_of(d)) ** 1.5
    torch = flame(d, rng) * rise
    gas = hiss(d, rng, 2500, 0.3, 0.1) * rise
    return finish(mix(d, (torch, 0, 1.0), (gas, 0, 0.4), (click(d, rng, fc=2000, tau=0.01), 0, 0.4)))


def welder_flame():
    rng = np.random.default_rng(218)
    d = 0.65
    roar = flame(d, rng) * env(d, 0.02, 0.1, hold=0.42)
    blast = thump(0.3, 90, 50, 0.08)
    return finish(mix(d, (roar, 0, 1.0), (blast, 0, 0.6), (hiss(d, rng, 4000, 0.02, 0.1, hold=0.4), 0, 0.25)))


def leech_drop():
    rng = np.random.default_rng(219)
    d = 0.4
    return finish(mix(d, (bloop(900, 300, 0.08), 0, 0.9), (splash(0.1, rng, 1200, 0.02), 0, 0.45), (whoosh(0.28, rng, 1200, 500, q=1.2, peak=0.5), 0.05, 0.5)))


def leech_land():
    rng = np.random.default_rng(220)
    d = 0.3
    slap = lowpass(noise(d, rng), 1500) * env(d, 0.001, 0.03)
    return finish(mix(d, (slap, 0, 0.8), (thump(d, 130, 70, 0.04), 0, 0.8), (splash(0.25, rng, 900, 0.05), 0, 0.5)))


def leech_windup():
    return finish(squeak(220, 480, 0.36, vibrato=5, depth=0.04))


def leech_bite():
    rng = np.random.default_rng(221)
    d = 0.25
    snap = splash(0.15, rng, 1500, 0.02) + 0.6 * click(0.15, rng, fc=2500, tau=0.005)
    return finish(mix(d, (whoosh(0.1, rng, 700, 1400, q=1.4, peak=0.6), 0, 0.6), (snap, 0.06, 1.0)))


def sentry_charge():
    rng = np.random.default_rng(222)
    d = 0.6
    parts = []
    at = 0.0
    gap = 0.11
    for i in range(8):
        parts.append((clank(0.06, 2000 + 120 * i, rng, decay=0.012) + click(0.06, rng, fc=4000, tau=0.002), at, 0.5))
        at += gap
        gap *= 0.82
    whistle = osc(glide(800, 2200, 0.55), 0.55) * np.linspace(0, 1, n_of(0.55)) ** 2
    parts.append((whistle, 0, 0.18))
    return finish(mix(d, *parts))


def sentry_fire():
    rng = np.random.default_rng(223)
    d = 0.45
    pfft = hiss(d, rng, 1000, 0.002, 0.08) + 0.6 * bandpass(noise(d, rng), 600, 1.0) * env(d, 0.002, 0.05)
    return finish(mix(d, (pfft, 0, 1.0), (thump(0.3, 110, 60, 0.05), 0, 0.7), (clank(0.3, 700, rng, decay=0.06), 0, 0.3)))


def mite_wake():
    rng = np.random.default_rng(224)
    d = 0.38
    parts = []
    for i in range(8):
        parts.append((click(0.05, rng, fc=rng.uniform(1800, 3200), tau=0.004), rng.uniform(0, 0.25), rng.uniform(0.4, 0.9)))
    for i in range(4):
        blip_ = lowpass(osc(rng.uniform(2000, 3000), 0.03, "square"), 5000) * env(0.03, 0.001, 0.01)
        parts.append((blip_, 0.15 + 0.04 * i, 0.25))
    return finish(mix(d, *parts))


def hunter_alert():
    rng = np.random.default_rng(225)
    d = 0.85
    horn = (osc(523.25, 0.75) + 0.8 * osc(554.4, 0.75) + 0.3 * osc(1046.5, 0.75)) * env(0.75, 0.05, 0.25, hold=0.15)
    breath = bandpass(noise(0.75, rng), 1500, 1.5) * env(0.75, 0.05, 0.2, hold=0.1)
    grunt = lowpass(osc(glide(120, 95, 0.18), 0.18, "saw"), 600) * env(0.18, 0.01, 0.07)
    return finish(mix(d, (grunt, 0, 0.6), (horn, 0.04, 0.7), (breath, 0.04, 0.25)))


def sentinel_open():
    rng = np.random.default_rng(226)
    d = 0.5
    lid = grind(0.35, rng, 500, 1.2) * bell_env(0.35, peak=0.4)
    t = times(0.45)
    hum = osc(glide(600, 900, 0.45), 0.45) * (0.7 + 0.3 * np.sin(2 * np.pi * 14 * t)) * np.linspace(0.1, 1.0, n_of(0.45))
    return finish(mix(d, (lid, 0, 0.8), (hum, 0.05, 0.35)))


def sentinel_fire():
    rng = np.random.default_rng(227)
    d = 0.45
    parts = []
    for i in range(3):
        zing = osc(glide(3200 - 200 * i, 1800, 0.16), 0.16) * env(0.16, 0.002, 0.06)
        parts.append((zing, 0.02 * i, 0.6))
        parts.append((hiss(0.1, rng, 6000, 0.001, 0.02), 0.02 * i, 0.3))
    parts.append((thump(0.2, 200, 120, 0.03), 0, 0.4))
    return finish(mix(d, *parts))


def experiment_charge():
    rng = np.random.default_rng(228)
    d = 0.42
    t = times(0.38)
    buzz = zap(0.38, rng, 150) * (0.6 + 0.4 * np.sin(2 * np.pi * 30 * t))
    whine = osc(glide(400, 1200, 0.38), 0.38, "tri") * np.linspace(0.2, 1.0, n_of(0.38))
    return finish(mix(d, (buzz, 0, 0.8), (whine, 0, 0.3)))


def experiment_lunge():
    rng = np.random.default_rng(229)
    d = 0.45
    screech = lowpass(osc(glide(950, 600, 0.3), 0.3, "saw"), 2500) * env(0.3, 0.01, 0.12)
    return finish(mix(d, (whoosh(0.4, rng, 500, 1800, q=1.3, peak=0.3), 0, 0.8), (screech, 0, 0.35)))


def thrower_windup():
    rng = np.random.default_rng(230)
    d = 0.35
    parts = [(modes(0.15, rng.uniform(3000, 4200), [1.0, 1.7], [0.05, 0.03], [1.0, 0.4], rng), at, 0.6)
             for at in (0.0, 0.08, 0.15)]
    return finish(mix(d, *parts))


def thrower_throw():
    rng = np.random.default_rng(231)
    d = 0.32
    t = times(0.3)
    spin = whoosh(0.3, rng, 700, 1600, q=1.3, peak=0.3) * (0.6 + 0.4 * np.sin(2 * np.pi * 20 * t))
    return finish(spin)


def flask_break():
    rng = np.random.default_rng(232)
    d = 0.9
    glass = glass_shatter(0.7, rng, 2400, 6000, shards=14, spread=0.12)
    wet = splash(0.5, rng, 900, 0.08)
    sizzle = hiss(0.8, rng, 3500, 0.05, 0.25)
    return finish(mix(d, (glass, 0, 0.9), (wet, 0, 0.7), (sizzle, 0.06, 0.3)))


def slime_split():
    rng = np.random.default_rng(233)
    d = 0.65
    big = splash(0.5, rng, 600, 0.1)
    return finish(mix(d, (big, 0, 1.0), (thump(0.3, 120, 70, 0.05), 0, 0.6),
                      (bloop(400, 700, 0.1), 0.12, 0.5), (bloop(450, 800, 0.1), 0.2, 0.5)))


def transmute():
    rng = np.random.default_rng(234)
    d = 0.75
    ring = modes(0.7, 1700, [1.0, 1.53, 2.42, 3.1, 4.4], [0.5, 0.35, 0.25, 0.18, 0.1], [1.0, 0.6, 0.4, 0.3, 0.2], rng)
    shimmer = sweep(noise(0.4, rng), glide(2000, 7000, 0.4), q=2.0) * bell_env(0.4, peak=0.3)
    return finish(mix(d, (ring, 0, 0.8), (shimmer, 0, 0.4)))


# ----------------------------------------------------------------------
# Mecanicas
# ----------------------------------------------------------------------

def gate_open():
    rng = np.random.default_rng(301)
    d = 1.55
    move = grind(1.2, rng, 700, 1.5, rattle=0.6) * env(1.2, 0.08, 0.15, hold=0.95)
    chain = grit(1.2, rng, 18, fc=1500) * 1.5
    start = clank(0.5, 260, rng, decay=0.15) + thump(0.5, 100, 60, 0.06)
    stop = clank(0.5, 300, rng, decay=0.12) + thump(0.5, 90, 55, 0.06)
    return finish(mix(d, (start, 0, 0.7), (move, 0.03, 0.8), (chain, 0.03, 0.3), (stop, 1.18, 0.6)))


def valve_turn():
    rng = np.random.default_rng(302)
    d = 1.4
    t = times(0.8)
    wheel = squeak(600, 520, 0.8, vibrato=4, depth=0.06)
    ratchet = grit(0.8, rng, 30, fc=2000) * 2
    steam = hiss(0.8, rng, 1200, 0.03, 0.35)
    return finish(mix(d, (wheel, 0, 0.6), (ratchet, 0, 0.3), (clank(0.4, 800, rng, decay=0.08), 0.75, 0.5),
                      (steam, 0.5, 0.7)))


def acid_drain():
    rng = np.random.default_rng(303)
    d = 1.8
    suction = lowpass(noise(1.6, rng), 600) * bell_env(1.6, peak=0.6)
    glug = bubbles(1.6, rng, 26, 180, 700, rising=False)
    slurp = sweep(noise(0.35, rng), glide(400, 1600, 0.35), q=2.5) * bell_env(0.35, peak=0.7)
    return finish(mix(d, (suction, 0, 0.6), (glug, 0, 0.9), (slurp, 1.35, 0.6)))


def acid_sizzle():
    rng = np.random.default_rng(304)
    d = 0.55
    sizzle = hiss(d, rng, 2500, 0.01, 0.2) * (0.5 + 0.5 * rng.random(n_of(d)))
    crackle = grit(d, rng, 1500, fc=3000) * env(d, 0.01, 0.2)
    return finish(mix(d, (sizzle, 0, 0.9), (crackle, 0, 0.5)))


def acid_alarm():
    d = 1.7
    parts = []
    for i in range(2):
        f = np.concatenate([glide(300, 430, 0.45), np.full(n_of(0.15), 430.0)])
        horn = osc(f, 0.6, "square") + 0.6 * osc(f * 1.5, 0.6, "square")
        horn = bandpass(lowpass(horn, 2600), 900, 0.6) * env(0.6, 0.02, 0.06, hold=0.5)
        parts.append((horn, i * 0.8, 1.0))
    return finish(mix(d, *parts))


def boiler_pressure():
    rng = np.random.default_rng(306)
    d = 0.6
    hull = modes(0.6, 260, [1.0, 1.5, 2.2, 2.9, 3.8], [0.3, 0.22, 0.16, 0.1, 0.07], [0.7, 0.5, 0.35, 0.25, 0.15], rng)
    gauge = squeak(600, 820, 0.22) * 0.6
    return finish(mix(d, (hull, 0, 1.0), (gauge, 0.08, 0.3), (hiss(0.2, rng, 3000, 0.002, 0.04), 0, 0.3)))


def boiler_fuse():
    rng = np.random.default_rng(307)
    d = 1.45
    n = n_of(d)
    rise = np.linspace(0.15, 1.0, n) ** 1.3
    whistle = (osc(glide(900, 2800, d), d) + 0.3 * osc(glide(1800, 5600, d), d)) * rise
    steam = hiss(d, rng, 1500, 0.05, 1.0, hold=1.3) * rise
    rattle = grit(d, rng, np.linspace(20, 200, n), fc=900)
    return finish(mix(d, (whistle, 0, 0.5), (steam, 0, 0.6), (rattle, 0, 0.6)))


def boiler_explode():
    rng = np.random.default_rng(308)
    d = 2.2
    boom = thud(70, 28, 0.5, d, attack=0.003)
    blast = lowpass(noise(d, rng), 1600) * env(d, 0.002, 0.3)
    crack = hiss(0.3, rng, 2000, 0.0005, 0.03)
    parts = [(boom, 0, 1.0), (blast, 0, 0.9), (crack, 0, 0.6)]
    for i in range(8):
        parts.append((clank(0.3, rng.uniform(600, 2500), rng, decay=0.05), rng.uniform(0.1, 1.2), 0.2))
    parts.append((hiss(1.5, rng, 1200, 0.05, 0.6), 0.2, 0.3))
    return finish(mix(d, *parts))


def steam_refresh():
    rng = np.random.default_rng(309)
    d = 0.42
    puff = bandpass(noise(0.35, rng), 2500, 0.8) * bell_env(0.35, peak=0.25)
    chime = bell(1760, 0.35, decay=0.2, rng=rng, ratios=(1.0, 2.0), amps=(1.0, 0.2))
    return finish(mix(d, (puff, 0, 1.0), (chime, 0.04, 0.25)))


def crystal_crack():
    rng = np.random.default_rng(310)
    d = 0.42
    parts = [(click(0.05, rng, fc=rng.uniform(3000, 5000), tau=0.002), rng.uniform(0, 0.33), 0.5) for _ in range(10)]
    parts.append((squeak(3000, 2600, 0.35, vibrato=13, depth=0.01), 0, 0.25))
    parts.append((click(0.1, rng, fc=2500, tau=0.006), 0.32, 1.0))
    return finish(mix(d, *parts))


def crystal_break():
    rng = np.random.default_rng(311)
    d = 1.0
    shatter = glass_shatter(0.9, rng, 1800, 5500, shards=18, spread=0.3)
    tumble = mix(0.8, *[(thump(0.2, rng.uniform(150, 250), 100, 0.03), rng.uniform(0.05, 0.6), 0.4) for _ in range(5)])
    return finish(mix(d, (shatter, 0, 1.0), (tumble, 0, 0.6)))


def barrier_shatter():
    rng = np.random.default_rng(312)
    d = 2.0
    shatter = glass_shatter(1.5, rng, 1500, 7000, shards=40, spread=0.5, decay=0.09)
    boom = thud(80, 40, 0.3, 1.0)
    ring = bell(660, 1.8, decay=1.0, rng=rng)
    tumble = mix(1.2, *[(thump(0.2, rng.uniform(120, 220), 80, 0.04), rng.uniform(0.1, 1.0), 0.35) for _ in range(8)])
    return finish(mix(d, (shatter, 0, 1.0), (boom, 0, 0.8), (ring, 0, 0.35), (tumble, 0, 0.5)))


def crystal_note():
    """A nota de um cristal ressonante (C5). O jogo muda o tom pra cada cristal."""
    rng = np.random.default_rng(313)
    d = 1.7
    t = times(1.6)
    vib = 1 + 0.002 * np.sin(2 * np.pi * 4.5 * t)
    tone = bell(523.25, 1.6, decay=1.4, rng=rng, ratios=(1.0, 2.0, 2.76, 4.07, 5.4), amps=(1.0, 0.35, 0.35, 0.12, 0.08), detune=1.2) * vib
    strike = click(0.05, rng, fc=4000, tau=0.002)
    return finish(mix(d, (tone, 0, 1.0), (strike, 0, 0.2)))


def crystal_dull():
    rng = np.random.default_rng(314)
    d = 0.25
    return finish(mix(d, (thump(d, 230, 160, 0.04), 0, 1.0), (lowpass(click(d, rng, fc=900, tau=0.01), 2000), 0, 0.6)))


def statue_press():
    rng = np.random.default_rng(315)
    d = 1.9
    clunk = thump(0.4, 95, 60, 0.06) + click(0.4, rng, fc=1500, tau=0.01)
    gong = modes(1.8, 180, [1.0, 1.47, 2.0, 2.53, 3.0, 3.6, 4.4], [1.2, 0.9, 0.8, 0.6, 0.45, 0.35, 0.25],
                 [0.7, 0.5, 0.4, 0.35, 0.25, 0.18, 0.12], rng, detune=0.8)
    return finish(mix(d, (clunk, 0, 0.9), (gong, 0.01, 0.8)))


def puzzle_right():
    rng = np.random.default_rng(316)
    d = 0.85
    a = bell(784, 0.7, decay=0.5, rng=rng, ratios=(1.0, 2.0, 3.0), amps=(1.0, 0.25, 0.1))
    b = bell(987.8, 0.7, decay=0.5, rng=rng, ratios=(1.0, 2.0, 3.0), amps=(1.0, 0.25, 0.1))
    return finish(mix(d, (a, 0, 0.8), (b, 0.1, 0.8)))


def puzzle_wrong():
    d = 0.6
    tones = (osc(146.8, 0.5, "tri") + osc(155.6, 0.5, "tri")) * env(0.5, 0.01, 0.18, hold=0.08)
    return finish(mix(d, (lowpass(tones, 1200), 0, 1.0), (thump(0.3, 90, 60, 0.05), 0, 0.6)))


def puzzle_solved():
    rng = np.random.default_rng(318)
    d = 2.0
    parts = []
    for i, f in enumerate((523.25, 659.3, 784.0, 1046.5)):
        parts.append((bell(f, 1.6, decay=1.0, rng=rng, ratios=(1.0, 2.0, 3.0, 4.2), amps=(1.0, 0.3, 0.15, 0.08)), 0.09 * i, 0.6))
    pad = (osc(261.6, 1.6, "tri") + osc(392, 1.6, "tri") + 0.6 * osc(523.25, 1.6, "tri")) * env(1.6, 0.15, 0.5, hold=0.4)
    parts.append((lowpass(pad, 1800), 0, 0.3))
    return finish(mix(d, *parts))


def lock_rattle():
    rng = np.random.default_rng(319)
    d = 0.45
    parts = [(clank(0.12, rng.uniform(850, 1150), rng, decay=0.03), at, 0.7) for at in (0, 0.07, 0.15, 0.24)]
    parts.append((thump(0.2, 140, 90, 0.03), 0, 0.5))
    return finish(mix(d, *parts))


def key_unlock():
    rng = np.random.default_rng(320)
    d = 0.9
    insert = bandpass(noise(0.12, rng), 3000, 2.0) * env(0.12, 0.005, 0.05)
    parts = [(insert, 0, 0.6)]
    for i, at in enumerate((0.15, 0.22, 0.29)):
        parts.append((clank(0.08, 1800 - 150 * i, rng, decay=0.015) + click(0.08, rng, fc=3500, tau=0.002), at, 0.5))
    parts.append((squeak(400, 330, 0.25), 0.15, 0.3))
    parts.append((thump(0.4, 110, 60, 0.06) + clank(0.4, 500, rng, decay=0.1), 0.42, 0.9))
    return finish(mix(d, *parts))


def mercury_drip():
    rng = np.random.default_rng(321)
    d = 0.3
    plop = bloop(700, 1500, 0.06, tau=0.03)
    ping = modes(0.25, 2500, [1.0, 2.2], [0.05, 0.03], [1.0, 0.3], rng)
    return finish(mix(d, (plop, 0, 1.0), (ping, 0.01, 0.35)))


def mercury_splat():
    rng = np.random.default_rng(322)
    d = 0.45
    wet = lowpass(splash(0.35, rng, 700, 0.06), 3000)
    shimmer = modes(0.4, 2100, [1.0, 1.9, 2.7], [0.12, 0.08, 0.05], [1.0, 0.5, 0.3], rng)
    return finish(mix(d, (wet, 0, 1.0), (bloop(600, 250, 0.1), 0, 0.5), (shimmer, 0.01, 0.25)))


def mercury_step(seed):
    rng = np.random.default_rng(seed)
    d = 0.26
    body = thud(110, 70, 0.03, d)
    slosh = bandpass(noise(d, rng), 900 * rng.uniform(0.85, 1.15), 0.9) * env(d, 0.005, 0.05)
    slosh *= 0.6 + 0.4 * rng.random(n_of(d))
    ping = modes(d, rng.uniform(1900, 2500), [1.0, 2.3], [0.04, 0.02], [1.0, 0.3], rng)
    return finish(mix(d, (body, 0, 0.7), (slosh, 0, 0.8), (ping, 0.005, 0.2), (bloop(500, 900, 0.05), 0.03, 0.25)))


def mirror_turn():
    rng = np.random.default_rng(323)
    d = 0.65
    pivot = thump(0.3, 160, 100, 0.03) + clank(0.3, 700, rng, decay=0.05)
    scrape = bandpass(noise(0.2, rng), 1800, 2.0) * bell_env(0.2, peak=0.3)
    glint = bell(2637, 0.5, decay=0.3, rng=rng, ratios=(1.0, 2.76), amps=(1.0, 0.3))
    return finish(mix(d, (scrape, 0, 0.5), (pivot, 0.12, 0.8), (glint, 0.15, 0.35)))


def beam_hum():
    """Zumbido do raio de luz. 2 s, emenda sem pulo (frequencias multiplas de 0,5 Hz)."""
    rng = np.random.default_rng(324)
    d = 2.0
    t = np.arange(n_of(d)) / SR
    tone = (np.sin(2 * np.pi * 120 * t) + 0.6 * np.sin(2 * np.pi * 240.5 * t)
            + 0.35 * np.sin(2 * np.pi * 360 * t) + 0.15 * np.sin(2 * np.pi * 1200.5 * t))
    crackle = bandpass(noise(d, rng), 3000, 1.0) * 0.12 + grit(d, rng, 60, fc=4000) * 0.5
    return finish(tone * 0.8 + crackle, peak=0.7, loop=True)


def crystal_lock_open():
    rng = np.random.default_rng(325)
    d = 1.5
    rise = sweep(noise(0.6, rng), glide(800, 5000, 0.6), q=3.0) * np.linspace(0.1, 1.0, n_of(0.6))
    parts = [(rise, 0, 0.4)]
    for i, f in enumerate((1318.5, 1661.2, 1975.5)):
        parts.append((bell(f, 1.0, decay=0.8, rng=rng, detune=1.5), 0.55 + 0.04 * i, 0.5))
    parts.append((thump(0.4, 120, 70, 0.05) + clank(0.4, 600, rng, decay=0.08), 0.6, 0.6))
    return finish(mix(d, *parts))


def alembic_insert():
    rng = np.random.default_rng(326)
    d = 0.9
    parts = [(modes(0.3, f, [1.0, 1.7, 2.9], [0.12, 0.06, 0.03], [1.0, 0.4, 0.2], rng), at, 0.7)
             for f, at in ((2900, 0.0), (3500, 0.07))]
    parts.append((bubbles(0.7, rng, 7, 300, 900), 0.12, 0.6))
    return finish(mix(d, *parts))


def alembic_boil():
    rng = np.random.default_rng(327)
    d = 2.6
    boil = bubbles(2.4, rng, 90, 200, 1100, density_curve=lambda u: u ** 0.5)
    swell = np.linspace(0.2, 1.0, n_of(2.4))
    rumble = lowpass(noise(2.4, rng), 220) * swell
    steam = hiss(2.4, rng, 2500, 1.2, 0.5, hold=0.8)
    return finish(mix(d, (boil * swell, 0, 0.9), (rumble, 0, 0.6), (steam, 0, 0.35)))


def circle_activate():
    rng = np.random.default_rng(328)
    d = 2.1
    shimmer = sweep(noise(1.1, rng), glide(400, 4500, 1.1), q=2.0) * bell_env(1.1, peak=0.7)
    parts = [(shimmer, 0, 0.55), (thud(70, 40, 0.4, 1.0), 0.9, 0.6)]
    for i, f in enumerate((659.3, 987.8, 1318.5)):
        parts.append((bell(f, 1.2, decay=0.9, rng=rng, ratios=(1.0, 2.0, 3.0), amps=(1.0, 0.3, 0.12), detune=1.5), 0.9 + 0.05 * i, 0.5))
    return finish(mix(d, *parts))


def elevator_start():
    rng = np.random.default_rng(329)
    d = 1.0
    clunk = thump(0.5, 90, 50, 0.08) + clank(0.5, 280, rng, decay=0.15)
    motor = lowpass(osc(glide(35, 90, 0.7), 0.7, "saw"), 600) * np.linspace(0.1, 1.0, n_of(0.7))
    chain = grit(0.7, rng, 25, fc=1500) * 2
    return finish(mix(d, (clunk, 0, 1.0), (motor, 0.15, 0.5), (chain, 0.15, 0.3)))


def elevator_loop():
    """O motor do elevador subindo. 2 s, emenda sem pulo."""
    rng = np.random.default_rng(330)
    d = 2.0
    n = n_of(d)
    t = np.arange(n) / SR
    motor = lowpass(osc(90.0, d, "saw"), 500)
    rumble = lowpass(noise(d, rng), 140)
    chain = np.zeros(n)
    for k in range(12):
        at = int(k * n / 12)
        tick = clank(0.08, 1100 + 60 * (k % 3), rng, decay=0.015)
        chain[at: at + len(tick)] += tick[: n - at]
    return finish(motor * 0.7 + rumble * 0.6 + chain * 0.35 + 0.05 * np.sin(2 * np.pi * 45 * t), peak=0.75, loop=True)


def purify_wave():
    rng = np.random.default_rng(331)
    d = 6.2
    swell_d = 1.5
    sparkle = mix(swell_d, *[(bell(rng.uniform(1500, 4000), 0.6, decay=0.4, rng=rng, ratios=(1.0, 2.76), amps=(1.0, 0.25)),
                              swell_d * rng.random() ** 0.6, rng.uniform(0.1, 0.35)) for _ in range(18)])
    pad = sum(osc(f, swell_d + 0.6, "tri") for f in (261.6, 329.6, 392.0))
    pad = lowpass(pad, 1600) * np.linspace(0, 1, n_of(swell_d + 0.6)) ** 2 * env(swell_d + 0.6, 0.0, 0.3, hold=swell_d)
    wave_d = 4.6
    centre = 300 + 2400 * np.sin(np.pi * np.linspace(0, 1, n_of(wave_d)) ** 0.7)
    wave_ = sweep(noise(wave_d, rng), centre, q=0.9) * bell_env(wave_d, peak=0.2, sharp=0.8)
    dust = grit(wave_d, rng, np.linspace(1500, 50, n_of(wave_d)), fc=6000)
    boom = thud(60, 35, 0.6, 1.5, attack=0.01)
    return finish(mix(d, (sparkle, 0, 0.6), (pad, 0, 0.25), (wave_, swell_d, 0.9), (dust, swell_d, 0.25), (boom, swell_d, 0.6)))


def oxide_bump():
    rng = np.random.default_rng(332)
    d = 0.38
    fizz = bandpass(noise(0.3, rng), 1500, 0.8) * bell_env(0.3, peak=0.2)
    fizz += 0.5 * grit(0.3, rng, 600, fc=3500) * env(0.3, 0.01, 0.1)
    return finish(mix(d, (fizz, 0, 1.0), (thump(0.25, 100, 70, 0.04), 0, 0.4)))


# ----------------------------------------------------------------------
# A lista
# ----------------------------------------------------------------------

SOUNDS = {
    # O Scrap
    "Scrap_Step_1": lambda: scrap_step(1),
    "Scrap_Step_2": lambda: scrap_step(2),
    "Scrap_Step_3": lambda: scrap_step(3),
    "Scrap_Step_4": lambda: scrap_step(4),
    "Scrap_Jump_1": lambda: scrap_jump(11),
    "Scrap_Jump_2": lambda: scrap_jump(12),
    "Scrap_WallJump": scrap_wall_jump,
    "Scrap_WallGrab": scrap_wall_grab,
    "Scrap_PressureJump": scrap_pressure_jump,
    "Scrap_Land_1": lambda: scrap_land(21),
    "Scrap_Land_2": lambda: scrap_land(22),
    "Scrap_LandHeavy": lambda: scrap_land(23, heavy=True),
    "Scrap_Swing_1": lambda: scrap_swing(41),
    "Scrap_Swing_2": lambda: scrap_swing(42),
    "Scrap_Swing_3": lambda: scrap_swing(43),
    "Scrap_Hit_1": lambda: scrap_hit(44),
    "Scrap_Hit_2": lambda: scrap_hit(45),
    "Scrap_Hit_3": lambda: scrap_hit(46),
    "Scrap_ChargeReady": scrap_charge_ready,
    "Scrap_Resonance": scrap_resonance,
    "Scrap_Dash": scrap_dash,
    "Scrap_Hurt": scrap_hurt,
    "Scrap_Death": scrap_death,
    "Scrap_Respawn": scrap_respawn,
    "Scrap_ScrapeLoop": scrap_scrape_loop,
    "Scrap_ScrapeDone": scrap_scrape_done,
    # Coleta
    "Pickup_Shard": pickup_shard,
    "Pickup_ShardsRecovered": pickup_shards_recovered,
    "Pickup_Gem": pickup_gem,
    "Pickup_KeyItem": pickup_key_item,
    "Pickup_Chassis": pickup_chassis,
    # Interface
    "Ui_AbilityGet": ui_ability_get,
    "Ui_Save": ui_save,
    "Ui_GemSocket": ui_gem_socket,
    "Ui_DialogueOpen": ui_dialogue_open,
    "Ui_DialogueNext": ui_dialogue_next,
    "Ui_DialogueClose": ui_dialogue_close,
    "Ui_ShopMove": ui_shop_move,
    "Ui_ShopBuy": ui_shop_buy,
    "Ui_ShopDeny": ui_shop_deny,
    "Ui_Notice": ui_notice,
    # Inimigos
    "Enemy_Block": enemy_block,
    "Enemy_DeathMetal": death_metal,
    "Enemy_DeathSteam": death_steam,
    "Enemy_DeathCrystal": death_crystal,
    "Enemy_DeathGlass": death_glass,
    "Enemy_DeathDeflate": death_deflate,
    "Enemy_DeathGoo": death_goo,
    "Crawler_Windup": crawler_windup,
    "Crawler_Bite": crawler_bite,
    "Bloater_Windup": bloater_windup,
    "Bloater_Spray": bloater_spray,
    "Bloater_Swell": bloater_swell,
    "Bloater_Burst": bloater_burst,
    "Welder_Windup": welder_windup,
    "Welder_Flame": welder_flame,
    "Leech_Drop": leech_drop,
    "Leech_Land": leech_land,
    "Leech_Windup": leech_windup,
    "Leech_Bite": leech_bite,
    "Sentry_Charge": sentry_charge,
    "Sentry_Fire": sentry_fire,
    "Mite_Wake": mite_wake,
    "Hunter_Alert": hunter_alert,
    "Sentinel_Open": sentinel_open,
    "Sentinel_Fire": sentinel_fire,
    "Experiment_Charge": experiment_charge,
    "Experiment_Lunge": experiment_lunge,
    "Thrower_Windup": thrower_windup,
    "Thrower_Throw": thrower_throw,
    "Flask_Break": flask_break,
    "Slime_Split": slime_split,
    "Glass_Transmute": transmute,
    # Mecanicas
    "Gate_Open": gate_open,
    "Valve_Turn": valve_turn,
    "Acid_Drain": acid_drain,
    "Acid_Sizzle": acid_sizzle,
    "Acid_Alarm": acid_alarm,
    "Boiler_Pressure": boiler_pressure,
    "Boiler_Fuse": boiler_fuse,
    "Boiler_Explode": boiler_explode,
    "Steam_Refresh": steam_refresh,
    "Crystal_Crack": crystal_crack,
    "Crystal_Break": crystal_break,
    "Crystal_BarrierShatter": barrier_shatter,
    "Crystal_Note": crystal_note,
    "Crystal_Dull": crystal_dull,
    "Statue_Press": statue_press,
    "Puzzle_Right": puzzle_right,
    "Puzzle_Wrong": puzzle_wrong,
    "Puzzle_Solved": puzzle_solved,
    "Lock_Rattle": lock_rattle,
    "Lock_Unlock": key_unlock,
    "Mercury_Drip": mercury_drip,
    "Mercury_Splat": mercury_splat,
    "Mercury_Step_1": lambda: mercury_step(341),
    "Mercury_Step_2": lambda: mercury_step(342),
    "Mirror_Turn": mirror_turn,
    "Beam_Hum": beam_hum,
    "CrystalLock_Open": crystal_lock_open,
    "Alembic_Insert": alembic_insert,
    "Alembic_Boil": alembic_boil,
    "Circle_Activate": circle_activate,
    "Elevator_Start": elevator_start,
    "Elevator_Loop": elevator_loop,
    "Purify_Wave": purify_wave,
    "Oxide_Bump": oxide_bump,
}


def main():
    prefix = sys.argv[1] if len(sys.argv) > 1 else ""
    for name, make in SOUNDS.items():
        if not name.startswith(prefix):
            continue
        x = make()
        write(name, x)
        rms = 20 * math.log10(max(1e-9, float(np.sqrt(np.mean(x ** 2)))))
        print(f"{name:26s} {len(x) / SR:5.2f}s  rms {rms:6.1f} dB")


if __name__ == "__main__":
    main()
