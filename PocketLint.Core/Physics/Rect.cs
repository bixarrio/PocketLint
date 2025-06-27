using System;

namespace PocketLint.Core.Physics;
public struct Rect
{
    #region Properties and Fields

    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public readonly float MinX => X;
    public readonly float MaxX => X + Width;
    public readonly float MinY => Y;
    public readonly float MaxY => Y + Height;

    #endregion

    #region ctor

    public Rect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width >= 0 ? width : throw new ArgumentException("Width must be non-negative");
        Height = height >= 0 ? height : throw new ArgumentException("Height must be non-negative");
    }

    #endregion
}
