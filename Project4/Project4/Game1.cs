using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Project4
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 

    public class Game1 : Game
    {

        public GraphicsDevice device;


        //Private variables to be used
        #region Private variables

        //for blast
        public static VertexBuffer vertexBuffer;

        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;
        SpriteBatch spriteBatch;

        private Random getRandom = new Random();

        private bool hit = false;
        private Model ship;

        private AsteroidType[] asteroidTypes;

        //private Asteroid[] asteroids;
        private Asteroid asteroid;
        private List<Asteroid> asteroids;

        private List<Blast> blastList;

        private Blast blast1;

        private int startingAsteroidCount = 500;
        private int currentAsteroidCount = 500;
        private int asteroidIndex = 500;


        //USED FOR TESTING COLLISON DETECTION 
        private Texture2D hitShipTexture;
        private Texture2D tempTexture; 
        
        private Texture2D shipTexture;
        private Texture2D blastTexture;
        private Texture2D backdrop;

        public Vector3 shipOrientation = Vector3.Zero;
        public Vector3 shipLocation = Vector3.Zero;

        private float zAngle;
        private float yAngle;
        
        private KeyboardState oldState;
        private int positionRange = 675;

        //private Vector3 cameraForward = -Vector3.UnitX;

        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 150, 0), new Vector3(0, 0, 0), -Vector3.UnitX);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), 800f / 480f, 0.01f, 1000f);

        #endregion
        struct Asteroid
        {
            public AsteroidType type;
            public Vector3 position;
            public Vector3 velocity;
            public int scale;
            public float size;
            public bool current;
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
            //public static VertexBuffer vertexBuffer;
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

            //GraphicsDevice.g

            //is it ok to load models in initialize?
            asteroidTypes = new AsteroidType[3];
            asteroidTypes[0].model = Content.Load<Model>("Models/rock1");
            asteroidTypes[1].model = Content.Load<Model>("Models/rock2");
            asteroidTypes[2].model = Content.Load<Model>("Models/rock3");
            asteroidTypes[0].texture = (Texture2D)Content.Load<Texture>("Textures/rock1");
            asteroidTypes[1].texture = (Texture2D)Content.Load<Texture>("Textures/rock2");
            asteroidTypes[2].texture = (Texture2D)Content.Load<Texture>("Textures/rock3");

            //asteroids = new Asteroid[4 * startingAsteroidCount]; //space for max possible number of asteroids

            asteroids = new List<Asteroid>();

            //for loop:
            for(int i = 0; i < startingAsteroidCount; ++i) //generating all initial asteroids
            {
                asteroid = new Asteroid();
                asteroid.type = asteroidTypes[random(0, 2)]; // pick random type
                asteroid.position = choosePosition();
                asteroid.velocity = chooseVelocity();
                asteroid.scale = 3; //maybe make weighted avg method if we want to vary this
                //asteroid.current = true;

                asteroids.Add(asteroid);
            }

            //Creates new list of blasts 
            blastList = new List<Blast>();

            float scale = .25f;

            basicEffect = new BasicEffect(GraphicsDevice); //or graphicsdevice??

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[6];

            Vector2 upperLeft = new Vector2(0, 0);
            Vector2 lowerLeft = new Vector2(0, 1);
            Vector2 upperRight = new Vector2(1, 0);
            Vector2 lowerRight = new Vector2(1, 1);

            vertices[0] = new VertexPositionColorTexture(scale*new Vector3(1, 1, 0), Color.White, upperRight);
            vertices[1] = new VertexPositionColorTexture(scale*new Vector3(1, -1, 0), Color.White, lowerRight);
            vertices[2] = new VertexPositionColorTexture(scale*new Vector3(-1, -1, 0), Color.White, lowerLeft);

            vertices[3] = new VertexPositionColorTexture(scale*new Vector3(1, 1, 0), Color.White, upperRight);
            vertices[4] = new VertexPositionColorTexture(scale*new Vector3(-1, -1, 0), Color.White, lowerLeft);
            vertices[5] = new VertexPositionColorTexture(scale*new Vector3(-1, 1, 0), Color.White, upperLeft);

            //vertices *= Matrix.CreateScale(.25f);

            vertexBuffer = new VertexBuffer(basicEffect.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

            base.Initialize();
        }

        #region Asteroid Stuff
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
        //NEED TO CHANGE 
        private int random(int v1, int v2)
        { 

            int rnd = getRandom.Next(v1, v2+1);

            return rnd;
        }
        #endregion

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ship = Content.Load<Model>("Models/Ship");

            //USED FOR COLLISON DETECTION
            hitShipTexture = Content.Load<Texture2D>("Textures/hitShip");

            shipTexture = Content.Load<Texture2D>("Textures/ship");
            backdrop = Content.Load<Texture2D>("Textures/galaxy");
            tempTexture = shipTexture;

            blastTexture = Content.Load<Texture2D>("Textures/blast");
            

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
            Matrix shipWorldMatrix = Matrix.CreateTranslation(shipLocation);

            float movementSpeed = gameTime.ElapsedGameTime.Milliseconds / 1000f; //* .75f;

            incrementAsteroids(movementSpeed);

            KeyboardState newState = Keyboard.GetState();  // get the newest state

            //Shoots a blast 
            if (newState.IsKeyDown(Keys.Space))
                shoot();

            //Ship  Movement
            #region ShipMovement
            //Ship rotation 
            if (newState.IsKeyDown(Keys.Left))
            {
                yAngle += 0.03f;
                shipOrientation.Y += 0.03f;
            }
                
           
            else if (newState.IsKeyDown(Keys.Right))
            {
                yAngle -= 0.03f;
                shipOrientation.Y -= 0.03f;
            }


            //Flipping the ship in 3D space
            if (newState.IsKeyDown(Keys.Down))
            {
                zAngle += 0.03f;
                shipOrientation.Z += 0.03f;
            }

            else if (newState.IsKeyDown(Keys.Up))
            {
                zAngle -= 0.03f;
                shipOrientation.Z -= 0.03f;
            }

            world = Matrix.CreateRotationZ(zAngle) * Matrix.CreateRotationY(yAngle);
            #endregion


            //cameraForward = world.Forward;
            //cameraRight = world.Right;
            //cameraUp = world.Up;

            //Detects if ship is hit by asteroid 

            foreach (var a in asteroids)
            {
                Matrix asteroidLocation = Matrix.CreateTranslation(a.position);
                if (IsCollision(ship, shipWorldMatrix, a.type.model, asteroidLocation))
                {
                    Console.WriteLine("Ship Hit! by Asteroid " + a.ToString());
                    hit = false;
                }
            }

            //for any blasts, eventually

            //Ship will turn red if hit by asteroid: testing purposes
            if (!hit)
                shipTexture = hitShipTexture;
            else
                shipTexture = tempTexture;

            hit = true;


            base.Update(gameTime);
        }

        private void incrementAsteroids(float updateSpeed)
        {

            for (int i = 0; i < currentAsteroidCount; ++i)
            {
                Asteroid temp = asteroids[i]; // needed because I can't access things in list directly like in array
                // could replace all asteroids[i] with temp. not sure what's best

                temp.position += asteroids[i].velocity * updateSpeed;

                if (asteroids[i].position.X > positionRange || asteroids[i].position.X < -positionRange)
                    temp.position = choosePosition();

                else if (asteroids[i].position.Y > positionRange || asteroids[i].position.Y < -positionRange)
                    temp.position = choosePosition();

                else if (asteroids[i].position.Z > positionRange || asteroids[i].position.Z < -positionRange)
                    temp.position = choosePosition();

                asteroids[i] = temp;
            }
        }

        private void incrementBlast(float updateSpeed)
        {
            blast1.position += blast1.velocity * updateSpeed;
        }

        private void shoot()
        {
            //creates new Blast at location of ship, figures out direction/velocity for it
            blast1 = new Blast();
            blast1.position = Vector3.Zero;
            blast1.texture = blastTexture;
            blast1.size = 1;
            blast1.velocity = world.Up;

            blastList.Add(blast1);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           // Matrix billboard = Matrix.CreateBillboard(shipOrientation, )
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(backdrop, new Rectangle(0, 0, 800, 480), Color.White);
            spriteBatch.End();

            //Implements a z-buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Draw the ship at the origin
            DrawModel(ship, world, view, projection, shipTexture);

            //Draw the array of asteroids 
            foreach (var a in asteroids)
            {
                    Matrix asteroidLocation = Matrix.CreateTranslation(a.position);

                    //Draw current asteroid
                    DrawModel(a.type.model, Matrix.CreateTranslation(a.position), view, projection, a.type.texture);

                    //Ensures Asteroids are not drawn on top of each other, 
                    //if current asteroid is drawn withing bounding sphere of previous asteroids, it is redrawn at a new location

                    //NOT WORKING ATM 
                    /*for (int j = 0; j < i; ++j)
                    {
                        Matrix asteroid2Location = Matrix.CreateTranslation(a.position);
                        if (IsCollision(asteroids[j].type.model, asteroid2Location, a.type.model, asteroidLocation))
                        {
                            DrawModel(a.type.model, Matrix.CreateTranslation(choosePosition()), view, projection, a.type.texture);
                        }
                    }*/
                
            }

            foreach(var blast in blastList){
                DrawBlast(blast, basicEffect, world.Right, world.Up, world.Forward);
            }
           
            base.Draw(gameTime);
        }

        private void DrawBlast(Blast blast, BasicEffect effect, Vector3 camera, Vector3 cameraUp, Vector3 cameraForward)
        {
            //mostly Dr. Wittman's Particle code

            Matrix billboard = Matrix.CreateBillboard(blast.position, world.Forward, shipOrientation, null);
            effect.World = Matrix.CreateScale(blast.size) * billboard;
            //effect.DiffuseColor = blast.color.toVector3();
            //effect.Alpha = blast.color.A / 255f
            effect.Texture = blast.texture; // or blastTexture
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
            effect.PreferPerPixelLighting = false;

            //GraphicsDevice device = effect.GraphicsDevice;
            //device.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);


            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                effect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

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

        //Checks for collision between 2 models
        //Credit to RB Whitaker
        private bool IsCollision(Model model1, Matrix world1, Model model2, Matrix world2)
        {
            for (int meshIndex1 = 0; meshIndex1 < model1.Meshes.Count; meshIndex1++)
            {
                BoundingSphere sphere1 = model1.Meshes[meshIndex1].BoundingSphere;
                sphere1 = sphere1.Transform(world1);

                for (int meshIndex2 = 0; meshIndex2 < model2.Meshes.Count; meshIndex2++)
                {
                    BoundingSphere sphere2 = model2.Meshes[meshIndex2].BoundingSphere;
                    sphere2 = sphere2.Transform(world2);

                    if (sphere1.Intersects(sphere2))
                        return true;
                }
            }
            return false;
        }

        private void breakApart(Asteroid asteroid)
        {
            //condition: if the asteroid is smallest
            //asteroid[i].current = false;
            //count--

            if (asteroid.scale == 1) //largest is 3, smallest is 1. could be .25
            {
                asteroids.Remove(asteroid); //not sure if this will work, or remove other random asteroid.
                currentAsteroidCount--;
            }
            else
            {
                Asteroid result = new Asteroid();
                result.size = asteroid.size / 2;
            }
           

            //else:
            
            //take current asteroid and halve its size. figure out scaling matrix

            //duplicate that asteroid

            //put new asteroid into asteroids[asteroidIndex]
            //then increment asteroidIndex
            //increment currentAsteroidCount
        }

    }
}
