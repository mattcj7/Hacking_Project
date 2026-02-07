# ADR (Architecture Decision Records)

Add entries like:

## ADR-0001: Module Layout for Core Assemblies
- Context: Need clear assembly boundaries to keep compile times fast and dependencies predictable.
- Decision: Create four runtime assemblies (Infrastructure, Systems, Game, UI) with one-way references: Systems -> Infrastructure; Game/UI -> Infrastructure + Systems. Add EditMode and PlayMode test assemblies referencing runtime modules.
- Consequences: Clear dependency direction and modular growth; tests compile against specific modules.
- Alternatives considered: Single monolithic assembly; feature-only splits without an infrastructure base.

## ADR-0002: GameStateMachine for Bootstrap Flow
- Context: Need a deterministic startup flow with logged transitions before committing to UI or gameplay systems.
- Decision: Implement a pure C# `GameStateMachine` owned by `GameBootstrapper`, with Boot/MainMenu/Gameplay states and explicit transitions.
- Consequences: Simple, testable flow with composition (no singletons) and easy future extension.
- Alternatives considered: Scene-only flow without state objects; global singleton state manager; coroutine-only transitions.

## ADR-0003: Unity Docs Baseline and Codex Guidance
- Context: Codex needs consistent guardrails to target Unity 6.3 LTS and modern APIs.
- Decision: Use Unity documentation version 6000.3 and enforce guidance through `AGENTS.md` and `.codex/config.toml`.
- Consequences: Consistent Unity version targeting, Input System usage, and safety constraints across future work.
- Alternatives considered: Rely on ad-hoc reminders in tickets.

## ADR-0004: Lightweight EventBus for Decoupling
- Context: Systems need a minimal, testable way to publish/subscribe without hard references.
- Decision: Add an instance-based `EventBus` with `IEvent` marker and explicit subscribe/unsubscribe tokens.
- Consequences: Local event distribution without introducing a service locator or DI container.
- Alternatives considered: Direct references between systems; static global event bus.

## ADR-0006: UI Toolkit Window Manager v1
- Context: The OS shell needs a minimal windowing system for spawning, focus, drag, and close behavior.
- Decision: Use a UI Toolkit window template with a lightweight `WindowManager`/`WindowView` and a dedicated desktop windows layer.
- Consequences: Window behavior is centralized without introducing a global singleton.
- Alternatives considered: IMGUI-based windows; a static global manager.

## ADR-0005: UI Toolkit Desktop Scaffold + Setup Tool
- Context: The OS shell needs an initial desktop/taskbar scaffold and reproducible scene wiring.
- Decision: Use UI Toolkit assets for the desktop shell and provide an Editor menu tool to wire `UIDocument` + `PanelSettings`.
- Consequences: Consistent UI setup with minimal manual scene edits.
- Alternatives considered: Manual scene wiring without a setup tool; IMGUI-based shell.

## ADR-0007: AppRegistry + Launcher for Taskbar Apps
- Context: Need a minimal, instance-based way to list installed apps and launch/focus windows from the taskbar.
- Decision: Add `AppRegistry` to hold installed `AppDefinition` data and an `AppLauncher` to create or focus single-instance windows via `WindowManager`. `DesktopShellController` owns both and populates the taskbar at runtime.
- Consequences: Taskbar buttons are data-driven and re-use the existing window system without global singletons.
- Alternatives considered: Hardcoded app buttons in UXML; a static/global registry.

## ADR-0008: TimeService as Single Time Source
- Context: The OS shell and future systems need a shared, consistent time source for clock updates and timers.
- Decision: Add an instance-based `TimeService` that publishes a once-per-second tick event via `EventBus` and is updated by `GameBootstrapper`.
- Consequences: UI and systems can subscribe to a single time source without ad-hoc timers.
- Alternatives considered: Per-UI timers; a static global clock.

## ADR-0009: JsonUtility Saves with Atomic + Backup + Integrity
- Context: Need a minimal, robust save pipeline that works across Unity platforms and detects corruption.
- Decision: Serialize a payload+envelope using `JsonUtility`, write via temp + replace, keep a `.bak` fallback, and store a SHA-256 hash of the payload for integrity checks.
- Consequences: Saves are portable, atomic, and corruption-evident without introducing encryption or cloud dependencies.
- Alternatives considered: Direct payload-only saves; no integrity check; non-atomic overwrites.

## ADR-0010: ScriptableObject App Catalog
- Context: Apps need to be content-driven without hardcoded lists in UI controllers.
- Decision: Define `AppDefinitionSO` assets and an `AppCatalogSO` that lists installed apps, with the taskbar building buttons from the catalog at runtime.
- Consequences: App lists can be edited via assets without code changes while keeping single-instance launch behavior.
- Alternatives considered: Hardcoded app lists; data in JSON/config files.

## ADR-0011: Simulated Virtual File System
- Context: File Manager needs a fictional file system without touching the host OS.
- Decision: Implement an in-memory VFS with directories/files, path resolution, and a default factory seeded under `/home/user`.
- Consequences: UI can browse a consistent, testable file tree that is safe and deterministic.
- Alternatives considered: Accessing the real filesystem; static/global data without path resolution.

## ADR-0012: Terminal Command Layer Over VFS
- Context: The Terminal needs basic shell-like commands without executing real OS operations.
- Decision: Implement a pure C# command parser/executor that operates on the in-memory VFS (help/pwd/ls/cd/cat/clear).
- Consequences: Terminal behavior is deterministic, safe, and testable while remaining entirely fictional.
- Alternatives considered: Executing OS commands; scripting with external interpreters.
