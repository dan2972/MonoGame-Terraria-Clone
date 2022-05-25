using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Misc
{
    class LightBlock : GameObject
    {

        private float brightness = 0f;

        private Texture2D texture;
        private Color color;

        public LightBlock(float x, float y, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            size = Game1.BLOCKSIZE;
            color = Color.Black;

            texture = content.Load<Texture2D>("projectilesprites/particle_basic");

        }

        public override void tick(float gTime)
        {
        }

        public override void render(SpriteBatch sb)
        {
            int scaledX = (int)(x * Game1.SCALE);
            int scaledY = (int)(y * Game1.SCALE);
            int scaledWidth = (int)(size * Game1.SCALE);
            int scaledHeight = (int)(size * Game1.SCALE);
            if(brightness < 1)
                sb.Draw(texture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), color: color * (1 - brightness));
        }

        public override string getType()
        {
            return "LIGHT";
        }

        public void setBrightness(float brightness)
        {
            this.brightness = brightness;
        }

        public void setColor(Color color)
        {
            this.color = color;
        }

        public float getBrightness()
        {
            return brightness;
        }

    }
}
