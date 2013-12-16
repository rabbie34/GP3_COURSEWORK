using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GP3coursework
{
    public class Camera
    {

        //GraphicsDeviceManager graphics;
        //SpriteBatch spriteBatch;

        // Matrix viewMatrix;
        public Matrix projectionMatrix;
        public Matrix worldMatrix;

        //--------------------------------------------------------------------------------------
        // Added for the creation of a camera
        //--------------------------------------------------------------------------------------
        public Matrix camViewMatrix; //Cameras view
        Matrix camRotationMatrix; //Rotation Matrix for camera to reflect movement around Y Axis
        public Vector3 camPosition; //Position of Camera in world
        public Vector3 camLookat; //Where the camera is looking or pointing at
        Vector3 camTransform; //Used for repositioning the camer after it has been rotated
        float camRotationSpeed; //Defines the amount of rotation
        float camMovementSpeed;
        float camYaw; //Cumulative rotation on Y
        Vector3 destinationPos;
        Vector3 destinationLook;
        Boolean moving;


        public Camera(GraphicsDeviceManager graphics, Vector3 position, Vector3 Lookat, int fov)
        {
            //---------------------------------------------------------------------------------------------------------------------------------------
            //Create initial camera view
            //---------------------------------------------------------------------------------------------------------------------------------------
            camPosition = position;
            camLookat = Lookat;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(fov),
                (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height,
                0.1f, 1000.0f);

            worldMatrix = Matrix.Identity;
            camRotationSpeed = 1f / 60f;
            camMovementSpeed = 1f / 60f;
        }

        public void Update()
        {
            //camRotationMatrix = Matrix.CreateRotationY(camYaw);
            //camTransform = Vector3.Transform(Vector3.Forward, camRotationMatrix);
            //camLookat = camPosition + camTransform;
            if (moving)
            {
                camPosition = ((destinationPos - camPosition) / 100 * GameConstants.CameraSpeed) + camPosition;
                camLookat = ((destinationLook - camLookat) / 100 * GameConstants.CameraSpeed) + camLookat;
                
            }
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);
        }

        public void Move(Vector3 Position, Vector3 Lookat)
        {
            moving = false;
            camPosition = Position;
            camLookat = Lookat;
            Update();
        }

        public void MoveTowards(Vector3 Position, Vector3 Lookat)
        {
            moving = true;
            destinationPos = Position;
            destinationLook = Lookat;
        }
        
        public void Rotate(float rotation)
        {
            camYaw += camRotationSpeed*rotation;
        }

        public void Translate(float direction)
        {
            camPosition += new Vector3(0, 0, camMovementSpeed*direction);
        }
    }
}
