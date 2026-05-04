# 3D Protein Visualizer

An interactive desktop application for visualizing protein structures from 
mmCIF files with real-time physics simulation and Ramachandran plot analysis.

## Features
- Parses mmCIF structural data files, handling multi-chain proteins and 
  alternate atom positions
- Real-time 3D rendering with CPK-colored atoms and bond cylinders
- Verlet physics simulation with spring constraints maintaining bond geometry
- Live Ramachandran plot that updates as protein conformation changes
- Spatial hash grid with linked-list chaining for O(1) atom lookup across 
  thousands of atoms
- Full side chain connectivity for all 20 amino acid types
- Residue filtering by biochemical property (polar, nonpolar, acidic, basic)
- Export modified conformations back to .cif format

## Tech Stack
- C# / .NET
- MonoGame (rendering)
- mmCIF file format (RCSB Protein Data Bank)

## How to Run
1. Clone the repo
2. Install .NET 8 SDK
3. Run `dotnet run` from the project directory
4. Press I to load a .cif file from RCSB (rcsb.org)

## Controls
| Key | Action |
|-----|--------|
| Arrow Keys | Rotate camera |
| Shift + Arrows | Pan camera |
| Scroll Wheel | Zoom |
| Click + Drag | Move individual atoms |
| B | Toggle backbone-only view |
| R | Reset model |
| P | Export modified .cif |
| 1-9 | Switch between loaded models |


