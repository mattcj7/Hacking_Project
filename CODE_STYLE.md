# Code Style (C# / Unity)

- Use namespaces
- Prefer composition over inheritance
- Avoid singletons unless justified and documented
- No giant MonoBehaviours: split responsibilities
- Public fields only when necessary; otherwise [SerializeField] private
- Keep Update() lean; favor events/timers/state machines
