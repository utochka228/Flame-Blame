# Core Patterns

Code lives under `Assets/Scripts/{Core|Features|Bootstrap|Tests}/` — see
[architecture.md](architecture.md) for the layer rules. **No DI container** — no Zenject,
no VContainer. Dependencies are wired by hand from `Bootstrap` (constructor arguments for plain
classes, serialized fields for MonoBehaviours).

## Behavior and Workflow

### ScriptableObject Configs

```csharp
[CreateAssetMenu(fileName = "FeatureConfig", menuName = "FlameBlame/Configs/Feature")]
public class FeatureConfigSO : ConfigSO
{
    [field: SerializeField] public MotionConfig Motion { get; private set; } = new();
}
```

- `menuName` always starts with `FlameBlame/`.
- Never hardcode balance numbers in a behavior — they belong in the SO.
- Configs derive from `FlameBlame.Core.Config.ConfigSO`. Group fields into `[Serializable]` sub-configs (`Config/` folder), each exposed on the SO as a `[field: SerializeField]` property (Unity draws the foldout); small configs stay flat with `[field: Header]` categories.
- Live tuning: `ConfigSO.Changed` fires when a value is edited in play mode (`LiveTuning` on) or via the Apply Now button. Consumers subscribe in `OnEnable`, unsubscribe in `OnDisable`, and re-apply (`ApplyTo(settings)` in place, `Configure(...)` on stateful models, or rebuild a stateless model). Structural values (particle counts, mesh resolution) apply at spawn only.

- SOs are shared assets — **stateless**. Runtime state lives on the consuming object.

### Events — two channels

```csharp
EventBus.Subscribe<SomeEvent>(OnSomeEventRaised);     // cross-system, static typed bus
EventBus.Unsubscribe<SomeEvent>(OnSomeEventRaised);   // mandatory, in OnDestroy/OnDisable

public event Action<int> AmountChanged;                  // local, on the object owning the data
```

Default to the local `System.Action` / `Action<T>` — not `UnityAction`. Use `UnityAction` only where a Unity API demands it (`Button.onClick.AddListener`); wrap with a lambda when passing an `Action` in. Use `EventBus` only when publisher and subscriber must not know each other. **Every subscribe needs a matching unsubscribe** — the bus is static and outlives scenes.

### Async — UniTask, narrowly scoped

`com.cysharp.unitask` is installed, but the project is **coroutine-first**.

| Need | Use |
|------|-----|
| Procedural motion / fades | DoTween — see [unity.md](unity.md) |
| Per-frame / gameplay sequencing on a MonoBehaviour | `IEnumerator` coroutine |
| Realtime timer loop in a plain-class manager (regen, periodic checks) | `UniTask` |
| Anything else | Neither — don't introduce async |

**Never use `System.Threading.Tasks.Task` / `Task<T>` in runtime code** — zero occurrences in
the project, and `UniTask` is the awaitable everywhere. The single exception is test code that
must await; if tests are ever added, they may use `Task` because the NUnit runner requires it.

Return types:

| Return Type | When to Use | Caller Pattern |
|-------------|-------------|----------------|
| `UniTask<T>` | Needs to return a value and be awaited | `var result = await DoAsync(ct);` |
| `UniTask` | No return value but must be awaited | `await DoAsync(ct);` |
| `UniTaskVoid` | Fire-and-forget, cannot be awaited | `DoAsync().Forget();` |

**Never use `async void`** — it cannot be awaited, swallows exceptions, and cannot be cancelled.

```csharp
public class FeatureManager : IDisposable
{
    private CancellationTokenSource _cts;       // manager lifetime
    private CancellationTokenSource _loopCts;   // one loop chain, linked to _cts

    public void Init()
    {
        _cts = new CancellationTokenSource();
        LoopAsync(_cts.Token).Forget();          // fire-and-forget, never awaited
    }

    private async UniTaskVoid LoopAsync(CancellationToken ct)
    {
        try
        {
            await UniTask.Delay(delay, DelayType.Realtime, cancellationToken: ct);
            ct.ThrowIfCancellationRequested();
            // work, then re-arm
        }
        catch (OperationCanceledException) { }   // expected on Dispose
    }

    public void Dispose()
    {
        _loopCts?.Cancel(); _loopCts?.Dispose(); _loopCts = null;
        _cts?.Cancel();     _cts?.Dispose();     _cts = null;
    }
}
```

- Every `await` takes a `CancellationToken`; restartable loops use `CreateLinkedTokenSource(_cts.Token)` so one chain dies without killing the manager.
- The owning manager implements `IDisposable` and cancels + disposes every CTS there.
- `DelayType.Realtime` for wall-clock timers, so `Time.timeScale` and pausing don't distort them.
- WebGL is single-threaded: `UniTask.Delay` and coroutines run on the main thread; never `UniTask.RunOnThreadPool` or `SwitchToThreadPool`.

---

## Logging

Use `Microsoft.Extensions.Logging.ILogger<T>` — never `Debug.Log`. Loggers come from the static
factory `FlameBlame.Core.Logging.Loggers`, not from a constructor parameter:

```csharp
private static readonly ILogger<MyManager> Logger = Loggers.For<MyManager>();

public void DoSomething(int value)
{
    Logger.LogInformation("Processing value: {Value}", value);

    try
    {
        // ...
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to process value: {Value}", value);
    }
}
```

- **Always `static readonly`, never a per-instance field.** The logger is stateless — nothing about
  it varies per object — and the cost is in *creation* (~1.6 µs), not storage (~8 bytes), so an
  instance field pays that on every spawn for a byte-identical value. `static` also works in static
  methods, which keeps one idiom per file.
- Name it `Logger` (PascalCase, per the constants rule), and **store it in a field** — calling
  `Loggers.For<T>()` at the log site costs ~100x the log call itself.
- The same field works in a plain class, a `MonoBehaviour` and a `ScriptableObject`. Unity
  constructs the latter two, so a constructor parameter is not an option — and there is no DI
  container to supply one.
- A **static class** cannot be a type argument: use `Loggers.For(typeof(X))`, which returns a
  non-generic `ILogger`, plus `using ILogger = Microsoft.Extensions.Logging.ILogger;` to
  disambiguate from `UnityEngine.ILogger`.
- **Always `{Named}` placeholders, never string interpolation.** The placeholder count must equal
  the argument count or the call throws `FormatException`.
- Pass a `UnityEngine.Object` whole rather than as `.name` — the sink makes the console line ping
  the object.
- Never prefix a message with a category tag like `"[FireSystem] "`; every line already carries the
  logger's category.

`Loggers` is Core infrastructure in the shape of [`EventBus`](#events--two-channels) — **not** a
third service locator. It resolves nothing with a lifetime and holds no state.

The sink (`FlameBlame.Core.Logging.UnityLoggerProvider`) forwards to `Debug.Log` / `LogWarning` /
`LogError`, and additionally hands `Error` and `Critical` to `ICrashReporter` when one is
registered on `Loggers.CrashReporter` (no implementation ships yet). The minimum level is
`Information` in the Editor and development builds, `Warning` in release.

> `Debug.Log` survives only in editor-only tooling (`Editor/` folders).

---
