#region File Description
//-----------------------------------------------------------------------------
// etch.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using SkinnedModel;
#endregion


namespace EtchTheOwl
{
    enum Controls { Keys, ControllerOne, ControllerTwo };

    class Etch : BasicModel
    {
        #region Fields

        private const float MinimumAltitude = 350.0f;
        private const float MaximumAltitude = 2000.0f;

        // A reference to the graphics device used to access the viewport for touch input.
        private GraphicsDevice graphicsDevice;

        // Location of etch in world space.
        public Vector3 Position;

        // Direction etch is facing.
        public Vector3 Direction;

        // etch's up vector.
        public Vector3 Up;

        private Vector3 right;
        // etch's right vector.
        public Vector3 Right
        {
            get { return right; }
        }

        // Full speed at which etch can rotate; measured in radians per second.
        private const float RotationRate = 1.5f;

        // Mass of etch.
        private const float Mass = 5.0f;

        // Maximum force that can be applied along the etch's direction.
        private const float ThrustForce = 24000.0f;

        // Velocity scalar to approximate drag.
        private const float DragFactor = 0.99f;

        // Current etch velocity.
        public Vector3 Velocity;

        //speed at which etch moves left to right
        public float HorizontalVelocity = 2000.0f;

        //Increase all of etches movement by a factor
        public float speedUp;

        //maximum speed up and down, used when right trigger is used for variable speed
        float maxSpeedUp = 1.0f;

        //The furthest etch can go from left to right
        private int maxX;

        //etch's flying animation
        private AnimationPlayer animationPlayer;

        //the starting clip for etch
        private AnimationClip startClip;

        //if etch is currently in a reset
        private Boolean reset;

        //time until etch can move after reset
        private double resetTime;

        //animation for when etch hits a tree
        private bool shakeDirection;

        //The camera chasing this etch
        private ChaseCamera camera;

        //Determines if etch can move up or down, or simply jumps
        private bool jumpOn = false;
        
        //Etch's controller type
        public Controls controller;

        //If etch's up and down controls are inverted
        public bool inverted;

        #endregion

        #region Initialization

        public Etch(GraphicsDevice device, Model model, Vector3 position, ChaseCamera camera, int maxX)
            : base(model, Matrix.Identity)
        {
            graphicsDevice = device;
            this.camera = camera;
            this.maxX = maxX;
            speedUp = 1;

            controller = Controls.Keys;

            // Look up our custom skinning information.
            SkinningData skinningData = model.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            startClip = skinningData.AnimationClips["Action"];

            animationPlayer.StartClip(startClip);

            reset = false;
            resetTime = 0;
            shakeDirection = true;
            inverted = false;
            ResetComplete();

            Position = new Vector3(position.X, MinimumAltitude, position.Z);
            world.Translation = Position;
            rotateAlongZ(-ZRotation);
        }

        public void Reset()
        {
            Position = new Vector3(Position.X, MinimumAltitude, Position.Z + 10000);
            world.Translation = Position;
            rotateAlongZ(-ZRotation);
            Velocity = Vector3.Zero;
            reset = true;
            resetTime = 1;
            animationPlayer.Update(new TimeSpan(0, 0, 0), false, Matrix.Identity);
        }

        //Reset etch to his original starting state
        public void ResetComplete()
        {
            Position = new Vector3(0, MinimumAltitude, 0);
            Direction = Vector3.Forward;
            Up = Vector3.Up;
            right = Vector3.Right;
            Velocity = Vector3.Zero;
            rotateAlongZ(-ZRotation);
        }

        public float getHeight()
        {
            return world.Translation.Y;
        }

        public void speed(float speedUp)
        {
            this.speedUp = speedUp;
        }

        #endregion

        /// <summary>
        /// Applies a simple rotation to the etch and animates position based
        /// on simple linear motion physics.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (reset)
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                resetTime -= elapsed;

                if (ZRotation > .15 * Math.PI)
                    shakeDirection = false;
                if (ZRotation < -.15 * Math.PI)
                    shakeDirection = true;

                if (shakeDirection)
                    rotateAlongZ(10 * RotationRate * elapsed);
                else
                    rotateAlongZ(-10 * RotationRate * elapsed);

                Direction.Normalize();
                Up.Normalize();

                // Re-calculate Right
                right = Vector3.Cross(Direction, Up);

                // The same instability may cause the 3 orientation vectors may
                // also diverge. Either the Up or Direction vector needs to be
                // re-computed with a cross product to ensure orthagonality
                Up = Vector3.Cross(Right, Direction);
                world.Translation = Position;

                if (resetTime < 0)
                {
                    reset = false;
                }
            }
            else
            {

                GamePadState gamePadState;
                bool moveLeft = false;
                bool moveRight = false;
                bool jump = false;
                bool thrust = false;
                bool moveUp = false;
                bool moveDown = false;

                switch (controller)
                {
                    case Controls.Keys:
                        KeyboardState keyboardState = Keyboard.GetState();
                        if (jumpOn)
                        {
                            if (keyboardState.IsKeyDown(Keys.A))
                                moveLeft = true;
                            if (keyboardState.IsKeyDown(Keys.D))
                                moveRight = true;
                            if (keyboardState.IsKeyDown(Keys.Space))
                                jump = true;
                            if (keyboardState.IsKeyDown(Keys.Up))
                                thrust = true;
                        }
                        else
                        {
                            if (keyboardState.IsKeyDown(Keys.A))
                                moveLeft = true;
                            if (keyboardState.IsKeyDown(Keys.D))
                                moveRight = true;
                            if (keyboardState.IsKeyDown(Keys.Space))
                                thrust = true;
                            if (keyboardState.IsKeyDown(Keys.W))
                            {
                                moveUp = inverted ? false : true;
                                moveDown = inverted ? true : false;
                            }
                            if (keyboardState.IsKeyDown(Keys.S))
                            {
                                moveUp = inverted ? true : false;
                                moveDown = inverted ? false : true;
                            }
                        }

                        break;

                    case Controls.ControllerOne:
                        gamePadState = GamePad.GetState(PlayerIndex.One);

                        if (jumpOn)
                        {
                            if (gamePadState.ThumbSticks.Left.X < 0)
                                moveLeft = true;
                            if (gamePadState.ThumbSticks.Left.X > 0)
                                moveRight = true;
                            if (gamePadState.Buttons.A == ButtonState.Pressed)
                                jump = true;
                            if (gamePadState.Triggers.Right >0)
                            {
                                thrust = true;
                                if (gamePadState.Triggers.Right > 0)
                                    speedUp = maxSpeedUp * gamePadState.Triggers.Right;
                                else
                                    speedUp = 1.0f;
                            }
                        }
                        else
                        {
                            if (gamePadState.ThumbSticks.Left.X < 0)
                                moveLeft = true;
                            if (gamePadState.ThumbSticks.Left.X > 0)
                                moveRight = true;
                            if (gamePadState.Triggers.Right > 0 || gamePadState.Buttons.A == ButtonState.Pressed)
                            {
                                thrust = true;
                                if (gamePadState.Triggers.Right > 0)
                                    speedUp = maxSpeedUp * gamePadState.Triggers.Right;
                                else
                                    speedUp = 1.0f;
                            }
                            if (gamePadState.ThumbSticks.Left.Y > 0)
                            {
                                moveUp = inverted ? false : true;
                                moveDown = inverted ? true : false;
                            }
                            if (gamePadState.ThumbSticks.Left.Y < 0)
                            {
                                moveUp = inverted ? true : false;
                                moveDown = inverted ? false : true;
                            }
                        }

                        break;

                    case Controls.ControllerTwo:
                        gamePadState = GamePad.GetState(PlayerIndex.Two);
                        if (jumpOn)
                        {
                            if (gamePadState.ThumbSticks.Left.X < 0)
                                moveLeft = true;
                            if (gamePadState.ThumbSticks.Left.X > 0)
                                moveRight = true;
                            if (gamePadState.Buttons.A == ButtonState.Pressed)
                                jump = true;
                            if (gamePadState.Triggers.Right > 0)
                            {
                                thrust = true;
                                if (gamePadState.Triggers.Right > 0)
                                {
                                    speedUp = maxSpeedUp * gamePadState.Triggers.Right;
                                }
                            }
                        }
                        else
                        {
                            if (gamePadState.ThumbSticks.Left.X < 0)
                                moveLeft = true;
                            if (gamePadState.ThumbSticks.Left.X > 0)
                                moveRight = true;
                            if (gamePadState.Triggers.Right > 0 || gamePadState.Buttons.A == ButtonState.Pressed)
                            {
                                thrust = true;
                                if (gamePadState.Triggers.Right > 0)
                                {
                                    speedUp = maxSpeedUp * gamePadState.Triggers.Right;
                                }
                                else
                                {
                                    speedUp = 1.0f;
                                }
                            }
                            if (gamePadState.ThumbSticks.Left.Y > 0)
                            {
                                moveUp = inverted ? false : true;
                                moveDown = inverted ? true : false;
                            }
                            if (gamePadState.ThumbSticks.Left.Y < 0)
                            {
                                moveUp = inverted ? true : false;
                                moveDown = inverted ? false : true;
                            }
                        }

                        break;
                }

                float elapsed = speedUp * (float)gameTime.ElapsedGameTime.TotalSeconds;
                animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

                // Determine rotation amount from input
                if (moveLeft)
                {
                    if (ZRotation < .25 * Math.PI)
                    {
                        rotateAlongZ(RotationRate * elapsed);
                    }
                    float newX = Position.X - HorizontalVelocity * elapsed;
                    if (newX > -maxX){
                        Position.X = newX;
                    }
                    else
                    {
                        speedUp = 1.0f;
                    }
                }
                if (moveRight)
                {
                    if (ZRotation > -.25 * Math.PI)
                    {
                        rotateAlongZ(-RotationRate * elapsed);
                    }

                    float newX = Position.X + HorizontalVelocity * elapsed;
                    if (newX < maxX)
                    {
                        Position.X = newX;
                    }
                }

                // Re-normalize orientation vectors
                // Without this, the matrix transformations may introduce small rounding
                // errors which add up over time and could destabilize the etch.
                Direction.Normalize();
                Up.Normalize();

                // Re-calculate Right
                right = Vector3.Cross(Direction, Up);

                // The same instability may cause the 3 orientation vectors may
                // also diverge. Either the Up or Direction vector needs to be
                // re-computed with a cross product to ensure orthagonality
                Up = Vector3.Cross(Right, Direction);

                // Determine thrust amount from input
                float thrustAmount = 0.0f;
                if (thrust)
                    thrustAmount = 1.0f;

                // Calculate force from thrust amount
                float force = thrustAmount * ThrustForce;


                // Apply acceleration
                float acceleration = force / Mass;
                Velocity.Z += acceleration * elapsed;

                if (jumpOn)
                {
                    if (Position.Y <= MinimumAltitude)
                    {
                        Velocity.Y = 0.0f;
                    }
                    else
                    {
                        Velocity.Y += -2500 * elapsed;
                    }

                    if (jump)
                    {
                        if (Position.Y <= MinimumAltitude)
                        {
                            Velocity.Y = 3000.0f;
                        }
                    }

                    // Prevent etch from flying under the ground
                    Position.Y += Velocity.Y * elapsed;
                }
                else
                {
                    if (moveUp && Position.Y <= MaximumAltitude)
                    {
                        Position.Y += 1500 * elapsed;
                    }
                    else if (moveDown && Position.Y >= MinimumAltitude)
                    {
                        Position.Y -= 1500 * elapsed;
                    }
                }

                // Apply psuedo drag
                Velocity.Z *= DragFactor;

                // Apply velocity
                Position.Z -= Velocity.Z * elapsed;

                world.Translation = Position;
                position = Position;
            }
        }


        /// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are taken from the camera object.
        /// </summary>        
        public override void DrawModel(ChaseCamera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix[] bones = animationPlayer.GetSkinTransforms();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;

                    // Use the matrices provided by the chase camera
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }
    }
}
