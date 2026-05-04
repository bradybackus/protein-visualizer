using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Final;

public class SelectionManager
{
    public Atom SelectedAtom;
    private Color _selectionColor = Color.Green;
    
    public void Update(MouseState mState, MouseState prevMState, Ray mouseRay, Model currentModel, MouseClicker clicker)
    {
        if (mState.LeftButton == ButtonState.Pressed && prevMState.LeftButton == ButtonState.Released)
        {
            HandleNewSelection(mouseRay, currentModel, clicker);
        }

        if (SelectedAtom != null)
        {
            if (mState.LeftButton == ButtonState.Pressed)
            {
                SelectedAtom.DragAtom(mouseRay);
            }
            else if (mState.LeftButton == ButtonState.Released)
            {
                SelectedAtom.Dragging = 0f;
            }
        }
    }

    private void HandleNewSelection(Ray mouseRay, Model currentModel, MouseClicker clicker)
    {
        ResetCurrentHighlight();
        SelectedAtom = clicker.SelectAtom(mouseRay, currentModel);
        if (SelectedAtom != null)
        {
            float dist = Vector3.Distance(mouseRay.Position, SelectedAtom.Position);
            SelectedAtom.StartDrag(mouseRay, dist);
        }
        ApplySelectionHighlight();
    }

    private void ApplySelectionHighlight()
    {
        if (SelectedAtom == null) return;
        foreach (Atom atom in SelectedAtom.Residue.Atoms)
        {
            atom.Color = _selectionColor;
        }
        SelectedAtom.Color = Color.White;
        SelectedAtom.Residue.BackBoneColor = _selectionColor;
        SelectedAtom.Residue.SideChainColor = _selectionColor;
    }

    public void ResetCurrentHighlight()
    {
        if (SelectedAtom == null) return;
        foreach (Atom atom in SelectedAtom.Residue.Atoms)
        {
            atom.SetColor();
        }
        SelectedAtom.Residue.SetColor();
    }
}
    