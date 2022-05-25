using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Entities.Mobs;
using MonoGameTestProject.Map;
using MonoGameTestProject.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Misc
{
    class Background
    {
        Texture2D backgroundTexture;
        Texture2D sunTexture;
        Camera cam;
        GameMap gMap;

        float x, y, sunSize;

        Player player;

        public Background(Camera cam, GameMap gMap, Player player, ContentManager content)
        {
            this.cam = cam;
            this.player = player;
            this.gMap = gMap;
            backgroundTexture = content.Load<Texture2D>("background");
            sunTexture = content.Load<Texture2D>("projectilesprites/particle_basic");

            sunSize = 64;
        }

        public void render(SpriteBatch sb)
        {
            int scaledX = (int)(x * Game1.SCALE);
            int scaledY = (int)(y * Game1.SCALE);
            int scaledWidth = (int)(sunSize * Game1.SCALE);
            int scaledHeight = (int)(sunSize * Game1.SCALE);
            //sb.Draw(backgroundTexture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight));
            //sb.Draw(sunTexture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), origin: new Vector2(sunTexture.Width / 2, sunTexture.Height / 2), rotation: gMap.getSunRotation());
            sb.Draw(sunTexture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight));
        }

        public Color getBackgroundColor()
        {
            return Color.Lerp(new Color(90, 150, 255), new Color(15, 15, 38), 1 - (gMap.getGlobalBrightness()));
        }

        public void tick(float gTime)
        {
            x = -cam.getX() / Game1.SCALE - (sunSize / 2) - (float)(384 * Math.Cos(gMap.getSunRotation()));
            y = 516 - (sunSize / 2) - (float)(384 * Math.Sin(gMap.getSunRotation()));
        }
    }
}
