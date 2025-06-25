using PocketLint.Core.Entities;

namespace PocketLint.Core.Components;

public interface IComponent
{
    uint EntityId { get; }
    EntityManager EntityManager { get; }
    EntityTransform Transform { get; }

    void Init();
    void Ready();
    void Update();
    void OnDestroy();

}
