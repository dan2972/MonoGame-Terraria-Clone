using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Entities.Mobs;
using MonoGameTestProject.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Blocks
{
    class GenericBlock : GameObject
    {

        Texture2D texture;
        Texture2D outlineTexture;
        ContentManager content;

        public float brightness { get; set; }
        public bool wallBehind { get; set; }
        public ItemID.ID wallBehindID { get; set; }

        private Handler handler;
        private GameMap gMap;

        private String fileName;
        private ItemID.ID id;

        private float alpha = 1;
        private float scale = 1;
        private float lightReductionRate = 0.18f;

        private bool[] outlineSide;
        private bool renderFullOutline;
        private bool highlighted = false;

        public GenericBlock(float x, float y, Handler handler, GameMap gMap, ContentManager content, ItemID.ID id) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;
            size = Game1.BLOCKSIZE;

            outlineSide = new bool[4];

            wallBehind = false;
            wallBehindID = ItemID.ID.NULL;

            applyFileName();
            
            texture = content.Load<Texture2D>("blocksprites/" + fileName);
            outlineTexture = content.Load<Texture2D>("projectilesprites/particle_basic");

            if ((int)id >= 5000 || (int)id == 0)
                lightReductionRate = 0.07f;

        }

        public GenericBlock(float x, float y, Handler handler, GameMap gMap, ContentManager content, ItemID.ID id, ItemID.ID wallBehindID) : base(x, y)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.handler = handler;
            this.gMap = gMap;
            this.content = content;
            this.wallBehindID = wallBehindID;
            size = Game1.BLOCKSIZE;

            outlineSide = new bool[4];

            wallBehind = true;

            applyFileName();

            texture = content.Load<Texture2D>("blocksprites/" + fileName);
            outlineTexture = content.Load<Texture2D>("projectilesprites/particle_basic");

            if ((int)id >= 5000 || (int)id == 0)
                lightReductionRate = 0.07f;

        }

        private void applyFileName()
        {
            switch (id)
            {
                case ItemID.ID.grass_block:
                    fileName = "dirt_basic_grass";
                    break;
                case ItemID.ID.dirt_block:
                    fileName = "dirt_basic";
                    break;
                case ItemID.ID.stone_block:
                    fileName = "stone_basic";
                    break;
                case ItemID.ID.dirt_wall:
                    fileName = "dirt_wall";
                    break;
                case ItemID.ID.stone_wall:
                    fileName = "stone_wall";
                    break;
                default:
                    fileName = "crate";
                    break;
            }
        }

        public override string getType()
        {
            if ((int)id == 0)
                return "AIR";
            if (isWall())
                return "WALL";
            return "BLOCK";
        }

        public override void render(SpriteBatch sb)
        {
            int scaledX = (int)(x * Game1.SCALE);
            int scaledY = (int)(y * Game1.SCALE);
            int scaledWidth = (int)(size * Game1.SCALE * scale);
            int scaledHeight = (int)(size * Game1.SCALE * scale);

            if ((int)id != 0)
            {
                if (renderFullOutline)
                    sb.Draw(outlineTexture, destinationRectangle: new Rectangle(scaledX - 1, scaledY - 1, scaledWidth + 2, scaledHeight + 2), color: new Color(200, 200, 200));

                sb.Draw(texture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), color: Color.White * alpha);
                if (alpha < 1)
                {
                    alpha += 0.1f;
                }

                int strokeWeight = (int)(1 * Game1.SCALE);
                if (outlineSide[0])
                    sb.Draw(outlineTexture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, strokeWeight), color: Color.Black);
                if (outlineSide[1])
                    sb.Draw(outlineTexture, destinationRectangle: new Rectangle(scaledX + scaledWidth - strokeWeight, scaledY, strokeWeight, scaledWidth), color: Color.Black);
                if (outlineSide[2])
                    sb.Draw(outlineTexture, destinationRectangle: new Rectangle(scaledX, scaledY + scaledWidth - strokeWeight, scaledWidth, strokeWeight), color: Color.Black);
                if (outlineSide[3])
                    sb.Draw(outlineTexture, destinationRectangle: new Rectangle(scaledX, scaledY, strokeWeight, scaledWidth), color: Color.Black);
            }
            if (highlighted)
                sb.Draw(outlineTexture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight), color: new Color(255, 255, 255) * 0.7f);
        }

        public override void tick(float gTime)
        {
            if(id == ItemID.ID.grass_block)
            {
                for (int i = 0; i < handler.characterList.Count; i++)
                {
                    GameObject obj = (GameObject)handler.characterList[i];
                    if (obj.getType() == "PLAYER")
                    {
                        if (gMap.getGlobalTime() >= 18 || gMap.getGlobalTime() < 5)
                        {
                            Random r = new Random();
                        
                            if(r.NextDouble() < 0.02f)
                                handler.addObject(new Zombie(x, y - 64, (Player)obj, handler, gMap, content), ObjectType.type.characters);
                        }
                    }
                }
            }
        }

        public void setRenderFullOutline(bool renderFullOutline)
        {
            this.renderFullOutline = renderFullOutline;
        }

        public void setOutline(int side, bool b)
        {
            outlineSide[side] = b;
        }

        public void setScale(float scale)
        {
            this.scale = scale;
        }

        public float getScale()
        {
            return scale;
        }

        public float getLightReductionRate()
        {
            return lightReductionRate;
        }

        public void highlight()
        {
            //highlighted = true;
            alpha = 0.5f;
        }

        public ItemID.ID getID()
        {
            return id;
        }

        public bool isWall()
        {
            if ((int)id >= 5000 && (int)id < 9000)
                return true;
            else
                return false;
        }

        public bool isBlock()
        {
            if ((int)id >= 5000)
                return false;
            else if ((int)id == 0)
                return false;
            else return true;
        }

    }
}
