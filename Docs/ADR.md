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

## ADR-0014: Persist OS Session State
- Context: The OS shell should restore open apps, window positions, and last-used paths between sessions.
- Decision: Store minimal session data in `SaveGameData` (open windows + paths) and let UI capture/restore via `SaveSessionCaptureEvent` without global access.
- Consequences: Session restoration is deterministic and data-driven while keeping the save format minimal.
- Alternatives considered: No session persistence; full app state serialization.

## ADR-0015: Event-Driven Mission System v1
- Context: Need a minimal mission system that reacts to simulated Terminal/VFS interactions and is visible in the UI.
- Decision: Add ScriptableObject-driven missions (`MissionDefinitionSO` + `MissionCatalogSO`) and a `MissionService` that subscribes to `TerminalCommandExecutedEvent`/`FileManagerOpenedFileEvent` and publishes mission lifecycle events. Provide a Missions app window to display the active mission and objective state.
- Consequences: Missions are data-driven and evaluated without polling, keeping systems decoupled and testable.
- Alternatives considered: Hardcoded mission checks in UI; polling the VFS/Terminal state each frame.

## ADR-0016: Wallet Credits + Mission Rewards + Chaining
- Context: Missions need a minimal progression loop with rewards and simple sequencing.
- Decision: Add a `WalletService` that tracks credits and publishes `CreditsChangedEvent`. Extend missions with `RewardCredits`, award them on completion, and auto-chain to the next mission in catalog order while tracking completed missions for UI.
- Consequences: Credits and mission flow are instance-based and event-driven without adding global state.
- Alternatives considered: No rewards; manual mission selection UI only; storing rewards in static singletons.

## ADR-0016A: Missions UI Scroll Layout
- Context: Missions UI content can overlap when content grows beyond the window size.
- Decision: Wrap missions content in a ScrollView and use simple VisualElements/Labels for objectives and completed lists.
- Consequences: Missions UI is resilient to long content without virtualization artifacts.
- Alternatives considered: ListView virtualization; static layout without scrolling.

## ADR-0016B: Notifications, Mission Progression UI, Window Resizing
- Context: Need player feedback for rewards, clearer mission progression, and resizable windows.
- Decision: Add a `NotificationService` that publishes toasts, add a Missions “Start Next Mission” button, and add a resize handle to the window template with pointer-based resizing.
- Consequences: UI feedback and window interaction feel more responsive without per-frame polling.
- Alternatives considered: No toast system; always auto-start next mission; fixed window sizes only.
- Process: Every completed ticket adds a short ADR entry.

## ADR-0016C: Window Instance Safety + Drag/Resize Clamp
- Context: Window instances must behave independently and remain reachable on screen.
- Decision: Keep per-window drag/resize state, clone window visuals per instance, take windows out of layout flow (absolute positioning), isolate resize by capturing pointer on the handle and stopping event propagation, and clamp window positions with a minimum-visible threshold so titlebars remain reachable.
- Consequences: Windows remain independently draggable/resizable without getting lost off-screen.
- Alternatives considered: Shared drag state; no clamping; fixed window positions.

## ADR-0016D: Root-Captured Resize Events
- Context: Resize handle pointer events can miss moves/ups when the cursor leaves the tiny handle during drag.
- Decision: Start resize on the handle but capture pointer on the window root and listen for move/up there to ensure reliable resizing.
- Consequences: Resizing remains smooth even when the pointer leaves the handle area.
- Alternatives considered: Handle-only events; global input polling.

## ADR-0016E: Resize Hot Zone + Hover Highlight
- Context: Resizing can feel finicky when the handle is small or hard to target.
- Decision: Add an 18px bottom-right hot zone that highlights the handle on hover and allows resize start from the root while logging debug start/end in Editor/Development builds.
- Consequences: Easier resize targeting with clear feedback and traceable resize sessions during development.
- Alternatives considered: Larger handle only; no hover feedback; always-on logs.

## ADR-0016F: Clear Right/Bottom Constraints for Resizing
- Context: Window roots can inherit right/bottom constraints that prevent width/height changes from taking effect.
- Decision: Explicitly clear right/bottom to `auto` on creation and position updates so resizing applies consistently.
- Consequences: Resizing honors explicit width/height without layout constraints overriding.
- Alternatives considered: Editing USS constraints only; forcing layout rebuilds.

## ADR-0016G: Window Frame as Resize Target
- Context: Resizing the template root does not always update the visible window frame.
- Decision: Introduce a `window-frame` element and apply size changes to that frame (mirrored to the root for input hit-testing).
- Consequences: Visual resizing matches the computed size updates.
- Alternatives considered: Only resizing the root; USS changes alone.

## ADR-0017: Store Purchase + Install Pipeline
- Context: The OS needs a fictional store loop with purchases, installs, and persistence.
- Decision: Add Store/Install services that mutate save data, publish events, and use a package database to resolve AppDefinition assets. UI surfaces a store list with buy/install actions and notifications.
- Consequences: New apps can be purchased, installed, and persist across sessions without real-world integration.
- Alternatives considered: Hard-coded installs; no persistence; direct AppRegistry edits from UI.

## ADR-0017A: Store/Install Boundary Fix (Systems + UI Reaction)
- Context: Store/Install logic referenced UI types from Infrastructure, breaking assembly boundaries and causing compile errors.
- Decision: Move Store/Install runtime code into Systems, replace AppId usage with string appIds in store data/events, and keep AppRegistry/AppPackageDatabase resolution in the UI layer by reacting to install events.
- Consequences: Systems remains UI-agnostic while UI updates taskbar/app lists when installs complete.
- Alternatives considered: Allow Infrastructure to reference UI; keep UI types in Systems.

## ADR-0018: Installer File Pipeline
- Context: Store installs should feel like OS activity rather than immediate app activation.
- Decision: On purchase, create a JSON `.installer` file in the VFS downloads folder and trigger installs via File Manager confirm or a Terminal `install` command, with store UI showing downloaded state only.
- Consequences: Installs are a distinct step, driven by VFS files and command/UI actions, while remaining fully simulated.
- Alternatives considered: Direct installs from Store UI; skipping installer files entirely.
