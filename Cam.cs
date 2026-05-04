using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final;

public class Cam
{
    private Vector3 _eye;
    private Vector3 _target = Vector3.Zero;
    private Vector3 _up = new Vector3(0f, 1f, 0f);

    public Matrix Projection;
    public Matrix View;
    public MouseState PreviousMouseState;

    public float CameraSpeed = 0.05f;
    public float CurrentDistance = 100f;
    public float CameraHorizontalAngle = 0f;
    public float CameraVerticalAngle = 0f;
    
    public Vector3 LightDirection => Vector3.Normalize(_target - _eye);

    public Cam(GraphicsDevice graphicsDevice)
    {
        float fieldOfView = MathHelper.PiOver4;
        float aspectRatio = graphicsDevice.Viewport.AspectRatio;
        Projection = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView,
            aspectRatio,
            1f,
            1000f);
        _eye = new Vector3(0, 0, 100);
        View = Matrix.CreateLookAt(_eye, _target, _up);
        PreviousMouseState = Mouse.GetState();
    }

    public void ResetCamera()
    {
        _eye = new Vector3(0, 0, 100);
        CurrentDistance = 100f;
        _target = Vector3.Zero;
        View = Matrix.CreateLookAt(_eye, _target, _up);
    }
    
    
    // just handles if the window is fullscreened
    public void Resize(GraphicsDeviceManager graphics, GameWindow window)
    {
        graphics.PreferredBackBufferWidth = window.ClientBounds.Width;
        graphics.PreferredBackBufferHeight = window.ClientBounds.Height;
        graphics.ApplyChanges();

        float aspectRatio = (float)window.ClientBounds.Width / window.ClientBounds.Height;
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            aspectRatio,
            1f,
            1000f);
    }

    public void UpdateCamera(GameTime gameTime, KeyboardState kState, MouseState mState)
    {
        // Cross products for panning
        Vector3 forward = Vector3.Normalize(_target - _eye);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.Up));
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));
        bool camChange = false;
        float Reverse = 1;

        if (CurrentDistance < 2)
        {
            Reverse = -0.5f;
        }
        
        //Different behavior for if shift is held down or not. holding shift makes arrow keys pan instead of rotate
        if (kState.IsKeyDown(Keys.Left) || kState.IsKeyDown(Keys.A))
        {
            if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift))
            {
                _eye += right * -1 *  CameraSpeed * 10;
                _target += right * -1 * CameraSpeed * 10;
            }
            else
            {
                CameraHorizontalAngle -= CameraSpeed * Reverse;
            }
            camChange = true;
        }

        if (kState.IsKeyDown(Keys.Right) || kState.IsKeyDown(Keys.D))
        {
            if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift))
            {
                _eye += right *  CameraSpeed * 10;
                _target += right * CameraSpeed * 10;
            }
            else
            {
                CameraHorizontalAngle += CameraSpeed * Reverse;
            }
            camChange = true;
        }

        if (kState.IsKeyDown(Keys.Up) || kState.IsKeyDown(Keys.W))
        {
            if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift))
            {
                _eye += up * CameraSpeed * 10;
                _target += up * CameraSpeed * 10;
            }
            else
            {
                CameraVerticalAngle = MathHelper.Clamp(CameraVerticalAngle + CameraSpeed * Reverse, -MathHelper.PiOver2 + 0.01f,
                    MathHelper.PiOver2 - 0.01f);
            }
            camChange = true;
        }

        if (kState.IsKeyDown(Keys.Down) || kState.IsKeyDown(Keys.S))
        {
            if (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift))
            {
                _eye += up * -1 *  CameraSpeed * 10;
                _target += up * -1 * CameraSpeed * 10;
            }
            else
            {
                CameraVerticalAngle = MathHelper.Clamp(CameraVerticalAngle - CameraSpeed * Reverse, -MathHelper.PiOver2 + 0.01f,
                    MathHelper.PiOver2 - 0.01f);
            }
            camChange = true;
        }

        //Calculate spherical angle of camera from protein, only if camera moved or zoomed
        if (camChange || mState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue)
        {
            float deltaM = (mState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue);
            // lets you scroll forever, if you are too close to the target, the target moves away
            if (CurrentDistance <= 1.01f && deltaM > 0)
            {
                _target +=  deltaM * 0.02f * forward;
            }
            else
            {
                CurrentDistance = Math.Clamp(CurrentDistance - deltaM * 0.02f, 1f, 5000f); 
            }

            float x = CurrentDistance * MathF.Cos(CameraVerticalAngle) * MathF.Sin(CameraHorizontalAngle);
            float y = CurrentDistance * MathF.Sin(CameraVerticalAngle);
            float z = CurrentDistance * MathF.Cos(CameraVerticalAngle) * MathF.Cos(CameraHorizontalAngle);
            _eye = _target + new Vector3(x, y, z);
            View = Matrix.CreateLookAt(_eye, _target, _up);
        }

        PreviousMouseState = mState;
    }

}