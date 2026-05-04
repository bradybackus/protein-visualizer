using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Microsoft.Xna.Framework;

namespace Final;

public static class DataReader
{
    public static List<Residue> ReadFile(string filename)
    {
        string[] lines = File.ReadAllLines(filename);
        List<Residue> residues = new List<Residue>();
        Residue currentRes = null;
        int countFields = 0;
        int currentResID = -1;
        string currentChain = "";
        
        //Placeholders for token indexes later on
        int bestGuessIdx = -1; // Multiple Positions for the atom, take best one
        int nameCheckIdx = -1; // ATOM vs Heteroatom
        int atomIDIdx = -1; // Needed to prevent bonds drawn twice, check if neighbor id is less than own id
        int elementIdx = -1; // Elemental Letter
        int xIdx = -1; // X pos
        int yIdx = -1; // Y pos
        int zIdx = -1; // Z pos
        int resIDIdx = -1; // Amino Acid ID (1,2,3,..N) Used in case 2 of same AA are in a row
        int atomNameIdx = -1; // Label for Alpha, Beta, Gamma Carbons etc.
        int resNameIdx = -1; // 3 Letter name of AA
        int resChainIdx = -1; // Which chain is it on? A, B, C, etc.
        
        Dictionary<string, int> dataDict = new Dictionary<string, int>(); //Necessary due to files containing different # and order of fields
        bool atAtoms = false;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!atAtoms && trimmed.StartsWith("_atom_site.")) //Check if at the atoms section yet, necessary so that atAtoms can be turned off later to skip rest of file
            {
                atAtoms = true;
            }
            if (!atAtoms){continue;}
            if (trimmed.StartsWith("_atom_site.")) 
            {
                dataDict[trimmed] = countFields;
                countFields++;
                continue;
            }
            if (trimmed.StartsWith("#"))
            {
                atAtoms = false;
                continue;
            }
            if (xIdx == -1) //On first pass through, goes through the dictionary and assigns the index number to the proper field, prevents checking dict for every AA for efficiency
            {
                atomIDIdx = dataDict["_atom_site.id"];
                bestGuessIdx = dataDict["_atom_site.label_alt_id"];
                elementIdx = dataDict["_atom_site.type_symbol"];
                nameCheckIdx = dataDict["_atom_site.group_PDB"];
                xIdx = dataDict["_atom_site.Cartn_x"];
                yIdx = dataDict["_atom_site.Cartn_y"];
                zIdx = dataDict["_atom_site.Cartn_z"];
                resIDIdx = dataDict["_atom_site.auth_seq_id"];
                atomNameIdx = dataDict["_atom_site.auth_atom_id"];
                resNameIdx = dataDict["_atom_site.auth_comp_id"];
                resChainIdx =  dataDict["_atom_site.auth_asym_id"];
            }
                
            string[] tokens = Tokenize(trimmed, dataDict.Count); //Run helper function

            if ((tokens[bestGuessIdx] != "." && tokens[bestGuessIdx] != "A") || (tokens[nameCheckIdx] != "ATOM" )) //Validate for multiple atom positions and filter out heteroatoms and water
            {
                continue;
            }
            if (int.Parse(tokens[resIDIdx]) != currentResID || tokens[resChainIdx] != currentChain) //Check if we are at a new AA for this atom
            {
                currentRes = new Residue(MapAminoAcidType(tokens[resNameIdx]), int.Parse(tokens[resIDIdx]), tokens[resChainIdx]);
                residues.Add(currentRes);
                currentResID = currentRes.ID;
                currentChain = currentRes.Chain;
            }
            if (tokens[elementIdx] == "H") //Could potentially add back in for H bonding
            {
                continue;
            }
            bool xValid = float.TryParse(tokens[xIdx], out float x); //Tries to parse the x, y, and z, if it works then great, returns the x, y, and z
            bool yValid = float.TryParse(tokens[yIdx], out float y); //but if not then it skips the atom
            bool zValid = float.TryParse(tokens[zIdx], out float z);
            if (!xValid || !yValid || !zValid) //The check for if its valid
            {
                //Skip the atom with invalid coordinates
                continue; 
            }
            Vector3 newAtomPos = new Vector3(x, y, z);
            Atom newAtom = new Atom(newAtomPos, currentRes, MapAtomType(tokens[atomNameIdx]), tokens[atomNameIdx],MapElement(tokens[elementIdx]), int.Parse(tokens[atomIDIdx]));
            currentRes.AddAtom(newAtom);
            
        }
        for (int i = 0; i < residues.Count; i++) //Final run through to assign connections and previous residues
        {
            if (i > 0) residues[i].Previous = residues[i-1];
            residues[i].CreateConnections();
        }
        return residues;
    }

//TODO Add in tracking for side chain 2 - Maybe handle in residue->residue connections?


    private static string[] Tokenize(string line, int arraySize)
    {
        
        string[] tokens = new string[arraySize];  //Counts parameters based on dict and makes an array of that size
        bool inQuotes = false; //Checks for unique possibility for .cif files to have whitespace quoted
        string currentToken = "";
        int count = 0;
        for (int i = 0; i < line.Length; i++) //Go through each character
        {
            char c = line[i];
            if (c == '"' || c == '\'') //Check for if the current char is a quote
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes && char.IsWhiteSpace(c) && currentToken.Length > 0) //If theres whitespace not in quotes, thats a new item
            {
                
                tokens[count] = (currentToken);
                currentToken = "";
                count++;
                
            }
            else if (!char.IsWhiteSpace(c))
            {
                currentToken += c;
            }
        }
        if (currentToken.Length > 0) //Last token
        {
            tokens[count] = (currentToken);
        }
        return tokens;
    }

    private static AtomType MapAtomType(string rawLabel) //Helper function to assist in assigning atomtypes
    {
        AtomType atomType;

        switch (rawLabel)
        {
            case "N":
                atomType = AtomType.N;
                break;
            case "CA":
                atomType = AtomType.CA;
                break;
            case "C":
                atomType = AtomType.C;
                break;
            case "O":
                atomType = AtomType.O;
                break;
            default:
                atomType = AtomType.Other;
                break;
        }
        return atomType;
    }
    private static ElementType MapElement(string rawLabel) //Helper function to assist in assigning Elements
    {
        ElementType elementType;

        switch (rawLabel)
        {
            case "C":
                elementType = ElementType.C;
                break;
            case "N":
                elementType = ElementType.N;
                break;
            case "O":
                elementType = ElementType.O;
                break;
            case "S":
                elementType = ElementType.S;
                break;
            default:
                elementType = ElementType.Other;
                break;
        }
        return elementType;
    }

    private static AminoAcidType MapAminoAcidType(string rawLabel) //Helper function to assist in assigning AA Types
    {
        AminoAcidType aminoAcidType;
        switch (rawLabel)
        {
            case "ALA":
                aminoAcidType = AminoAcidType.ALA;
                break;
            case "ARG":
                aminoAcidType = AminoAcidType.ARG;
                break;
            case "ASN":
                aminoAcidType = AminoAcidType.ASN;
                break;
            case "ASP":
                aminoAcidType = AminoAcidType.ASP;
                break;
            case "CYS":
                aminoAcidType = AminoAcidType.CYS;
                break;
            case "GLU":
                aminoAcidType = AminoAcidType.GLU;
                break;
            case "GLN":
                aminoAcidType = AminoAcidType.GLN;
                break;
            case "GLY":
                aminoAcidType = AminoAcidType.GLY;
                break;
            case "HIS":
                aminoAcidType = AminoAcidType.HIS;
                break;
            case "ILE":
                aminoAcidType = AminoAcidType.ILE;
                break;
            case "LEU":
                aminoAcidType = AminoAcidType.LEU;
                break;
            case "LYS":
                aminoAcidType = AminoAcidType.LYS;
                break;
            case "MET":
                aminoAcidType = AminoAcidType.MET;
                break;
            case "PHE":
                aminoAcidType = AminoAcidType.PHE;
                break;
            case "PRO":
                aminoAcidType = AminoAcidType.PRO;
                break;
            case "SER":
                aminoAcidType = AminoAcidType.SER;
                break;
            case "THR":
                aminoAcidType = AminoAcidType.THR;
                break;
            case "TRP":
                aminoAcidType = AminoAcidType.TRP;
                break;
            case "TYR":
                aminoAcidType = AminoAcidType.TYR;
                break;
            case "VAL":
                aminoAcidType = AminoAcidType.VAL;
                break;
            default:
                aminoAcidType = AminoAcidType.UNK;
                break;
        }
        return aminoAcidType;
        
    }
    
}
