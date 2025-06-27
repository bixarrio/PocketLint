using PocketLint.Core.Components;
using PocketLint.Core.TimeSystem;
using System.Collections;

namespace MyDemoGame.Scripts;
public class EnemyController : GameScript
{

    public override void Ready()
    {
        StartCoroutine(MoveInCircle(15, 5));
    }

    private IEnumerator MoveInCircle(float radius, float speed)
    {
        var angle = 0f;
        var centerX = Transform.LocalX;
        var centerY = Transform.LocalY;

        while (true) // Loop forever
        {
            angle += speed * Time.DeltaTime;
            var x = centerX + MathF.Sin(angle) * radius;
            var y = centerY + MathF.Cos(angle) * radius;

            Transform.LocalX = x;
            Transform.LocalY = y;
            yield return null; // Wait one frame
        }
    }
}
