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
