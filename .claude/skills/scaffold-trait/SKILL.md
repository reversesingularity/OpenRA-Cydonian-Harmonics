---
name: scaffold-trait
description: Generates boilerplate OpenRA C# TraitInfo + runtime trait pair for OpenRA.Mods.Cydonian, plus the MiniYaml registration stub. Manual playbook — invoke as /scaffold-trait <TraitName> [interfaces].
disable-model-invocation: true
---

# Scaffold Trait

Generate a canonical OpenRA trait pair in `src/Traits/<TraitName>.cs`.

## Inputs

- `$1` — trait name in PascalCase (e.g. `AcousticResonance`).
- `$2+` — optional interfaces for the runtime class (`ITick`, `INotifyCreated`,
  `INotifyOwnerCreated`, `INotifyDamage`...). Default: `INotifyCreated`.

## Template

```csharp
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
    [Desc("TODO: one-line description of what this trait does.")]
    public class {Name}Info : TraitInfo
    {
        [Desc("TODO: describe this tunable.")]
        public readonly int Range = 0;

        public override object Create(ActorInitializer init)
        {
            return new {Name}(init.Self, this);
        }
    }

    public class {Name} : {Interfaces}
    {
        readonly {Name}Info info;

        public {Name}(Actor self, {Name}Info info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            // Resolve and cache cross-trait dependencies HERE, never in Tick.
        }
    }
}
```

## Rules baked into the scaffold

- Config on `*Info` only: `public readonly` + `[Desc]`, bound from MiniYaml.
- If `ITick` was requested, emit an empty `Tick` with the comment
  `// HOT PATH: no allocations, no trait lookups, integer math only.`
- Also emit the YAML registration stub to paste into `mods/cydonian/rules/`:

  ```
  <actor>:
    {Name}:
      Range: 0
  ```

## Mandatory verification

After writing the file run `make` (or `dotnet build src`) and show the output.
The scaffold is not complete until it compiles clean.
