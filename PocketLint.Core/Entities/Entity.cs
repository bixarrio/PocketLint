using PocketLint.Core.Components;
using System.Collections.Generic;

namespace PocketLint.Core.Entities;

public class Entity
{
    #region Properties and Fields

    public uint Id { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public List<IComponent> Components { get; set; }

    #endregion
}
