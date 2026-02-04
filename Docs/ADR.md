# ADR (Architecture Decision Records)

Add entries like:

## ADR-0001: Module Layout for Core Assemblies
- Context: Need clear assembly boundaries to keep compile times fast and dependencies predictable.
- Decision: Create four runtime assemblies (Infrastructure, Systems, Game, UI) with one-way references: Systems -> Infrastructure; Game/UI -> Infrastructure + Systems. Add EditMode and PlayMode test assemblies referencing runtime modules.
- Consequences: Clear dependency direction and modular growth; tests compile against specific modules.
- Alternatives considered: Single monolithic assembly; feature-only splits without an infrastructure base.