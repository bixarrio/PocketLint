using System.Collections.Generic;

namespace PocketLint.Core.Systems.GameLoopSystems;
public class PlayerUpdateLoop : ISubSystem
{
    public List<ISubSystem> SubSystems { get; set; } = new();

    public void Update() { /* container sub-system, so no-op */ }
}
