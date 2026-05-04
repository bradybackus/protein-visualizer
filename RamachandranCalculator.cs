using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Final;

public class RamachandranCalculator
{
    // phi: angle between alpha carbon and N
    // psi: angles between alpha carbon and carbon with the double bond O 
    
    public static List<(float phi, float psi, int residueId)> GetAngles(List<Residue> proteinData)
    {
        var angles = new List<(float phi, float psi, int residueId)>();

        // goes though all amino acids, except first and last (because they are missing a neighboring amino acid on one
        // side) 
        for (int i = 1; i < proteinData.Count - 1; i++)
        {
            // stores previous, current and next amino acid 
            Residue prev = proteinData[i - 1];
            Residue curr = proteinData[i];
            Residue next = proteinData[i + 1];

            // safety check 
            // skips if residue is missing any atoms needed to calculate angle
            if (prev.C == null || curr.N == null || curr.CA == null || curr.C == null || next.N == null)
                continue;

            // ensures resided are bonded
            if (curr.Previous != prev || curr.Chain != prev.Chain)
                continue;

            float phi = CalcDihedral(prev.C.Position, curr.N.Position, curr.CA.Position, curr.C.Position);
            float psi = CalcDihedral(curr.N.Position, curr.CA.Position, curr.C.Position, next.N.Position);

            angles.Add((phi, psi, curr.ID));
        }

        return angles;
    }

    // takes in 4 points from 3D space
    // there are 2 planes, one plane has points A, B and C, the other plane has points B, C, and D
    // find the angle between the 2 planes 
    private static float CalcDihedral(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vector3 b1 = b - a;     // vector from a to b
        Vector3 b2 = c - b;     // vector from b to c
        Vector3 b3 = d - c;     // vector from c to d

        // finding normal to the plane using cross product 
        Vector3 n1 = Vector3.Cross(b1, b2);
        Vector3 n2 = Vector3.Cross(b2, b3);

        // if cross product is close to 0 return NaN
        if (n1.LengthSquared() < 1e-10f || n2.LengthSquared() < 1e-10f)
            return float.NaN;

        // normalizing 
        n1 = Vector3.Normalize(n1);
        n2 = Vector3.Normalize(n2);
        
        // m1 is perpendicular to n1 and b2 and acts as a reference
        Vector3 m1 = Vector3.Cross(n1, Vector3.Normalize(b2));

        float x = Vector3.Dot(n1, n2);
        float y = Vector3.Dot(m1, n2);

        // converting degree to radians 
        return -MathF.Atan2(y, x) * (180f / MathF.PI);
    }
    
    public static void PrintAnglesToConsole(List<Residue> proteinData)
    {
        List<(float phi, float psi, int residueId)> angles = GetAngles(proteinData);
        int valid = 0, skipped = 0;
    
        foreach ((float phi, float psi, int residueId) in angles)
        {
            if (float.IsNaN(phi) || float.IsNaN(psi))
            {
                skipped++;
                continue;
            }
            Console.WriteLine($"Residue {residueId}: phi={phi:F2}, psi={psi:F2}");
            valid++;
        }
        Console.WriteLine($"Valid points: {valid}, Skipped (NaN): {skipped}");
    }

}