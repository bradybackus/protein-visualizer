using System;
using System.Collections.Generic;
using Eto.Drawing;
using Color = Microsoft.Xna.Framework.Color;

namespace Final;

public class FilterHelper
{
    public List<Model> Models;
    public int CurrentProtein;
    private Color _highlightColor = Color.LimeGreen;

    public Dictionary<string, List<AminoAcidType>> AATypeDict = new Dictionary<string, List<AminoAcidType>>()
    {
        { "Basic", [AminoAcidType.LYS, AminoAcidType.HIS, AminoAcidType.ARG] },
        { "Acidic", [AminoAcidType.GLU, AminoAcidType.ASP] },
        {
            "Polar",
            [
                AminoAcidType.TYR, AminoAcidType.THR, AminoAcidType.GLN, AminoAcidType.GLY, AminoAcidType.SER,
                AminoAcidType.CYS, AminoAcidType.ASN
            ]
        },
        {
            "Nonpolar",
            [
                AminoAcidType.ALA, AminoAcidType.MET, AminoAcidType.LEU, AminoAcidType.ILE, AminoAcidType.VAL,
                AminoAcidType.PRO, AminoAcidType.TRP, AminoAcidType.PHE
            ]
        }
    };

    // public Dictionary<string, Color> ColorDict = new Dictionary<string, Color>()
    // {
    //     { "Basic", Color.Cyan },
    //     { "Acidic", Color.Red },
    //     { "Polar", Color.LimeGreen },
    //     { "Nonpolar", Color.Magenta },
    // };

    public FilterHelper(List<Model> models, int currentProtein)
    {
        Models = models;
        CurrentProtein = currentProtein;
    }

    public void ApplyFilter(String selection)
    {
        if (selection == "No Filter") return;
        
        foreach (Residue residue in Models[CurrentProtein].ProteinData)
        {
            foreach (Atom atom in residue.Atoms)
            {
                if (AATypeDict[selection].Contains(residue.AAType))
                {
                    atom.Color = _highlightColor;
                    residue.BackBoneColor = _highlightColor;
                    residue.SideChainColor = _highlightColor;
                }
            }
        }
    }
    

    public void SetModel(int model)
    {
        CurrentProtein = model;
    }
}
    