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
        private Viewport defaultViewport;
        private Viewport leftViewport;
        private Viewport rightViewport;

        private IList<Tree> trees;
        private IList<Bush> bushes;
        private IList<BasicModel> bugs;

        private BasicModel groundCurrent;
        private BasicModel groundSecondary;
        private Boolean ground;

        private double resetTime;

        private int groundDisplacement = -131070;
        private int groundSwaps;
        private int maxX;

        private bool singlePlayer;

        private SoundEffect collision;

        public ModelManager(Game game, GraphicsDeviceManager graphics, bool singlePlayer, int level)
            : base(game)
        {
            this.singlePlayer = singlePlayer;

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

                etch1 = new Etch(graphics.GraphicsDevice, Game.Content.Load<Model>("Models\\EtchAnimated"), Matrix.Identity, camera1, maxX);

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

                etch1 = new Etch(graphics.GraphicsDevice, Game.Content.Load<Model>("Models\\EtchAnimated"), Matrix.Identity, camera1, maxX);

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

                etch2 = new Etch(graphics.GraphicsDevice, Game.Content.Load<Model>("Models\\EtchAnimated"), Matrix.Identity, camera1, maxX);

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

        public void loadLevel(int level)
        {
            Level current = Levels.getLevel(level);
            trees = current.trees;
            bushes = current.bushes;
            bugs = current.bugs;
            maxX = current.maxX;
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
            Bug.model = Game.Content.Load<Model>("Models\\fly");

            collision = Game.Content.Load<SoundEffect>("Audio\\owl");
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
                // Update the Etch
                etch1.Update(gameTime);
                camera1.Update(gameTime);
            }
            else
            {
                // Update the Etch
                etch1.Update(gameTime);
                camera1.Update(gameTime);
                etch2.Update(gameTime);
                camera2.Update(gameTime);
            }
            
            // Reset the ship on R key or right thumb stick clicked
            if (resetTime > 0)
            {
                resetTime -= gameTime.ElapsedGameTime.TotalSeconds;
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
                    if (inRange(tree))
                    {
                        if (etch1.CollidesWith(tree))
                        {
                            collision.Play();
                            resetTime = 1;
                            etch1.Reset();
                            camera1.Reset();
                        }
                    }
                }
            }

            //if etcch gets near the edge of one ground plane translate it
            if (etch1.Position.Z < groundSwaps * groundDisplacement)
            {
                continueGround();
                groundSwaps++;
            }

            if (singlePlayer)
            {
                // Update the camera to chase the new target
                UpdateCameraChaseTarget(camera1, etch1);
            }
            else
            {
                // Update the camera to chase the new target
                UpdateCameraChaseTarget(camera1, etch1);
                UpdateCameraChaseTarget(camera2, etch2);
            }

            base.Update(gameTime);
        }

        private bool inRange(Tree t)
        {
            Vector3 treePos = t.GetWorld().Translation;
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

        public override void Draw(GameTime gameTime)
        {
            if (singlePlayer)
            {
                etch1.DrawModel(camera1);
                foreach (Tree tree in trees)
                {
                    tree.DrawModel(camera1);
                }

                foreach (Bush bush in bushes)
                {
                    bush.DrawModel(camera1);
                }

                foreach (BasicModel bug in bugs)
                {
                    bug.DrawModel(camera1);
                }
                groundCurrent.DrawModel(camera1);
                groundSecondary.DrawModel(camera1);
            }
            else
            {
                GraphicsDevice.Viewport = leftViewport;

                etch1.DrawModel(camera1);
                etch2.DrawModel(camera1);
                foreach (Tree tree in trees)
                {
                    tree.DrawModel(camera1);
                }

                foreach (Bush bush in bushes)
                {
                    bush.DrawModel(camera1);
                }

                foreach (BasicModel bug in bugs)
                {
                    bug.DrawModel(camera1);
                }
                groundCurrent.DrawModel(camera1);
                groundSecondary.DrawModel(camera1);

                GraphicsDevice.Viewport = rightViewport;

                etch1.DrawModel(camera2);
                etch2.DrawModel(camera2);
                foreach (Tree tree in trees)
                {
                    tree.DrawModel(camera2);
                }

                foreach (Bush bush in bushes)
                {
                    bush.DrawModel(camera2);
                }

                foreach (BasicModel bug in bugs)
                {
                    bug.DrawModel(camera2);
                }
                groundCurrent.DrawModel(camera2);
                groundSecondary.DrawModel(camera2);
            }

            //GraphicsDevice.Viewport = defaultViewport;

            base.Draw(gameTime);
        }
    }
}
