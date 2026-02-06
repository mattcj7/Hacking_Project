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
