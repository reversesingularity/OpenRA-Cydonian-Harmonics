/**
 * Loop Engineering grind (TypeScript entry) — delegates to grind.sh.
 * Use when a Bun/Node runner is preferred; the shell script is the source of truth.
 *
 *   bun .cursor/hooks/grind.ts
 *   npx tsx .cursor/hooks/grind.ts
 */
import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const script = join(here, "grind.sh");
const maxRounds = process.env.GRIND_MAX_ROUNDS ?? "3";

const result = spawnSync("bash", [script], {
  stdio: "inherit",
  env: { ...process.env, GRIND_MAX_ROUNDS: maxRounds },
  shell: false,
});

process.exit(result.status ?? 1);
