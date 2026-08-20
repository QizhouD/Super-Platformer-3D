# Documentation

Project documentation for Super Platformer 3D. GitHub-facing docs live here.
Unity still versions `Assets/` and `Packages/` markdown that belongs to plugins
or sits next to gameplay assets.

| File | Contents |
|---|---|
| [agents.md](agents.md) | Architecture, namespaces, conventions for coding agents |
| [pcg.md](pcg.md) | PCG Lab generation, reachability, training contract |
| [training.md](training.md) | ML-Agents Python trainer setup |
| [changelog.md](changelog.md) | Feature history |
| [grok-handoff.md](grok-handoff.md) | Unity CLI / Grok handoff notes |

Root `README.md` is the repository landing page. `AGENTS.md` at the repo root
points here so agent tools still discover the project guide.

Not moved (third-party or Unity assets):

- `.claude/skills/unity-style-guide/SKILL.md`
- `Assets/_Project/PCG/README.md` (also copied to `pcg.md`)
- `Training/README.md` (also copied to `training.md`)
- `Assets/_Project/NavMeshComponents-master/Documentation/`
- `Packages/com.bezi.sidekick/README.md`
