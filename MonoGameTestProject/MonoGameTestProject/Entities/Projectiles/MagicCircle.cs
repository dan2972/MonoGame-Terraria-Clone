using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Projectiles
{
    class MagicCircle : GameObject
    {

        Texture2D texture;
        private SoundEffect summonSound;
        private Handler handler;
        private GameMap gMap;

        private ImageLoader ml;

        private Random seed;

        private Rectangle sourceRectangle;

        private float alpha = 1;
        private float animationDelay = 0;

        public MagicCircle(float x, float y, Handler handler, GameMap gMap, ContentManager content, String spellType) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.handler = handler;
            this.gMap = gMap;
            size = 256;

            sourceRectangle = new Rectangle(0, 0, 128, 128);

            texture = content.Load<Texture2D>("spellcastsprites/" + spellType);
            summonSound = content.Load<SoundEffect>("soundfx/summon_gong");
            summonSound.Play(0.9f, 0, 0);

            ml = new ImageLoader(texture, handler, gMap, content);
            ml.loadParticles(x, y);

            seed = new Random();

            for(int i = 0; i < 500; i++)
            {
                Random r = new Random(seed.Next() + i);
                Random r2 = new Random(seed.Next() + i);
                double angle = r.NextDouble() * 2 * Math.PI;
                Particle p = new Particle(x + 128, y + 128, r2.Next(190,210)*(float)Math.Cos(angle), r2.Next(190, 210) * (float)Math.Sin(angle), 0f, "collideAndDestroy", Color.White, handler, gMap, content);
                p.fadeSpeed = 0.3f;
                p.setFriction(0f);
                p.emitLight = true;
                p.setLightFadeSpeed(0.3f);
                handler.addObject(p, ObjectType.type.particles);
            }
        }

        public override string getType()
        {
            return "MAGIC_CIRCLE";
        }

        public override void render(SpriteBatch sb)
        {
            int scaledX = (int)(x * Game1.SCALE);
            int scaledY = (int)(y * Game1.SCALE);
            int scaledWidth = (int)(size * Game1.SCALE);
            int scaledHeight = (int)(size * Game1.SCALE);
            //sb.Draw(texture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), sourceRectangle: sourceRectangle, color: Color.White * alpha);
        }

        public override void tick(float gTime)
        {
            //animationDelay += gTime;

            alpha -= 0.5f * gTime;

            if (alpha <= 0)
            {
                handler.removeObject(this, ObjectType.type.projectiles);
            }

            /*
            if (animationDelay >= 0.1)
            {
                animationDelay = 0;
                sourceRectangle.X += 128;
            }

            if(sourceRectangle.X > 128)
            {
                sourceRectangle.X = 0;
            }*/
        }

    }
}
