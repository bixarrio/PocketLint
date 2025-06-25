using PocketLint.Core.Components;
using PocketLint.Core.TimeSystem;
using System.Collections;

namespace MyDemoGame;
public class EnemyController : Script
{

    public override void Ready()
    {
        StartCoroutine(MoveInCircle(15, 5));
    }

    private IEnumerator MoveInCircle(float radius, float speed)
    {
        float angle = 0f;
        float centerX = Transform.LocalX;
        float centerY = Transform.LocalY;

        while (true) // Loop forever
        {
            angle += speed * Time.DeltaTime;
            float x = centerX + MathF.Sin(angle) * radius;
            float y = centerY + MathF.Cos(angle) * radius;

            Transform.LocalX = x;
            Transform.LocalY = y;
            yield return null; // Wait one frame
        }
    }
}
