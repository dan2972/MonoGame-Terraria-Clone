using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Entities;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Misc;
using MonoGameTestProject.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Map
{
    class GameMap
    {

        public GenericBlock[,] gMap;
        public LightBlock[,] lightMap;

        private Camera cam;

        private int sizeX, sizeY;

        private float globalTime = 0;
        private int globalTimeSec = 0;
        private float globalBrightness = 1;
        private double sunRotation = Math.PI / 2;

        public GameMap(int sizeX, int sizeY, Camera cam, ContentManager content)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.cam = cam;
            gMap = new GenericBlock[sizeX, sizeY];

            lightMap = new LightBlock[sizeX, sizeY];
            for(int i = 0; i < sizeY; i++)
            {
                for(int j = 0; j < sizeX; j++)
                {
                    lightMap[i, j] = new LightBlock(j * Game1.BLOCKSIZE, i * Game1.BLOCKSIZE, content);
                }
            }
        }

        public void applyVisualAffects()
        {
            int xMap = (int)-(cam.getX() / Game1.SCALE) / (int)Game1.BLOCKSIZE;
            int yMap = (int)-(cam.getY() / Game1.SCALE) / (int)Game1.BLOCKSIZE;
            int renderDistX = Game1.GAMEWIDTH / (int)Game1.BLOCKSIZE / 2 + 10;
            int renderDistY = Game1.GAMEHEIGHT / (int)Game1.BLOCKSIZE / 2 + 1 + 10;
            int minCheckX = xMap - renderDistX < 0 ? 0 : xMap - renderDistX;
            int maxCheckX = xMap + renderDistX >= sizeX - 1 ? sizeX - 1 : xMap + renderDistX;
            int minCheckY = yMap - renderDistY < 0 ? 0 : yMap - renderDistY;
            int maxCheckY = yMap + renderDistY >= sizeY - 1 ? sizeY - 1 : yMap + renderDistY;

            for (int i = minCheckY; i <= maxCheckY; i++)
            {
                for (int j = minCheckX; j <= maxCheckX; j++)
                {
                    gMap[i, j].brightness = 0;
                    GenericBlock obj = gMap[i, j];

                    if (!obj.isWall())
                        applyOutline(j, i);
                    float largestBrightness = 0f;

                    //check blocks on each edge
                    if (i > 0 && gMap[i - 1, j].brightness > largestBrightness)
                        largestBrightness = gMap[i - 1, j].brightness;
                    if (i < sizeY - 1 && gMap[i + 1, j].brightness > largestBrightness)
                        largestBrightness = gMap[i + 1, j].brightness;
                    if (j > 0 && gMap[i, j - 1].brightness > largestBrightness)
                        largestBrightness = gMap[i, j - 1].brightness;
                    if (j < sizeX - 1 && gMap[i, j + 1].brightness > largestBrightness)
                        largestBrightness = gMap[i, j + 1].brightness;
                    float lbBeforeCornerCheck = largestBrightness;

                    //check blocks on each corner
                    if (i > 0 && j > 0 && gMap[i - 1, j - 1].brightness > largestBrightness)
                        largestBrightness = gMap[i - 1, j - 1].brightness;
                    if (i > 0 && j < sizeX - 1 && gMap[i - 1, j + 1].brightness > largestBrightness)
                        largestBrightness = gMap[i - 1, j + 1].brightness;
                    if (i < sizeY - 1 && j > 0 && gMap[i + 1, j - 1].brightness > largestBrightness)
                        largestBrightness = gMap[i + 1, j - 1].brightness;
                    if (i < sizeY - 1 && j < sizeX - 1 && gMap[i + 1, j + 1].brightness > largestBrightness)
                        largestBrightness = gMap[i + 1, j + 1].brightness;

                    float newLightLevel;

                    if (lbBeforeCornerCheck < largestBrightness)
                    {
                        newLightLevel = largestBrightness - gMap[i, j].getLightReductionRate() * (float)Math.Sqrt(2);//(largestBrightness * gMap[i, j].getLightPassStrength() * (float)Math.Sqrt(2));
                    }
                    else
                    {
                        newLightLevel = largestBrightness - gMap[i, j].getLightReductionRate();//(largestBrightness * gMap[i, j].getLightPassStrength());
                    }

                    gMap[i, j].brightness = newLightLevel;

                    if (blockTouchingAir(j, i))
                    {
                        if (newLightLevel < globalBrightness)
                            gMap[i, j].brightness = globalBrightness;
                        else
                            gMap[i, j].brightness = newLightLevel;
                    }
                    
                    lightMap[i, j].setBrightness(gMap[i, j].brightness);
                    if (gMap[i, j].getID() == ItemID.ID.air_block)
                        lightMap[i, j].setBrightness(1);
                }
            }
        }

        public void tick(float gTime)
        {
            globalBrightness = (float)(Math.Sin(sunRotation) + 1.2) * 0.5f;
            sunRotation += ((2 * Math.PI) / 24) / 60 * gTime;
            if (sunRotation >= 2 * 3.14f)
                sunRotation = 0;
            globalTime = ((float)sunRotation + (3.14f / 2)) / 3.14f * 12;
            if (sunRotation + (3.14f / 2) > 2 * 3.14f)
                globalTime -= 24;
            globalTimeSec = (int)((globalTime - (int)globalTime) * 60);

            Random r = new Random();
            for (int i = 0; i < 50; i++)
            {
                int x = r.Next(0, sizeX);
                int y = r.Next(0, sizeY);
                //gMap[x, y].highlight();
                gMap[y, x].tick(gTime);
            }
            applyVisualAffects();
        }

        public void render(SpriteBatch sb)
        {
            int xMap = (int)-(cam.getX()/Game1.SCALE) / (int)Game1.BLOCKSIZE;
            int yMap = (int)-(cam.getY()/Game1.SCALE) / (int)Game1.BLOCKSIZE;
            int renderDistX = Game1.GAMEWIDTH / (int)Game1.BLOCKSIZE / 2;
            int renderDistY = Game1.GAMEHEIGHT / (int)Game1.BLOCKSIZE / 2 + 1;
            int minCheckX = xMap - renderDistX < 0 ? 0 : xMap - renderDistX;
            int maxCheckX = xMap + renderDistX >= sizeX - 1 ? sizeX - 1 : xMap + renderDistX;
            int minCheckY = yMap - renderDistY < 0 ? 0 : yMap - renderDistY;
            int maxCheckY = yMap + renderDistY >= sizeY - 1 ? sizeY - 1 : yMap + renderDistY;
            for (int i = minCheckY; i <= maxCheckY; i++)
            {
                for (int j = minCheckX; j <= maxCheckX; j++)
                {
                    GenericBlock gb = gMap[i, j];
                    if (gb.getID() != ItemID.ID.air_block && lightMap[i, j].getBrightness() != 0)
                    {
                        gMap[i, j].render(sb);
                    }
                }
            }
        }

        public void renderLight(SpriteBatch sb)
        {
            int xMap = (int)-(cam.getX() / Game1.SCALE) / (int)Game1.BLOCKSIZE;
            int yMap = (int)-(cam.getY() / Game1.SCALE) / (int)Game1.BLOCKSIZE;
            int renderDistX = Game1.GAMEWIDTH / (int)Game1.BLOCKSIZE / 2;
            int renderDistY = Game1.GAMEHEIGHT / (int)Game1.BLOCKSIZE / 2 + 1;
            int minCheckX = xMap - renderDistX < 0 ? 0 : xMap - renderDistX;
            int maxCheckX = xMap + renderDistX >= sizeX - 1 ? sizeX - 1 : xMap + renderDistX;
            int minCheckY = yMap - renderDistY < 0 ? 0 : yMap - renderDistY;
            int maxCheckY = yMap + renderDistY >= sizeY - 1 ? sizeY - 1 : yMap + renderDistY;
            for (int i = minCheckY; i <= maxCheckY; i++)
            {
                for (int j = minCheckX; j <= maxCheckX; j++)
                {
                    lightMap[i, j].render(sb);
                }
            }
        }

        private bool blockTouchingAir(int x, int y)
        {
            if ((y > 0 && gMap[y - 1, x].getID() == ItemID.ID.air_block) || (y < sizeY - 1 && gMap[y + 1, x].getID() == ItemID.ID.air_block) || (x > 0 && gMap[y, x - 1].getID() == ItemID.ID.air_block) || (x < sizeX - 1 && gMap[y, x + 1].getID() == ItemID.ID.air_block))
            {
                return true;
            }/*
            if ((y > 0 && gMap[y - 1, x].isWall()) || (y < sizeY - 1 && gMap[y + 1, x].isWall()) || (x > 0 && gMap[y, x - 1].isWall()) || (x < sizeX - 1 && gMap[y, x + 1].isWall()))
            {
                return true;
            }*/
            return false;
        }

        private void applyOutline(int x, int y)
        {
            if ((y > 0 && gMap[y - 1, x].getID() == ItemID.ID.air_block) || (y > 0 && gMap[y - 1, x].isWall()))
                gMap[y, x].setOutline(0, true);
            else
                gMap[y, x].setOutline(0, false);
            if ((y < sizeY - 1 && gMap[y + 1, x].getID() == ItemID.ID.air_block) || (y < sizeY - 1 && gMap[y + 1, x].isWall()))
                gMap[y, x].setOutline(2, true);
            else
                gMap[y, x].setOutline(2, false);
            if ((x > 0 && gMap[y, x - 1].getID() == ItemID.ID.air_block) || (x > 0 && gMap[y, x - 1].isWall()))
                gMap[y, x].setOutline(3, true);
            else
                gMap[y, x].setOutline(3, false);
            if ((x < sizeX - 1 && gMap[y, x + 1].getID() == ItemID.ID.air_block) || (x < sizeX - 1 && gMap[y, x + 1].isWall()))
                gMap[y, x].setOutline(1, true);
            else
                gMap[y, x].setOutline(1, false);
        }

        public GenericBlock[,] getMap()
        {
            return gMap;
        }

        public int getSizeX()
        {
            return sizeX;
        }

        public int getSizeY()
        {
            return sizeY;
        }

        public int getGlobalTime()
        {
            return (int)globalTime;
        }

        public int getGlobalTimeSec()
        {
            return globalTimeSec;
        }

        public float getGlobalBrightness()
        {
            return globalBrightness;
        }

        public float getSunRotation()
        {
            return (float)sunRotation;
        }

    }
}
