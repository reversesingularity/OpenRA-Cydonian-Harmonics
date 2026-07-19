# Governance — Cydonian Harmonics

## Purpose

This document defines how technical, narrative, and release decisions are made
for the Cydonian Harmonics OpenRA total conversion.

## Roles

| Role | Authority |
|---|---|
| **Project Lead** | Final say on roadmap, releases, grant submissions, and hard canon conflicts |
| **Canon Steward** | Owns lore hard lines (Binitarian frame, Azazel identity, Acoustic Paradigm, Tobit-only Raphael) |
| **Engine Steward** | Owns C# traits, MiniYaml composition, Loop Engineering gates |
| **Release Steward** | Owns packaging tags, installer verification, artifact attestation |
| **Contributors** | Propose PRs; may not merge lore-breaking or unverified builds |

AI agents operate under [`AGENTS.md`](AGENTS.md) and may not bypass Loop Engineering
or canon hard lines.

## Decision process

1. **Plans first** for complex OpenRA logic — draft `.cursor/plans/<feature>.md`,
   await human approval, then implement.
2. **Canon conflicts** escalate to Canon Steward; gameplay never invents
   Trinitarian language, Watcher-Azazel identity, or combat-unit Raphael.
3. **Merge gate:** `make` / `.\make.cmd all`, `--check-yaml`, and `make test`
   must be observed green before claiming completion.
4. **Release gate:** packaging scripts must produce verifiable installer files
   on disk; paths and sizes recorded in [`CHANGELOG.md`](CHANGELOG.md) /
   [`docs/SESSION.md`](docs/SESSION.md).

## Canon ownership (non-negotiable)

1. Binitarian theology only (Father + Son).
2. Azazel is Nephilim (son of Gadreel), not a Watcher.
3. Acoustic Paradigm only — no mana / magic / psionics vocabulary.
4. Raphael / Liaigh = Tobit Protocol support power only.

Authoritative sources: `MASTER_LORE_BOOK.md`, `.claude/docs/lore-bible.md`.

## Release process

1. Pre-flight: rebuild + YAML lint on a clean tree.
2. Tag form: `release-YYYYMMDD` or `playtest-YYYYMMDD`.
3. Package via `bash packaging/package-all.sh <tag> packaging/output`
   (Linux/WSL for Windows NSIS + Linux AppImage).
4. Verify physical artifacts and sizes before grant upload.
5. Do not commit `packaging/output/` binaries to git.

## Conflict resolution

If technical convenience conflicts with canon, **canon wins**.
If packaging convenience conflicts with reproducibility, **reproducibility wins**.
