# Architecture

Full breakdown of project structure. Source: [DESIGN.md](../DESIGN.md) "Architecture". Updated iteratively. The layout below is planned, not yet created.

## Decisions

- Feature folders on a thin Core; one assembly definition per feature (compiler enforces dependency direction).
- Simulation in pure C# (testable without a scene).
- No DI container, one composition root (Bootstrap), no singletons or service locators. Dependencies are visible in constructors or serialized fields.
- Small interfaces where systems meet: `IWettable`, `IExtinguishable`, `IPushable`, `IPinchSource`, `IHeatSource`.
- Typed event bus for feedback: VFX, audio and camera react to events; gameplay code never references them.
- `EventBus` and `Loggers` are the only static infrastructure in Core. They resolve nothing and own no lifetimes, so they are not singletons or service locators.
- Each feature's `.asmdef` is created together with the feature, not ahead of it.
- ScriptableObjects for tuning, including per-platform (WebGL) budgets.

## Layout

```
Assets/
  Scripts/ Prefabs/ Art/ VFX/ Audio/ Configs/ Scenes/
  ThirdParty/                 Asset Store packages, untouched
Assets/Scripts/
  Core/                       FlameBlame.Core: Events, Extensions, Logging exist; Pooling, Math, Config added when first needed
  Features/
    Hose/                     FlameBlame.Hose
      Simulation/             pure C#: XpbdRope, constraints, particles
      Components/             MonoBehaviours: HoseSegment, Coupling, Nozzle, TapePatch
      Presentation/           HoseTubeRenderer, PressureVisuals, NozzleJetVfx, HoseAudio
      Config/                 HoseConfigSO (ScriptableObject)
      Events/                 HoseBurst, HoseConnected, NozzleRecoil
      Editor/                 FlameBlame.Hose.Editor
    Pressure/ Water/ Fire/ Firefighter/ Interactables/ CameraRig/
    UI/                       FlameBlame.UI
  Bootstrap/                  FlameBlame.Bootstrap: GameBootstrap, scene wiring
  Tests/EditMode, Tests/PlayMode
```

Every feature uses the same roles: **Simulation** (no Unity objects), **Components** (MonoBehaviours bridging to the scene), **Presentation** (visuals/audio), **Config** (ScriptableObject definitions). Namespaces match folders (`FlameBlame.Hose.Simulation`). Content (prefabs, art) stays out of feature script folders.

## Dependency direction

`Core <- Features <- UI <- Bootstrap`. Arrow = depends on. Features never reference UI or Bootstrap; Core references nothing; Bootstrap references everything.

- World visuals belong to the feature's Presentation folder.
- Screen UI is its own feature; it listens to events or reads small read-only interfaces (e.g. `IInteractable.PromptText`). UI depends on features, never the reverse.
