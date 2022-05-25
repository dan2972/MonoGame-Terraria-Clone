using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameTestProject.Entities;
using MonoGameTestProject.Entities.Blocks;
using MonoGameTestProject.Entities.Mobs;
using MonoGameTestProject.Entities.Projectiles;
using MonoGameTestProject.GameStates;
using MonoGameTestProject.Map;
using MonoGameTestProject.Misc;
using MonoGameTestProject.Runner;
using System;

namespace MonoGameTestProject
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Handler handler;
        GameMap gMap;
        Background bg;

        Camera camera;

        Menu menu;
        Inventory inventory;

        ImageLoader ml;

        public static int TRUEGAMEWIDTH = 1280;//GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public static int TRUEGAMEHEIGHT = 720;//GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        public static int GAMEWIDTH = 1280;
        public static int GAMEHEIGHT = 720;
        public static int MAPSIZEX = 256;
        public static int MAPSIZEY = 256;
        public static float SCALE = (float)TRUEGAMEHEIGHT / (float)GAMEHEIGHT;
        public static float BLOCKSIZE = 16;
        public static string GAMESTATE = "GAME";
        public static Boolean usingGamePad = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = TRUEGAMEWIDTH;
            graphics.PreferredBackBufferHeight = TRUEGAMEHEIGHT;
            //graphics.IsFullScreen = true;
            graphics.HardwareModeSwitch = false;
            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
            graphics.ApplyChanges();

            System.Random r = new System.Random();

            Texture2D mapTexture = Content.Load<Texture2D>("map");

            camera = new Camera();
            gMap = new GameMap(256, 256, camera, Content);
            handler = new Handler(camera, gMap);
            
            inventory = new Inventory(camera, gMap, handler, Content);
            menu = new Menu(Content);
            ml = new ImageLoader(mapTexture, handler, gMap, Content);

            Player player = new Player(550, 100, handler, gMap, inventory, camera, Content);
            handler.addObject(player, ObjectType.type.characters);
            bg = new Background(camera, gMap, player, Content);

            ml.loadMap();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float gTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
            {
                usingGamePad = true;
            }
            if(Keyboard.GetState().GetPressedKeys().Length > 0 || Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                usingGamePad = false;
            }


            if (GAMESTATE == "MENU")
            {
                menu.tick();
            }
            else if (GAMESTATE == "GAME")
            {
                gMap.tick(gTime);
                handler.tick(gTime);
                inventory.tick(gTime);
                bg.tick(gTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Color c = bg.getBackgroundColor();
            GraphicsDevice.Clear(c);

            //spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.getTransform());
            if (GAMESTATE == "MENU")
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
                menu.render(spriteBatch);
            }
            else if (GAMESTATE == "GAME")
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.getTransform());
                bg.render(spriteBatch);
                handler.render(spriteBatch);
                inventory.render(spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
