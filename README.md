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

### Download & Run (Windows)
Download the latest release from the Releases tab — no install required.

### Build from Source
1. Install .NET 8 SDK and MonoGame
2. Clone the repo
3. Run `dotnet run` from the project directory

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


## Protein Interaction and Ramachandran Plot
<img width="952" height="576" alt="gif1" src="https://github.com/user-attachments/assets/c2ff3de5-dcad-4fb3-ab42-9cfcf1f0bbef" />

## Backbone View and Dynamic Camera Movement
<img width="956" height="584" alt="gif2" src="https://github.com/user-attachments/assets/d82d4029-ae64-4a03-a325-ee50e68ea49f" />

## Amino Acid Filter
<img width="952" height="582" alt="gif3" src="https://github.com/user-attachments/assets/3ded1f51-e1a7-4e1c-8a42-4afc899fd43c" />

