using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Project4
{
    /// <summary>
    /// Byron Lewandowski and Lucas Garges
    /// Project 4 - Asteroids
    /// Dr. Wittman - Computer Graphics
    /// 5/5/2017
    /// </summary>
    /// 

    public class Game1 : Game
    {
        public GraphicsDevice device;

        //Private variables to be used
        #region Private variables

        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;
        SpriteBatch spriteBatch;

        private SpriteFont font; 
        private Random getRandom = new Random();
        
        private Model shipModel;
        private bool hit = false;

        private Model asteroidModel;
        private Texture2D asteroidTexture;
        private Asteroid asteroid;
        private List<Asteroid> asteroids;

        private List<Blast> blastList;
        private Blast blast;
        public static VertexBuffer vertexBuffer;

        private int startingAsteroidCount = 200;
        private int currentAsteroidCount = 200;
        private Matrix asteroidLocation;

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
        private bool isCurrentCollision;

        private int velocityRange = 30;

        //gameplay variables

        int level;
        int score;
        int lives;
        int nextLevelScore;
        bool gameover = false;
        bool win = false;
        bool lose = false;
        int positionRange = 675;

        private SoundEffect winEffect;
        private SoundEffect newLevel; //found- nextLevel
        private SoundEffect laser; //for blasts
        private SoundEffect loseGame;
        private SoundEffect breakEffect; // for asteroids
        private SoundEffect minorShipHit;
        private SoundEffect majorShipHit; 

        private Song song;
        
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 150), new Vector3(0, 0, 0), Vector3.UnitY); //standard
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 800f / 480f, 0.01f, 1000f);

        #endregion
        class Asteroid
        {
            //public AsteroidType type;
            public Vector3 position;
            public Vector3 velocity;
            public float scale;
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
            asteroidModel = Content.Load<Model>("Models/rock1");
            asteroidTexture = Content.Load<Texture2D>("Textures/rock1");

            asteroids = new List<Asteroid>();
            
            for(int i = 0; i < startingAsteroidCount; ++i) //generating all initial asteroids
            {
                asteroid = new Asteroid();
                asteroid.position = Vector3.Zero;
                while(Math.Abs(asteroid.position.X) < 75 || Math.Abs(asteroid.position.Y) < 75 || Math.Abs(asteroid.position.Z) < 75)
                    asteroid.position = choosePosition(-675, 675);

                asteroid.velocity = chooseVelocity();
                asteroid.scale = 2;
                asteroids.Add(asteroid);
            }

            //Creates new list of blasts 
            blastList = new List<Blast>();

            basicEffect = new BasicEffect(GraphicsDevice);

            //sets up vertexBuffer for a square that is later used for blasts

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

            vertexBuffer = new VertexBuffer(basicEffect.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

			//gameplay variables
            lives = 5;
            score = 0;
            level = 1;
            nextLevelScore = 200;
            gameover = false;
            
            isSpacePressed = false;
            isCurrentCollision = false;

            base.Initialize();
        }

        #region Asteroid Stuff
        // pick a random velocity for an asteroid
        private Vector3 chooseVelocity()
        {
            return new Vector3(random(-velocityRange, velocityRange), random(-velocityRange, velocityRange), random(-velocityRange, velocityRange));
        }

        private Vector3 choosePosition(int lower, int upper) //!!!
        {
            Vector3 p = Vector3.Zero; //initial value
            //while (IsCollision(ship, shipWorldMatrix, asteroids[i].type.model, asteroidLocation))
            {
                 p = new Vector3(random(lower, upper), random(lower, upper), random(lower, upper));
            }
            return p;
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
            shipModel = Content.Load<Model>("Models/Ship");

            //USED FOR COLLISON DETECTION
            hitShipTexture = Content.Load<Texture2D>("Textures/hitShip");
            shipTexture = Content.Load<Texture2D>("Textures/ship");
            backdrop = Content.Load<Texture2D>("Textures/galaxy");
            tempTexture = shipTexture;
            blastTexture = Content.Load<Texture2D>("Textures/blast");

            //FONT
            font = (SpriteFont)Content.Load<SpriteFont>("font");

            //AUDIO
            winEffect = Content.Load<SoundEffect>("Audio/winSound");
            newLevel = Content.Load<SoundEffect>("Audio/nextLevel");
            laser = Content.Load<SoundEffect>("Audio/laserSound");
            breakEffect = Content.Load<SoundEffect>("Audio/majorCrash"); //currently duplicate **
            minorShipHit = Content.Load<SoundEffect>("Audio/snare");
            majorShipHit = Content.Load<SoundEffect>("Audio/majorCrash");
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
            
            Matrix shipWorldMatrix = Matrix.CreateTranslation(shipLocation);

            float movementSpeed = gameTime.ElapsedGameTime.Milliseconds / 1000f * .25f; //.75f to try and slow movements

            incrementAsteroids(movementSpeed);

            newState = Keyboard.GetState();  // get the newest state

            //Ship  Movement
            #region ShipMovement
            //Ship rotation 
            if (!gameover) // ship movement only doable while game is in progress
            {
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
                if (newState.IsKeyDown(Keys.Up))
                {
                    //zAngle += 0.03f;
                    shipOrientation.X += 0.03f;
                }

                else if (newState.IsKeyDown(Keys.Down))
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

                if (shipVelocity.X > 30f) shipVelocity.X = 30f;
                if (shipVelocity.Y > 30f) shipVelocity.Y = 30f;
                if (shipVelocity.Z > 30f) shipVelocity.Z = 30f;

                //right control, STOP movement
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
                    asteroidLocation = Matrix.CreateTranslation(asteroids[i].position);
                    if (IsCollision(shipModel, shipWorldMatrix, asteroidModel, asteroidLocation))
                    {
                        resetAsteroids();
                        hit = true;
                        isCurrentCollision = true;
                    }

                    //blast-asteroid collison here
                    foreach (var blast in blastList)
                    {
                        if (hitCheck(asteroidModel, asteroidLocation, blast))
                        {
                            breakApart(asteroids[i], blast);
                            break;
                        }
                    }
                    //break;
                }

                //Ship will turn red if hit by asteroid: testing purposes
                if (hit)
                {
                    shipTexture = hitShipTexture;
                    lives--;
                    minorShipHit.Play();
                }
                else
                    shipTexture = tempTexture;

                hit = false;
            }
            else
            {
                shipVelocity = Vector3.Zero;
            }
            #endregion

            //Increment Blast
            incrementBlast(movementSpeed);

            #region Scoring and Game Ending 
            //Check if the player has any remaining lives, and checks if the level
            if (lives <= 0)
            {
                gameover = true;
                lose = true;
                //Game over!!
                //lose sound
            }

            else if (score >= nextLevelScore)
            {
                level++;
                lives++;
                nextLevelScore += level * level * 100; 
                Console.WriteLine("level: " + level + "/tscore: " + score);
                //play sound
            }

            if (level == 5)
            {
                gameover = true;
                winEffect.Play();
                win = true; 
            }
            #endregion

            base.Update(gameTime);
        }
       
        //Increment the asteroids 
        private void incrementAsteroids(float updateSpeed)
        {
            for (int i = 0; i < asteroids.Count; ++i)
            {
                Asteroid temp = asteroids[i]; // needed because I can't access things in list directly like in array

                temp.position += asteroids[i].velocity * updateSpeed + shipVelocity;
                
                if (asteroids[i].position.X > positionRange || asteroids[i].position.X < -positionRange)
                    temp.position = choosePosition(-675, 750);

                else if (asteroids[i].position.Y > positionRange || asteroids[i].position.Y < -positionRange)
                    temp.position = choosePosition(-675, 750);

                else if (asteroids[i].position.Z > positionRange || asteroids[i].position.Z < -positionRange)
                    temp.position = choosePosition(-675, 750);

                asteroids[i] = temp;
            }
        }

        private void resetAsteroids()
        {
            for (int i = 0; i < asteroids.Count; ++i)
            {
                asteroids[i].position = Vector3.Zero;
                while (Math.Abs(asteroid.position.X) < 75 || Math.Abs(asteroid.position.Y) < 75 || Math.Abs(asteroid.position.Z) < 75)
                    asteroids[i].position = choosePosition(-675, 675);
            }
        }

        //Increments the position of the blast according to its velocity, and removes it if out of bounds
        private void incrementBlast(float updateSpeed)
        {
            for(int i = 0; i < blastList.Count; ++i)
            {
                blastList[i].position += blastList[i].velocity * updateSpeed;// + shipVelocity;

                if (blastList[i].position.X > positionRange || blastList[i].position.X < -positionRange)
                    blastList.Remove(blastList[i]);

                else if (blastList[i].position.Y > positionRange || blastList[i].position.Y < -positionRange)
                    blastList.Remove(blastList[i]);

                else if (blastList[i].position.Z > positionRange || blastList[i].position.Z < -positionRange)
                    blastList.Remove(blastList[i]);
            }
        }

        //Creates a new blast at location of ship and figures out velocity for it, then adds it to the list
        private void shoot() 
        {
            blast = new Blast();
            //blast1.position = Vector3.Zero;
            blast.scale = 10f;
            blast.velocity = world.Up*500f;
            blast.position = blast.velocity * .05f;

            blastList.Add(blast);

            laser.Play(.5f,.7f,1f);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draws background
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.None);
            spriteBatch.Draw(backdrop, new Rectangle(0, 0, 800, 480), Color.White);
            spriteBatch.End();
            
            //Implements a z-buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Draw the ship at the origin 
            if (!gameover) //if game is still in play
                DrawModel(shipModel, Matrix.CreateRotationX(MathHelper.ToRadians(90)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90)) * world, Vector3.Zero, shipTexture);

            //Draw the array of asteroids 
            foreach (var a in asteroids)
            {
                   asteroidLocation = Matrix.CreateScale(a.scale)*Matrix.CreateTranslation(a.position);

                   //Draw current asteroid
                   DrawModel(asteroidModel, asteroidLocation, a.position, asteroidTexture);
            }

            foreach(var blast in blastList){
                DrawBlast(blast, basicEffect);
            }
              
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.None);

            #region Text Drawn on Screen 
            //Draw Scoreboard
            spriteBatch.DrawString(font, "Number of lives: " + lives, new Vector2(50, 50), Color.Blue);
            spriteBatch.DrawString(font, "Points: " + score, new Vector2(650, 50), Color.Blue);
            spriteBatch.DrawString(font, "Level: " + level, new Vector2(50, 400), Color.Blue);
            spriteBatch.DrawString(font, "Remaining Asteroids: " + currentAsteroidCount, new Vector2(550, 400), Color.Blue);
            spriteBatch.End();

            if (win)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "You won!", new Vector2(320, 200), Color.Yellow);
                spriteBatch.End();
            }

            if (lose)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "You Lost!", new Vector2(320, 200), Color.Yellow);
                spriteBatch.End();
            }
            #endregion

            base.Draw(gameTime);
        }

        //draws a blast to the screen as a billboard
        private void DrawBlast(Blast blast, BasicEffect effect)
        {
            //mostly Dr. Wittman's Particle code
            Matrix billboard = Matrix.CreateBillboard(blast.position, new Vector3(0, 0, 150), Vector3.UnitY, null);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            effect.World = Matrix.CreateScale(blast.scale) * billboard;
            effect.View = view;
            effect.Projection = projection;
            effect.Texture = blastTexture;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
            effect.PreferPerPixelLighting = false;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        // Draws a model
        private void DrawModel(Model model, Matrix world, Vector3 position, Texture2D texture)
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

                    // if a model is between camera and ship, make semi-transparent
                    if (position.Z > shipLocation.Z && Math.Abs(position.X)<40 && Math.Abs(position.Y) <40)
                        effect.Alpha = .5f;
                    else
                        effect.Alpha = 1f;
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
        
        // Checks if a blast has hit an asteroid
        private bool hitCheck(Model model1, Matrix world1, Blast blast)
        {
            for (int meshIndex1 = 0; meshIndex1 < model1.Meshes.Count; meshIndex1++)
            {
                BoundingSphere sphere1 = model1.Meshes[meshIndex1].BoundingSphere;
                sphere1 = sphere1.Transform(world1);

                if (Vector3.Distance(sphere1.Center, blast.position) < sphere1.Radius)
                {
                    Console.WriteLine("Hit asteroid!!");
                    blastList.Remove(blast);
                    return true; 
                }

            }
            return false;
        }

        // Breaks an asteroid apart when it is hit by a blast
        private void breakApart(Asteroid asteroid, Blast blast)
        {
            //crash sound
            breakEffect.Play(.5f,.5f,1f);

            Vector3 explosionVector;

            if (asteroid.scale < .75) 
            {
                score += 150; 
                asteroids.Remove(asteroid);
                currentAsteroidCount--;
            }
            else
            {
                if (asteroid.scale == 2)
                    score += 50;

                if (asteroid.scale == 1)
                    score += 100; 
                
                asteroid.scale /= 2;

                Asteroid copy1 = new Asteroid();
                copy1.scale = asteroid.scale;
                copy1.position = asteroid.position;

                Asteroid copy2 = new Asteroid();
                copy2.scale = asteroid.scale;
                copy2.position = asteroid.position;

                //find vector perpendicular to asteroid velocity and blaster velocity
                //using the cross product, then normalized
                explosionVector =  Vector3.Normalize(Vector3.Cross(asteroid.velocity, blast.velocity));
                
                //updates positions along the explosion vector
                copy1.position += asteroid.scale*explosionVector;
                copy2.position -= asteroid.scale*explosionVector;

                //updates velocities
                copy1.velocity += (40f / asteroid.scale)*explosionVector;
                copy2.velocity -= (40f / asteroid.scale) * explosionVector;
                
                asteroids.Remove(asteroid);

                asteroids.Add(copy1);
                asteroids.Add(copy2);

                currentAsteroidCount++;
                
            }
        }
    }
}
