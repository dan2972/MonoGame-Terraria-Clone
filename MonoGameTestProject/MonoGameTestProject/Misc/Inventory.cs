using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameTestProject.Calculations;
using MonoGameTestProject.Entities;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Entities.Item;
using MonoGameTestProject.Map;
using MonoGameTestProject.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Misc
{
    class Inventory
    {
        public ItemID.ID selectedItemID { get; set; }

        private Texture2D hotbarTexture;
        private Texture2D hotbarHighlightTexture;
        private Camera cam;
        
        private GenericItem[] hotbar;
        private GenericItem[] inventory;
        private GenericItem pickedUpItem;
        private GameMap gMap;
        private Handler handler;
        private ContentManager content;

        private SpriteFont basicfont;
        private Color fontColor;

        private int hotbarPosition = 0;
        private int hotbarX, hotbarY, hbXNoScale, hbYNoScale;

        private float previousScrollValue;

        private bool inventoryOpen = false;
        private bool previousInventoryKeyState = false;

        public Inventory(Camera cam, GameMap gMap, Handler handler, ContentManager content)
        {
            this.cam = cam;
            this.gMap = gMap;
            this.content = content;
            this.handler = handler;
            hotbar = new GenericItem[10];

            fontColor = new Color(new Vector4(255, 255, 255, 255));

            hotbarTexture = content.Load<Texture2D>("inventory_bar");
            hotbarHighlightTexture = content.Load<Texture2D>("hotbar_highlight");
            basicfont = content.Load<SpriteFont>("BasicText");

            GenericItem gi = new GenericItem(750, 100, gMap, handler, this, content, ItemID.ID.fire_wand);
            GenericItem gi2 = new GenericItem(782, 100, gMap, handler, this, content, ItemID.ID.pistol_basic);
            GenericItem gi3 = new GenericItem(814, 100, gMap, handler, this, content, ItemID.ID.stone_hammer);
            handler.addObject(gi);
            handler.addObject(gi2);
            handler.addObject(gi3);
        }

        public void render(SpriteBatch sb)
        {
            int scaledX = (int)(-cam.getX()-(192 * Game1.SCALE));
            int scaledY = (int)(-cam.getY()-(350 * Game1.SCALE));
            hbXNoScale = (int)(-cam.getX() / Game1.SCALE - 192);
            hbYNoScale = (int)(-cam.getY() / Game1.SCALE - 350);
            hotbarX = scaledX;
            hotbarY = scaledY;
            int scaledWidth = (int)(384 * Game1.SCALE);
            int scaledHeight = (int)(40 * Game1.SCALE);
            
            if (inventoryOpen)
            {
                for (int i = 0; i < 4; i++)
                {
                    sb.Draw(hotbarTexture, destinationRectangle: new Rectangle(scaledX, scaledY + (i * scaledHeight), scaledWidth, scaledHeight));
                }
            }
            else
            {
                sb.Draw(hotbarTexture, destinationRectangle: new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight));

                sb.Draw(hotbarHighlightTexture, destinationRectangle: new Rectangle(hotbarX + 1 + (hotbarPosition) * (int)(38 * Game1.SCALE), hotbarY, (int)(40 * Game1.SCALE), (int)(40 * Game1.SCALE)));
            }

            for (int i = 0; i < hotbar.Length; i++)
            {
                if(hotbar[i] != null)
                {
                    hotbar[i].setX(hbXNoScale + 8 + (i) * 38);
                    hotbar[i].setY(hbYNoScale + 8);
                    hotbar[i].render(sb);
                    sb.DrawString(basicfont, hotbar[i].count.ToString(), new Vector2(hotbarX + 4 + (i) * (int)(38 * Game1.SCALE), hotbarY + 4), fontColor, 0, new Vector2(0, 0), Game1.SCALE, SpriteEffects.None, 0);
                }
            }

            sb.DrawString(basicfont, gMap.getGlobalTime() + ":" + gMap.getGlobalTimeSec(), new Vector2(hotbarX - 100, hotbarY + 4), fontColor, 0, new Vector2(0, 0), Game1.SCALE, SpriteEffects.None, 0);
        }

        public void tick(float gTime)
        {
            MouseState state = Mouse.GetState();
            KeyboardState kState = Keyboard.GetState();

            handleMouseInput(state);

            handleScrollWheel(state);

            if(kState.IsKeyDown(Keys.E))
            {
                if(kState.IsKeyDown(Keys.E) != previousInventoryKeyState)
                    inventoryOpen = !inventoryOpen;
            }
            previousInventoryKeyState = kState.IsKeyDown(Keys.E);
        }

        private void handleMouseInput(MouseState state)
        {
            if (!inventoryOpen)
            {
                if (state.LeftButton == ButtonState.Pressed || state.RightButton == ButtonState.Pressed)
                {
                    float offsetX = Game1.TRUEGAMEWIDTH / 2 - (-cam.getX());
                    float offsetY = Game1.TRUEGAMEHEIGHT / 2 - (-cam.getY());
                    float placeX = (int)((state.X - offsetX) / Game1.SCALE / Game1.BLOCKSIZE) * Game1.BLOCKSIZE;
                    float placeY = (int)((state.Y - offsetY) / Game1.SCALE / Game1.BLOCKSIZE) * Game1.BLOCKSIZE;
                    int mapX = (int)((state.X - offsetX) / Game1.SCALE / Game1.BLOCKSIZE);
                    int mapY = (int)((state.Y - offsetY) / Game1.SCALE / Game1.BLOCKSIZE);

                    if (mapX >= 0 && mapX < gMap.getSizeX() && mapY >= 0 && mapY < gMap.getSizeY())
                    {
                        if (state.LeftButton == ButtonState.Pressed)
                        {
                            GenericBlock gb = gMap.getMap()[mapY, mapX];

                            if (gb.isBlock())
                            {
                                if (selectedItemID != ItemID.ID.fire_wand && selectedItemID != ItemID.ID.pistol_basic && selectedItemID != ItemID.ID.stone_hammer)
                                {
                                    GenericItem gi = new GenericItem(gb.x + 2, gb.y + 2, gMap, handler, this, content, gb.getID());
                                    if (gb.wallBehind)
                                    {
                                        gMap.getMap()[mapY, mapX] = new GenericBlock(mapX * Game1.BLOCKSIZE, mapY * Game1.BLOCKSIZE, handler, gMap, content, gb.wallBehindID);
                                    }
                                    else
                                    {
                                        gMap.getMap()[mapY, mapX] = new GenericBlock(mapX * Game1.BLOCKSIZE, mapY * Game1.BLOCKSIZE, handler, gMap, content, ItemID.ID.air_block);
                                    }

                                    handler.addObject(gi);
                                }
                            }
                            else if (gb.isWall())
                            {
                                if (selectedItemID == ItemID.ID.stone_hammer)
                                {
                                    GenericItem gi = new GenericItem(gb.x + 2, gb.y + 2, gMap, handler, this, content, gb.getID());
                                    gMap.getMap()[mapY, mapX] = new GenericBlock(mapX * Game1.BLOCKSIZE, mapY * Game1.BLOCKSIZE, handler, gMap, content, ItemID.ID.air_block);
                                    handler.addObject(gi);
                                }
                            }
                        }
                        else
                        {
                            if (hotbar[hotbarPosition] != null)
                            {
                                if (hotbar[hotbarPosition].count > 0)
                                {
                                    GenericBlock gb = gMap.getMap()[mapY, mapX];
                                    if (gb.getID() == ItemID.ID.air_block || (!IDChecker.isWall(selectedItemID) && gb.isWall()))
                                    {
                                        GenericBlock placeBlock;
                                        if (gb.isWall())
                                            placeBlock = new GenericBlock(placeX, placeY, handler, gMap, content, hotbar[hotbarPosition].getID(), gb.getID());
                                        else
                                            placeBlock = new GenericBlock(placeX, placeY, handler, gMap, content, hotbar[hotbarPosition].getID());
                                        gMap.getMap()[mapY, mapX] = placeBlock;
                                        hotbar[hotbarPosition].count--;
                                        if (hotbar[hotbarPosition].count < 1)
                                        {
                                            handler.removeObject(hotbar[hotbarPosition]);
                                            hotbar[hotbarPosition] = null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {

            }
        }

        private void handleScrollWheel(MouseState state)
        {
            if (state.ScrollWheelValue > previousScrollValue)
            {
                hotbarPosition--;
                if (hotbarPosition < 0)
                    hotbarPosition = 9;
            }
            else if (state.ScrollWheelValue < previousScrollValue)
            {
                hotbarPosition++;
                if (hotbarPosition > 9)
                    hotbarPosition = 0;
            }
            previousScrollValue = state.ScrollWheelValue;

            if (hotbar[hotbarPosition] != null)
            {
                selectedItemID = hotbar[hotbarPosition].getID();
            }
            else
            {
                selectedItemID = ItemID.ID.NULL;
            }
        }

        public bool addItem(GenericItem gi)
        {
            for (int i = 0; i < hotbar.Length; i++)
            {
                if(hotbar[i] != null && gi.getID() == hotbar[i].getID() && hotbar[i].count < hotbar[i].maxCount)
                {
                    if(!gi.isPickedUp())
                        hotbar[i].count ++;
                    hotbar[i].setPickedUp(true);
                    handler.removeObject(gi);
                    return true;
                }
            }
            for (int i = 0; i < hotbar.Length; i++)
            {
                if (hotbar[i] == null)
                {
                    gi.setX(hbXNoScale + 8 + i * 38);
                    gi.setY(hbYNoScale + 8);
                    hotbar[i] = gi;
                    hotbar[i].count = 1;
                    hotbar[i].setPickedUp(true);
                    handler.removeObject(gi);
                    return true;
                }
            }
            return false;
        }

        public bool inventoryIsOpen()
        {
            return inventoryOpen;
        }
    }
}
