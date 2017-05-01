using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project4
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 

    

    public class Game1 : Game
    {



        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Model ship;
        private Model asteroid1;

        private AsteroidType[] asteroidTypes;

        private Asteroid[] asteroids;
        private Asteroid test;

        private int startingAsteroidCount = 50;
        private int currentAsteroidCount = 50;

        private Texture2D shipTexture;
        private Texture2D rock1Texture;
        private Texture2D backdrop;

        private float angle;
        private KeyboardState oldState;

        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 150, 0), new Vector3(0, 0, 0), -Vector3.UnitX);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), 800f / 480f, 0.01f, 1000f);

        struct Asteroid
        {
            public AsteroidType type;
            public Vector3 position;
            public Vector3 velocity;
            public int scale;

        }

        struct AsteroidType
        {
            public Model model;
            public Texture2D texture;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //is it ok to load models in initialize?
            asteroidTypes = new AsteroidType[3];
            asteroidTypes[0].model = Content.Load<Model>("Models/rock1");
            asteroidTypes[1].model = Content.Load<Model>("Models/rock2");
            asteroidTypes[2].model = Content.Load<Model>("Models/rock3");
            asteroidTypes[0].texture = (Texture2D)Content.Load<Texture>("Textures/rock1");
            asteroidTypes[1].texture = (Texture2D)Content.Load<Texture>("Textures/rock2");
            asteroidTypes[2].texture = (Texture2D)Content.Load<Texture>("Textures/rock3");

            asteroids = new Asteroid[4*startingAsteroidCount];

            //for loop:

            test = new Asteroid();
            test.type = asteroidTypes[random(0,2)]; // model
            test.position = choosePosition();
            test.velocity = chooseVelocity();
            test.scale = 3; // maybe make weighted avg method if we want to vary this

            base.Initialize();
        }

        // pick a random velocity for an asteroid
        private Vector3 chooseVelocity()
        {
            // calls random number method
            return Vector3.Zero;
        }

        private Vector3 choosePosition()
        {
            return new Vector3(0, 0, -60);
        }

        // when asteroid reaches edge of field, randomly assign another position on the boundary
        private Vector3 chooseEdgePosition()
        {
            return Vector3.Zero;
        }

        //selects random number from v1 to v2
        private int random(int v1, int v2)
        {
            return 2;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ship = Content.Load<Model>("Models/Ship");
            //asteroid1 = Content.Load<Model>("Models/rock1");

            shipTexture = (Texture2D)Content.Load<Texture>("Textures/ship");
            rock1Texture = (Texture2D)Content.Load<Texture>("Textures/rock1");
            backdrop = (Texture2D)Content.Load<Texture>("Textures/galaxy");

            

            angle = 0;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            KeyboardState newState = Keyboard.GetState();  // get the newest state

            // handle the input
            if (newState.IsKeyDown(Keys.Left))
            {
                angle += 0.03f;
            }

            else if (newState.IsKeyDown(Keys.Right))
            {
                angle -= 0.03f;
            }

            oldState = newState;  // set the new state as the old state for next time

            world = Matrix.CreateRotationY(angle);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(backdrop, new Rectangle(0, 0, 800, 480), Color.White);

            spriteBatch.End();

            // TODO: Add your drawing code here
            DrawModel(ship, world, view, projection, shipTexture);
            DrawModel(test.type.model, Matrix.CreateTranslation(test.position), view, projection, test.type.texture);

            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Texture2D texture)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = texture;
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }


    }
}
