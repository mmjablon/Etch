using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace EtchTheOwl
{
    class ModelManager : DrawableGameComponent
    {

        private KeyboardState currentKeyboardState = new KeyboardState();
        private GamePadState currentGamePadState = new GamePadState();
        private MouseState currentMouseState = new MouseState();

        public Etch etch1;
        public Etch etch2;
        private ChaseCamera camera1;
        private ChaseCamera camera2;
        public Viewport defaultViewport;
        public Viewport leftViewport;
        public Viewport rightViewport;

        private IList<Tree> trees;
        private IList<Bush> bushes;
        private IList<Bug> bugs;

        private BasicModel groundCurrent;
        private BasicModel groundSecondary;
        private BasicModel endTree;
        private BasicModel moon;
        private Boolean ground;

        private float resetTime;
        private float collisionTime;

        private int groundDisplacement = -131070;
        private int groundSwaps;
        private int maxX;

        private bool singlePlayer;
        public int numBugs1;
        public int numBugs2;

        private SoundEffect collision;
        private SoundEffect crunch;
        private Level currentLevel;

        public ModelManager(Game game, GraphicsDeviceManager graphics, bool singlePlayer, int level)
            : base(game)
        {
            this.singlePlayer = singlePlayer;
            numBugs1 = 0;
            numBugs2 = 0;

            //set up player independent variables
            groundSwaps = 1;
            ground = true;

            loadLevel(level);

            if (singlePlayer)
            {
                camera1 = new ChaseCamera();

                // Set the camera offsets
                camera1.DesiredPositionOffset = new Vector3(0.0f, 400.0f, 700.0f);
                camera1.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

                // Set camera perspective
                camera1.NearPlaneDistance = 10.0f;
                camera1.FarPlaneDistance = 100000.0f;

                etch1 = new Etch(graphics.GraphicsDevice, Game.Content.Load<Model>("Models\\EtchAnimated"), new Vector3(0,0, 10000), camera1, maxX);

                camera1.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                   graphics.GraphicsDevice.Viewport.Height;

                UpdateCameraChaseTarget(camera1, etch1);
                camera1.Reset();
            }
            else
            {
                //player one parameters
                camera1 = new ChaseCamera();

                // Set the camera offsets
                camera1.DesiredPositionOffset = new Vector3(0.0f, 400.0f, 700.0f);
                camera1.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

                // Set camera perspective
                camera1.NearPlaneDistance = 10.0f;
                camera1.FarPlaneDistance = 100000.0f;

                etch1 = new Etch(graphics.GraphicsDevice, Game.Content.Load<Model>("Models\\EtchAnimated"),
                    new Vector3(0,0,10000), camera1, maxX);

                camera1.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /2 /
                   graphics.GraphicsDevice.Viewport.Height;

                UpdateCameraChaseTarget(camera1, etch1);
                camera1.Reset();

                //player two parameters
                camera2 = new ChaseCamera();

                // Set the camera offsets
                camera2.DesiredPositionOffset = new Vector3(0.0f, 400.0f, 700.0f);
                camera2.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

                // Set camera perspective
                camera2.NearPlaneDistance = 10.0f;
                camera2.FarPlaneDistance = 100000.0f;

                etch2 = new Etch(graphics.GraphicsDevice, Game.Content.Load<Model>("Models\\EtchAnimated"),
                    new Vector3(-1000, 0, 10000), camera1, maxX);

                // Set the camera aspect ratio
                // This must be done after the class to base.Initalize() which will
                // initialize the graphics device.
                camera2.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width/2 /
                   graphics.GraphicsDevice.Viewport.Height;


                // Perform an inital reset on the camera so that it starts at the resting
                // position. If we don't do this, the camera will start at the origin and
                // race across the world to get behind the chased object.
                // This is performed here because the aspect ratio is needed by Reset.
                UpdateCameraChaseTarget(camera2, etch2);
                camera2.Reset();
            }

            resetTime = 0;
        }

        public int getPlayer1Bugs(){
            return numBugs1;
        }

        public int getPlayer2Bugs()
        {
            return numBugs2;
        }

        public void loadLevel(int level)
        {
            currentLevel = Levels.getLevel(level);
            trees = currentLevel.trees;
            bushes = currentLevel.bushes;
            bugs = currentLevel.bugs;
            maxX = currentLevel.maxX;
        }

        /// <summary>
        /// Update the values to be chased by the camera
        /// </summary>
        private void UpdateCameraChaseTarget(ChaseCamera camera, Etch target)
        {
            camera.ChasePosition = target.Position;
            camera.ChaseDirection = target.Direction;
            camera.Up = target.Up;
        }

        protected override void LoadContent()
        {
            defaultViewport = GraphicsDevice.Viewport;
            leftViewport = defaultViewport;
            rightViewport = defaultViewport;
            leftViewport.Width = leftViewport.Width / 2;
            rightViewport.Width = rightViewport.Width / 2;
            rightViewport.X = leftViewport.Width;

            Enemy.model = Game.Content.Load<Model>("Models\\Enemy");
            Tree.model = Game.Content.Load<Model>("Models\\tree");
            Bush.model = Game.Content.Load<Model>("Models\\bush");
            Bug.model = Game.Content.Load<Model>("Models\\flyrotated");
            endTree = new BasicModel(Game.Content.Load<Model>("Models\\endtree"), Matrix.CreateTranslation(0,0,-currentLevel.levelEnd - 30000));
            //moon on the horizon, change size to x100
            moon = new BasicModel(Game.Content.Load<Model>("Models\\moon"), Matrix.CreateTranslation(0, 0, -currentLevel.levelEnd - 120000));
            //moon behind tree, change size to x10
            //moon = new BasicModel(Game.Content.Load<Model>("Models\\moon"), Matrix.CreateTranslation(10000, 10000, -currentLevel.levelEnd - 70000));


            collision = Game.Content.Load<SoundEffect>("Audio\\owl");
            crunch = Game.Content.Load<SoundEffect>("Audio\\crunch");
            groundCurrent = new BasicModel(Game.Content.Load<Model>("Models\\Ground"), Matrix.Identity);
            groundSecondary = new BasicModel(Game.Content.Load<Model>("Models\\Ground"), Matrix.Identity);
            groundSecondary.translate(new Vector3(0, 0, groundDisplacement));
        }

        public override void Update(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

            if (singlePlayer)
            {
                // Update Etch
                etch1.Update(gameTime);
                camera1.Update(gameTime);
            }
            else
            {
                // Update Etch
                etch1.Update(gameTime);
                camera1.Update(gameTime);
                etch2.Update(gameTime);
                camera2.Update(gameTime);
            }
            
            // Reset the ship on R key or right thumb stick clicked
            if (resetTime > 0)
            {
                resetTime -= (float) gameTime.ElapsedGameTime.TotalSeconds;
                camera1.Reset();
            }
            else if (currentKeyboardState.IsKeyDown(Keys.R) ||
               currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                resetTime = 2;
                etch1.ResetComplete();
                camera1.Reset();
            }
            else
            {
                foreach (Tree tree in trees)
                {
                    if (collisionRange(tree))
                    {
                        if (tree.CollidesWith(etch1))
                        {
                            collision.Play();
                            resetTime = 1;
                            etch1.Reset();
                            camera1.Reset();
                        }
                    }
                }


                foreach (Bush bush in bushes)
                {
                    if (collisionRange(bush))
                    {
                        if (etch1.CollidesWith(bush) && etch1.getHeight() < 550)
                        {
                            collision.Play();
                            resetTime = 1;
                            etch1.Reset();
                            camera1.Reset();
                        }
                    }
                }

                //iterate backwards to delete while iterating
                for(int i = bugs.Count - 1; i >= 0; i--)
                {
                    Bug bug = bugs[i];
                    if (inViewRange(bug, etch1))
                    {
                        bug.Update(gameTime);
                        if (collisionRange(bug))
                        {
                            if (etch1.CollidesWith(bug))
                            {
                                numBugs1++;
                                crunch.Play();
                                bugs.Remove(bug);
                            }
                        }
                    }
                }
            }

            if (singlePlayer)
            {
                // Update the camera to chase the new target
                UpdateCameraChaseTarget(camera1, etch1);

                //if etcch gets near the edge of one ground plane translate it
                if (etch1.Position.Z < groundSwaps * groundDisplacement)
                {
                    continueGround();
                    groundSwaps++;
                }
            }
            else
            {
                // Update the camera to chase the new target
                UpdateCameraChaseTarget(camera1, etch1);
                UpdateCameraChaseTarget(camera2, etch2);
            }

            base.Update(gameTime);
        }

        private bool collisionRange(BasicModel t)
        {
            Vector3 treePos = t.getWorld().Translation;
            Vector3 etchPos = etch1.Position;

            double maxZ = treePos.Z + 1000;
            double minZ = treePos.Z - 1000;

            if (etchPos.Z <= maxZ && etchPos.Z >= minZ)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool inViewRange(BasicModel t, Etch etch)
        {
            float modelLocation = t.getWorld().Translation.Z;
            float etchLocation = etch.Position.Z;
            if (modelLocation -1000<= etchLocation && modelLocation >= etchLocation - 100000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void continueGround()
        {
            if (ground)
            {
                groundCurrent.translate(new Vector3(0, 0, 2 * groundDisplacement));
                ground = !ground;
            }
            else
            {
                groundSecondary.translate(new Vector3(0, 0, 2 * groundDisplacement));
                ground = !ground;
            }
        }

        public bool finished1()
        {
            if (etch1.Position.Z < -currentLevel.levelEnd - 25000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool finished2()
        {
            if (etch2.Position.Z < -currentLevel.levelEnd - 25000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (singlePlayer)
            {
                etch1.DrawModel(camera1);
                foreach (Tree tree in trees)
                {
                    if(inViewRange(tree, etch1)){
                        tree.DrawModel(camera1);
                    }
                }

                foreach (Bush bush in bushes)
                {
                    if (inViewRange(bush, etch1))
                    {
                        bush.DrawModel(camera1);
                    }
                }

                foreach (BasicModel bug in bugs)
                {
                    if (inViewRange(bug, etch1))
                    {
                        bug.DrawModel(camera1);
                    }
                }
                groundCurrent.DrawModel(camera1);
                groundSecondary.DrawModel(camera1);
                endTree.DrawModel(camera1);
                moon.DrawModel(camera1);
            }
            else
            {
                GraphicsDevice.Viewport = leftViewport;

                etch1.DrawModel(camera1);
                etch2.DrawModel(camera1);
                foreach (Tree tree in trees)
                {
                    if (inViewRange(tree, etch1))
                    {
                        tree.DrawModel(camera1);
                    }
                }

                foreach (Bush bush in bushes)
                {
                    if (inViewRange(bush, etch1))
                    {
                        bush.DrawModel(camera1);
                    }
                }

                foreach (BasicModel bug in bugs)
                {
                    if (inViewRange(bug, etch1))
                    {
                        bug.DrawModel(camera1);
                    }
                }
                groundCurrent.DrawModel(camera1);
                groundSecondary.DrawModel(camera1);
                endTree.DrawModel(camera1);
                moon.DrawModel(camera1);

                GraphicsDevice.Viewport = rightViewport;

                etch1.DrawModel(camera2);
                etch2.DrawModel(camera2);
                foreach (Tree tree in trees)
                {
                    if (inViewRange(tree, etch2))
                    {
                        tree.DrawModel(camera2);
                    }
                }

                foreach (Bush bush in bushes)
                {
                    if (inViewRange(bush, etch2))
                    {
                        bush.DrawModel(camera2);
                    }
                }

                foreach (BasicModel bug in bugs)
                {
                    if (inViewRange(bug, etch2))
                    {
                        bug.DrawModel(camera2);
                    }
                }
                groundCurrent.DrawModel(camera2);
                groundSecondary.DrawModel(camera2);
                endTree.DrawModel(camera2);
                moon.DrawModel(camera2);
            }

            //GraphicsDevice.Viewport = defaultViewport;

            base.Draw(gameTime);
        }
    }
}
