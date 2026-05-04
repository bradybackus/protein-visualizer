using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Final;

public class Residue
{
    public Atom N;
    public Atom CA;
    public Atom C;
    public Atom O;
    public Atom CB;
    public List<Atom> Atoms;
    public Dictionary<string, Atom> AtomMap;
    public AminoAcidType AAType;
    public int ID;
    public string Chain;
    public Residue Previous;
    public Color BackBoneColor = Color.Gainsboro;
    public Color SideChainColor = Color.Chocolate;
    public Color OriginalBBColor = Color.Gainsboro;
    public Color OriginalSCColor = Color.Chocolate;
    public float BondWidth = 0.25f;
    
    AtomType [] BackBoneAtoms = new AtomType[] { AtomType.N, AtomType.C, AtomType.CA};

    public Residue(AminoAcidType aatype, int id, string chain)
    {
        AAType = aatype;
        ID = id;
        Chain = chain;
        Atoms = new List<Atom>();
        AtomMap = new Dictionary<string, Atom>();
    }

    public void AddAtom(Atom atom) // Called in Data Parser to handle type assignments and dictionary mapping
    {
        AtomMap[atom.Name] = atom;
        Atoms.Add(atom);
        switch (atom.Type)
        {
            case AtomType.N:
                N = atom;
                break;
            case AtomType.CA:
                CA = atom;
                break;
            case AtomType.C:
                C = atom;
                break;
            case AtomType.O:
                O = atom;
                break;
        }
    }

    public void SetColor()
    {
        BackBoneColor = OriginalBBColor;
        SideChainColor = OriginalSCColor;
    }
    
    public List<AtomData> GetAtomVisuals(bool backboneOnly = false) //Takes all atoms data, puts it into a list of structs, faster for the GPU
    {
        List<AtomData> visuals = new List<AtomData>();
        foreach (var atom in Atoms)
        {
            if (backboneOnly && !BackBoneAtoms.Contains(atom.Type))
            {
                atom.Shown = false;
                continue;
            }
            visuals.Add(new AtomData(atom.Position, atom.Color));
            atom.Shown = true;
        }
        return visuals;
    }
    
    public List<BondData> GetBondVisuals(bool backboneOnly = false) //Takes all bond data, puts it into a list of structs, faster for the GPU
    {
        List<BondData> visuals = new List<BondData>();
        foreach (var atom in Atoms)
        {
            if (backboneOnly && !BackBoneAtoms.Contains(atom.Type))
                continue;
            foreach (var neighbor in atom.Neighbors) //Go through each connection in each atom
            {
                //Each atom knows what its connected to, so to avoid two bonds for one connection, check ID
                if (atom.Id > neighbor.Id) {continue;}
                if (backboneOnly && !BackBoneAtoms.Contains(neighbor.Type)) {continue;}
                Color color;
                if (atom.Type != AtomType.Other && neighbor.Type != AtomType.Other && neighbor.Type != AtomType.O) //Handle Backbone and Side Chain colors
                {
                    color = BackBoneColor;
                }
                else
                {
                    color = SideChainColor;
                }
                visuals.Add(CalculateBond(atom.Position, neighbor.Position, color));
            }
        }
        return visuals;
    }

    private BondData CalculateBond(Vector3 start, Vector3 end, Color color) // Creates transformation matrix for each bond
    {
        Vector3 delta = end - start;
        float distance = delta.Length();
        Vector3 direction = Vector3.Normalize(delta);
        
        Quaternion rot = GetRotation(Vector3.UnitY, direction); //Helper Function
        Vector3 midpoint = (start + end) / 2f;
        
        //Standard Matrix multiplication/creation
        Matrix scale = Matrix.CreateScale(BondWidth, distance, BondWidth);
        Matrix rotation = Matrix.CreateFromQuaternion(rot);
        Matrix translation = Matrix.CreateTranslation(midpoint);
        
        return new BondData(scale * rotation * translation, color);
    }
    private Quaternion GetRotation(Vector3 source, Vector3 dest)
    {
        float dot = Vector3.Dot(source, dest); //Get dot product (How different the direction is from straight up and down)
        Vector3 cross = Vector3.Cross(source, dest); //Get the cross product (The vector 90 degrees from straight up and down and the direction), acts as axis of rotation
        float w = (float)Math.Sqrt(source.LengthSquared() * dest.LengthSquared()) + dot; // Calculate the W component using the half-angle identity (effectively finding the cosine of half the angle between the vectors).
        return Quaternion.Normalize(new Quaternion(cross.X, cross.Y, cross.Z, w)); //Create a Quaternion (rotation datatype I looked into). In short, a Quaternion takes the cross product (perpendicular line to up and the direction) as the axis of rotation, and w is "how much" should I rotate it
    }

    public void CreateConnections() //Go through and connect all atoms
    {
        if (Previous != null && Previous.ID == ID-1) //Make sure previous exists and is the residue immediately before this one
        {
            Previous.C.Connect(N);
        }
        N.Connect(CA);
        CA.Connect(C);
        C.Connect(O);
        ConnectSideChains();
        SafeConnect("C", "OXT");
    }

    private void ConnectSideChains() //Connection map for each type of side chain
    {
        if (AAType != AminoAcidType.GLY) //If it isnt glycine, it has a beta carbon
        {
            SafeConnect("CA", "CB");
        }
        switch (AAType)
        {
            case AminoAcidType.ARG: //Arginine
                SafeConnect("CB","CG");
                SafeConnect("CG","CD");
                SafeConnect("CD","NE");
                SafeConnect("NE","CZ");
                SafeConnect("CZ","NH1");
                SafeConnect("CZ","NH2");
                break;
            case AminoAcidType.ASN: //Asparagine
                SafeConnect("CB","CG");
                SafeConnect("CG","OD1");
                SafeConnect("CG","ND2");
                break;
            case AminoAcidType.ASP: //Aspartic Acid
                SafeConnect("CB","CG");
                SafeConnect("CG","OD1");
                SafeConnect("CG","OD2");
                break;
            case AminoAcidType.CYS: //Cysteine
                SafeConnect("CB","SG");
                break;
            case AminoAcidType.GLN: //Glutamine
                SafeConnect("CB","CG");
                SafeConnect("CG","CD");
                SafeConnect("CD","OE1");
                SafeConnect("CD","NE2");
                break;
            case AminoAcidType.GLU: //Glutamic Acid
                SafeConnect("CB","CG");
                SafeConnect("CG","CD");
                SafeConnect("CD","OE1");
                SafeConnect("CD","OE2");
                break;
            case AminoAcidType.HIS: //Histidine
                SafeConnect("CB","CG");
                SafeConnect("CG","ND1");
                SafeConnect("ND1","CE1");
                SafeConnect("CE1","NE2");
                SafeConnect("NE2","CD2");
                SafeConnect("CD2","CG");
                break;
            case AminoAcidType.ILE: //Isoleucine
                SafeConnect("CB","CG1");
                SafeConnect("CB","CG2");
                SafeConnect("CG1","CD1");
                break;
            case AminoAcidType.LEU: //Leucine
                SafeConnect("CB","CG");
                SafeConnect("CG","CD1");
                SafeConnect("CG","CD2");
                break;
            case AminoAcidType.LYS: //Lysine
                SafeConnect("CB","CG");
                SafeConnect("CG","CD");
                SafeConnect("CD","CE");
                SafeConnect("CE","NZ");
                break;
            case AminoAcidType.MET: //Methionine
                SafeConnect("CB","CG");
                SafeConnect("CG","SD");
                SafeConnect("SD","CE");
                break;
            case AminoAcidType.PHE: //Phenylalanine
                SafeConnect("CB","CG");
                SafeConnect("CG","CD1");
                SafeConnect("CD1","CE1");
                SafeConnect("CE1","CZ");
                SafeConnect("CZ","CE2");
                SafeConnect("CE2","CD2");
                SafeConnect("CD2","CG");
                break;
            case AminoAcidType.PRO: //Proline
                SafeConnect("CB","CG");
                SafeConnect("CG","CD");
                if (N != null) SafeConnect("CD", "N");
                break;
            case AminoAcidType.SER: //Serine
                SafeConnect("CB","OG");
                break;
            case AminoAcidType.THR: // Threonine
                SafeConnect("CB","OG1");
                SafeConnect("CB","CG2");
                break;
            case AminoAcidType.TRP: // Tryptophan
                SafeConnect("CB","CG");
                SafeConnect("CG","CD1");
                SafeConnect("CD1","NE1");
                SafeConnect("NE1","CE2");
                SafeConnect("CE2","CZ2");
                SafeConnect("CZ2","CH2");
                SafeConnect("CH2","CZ3");
                SafeConnect("CZ3","CE3");
                SafeConnect("CE3","CD2");
                SafeConnect("CD2","CG");
                SafeConnect("CD2","CE2");
                break;
            case AminoAcidType.TYR: // Tyrosine
                SafeConnect("CB","CG");
                SafeConnect("CG","CD1");
                SafeConnect("CD1","CE1");
                SafeConnect("CE1","CZ");
                SafeConnect("CZ","OH");
                SafeConnect("CZ","CE2");
                SafeConnect("CE2","CD2");
                SafeConnect("CD2","CG");
                break;
            case AminoAcidType.VAL: // Valine
                SafeConnect("CB","CG1");
                SafeConnect("CB","CG2");
                break;
        }
        
    }
    
    private void SafeConnect(string fromName, string toName) //Makes sure the dict contains both atoms before connecting
    {
        if (AtomMap.TryGetValue(fromName, out Atom fromAtom) && 
            AtomMap.TryGetValue(toName, out Atom toAtom))
        {
            fromAtom.Connect(toAtom);
        }
    }

}