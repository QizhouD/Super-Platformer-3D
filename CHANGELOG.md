# Changelog

## [Unreleased] - 2026-07-27

### Added

- Added a multimodal Game AI observation layer:
  - fixed 20-value structured state vectors at 5 Hz;
  - independent 84x84 RGB observations at 2 Hz;
  - JSON export of the latest structured observation.
- Added `PCGAdaptiveDifficultyDirector`, which estimates skill from checkpoint time
  and respawns, then adapts platform timing and future PCG target difficulty.
- Added telemetry events for adaptive difficulty decisions and live skill/bias
  readouts to the PCG debug panel.
- Added three EditMode tests for performance scoring, model smoothing, and observation
  vector stability.
- Added deterministic runtime behaviours for generated content:
  - `PCGOscillatingPlatform` for moving-platform challenges.
  - `PCGTimedPlatform` with visible, warning, hidden, and recovery phases.
- Added `PCGRunTelemetry` with bounded JSON-exportable event history and player snapshots.
  Telemetry covers generation completion, checkpoint progress, respawns, timed-platform
  transitions, player position, velocity, and generated chunk count.
- Added six spatial chunk archetypes:
  - `turn_left_01`
  - `turn_right_01`
  - `offset_left_01`
  - `climb_01`
  - `descend_01`
  - `climb_turn_left_01`
- Added spatial metadata for elevation, heading, and lateral displacement to
  `PlatformChunkData`.
- Added `Copy Telemetry` and live telemetry counters to the PCG debug panel.
- Added five EditMode tests for dynamic-platform timing, telemetry buffering, spatial
  variation requirements, and elevation-envelope filtering.

### Changed

- Increased the default generated level from 12 to 16 chunks.
- Added a spatial grammar that limits consecutive flat and straight chunks to three.
- Expanded the relative elevation envelope from `-2.5m...+6m` to `-4m...+8m`.
- Scaled horizontal platform placement by `1.25x` and platform footprints by `1.05x`
  to produce wider gaps and a larger play area.
- Updated traversal capability metadata to keep expanded gaps reachable by Player 2.
- Reused Tutorial presentation assets:
  - Normal and start platforms use `NoiseGround.mat`.
  - The disappearing timed surface uses `TimedPlatform.mat`.
- Expanded moving-platform travel distance and timing to match the larger layout.
- Extended generator and run-controller events so Game AI systems can observe
  generation, checkpoints, and respawns without polling scene objects.
- Updated generated prefabs, chunk data assets, `LevelGenerationConfig`, and
  `PCG_Lab.unity`.

### Fixed

- Prevented generated layouts from degenerating into a single straight, flat line.
- Filtered telemetry emitted by temporary chunk candidates rejected during generation.
- Kept generated routes inside a bounded vertical envelope.
- Preserved deterministic output, ability gating, reachability filtering, category
  limits, and overlap validation after the spatial expansion.

### Validation

- EditMode tests: **21 passed, 0 failed**.
- Default seed: `82431`.
- Runtime generation: **16 chunks and 16 checkpoints**.
- Multimodal observation smoke test:
  - structured vector: `20` values;
  - visual frame: `84x84`, allocated and updating;
  - initial skill/bias: `0.500 / 0.000`;
  - fast clean sample: skill `0.625`, bias `+0.050`;
  - first target difficulty changed from `0.150` to `0.200`.
- Measured layout:
  - X range: approximately `61.9m`
  - Z range: approximately `48.1m`
  - Y range: approximately `8m`
  - Heading variants: `4`
- Runtime material audit: no unexpected platform materials.
- Play Mode smoke test: generation completed with no new runtime errors.

### Suggested commit

```text
feat(game-ai): add multimodal observations and adaptive pcg difficulty

- add 20-value structured and 84x84 visual observations
- estimate player skill from checkpoint pace and respawns
- adapt dynamic-platform timing and future PCG difficulty
- expose Game AI state and observation export in the lab panel
- extend EditMode coverage to 21 passing tests
```

### Commit scope note

Do not include `.claude/settings.local.json`; it is a local untracked settings file
unrelated to this PCG update.
