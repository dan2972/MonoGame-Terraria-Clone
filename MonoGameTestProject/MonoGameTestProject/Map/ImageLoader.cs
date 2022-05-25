using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Entities;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Entities.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Map
{
    class ImageLoader
    {

        Texture2D mapTexture;
        Handler handler;
        GameMap gMap;
        Color[] rawData;
        Color[,] colorGrid;
        ContentManager content;

        private int width, height;

        public ImageLoader(Texture2D mapTexture, Handler handler, GameMap gMap, ContentManager content)
        {
            this.mapTexture = mapTexture;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;

            //mapTexture = content.Load<Texture2D>("map");

            width = mapTexture.Width;
            height = mapTexture.Height;
            Color[] rawData = new Color[width * height];
            mapTexture.GetData<Color>(0, new Rectangle(0,0, width, height), rawData, 0, width * height);
            colorGrid = new Color[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    colorGrid[row, column] = rawData[row * width + column];
                }
            }
        }

        public void loadParticles(float x, float y)
        {
            Random seed = new Random();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int r = colorGrid[i, j].R;
                    int g = colorGrid[i, j].G;
                    int b = colorGrid[i, j].B;
                    if (r == 99 && g == 155 && b == 255)
                    {
                        Random rand = new Random(seed.Next());
                        Particle particle = new Particle(x + j*2, y + i*2, ((float)rand.NextDouble() * 10) - 5, ((float)rand.NextDouble() * 10) - 5, 6f, "collideAndDestroy", new Color(255, 255, 255), handler, gMap, content);
                        handler.addObject(particle, ObjectType.type.particles);
                        //trailParticle.fadeToColor(Color.White, 0.2f);
                        particle.size = 4;
                        particle.shrinkEnabled = true;
                        particle.shrinkSpeed = 0.2f;
                        particle.fadeSpeed = 0.5f;
                        particle.emitLight = true;
                        particle.setLightFadeSpeed(0.5f);
                    }
                }
            }
        }

        public void loadMap()
        {
            float size = Game1.BLOCKSIZE;
            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    int r = colorGrid[i, j].R;
                    int g = colorGrid[i, j].G;
                    int b = colorGrid[i, j].B;
                    
                    if (r == 0 && g == 255 && b == 0)
                        gMap.getMap()[i,j] = new GenericBlock(j * size, i * size, handler, gMap, content, ItemID.ID.grass_block);
                    else if (r == 100 && g == 0 && b == 0)
                        gMap.getMap()[i, j] = new GenericBlock(j * size, i * size, handler, gMap, content, ItemID.ID.dirt_block, ItemID.ID.dirt_wall);
                    else if (r == 100 && g == 100 && b == 100)
                        gMap.getMap()[i, j] = new GenericBlock(j * size, i * size, handler, gMap, content, ItemID.ID.stone_block, ItemID.ID.stone_wall);
                    else if (r == 73 && g == 0 && b == 0)
                        gMap.getMap()[i, j] = new GenericBlock(j * size, i * size, handler, gMap, content, ItemID.ID.dirt_wall);
                    else if (r == 64 && g == 64 && b == 64)
                        gMap.getMap()[i, j] = new GenericBlock(j * size, i * size, handler, gMap, content, ItemID.ID.stone_wall);
                    else
                        gMap.getMap()[i, j] = new GenericBlock(j * size, i * size, handler, gMap, content, ItemID.ID.air_block);
                }
            }
        }

    }
}
