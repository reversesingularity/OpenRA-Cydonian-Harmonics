# Contributing to Cydonian Harmonics

Thank you for helping build this OpenRA total conversion. This repository is
**not** a generic Mod SDK bug tracker — engine issues belong upstream at
[OpenRA/OpenRA](https://github.com/OpenRA/OpenRA).

## Before you write code

1. Read [`AGENTS.md`](AGENTS.md) and the [canon hard lines](README.md#canon-hard-lines).
2. For non-trivial traits or campaign logic, open or update a plan under
   `.cursor/plans/` and wait for approval.
3. Prefer conditions over cross-trait hard references.

## Loop Engineering

Never claim a green build without observing output:

```powershell
.\make.cmd all
.\utility.cmd cydonian --check-yaml
.\make.cmd test
```

```bash
make
./utility.sh cydonian --check-yaml
make test
```

## Style

| Area | Rule |
|---|---|
| C# | `TraitInfo` + runtime pair; no allocations in `ITick`; balance in YAML |
| MiniYaml | Match existing indent; `^Templates`; `@` labels for duplicate traits |
| Lua | Map-relative scripts; campaign-only; use `OperationalSilence` / `EmpyrealMedia` helpers |
| Lore copy | Acoustic vocabulary only; Raphael never a combat VoiceSet |

## Pull requests

- Keep PRs focused; avoid drive-by refactors.
- Include verify commands and observed results in the PR body.
- Lore-facing text should pass Binitarian / Acoustic / Azazel checks.
- Do not commit `engine/`, `packaging/output/`, `.env`, or PDF binaries.

## Platform notes

- Windows installers are produced from Linux/WSL (`makensis` + `wine64`).
- macOS `.dmg` requires a macOS host.
- Shell scripts should remain POSIX-friendly (`shellcheck` encouraged).

## Community

Participation is governed by [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
Security reports: [`SECURITY.md`](SECURITY.md).
