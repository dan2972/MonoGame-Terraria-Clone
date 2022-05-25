using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Calculations;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Map;
using MonoGameTestProject.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Item
{
    class GenericItem
    {
        public int count {get; set;}
        public float x, y;

        public int maxCount { get; set; }

        private float vx, vy;
        private float floatHeight, floatRadians;
        private float scale = 1f;

        private String fileName;

        private ItemID.ID id;
        private GenericBlock block;
        private GameMap gMap;
        private Handler handler;
        private Inventory inventory;
        private Texture2D texture;

        private bool pickedUp = false;
        private bool collisionEnabled = true;
        private bool itemIsBlock = true;

        public GenericItem(float x, float y, GameMap gMap, Handler handler, Inventory inventory, ContentManager content, ItemID.ID id)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.gMap = gMap;
            this.handler = handler;
            this.inventory = inventory;

            if ((int)id >= 9000)
                itemIsBlock = false;
            if (itemIsBlock)
            {
                block = new GenericBlock(x, y, handler, gMap, content, id);
                block.setScale(0.7f);
                block.setRenderFullOutline(true);

                vy = -50f;
            }
            else
            {
                applyFileName();

                texture = content.Load<Texture2D>("itemsprites/" + fileName);
            }

            Random r = new Random();
            vx = (float)r.NextDouble()*20f - 10;

            initMaxCount();
        }

        private void applyFileName()
        {
            switch (id)
            {
                case ItemID.ID.fire_wand:
                    fileName = "fire_wand";
                    break;
                case ItemID.ID.pistol_basic:
                    fileName = "pistol_basic";
                    break;
                case ItemID.ID.stone_hammer:
                    fileName = "stone_hammer";
                        break;
                default:
                    fileName = "fire_wand";
                    break;
            }
        }

        public void tick(float gTime)
        {
            
            if (!pickedUp)
            {
                if (itemIsBlock)
                {
                    tickAsBlock(gTime);
                }
                else
                {
                    tickAsNonBlock(gTime);
                }
            }
        }

        private void tickAsBlock(float gTime)
        {
            block.x = x;
            block.y = y - floatHeight;

            floatHeight = 6 * (float)Math.Sin(floatRadians);
            floatRadians += gTime;
            if (floatRadians >= Math.PI)
                floatRadians = 0;

            x += vx * gTime;
            y += vy * gTime;

            if (vx > 0.2f)
                vx -= 10f * gTime;
            else if (vx < 0.2f)
                vx += 10f * gTime;
            else
                vx = 0;

            vy += 9.8f * 120 * gTime;
            if (vy >= 800)
                vy = 800;

            int xMap = (int)x / (int)Game1.BLOCKSIZE;
            int yMap = (int)y / (int)Game1.BLOCKSIZE + 1;
            int minCheckX = xMap - 2 < 0 ? 0 : xMap - 2;
            int maxCheckX = xMap + 2 > 255 ? 255 : xMap + 2;
            int minCheckY = yMap - 2 < 0 ? 0 : yMap - 2;
            int maxCheckY = yMap + 2 > 255 ? 255 : yMap + 2;

            float blockS = block.size * block.getScale();

            if (collisionEnabled)
            {
                for (int i = minCheckY; i <= maxCheckY; i++)
                {
                    for (int j = minCheckX; j <= maxCheckX; j++)
                    {
                        GenericBlock obj = gMap.getMap()[i, j];
                        if (obj.getID() != ItemID.ID.air_block && !obj.isWall())
                        {
                            if (obj.getType() == "BLOCK")
                            {
                                if (CollisionDetector.intersects(x + (vx * gTime), y + (vy * gTime), blockS, blockS, obj.x, obj.y, obj.size, obj.size))
                                {
                                    if (CollisionDetector.intersects(x, y + blockS + (vy * gTime), blockS, 1, obj.x, obj.y, obj.size, obj.size) && vy > 0)
                                    {
                                        vy = 0;
                                        y = obj.y - blockS;
                                    }
                                    else if (CollisionDetector.intersects(x, y + (vy * gTime), blockS, blockS, obj.x, obj.y + obj.size, obj.size, 1) && vy < 0)
                                    {
                                        vy = 0;
                                        y = obj.y + obj.size;
                                    }
                                    else if (!CollisionDetector.intersects(x, y + blockS, blockS, 1, obj.x, obj.y, obj.size, obj.size))
                                    {
                                        if (vx > 0)
                                        {
                                            x = obj.x - blockS;
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
                }
            }
            for (int i = 0; i < handler.characterList.Count; i++)
            {
                GameObject obj = (GameObject)handler.characterList[i];
                if (obj.getType() == "PLAYER")
                {
                    if (CollisionDetector.intersects(x - 16, y - 16 - floatHeight, blockS + 32, blockS + 32, obj.x, obj.y, obj.sizeX, obj.sizeY))
                    {
                        collisionEnabled = false;
                    }

                    if (!collisionEnabled)
                    {
                        vx = ((obj.x + (obj.sizeX / 2)) - (x - floatHeight + blockS / 2)) * 6f;
                        vy = ((obj.y + (obj.sizeY / 2)) - (y - floatHeight + blockS / 2)) * 6f;
                    }

                    if (CollisionDetector.intersects(x, y - floatHeight, blockS, blockS, obj.x + obj.sizeX / 4, obj.y + obj.sizeY / 4, 0.5f * obj.sizeX, 0.5f * obj.sizeY))
                    {
                        if (inventory.addItem(this))
                            block.setScale(1.5f);
                    }
                }
            }
        }

        private void tickAsNonBlock(float gTime)
        {
            floatHeight = 6 * (float)Math.Sin(floatRadians);
            floatRadians += gTime;
            if (floatRadians >= Math.PI)
                floatRadians = 0;

            x += vx * gTime;
            y += vy * gTime;

            if (vx > 0.2f)
                vx -= 10f * gTime;
            else if (vx < 0.2f)
                vx += 10f * gTime;
            else
                vx = 0;

            vy += 9.8f * 120 * gTime;
            if (vy >= 800)
                vy = 800;

            int xMap = (int)x / (int)Game1.BLOCKSIZE;
            int yMap = (int)y / (int)Game1.BLOCKSIZE + 1;
            int minCheckX = xMap - 2 < 0 ? 0 : xMap - 2;
            int maxCheckX = xMap + 2 > 255 ? 255 : xMap + 2;
            int minCheckY = yMap - 2 < 0 ? 0 : yMap - 2;
            int maxCheckY = yMap + 2 > 255 ? 255 : yMap + 2;

            float itemSize = 16 * scale;

            if (collisionEnabled)
            {
                for (int i = minCheckY; i <= maxCheckY; i++)
                {
                    for (int j = minCheckX; j <= maxCheckX; j++)
                    {
                        GameObject obj = gMap.getMap()[i, j];
                        if (obj != null)
                        {
                            if (obj.getType() == "BLOCK")
                            {
                                if (CollisionDetector.intersects(x + (vx * gTime), y + (vy * gTime), itemSize, itemSize, obj.x, obj.y, obj.size, obj.size))
                                {
                                    if (CollisionDetector.intersects(x, y + itemSize + (vy * gTime), itemSize, 1, obj.x, obj.y, obj.size, obj.size) && vy > 0)
                                    {
                                        vy = 0;
                                        y = obj.y - itemSize;
                                    }
                                    else if (CollisionDetector.intersects(x, y + (vy * gTime), itemSize, itemSize, obj.x, obj.y + obj.size, obj.size, 1) && vy < 0)
                                    {
                                        vy = 0;
                                        y = obj.y + obj.size;
                                    }
                                    else if (!CollisionDetector.intersects(x, y + itemSize, itemSize, 1, obj.x, obj.y, obj.size, obj.size))
                                    {
                                        if (vx > 0)
                                        {
                                            x = obj.x - itemSize;
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
                }
            }
            for (int i = 0; i < handler.characterList.Count; i++)
            {
                GameObject obj = (GameObject)handler.characterList[i];
                if (obj.getType() == "PLAYER")
                {
                    if (CollisionDetector.intersects(x - 16, y - 16 - floatHeight, itemSize + 32, itemSize + 32, obj.x, obj.y, obj.sizeX, obj.sizeY))
                    {
                        collisionEnabled = false;
                    }

                    if (!collisionEnabled)
                    {
                        vx = ((obj.x + (obj.sizeX / 2)) - (x - floatHeight + itemSize / 2)) * 6f;
                        vy = ((obj.y + (obj.sizeY / 2)) - (y - floatHeight + itemSize / 2)) * 6f;
                    }

                    if (CollisionDetector.intersects(x, y - floatHeight, itemSize, itemSize, obj.x + obj.sizeX / 4, obj.y + obj.sizeY / 4, 0.5f * obj.sizeX, 0.5f * obj.sizeY))
                    {
                        if (inventory.addItem(this))
                            scale = 1.5f;
                    }
                }
            }
        }

        public void render(SpriteBatch sb)
        {
            if (itemIsBlock)
                block.render(sb);
            else
            {
                int scaledX = (int)(x * Game1.SCALE);
                int scaledY;
                if (pickedUp)
                    scaledY = (int)((y) * Game1.SCALE);
                else
                    scaledY = (int)((y - floatHeight) * Game1.SCALE);
                int scaledWidth = (int)(16 * scale * Game1.SCALE);
                int scaledHeight = (int)(16 * scale * Game1.SCALE);
                sb.Draw(texture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), color: Color.White);
            }
        }

        public void useItem()
        {
            count--;
        }

        public ItemID.ID getID()
        {
            return id;
        }

        public void setX(float x)
        {
            this.x = x;
            if(itemIsBlock)
                block.x = x;
        }

        public void setY(float y)
        {
            this.y = y;
            if (itemIsBlock)
                block.y = y;
        }

        public Boolean isPickedUp()
        {
            return pickedUp;
        }

        public void setPickedUp(Boolean pickedUp)
        {
            this.pickedUp = pickedUp;
        }

        private void initMaxCount()
        {
            switch ((int)id)
            {
                case 0:
                    maxCount = 99;
                    break;
                case 9000:
                    maxCount = 1;
                    break;
                default:
                    maxCount = 99;
                    break;
            }
        }

    }
}
