using System;

namespace Platformer.PCG {
    public static class PCGLabSignals {
        public static event Action JumpStarted;
        public static event Action DashStarted;

        public static void RaiseJumpStarted() => JumpStarted?.Invoke();
        public static void RaiseDashStarted() => DashStarted?.Invoke();
    }
}
