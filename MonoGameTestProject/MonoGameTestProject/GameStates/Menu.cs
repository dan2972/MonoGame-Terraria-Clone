using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.GameStates
{
    class Menu
    {

        private SpriteFont basicfont;
        private Color menuTextColor;

        public Menu(ContentManager content)
        {
            basicfont = content.Load<SpriteFont>("BasicText");
        }

        public void tick()
        {
            MouseState state = Mouse.GetState();
            float scaledMouseX = state.X * Game1.SCALE;
            float scaledMouseY = state.Y * Game1.SCALE;

            if (state.X >= 100 * Game1.SCALE && state.X < 300 * Game1.SCALE && state.Y >= 100 * Game1.SCALE && state.Y < 120 * Game1.SCALE)
            {
                menuTextColor = new Color(new Vector4(255, 255, 255, 255));
                if (state.LeftButton == ButtonState.Pressed)
                {
                    Game1.GAMESTATE = "GAME";
                }
            }
            else
            {
                menuTextColor = new Color(new Vector4(0, 0, 0, 255));
            }
        }

        public void render(SpriteBatch sb)
        {
            sb.DrawString(basicfont, "MENU (Click to Continue)", new Vector2(100 * Game1.SCALE, 100 * Game1.SCALE), menuTextColor, 0, new Vector2(0,0), Game1.SCALE, SpriteEffects.None, 0);
        }

    }
}
