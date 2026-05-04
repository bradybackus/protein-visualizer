using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final;

public class MouseClicker
{
    public MouseState CurrentMouseState;
    public MouseState PreviousMouseState;
    public Cam Camera;

    public MouseClicker(Cam camera)
    {
        Camera = camera;
        CurrentMouseState = new MouseState();
        PreviousMouseState = new MouseState();
    }

    public Ray GetRay(Viewport viewport)
    {
        CurrentMouseState = Mouse.GetState();
        float mouseX = CurrentMouseState.X;
        float mouseY = CurrentMouseState.Y;

        Vector3 nearSource = new Vector3(mouseX, mouseY, 0f);
        Vector3 farSource = new Vector3(mouseX, mouseY, 1f);
        Vector3 nearPoint = viewport.Unproject(nearSource, Camera.Projection, Camera.View, Matrix.Identity);
        Vector3 farPoint = viewport.Unproject(farSource, Camera.Projection, Camera.View, Matrix.Identity);
        Vector3 direction = farPoint - nearPoint;
        direction.Normalize();
        return new  Ray(nearPoint, direction);
    }

    public Atom SelectAtom(Ray ray, Model model)
    {
        Atom closestAtom = null;
        float closestDistance = float.MaxValue;
        float currentDistance = 0f;
        int lastHash = -1;
        float maxDistance = 1000f;
        float stepSize = model.HashGrid.CellLength / 10;

        while (currentDistance < maxDistance)
        {
            Vector3 currentPos = ray.Position + (ray.Direction * currentDistance);
            int currentHash = model.HashGrid.HashCalc(currentPos);
            if (currentHash == lastHash)
            {
                currentDistance += stepSize;
                continue;
            }
            lastHash = currentHash;
            if (!model.HashGrid.HashMap.TryGetValue(currentHash, out Atom startAtom))
            {
                currentDistance += stepSize;
                continue;
            }
            (Atom hit, float distance) = CheckCellAtoms(ray, startAtom);
            if (hit != null && distance < closestDistance)
            {
                hit.Dragging = distance;
                return hit; 
            }
            currentDistance += stepSize;
        }
        return closestAtom;
    }

    private (Atom, float) CheckCellAtoms(Ray ray, Atom startAtom)
    {
        Atom closestAtom = null;
        float closestDistance = float.MaxValue;

        Atom currentAtomCheck = startAtom;
        while (currentAtomCheck != null)
        {
            BoundingSphere hitbox = new BoundingSphere(currentAtomCheck.Position, 0.3f);
            float? distance = ray.Intersects(hitbox);
            if (distance.HasValue && distance < closestDistance && currentAtomCheck.Shown)
            {
                closestAtom = currentAtomCheck;
                closestDistance = distance.Value;
            }
            currentAtomCheck = currentAtomCheck.LinkedListNext;
        }
        return (closestAtom, closestDistance);
    }
}