using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Microsoft.Xna.Framework.Audio;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Final;

public class Model
{
    public string Name;
    public float Radius;
    public Vector3 Center;
    public Vector3 Min = Vector3.Zero;
    public Vector3 Max = Vector3.Zero;
    public Matrix WorldSpace;
    public List<Residue> ProteinData;
    public List<AtomData> AtomsToDraw;
    public List<BondData> BondsToDraw;
    public List<BondSpring> Springs;
    public Atom[] Atoms;
    public AtomGrid HashGrid;

    public float CurrentDistance;
    public float LJRadius = 0.5f;

    private int _totalAtoms = 0;

    public bool Shown = false;
    public bool BackboneOnly = false;
    
    

    public Model(string name, List<Residue> proteinData)
    {
        AtomsToDraw = new List<AtomData>();
        BondsToDraw = new List<BondData>();
        Springs = new List<BondSpring>();
        Name = name;
        ProteinData = proteinData;

        WorldSpace = Matrix.Identity;

        Center = CalculateCenter(ProteinData);

        Vector3 furthestAtomPos = Vector3.Zero;
        foreach (var res in ProteinData)
        {
            foreach (var atom in res.Atoms) //Count up atoms and adjust position to center on 0,0
            {
                _totalAtoms++;
                atom.Position -= Center;
                if (atom.Position.LengthSquared() > furthestAtomPos.LengthSquared()) //Set radius via the furthest atom
                {
                    furthestAtomPos = atom.Position;
                }

                Min = Vector3.Min(atom.Position, Min);
                Max = Vector3.Max(atom.Position, Max);
            }
        }

        foreach (var res in ProteinData)
        {
            foreach (var atom in res.Atoms)
            {
                Springs.AddRange(atom.CalculateSprings());
            }
        }

        Atoms = new Atom[_totalAtoms];
        Radius = furthestAtomPos.Length();
        HashGrid = new AtomGrid(_totalAtoms, Min, Max, ProteinData);
        int counter = 0;
        foreach (var residue in ProteinData)
        {
            foreach (var atom in residue.Atoms)
            {
                atom.HashMap = HashGrid;
                Atoms[counter] = atom;
                counter++;
            }
        }
    }

    private Vector3 CalculateCenter(List<Residue> proteinData)
    {
        //Calculates center of protein
        Vector3 sum = Vector3.Zero;
        int count = 0;
        foreach (var res in proteinData)
        {
            foreach (var atom in res.Atoms)
            {
                sum += atom.Position;
                count++;
            }
        }

        if (count == 0)
        {
            return Vector3.Zero;
        }

        return sum / count;
    }

    public void DrawAtoms(Microsoft.Xna.Framework.Graphics.Model sphere, Matrix view, Matrix projection, 
        float atomScaler, Vector3 lightDirection)
    {
        ModelMesh mesh = sphere.Meshes[0];
        BasicEffect effect = (BasicEffect)mesh.Effects[0];
        effect.View = view;          // missing!
        effect.Projection = projection;  // missing!
        effect.EnableDefaultLighting();
        effect.DirectionalLight0.Direction = lightDirection;
        effect.DirectionalLight0.DiffuseColor = Vector3.One * 0.6f;  // was 1f
        effect.AmbientLightColor = Vector3.One * 0.2f;               // was 0.3f
        foreach (AtomData atom in AtomsToDraw)
        {
            effect.World = Matrix.CreateScale(atomScaler) * Matrix.CreateTranslation(atom.Position) * WorldSpace;
            effect.DiffuseColor = atom.Color.ToVector3();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                mesh.Draw();
            }
        }
    }

    public void DrawBonds(Microsoft.Xna.Framework.Graphics.Model bond, Matrix view, Matrix projection, 
        Vector3 lightDirection)
    {
        ModelMesh bondMesh = bond.Meshes[0];
        BasicEffect bondEffect = (BasicEffect)bondMesh.Effects[0];
        bondEffect.View = view;          // missing!
        bondEffect.Projection = projection;  // missing!
        bondEffect.EnableDefaultLighting();
        bondEffect.DirectionalLight0.Direction = lightDirection;
        bondEffect.DirectionalLight0.DiffuseColor = Vector3.One * 0.6f;  // was 1f
        bondEffect.AmbientLightColor = Vector3.One * 0.2f;               // was 0.3f

        foreach (BondData indvBond in BondsToDraw)
        {
            bondEffect.World = indvBond.World * WorldSpace;
            bondEffect.DiffuseColor = indvBond.Color.ToVector3();
            foreach (EffectPass pass in bondEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                bondMesh.Draw();
            }
        }
    }

    public void
        UpdateAtomData(GameTime gameTime) //Recalculated every frame to account for physics changing atom positions
    {
        var firstAtom = ProteinData[0].Atoms[0];

        foreach (Residue res in ProteinData)
        {
            foreach (Atom atom in res.Atoms)
            {
                atom.UpdateAtom();
                atom.Friction = 0.8f;
            }
        }

        for (int i = 0; i < Springs.Count; i++)
        {
            ref BondSpring spring = ref CollectionsMarshal.AsSpan(Springs)[i];
            spring.CheckDistances();
        }

        foreach (Atom atom in Atoms)
        {
            CheckCollision(atom);
        }

        AtomsToDraw.Clear();
        BondsToDraw.Clear();
        foreach (Residue res in ProteinData)
        {
            AtomsToDraw.AddRange(res.GetAtomVisuals(BackboneOnly));
            BondsToDraw.AddRange(res.GetBondVisuals(BackboneOnly));
        }
    }

    public void CheckCollision(Atom atom)
    {
        float minDist = 2f;
        float minDistSqr = minDist * minDist;

        if (!HashGrid.HashMap.TryGetValue(atom.CurrentCellKey, out Atom current))
        {
            return;
        }

        while (current != null)
        {
            if (current.Id > atom.Id && !atom.NeighborIds.Contains(current.Id))
            {
                float dx = atom.Position.X - current.Position.X;
                float dy = atom.Position.Y - current.Position.Y;
                float dz = atom.Position.Z - current.Position.Z;
                float distSqr = dx*dx + dy*dy + dz*dz;

                if (distSqr < minDistSqr && distSqr > 0)
                {
                    float dist = MathF.Sqrt(distSqr);
                    float push = (minDist - dist) * 0.5f / dist;
                    atom.Position.X += dx * push;
                    atom.Position.Y += dy * push;
                    atom.Position.Z += dz * push;
                    current.Position.X -= dx * push;
                    current.Position.Y -= dy * push;
                    current.Position.Z -= dz * push;
                }
            }
            current = current.LinkedListNext;
        }
    }
    
}
