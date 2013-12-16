using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GP3coursework
{
    struct Laser
    {
        public Vector3 position;
        public Vector3 direction;
        public float rotation;
        public float pitch;
        public float speed;
        public bool isActive;

        public void Update(float delta)
        {
            position += direction * speed *
                        GameConstants.LaserSpeedAdjustment * delta;
            if (position.X > GameConstants.PlayfieldSizeX ||
                position.X < -GameConstants.PlayfieldSizeX ||
                position.Z > GameConstants.PlayfieldSizeZ ||
                position.Z < -GameConstants.PlayfieldSizeZ ||
                position.Y < 0)
                isActive = false;
        }
    }
}
