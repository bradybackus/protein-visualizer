using Microsoft.Xna.Framework;

namespace Final;

public struct BondData //From What I understand, a struct is faster when we have so many models to draw 
{
    public Matrix World;
    public Color Color;
    
    public BondData(Matrix world, Color color)
    {
        World = world;
        Color = color;
    }
}