using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GP3coursework
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region User Defined Variables
        //------------------------------------------
        // Added for use with fonts
        //------------------------------------------
        SpriteFont fontToUse;

        //--------------------------------------------------
        // Added for use with playing Audio via Media player
        //--------------------------------------------------
        private Song bkgMusic;
        private String songInfo;
        //--------------------------------------------------
        //Set the sound effects to use
        //--------------------------------------------------
        private SoundEffectInstance tardisSoundInstance;
        private SoundEffect tardisSound;
        private SoundEffect explosionSound;
        private SoundEffect firingSound;

        private SoundEffect engineSound;
        private SoundEffectInstance engineSoundInstance;

        private SoundEffect revSound;
        private SoundEffectInstance revSoundInstance;

        // Set the 3D model to draw.
        private Model mdlTankBarrel;
        private Matrix[] mdlTankBarrelTransforms;
        private Model mdlTankTracks;
        private Matrix[] mdlTankTracksTransforms;
        private Model mdlTankTurret;
        private Matrix[] mdlTankTurretTransforms;

        private Model mdlFence;
        private Matrix[] mdlFenceTransforms;

        private Model mdlGround;
        private Matrix[] mdlGroundTransforms;

        // The aspect ratio determines how to scale 3d to 2d projection.
        private float aspectRatio;

        // Set the position of the model in world space, and set the rotation.
        private Vector3 mdlPosition = Vector3.Zero;
        private float mdlRotation = 0.0f;
        private Vector3 mdlVelocity = Vector3.Zero;
        private float mdlTurretRotation = 0.0f;
        private float mdlBarrelPitch = 0.0f;

        // create an array of enemy daleks
        private Model mdlDalek;
        private Matrix[] mdDalekTransforms;
        private Daleks[] dalekList = new Daleks[GameConstants.NumDaleks];

        // create an array of laser bullets
        private Model mdlLaser;
        private Matrix[] mdlLaserTransforms;
        private Laser[] laserList = new Laser[GameConstants.NumLasers];

        private Random random = new Random();

        private KeyboardState lastState;
        private GamePadState lastGame;
        private int hitCount;

        // Set the position of the camera in world space, for our view matrix.
        private Vector3 cameraPosition = new Vector3(10.0f, 5.0f, 0.0001f);
        private Matrix viewMatrix;
        private Matrix projectionMatrix;

        float acceleration;

        Camera camera1;
        Camera camera2;
        Camera mainCamera;

        BasicEffect basicEffect;

        private Boolean audioMuted;

        private String state = "menu";

        private Texture2D mainMenu;
        private Texture2D instructions;
        private Texture2D gameOver;

        private float gameTimer;

        

        private void InitializeTransform()
        {
            
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), aspectRatio, 1.0f, 350.0f);
            
            camera1 = new Camera(graphics, new Vector3(10.0f, 5.0f, 0.0001f), new Vector3(0,0,0), 45);
            camera2 = new Camera(graphics, new Vector3(10.0f, -5.0f, 0.0001f), new Vector3(0, 0, 0), 25);

            mainCamera = camera1;

        }

        private void InitializeEffect()
        {
            //camera.initializeEffect(graphics);
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.View = mainCamera.camViewMatrix;
            basicEffect.Projection = mainCamera.projectionMatrix;
            basicEffect.World = mainCamera.worldMatrix;
            basicEffect.TextureEnabled = true; //added
            
        }

        private void setUpEffectTransforms()
        {
            mdlTankBarrelTransforms = SetupEffectTransformDefaults(mdlTankBarrel);

            mdlTankTracksTransforms = SetupEffectTransformDefaults(mdlTankTracks);

            mdlTankTurretTransforms = SetupEffectTransformDefaults(mdlTankTurret);

            mdlFenceTransforms = SetupEffectTransformDefaults(mdlFence);

            mdlGroundTransforms = SetupEffectTransformDefaults(mdlGround);

            mdlLaserTransforms = SetupEffectTransformDefaults(mdlLaser);

            mdDalekTransforms = SetupEffectTransformDefaults(mdlDalek);
        }

        private void MoveModel()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gameState = GamePad.GetState(PlayerIndex.One);

            // Create some velocity if the right trigger is down.
            Vector3 mdlVelocityAdd = Vector3.Zero;

            // Find out what direction we should be thrusting, using rotation.
            mdlVelocityAdd.X = -(float)Math.Sin(mdlRotation);
            mdlVelocityAdd.Z = -(float)Math.Cos(mdlRotation);

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                // Rotate the entire tank left
                mdlRotation -= (-0.25f * 0.07f) / ((mdlVelocity.Length()+0.1f) * 7);
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                // Rotate the entire tank right
                mdlRotation -= (0.25f * 0.07f) / ((mdlVelocity.Length()+0.1f) * 7);
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                // rotate the turret left

                // if we're in third person, turn speed is increased
                // if we're in first person, has fine tuning accuracy.
                if (mainCamera == camera1)
                {
                    
                    mdlTurretRotation -= -0.25f * 0.1f;
                }
                if (mainCamera == camera2)
                {
                    mdlTurretRotation -= -0.25f * 0.025f;
                }
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                // rotate the turret right

                // if we're in third person, turn speed is increased
                // if we're in first person, has fine tuning accuracy.
                if (mainCamera == camera1)
                {
                    // Rotate left.
                    mdlTurretRotation -= 0.25f * 0.1f;
                    
                }
                if (mainCamera == camera2)
                {
                    mdlTurretRotation -= 0.25f * 0.025f;
                    
                }
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                // pitch the barrel up
                if (mdlBarrelPitch < 0.2f)
                {
                    // if we're in third person, turn speed is increased
                    // if we're in first person, has fine tuning accuracy.
                    if (mainCamera == camera1)
                    {
                        mdlBarrelPitch -= -0.1f * 0.10f;
                    }
                    if (mainCamera == camera2)
                    {
                        mdlBarrelPitch -= -0.1f * 0.025f;
                    }
                }
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                // pitch the barrel down
                if (mdlBarrelPitch > -0.12f)
                {
                    // if we're in third person, turn speed is increased
                    // if we're in first person, has fine tuning accuracy.
                    if (mainCamera == camera1)
                    {
                        mdlBarrelPitch -= 0.1f * 0.10f;
                        
                    }
                    if (mainCamera == camera2)
                    {
                        mdlBarrelPitch -= 0.1f * 0.025f;
                        
                    }
                }
            }

            if (gameState.IsConnected)
            {
                mdlVelocityAdd *= gameState.ThumbSticks.Left.Y / 100;
                mdlVelocity += mdlVelocityAdd;
                if (mainCamera == camera1)
                {
                    
                        mdlBarrelPitch += gameState.ThumbSticks.Right.Y * 0.1f * 0.1f;
                    
                    mdlTurretRotation -= gameState.ThumbSticks.Right.X * 0.25f * 0.1f;
                }
                if (mainCamera == camera2)
                {
                    
                        mdlBarrelPitch += gameState.ThumbSticks.Right.Y * 0.1f * 0.025f;
                    
                    mdlTurretRotation -= gameState.ThumbSticks.Right.X * 0.25f * 0.025f;
                    
                }
                if (mdlBarrelPitch > 0.2f)
                {
                    mdlBarrelPitch = 0.2f;
                }
                if (mdlBarrelPitch < -0.12f)
                {
                    mdlBarrelPitch = -0.12f;
                }
                mdlRotation += gameState.ThumbSticks.Left.X * ((-0.25f * 0.07f) / ((mdlVelocity.Length() + 0.1f) * 7));
            }


            if (keyboardState.IsKeyDown(Keys.Up))
            {
                // go forwards
                mdlVelocityAdd *= 0.01f;
                mdlVelocity += mdlVelocityAdd;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {
                // go backwards
                mdlVelocityAdd *= -0.005f;
                mdlVelocity += mdlVelocityAdd;
            }

            if (keyboardState.IsKeyDown(Keys.R))
            {
                
                ResetDaleks();
                //tardisSoundInstance.Play();
            }

            if (gameState.Triggers.Left > 0 && mdlVelocity.Length() < 0.01f)
            {
                mainCamera = camera2;
            }
            else
            {
                mainCamera = camera1;
            }
            if (keyboardState.IsKeyDown(Keys.LeftShift) && !lastState.IsKeyDown(Keys.LeftShift))
            {
                // Rotate right.
                if (mainCamera == camera2)
                {
                    mainCamera = camera1;

                }

                else if (mainCamera == camera1)
                {
                    mainCamera = camera2;
                }

                setUpEffectTransforms();
            }
            
            if (!keyboardState.IsKeyDown(Keys.Z))
            {
                // Rotate right.
                camera1.MoveTowards(mdlPosition - Matrix.CreateRotationY(mdlTurretRotation + mdlRotation).Forward*10 + new Vector3(0,5-mdlBarrelPitch*4,0), 
                    Matrix.CreateRotationY(mdlTurretRotation + mdlRotation).Forward * 5 + mdlPosition + new Vector3(0,2 +mdlBarrelPitch*5,0));
                //Vector3 offset = new Vector3(0, 0.93402f*2, 0);
                Vector3 offset = new Vector3(0, 0.94002f * 2, 0);
                camera2.Move(mdlPosition + offset + (Matrix.CreateRotationX(mdlBarrelPitch)*Matrix.CreateRotationY(mdlRotation + mdlTurretRotation)).Forward * 3.5f + Matrix.CreateRotationY(mdlRotation + mdlTurretRotation).Forward * 0.93402f, mdlPosition + offset + (Matrix.CreateRotationX(mdlBarrelPitch) * Matrix.CreateRotationY(mdlRotation + mdlTurretRotation)).Forward * 50);
                //camera2.MoveTowards(camera2.camPosition, camera2.camLookat);
                setUpEffectTransforms();
            }

            if (keyboardState.IsKeyDown(Keys.M) && !lastState.IsKeyDown(Keys.M))
            {
                // if the user presses m, toggle audio mute
                if (audioMuted)
                {
                    audioMuted = false;
                    tardisSoundInstance.Resume();
                    
                }
                else if (!audioMuted)
                {
                    audioMuted = true;
                    tardisSoundInstance.Pause();
                    
                }
            }
            
            //are we shooting?
            if (keyboardState.IsKeyDown(Keys.Space) && !lastState.IsKeyDown(Keys.Space) || (gameState.Triggers.Right > 0 && lastGame.Triggers.Right == 0))
            {
                //add another bullet.  Find an inactive bullet slot and use it
                //if all bullets slots are used, ignore the user input
                for (int i = 0; i < GameConstants.NumLasers; i++)
                {
                    if (!laserList[i].isActive)
                    {
                        Matrix tardisTransform = Matrix.CreateRotationX(mdlBarrelPitch) * Matrix.CreateRotationY(mdlRotation + mdlTurretRotation) ;
                        laserList[i].direction = tardisTransform.Forward;
                        laserList[i].speed = GameConstants.LaserSpeedAdjustment;
                        laserList[i].position = mdlPosition + new Vector3(0, 0.94002f * 2, 0) + Matrix.CreateRotationY(mdlRotation + mdlTurretRotation).Forward * 0.93402f;
                        laserList[i].rotation = mdlRotation + mdlTurretRotation;
                        laserList[i].pitch = mdlBarrelPitch;
                        laserList[i].isActive = true;
                        if(!audioMuted)
                            firingSound.Play();
                        break; //exit the loop     
                    }
                }
            }

            
            
            lastState = keyboardState;
            lastGame = gameState;

        }

        private void CheckSpeed()
        {
            engineSoundInstance.Pitch = 0.1f+ mdlVelocity.Length();
            if (mdlVelocity.Length() > 0.01f)
            {
               
                if (mainCamera != camera1)
                {
                    mainCamera = camera1;
                    setUpEffectTransforms();

                }
            }
            
        }

        private void ResetDaleks()
        {
            mdlVelocity = Vector3.Zero;
            mdlPosition = Vector3.Zero;
            mdlRotation = 0.0f;
            mdlTurretRotation = 0.0f;
            hitCount = 0;
            gameTimer = 0.0f;
            float xStart;
            float zStart;
            for (int i = 0; i < GameConstants.NumDaleks; i++)
            {
                if (random.Next(2) == 0)
                {
                    xStart = (float)-GameConstants.PlayfieldSizeX;
                }
                else
                {
                    xStart = (float)GameConstants.PlayfieldSizeX;
                }
                zStart = (float)random.NextDouble() * GameConstants.PlayfieldSizeZ;
                dalekList[i].position = new Vector3(random.Next(-44,45), random.Next(4,7), random.Next(-44,45));
                double angle = random.NextDouble() * 2 * Math.PI;
                //float realAngle = (float)Math.Sin(Vector3.Normalize(-dalekList[i].position).X) + (float)Math.Sin(Vector3.Normalize(-dalekList[i].position).Z);

                dalekList[i].angle = (float)angle;
                dalekList[i].direction.X = -(float)Math.Sin(angle);
                dalekList[i].direction.Z = (float)Math.Cos(angle);
                dalekList[i].speed = GameConstants.DalekMinSpeed +
                   (float)random.NextDouble() * GameConstants.DalekMaxSpeed;
                dalekList[i].isActive = true;
            }

        }

        private Matrix[] SetupEffectTransformDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = mainCamera.projectionMatrix;
                    effect.View = mainCamera.camViewMatrix;
                }
            }
            return absoluteTransforms;
        }

        public void DrawModel(Model model, Matrix modelTransform, Matrix[] absoluteBoneTransforms)
        {
           
            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * modelTransform;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        private void writeText(string msg, Vector2 msgPos, Color msgColour)
        {
            spriteBatch.Begin();
            string output = msg;
            // Find the center of the string
            Vector2 FontOrigin = fontToUse.MeasureString(output) / 2;
            Vector2 FontPos = msgPos;
            // Draw the string
            spriteBatch.DrawString(fontToUse, output, FontPos, msgColour);
            spriteBatch.End();
        }

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
           
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            Window.Title = "Lab 6 - Collision Detection";
            
            hitCount = 0;
            ResetDaleks();
            InitializeTransform();
            InitializeEffect();
            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            InitializeTransform();
            InitializeEffect();
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //-------------------------------------------------------------
            // added to load font
            //-------------------------------------------------------------
            fontToUse = Content.Load<SpriteFont>(".\\Fonts\\DrWho");
            //-------------------------------------------------------------
            // added to load Song
            //-------------------------------------------------------------
            bkgMusic = Content.Load<Song>(".\\Audio\\DoctorWhotheme11");
            MediaPlayer.Volume = 0.25f;
            //MediaPlayer.Play(bkgMusic);
            MediaPlayer.IsRepeating = true;
            songInfo = "Song: " + bkgMusic.Name + " Song Duration: " + bkgMusic.Duration.Minutes + ":" + bkgMusic.Duration.Seconds;
            
            //-------------------------------------------------------------
            // added to load Model
            //-------------------------------------------------------------
            mdlTankBarrel = Content.Load<Model>(".\\Models\\T_302barrel");
            
            mdlTankTracks = Content.Load<Model>(".\\Models\\T_302tracks");
            
            mdlTankTurret = Content.Load<Model>(".\\Models\\T_302turret");

            mdlGround = Content.Load<Model>(".\\Models\\ground");

            mdlFence = Content.Load<Model>(".\\Models\\fence");

            mdlLaser = Content.Load<Model>(".\\Models\\shell");

            mdlDalek = Content.Load<Model>(".\\Models\\target");
            setUpEffectTransforms();



            mainMenu = Content.Load<Texture2D>(".\\Textures\\mainmenu");
            instructions = Content.Load<Texture2D>(".\\Textures\\instructions");
            gameOver = Content.Load<Texture2D>(".\\Textures\\gameover");
            
            
             
            //-------------------------------------------------------------
            // added to load SoundFX's
            //-------------------------------------------------------------
            tardisSound = Content.Load<SoundEffect>("Audio\\birds");
            explosionSound = Content.Load<SoundEffect>("Audio\\explosion");
            firingSound = Content.Load<SoundEffect>("Audio\\tankshot");
            engineSound = Content.Load<SoundEffect>("Audio\\engineidle");

            
            tardisSoundInstance = tardisSound.CreateInstance();

            tardisSoundInstance.Volume = 0.25f;
            
            
            tardisSoundInstance.Play();
            engineSoundInstance = engineSound.CreateInstance();
            engineSoundInstance.Volume = 0.25f;
            engineSoundInstance.IsLooped = true;
            engineSoundInstance.Play();


             // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gameState = GamePad.GetState(PlayerIndex.One);
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (state.Equals("game"))
            {
                gameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (gameTimer >= 30.0f)
                {
                    state = "gameover";
                    ResetDaleks();
                }
                // TODO: Add your update logic here
                MoveModel();
                CheckSpeed();

                // Add velocity to the current position.
                mdlPosition += mdlVelocity;

                // Bleed off velocity over time.
                mdlVelocity *= 0.95f;
                camera1.Update();
                setUpEffectTransforms();
                float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                /*
                for (int i = 0; i < GameConstants.NumDaleks; i++)
                {
                    dalekList[i].Update(timeDelta);
                }
                */
                for (int i = 0; i < GameConstants.NumLasers; i++)
                {
                    if (laserList[i].isActive)
                    {
                        laserList[i].Update(timeDelta);
                    }
                }

                Vector3 tankForward = Matrix.CreateRotationY(mdlRotation).Forward;
                Vector3 tankRight = Vector3.Cross(tankForward, Vector3.Up);
                /*
                BoundingBox TankBox =
                  new BoundingBox(mdlPosition - tankForward * 4 - Vector3.Cross(tankForward,Vector3.Up) * 2,
                                  mdlPosition + tankForward * 4 + Vector3.Cross(tankForward, Vector3.Up) * 2);

                BoundingBox northWall = new BoundingBox(new Vector3(46.5f, -10, -46.5f), new Vector3(-46.5f, 10, -47.5f));
                if (TankBox.Intersects(northWall))
                {
                    mdlVelocity = new Vector3(mdlVelocity.X, mdlVelocity.Y, -mdlVelocity.Z);
                }*/

                if (mdlPosition.X > 44)
                {
                    mdlPosition = new Vector3(44, mdlPosition.Y, mdlPosition.Z);
                    mdlVelocity *= 0.4f;
                }
                if (mdlPosition.X < -44)
                {
                    mdlPosition = new Vector3(-44, mdlPosition.Y, mdlPosition.Z);
                    mdlVelocity *= 0.4f;
                }
                if (mdlPosition.Z > 44)
                {
                    mdlPosition = new Vector3(mdlPosition.X, mdlPosition.Y, 44);
                    mdlVelocity *= 0.4f;
                }
                if (mdlPosition.Z < -44)
                {
                    mdlPosition = new Vector3(mdlPosition.X, mdlPosition.Y, -44);
                    mdlVelocity *= 0.4f;
                }
                //Check for collisions
                for (int i = 0; i < dalekList.Length; i++)
                {
                    if (dalekList[i].isActive)
                    {
                        BoundingSphere dalekSphereA =
                          new BoundingSphere(dalekList[i].position, 2 *
                                         GameConstants.DalekBoundingSphereScale);

                        for (int k = 0; k < laserList.Length; k++)
                        {
                            if (laserList[k].isActive)
                            {
                                BoundingSphere laserSphere = new BoundingSphere(
                                  laserList[k].position, 2 *
                                         GameConstants.LaserBoundingSphereScale);
                                if (dalekSphereA.Intersects(laserSphere))
                                {
                                    if (!audioMuted)
                                    {
                                        SoundEffectInstance explosionSoundInstance = explosionSound.CreateInstance();
                                        if ((100 - (dalekList[i].position - mdlPosition).Length()) / 100.0f > 0.1f)
                                        {
                                            explosionSoundInstance.Volume = (100 - (dalekList[i].position - mdlPosition).Length()) / 100.0f;
                                        }
                                        else
                                        {
                                            explosionSoundInstance.Volume = 0.1f;
                                        }
                                        explosionSoundInstance.Play();
                                    }
                                    dalekList[i].isActive = false;
                                    laserList[k].isActive = false;

                                    hitCount++;
                                    //no need to check other bullets
                                }
                            }
                            /*if (dalekSphereA.Intersects(TardisSphere)) //Check collision between Dalek and Tardis
                            {
                                explosionSound.Play();
                                dalekList[i].direction *= -1.0f;
                                //laserList[k].isActive = false;
                                break; //no need to check other bullets
                            }*/

                        }
                    }
                }

            }
            if(state.Equals("menu"))
            {
                if ((keyboardState.IsKeyDown(Keys.Enter) && !lastState.IsKeyDown(Keys.Enter)) || (gameState.IsButtonDown(Buttons.A) && !lastGame.IsButtonDown(Buttons.A)))
                {
                   
                    state = "instructions";
                    
                }

            }
            
            else if (state.Equals("instructions"))
            {
                if ((keyboardState.IsKeyDown(Keys.Enter) && !lastState.IsKeyDown(Keys.Enter)) || (gameState.IsButtonDown(Buttons.A) && !lastGame.IsButtonDown(Buttons.A)))
                {
                    
                    state = "game";
                    
                }
            }
            else if (state.Equals("gameover"))
            {
                if ((keyboardState.IsKeyDown(Keys.Enter) && !lastState.IsKeyDown(Keys.Enter)) || (gameState.IsButtonDown(Buttons.A) && !lastGame.IsButtonDown(Buttons.A)))
                {
                    
                    state = "menu";
                    
                }
            }
            lastState = keyboardState;
            lastGame = gameState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (state.Equals("game"))
            {

                // TODO: Add your drawing code here
                for (int i = 0; i < GameConstants.NumDaleks; i++)
                {
                    if (dalekList[i].isActive)
                    {
                        Matrix dalekTransform = Matrix.CreateRotationY(dalekList[i].angle) * Matrix.CreateTranslation(dalekList[i].position);
                        DrawModel(mdlDalek, dalekTransform, mdDalekTransforms);
                    }
                }

                for (int i = 0; i < GameConstants.NumLasers; i++)
                {
                    if (laserList[i].isActive)
                    {
                        Matrix laserTransform = Matrix.CreateScale(GameConstants.LaserScalar) * Matrix.CreateRotationX(laserList[i].pitch) * Matrix.CreateRotationY(laserList[i].rotation) * Matrix.CreateTranslation(laserList[i].position);
                        DrawModel(mdlLaser, laserTransform, mdlLaserTransforms);
                    }
                }

                this.basicEffect.View = mainCamera.camViewMatrix;
                float offset = -0.93402f;
                float rotation = (float)(mdlRotation / (Math.PI * 2) * 360);

                Matrix modelBarrelTransform = Matrix.CreateRotationX(mdlBarrelPitch) *
                    Matrix.CreateRotationY(mdlTurretRotation + mdlRotation) *
                    Matrix.CreateTranslation(mdlPosition + Vector3.Cross(Matrix.CreateRotationY(mdlTurretRotation + mdlRotation).Forward, Vector3.Up) * offset - Matrix.CreateRotationY(mdlTurretRotation + mdlRotation).Forward * (offset - mdlBarrelPitch * 2));//offset + (float)Math.Sin(mdlRotation) * offset ,0,  0));
                Matrix modelTracksTransform = Matrix.CreateRotationY(mdlRotation) * Matrix.CreateTranslation(mdlPosition);
                Matrix modelTurretTransform = Matrix.CreateRotationY(mdlTurretRotation + mdlRotation) * Matrix.CreateTranslation(mdlPosition);
                DrawModel(mdlTankBarrel, modelBarrelTransform, mdlTankBarrelTransforms);
                DrawModel(mdlTankTracks, modelTracksTransform, mdlTankTracksTransforms);
                DrawModel(mdlTankTurret, modelTurretTransform, mdlTankTurretTransforms);
                DrawModel(mdlFence, Matrix.Identity, mdlFenceTransforms);
                DrawModel(mdlGround, Matrix.Identity, mdlGroundTransforms);
                //* 
                String text = "";
                if (audioMuted)
                {
                    text = "Audio muted";
                }
                else if (!audioMuted)
                {
                    text = "";
                }

                Vector3 tankForward = Matrix.CreateRotationY(mdlRotation).Forward;
                Vector3 tankRight = Vector3.Cross(tankForward, Vector3.Up);
                writeText(text, new Vector2(50, 10), Color.Yellow);
                writeText("Time left: "+ (30 - ((int)gameTimer)).ToString(), new Vector2(50, 50), Color.Black);

                writeText("Score: " + hitCount.ToString(), new Vector2(50, 125), Color.AntiqueWhite);
            }
            if (state.Equals("menu"))
            {
                Rectangle splash = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                spriteBatch.Begin();
                spriteBatch.Draw(mainMenu, splash, Color.White);
                spriteBatch.End();
                
            }
            if (state.Equals("instructions"))
            {
                Rectangle splash = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                spriteBatch.Begin();
                spriteBatch.Draw(instructions, splash, Color.White);
                spriteBatch.End();
            }
            if(state.Equals("gameover"))
            {
                Rectangle splash = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                spriteBatch.Begin();
                spriteBatch.Draw(gameOver, splash, Color.White);
                spriteBatch.End();
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
