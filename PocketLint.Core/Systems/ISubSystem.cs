using System.Collections.Generic;

namespace PocketLint.Core.Systems;
public interface ISubSystem
{
    List<ISubSystem> SubSystems { get; set; }
    void Update();
}
