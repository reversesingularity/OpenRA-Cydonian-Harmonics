---
name: lint-rules
description: Manually triggers the OpenRA MiniYaml validation utility over the cydonian mod rules. Use before commits, after bulk YAML edits, or when the PostToolUse hook was bypassed.
---

# Lint Rules

Run the OpenRA YAML validator over the full mod and act on the results.

## Procedure

1. Run the linter from the repo root:

   ```
   ./utility.sh cydonian --check-yaml
   ```

2. If the engine checkout is not vendored yet, fall back to the surface lint:

   ```
   bash .claude/hooks/verify-post-tool.sh <<< '{"tool_input":{"file_path":"<file>"}}'
   ```

3. **Read every line of output.** For each error: open the file at the reported
   line, fix it per `.claude/rules/rules-yaml.md` (2-space indent, no tabs,
   `^Template` inheritance, `@` labels for duplicate traits), then re-run.
4. Repeat until the utility exits 0. Zero warnings is the bar, not zero errors.

## Done Criteria

Paste the final clean run's output. "It should pass now" is not a result;
the observed exit-0 run is.
