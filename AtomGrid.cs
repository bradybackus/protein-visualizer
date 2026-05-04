using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Final;

public class AtomGrid
{
    public Dictionary<int, Atom> HashMap;
    public int AtomsPerCell = 8;
    public float CellLength = 8; 


    public AtomGrid(int numAtoms, Vector3 min, Vector3 max, List<Residue> residues)
    {
        HashMap = new Dictionary<int, Atom>();
        float targetCells = numAtoms / (float)AtomsPerCell;
        CellLength = MathF.Cbrt(FindVolume(max, min) / targetCells);

        foreach (Residue residue in residues)
        {
            foreach (Atom atom in residue.Atoms)
            {
                Hash(atom);
            }
        }
    }

    
    // Just gonna talk about what each function does bc theres a lot to explain.
    // This finds the volume of a cube with corners position1 and position2, needed to calculate
    // the full volume and then volume of each cell, so that for each protein we can
    // take the cube root and determine the length of each cell
    public float FindVolume(Vector3 position1, Vector3 position2)
    {
        float width = Math.Abs(position1.X - position2.X);
        float height = Math.Abs(position1.Y - position2.Y);
        float depth = Math.Abs(position1.Z - position2.Z);
        
        return width * height * depth;
    }

    
    // Every atom will check this every frame, but returns if nothing changed just uses their coordinates
    // and a hash method with multiple large prime numbers to create a unique key for a 
    // range of coordinates. if theres nothing at that hash in the dict, that atom becomes
    // the head and if there is something at the hash, it moves the current head back and makes
    // itself the head. its a linked list implementation so that moving between cells is O(1) time
    // if you get confused about the .LinkedListNext.LinkedListPrevious dw I coded it and I did too
    public void Hash(Atom atom)
    {
        int HashValue = HashCalc(atom.Position);

        if (atom.CurrentCellKey == HashValue)
        {
            return;
        }
        
        if (atom.CurrentCellKey != -1)
        {
            if (HashMap[atom.CurrentCellKey] == atom)
            {
                if (atom.LinkedListNext != null)
                {
                    HashMap[atom.CurrentCellKey] = atom.LinkedListNext;
                    atom.LinkedListNext.LinkedListPrevious = null;
                }
                else
                {
                    HashMap[atom.CurrentCellKey] = null;
                }
            }
            else
            {
                if (atom.LinkedListNext != null)
                {
                    atom.LinkedListNext.LinkedListPrevious = atom.LinkedListPrevious;
                    atom.LinkedListPrevious.LinkedListNext = atom.LinkedListNext;
                }
                else
                {
                    atom.LinkedListPrevious.LinkedListNext = null;
                }
            }
            atom.LinkedListPrevious = null;
            atom.LinkedListNext = null;
        }
        if (!HashMap.ContainsKey(HashValue))
        {
            HashMap[HashValue] = atom;
        }
        else if (HashMap[HashValue] == null)
        {
            HashMap[HashValue] = atom;
        }
        else
        {
            HashMap[HashValue].LinkedListPrevious = atom;
            atom.LinkedListNext = HashMap[HashValue];
            HashMap[HashValue] = atom;
        }
        
        atom.CurrentCellKey = HashValue;
        
    }


    public int HashCalc(Vector3 position)
    {
        int i = (int)MathF.Floor(position.X / CellLength);
        int j = (int)MathF.Floor(position.Y / CellLength);
        int k = (int)MathF.Floor(position.Z / CellLength);
        // These are just rlly big prime numbers
        int HashValue = (i*73856093) ^  (j*19349663) ^ (k*83492791);
        return HashValue;
    }

}