using Microsoft.Xna.Framework;

namespace Final;

public struct AtomData //From What I understand, a struct is faster when we have so many models to draw 
{
    public Vector3 Position;
    public Color Color;

    public AtomData(Vector3 position, Color color)
    {
        Position = position;
        Color = color;
    }
}