using System;
using System.Diagnostics.CodeAnalysis;

namespace Final;

public struct BondSpring
{
    public Atom Atom1;
    public Atom Atom2;
    public float RefDistSqr;
    public float RefDist;
    public float currentDist;
    public float Tolerance = 0.001f;
    public int ConnectionNumber;
    public float Strength;
    public float MaxSqr;
    public float MinSqr;

    public BondSpring(Atom atom1, Atom atom2, int connectionNumber)
    {
        Atom1 = atom1;
        Atom2 = atom2;
        ConnectionNumber = connectionNumber;
        
        float xdist = atom1.Position.X - atom2.Position.X;
        float ydist = atom1.Position.Y - atom2.Position.Y;
        float zdist = atom1.Position.Z - atom2.Position.Z;
        
        RefDistSqr = (xdist * xdist) + (ydist * ydist) + (zdist * zdist);
        RefDist = MathF.Sqrt(RefDistSqr);
        MaxSqr = (RefDist + Tolerance) * (RefDist + Tolerance);
        MinSqr = (RefDist - Tolerance) * (RefDist - Tolerance);

        switch (connectionNumber)
        {
            case 1:
                Strength = 1f;
                break;
            case 2:
                Strength = 1f;
                break;
            case 3:
                Strength = 1f;
                break;
            default:
                Strength = 0.1f;
                break;
        }
        
        
    }

    public void CheckDistances()
    {
        float xdist = Atom1.Position.X - Atom2.Position.X;
        float ydist = Atom1.Position.Y - Atom2.Position.Y;
        float zdist = Atom1.Position.Z - Atom2.Position.Z;
        
        float currentDistSqr = (xdist * xdist) + (ydist * ydist) + (zdist * zdist);
        if (currentDistSqr > MaxSqr || currentDistSqr < MinSqr)
        {
            currentDist = MathF.Sqrt(currentDistSqr);
            ApplySpringPhysics(xdist, ydist, zdist);
        }
    }

    public void ApplySpringPhysics(float dx, float dy, float dz)
    {
        float diff = (RefDist - currentDist) / currentDist;
        float moveSpeed = diff * 0.5f * Strength;
        
        float offsetX = dx * moveSpeed;
        float offsetY = dy * moveSpeed;
        float offsetZ = dz * moveSpeed;
        
        Atom1.Position.X += offsetX;
        Atom1.Position.Y += offsetY;
        Atom1.Position.Z += offsetZ;
        
        Atom2.Position.X -= offsetX;
        Atom2.Position.Y -= offsetY;
        Atom2.Position.Z -= offsetZ;
    }
    
    
    
}