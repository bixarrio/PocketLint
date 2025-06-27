using PocketLint.Core.Components;
using PocketLint.Core.Entities;

namespace MyDemoGame.Scripts;
public class PlayerHealth : GameScript
{
    public int Health { get; set; } = 10;
    public int MaxHealth { get; private set; } = 10;

    private List<SpriteRenderer> _renderers = new();

    public override void Init()
    {
        var sprites = (Health % 2 == 0 ? Health : Health + 1) / 2;
        for (var i = 0; i < sprites; i++)
        {
            var heart = Scene.CreateEntity($"Heart{i}", i * 8, 120); // attaching to root
            var renderer = Scene.AddComponent<SpriteRenderer>(heart);
            renderer.SpriteIndex = 102;
            renderer.Layer = 2; // UI Layer
            _renderers.Add(renderer);
        }
        UpdateHealthUI();
    }

    public void UpdateHealth(int amount)
    {
        Health = Math.Clamp(Health + amount, 0, MaxHealth);
        if (Health == 0) { /* Ded. Do something */ }
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        var health = Health;
        for (var i = 0; i < _renderers.Count; i++)
        {
            var mine = Math.Min(2, health);
            _renderers[i].SpriteIndex = (byte)(100 + mine);
            health -= mine;
        }
    }
}
