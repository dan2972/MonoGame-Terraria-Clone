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
    class Zombie : GameObject, Mob
    {

        private float speed = 100; // set max to 700 later| original: 300
        private float gravity = 9.8f * 120;
        private float jumpPower = 400; // set max to 800 later| original: 400
        private float friction = 600f;
        private float health = 100;

        private float onObjectCount = 0;

        private Texture2D texture;

        private Boolean inAir = true;

        private ContentManager content;
        private Handler handler;
        private GameMap gMap;
        private Player player;

        private Rectangle sourceRectangle;
        private SpriteEffects direction;

        int scaledX, scaledY, scaledWidth, scaledHeight;

        private float cx, cy, csizeX, csizeY;

        public Zombie(float x, float y, Player player, Handler handler, GameMap gMap, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;
            this.player = player;

            //size = 32;
            sizeX = 32;
            sizeY = 50;
            csizeX = 32 - 8;
            csizeY = 50 - 8;

            scaledWidth = (int)(sizeX * Game1.SCALE);
            scaledHeight = (int)(sizeY * Game1.SCALE);

            sourceRectangle = new Rectangle(0, 0, 16, 25);
            direction = SpriteEffects.None;

            texture = content.Load<Texture2D>("mobsprites/zombie1");
        }

        public override string getType()
        {
            return "ZOMBIE";
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

            movement(gTime);
            collision(gTime);
        }

        private void movement(float gTime)
        {

            if (player.x < x)
            {
                if (vx > -speed)
                    vx -= speed * 10 * gTime;
                direction = SpriteEffects.FlipHorizontally;
            }
            else if (player.x > x)
            {
                if (vx < speed)
                    vx += speed * 10 * gTime;
                direction = SpriteEffects.None;
            }

            if (inAir)
                vy += gravity * gTime;
            if (vy >= 800)
                vy = 800;
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
                                    if (i + 1 < gMap.getSizeY() && j - 1 > 0 && gMap.getMap()[i + 1, j - 1].isBlock() && !inAir)
                                        y = obj.y - (csizeY + (cy - y));
                                    else
                                    {
                                        x = obj.x - (csizeX + (cx - x));
                                        vx = 0;
                                        if (!inAir)
                                        {
                                            vy = -jumpPower;
                                            inAir = true;
                                        }
                                    }
                                }
                                else if (vx < 0)
                                {
                                    if (i + 1 < gMap.getSizeY() && j + 1 < gMap.getSizeX() && gMap.getMap()[i + 1, j + 1].isBlock() && !inAir)
                                        y = obj.y - (csizeY + (cy - y)) - 4;
                                    else
                                    {
                                        x = obj.x + obj.size - (cx - x); ;
                                        vx = 0;
                                        if (!inAir)
                                        {
                                            vy = -jumpPower;
                                            inAir = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (onObjectCount == 0)
                inAir = true;

            if (y + (vy * gTime) + sizeY > 256 * 32)
            {
                x = 0;
                y = 0;
                vy = 0;
            }
        }

        public void applyDamage(float amount)
        {
            health -= amount;
            if(!inAir)
                vy -= 300;
            if (health <= 0)
            {
                handler.removeObject(this, ObjectType.type.characters);
            }
        }

    }
}
