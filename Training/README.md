# ML-Agents training environment

The Unity project uses `com.unity.ml-agents` `2.0.1`, the stable package released
for Unity 2022.3. The matching Python trainer is ML-Agents `0.30.0`.

The local virtual environment is stored in `.venv-mlagents` and should remain
untracked. It uses the official CPU build of PyTorch `1.11.0`, which matches the
trainer's supported version range and works independently of the machine's CUDA
driver. Protobuf is pinned to `3.20.3` for compatibility with the communicator code
generated for ML-Agents `0.30.0`; TensorBoard is pinned to `2.11.2` to preserve
that Protobuf constraint. `six` is explicit because PyTorch 1.11 imports it without
declaring it in the Windows wheel metadata.

## Activate on Windows

```powershell
.\.venv-mlagents\Scripts\Activate.ps1
mlagents-learn --help
```

Do not use the machine's Python 3.12 or 3.13 installations for this trainer.

## Train the PCG navigation agent

1. Open `Assets/_Project/Scenes/PCG_Lab.unity`.
2. In a terminal, run:

```powershell
.\.venv-mlagents\Scripts\Activate.ps1
mlagents-learn Training\pcg_navigation_ppo.yaml --run-id=pcg-navigation-v1
```

3. When the trainer reports that it is listening, enter Play Mode.
4. Enable `ML-Agents Training Mode` in the debug panel.

Keep the Unity Editor in Play Mode while training. Outputs are written to
`Training/results/` and are ignored by Git.
