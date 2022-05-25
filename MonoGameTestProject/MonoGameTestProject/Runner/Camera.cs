using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Runner
{
    class Camera
    {

        private Matrix transform;

        private float x = 0, y = 0, xTarg, yTarg;

        public void follow(float playerX, float playerY, float sizeX, float sizeY, float gTime)
        {
            xTarg = -playerX * Game1.SCALE - (sizeX * Game1.SCALE / 2);
            yTarg = -playerY * Game1.SCALE - (sizeY * Game1.SCALE / 2);
            x += (xTarg - this.x) * gTime * 3;
            y += (yTarg - this.y) * gTime * 3;

            clampCamera();

            Matrix position = Matrix.CreateTranslation((int)(x),(int)(y), 0);
            Matrix offset = Matrix.CreateTranslation(Game1.TRUEGAMEWIDTH / 2, Game1.TRUEGAMEHEIGHT / 2, 0);
            transform = position * offset;
        }

        private void clampCamera()
        {
            if (-x <= Game1.TRUEGAMEWIDTH / 2)
                x = -(Game1.TRUEGAMEWIDTH / 2);
            if (-x >= (Game1.MAPSIZEX * Game1.BLOCKSIZE * Game1.SCALE) - (Game1.TRUEGAMEWIDTH / 2))
                x = -((Game1.MAPSIZEX * Game1.BLOCKSIZE * Game1.SCALE) - (Game1.TRUEGAMEWIDTH / 2));
            if (-y <= Game1.TRUEGAMEHEIGHT / 2)
                y = -(Game1.TRUEGAMEHEIGHT / 2);
            if (-y >= (Game1.MAPSIZEY * Game1.BLOCKSIZE) - (Game1.TRUEGAMEHEIGHT / 2))
                y = -((Game1.MAPSIZEY * Game1.BLOCKSIZE) - (Game1.TRUEGAMEHEIGHT / 2));
        }

        public Matrix getTransform()
        {
            return transform;
        }

        public float getX()
        {
            return x;
        }

        public float getY()
        {
            return y;
        }

    }
}
