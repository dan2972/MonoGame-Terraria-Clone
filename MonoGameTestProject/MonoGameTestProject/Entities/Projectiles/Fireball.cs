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
    class Fireball : GameObject
    {

        Texture2D texture;
        private float rotation = 0;

        private Handler handler;
        private GameMap gMap;
        private Camera camera;
        private ContentManager content;

        private Random seed;

        int scaledX;
        int scaledY;
        int scaledWidth;
        int scaledHeight;

        private float gravity = 9.8f * 120f;
        private float speed = 800;
        private float lifeTimeCollected = 0, lifeTime = 10f;
        private float trailPartcleTimeCollected = 0;

        //private SoundEffect shootingSound;

        public Fireball(float x, float y, Handler handler, GameMap gMap, Camera camera, ContentManager content) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;
            this.camera = camera;

            sizeX = 8;
            sizeY = 8;

            seed = new Random();

            //translate mouse position according to the camera
            MouseState state = Mouse.GetState();
            float offsetX, offsetY;
            offsetX = Game1.TRUEGAMEWIDTH / 2 - (-camera.getX());
            offsetY = Game1.TRUEGAMEHEIGHT / 2 - (-camera.getY());
            float mx = (state.X - offsetX) / Game1.SCALE;
            float my = (state.Y - offsetY) / Game1.SCALE;

            float distanceToMouse = Vector2.Distance(new Vector2(x, y), new Vector2(mx, my));

            scaledWidth = (int)(sizeX * Game1.SCALE);
            scaledHeight = (int)(sizeY * Game1.SCALE);

            rotation = (float)Math.Atan2((my - y), (mx - x));

            vx = (mx - x) / distanceToMouse * speed;
            vy = (my - y) / distanceToMouse * speed;

            texture = content.Load<Texture2D>("projectilesprites/bullet_line");
            //shootingSound = content.Load<SoundEffect>("soundfx/bulletBasic");
            //shootingSound.Play(0.1f, 0, 0);
        }

        public override string getType()
        {
            return "FIREBALL";
        }

        public override void render(SpriteBatch sb)
        {
            scaledX = (int)(x * Game1.SCALE);
            scaledY = (int)(y * Game1.SCALE);

            sb.Draw(texture, destinationRectangle: new Rectangle((int)scaledX, (int)scaledY, scaledWidth, scaledHeight), origin: new Vector2(texture.Width/2, texture.Height/2), rotation: rotation, color: new Color(255, 150, 0));

        }

        public override void tick(float gTime)
        {
            x += vx * gTime;
            y += vy * gTime;

            vy += gravity * gTime;
            if (vy >= 800)
                vy = 800;

            rotation = (float)Math.Atan2(vy, vx);
            //rotation += 10f * gTime;

            trailPartcleTimeCollected += gTime;

            if (trailPartcleTimeCollected >= (1f/60f))
            {
                trailPartcleTimeCollected = 0;
                for (int i = 0; i < 2; i++)
                {
                    Random r = new Random(seed.Next());
                    Particle trailParticle = new Particle(x - 4, y - 4, ((float)r.NextDouble() * 50) - 25, ((float)r.NextDouble() * 50) - 25, -60f, "collideAndDestroy", new Color(255, 150, 0), handler, gMap, content);
                    handler.addObject(trailParticle, ObjectType.type.particles);
                    trailParticle.fadeToColor(Color.Black, 1.5f);
                    trailParticle.size = 8;
                    trailParticle.shrinkEnabled = true;
                    trailParticle.shrinkSpeed = 4f;
                    trailParticle.emitLight = true;
                    trailParticle.setLightFadeSpeed(1.5f);
                }
            }

            int mapX = (int)x / (int)Game1.BLOCKSIZE;
            int mapY = (int)y / (int)Game1.BLOCKSIZE;
            if (mapX > 0 && mapY > 0 && mapX < Game1.MAPSIZEX && mapY < Game1.MAPSIZEY)
                gMap.getMap()[mapY, mapX].brightness = 1;

            lifeTimeCollected += gTime;
            if (lifeTimeCollected > lifeTime)
                destroy();

            checkCollision();
        }

        private void checkCollision()
        {
            int xMap = (int)x / (int)Game1.BLOCKSIZE;
            int yMap = (int)y / (int)Game1.BLOCKSIZE + 1;
            int minCheckX = xMap - 2 < 0 ? 0 : xMap - 2;
            int maxCheckX = xMap + 2 >= 255 ? 255 : xMap + 2;
            int minCheckY = yMap - 2 < 0 ? 0 : yMap - 2;
            int maxCheckY = yMap + 2 >= 255 ? 255 : yMap + 2;
            for (int i = minCheckY; i <= maxCheckY; i++)
            {
                for (int j = minCheckX; j <= maxCheckX; j++)
                {
                    GenericBlock obj = gMap.getMap()[i, j];
                    if (obj.isBlock())
                    {
                        if (CollisionDetector.intersects(x - sizeX / 2, y - sizeY / 2, sizeX, sizeY, obj.x, obj.y, obj.size, obj.size))
                        {
                            destroy();
                        }
                    }
                }
            }
            for(int i = 0; i < handler.characterList.Count; i ++)
            {
                GameObject obj = (GameObject)handler.characterList[i];
                if(obj.getType() != "PLAYER")
                {
                    if (CollisionDetector.intersects(x, y, sizeX, sizeY, obj.x, obj.y, obj.sizeX, obj.sizeY))
                    {
                        Mob mob = (Mob)obj;
                        obj.vx += vx/3;
                        mob.applyDamage(30);
                        destroy();
                    }
                }
            }
        }

        private void destroy()
        {
            for (int i = 0; i < 10; i++)
            {
                Random r = new Random(seed.Next() + i);
                Particle p = new Particle(x, y, ((float)r.NextDouble() * 150) - 75, ((float)r.NextDouble() * 150) - 75, -60f, "none", new Color(255, 200, 200), handler, gMap, content);
                handler.addObject(p, ObjectType.type.particles);
                p.fadeToColor(Color.Black, 0.8f);
                p.size = 8;
                p.shrinkEnabled = true;
                p.shrinkSpeed = 4f;
            }
            handler.removeObject(this, ObjectType.type.projectiles);
        }

    }
}
