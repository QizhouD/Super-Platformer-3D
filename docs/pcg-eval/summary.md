# PCG generation report

Written by `Platformer > PCG > Write Generation Report` (batch mode: `-executeMethod PCGGenerationReport.WriteBatchMode`). Do not edit by hand.

Seeds 1-100 for each ability profile, 16 chunks per level. Reach comes from the `Player 2` prefab: 5.02m horizontal and 0.42m vertical without abilities. Quality columns average completed levels only; raw rows are in `levels.csv`.

| Abilities | Completed | Deterministic | Safe-fallback chunks | Mean difficulty error | Distinct chunks / level | Elevation span (m) | Heading changes / level |
|---|---|---|---|---|---|---|---|
| none | 99/100 | 100/100 | 0/1584 | 0.22 | 7.84 | 4.00 | 3.87 |
| double jump | 97/100 | 100/100 | 1/1552 | 0.21 | 9.32 | 6.34 | 4.22 |
| dash | 98/100 | 100/100 | 0/1568 | 0.21 | 8.13 | 4.00 | 3.71 |
| double jump + dash | 99/100 | 100/100 | 0/1584 | 0.20 | 9.51 | 6.22 | 4.09 |

## Seed 82431, no abilities

First line: target difficulty from the progress curve and rhythm plan. Second line: difficulty of the chunk actually placed.

```mermaid
xychart-beta
    x-axis "Chunk" [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
    y-axis "Difficulty" 0 --> 1
    line [0.15, 0.16, 0.18, 0.21, 0.25, 0.29, 0.34, 0.40, 0.45, 0.51, 0.56, 0.60, 0.64, 0.67, 0.69, 0.70]
    line [0.37, 0.10, 0.28, 0.27, 0.10, 0.37, 0.28, 0.27, 0.43, 0.10, 0.37, 0.27, 0.27, 0.37, 0.10, 0.04]
```

Chunks: `moving_01` → `basic_01` → `descend_01` → `turn_right_01` → `basic_01` → `moving_01` → `descend_01` → `turn_right_01` → `timed_01` → `basic_01` → `moving_01` → `turn_left_01` → `turn_right_01` → `moving_01` → `basic_01` → `recovery_01`
