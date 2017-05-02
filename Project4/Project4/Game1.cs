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

        private AsteroidType[] asteroidTypes;

        private Asteroid[] asteroids;
        private Asteroid test;

        private Blast blast1;

        private int startingAsteroidCount = 50;
        private int currentAsteroidCount = 50;

        private Texture2D shipTexture;
        private Texture2D blastTexture;
        private Texture2D backdrop;
        

        private float zAngle;
        private float yAngle;
        
        private KeyboardState oldState;
        private int positionRange = 675;

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

        struct Blast
        {
            public Vector3 position; // should start at zero
            public Vector3 velocity; // should be consistent
            public float size;
            public Texture2D texture;
            //color? time to live (ends at asteroid, so may not be needed?)
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

            asteroids = new Asteroid[4 * startingAsteroidCount]; //space for max possible number of asteroids

            //for loop:
            for(int i = 0; i < startingAsteroidCount; ++i) //generating all initial asteroids
            {
                asteroids[i] = new Asteroid();
                asteroids[i].type = asteroidTypes[random(0, 2)]; // pick random type
                asteroids[i].position = choosePosition();
                asteroids[i].velocity = chooseVelocity();
                asteroids[i].scale = 3; // maybe make weighted avg method if we want to vary this
            }

            base.Initialize();
        }

        // pick a random velocity for an asteroid
        private Vector3 chooseVelocity()
        {
            // calls random number method
            int velocityRange = 20;

            return new Vector3(random(-velocityRange, velocityRange), random(-velocityRange, velocityRange), random(-velocityRange, velocityRange));
        }

        private Vector3 choosePosition()
        {
            return new Vector3(random(-positionRange, positionRange), random(-positionRange, positionRange), random(-positionRange, positionRange));
        }

        // when asteroid reaches edge of field, randomly assign another position on the boundary
        private Vector3 chooseEdgePosition()
        {
            return Vector3.Zero;
        }

        //selects random number from v1 to v2
        private int random(int v1, int v2)
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));

            int rnd = rndNum.Next(v1, v2);

            return rnd;
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

            shipTexture = (Texture2D)Content.Load<Texture>("Textures/ship");
            backdrop = (Texture2D)Content.Load<Texture>("Textures/galaxy");

            blastTexture = (Texture2D)Content.Load<Texture>("Textures/blast");

            zAngle = 1;
            yAngle = 0;
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

            float movementSpeed = gameTime.ElapsedGameTime.Milliseconds / 1000f; //* .75f;

            incrementAsteroids(movementSpeed);

            KeyboardState newState = Keyboard.GetState();  // get the newest state

            // handle the input
            if (newState.IsKeyDown(Keys.Left))
                yAngle += 0.03f;
           
            else if (newState.IsKeyDown(Keys.Right))
                yAngle -= 0.03f;

   
            //Potential code for flipping the ship in 3D space

            if (newState.IsKeyDown(Keys.Down))
                zAngle += 0.03f;

            else if (newState.IsKeyDown(Keys.Up))
                zAngle -= 0.03f;

            world = Matrix.CreateRotationZ(zAngle) * Matrix.CreateRotationY(yAngle);
            //world = Matrix.CreateRotationX(xAngle);

            base.Update(gameTime);
        }

        private void incrementAsteroids(float updateSpeed)
        {
            for (int i = 0; i < currentAsteroidCount; i++)
            {
                asteroids[i].position += asteroids[i].velocity * updateSpeed;

                if (asteroids[i].position.X > positionRange || asteroids[i].position.X < -positionRange)
                    asteroids[i].position = choosePosition();

                else if (asteroids[i].position.Y > positionRange || asteroids[i].position.Y < -positionRange)
                    asteroids[i].position = choosePosition();

                else if (asteroids[i].position.Z > positionRange || asteroids[i].position.Z < -positionRange)
                    asteroids[i].position = choosePosition();
            }
                

        }

        private void incrementBlast(float updateSpeed)
        {
            blast1.position += blast1.velocity * updateSpeed;
        }

        private void shoot()
        {
            //creates new Blast at location of ship, figures out direction/velocity for it
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

            //Implements a z-buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Draw the ship at the origin
            DrawModel(ship, world, view, projection, shipTexture);

            //Draw the array of asteroids 
            for (int i = 0; i < currentAsteroidCount; ++i)
            {
                DrawModel(asteroids[i].type.model, Matrix.CreateTranslation(asteroids[i].position), view, projection, asteroids[i].type.texture);
            }

            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Texture2D texture)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
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
