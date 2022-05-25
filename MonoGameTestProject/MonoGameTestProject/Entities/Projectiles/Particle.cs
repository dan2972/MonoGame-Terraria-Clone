using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Calculations;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Projectiles
{
    class Particle : GameObject
    {

        Texture2D texture;

        private Handler handler;
        private GameMap gMap;

        private float alpha = 1;
        private float initialVX;
        private float friction = 0.01f;
        private String collisionType;
        private Color color, startColor, newColor;
        private float changeSpeed, changeAmount = 0;
        private Boolean colorChange = false;
        private float lightBrightness = 1;
        private float lightFadeSpeed = 1;

        public bool shrinkEnabled {get; set;}
        public bool emitLight { get; set; }
        public float size { get; set; }
        public float shrinkSpeed { get; set; }
        public float fadeSpeed { get; set; }
        public float gravity { get; set; }

        public Particle(float x, float y, float vx, float vy, Handler handler, GameMap gMap, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.vx = vx;
            this.vy = vy;
            this.gravity = 9.8f*120;
            this.initialVX = vx;
            this.handler = handler;
            this.gMap = gMap;
            this.collisionType = "none";
            color = Color.White;
            startColor = color;
            size = 4;
            fadeSpeed = 0.5f;

            texture = content.Load<Texture2D>("projectilesprites/particle_basic");
        }
        public Particle(float x, float y, float vx, float vy, float gravity, String collisionType, Color color, Handler handler, GameMap gMap, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.vx = vx;
            this.vy = vy;
            this.gravity = gravity;
            this.initialVX = vx;
            this.handler = handler;
            this.gMap = gMap;
            this.collisionType = collisionType;
            this.color = color;
            startColor = color;
            size = 4;
            fadeSpeed = 0.5f;

            texture = content.Load<Texture2D>("projectilesprites/particle_basic");
        }

        public override string getType()
        {
            return "PARTICLE";
        }

        public override void render(SpriteBatch sb)
        {
            int scaledX = (int)(x * Game1.SCALE);
            int scaledY = (int)(y * Game1.SCALE);
            int scaledWidth = (int)(size * Game1.SCALE);
            int scaledHeight = (int)(size * Game1.SCALE);
            sb.Draw(texture, new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), color * alpha);
        }

        public override void tick(float gTime)
        {
            x += vx * gTime;
            y += vy * gTime;

            collision(gTime);
            vy += gravity*gTime;

            if (initialVX > 0)
            {
                vx -= vx * friction;
                vx = vx < 0 ? 0 : vx;
            }
            else if (initialVX < 0)
            {
                vx += -vx * friction;
                vx = vx > 0 ? 0 : vx;
            }

            if(shrinkEnabled)
                size -= shrinkSpeed * gTime;
            size = size < 0 ? 0 : size;

            if (colorChange)
            {
                color = Color.Lerp(startColor, newColor, changeAmount);
                if (changeAmount + changeSpeed * gTime < 1f)
                {
                    changeAmount += changeSpeed * gTime;
                }

            }

            alpha-= fadeSpeed * gTime;
            if(alpha <= 0)
            {
                handler.removeObject(this, ObjectType.type.particles);
            }

            if (emitLight)
            {
                int mapX = (int)x / (int)Game1.BLOCKSIZE;
                int mapY = (int)y / (int)Game1.BLOCKSIZE;
                if (mapX > 0 && mapY > 0 && mapX < Game1.MAPSIZEX && mapY < Game1.MAPSIZEY && gMap.getMap()[mapY, mapX].brightness < lightBrightness)
                    gMap.getMap()[mapY, mapX].brightness = lightBrightness;
                lightBrightness -= lightFadeSpeed * gTime;
                lightBrightness = lightBrightness < 0 ? 0 : lightBrightness;
            }
        }

        public void collision(float gTime)
        {
            int xMap = (int)x / (int)Game1.BLOCKSIZE;
            int yMap = (int)y / (int)Game1.BLOCKSIZE + 1;
            int minCheckX = xMap - 2 < 0 ? 0 : xMap - 2;
            int maxCheckX = xMap + 2 >= 255 ? 255 : xMap + 2;
            int minCheckY = yMap - 2 < 0 ? 0 : yMap - 2;
            int maxCheckY = yMap + 2 >= 255 ? 255 : yMap + 2;
            switch (collisionType)
            {
                case "collideWithBlock":
                    for (int i = minCheckY; i <= maxCheckY; i++)
                    {
                        for (int j = minCheckX; j <= maxCheckX; j++)
                        {
                            GenericBlock obj = gMap.getMap()[i, j];
                            if (obj.isBlock())
                            {
                                if (CollisionDetector.intersects(x + (vx * gTime), y + (vy * gTime), 4, 4, obj.x, obj.y, obj.size, obj.size))
                                {
                                    if (CollisionDetector.intersects(x, y + 4 + (vy * gTime), 4, 1, obj.x, obj.y, obj.size, obj.size) && vy > 0)
                                    {
                                        vy = 0;
                                        y = obj.y - 4;
                                    }
                                    else if (CollisionDetector.intersects(x, y + (vy * gTime), 4, 4, obj.x, obj.y + obj.size, obj.size, 1) && vy < 0)
                                    {
                                        vy = 0;
                                        y = obj.y + obj.size;
                                    }
                                    else if (!CollisionDetector.intersects(x, y + 4, 4, 1, obj.x, obj.y, obj.size, obj.size))
                                    {
                                        if (vx > 0)
                                        {
                                            x = obj.x - 4;
                                        }
                                        else
                                        {
                                            x = obj.x + obj.size;
                                        }
                                        vx = 0;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case "collideAndDestroy":
                    for (int i = minCheckY; i <= maxCheckY; i++)
                    {
                        for (int j = minCheckX; j <= maxCheckX; j++)
                        {
                            GenericBlock obj = gMap.getMap()[i, j];
                            if (obj.isBlock())
                            {
                                if (CollisionDetector.intersects(x, y, 4, 4, obj.x, obj.y, obj.size, obj.size))
                                {
                                    handler.removeObject(this, ObjectType.type.particles);
                                }
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
            
        }

        public void fadeToColor(Color newColor, float changeSpeed)
        {
            this.newColor = newColor;
            this.changeSpeed = changeSpeed;
            colorChange = true;
        }

        public void setFriction(float f)
        {
            this.friction = f;
        }

        public void setLightFadeSpeed(float speed)
        {
            lightFadeSpeed = speed;
        }

    }
}
