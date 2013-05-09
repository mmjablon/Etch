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
        private BasicModel groundCurrent2;
        private BasicModel groundSecondary2;
        private BasicModel endTree;
        private BasicModel moon;
        private Boolean ground;
        private Boolean ground2;

        private float resetTime1;
        private float collisionTime1;

        private float resetTime2;
        private float collisionTime2;

        private int groundDisplacement = -131070;
        private int groundSwaps;
        private int groundSwaps2;
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

                //set up player independent variables
                groundSwaps2 = 1;
                ground2 = true;


                // Perform an inital reset on the camera so that it starts at the resting
                // position. If we don't do this, the camera will start at the origin and
                // race across the world to get behind the chased object.
                // This is performed here because the aspect ratio is needed by Reset.
                UpdateCameraChaseTarget(camera2, etch2);
                camera2.Reset();
            }

            resetTime1 = 0;
            resetTime2 = 0;
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

            if (!singlePlayer)
            {
                groundCurrent2 = new BasicModel(Game.Content.Load<Model>("Models\\Ground"), Matrix.Identity);
                groundSecondary2 = new BasicModel(Game.Content.Load<Model>("Models\\Ground"), Matrix.Identity);
                groundSecondary2.translate(new Vector3(0, 0, groundDisplacement));
            }
        }

        private void Player1Collisions(GameTime gameTime)
        {
            //player 1 stuff
            collisionTime1 -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Reset the ship on R key or right thumb stick clicked
            if (resetTime1 > 0)
            {
                resetTime1 -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                camera1.Reset();
            }
            else
            {
                GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
                if (collisionTime1 < 0)
                {
                    foreach (Tree tree in trees)
                    {
                        if (collisionRange(tree, etch1))
                        {
                            if (tree.CollidesWith(etch1))
                            {
                                collision.Play();
                                GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
                                resetTime1 = 1;
                                collisionTime1 = 3;
                                etch1.Reset();
                                camera1.Reset();
                            }
                        }
                    }
                }

                if (collisionTime1 < 0)
                {
                    foreach (Bush bush in bushes)
                    {
                        if (collisionRange(bush, etch1))
                        {
                            if (etch1.CollidesWith(bush) && etch1.getHeight() < 550)
                            {
                                collision.Play();
                                GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
                                resetTime1 = 1;
                                collisionTime1 = 3;
                                etch1.Reset();
                                camera1.Reset();
                            }
                        }
                    }
                }

                //iterate backwards to delete while iterating
                for (int i = bugs.Count - 1; i >= 0; i--)
                {
                    Bug bug = bugs[i];
                    if (inViewRange(bug, etch1))
                    {
                        bug.Update(gameTime);
                        if (collisionRange(bug, etch1))
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
        }

        private void Player2Collisions(GameTime gameTime)
        {
            collisionTime2 -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Reset the ship on R key or right thumb stick clicked
            if (resetTime2 > 0)
            {
                resetTime2 -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                camera2.Reset();
            }
            else
            {
                GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
                if (collisionTime2 < 0)
                {
                    foreach (Tree tree in trees)
                    {
                        if (collisionRange(tree, etch2))
                        {
                            if (tree.CollidesWith(etch2))
                            {
                                collision.Play();
                                GamePad.SetVibration(PlayerIndex.Two, 0.5f, 0.5f);
                                resetTime2 = 1;
                                collisionTime2 = 3;
                                etch2.Reset();
                                camera2.Reset();
                            }
                        }
                    }
                }

                if (collisionTime2 < 0)
                {
                    foreach (Bush bush in bushes)
                    {
                        if (collisionRange(bush, etch2))
                        {
                            if (etch2.CollidesWith(bush) && etch2.getHeight() < 550)
                            {
                                collision.Play();
                                GamePad.SetVibration(PlayerIndex.Two, 0.5f, 0.5f);
                                resetTime2 = 1;
                                collisionTime2 = 3;
                                etch2.Reset();
                                camera2.Reset();
                            }
                        }
                    }
                }

                //iterate backwards to delete while iterating
                for (int i = bugs.Count - 1; i >= 0; i--)
                {
                    Bug bug = bugs[i];
                    if (inViewRange(bug, etch2))
                    {
                        //make sure we dont double update
                        if (!inViewRange(bug, etch1))
                        {
                            bug.Update(gameTime);
                        }
                        if (collisionRange(bug, etch2))
                        {
                            if (etch2.CollidesWith(bug))
                            {
                                numBugs2++;
                                crunch.Play();
                                bugs.Remove(bug);
                            }
                        }
                    }
                }
            }
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
                if (!finished1())
                {
                    etch1.Update(gameTime);
                    camera1.Update(gameTime);
                }

                if(!finished2())
                {
                    etch2.Update(gameTime);
                    camera2.Update(gameTime);
                }
            }

            if (singlePlayer)
            {
                Player1Collisions(gameTime);
            }
            else
            {
                Player1Collisions(gameTime);
                Player2Collisions(gameTime);
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
                //if etcch gets near the edge of one ground plane translate it
                if (etch1.Position.Z < groundSwaps * groundDisplacement)
                {
                    continueGround();
                    groundSwaps++;
                }

                //if etcch gets near the edge of one ground plane translate it
                if (etch2.Position.Z < groundSwaps2 * groundDisplacement)
                {
                    continueGround2();
                    groundSwaps2++;
                }
                UpdateCameraChaseTarget(camera1, etch1);
                UpdateCameraChaseTarget(camera2, etch2);
            }

            base.Update(gameTime);
        }

        private bool collisionRange(BasicModel t, Etch etch)
        {
            Vector3 treePos = t.getWorld().Translation;
            Vector3 etchPos = etch.Position;

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

        public void continueGround2()
        {
            if (ground2)
            {
                groundCurrent2.translate(new Vector3(0, 0, 2 * groundDisplacement));
                ground2 = !ground2;
            }
            else
            {
                groundSecondary2.translate(new Vector3(0, 0, 2 * groundDisplacement));
                ground2 = !ground2;
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

        public float percentComplete1()
        {
            if (etch1.Position.Z < -currentLevel.levelEnd)
            {
                return 1;
            }
            else
            {
                return etch1.Position.Z / -currentLevel.levelEnd;
            }
        }


        public float percentComplete2()
        {
            if (etch2.Position.Z < -currentLevel.levelEnd)
            {
                return 1;
            }
            else
            {
                return etch2.Position.Z / -currentLevel.levelEnd;
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
                groundCurrent2.DrawModel(camera2);
                groundSecondary2.DrawModel(camera2);
                endTree.DrawModel(camera2);
                moon.DrawModel(camera2);
            }

            GraphicsDevice.Viewport = defaultViewport;

            base.Draw(gameTime);
        }

        public void reset()
        {
            etch1.ResetComplete();
            camera1.Reset();
            UpdateCameraChaseTarget(camera1, etch1);

            if (!singlePlayer)
            {
                etch2.ResetComplete();
                camera2.Reset();
            }

            loadLevel(1);
        }
    }
}
