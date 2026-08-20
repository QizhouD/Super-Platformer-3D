# Changelog

## [Unreleased] - 2026-08-19

### Added

- Extended PCG_LAB generation without replacing the chunk pipeline:
  - player-derived reach model with a 0.82 safety factor;
  - start / easy / challenge / recovery / reward rhythm weights;
  - safest-chunk fallback when placement retries fail;
  - reuse of existing `crate-box` and `chest` prefabs on combat/recovery slots;
  - Scene gizmos for chunk bounds, connections, and reach.

- Upgraded `PCG_Lab` presentation without changing generation, observations, or control:
  - dusk canyon lighting, fog, mesa backdrop, and category-colored platforms;
  - checkpoint beacons, recovery markers, and timed-platform warning pulse;
  - runtime UGUI HUD that keeps seed, generate, ability, training, and dataset controls;
  - jump / dash / checkpoint / respawn / generate SFX plus a light ambient bed.

## [Unreleased] - 2026-07-27

### Added

- Added the first trainable `PCGNavigationAgent`:
  - 20 normalized navigation observations plus the 84x84 RGB sensor;
  - continuous movement and discrete jump/dash actions;
  - checkpoint, completion, death, time, and target-approach rewards;
  - human heuristic and external trainer control modes.
- Added an external-control channel to `InputReader` so ML actions reuse Player 2's
  existing movement, jump, dash, and state-machine implementation.
- Added `Training/pcg_navigation_ppo.yaml` and Lab training-mode controls.
- Added two EditMode tests for normalized navigation observations and target encoding.
- Installed the Unity ML-Agents training stack compatible with Unity 2022.3:
  - `com.unity.ml-agents 2.0.1`;
  - project-local Python 3.9 virtual environment;
  - `mlagents` / `mlagents-envs 0.30.0`;
  - PyTorch `1.11.0+cpu`;
  - compatibility-pinned Protobuf `3.20.3` and TensorBoard `2.11.2`.
- Added reproducible trainer requirements and setup notes under `Training/`.
- Added the `Unity.ML-Agents` reference to the PCG runtime assembly.
- Added `PCGMultimodalDatasetRecorder` for training-data collection:
  - JSONL structured observations and reward signals;
  - synchronized 84x84 PNG frames;
  - idle, traversal, airborne, falling, and recovery behavior labels;
  - episode summaries containing seed, completion, resets, return, and final
    adaptive difficulty state.
- Added dataset recording controls and output-path access to the PCG debug panel.
- Added three EditMode tests for behavior labeling, reward shaping, and episode
  metadata serialization.
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

- EditMode tests: **26 passed, 0 failed**.
- Default seed: `82431`.
- Runtime generation: **16 chunks and 16 checkpoints**.
- Multimodal observation smoke test:
  - structured vector: `20` values;
  - visual frame: `84x84`, allocated and updating;
  - initial skill/bias: `0.500 / 0.000`;
  - fast clean sample: skill `0.625`, bias `+0.050`;
  - first target difficulty changed from `0.150` to `0.200`.
- Dataset recording smoke test:
  - `120` structured JSONL samples;
  - `48` synchronized PNG frames;
  - episode summary written successfully;
  - no Play Mode runtime errors.
- ML-Agents navigation smoke test:
  - behavior: `PCGNavigation`;
  - observations: `20` normalized values plus an allocated `84x84` RGB sensor;
  - actions: `2` continuous axes plus `2` binary branches;
  - decision period: `5`;
  - training toggle switches `HeuristicOnly -> Default -> HeuristicOnly`;
  - PPO configuration parsed successfully by ML-Agents `0.30.0`.
- Measured layout:
  - X range: approximately `61.9m`
  - Z range: approximately `48.1m`
  - Y range: approximately `8m`
  - Heading variants: `4`
- Runtime material audit: no unexpected platform materials.
- Play Mode smoke test: generation completed with no new runtime errors.

### Suggested commit

```text
feat(ml-agents): add trainable pcg navigation agent

- add Player 2 external input bridge for policy control
- add vector and visual observations with hybrid actions
- add checkpoint, completion, death and approach rewards
- add human/training mode toggle and PPO configuration
- extend EditMode coverage to 26 passing tests
```

### Commit scope note

Do not include `.claude/settings.local.json`; it is a local untracked settings file
unrelated to this PCG update.
