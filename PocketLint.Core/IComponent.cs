namespace PocketLint.Core;

public interface IComponent
{
    GameObject GameObject { get; }
    void Update(float dt);
    void Draw();
}