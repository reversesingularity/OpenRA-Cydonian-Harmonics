# Security Policy

## Supported versions

Development builds on `main` and tagged `release-*` / `playtest-*` packages are
in scope for security reports related to this mod's custom code and packaging
scripts. The vendored OpenRA engine is upstream software — report engine-core
issues to [OpenRA/OpenRA](https://github.com/OpenRA/OpenRA/security).

## Reporting a vulnerability

Please **do not** open a public issue for exploitable vulnerabilities.

Email or privately message the Project Lead / repository owner with:

- Affected commit or release tag
- Reproduction steps
- Impact assessment (crash, RCE, save corruption, network desync abuse, etc.)

We will acknowledge receipt within a reasonable window and coordinate a fix
before public disclosure.

## Scope notes

- Custom assemblies: `OpenRA.Mods.Cydonian/`
- Campaign Lua under `mods/cydonian/maps/`
- Packaging scripts under `packaging/`
- Out of scope: third-party engines, OS packages, and grant-platform accounts
