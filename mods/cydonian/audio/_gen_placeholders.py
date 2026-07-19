"""One-shot generator for Phase I acoustic placeholder WAVs. Not part of runtime."""
import math
import struct
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parent


def write_tone(path: Path, *, duration: float, freq: float, rate: int = 22050, volume: float = 0.35, kind: str = "sine"):
	path.parent.mkdir(parents=True, exist_ok=True)
	n = int(duration * rate)
	with wave.open(str(path), "w") as w:
		w.setnchannels(1)
		w.setsampwidth(2)
		w.setframerate(rate)
		frames = bytearray()
		for i in range(n):
			t = i / rate
			env = 1.0
			attack = min(0.02, duration * 0.15)
			release = min(0.05, duration * 0.25)
			if kind != "click":
				if t < attack:
					env = t / attack
				elif t > duration - release:
					env = max(0.0, (duration - t) / release)

			if kind == "clash":
				s = 0.55 * math.sin(2 * math.pi * freq * t) + 0.45 * math.sin(2 * math.pi * (freq * 1.07) * t)
			elif kind == "drone":
				s = 0.7 * math.sin(2 * math.pi * freq * t) + 0.3 * math.sin(2 * math.pi * (freq * 0.5) * t)
			elif kind == "shatter":
				s = (
					0.4 * math.sin(2 * math.pi * freq * t)
					+ 0.3 * math.sin(2 * math.pi * freq * 2.3 * t)
					+ 0.2 * math.sin(2 * math.pi * freq * 5.1 * t + t * 40)
					+ 0.1 * math.sin(2 * math.pi * freq * 9.7 * t)
				)
			elif kind == "click":
				s = math.sin(2 * math.pi * freq * t) * math.exp(-t * 40)
				env = 1.0
			else:
				s = math.sin(2 * math.pi * freq * t)

			sample = int(max(-1.0, min(1.0, s * env * volume)) * 32767)
			frames += struct.pack("<h", sample)
		w.writeframes(frames)
	print(f"wrote {path.relative_to(ROOT)} ({duration:.2f}s @ {rate}Hz)")


SPECS = [
	("weapons/shatter-clash.wav", 0.55, 880.0, 22050, 0.40, "clash"),
	("weapons/shatter-drone.wav", 1.00, 110.0, 22050, 0.45, "drone"),
	("weapons/dermal-mark-tone.wav", 0.45, 180.0, 22050, 0.30, "drone"),
	("weapons/sustain-harmony-strike.wav", 0.60, 432.0, 22050, 0.35, "sine"),
	("weapons/shatter-onfire.wav", 0.70, 140.0, 22050, 0.40, "drone"),
	("ui/chat-tone.wav", 0.20, 720.0, 22050, 0.28, "sine"),
	("ui/click.wav", 0.08, 1400.0, 22050, 0.35, "click"),
	("ui/click-disabled.wav", 0.10, 220.0, 22050, 0.30, "click"),
	("voices/cian/select.wav", 0.70, 196.0, 22050, 0.32, "sine"),
	("voices/cian/action.wav", 0.65, 220.0, 22050, 0.32, "sine"),
	("voices/cian/die.wav", 0.90, 130.0, 22050, 0.28, "drone"),
	("voices/brennan/select.wav", 0.65, 330.0, 22050, 0.30, "sine"),
	("voices/brennan/action.wav", 0.60, 349.0, 22050, 0.30, "sine"),
	("voices/brennan/die.wav", 0.85, 165.0, 22050, 0.28, "drone"),
	("voices/guardians/select.wav", 0.55, 392.0, 22050, 0.30, "sine"),
	("voices/guardians/action.wav", 0.50, 440.0, 22050, 0.30, "sine"),
	("voices/guardians/die.wav", 0.80, 247.0, 22050, 0.28, "drone"),
	("voices/guardians/base-attack.wav", 0.90, 523.0, 22050, 0.34, "clash"),
	("voices/guardians/insufficient-resonance.wav", 0.85, 175.0, 22050, 0.32, "drone"),
	("voices/guardians/select-target.wav", 0.70, 494.0, 22050, 0.30, "sine"),
	("voices/nephilim/select.wav", 0.55, 155.0, 22050, 0.32, "drone"),
	("voices/nephilim/action.wav", 0.50, 185.0, 22050, 0.32, "clash"),
	("voices/nephilim/die.wav", 0.85, 98.0, 22050, 0.30, "drone"),
	("voices/tobit/tobit-engage.wav", 1.20, 528.0, 22050, 0.38, "sine"),
]


if __name__ == "__main__":
	for rel, dur, freq, rate, vol, kind in SPECS:
		write_tone(ROOT / rel, duration=dur, freq=freq, rate=rate, volume=vol, kind=kind)
	print("done", len(SPECS), "files")
