# Unity-Specific Patterns

## Unity Object Null Checks

**CRITICAL:** Unity overrides `==` for `UnityEngine.Object`. Destroyed objects are not C# `null` but compare equal to `null` via Unity's operator. The `?.` and `??` operators use C# reference equality and **do not** respect this.

```csharp
// ❌ WRONG — C# null check, doesn't catch destroyed objects reliably
if (something == null) { }
if (something != null) { }
something?.DoThing();

// ✅ CORRECT — Unity's implicit bool operator, handles destroyed objects
if (!something) { }
if (something) { something.DoThing(); }
```

**Rule:** For any variable that is or could be a `UnityEngine.Object` (GameObject, Component, MonoBehaviour, ScriptableObject, etc.), **always** use `if (!obj)` / `if (obj)` — never `== null` / `!= null`.

For plain C# types (services, DTOs, collections, strings) `?.` / `??` / `??=` are correct — see [conventions.md](conventions.md).

---

## MonoBehaviour Lifecycle

`Awake` is the **early** phase and `Start` the **late** one: publish and cache in `Awake`, resolve
other objects in `Start`. `Bootstrap` is the composition root (see [architecture.md](architecture.md)).

```csharp
public class ExampleBehaviour : MonoBehaviour
{
    private SomeComponent _component;
    private Tween _fade;

    private void Awake()
    {
        _component = GetComponent<SomeComponent>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<SomeEvent>(OnSomeEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SomeEvent>(OnSomeEvent);
    }

    private void OnDestroy()
    {
        _fade.Kill();
    }
}
```

- Cache `GetComponent` results in `Awake`; never call it in `Update`.
- Every `EventBus.Subscribe` needs a matching `Unsubscribe` — the bus is static and outlives scenes.
- Anything with a handle (`Tween`, `IDisposable`) is released in `OnDestroy`/`OnDisable`.

---

## UI Text (TextMeshPro) — design implementation

When implementing a design (Figma → prefab) or setting a font on any `TMP_Text`:

- **Font asset is always the family's `-Regular SDF` variant** (`Oxanium-Regular SDF`, `Teko-Regular SDF`). Never assign `-Bold SDF`, `-ExtraBold SDF`, etc. directly. Single-weight families (`BlackOpsOne-Regular`, `PermanentMarker-Regular`) just use their one asset.
- **Weight comes from a TMP text style**, not the font asset: set `textStyle` from the default style sheet by weight — `Regular` (400), `Medium` (500), `Semi-Bold` (600), `Bold` (700), `Heavy` (800). The Regular asset's weight table resolves the style to the right face.
- Match the design's weight to the style by name (Figma Oxanium 700 → `Oxanium-Regular SDF` + `Bold`). Do not use the `fontStyle` Bold flag as a weight substitute.
- Apply this to every TMP in the prefabs you create or touch, including text inside reused or cloned popups.

---

## Animation

Two systems, and they do not overlap:

| Need | Use                              |
|------|----------------------------------|
| Procedural motion, fades, scale/colour/volume ramps, timed callbacks | DoTween (`DG.Tweening`) |
| Authored character/VFX state machines | `Animator` + hashed parameter ids |

### DoTween

DoTween is an Asset Store package imported into `Assets/Plugins/Demigiant/`; leave it untouched.

- Store the returned `Tween` and `Kill()` it in `OnDestroy`/`OnDisable`; or chain `.SetLink(gameObject)` so it dies with the object.
- Never animate `.material` — see [Performance Rules](#performance-rules).
- Never tween the rope simulation: particle positions belong to `FixedUpdate`. Tweens are for presentation only.

### Animator parameters

Always drive `Animator` through cached hashed ids, never strings:

```csharp
private static readonly int IsChargingBool = Animator.StringToHash("IsCharging");
private static readonly int DefeatTrigger = Animator.StringToHash("Defeat");

_animator.SetBool(IsChargingBool, true);
_animator.SetTrigger(DefeatTrigger);
```

- One `private static readonly int` per parameter, hashed once at type load — **never `StringToHash` in `Update`** or a spawn loop.
- Name the field after the parameter plus its kind (`…Bool`, `…Trigger`, `…Float`), PascalCase.
- The string is a **contract with the authored controller**: renaming the parameter in the Animator silently breaks the call. Grep before renaming either side.
- Shader properties follow the same shape with `Shader.PropertyToID`.

---

## Object Pooling

Unity's `ObjectPool<T>` lives in `UnityEngine.Pool`. Particles, decals and audio sources are pooled, never instantiated mid-play.

---

## Simulation and WebGL

- Rope simulation runs in `FixedUpdate` with a capped substep count per frame.
- Clamp particle and recoil velocities; reset a hose to its last good state on any NaN.
- WebGL first: no threads or jobs for gameplay; particle budgets live in ScriptableObjects.

---

## Performance Rules

Standard WebGL-first Unity perf hygiene applies (avoid GC allocs in hot paths, cache `GetComponent`, pool, `HashSet` for lookups, no `Find()` at runtime, etc.). The non-obvious gotchas that actually correct defaults:

| Rule | Description |
|------|-------------|
| **Never access `.material` directly** | Use `.sharedMaterial` for reading, `MaterialPropertyBlock` for per-instance changes — `.material` creates a copy, breaks batching, leaks memory |
| **Use CompareTag** | `CompareTag("tag")` instead of `.tag == "tag"` — avoids string allocation |
| **Avoid lambda captures in hot paths** | Lambdas capturing variables allocate closures; use cached delegates or direct method references |
| **Hash once, reuse** | `Animator.StringToHash` and `Shader.PropertyToID` go in `private static readonly int` fields — never in `Update` or a spawn loop |
