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

        private SpriteFont font; 
        private Random getRandom = new Random();

        private bool hit = false;
        private Model ship;

        private AsteroidType[] asteroidTypes;

        //private Asteroid[] asteroids;
        private Asteroid asteroid;
        private List<Asteroid> asteroids;

        private List<Blast> blastList;

        private Blast blast1;
        private float blastScale;

        private int startingAsteroidCount = 200;
        private int currentAsteroidCount = 200;

        //USED FOR TESTING COLLISON DETECTION 
        private Texture2D hitShipTexture;
        private Texture2D tempTexture; 
        
        private Texture2D shipTexture;
        private Texture2D blastTexture;
        private Texture2D backdrop;

        public Vector3 shipOrientation = Vector3.Zero;
        public Vector3 shipLocation = Vector3.Zero;
        public Vector3 shipVelocity = Vector3.Zero;
        
        private KeyboardState newState;
        private bool isSpacePressed;

        private int positionRange = 675;

        //gameplay variables

        int level;
        int score;
        int lives;
        int nextLevelScore;

        //private Vector3 cameraForward = -Vector3.UnitX;

        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 150), new Vector3(0, 0, 0), Vector3.UnitY); //standard
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 800f / 480f, 0.01f, 1000f);

        #endregion
        class Asteroid
        {
            public AsteroidType type;
            public Vector3 position;
            public Vector3 velocity;
            public float scale;
        }

        struct AsteroidType
        {
            public Model model;
            public Texture2D texture;
        }

        class Blast
        {
            public Vector3 position; // should start at zero
            public Vector3 velocity; // should be consistent
            public float scale;
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
            //revise to just textures (!!)
            asteroidTypes = new AsteroidType[3];
            asteroidTypes[0].model = Content.Load<Model>("Models/rock1");
            asteroidTypes[1].model = Content.Load<Model>("Models/rock2");
            asteroidTypes[2].model = Content.Load<Model>("Models/rock3");
            asteroidTypes[0].texture = (Texture2D)Content.Load<Texture>("Textures/rock1");
            asteroidTypes[1].texture = (Texture2D)Content.Load<Texture>("Textures/rock2");
            asteroidTypes[2].texture = (Texture2D)Content.Load<Texture>("Textures/rock3");
            
            asteroids = new List<Asteroid>();
            
            for(int i = 0; i < startingAsteroidCount; ++i) //generating all initial asteroids
            {
                asteroid = new Asteroid();
                asteroid.type = asteroidTypes[0]; // pick random type
                asteroid.position = choosePosition();
                asteroid.velocity = chooseVelocity();
                asteroid.scale = 2;
                asteroids.Add(asteroid);
            }

            //Creates new list of blasts 
            blastList = new List<Blast>();
            blastScale = .15f;

            basicEffect = new BasicEffect(GraphicsDevice); //or graphicsdevice??

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[6];

            Vector2 upperLeft = new Vector2(0, 0);
            Vector2 lowerLeft = new Vector2(0, 1);
            Vector2 upperRight = new Vector2(1, 0);
            Vector2 lowerRight = new Vector2(1, 1);

            vertices[0] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, upperRight);
            vertices[1] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, lowerRight);
            vertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, lowerLeft);

            vertices[3] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, upperRight);
            vertices[4] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, lowerLeft);
            vertices[5] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, upperLeft);

            //vertices *= Matrix.CreateScale(.25f);

            vertexBuffer = new VertexBuffer(basicEffect.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

			//gameplay:

            lives = 3;
            score = 0;
            level = 1;
            nextLevelScore = 200;

            isSpacePressed = false;

            //remember to put in updates to these variables (lives, score) later

            //and create a nextLevel method. so that you can increase levels indefinitely based on some math
			
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

        
        //selects random number from v1 to v2
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ship = Content.Load<Model>("Models/Ship");

            //USED FOR COLLISON DETECTION
            hitShipTexture = Content.Load<Texture2D>("Textures/hitShip");
            shipTexture = Content.Load<Texture2D>("Textures/ship");
            backdrop = Content.Load<Texture2D>("Textures/galaxy");
            tempTexture = shipTexture;
            blastTexture = Content.Load<Texture2D>("Textures/blast");

            //FONT
            font = (SpriteFont)Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
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

            float movementSpeed = gameTime.ElapsedGameTime.Milliseconds / 1000f * .25f; //.75f to try and slow movements

            incrementAsteroids(movementSpeed);

            newState = Keyboard.GetState();  // get the newest state

            //Ship  Movement
            #region ShipMovement
            //Ship rotation 
            if (newState.IsKeyDown(Keys.Left))
            {
                //yAngle += 0.03f;
                shipOrientation.Z += 0.03f;
            }
           
            else if (newState.IsKeyDown(Keys.Right))
            {
                //yAngle -= 0.03f;
                shipOrientation.Z -= 0.03f;
            }

            //Flipping the ship in 3D space
            if (newState.IsKeyDown(Keys.Down))
            {
                //zAngle += 0.03f;
                shipOrientation.X += 0.03f;
            }

            else if (newState.IsKeyDown(Keys.Up))
            {
                //zAngle -= 0.03f;
                shipOrientation.X -= 0.03f;
            }

            world = Matrix.CreateRotationX(shipOrientation.X) * Matrix.CreateRotationZ(shipOrientation.Z);
            #endregion

            #region Ship Velocity and Shooting 
            //left control, forward movement
            if (newState.IsKeyDown(Keys.LeftControl))
            {
                shipVelocity -= world.Up;
            }
            //right control, forward movement
            if (newState.IsKeyDown(Keys.RightControl))
            {
                shipVelocity = Vector3.Zero;
            }

            //Shoots a blast 
            if (newState.IsKeyDown(Keys.Space))
            {
                if (!isSpacePressed)
                {
                    shoot();
                }

                isSpacePressed = true;
            }
            else
                isSpacePressed = false;

            #endregion

            #region Ship-Asteroid Collision
            //Detects if ship is hit by asteroid 
            //foreach (var a in asteroids)
            for (int i = 0; i < asteroids.Count; i++) //count or currentnumberasteroids
            {
                Matrix asteroidLocation = Matrix.CreateTranslation(asteroids[i].position);
                if (IsCollision(ship, shipWorldMatrix, asteroids[i].type.model, asteroidLocation))
                {
                    //Console.WriteLine("Ship Hit! by Asteroid " + a.ToString());
                    hit = false;
                }
                //blast-asteroid collison here
                foreach (var blast in blastList)
                {
                    if (hitCheck(asteroids[i].type.model, asteroidLocation, blast))
                    {
                        breakApart(asteroids[i], blast);
                        break;
                    }
                }
                //break;
            }

            //Ship will turn red if hit by asteroid: testing purposes
            if (!hit)
            {
                shipTexture = hitShipTexture;
                lives--;
                //sad sound
            }
            else
                shipTexture = tempTexture;

            hit = true;
            #endregion

            //Increment Blast
            incrementBlast(movementSpeed);

            //leveling up
            if (score>=nextLevelScore)
            {
                level++;
                lives++;
                nextLevelScore += level*level*100;
                Console.WriteLine("level: " + level + "/tscore: " + score);
                //play sound
            }
            
            if (lives >= 0)
            {
                //Game over!!
                //Sound
            }

            base.Update(gameTime);
        }
       
        private void incrementAsteroids(float updateSpeed)
        {
            for (int i = 0; i < asteroids.Count; ++i) //count or currentnumasteroids
            {
                Asteroid temp = asteroids[i]; // needed because I can't access things in list directly like in array

                temp.position += asteroids[i].velocity * updateSpeed + shipVelocity;
                
                if (asteroids[i].position.X > positionRange || asteroids[i].position.X < -positionRange)
                    temp.position = choosePosition();

                else if (asteroids[i].position.Y > positionRange || asteroids[i].position.Y < -positionRange)
                    temp.position = choosePosition();

                else if (asteroids[i].position.Z > positionRange || asteroids[i].position.Z < -positionRange)
                    temp.position = choosePosition();

                asteroids[i] = temp;
				
				//if (random(0,100) > 94) // testing breakApart prior to successful blasting.
                //breakApart(asteroids[random(0, currentAsteroidCount)]); // this eventually gives an out of bounds error for unknown reasons
            }
        }

        private void incrementBlast(float updateSpeed)
        {
            for(int i = 0; i < blastList.Count; ++i)
            {
                blastList[i].position += blastList[i].velocity * updateSpeed;

                if (blastList[i].position.X > positionRange || blastList[i].position.X < -positionRange)
                    blastList.Remove(blastList[i]);

                else if (blastList[i].position.Y > positionRange || blastList[i].position.Y < -positionRange)
                    blastList.Remove(blastList[i]);

                else if (blastList[i].position.Z > positionRange || blastList[i].position.Z < -positionRange)
                    blastList.Remove(blastList[i]);
            }
        }

        private void shoot() 
        {
            //creates new Blast at location of ship, figures out direction/velocity for it
            blast1 = new Blast();
            //blast1.position = Vector3.Zero;
            blast1.scale = 10f;
            blast1.velocity = world.Up*500f;
            blast1.position = blast1.velocity * .05f;

            blastList.Add(blast1);

            //blast sound
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Matrix billboard = Matrix.CreateBillboard(shipOrientation, )
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.None);
            spriteBatch.Draw(backdrop, new Rectangle(0, 0, 800, 480), Color.White);
            spriteBatch.End();
            
            //Implements a z-buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            //Draw the ship at the origin 
            DrawModel(ship, Matrix.CreateRotationX(MathHelper.ToRadians(90)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90)) * world, shipTexture);

            //Draw the array of asteroids 
            foreach (var a in asteroids)
            {
                   Matrix asteroidLocation = Matrix.CreateTranslation(a.position);

                   //Draw current asteroid
                   DrawModel(a.type.model, Matrix.CreateScale(a.scale) * Matrix.CreateTranslation(a.position), a.type.texture);
            }

            foreach(var blast in blastList){
                //Console.WriteLine("blast drawn!");
                DrawBlast(blast, basicEffect);
            }
              
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.None);
            //Draws the scoreboard and what player wins, when the max points is met
            //Should be last thing drawn 
            spriteBatch.DrawString(font, "Number of lives: " + lives, new Vector2(50, 50), Color.Blue);

            spriteBatch.DrawString(font, "Points: " + score, new Vector2(650, 50), Color.Blue);

            spriteBatch.DrawString(font, "Level: " + level, new Vector2(50, 400), Color.Blue);

            spriteBatch.DrawString(font, "Remaining Asteroids: " + currentAsteroidCount, new Vector2(550, 400), Color.Blue);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBlast(Blast blast, BasicEffect effect)
        {
            //mostly Dr. Wittman's Particle code
            Matrix billboard = Matrix.CreateBillboard(blast.position, new Vector3(0, 0, 150), Vector3.UnitY, null);
            
           // DrawModel(asteroid.type.model, Matrix.CreateTranslation(blast.position), view, projection, asteroid.type.texture);

            //GraphicsDevice device = effect.GraphicsDevice;
            //device.SetVertexBuffer(vertexBuffer);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            effect.World = Matrix.CreateScale(blast.scale) * billboard;
            effect.View = view;
            effect.Projection = projection;
            effect.Texture = blastTexture; // or blastTexture
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
            effect.PreferPerPixelLighting = false;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        private void DrawModel(Model model, Matrix world, Texture2D texture)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.TextureEnabled = true;
                    effect.Texture = texture;
                    effect.World =  world; // temporary increase
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
        
        private bool hitCheck(Model model1, Matrix world1, Blast blast)
        {
            for (int meshIndex1 = 0; meshIndex1 < model1.Meshes.Count; meshIndex1++)
            {
                BoundingSphere sphere1 = model1.Meshes[meshIndex1].BoundingSphere;
                sphere1 = sphere1.Transform(world1);
                //if dist btwn center and point < sphere.radius

                if (Vector3.Distance(sphere1.Center, blast.position) < sphere1.Radius)
                {
                    Console.WriteLine("Hit asteroid!!");
                    blastList.Remove(blast);
                    return true; 
                }

            }
            return false;
        }

        private void breakApart(Asteroid asteroid, Blast blast)
        {
            //crash sound

            Vector3 explosionVector;

            if (asteroid.scale < .75) //largest is 3, smallest is 1. could be .25
            {
                //make a blast/explosion at the asteroid's previous position
                score += 150; 
                asteroids.Remove(asteroid); //not sure if this will work, or remove other random asteroid.
                currentAsteroidCount--;
            }
            else
            {
                if (asteroid.scale == 2)
                    score += 50;

                if (asteroid.scale == 1)
                    score += 100; 


                asteroid.scale /= 2;

                
                Asteroid copy1 = new Asteroid(); //does this need to be a new asteroid also (????)
                copy1.scale = asteroid.scale;
                copy1.type = asteroid.type; // we should get away from type
                copy1.position = asteroid.position;

                Asteroid copy2 = new Asteroid();
                copy2.scale = asteroid.scale;
                copy2.type = asteroid.type; // we want to get away from using type (just texture.)
                copy2.position = asteroid.position;

                //find explosion vector:
                //perp to asteroid velocity and blaster velocity found using the cross product, then normalize it
                explosionVector =  Vector3.Normalize(Vector3.Cross(asteroid.velocity, blast.velocity));

                //*******use bounding sphere for radius, unless radius is the scale (which is a good idea)
                
                // result.position set to half asteroid's radius away (along explosion vector)
                // these really should be different and in relation 
                // how do we determine the asteroid's radius? (?????)

                // should be scale
                
                //new position = old position plus (radius) (vectorbetweentwo)

                copy1.position += asteroid.scale*explosionVector; //just on X axis to make something work.
                copy2.position -= asteroid.scale*explosionVector;

                copy1.velocity += (40f / asteroid.scale)*explosionVector;
                copy2.velocity -= (40f / asteroid.scale) * explosionVector;
                
                asteroids.Remove(asteroid);

                asteroids.Add(copy1);
                asteroids.Add(copy2);

                // will this successfully remove the asteroid (?????)

                currentAsteroidCount++;
                
            }
        }
    }
}
