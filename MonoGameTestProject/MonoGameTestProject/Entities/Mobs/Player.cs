using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameTestProject.Calculations;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Entities.Projectiles;
using MonoGameTestProject.Map;
using MonoGameTestProject.Misc;
using MonoGameTestProject.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Mobs
{
    class Player : GameObject
    {

        private float speed = 200; // set max to 700 later| original: 200
        private float gravity = 9.8f * 120;
        private float jumpPower = 400; // set max to 800 later| original: 400
        private float friction = 750f;

        private float cursorX = Game1.TRUEGAMEWIDTH / 2, cursorY = Game1.TRUEGAMEHEIGHT / 2;
        private float cursorVX = 0, cursorVY = 0;

        private float onObjectCount = 0;
        private float offsetX, offsetY, mx, my;

        private float bulletTimeCollected = 0;
        private float bulletCooldownTime = 0.2f;
        private float spellTimeCollected = 2;
        private float spellCoolDownTime = 2f;

        private Texture2D texture;
        private float animationDelay = 0;

        private Boolean inAir = true;

        private Camera camera;
        private ContentManager content;
        private Handler handler;
        private GameMap gMap;
        private Inventory inventory;

        private GamePadState gamePadState;
        private MouseState mouseState;
        private KeyboardState keyboardState;

        private Rectangle sourceRectangle;
        private SpriteEffects direction;

        int scaledX, scaledY, scaledWidth, scaledHeight;

        private float cx, cy, csizeX, csizeY;

        public Player(float x, float y, Handler handler, GameMap gMap, Inventory inventory, Camera camera, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;
            this.camera = camera;
            this.inventory = inventory;

            //size = 32;
            sizeX = 32;
            sizeY = 50;
            csizeX = 32 - 8;
            csizeY = 50 - 8;

            scaledWidth = (int)(sizeX * Game1.SCALE);
            scaledHeight = (int)(sizeY * Game1.SCALE);

            sourceRectangle = new Rectangle(0, 0, 16, 25);
            direction = SpriteEffects.None;

            texture = content.Load<Texture2D>("playersprites/player_basic_sprite_sheet");
        }

        public override string getType()
        {
            return "PLAYER";
        }

        public override void render(SpriteBatch sb)
        {
            scaledX = (int)(x * Game1.SCALE);
            scaledY = (int)(y * Game1.SCALE);
            sb.Draw(texture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), sourceRectangle: sourceRectangle, effects: direction);
        }

        public override void tick(float gTime)
        {
            x += vx * gTime;
            y += vy * gTime;
            cx = x + 4;
            cy = y + 8;

            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            handleMouse();

            shootBullet(gTime);
            castSpells(gTime);
            movement(gTime);
            collision(gTime);
            animate(gTime);
        }

        private void handleMouse()
        {
            mouseState = Mouse.GetState();
            offsetX = Game1.TRUEGAMEWIDTH / 2 - (-camera.getX());
            offsetY = Game1.TRUEGAMEHEIGHT / 2 - (-camera.getY());
            mx = (mouseState.X - offsetX) / Game1.SCALE;
            my = (mouseState.Y - offsetY) / Game1.SCALE;
        }

        private void shootBullet(float gTime)
        {
            bulletTimeCollected += gTime;

            if (!inventory.inventoryIsOpen() && (mouseState.LeftButton == ButtonState.Pressed || gamePadState.Triggers.Right > 0) && bulletTimeCollected >= bulletCooldownTime)
            {
                bulletTimeCollected = 0;
                if (inventory.selectedItemID != ItemID.ID.NULL && inventory.selectedItemID == ItemID.ID.fire_wand)
                {
                    handler.addObject(new Fireball(x + (sizeX / 2), y + (sizeY / 2), handler, gMap, camera, content), ObjectType.type.projectiles);
                }else if (inventory.selectedItemID != ItemID.ID.NULL && inventory.selectedItemID == ItemID.ID.pistol_basic)
                {
                    handler.addObject(new Bullet(x + (sizeX / 2), y + (sizeY / 2), handler, gMap, camera, content), ObjectType.type.projectiles);
                }
            }
        }

        private void castSpells(float gTime)
        {
            spellTimeCollected += gTime;

            if (!inventory.inventoryIsOpen() && keyboardState.IsKeyDown(Keys.LeftShift) && spellTimeCollected >= spellCoolDownTime)
            {
                spellTimeCollected = 0;
                handler.addObject(new MagicCircle(x + 16 - 128, y - 300, handler, gMap, content, "ice_spell_cast_mc"), ObjectType.type.projectiles);
            }
        }

        private void animate(float gTime)
        {
            animationDelay += gTime;
            if (animationDelay >= 1/(Math.Abs(vx/5)))
            {
                animationDelay = 0;
                if(Math.Abs(vy) < 3)
                {
                    if (Math.Abs(vx) >= 0.2)
                    {
                        if (sourceRectangle.X < 3 * 16)
                            sourceRectangle.X = 16 * 3;
                        sourceRectangle.X += 16;
                        if (sourceRectangle.X >= 16 * 16)
                        {
                            sourceRectangle.X = 16 * 2;
                        }
                    }
                }
            }
            if (Math.Abs(vy) >= 3)
            {
                sourceRectangle.X = 16;
            }
            else if(Math.Abs(vx) < 0.2)
            {
                sourceRectangle.X = 0;
            }
        }

        private void movement(float gTime)
        {
            //moving cursor with gamepad
            if (Game1.usingGamePad)
            {
                cursorX += cursorVX;
                cursorY += cursorVY;
                cursorVX = gamePadState.ThumbSticks.Right.X * 50;
                cursorVY = -gamePadState.ThumbSticks.Right.Y * 50;
                Mouse.SetPosition((int)cursorX, (int)cursorY);
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (vx > -speed)
                    vx -= speed * 10 * gTime;
                direction = SpriteEffects.FlipHorizontally;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                if (vx < speed)
                    vx += speed * 10 * gTime;
                direction = SpriteEffects.None;
            }
            else
            {
                if (vx > 30)
                    vx -= friction * gTime;
                else if (vx < -30)
                    vx += friction * gTime;
                else
                    vx = 0;
            }

            if (Game1.usingGamePad)
                vx = gamePadState.ThumbSticks.Left.X * speed;

            if (inAir)
                vy += gravity * gTime;
            if (vy >= 800)
                vy = 800;

            if (!inAir && (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Space) || gamePadState.Buttons.A == ButtonState.Pressed))
            {
                vy = -jumpPower;
                inAir = true;
            }
        }

        private void collision(float gTime)
        {
            onObjectCount = 0;
            int xMap = (int)cx / (int)Game1.BLOCKSIZE;
            int yMap = (int)cy / (int)Game1.BLOCKSIZE + 1;
            int minCheckX = xMap - 3 < 0 ? 0 : xMap - 3;
            int maxCheckX = xMap + 3 >= 255 ? 255 : xMap + 3;
            int minCheckY = yMap - 3 < 0 ? 0 : yMap - 3;
            int maxCheckY = yMap + 3 >= 255 ? 255 : yMap + 3;
            for (int i = minCheckY; i <= maxCheckY; i++)
            {
                for (int j = minCheckX; j <= maxCheckX; j++)
                {
                    GenericBlock obj = gMap.getMap()[i, j];
                    if (obj.isBlock())
                    {
                        if (obj is GenericBlock)
                        {
                            GenericBlock tempObj = (GenericBlock)obj;
                            //tempObj.highlight();
                        }
                        if (CollisionDetector.intersects(cx, cy + csizeY + (vy * gTime), csizeX, 2, obj.x, obj.y, obj.size, obj.size))
                            onObjectCount++;
                        if (CollisionDetector.intersects(cx + (vx * gTime), cy + (vy * gTime), csizeX, csizeY, obj.x, obj.y, obj.size, obj.size))
                        {
                            
                            if (CollisionDetector.intersects(cx, cy + csizeY + (vy * gTime) + 1, csizeX, 1, obj.x, obj.y, obj.size, obj.size) && inAir && vy > 0)
                            {
                                animationDelay = 0;
                                sourceRectangle.X = 0;
                                inAir = false;
                                vy = 0;
                                y = obj.y - (csizeY + (cy - y));
                            }
                            else if (CollisionDetector.intersects(cx, cy + (vy * gTime) - 1, csizeX, 1, obj.x, obj.y, obj.size, obj.size) && inAir && vy < 0)
                            {
                                vy = 0;
                                y = obj.y + obj.size - (cy - y);
                            }
                            else if (!CollisionDetector.intersects(cx, cy + csizeY + 1, csizeX, 1, obj.x, obj.y, obj.size, obj.size) && !CollisionDetector.intersects(cx, cy - 1, csizeX, 1, obj.x, obj.y, obj.size, obj.size))
                            {
                                if (vx > 0)
                                {
                                    if (i + 1 < gMap.getSizeY() && j - 1 > 0 && gMap.getMap()[i + 1, j - 1].isBlock() && !inAir && keyboardState.IsKeyDown(Keys.D))
                                        y = obj.y - (csizeY + (cy - y));
                                    else
                                    {
                                        x = obj.x - (csizeX + (cx - x));
                                        vx = 0;
                                    }
                                }
                                else if (vx < 0)
                                {
                                    if (i + 1 < gMap.getSizeY() && j + 1 < gMap.getSizeX() && gMap.getMap()[i + 1, j + 1].isBlock() && !inAir && keyboardState.IsKeyDown(Keys.A))
                                        y = obj.y - (csizeY + (cy - y))-4;
                                    else
                                    {
                                        x = obj.x + obj.size - (cx - x); ;
                                        vx = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (onObjectCount == 0)
                inAir = true;

            if (y + (vy * gTime) + sizeY > 256*32)
            {
                x = 0;
                y = 0;
                vy = 0;
            }
        }

    }
}