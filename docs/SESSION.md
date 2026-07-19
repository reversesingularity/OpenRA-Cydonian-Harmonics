# Session handoff — 2026-07-19

## Snapshot

| Item | Value |
|---|---|
| Branch | `main` → `origin/main` |
| GitHub | https://github.com/reversesingularity/OpenRA-Cydonian-Harmonics |
| Phase J commit | `15cf594` — Lua campaign architecture and Operational Silence |
| Docs commit | `81cec05` — README, governance, session, key art |
| Release tag used for packaging | `release-20260719` |
| Pre-flight | `.\make.cmd all` + `--check-yaml` green before packaging |
| Packaging host | WSL Ubuntu (Git Bash lacked `makensis` / `wine64` / GNU `make`) |
| Packaging exit | `0` — `Package build done.` |

## What shipped this session

1. **Phase J implementation** — World `LuaScript`, Operational Silence bus, Tobit /
   radar gates, `EmpyrealSpeechGate`, Eden dilation, Dudael `Actor.Create` maps.
2. **Phase J merge** — committed to `main` as `15cf594`.
3. **Final release packaging** — Windows NSIS + portable zips + Linux AppImage under
   `packaging/output/` (gitignored binaries; sizes in `CHANGELOG.md`).
4. **Governance + README refresh** — comprehensive README with diagrams/images,
   `GOVERNANCE.md`, `SECURITY.md`, updated `CONTRIBUTING.md`, `CHANGELOG.md`.

## Known notes

- wine32 multiarch warnings appeared during rcedit; packaging still succeeded.
- macOS `.dmg` not produced (requires macOS host).
- Unrelated local dirty files may remain (`CydonianHarmonics.sln` Mods.Common
  removal, `.vscode/`, hook lockfiles) — do not ship without review.

## Next recommended actions

1. Attach `packaging/output/CydonianHarmonics-release-20260719-x64.exe` (and
   AppImage if needed) to the CODE KickStart grant packet.
2. Optional: produce macOS `.dmg` on a macOS builder.
3. Optional: install `wine32:i386` in WSL to silence rcedit warnings.
4. Optional: add GitHub Actions CI for `make` + `--check-yaml`.

## Canon reminder

Binitarian only. Azazel = Nephilim. Acoustic Paradigm only. Raphael = Tobit Protocol only.
