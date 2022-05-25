using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameTestProject.Calculations;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Entities.Mobs;
using MonoGameTestProject.Map;
using MonoGameTestProject.Runner;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Projectiles
{
    class Bullet : GameObject
    {

        Texture2D texture;
        private float rotation = 0;
        private float distance = 2000;
        private float alpha = 1;
        private float slope = 0;
        private float damage = 90;

        private Handler handler;
        private GameMap gMap;
        private Camera camera;
        private ContentManager content;

        private Random seed;

        private Boolean ticked = false;

        int scaledX;
        int scaledY;
        int scaledWidth;
        int scaledHeight;

        private float changeX, changeY, renderX, renderY, bulletSpeed = 4000f;
        private float gTime;

        private int particleCount = 1;

        private SoundEffect shootingSound;

        public Bullet(float x, float y, Handler handler, GameMap gMap, Camera camera, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;
            this.camera = camera;
            sizeX = distance;
            sizeY = 2;

            seed = new Random();

            //translate mouse position according to the camera
            MouseState state = Mouse.GetState();
            float offsetX, offsetY;
            offsetX = Game1.TRUEGAMEWIDTH / 2 - (-camera.getX());
            offsetY = Game1.TRUEGAMEHEIGHT / 2 - (-camera.getY());
            float mx = (state.X - offsetX) / Game1.SCALE;
            float my = (state.Y - offsetY) / Game1.SCALE;

            scaledWidth = (int)(sizeX * Game1.SCALE);
            scaledHeight = (int)(sizeY * Game1.SCALE);

            float spread = (float)seed.NextDouble() * 0.1f - 0.05f;
            rotation = (float)Math.Atan2((my - y), (mx - x)) + spread;
            slope = (float)Math.Tan(rotation);

            changeX = (float)Math.Cos(rotation) * bulletSpeed;
            changeY = (float)Math.Sin(rotation) * bulletSpeed;
            renderX = x;
            renderY = y;

            /*
            float impulseX = ((float)Math.Cos(rotation) * 200);
            float impulseY = ((float)Math.Sin(rotation) * 200);
            foreach(GameObject gm in handler.characterList)
            {
                if(gm.getType() == "PLAYER")
                {
                    gm.vx += -impulseX;
                    gm.vy += -impulseY;
                }
            }*/

            texture = content.Load<Texture2D>("projectilesprites/bullet_line");
            shootingSound = content.Load<SoundEffect>("soundfx/bulletBasic");
            shootingSound.Play(0.1f, 0, 0);
        }

        public override string getType()
        {
            return "BULLET";
        }

        public override void render(SpriteBatch sb)
        {
            scaledX = (int)(renderX * Game1.SCALE);
            scaledY = (int)(renderY * Game1.SCALE);
            
            sb.Draw(texture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), origin: new Vector2(0, scaledHeight / 2), rotation: rotation, color: Color.White * alpha);

            //renderX += changeX * gTime;
            //renderY += changeY * gTime;
            //sizeX -= bulletSpeed * gTime;
            sizeY -= 16f * gTime;
            alpha -= 6f * gTime;
        }

        public override void tick(float gTime)
        {
            this.gTime = gTime;

            checkCollision();

            scaledWidth = (int)(sizeX * Game1.SCALE);
            scaledHeight = (int)(sizeY * Game1.SCALE);

            if (alpha <= 0)
            {
                handler.removeObject(this, ObjectType.type.projectiles);
            }
        }

        public void checkCollision()
        {
            if (!ticked)
            {
                if (Math.Abs(slope) <= 1)
                {
                    xOrientedCollision();
                }
                else
                {
                    yOrientedCollision();
                }
            }
        }

        public void xOrientedCollision()
        {
            int collisionCount = 0;
            ArrayList vectorList = new ArrayList();
            ArrayList sides = new ArrayList();
            float startX = (x / Game1.BLOCKSIZE);
            float maxX = startX + ((float)Math.Cos(rotation) * distance) / Game1.BLOCKSIZE;
            maxX = maxX > gMap.getSizeX() ? gMap.getSizeX() : maxX;
            maxX = maxX < 0 ? 0 : maxX;

            Vector2 initialPoint = new Vector2(x, y);
            Vector2 terminalPoint = new Vector2(x + distance * (float)Math.Cos(rotation), y + distance * (float)Math.Sin(rotation));

            if (startX > maxX)
            {
                float temp = startX;
                startX = maxX;
                maxX = temp;
            }
            for (float j = startX; j < maxX; j += 0.1f)
            {
                float checkY = (int)(y / Game1.BLOCKSIZE) + (j - (int)(x / Game1.BLOCKSIZE)) * slope;
                checkY = checkY > gMap.getSizeY() - 1 ? gMap.getSizeY() - 1 : checkY;
                checkY = checkY < 0 ? 0 : checkY;
                int finalX = (int)Math.Round(j);
                finalX = finalX > gMap.getSizeX() - 1 ? gMap.getSizeX() - 1 : finalX;
                GenericBlock obj = gMap.getMap()[(int)Math.Round(checkY), finalX];
                //obj.highlight();

                if (obj.isBlock())
                {
                    Vector2 blockTopLeft = new Vector2(obj.x, obj.y);
                    Vector2 blockTopRight = new Vector2(obj.x + obj.size, obj.y);
                    Vector2 blockBottomLeft = new Vector2(obj.x, obj.y + obj.size);
                    Vector2 blockBottomRight = new Vector2(obj.x + obj.size, obj.y + obj.size);

                    //check left bound of object -> 1
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockTopLeft, blockBottomLeft))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockTopLeft, blockBottomLeft));
                        sides.Add(1);
                    }
                    //check right bound of object -> 2
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockTopRight, blockBottomRight))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockTopRight, blockBottomRight));
                        sides.Add(2);
                    }
                    //check upper bound of object -> 3
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockTopLeft, blockTopRight))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockTopLeft, blockTopRight));
                        sides.Add(3);
                    }
                    //check left bound of object -> 4
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockBottomLeft, blockBottomRight))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockBottomLeft, blockBottomRight));
                        sides.Add(4);
                    }
                }
            }
            
            if (collisionCount != 0 && !ticked)
            {
                //determine shortest collision point and cut the distance accordingly

                Vector2 collisionVector = new Vector2(float.MaxValue, float.MaxValue);
                for (int v = 0; v < vectorList.Count; v++)
                {
                    if (Vector2.Distance(new Vector2(x, y), (Vector2)vectorList[v]) < Vector2.Distance(new Vector2(x, y), collisionVector))
                    {
                        collisionVector = (Vector2)vectorList[v];
                    }
                }
                sizeX = Vector2.Distance(new Vector2(x, y), collisionVector);
                terminalPoint = collisionVector;

                //create particle effect

                Random r = new Random();
                if ((int)sides[vectorList.IndexOf(collisionVector)] == 1)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X - 4, collisionVector.Y, r.Next(-100, 0), r.Next(-200, 0), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
                else if ((int)sides[vectorList.IndexOf(collisionVector)] == 2)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X, collisionVector.Y, r.Next(0, 100), r.Next(-200, 0), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
                else if ((int)sides[vectorList.IndexOf(collisionVector)] == 3)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X, collisionVector.Y - 4, r.Next(-100, 100), r.Next(-200, 0), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
                else if ((int)sides[vectorList.IndexOf(collisionVector)] == 4)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X, collisionVector.Y, r.Next(-100, 100), r.Next(0, 100), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
            }

            for (int i = 0; i < handler.characterList.Count; i++)
            {
                GameObject obj = (GameObject)handler.characterList[i];
                if (obj.getType() != "PLAYER")
                {
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, new Vector2(obj.x + obj.sizeX / 2, obj.y), new Vector2(obj.x + obj.sizeX / 2, obj.y + obj.sizeY)))
                    {
                        Mob mob = (Mob)obj;
                        obj.vx += (float)Math.Cos(rotation) * 500f;
                        mob.applyDamage(90);
                        //terminalPoint = CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, new Vector2(obj.x + obj.sizeX / 2, obj.y), new Vector2(obj.x + obj.sizeX / 2, obj.y + obj.sizeY));
                        //sizeX = Vector2.Distance(new Vector2(x, y), terminalPoint);
                    }
                }
            }
            ticked = true;
        }

        public void yOrientedCollision()
        {
            int collisionCount = 0;
            ArrayList vectorList = new ArrayList();
            ArrayList sides = new ArrayList();
            float startY = (y / Game1.BLOCKSIZE);
            float maxY = startY + ((float)Math.Sin(rotation) * distance) / Game1.BLOCKSIZE;
            maxY = maxY > gMap.getSizeY() ? gMap.getSizeY() : maxY;
            maxY = maxY < 0 ? 0 : maxY;

            Vector2 initialPoint = new Vector2(x, y);
            Vector2 terminalPoint = new Vector2(x + distance * (float)Math.Cos(rotation), y + distance * (float)Math.Sin(rotation));

            if (startY > maxY)
            {
                float temp = startY;
                startY = maxY;
                maxY = temp;
            }
            for (float i = startY; i < maxY; i += 0.1f)
            {
                float checkX = (int)(x / Game1.BLOCKSIZE) + (i - (int)(y / Game1.BLOCKSIZE)) * (1/slope);
                checkX = checkX < 0 ? 0 : checkX;
                checkX = checkX > gMap.getSizeX() - 1 ? gMap.getSizeX() - 1 : checkX;
                int finalY = (int)Math.Round(i);
                finalY = finalY > gMap.getSizeY() - 1 ? gMap.getSizeY() - 1 : finalY;
                GenericBlock obj = gMap.getMap()[finalY, (int)Math.Round(checkX)];
                //obj.highlight();

                if (obj.isBlock())
                {
                    Vector2 blockTopLeft = new Vector2(obj.x, obj.y);
                    Vector2 blockTopRight = new Vector2(obj.x + obj.size, obj.y);
                    Vector2 blockBottomLeft = new Vector2(obj.x, obj.y + obj.size);
                    Vector2 blockBottomRight = new Vector2(obj.x + obj.size, obj.y + obj.size);

                    //check left bound of object -> 1
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockTopLeft, blockBottomLeft))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockTopLeft, blockBottomLeft));
                        sides.Add(1);
                    }
                    //check right bound of object -> 2
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockTopRight, blockBottomRight))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockTopRight, blockBottomRight));
                        sides.Add(2);
                    }
                    //check upper bound of object -> 3
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockTopLeft, blockTopRight))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockTopLeft, blockTopRight));
                        sides.Add(3);
                    }
                    //check left bound of object -> 4
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, blockBottomLeft, blockBottomRight))
                    {
                        collisionCount++;
                        vectorList.Add(CollisionDetector.lineIntersectionPoint(initialPoint, terminalPoint, blockBottomLeft, blockBottomRight));
                        sides.Add(4);
                    }
                }
            }
            
            if (collisionCount != 0 && !ticked)
            {
                //determine shortest collision point and cut the distance accordingly

                Vector2 collisionVector = new Vector2(float.MaxValue, float.MaxValue);
                for (int v = 0; v < vectorList.Count; v++)
                {
                    if (Vector2.Distance(new Vector2(x, y), (Vector2)vectorList[v]) < Vector2.Distance(new Vector2(x, y), collisionVector))
                    {
                        collisionVector = (Vector2)vectorList[v];
                    }
                }
                sizeX = Vector2.Distance(new Vector2(x, y), collisionVector);
                terminalPoint = collisionVector;

                //create particle effect

                Random r = new Random();
                if ((int)sides[vectorList.IndexOf(collisionVector)] == 1)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X - 4, collisionVector.Y, r.Next(-100, 0), r.Next(-200, 0), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
                else if ((int)sides[vectorList.IndexOf(collisionVector)] == 2)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X, collisionVector.Y, r.Next(0, 100), r.Next(-200, 0), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
                else if ((int)sides[vectorList.IndexOf(collisionVector)] == 3)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X, collisionVector.Y - 4, r.Next(-100, 100), r.Next(-200, 0), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
                else if ((int)sides[vectorList.IndexOf(collisionVector)] == 4)
                {
                    for (int c = 0; c < particleCount; c++)
                        handler.addObject(new Particle(collisionVector.X, collisionVector.Y, r.Next(-100, 100), r.Next(0, 100), 9.8f * 120, "collideWithBlock", Color.White, handler, gMap, content), ObjectType.type.particles);
                }
            }

            for (int i = 0; i < handler.characterList.Count; i++)
            {
                GameObject obj = (GameObject)handler.characterList[i];
                if (obj.getType() != "PLAYER")
                {
                    if (CollisionDetector.lineIntersects(initialPoint, terminalPoint, new Vector2(obj.x + obj.sizeX / 2, obj.y), new Vector2(obj.x + obj.sizeX / 2, obj.y + obj.sizeY)))
                    {
                        Mob mob = (Mob)obj;
                        obj.vx += (float)Math.Cos(rotation) * 500f;
                        mob.applyDamage(damage);
                    }
                }
            }
            ticked = true;
        }

    }
}
