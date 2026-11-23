# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick Navigation

| Need | File |
| --- | --- |
| Gameplay rules, parts, puzzles, scope, milestones (design source of truth) | [DESIGN.md](DESIGN.md) |
| Project structure, assemblies, dependency direction | [.claude/architecture.md](.claude/architecture.md) |
| C# style, naming, units | [.claude/conventions.md](.claude/conventions.md) |
| Async, events, ScriptableObjects, wiring, pooling | [.claude/patterns.md](.claude/patterns.md) |
| Unity specifics: null checks, TMP, rendering, physics | [.claude/unity.md](.claude/unity.md) |

## Core Principles

- **YAGNI** — build only what the current task needs. No speculative infrastructure, wrappers, abstractions, or config around a change a single edit covers. No functionality for hypothetical futures. Fewer files, shortest working diff. If a senior engineer could call it overcomplicated or speculative, cut it.
- **Surgical changes** — touch only what the task requires. Never refactor, restyle, or "improve" unrelated code. Clean up only what your own change made obsolete.
- **No error handling for impossible cases** — no try/catch, null checks, or fallbacks for paths that cannot occur given codebase invariants.

## Communication

Be brief. No preambles, no closing recaps. One sentence between tool calls, not a paragraph. Files hold durable content only — never write explanations or "what changed" notes into a file being edited.

## Project

"Flame & Blame" — Unity 6 project (Editor 6000.3.17f1), URP 17.3, new Input System. A demo of one modular fire hose system where water pressure sets hose stiffness and nozzle force. Details in [DESIGN.md](DESIGN.md).

State: one scene (`Assets/Scenes/SampleScene.unity`), no gameplay scripts yet. Core infrastructure exists in `Assets/Scripts/Core/` (`EventBus`, `PercentExtensions`, `Loggers`); no `.asmdef` files yet — each is created with its feature. Packages added beyond the template: UniTask and `org.nuget.microsoft.extensions.logging` (OpenUPM). DoTween is an Asset Store import (not UPM), living in `Assets/Plugins/Demigiant/`. Next: milestone 1, the rope prototype (XPBD rope on a hydrant, pressure slider, tube mesh).

## Project Structure

See [.claude/architecture.md](.claude/architecture.md) for the full breakdown. It is updated iteratively.

## Working with the project

- No CLI build/test/lint. Work happens in the Unity Editor (Rider as IDE); `Flame&Blame.sln` and `.csproj` files are generated.
- Unity MCP tools (`unityMCP`, `unity`) drive the Editor. After any script change, read the console for compile errors before continuing.
- Tests use `com.unity.test-framework`: EditMode for pressure solver and rope math, PlayMode for rope stability. Run via Test Runner or MCP `run_tests`.
- Keep a short AI log (what was generated, hand-changed, time saved); it is a deliverable.
- Rendering: URP with PC and Mobile tiers in `Assets/Settings/`.
- Platform: WebGL first (Windows .exe fallback), single-threaded budget. See DESIGN.md "Stability and performance guardrails" and "Platform".

## Critical Rules

| Rule | Description |
| --- | --- |
| No .meta files | Never create or modify .meta files - Unity manages these |
| No test removal | Don't remove or rewrite existing tests unless explicitly instructed |
| No new packages | Don't add new packages unless specifically stated |
| Plan before edit | For fixes and refactors, present the approach FIRST — do not edit code without user approval |
| Regression baseline = dev | Diff against dev, not main. Don't `git diff main...HEAD` unless the user asks for main. main is released/working, so a feature "absent from main" is likely released-via-dev — treat the bug as a regression, not new-feature bring-up |
| No git operations | Do NOT commit, push, create branches, or amend commits unless a skill is explicitly invoked by the user |
| Preserve directives | When editing CLAUDE.md or skill files, NEVER weaken MUST/ALWAYS/NEVER to suggestions |
| Delegate to stay lean | As the main session fills toward the context budget you reserve for orchestration, hand heavy work to subagents and stay on their results — see Delegation |

## Wrong-by-Default Code Patterns

These are the patterns where my training defaults produce wrong code for this project. Apply unconditionally — the source files have the rationale and code examples.

| # | Rule | Source |
| --- | --- | --- |
| 1 | Unity null-check: `if (!obj)` / `if (obj)` on any `UnityEngine.Object` — never `obj == null` or `obj?.` | unity.md |
| 2 | C# 9.0 ceiling: no file-scoped namespaces, no `[]` collection expressions, no `"""` raw strings, no global usings | conventions.md |
| 3 | Constants are PascalCase — including cached hash ids (`BurstTrigger`, `IsChargingBool`), never UPPER_CASE | conventions.md |
| 4 | Async: coroutine-first; UniTask only for realtime loops in plain-class managers. Never `Task`, never `async void` | patterns.md |
| 5 | Units: fractions (0-1) at runtime; percent (0-100) only on serialized fields and UI strings. Never `/ 100f` — use `ToFraction()` / `ToPercent()` | conventions.md |
| 6 | TMP fonts: always the family's `-Regular` SDF font asset, with the weight set through a TMP text style (Regular, Bold, Heavy…) — never a `-Bold` SDF asset directly | unity.md |

## Context Loading (Required)

MUST read the relevant context file BEFORE starting work — new files and modifications, fixes, refactors. Announce it (e.g. "Reading .claude/unity.md because this drives an Animator").

| Trigger | Action |
| --- | --- |
| Editing or creating any `.cs` file anywhere in the project | MUST read `.claude/conventions.md` — at minimum the "How to read this file" section at the top |
| Touching files matching `*Manager.cs`, `*SO.cs`, or anything using `EventBus` | MUST read `.claude/patterns.md` |
| Touching MonoBehaviours, Animator, MaterialPropertyBlock, pooling code, or anything under `Assets/Scripts/Features/` | MUST read `.claude/unity.md` |

## Skills (Auto-Invoke)

IF A SKILL APPLIES, YOU MUST USE IT via the Skill tool. NEVER Read a skill file directly; NEVER do a skill's work by hand.

If there is even a 1% chance a skill applies — invoke it. A wrong skill costs nearly nothing; a skipped right one costs broken conventions and rework.

Available skills and their triggers are listed in the system skill index. Do NOT maintain a separate skill catalog in CLAUDE.md files.

## Delegation

A subagent reloads full context (CLAUDE.md + skill descriptions) and starts with no history — a fixed startup cost. Delegate only when the work outweighs it:

- **Worth it:** fan-out exploration/search, genuinely independent chunks run concurrently, or offloading to a cheaper model or lower reasoning effort when full capability isn't needed. Also to keep the main session lean once its context fills toward the budget you reserve for orchestration.
- **Inline:** trivial/small work where startup cost exceeds the work, or anything a single edit covers.
- Spawn independent tasks in a single message to run concurrently.
