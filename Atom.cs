using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final;

public class Atom
{
    public List<Atom> Neighbors;
    public HashSet<int> NeighborIds = new HashSet<int>();
    public Vector3 Position;
    public Vector3 OldPosition;
    public Residue Residue;
    public AtomType Type;
    public ElementType Element;
    public Color Color;
    public int Id;
    public int CurrentCellKey = -1;
    public Atom LinkedListNext;
    public Atom LinkedListPrevious;
    public string Name;
    public AtomGrid HashMap;
    public float Dragging = 0f;
    public bool Shown = true;
    private Vector3 _dragOffset;
    public float Friction = 0f;
    

    public Atom(Vector3 position, Residue residue, AtomType atomType, string atomName, ElementType element, int id)
    {
        Neighbors = new List<Atom>();
        Position = position;
        OldPosition = position;
        Residue = residue;
        Type = atomType;
        Element = element; 
        Id = id;
        Name = atomName;
        SetColor();
    }

    public void Connect(Atom atom)
    {
        if (atom == null) //Validation in case atom doesnt exist in PDB file
        {
            return;
        }
        Neighbors.Add(atom); //2 way reference
        atom.Neighbors.Add(this);
        NeighborIds.Add(atom.Id);
        atom.NeighborIds.Add(Id);
    }

    public List<BondSpring> CalculateSprings()
    {
        List<BondSpring> springs =  new List<BondSpring>();
        HashSet<int> alreadyConnected = new HashSet<int>();
        bool needs14 = (Type == AtomType.CA || Type == AtomType.O);
        foreach (Atom neighbor in Neighbors)
        {
            if (neighbor == null)
            {
                continue;
            }
            if (neighbor.Id > Id &&  !alreadyConnected.Contains(neighbor.Id))
            {
                springs.Add(new BondSpring(this, neighbor, 1));
                alreadyConnected.Add(neighbor.Id);
            }
            foreach (Atom neighbor2 in neighbor.Neighbors)
            {
                if (neighbor2 != null && neighbor2.Id > Id && !alreadyConnected.Contains(neighbor2.Id))
                {
                    springs.Add(new BondSpring(this, neighbor2, 2));
                    alreadyConnected.Add(neighbor2.Id);
                    NeighborIds.Add(neighbor2.Id); 
                    neighbor2.NeighborIds.Add(Id);
                }
                if (!needs14)
                {
                    continue;
                }
                foreach (Atom neighbor3 in neighbor2.Neighbors)
                {
                    if ((neighbor3 == null || (neighbor3.Type != AtomType.CA && neighbor3.Type != AtomType.O) || neighbor3.Type == Type))
                    {
                        continue;
                    }

                    if (neighbor3 != null && neighbor3.Id > Id && !alreadyConnected.Contains(neighbor3.Id))
                    {
                        springs.Add(new BondSpring(this, neighbor3, 3));
                        alreadyConnected.Add(neighbor3.Id);
                    }
                    
                }
            }
        }
        return springs;
    }

    public void UpdateAtom()
    {
        float velX = (Position.X - OldPosition.X) * Friction;
        float velY = (Position.Y - OldPosition.Y) * Friction;
        float velZ = (Position.Z - OldPosition.Z) * Friction;
        
        OldPosition = Position;
        Position.X += velX;
        Position.Y += velY;
        Position.Z += velZ;
        HashMap.Hash(this);
    }
    
    public void StartDrag(Ray ray, float distance)
    {
        Dragging = distance;
        Vector3 hitPoint = ray.Position + (ray.Direction * Dragging);
        _dragOffset = Position - hitPoint;
    }
    public void DragAtom(Ray ray)
    {
        if (Dragging == 0f) { return; }
        Vector3 newPos = ray.Position + (ray.Direction * Dragging);
        Position = newPos +  _dragOffset;
        HashMap.Hash(this);
    }
    

    public void SetColor()
    {
        switch (Element)
        {
            case ElementType.N:
                Color = Color.Blue;
                break;
            case ElementType.C:
                Color = Color.Gray;
                break;
            case ElementType.O:
                Color = Color.Red;
                break;
            case ElementType.S:
                Color = Color.Yellow;
                break;
            case ElementType.Other:
                Color = Color.White;
                break;
        }
    }
    
}