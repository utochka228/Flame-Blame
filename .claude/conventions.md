# Code Conventions

## How to read this file

Sections marked **project-specific** override common C# style — read closely. Generic sections are reference; skim as needed.

**Project-specific (override defaults):**
- [C# Version](#c-version) — C# 9.0 ceiling
- [Naming](#naming-non-defaults-only) — constants are PascalCase; private fields are `_camelCase`; enums start at a zero member; no whitespace in asset/file names or `CreateAssetMenu` `fileName`; UI types end in `View`/`Window`/`Screen`/`Popup`
- [Numeric Units](#numeric-units) — fractions at runtime; percent only at the inspector and the UI
- [Class Member Organization](#class-member-organization) — Unity Messages at position #5
- [Collection Return Types](#collection-return-types) — return `IReadOnlyList<T>` without copying
- [Prefer Named Models over Value Tuples](#prefer-named-models-over-value-tuples) — tuples only inside a single private method body

**Mixed (contains project-specific items inline):**
- [Code Style](#code-style) — Allman braces, 160-char line limit, comment policy

**Generic C# (skim only):**
- [Error Handling Conventions](#error-handling-conventions)

Async/UniTask rules live in [patterns.md](patterns.md); Unity API rules live in [unity.md](unity.md).

---

## Naming (non-defaults only)

Standard C#/.NET naming applies, with these deviations:

- **Constants and `static readonly` fields are PascalCase, never `UPPER_CASE`** — including
  cached hash ids (`IsChargingBool`, `BurstTrigger`).
- **Private and protected instance fields are `_camelCase`** — including `[SerializeField]` ones
  (`_panelRect`, `_ownsTimeScale`). Locals and parameters take no prefix, and `const` /
  `static readonly` stay PascalCase per the rule above.
- **Enums start with a zero member** — `None = 0` where "nothing" is meaningful, otherwise a
  `Default = 0`. `[Flags]` enums use bit shifts.
- **Events are PascalCase** (`AmountChanged`, `Finished`).
- **No whitespace in asset or file names** — ScriptableObjects, scripts, sprites, textures, prefabs,
  materials, animations, audio, etc. use `PascalCase` or `Prefix_PascalCase` (`HoseConfig`,
  `Hose_Thick`), never `Hose Config`. **Sprites and textures use `snake_case`**
  (`sprite_character_portrait`, `button_primary`). Each asset type keeps **one** naming format: look
  at the sibling assets in the target folder and match them before creating a new one.
- **`[CreateAssetMenu]`: `fileName` has no whitespace, `menuName` may.** `fileName` becomes the
  default asset name; `menuName` is only the label in the create dialog.
  `[CreateAssetMenu(fileName = "HoseConfig", menuName = "FlameBlame/Hose Config")]`
- **UI types end in `View` / `Window` / `Screen` / `Popup`, never `…Behavior` or `…UI`** — pick
  the one that fits the role:
    - `Screen` — a full-screen state the player navigates to (`TitleScreen`, `LevelCompleteScreen`)
    - `Window` — a large modal or panel opened over a screen (`PauseWindow`)
    - `Popup` — a small transient dialog: a confirmation, an alert, an info bubble (`InfoPopup`)
    - `View` — a reusable piece rendered inside any of the above (`InteractionPromptView`)

  `Behavior` is reserved for gameplay MonoBehaviours. A `…UI` suffix says nothing about the role — use one of the four instead.

---

## Numeric Units

**The fraction (0-1) is the only runtime unit.** Percent (0-100) is an authoring and display unit:
it may appear on a serialized field and in a formatted string, and nowhere else. Convert exactly
once — in the property that reads the serialized field, or in the formatter that prints it. No
runtime method signature takes a percent, and no value is scaled up only to be scaled back down.

**Never write `/ 100f` or `* 100f`.** Use `ToFraction()` / `ToPercent()` from
`FlameBlame.Core.Extensions.PercentExtensions`, which owns the only `100f` literal in the project.

### Suffixes

| Suffix | Range | Allowed on |
|--------|-------|------------|
| `...Percent` | 0-100 | serialized fields and UI strings only |
| `...Fraction` | 0-1 | anywhere |
| `...Multiplier` | 1 = no change | anywhere; a bare `1f` identity is fine inline |
| `...Chance` | 0-1 | anywhere |

The suffix and the `[Range]` attribute must agree: `[Range(0f, 100f)]` on a `...Percent` field,
`[Range(0f, 1f)]` on a `...Fraction` field. A `...Percent` name on a 0-1 value is a bug.

```csharp
// Good - percent authored, converted once at the boundary, fraction from there on
[SerializeField, Range(0, 100)] private int _thumbBoostPercent = 40;
public int ThumbBoostPercent => _thumbBoostPercent;

nozzle.ApplyThumbBoost(ThumbBoostPercent.ToFraction());
public void ApplyThumbBoost(float boostFraction) { ... }

// Avoid - percent in a runtime signature forces every caller to know the unit,
// and a fraction caller ends up scaling up just so the callee can scale back down
public void ApplyThumbBoost(float boostPercent) => _tipPressure *= 1f + boostPercent / 100f;
```

Renaming a **serialized** field to fix its suffix requires `[FormerlySerializedAs("oldName")]`,
or every prefab and asset silently resets to the default.

---

## C# Version

**C# 9.0 only** - Do not use features from C# 10+:
- ❌ File-scoped namespaces (`namespace X;`)
- ❌ Global usings
- ❌ Raw string literals (`"""`)
- ❌ Collection expressions (`[]`)
- ❌ Required members
- ❌ Primary constructors for classes

---

## Structure

- Namespaces match folders (`FlameBlame.Hose.Simulation`).
- `Simulation` code is pure C#: no `UnityEngine.Object` dependencies, so it is unit-testable without a scene.

---

## Class Member Organization

Follow this order in all classes:

1. **Fields** (constants and `static readonly` first, then instance fields grouped by access level)
2. **Properties**
3. **Events/Delegates**
4. **Constructor**
5. **Unity Message Methods** (Awake, Start, Update, etc.)
6. **Public methods**
7. **Internal methods**
8. **Protected methods**
9. **Private methods**

**Unity lifecycle methods on a `MonoBehaviour`** (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnEnable`, `OnDisable`, `OnDestroy`) **may instead go first among the methods, right after Events/Delegates** — i.e. before Constructor/Public/Internal/Protected methods, rather than at position #5. Either placement is acceptable; be consistent within a single file rather than mixing both.

**No `#region` blocks** — never use regions in production classes. If a class needs regions to be navigable, it is too large; split it instead.

**Within each block, group by cohesion** — methods that work on the same concept stay together (e.g., `Init` next to `Dispose`, `Connect` next to `Disconnect`). Do not sort alphabetically.

### Example

```csharp
public class ExampleManager : IExampleManager
{
    // 1. Fields
    private static readonly int ReadyHash = "Ready".GetHashCode();

    private readonly ExampleSettings _settings;
    private int _value;

    // 2. Properties
    public int CurrentValue => _value;

    // 3. Events
    public event Action<int> ValueChanged;

    // 4. Constructor
    public ExampleManager(ExampleSettings settings)
    {
        _settings = settings;
    }

    // 6. Public methods
    public void UpdateValue(int newValue)
    {
        _value = newValue;
        ValueChanged?.Invoke(newValue);
    }

    // 9. Private methods
    private bool IsWithinRange(int value) => value <= _settings.MaxValue;
}
```

---

## Code Style

### General Rules

- Follow **Rider/JetBrains C# style** (Ctrl+E, C to reformat)
- All variables must have **descriptive names** - no 1-symbol names/abbreviations
- **No comments by default** — no XML doc summaries, no inline "what"/"why" notes. Express intent through naming and structure. Comment ONLY when logic is genuinely non-obvious and the code itself cannot be made clear (a subtle invariant, a workaround and its reason, a deliberately empty override). If a comment just restates what the code does, delete it and fix the code. The structural comments in this file's examples are teaching aids, not a style to copy.
- Use `nameof()` instead of string literals for symbol names (arguments in exceptions, storage keys, event names)
- Default to least-accessible modifier: `private` for members, `public` only for interfaces and explicit API surface
- **Allman style braces** — opening and closing braces on their own lines; 4 spaces for indentation, not tabs
- Single-statement `if`/`while` bodies may omit braces — place the statement on its own indented line: `if (cond)\n    DoSomething();`
- Keep lines under **160 characters**; break at logical points and maintain indentation. Generated files are exempt — never reformat them.

### Collection Return Types

- Return `IReadOnlyList<T>` or `IReadOnlyCollection<T>` from public methods — never expose `List<T>` directly
- `List<T>` implements `IReadOnlyList<T>`, so return internal lists directly via the readonly interface — no need to copy

```csharp
// Good - return internal list as readonly interface
private readonly List<Item> _items = new();
public IReadOnlyList<Item> Items => _items;

// Good - return filtered result
public IReadOnlyList<Item> GetActiveItems() => _items.Where(i => i.IsActive).ToArray();
```

### Null Handling

- Use `?.`, `??`, `??=` for plain C# types (services, DTOs, collections, strings)
- For `UnityEngine.Object` types use `if (obj)` / `if (!obj)` — **never** `?.` or `== null`. Full rule and rationale in [unity.md](unity.md).

```csharp
// Good - plain C# object
var name = user?.DisplayName ?? "Unknown";
_cache ??= new Dictionary<string, int>();
```

### Methods

- Keep methods **short** (single task)
- Maximum **3-4 parameters**
- **Return early** to reduce nesting
- **No boolean flags** - use separate methods or enums

### Prefer Named Models over Value Tuples

Never use `ValueTuple` (including C# tuple syntax) as a return type, field, or parameter. Define a named `class`, `struct`, or `record` instead — it communicates intent, supports extension, and prevents fragile positional access.

```csharp
// Avoid - intent unclear, fragile to reordering
public (float health, float armor) GetPlayerStats() => (_health, _armor);

// Good - explicit, named, extensible
public class PlayerStats
{
    public float Health { get; }
    public float Armor { get; }

    public PlayerStats(float health, float armor) { Health = health; Armor = armor; }
}

public PlayerStats GetPlayerStats() => new PlayerStats(_health, _armor);
```

For lightweight value objects a `readonly struct` is preferred (no `record struct` or `init` — C# 10 / needs a polyfill under the C# 9 ceiling):

```csharp
public readonly struct DamageResult
{
    public float Damage { get; }
    public bool IsCritical { get; }

    public DamageResult(float damage, bool isCritical)
    {
        Damage = damage;
        IsCritical = isCritical;
    }
}
```

Tuples are acceptable **only** inside a single private method body where the tuple never crosses a method boundary.

### Unity Serialization

```csharp
// Each attribute in its own brackets, all inline with the member
[SerializeField] private float _speed = 10f;

[SerializeField] [Range(0, 100)] private int _health = 100;

// Only layout/decoration attributes (Header, Space, Tooltip) get their own row
[Header("Combat")]
[SerializeField] [Min(0)] private int _damage = 10;
```

**Prefer `[field: SerializeField]` auto-properties for authored, read-only data** (ScriptableObjects,
`[Serializable]` structs/classes): one member instead of a private field plus a getter, and it works in
structs, where `readonly` fields are not serialized.

```csharp
[field: SerializeField] public float HealthMultiplier { get; private set; } = 1f;
```

- The serialized name is the compiler's backing field (`<HealthMultiplier>k__BackingField`), so
  converting or renaming a serialized member drops its value in existing assets — re-serialize them
  (re-enter the values) rather than adding `FormerlySerializedAs`.
- A collection exposed as `IReadOnlyList<T>` stays a private `[SerializeField]` array/list plus a
  getter; an auto-property can't be both the interface type and serializable.
- Attributes go on the field target, each in its own brackets and inline with the member: `[field: SerializeField] [field: Range(0f, 1f)] public float Chance { get; private set; }`.

---

## Error Handling Conventions

Async/cancellation mechanics (CTS ownership, `UniTaskVoid`, `DelayType.Realtime`) are in
[patterns.md](patterns.md). This section covers only what to do when something throws.

### Strategy by Context

| Context | Pattern | Example |
|---------|---------|---------|
| **Critical operation** (a path that can really fail) | Catch, log, re-throw | Bootstrap, scene wiring |
| **Optional operation** | Catch, log, return false/default | Loading an optional asset |
| **Fire-and-forget** | `.Forget()` — errors go to global handler | Analytics, audio, non-critical saves |
| **Expected cancellation** | `catch (OperationCanceledException) { }` — no logging | Manager disposal, scene unload |

### Log Levels in Error Recovery

- `LogError` — first failure detection
- `LogWarning` — recovery attempts
- `LogInformation` — success after retry

### Rules

| Rule | Description |
|------|-------------|
| **Always log in catch** | Never swallow exceptions silently (except `OperationCanceledException` in expected cancellation paths) |
| **Structured parameters** | Use `{Named}` placeholders, not string interpolation in log templates |
| **Finally for cleanup** | Always dispose `CancellationTokenSource`, release handles, unsubscribe in `finally` |
| **No catch-all at boundaries** | Managers should catch specific exceptions; the global `UniTaskLogger` handles truly unobserved ones |
| **No defensive handling** | No try/catch, null checks, or fallbacks for paths that cannot occur given codebase invariants |
