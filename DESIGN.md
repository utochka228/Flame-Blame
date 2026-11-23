# Flame & Blame — Hose System Design Doc

Source of truth: https://claude.ai/artifact/25BaRzKnSzxLpxwA9tV63G (exported 2026-11-23)

## Overview

The demo is one playable Unity scene built around a single modular fire hose system, where one variable, water pressure, decides how the hose behaves. The hose puts out fires, carries players, moves objects and causes chaos, so the player solves different kinds of problems with one tool.

It answers the test brief: take a mechanic from the Flame and Blame trailer, or invent one, and build it as a system, not a one-off effect. The demo has to be funny, readable at a glance, feel like co-op chaos, respond to every player action, interact with other objects and systems, and run without breaking or freezing.

Design pillars:

- **One system, many problems.** Firefighting, getting places and moving things are all solved with the same hose.
- **One rule to learn.** Pressure sets stiffness and force. Everything else follows from it.
- **Physical and floppy.** The hose is a jelly-like rope that reacts to everything: players, doors, fire, water.
- **Lego-like but readable.** A few parts snap together. Each part does one obvious thing.
- **Ridiculous by design.** Thumb over the nozzle, hoses thrashing like snakes, balloon bursts, teammates standing on your hose.
- **Rock solid.** The demo never explodes, jitters or freezes while someone plays it.

## Core rule: pressure = stiffness + force

Pressure at any point in the hose sets two things: how stiff the hose is there, and how hard water pushes out of the nozzle. Players learn this one dial and work out the rest themselves.

| State | Pressure | Hose behaviour | What players use it for |
| --- | --- | --- | --- |
| Empty | 0 | Floppy rope, drags on the floor | Throw it, lasso, tie objects, climb it as a rope |
| Low | Below ~40% | Semi-stiff, wobbly jelly | Spray water, put out fires, light pushes |
| High | ~40–100% | Rigid pole, strong recoil | Bridge over gaps, push heavy objects, jet the player around |
| Over limit | Above the hose's max | Bulges into a balloon, shakes, then pops | Nothing good: it leaks, whips around and needs tape |

The percentages are starting values for tuning, not final numbers. Each hose type has its own max pressure, so the same source can be fine for a thick hose and burst a thin one.

## System components

The demo has seven part types. Each one does one obvious thing, so a player can read a hose network at a glance.

| Part | What it does | Key properties |
| --- | --- | --- |
| Thin hose | Light, long, carried by one player | Low max pressure, bursts easily |
| Thick hose | Heavy, carries high pressure | Needs two players, or one player drags it slowly |
| Universal coupling | Joins any two hose ends | Snaps on with a satisfying pop; can be pulled apart by force |
| Y-splitter | Splits one line into two | Each output gets half the pressure |
| Source (hydrant, fire truck) | Feeds water into the network | Fixed source pressure; stationary |
| Valve (on the source) | Turns the water on or off | Operated by a player, usually not the one holding the nozzle |
| Nozzle | The hose's working end | Aimed by the holder; applies recoil |

Two player-made modifiers sit on top of the parts:

- **Thumb over the nozzle.** Covering part of the opening raises pressure at the tip, per player. It's the signature move: funny, physical and it teaches pressure without any text.
- **Tape.** A quick fix that joins two cut ends or patches a burst. It always leaks a little, losing pressure and making puddles that become someone else's problem.

Hose length is a property of each piece, not a separate type. Pieces come in a few fixed lengths.

## Player actions

Every action uses the same hose, and each one depends on the current pressure.

| Action | Needs | Result |
| --- | --- | --- |
| Grab / carry | Any hose piece | Pick up an end or a middle point; thick hose slows you or needs a buddy |
| Connect / disconnect | Two free ends + coupling or splitter | Extends or branches the network |
| Spray | Low or high pressure | Puts out fire, makes puddles, pushes objects |
| Thumb boost | Holding the nozzle | Higher tip pressure, narrower jet, more recoil |
| Jet move | High pressure, nozzle aimed down or back | Recoil lifts or launches the player; limited by hose length |
| Throw | Empty or low pressure | Hose flies like a rope; can wrap around or hook onto things |
| Tie / pull | Empty hose wrapped around an object | Drag the object, or turn the water on to yank it |
| Bridge / climb | High pressure for a bridge, empty for a rope | Walk across a rigid hose, or climb a hanging one |
| Tape | A cut or burst point | Restores flow, with a permanent leak |
| Valve | Standing at a source | Turns the whole network on or off |

Controls are kept minimal: move, jump, grab/drop, use (spray, or tape when near a break), thumb (hold), and aim. Context decides what "use" does.

## System interactions

The hose and the scene affect each other both ways, using systems the scene already has: fire, doors, physics objects and players. No extra hazard systems are added.

| The scene affects the hose | The hose affects the scene |
| --- | --- |
| Fire burns through a hose section, leaving a leak or a cut | Water puts out fire and makes steam that blocks the view |
| A player standing on the hose pinches it and cuts the flow | Puddles make the floor slippery, so players slide |
| A door closing on the hose pinches it, or cuts it if it slams hard | A strong jet knocks over crates, shelves and teammates |
| An explosion or heavy object cuts it, leaving a loose, whipping end | A rigid hose becomes a bridge; an empty one becomes a rope |
| Too much pressure bursts a weak section | A tied hose drags objects when the water turns on |

Cutting is never a player tool. Hoses get cut only by fire, doors, bursts and heavy impacts, so every cut has a cause the players can see and blame.

## Co-op blame and comedy beats

The funniest moments come from one player's action becoming another player's problem. The system should produce these on its own rather than through scripts.

Blame sources:

- **Standing on the hose.** The nozzle sputters and dies because a teammate is standing on the line. This is the main co-op blame mechanic.
- **The valve.** One player opens or closes the water while another is mid-jet or mid-bridge.
- **Tape leaks.** A quick fix floods the floor, and someone else slides into the fire.
- **The splitter.** Adding a second branch halves everyone's pressure.
- **Slamming doors.** Closing a door on a hose pinches or cuts someone else's line.

Comedy beats:

- **Hose snake.** A dropped nozzle at high pressure thrashes around and knocks everything over.
- **Late water.** Opening the valve sends a visible bulge down the hose, giving a beat of "oh no" before the spray hits.
- **Balloon section.** An over-pressured section swells into a huge sausage, shakes, then pops.
- **Thumb spray.** A thumb over the nozzle sprays a wide, silly fan that soaks teammates.
- **Recoil fails.** Too much pressure with no thumb control sends the holder flying backwards.

## Multiplayer showcase

Decided: local co-op for two players on one screen, sharing a keyboard or using gamepads. A reviewer playing alone can still try both roles by swapping between the two firefighters.

| Mode | How it works | Status |
| --- | --- | --- |
| Local two-player co-op | Player 1 on one half of the keyboard, player 2 on the other half, or either on a gamepad | Core |
| Character swap (solo) | One key switches control between the two firefighters; the idle one stays where it is, and can still end up standing on your hose | Core |
| Simple AI buddy | Takes the second firefighter in solo play: holds a line or works the valve | Stretch |
| Online multiplayer | Not built: XPBD rope sync over WebSockets is high risk for a small demo | Cut |

Shared camera: one dynamic camera frames all firefighters, zooming out as they spread, as in other couch co-op games.

If the AI buddy stretch goal is built, it's deliberately a bit dumb. It follows simple behaviours on a NavMesh, and its mistakes are a feature: it wanders onto your hose, opens the valve at the wrong moment, or slides through puddles.

Architecture: every firefighter is the same component driven by an input source (keyboard, gamepad or AI). All hose rules are per firefighter (thumb, carrying, pinching), so adding players needs no special cases. The design stays open to online play later without building it now.

## Demo slice: the burning building

One small burning building with four problems, each solved by a different side of the pressure rule. Players start at a hydrant and fire truck outside with a pile of hose pieces, couplings and one Y-splitter.

1. **Fire behind a gap.** A collapsed floor separates the players from a fire. Make a rigid high-pressure hose into a bridge, or jet across using recoil.
2. **Fire on the upper floor.** No stairs. Throw an empty hose up and over a beam, climb it like a rope, then turn the water on.
3. **A crate blocking a door.** Push it away with a high-pressure jet, or tie an empty hose around it and yank it by turning the water on.
4. **A burst hose.** Fire burns through the main line mid-level. Tape it (fast, but the leak floods the floor and makes it slippery) or fetch a coupling (slower but clean).

The problems can be solved in any order. The level is done when all fires are out. The point of the level is to show that one system solves four different kinds of problems, and causes chaos along the way.

## Feedback and readability

Pressure is shown on the hose itself, not on a HUD gauge. A player should read the state of any hose in under a second.

| Signal | How it reads |
| --- | --- |
| Hose thickness | The tube swells as pressure rises |
| Pulse | A bulge travels from the source when water turns on |
| Colour tint | Shifts from neutral to warm as pressure nears the hose's max |
| Shake | Sections near the limit tremble, then balloon before bursting |
| Sag | Empty hose sags and drags; full hose holds its shape |
| Jet shape | Width and length of the spray match tip pressure; the thumb makes a wide fan |
| Leak spray | Tape and burst points spit visible water and leave puddles |
| Pinch | A hose squashes flat under a foot or door, and flow beyond it visibly drops |

Every action gets juice: a pop on connect, squish sounds on grab, camera shake on strong recoil and bursts, splash particles on impact, and jelly squash-and-stretch on the hose and on players hit by water.

## Technical design

The hose is a custom XPBD rope simulation driven by a simple network pressure solver. No fluid simulation, no third-party rope asset.

### Rope simulation (XPBD)

- **Method:** Extended Position-Based Dynamics (Macklin et al., 2016), built on Verlet-style integration (Jakobsen, 2001). XPBD's stiffness is a compliance value that behaves the same whatever the timestep or iteration count, so frame-rate drops don't change how the hose feels.
- **Particles:** each hose piece is a chain of particles spaced ~0.15–0.25 m apart.
- **Distance constraints** between neighbours stop stretching.
- **Bending constraints** between particle triples set stiffness. Bend compliance is mapped from local pressure: high pressure means low compliance, so the hose goes rigid.
- **Collisions:** each particle does a sphere overlap against scene colliders and is pushed out with `Physics.ComputePenetration`. Several substeps per `FixedUpdate`.
- **Couplings** are shared particles or a stiff distance constraint between two chain ends, so networks behave as one rope.

### Pressure solver

The network is a tree rooted at a source. It's recalculated a few times per second, not every frame.

```
P_tip = (P_source - k_len * L - sum(P_leak)) * (1 / 2^n_split) * (1 + b_thumb)
```

A pinch (foot, door) sets flow beyond that point to zero. A section above its hose's max pressure starts the burst sequence. Values are smoothed over time so pressure rises and falls visibly rather than snapping.

### Gameplay hooks

| Hook | Implementation |
| --- | --- |
| Recoil and jetting | Force on the end particle along the nozzle's back direction, plus the opposite force on the holder's rigidbody |
| Standing on the hose | Player capsule overlapping hose particles marks a pinch |
| Door pinch and cut | Door collider overlap marks a pinch; a high closing speed cuts the chain |
| Fire damage | Particles inside a fire volume build heat; past a threshold the section leaks or cuts |
| Burst | Section pressure above max for a short time triggers balloon, shake, pop, leak |
| Bridge | When rigid, kinematic capsule colliders are placed along the hose so players can walk on it |
| Rope climb | An empty hose hanging from an anchor gives a climbable path along its particles |

### Water and puddles

- The jet is a particle system. Its collision events hit components such as `Extinguishable`, `Wettable` and `Pushable` on scene objects.
- Puddles are decals or spreading discs with a slippery physics material area. They evaporate near fire.
- Steam is a particle burst when water hits fire.

### Rendering

- A tube mesh is rebuilt each frame along a Catmull-Rom spline through the particles.
- Vertex radius per ring handles swelling, pulses and balloon bulges. Colour tint comes from a per-vertex pressure value.

## Architecture

The code is organized by feature on top of a thin Core, with one assembly per feature and dependency injection done by hand. Clean Architecture and DI containers are left out on purpose: at this size they add indirection a reviewer has to dig through before reaching the interesting code.

### Decisions

| Decision | Why | If the project grew |
| --- | --- | --- |
| Feature folders, not horizontal layers | Each system lives in one place and matches a section of this doc | Same structure scales; split big features into sub-features |
| One assembly definition per feature | Dependency direction is enforced by the compiler; faster compiles | Keep; add separate Simulation assemblies if needed |
| Simulation in pure C# | Rope and pressure logic is testable without a scene; backs the stability claim | Keep; could move to Burst jobs on desktop builds |
| No DI container; one composition root | Small object graph, scene built by hand; dependencies stay explicit and easy to trace | Move to VContainer |
| No singletons or service locator | Every dependency is visible in a constructor or serialized field | Keep |
| Small interfaces where systems meet (`IWettable`, `IExtinguishable`, `IPushable`, `IPinchSource`, `IHeatSource`) | Water doesn't know about fire or crates; this is where the systemic design lives in code | Keep; add interfaces per new system |
| Typed event bus for feedback | VFX, audio and camera react to every action without gameplay code knowing they exist | Keep |
| ScriptableObjects for tuning | Feel and WebGL budgets change without code changes | Keep |

These decisions also go into the README as a short Architecture decisions list, so reviewers see the reasoning.

### Project layout

Scripts live in their own tree, organized by feature. Content is organized by asset type, with feature subfolders where it helps.

```
Assets/
  Scripts/          all code, by feature
  Prefabs/          by feature: Prefabs/Hose, Prefabs/Fire, ...
  Art/              Models, Materials, Textures, Shaders
  VFX/
  Audio/
  Configs/          ScriptableObject instances (tuning values)
  Scenes/
  ThirdParty/         Asset Store packages, untouched
```

Content stays out of feature folders because it's shared across features and Asset Store packages arrive sorted by type. That keeps each feature folder under Scripts exactly one assembly, with no art inside.

### Scripts layout

```
Scripts/
  Core/                       FlameBlame.Core
    Events/  Pooling/  Math/  Config/
  Features/
    Hose/                     FlameBlame.Hose
      Simulation/             pure C#: XpbdRope, constraints, particles
      Components/             MonoBehaviours: HoseSegment, Coupling, Nozzle, TapePatch
      Presentation/           HoseTubeRenderer, PressureVisuals, NozzleJetVfx, HoseAudio
      Config/                 HoseConfig (ScriptableObject definition)
      Events/                 HoseBurst, HoseConnected, NozzleRecoil
      Editor/                 FlameBlame.Hose.Editor: gizmos, debug inspector
    Pressure/                 FlameBlame.Pressure (mostly Simulation/)
    Water/  Fire/  Firefighter/  Interactables/  CameraRig/
    UI/                       FlameBlame.UI
  Bootstrap/                  FlameBlame.Bootstrap: GameBootstrap, scene wiring
  Tests/
    EditMode/                 pressure solver, rope math
    PlayMode/                 rope stability stress tests
```

Every feature uses the same four roles, so learning one teaches the rest:

- **Simulation:** logic with no Unity objects.
- **Components:** MonoBehaviours that bridge the logic to the scene.
- **Presentation:** how it looks and sounds.
- **Config:** tuning definitions.

Namespaces match folders, for example `FlameBlame.Hose.Simulation`.

### Feature visuals vs UI

- **World visuals belong to the feature,** in its Presentation folder: tube mesh, swelling, jet, leaks, puddles, fire. Since pressure is shown on the hose, most of the demo's "UI" lives here.
- **Screen UI is its own feature:** title screen, click to start, controls overlay, join and swap prompts, pause.
- **Interaction prompts:** UI owns one generic prompt system; features only provide data, such as `IInteractable.PromptText`.
- **UI depends on features, never the reverse.** It listens to events or reads small read-only interfaces.

### Dependency direction

```
Core  <-  Features (Hose, Pressure, Water, Fire, ...)  <-  UI  <-  Bootstrap
```

Arrow = depends on. Features never reference UI or Bootstrap; Core references nothing. Bootstrap is the only assembly that references everything, because it is the composition root that wires the scene.

## Stability and performance guardrails

Rope physics is the biggest risk to a demo that must never break. These guardrails are built in from the first prototype, not added at the end.

- **Hard caps:** a maximum number of particles per scene and per hose piece, and a maximum number of connected pieces per network.
- **Velocity clamps** on every particle and on players launched by recoil.
- **Stretch recovery:** if a section stretches far past its rest length, or a particle ends up inside geometry, it snaps back to a safe position.
- **No NaNs:** any invalid particle position resets that hose to its last good state.
- **Fixed timestep and substeps** so simulation quality doesn't depend on frame rate.
- **Pressure solver off the frame loop:** a few updates per second, with smoothing.
- **Pooled effects:** particles, decals and audio sources are pooled, never created mid-play.
- **Soft reset:** a button that respawns the player and resets one hose if anything goes wrong.
- **Performance budget:** the WebGL build holds 60 fps in Chrome on a mid-range laptop, checked with the Unity Profiler at each milestone.

## Platform: WebGL first

The target build is WebGL, with a Windows .exe as the allowed fallback. The browser sets tighter limits, so these rules apply from day one:

- **Single-threaded budget.** Plan for the rope sim, pressure solver and AI to run on the main thread. Multithreaded jobs can't be relied on in WebGL.
- **Particle budget.** Fewer particles per hose and lower water particle counts than a desktop build; budgets are set in ScriptableObjects per platform.
- **Physics:** fixed timestep with a cap on substeps per frame, so a slow browser frame never causes a spiral of catch-up steps.
- **Input:** keyboard plus the browser Gamepad API through the Input System. The canvas must grab focus on click.
- **Audio:** starts only after the first click (browser autoplay rules), so the title screen asks for a click to start.
- **Build size and loading:** compressed build (Brotli or Gzip, matched to the host), stripped unused code and assets, a short loading screen.
- **Hosting:** itch.io or similar, tested in Chrome, Firefox and Edge.
- **Fallback trigger:** if WebGL can't hold 60 fps on a mid-range laptop after optimisation, ship the Windows .exe as the main build and keep WebGL as a lighter version.

## AI-assisted workflow

The brief's "AI must have" is met by building the demo with Claude working directly inside the Unity Editor through the Unity MCP connection, to cut development time and raise quality.

| Area | What Claude does |
| --- | --- |
| Code | Writes the XPBD rope, pressure solver and gameplay components; compiles, reads the console and fixes errors in a loop |
| Scene building | Creates the level layout, prefabs, materials and physics settings directly in the editor |
| Tests | Writes EditMode tests for the pressure solver (splits, leaks, pinches, thumb bonus) and stress tests for the rope |
| Tuning | Puts feel values in ScriptableObjects so they can be tweaked without code changes |
| Visual checks | Takes Game view captures to check readability and whether moments land as funny |
| Profiling | Reads performance stats and flags spikes against the 60 fps budget |
| Design docs | Keeps this doc and the decision log up to date |

A short AI log is kept alongside the project: what was generated, what was changed by hand, and the rough time saved. It's part of the deliverable, so reviewers can see how AI was used.

## Scope

The demo ships the core rule, seven part types and four puzzles. Everything else is cut or left as a stretch goal.

| Feature | Status | Reason |
| --- | --- | --- |
| Pressure = stiffness + force | In | The core of the whole system |
| Thin and thick hoses, coupling, Y-splitter, valve, sources, nozzle | In | Minimum set that shows the lego idea and stays readable |
| Thumb boost, tape, recoil, jetting, bridge, rope, throw, tie | In | The actions the four puzzles need |
| Standing on the hose, door pinch and cut, fire damage, bursts | In | Two-way interactions with existing systems |
| Slippery puddles, steam | In | Cheap, funny, and reuse the water system |
| Local two-player co-op, character swap | In | Shows co-op on one machine; solo reviewers can try both roles |
| Simple AI buddy | Stretch | Nice for solo play, not required |
| Fake kinks (sharp bend angle reduces flow) | Stretch | Stable and cheap, but standing on the hose already covers the idea |
| Real knot detection | Cut | Unstable and expensive for little payoff |
| Electricity and wet wires | Cut | A second hazard system dilutes the one-system message |
| Many adapter, tip and valve types | Cut | Merged into one coupling and one splitter for readability |
| Configurable carry weight | Cut | Fixed rule instead: thick hose needs a buddy |
| Player-operated cutting tool | Cut | Cuts come only from fire, doors, bursts and impacts |
| Online multiplayer | Cut | Rope sync over the network is too risky for the demo |

## Milestones

Work runs in five phases, and each one must pass its gate before the next starts. No dates are set yet.

1. **Rope prototype (next step).** One hose fixed to a hydrant, a pressure slider, a few boxes to collide with. XPBD distance and bend constraints, collisions, tube mesh with swelling.
   - Gate: stays stable at 60 fps, and floppy-to-rigid feels good.
2. **Pressure network.** Couplings, Y-splitter, valve, pinches, leaks, burst sequence. EditMode tests for the pressure solver.
   - Gate: solver tests pass; bursts and pinches read at a glance.
3. **Player and hose actions.** Grab, carry, spray, thumb, recoil, jet, throw, tie, bridge, climb, tape. Local two-player co-op, character swap, shared camera.
   - Gate: every action works in a test room.
4. **Systems and level.** Fire, doors, puddles, steam, pushable objects. The burning building with its four puzzles.
   - Gate: level completable start to finish by every route.
5. **Polish and hardening.** Juice, sound, camera, comedy beats. Stability guardrails, profiling, playtests, AI log.
