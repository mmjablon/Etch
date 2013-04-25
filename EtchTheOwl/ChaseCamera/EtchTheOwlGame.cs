#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
#endregion

namespace EtchTheOwl
{
    /// <summary>
    /// Sample showing how to implement a simple chase camera.
    /// </summary>
    /// 

    public class EtchTheOwlGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ModelManager modelManager;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        SpriteFont titleFont;
        Texture2D fly;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        KeyboardState previousKeyboardState = new KeyboardState();
        GamePadState previousGamePadState = new GamePadState();

        //sound effect
        Song backgroundMusic;
        private SoundEffect beepUp;
        private SoundEffect beepDown;

        enum GameState { Start, Controls, InGame, Settings, End};
        GameState currentGameState;
        Stopwatch timer;
        bool singlePlayer;

        int mainMenuState;
        int numMenuStates = 3;
        int controlMenuState;
        int numControlStates = 3;
        int settingsMenuState;
        int numSettingsStates = 3;

        bool fullscreen;
        bool toggleFullscreen;

        #endregion

        #region Initialization

        public EtchTheOwlGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 600;
            fullscreen = false;
            graphics.IsFullScreen = fullscreen;

            currentGameState = GameState.Start;
            mainMenuState = 0;
            controlMenuState = 0;
            settingsMenuState = 0;
        }


        /// <summary>
        /// Initalize the game
        /// </summary>
        protected override void Initialize()
        {
            singlePlayer = true;
            modelManager = new ModelManager(this, graphics, singlePlayer, 1);
            modelManager.Enabled = false;
            Components.Add(modelManager);
            timer = new Stopwatch();
            base.Initialize();
        }


        /// <summary>
        /// Load graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Fonts\\gameFont");
            titleFont = Content.Load<SpriteFont>("Fonts\\titleFont");
            backgroundMusic = Content.Load<Song>("Audio\\Departure");
            MediaPlayer.Play(backgroundMusic);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.6f;
            fly = Content.Load<Texture2D>("Textures\\fly");
            beepUp = Content.Load<SoundEffect>("Audio\\vulture");
            beepDown = Content.Load<SoundEffect>("Audio\\vulture");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            switch (currentGameState)
            {
                case GameState.Start:
                    if ((currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                        || (currentGamePadState.Buttons.A == ButtonState.Pressed && previousGamePadState.Buttons.A == ButtonState.Released))
                    {
                        if (mainMenuState == 0)
                        {
                            singlePlayer = true;
                            currentGameState = GameState.Controls;
                        }
                        else if (mainMenuState == 1)
                        {
                            singlePlayer = false;
                            currentGameState = GameState.Controls;
                        }
                        else
                        {
                            currentGameState = GameState.Settings;
                        }
                    }
                    if ((currentKeyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down)) || (currentGamePadState.ThumbSticks.Left.Y >= 0 && previousGamePadState.ThumbSticks.Left.Y < 0 ))
                    {
                        if (mainMenuState < numMenuStates - 1)
                        {
                            mainMenuState++;
                            beepDown.Play();
                        }
                    }

                    if ((currentKeyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up)) || (currentGamePadState.ThumbSticks.Left.Y <= 0 && previousGamePadState.ThumbSticks.Left.Y > 0))
                    {
                        if (mainMenuState > 0)
                        {
                            mainMenuState--;
                            beepUp.Play();
                        }
                    }
                    break;


                case GameState.Controls:
                    if ((currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                        || (currentGamePadState.Buttons.A == ButtonState.Pressed && previousGamePadState.Buttons.A == ButtonState.Released))
                    {
                        if (controlMenuState == 0)
                        {
                            if (singlePlayer)
                            {
                                modelManager.Enabled = true;
                                currentGameState = GameState.InGame;
                                timer.Start();
                                modelManager.etch1.controller = Controls.Keys;
                            }
                            else
                            {
                                Components.Remove(modelManager);
                                modelManager = new ModelManager(this, graphics, singlePlayer, 1);
                                Components.Add(modelManager);
                                currentGameState = GameState.InGame;
                                timer.Start();
                                modelManager.etch1.controller = Controls.Keys;
                                modelManager.etch2.controller = Controls.ControllerOne;
                            }
                        }
                        else if (controlMenuState == 1)
                        {
                            if (singlePlayer)
                            {
                                modelManager.Enabled = true;
                                currentGameState = GameState.InGame;
                                timer.Start();
                                modelManager.etch1.controller = Controls.ControllerOne;
                            }
                            else
                            {
                                Components.Remove(modelManager);
                                singlePlayer = false;
                                modelManager = new ModelManager(this, graphics, singlePlayer, 1);
                                Components.Add(modelManager);
                                currentGameState = GameState.InGame;
                                timer.Start();
                                modelManager.etch1.controller = Controls.ControllerOne;
                                modelManager.etch2.controller = Controls.ControllerTwo;
                            }
                        }
                        else
                        {
                            //go  back
                            currentGameState = GameState.Start;
                            controlMenuState = 0;
                        }
                    }

                  if ((currentKeyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down)) || (currentGamePadState.ThumbSticks.Left.Y >= 0 && previousGamePadState.ThumbSticks.Left.Y < 0 ))
                    {
                        if (controlMenuState < numControlStates - 1)
                        {
                            controlMenuState++;
                            beepDown.Play();
                        }
                    }

                    if ((currentKeyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up)) || (currentGamePadState.ThumbSticks.Left.Y <= 0 && previousGamePadState.ThumbSticks.Left.Y > 0))
                    {
                        if (controlMenuState > 0)
                        {
                            controlMenuState--;
                            beepUp.Play();
                        }
                    }
                    break;


                case GameState.Settings:
                    if ((currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                        || (currentGamePadState.Buttons.A == ButtonState.Pressed && previousGamePadState.Buttons.A == ButtonState.Released))
                    {
                        if (settingsMenuState == 0)
                        {
                            fullscreen = !fullscreen;
                            toggleFullscreen = true;
                        }
                        else if (settingsMenuState == 1)
                        {
                            
                        }
                        else
                        {
                            //go  back
                            currentGameState = GameState.Start;
                            settingsMenuState = 0;
                        }
                    }

                  if ((currentKeyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down)) || (currentGamePadState.ThumbSticks.Left.Y >= 0 && previousGamePadState.ThumbSticks.Left.Y < 0 ))
                    {
                        if (settingsMenuState < numSettingsStates - 1)
                        {
                            settingsMenuState++;
                            beepDown.Play();
                        }
                    }

                  if ((currentKeyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up)) || (currentGamePadState.ThumbSticks.Left.Y <= 0 && previousGamePadState.ThumbSticks.Left.Y > 0))
                  {
                      if (settingsMenuState > 0)
                      {
                          settingsMenuState--;
                          beepUp.Play();
                      }
                  }
                    break;

                case GameState.InGame:

                    break;
                case GameState.End:

                    break;
            }

            // Exit when the Escape key or Back button is pressed
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Draws the ship and ground.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            if (toggleFullscreen)
            {
                graphics.ToggleFullScreen();
                toggleFullscreen = false;
            }
            device.Clear(Color.Black);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);

            DrawOverlayText();
        }

        /// <summary>
        /// Displays an overlay showing what the controls are,
        /// and which settings are currently selected.
        /// </summary>
        private void DrawOverlayText()
        {
            string title;
            string stringOne;
            string stringTwo;
            string stringThree;

            SpriteFont menuFont = spriteFont;
            int startHeight = 100;
            Color stringOneColor = Color.White;
            Color stringTwoColor = Color.White;
            Color stringThreeColor = Color.White;

            switch (currentGameState)
            {

                case GameState.Start:
                    spriteBatch.Begin();

                    title = "Etch The Owl";
                    stringOne = "One-Player";
                    stringTwo = "Two-Player";
                    stringThree = "Settings";
                    menuFont = spriteFont;
                    startHeight = 100;

                    // Draw the string twice to create a drop shadow, first colored black
                    // and offset one pixel to the bottom right, then again in white at the
                    // intended position. This makes text easier to read over the background.
                    spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2) - 3,
                        startHeight-3), Color.Black);
                    spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2),
                        startHeight), Color.White);

                    if (mainMenuState == 0)
                    {
                        stringOneColor = Color.Green;
                    }
                    else if (mainMenuState == 1)
                    {
                        stringTwoColor = Color.Green;
                    }
                    else
                    {
                        stringThreeColor = Color.Green;
                    }
                    spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2) - 2,
                        startHeight + titleFont.MeasureString(title).Y - 2), Color.Black);
                    spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2),
                        startHeight + titleFont.MeasureString(title).Y), stringOneColor);

                    spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2) - 2,
                        startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                    spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2),
                        startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y), stringTwoColor);

                    spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2) - 2,
                        startHeight + titleFont.MeasureString(title).Y + 2 *  menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                    spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2),
                        startHeight + titleFont.MeasureString(title).Y + 2* menuFont.MeasureString(stringOne).Y), stringThreeColor);

                    spriteBatch.End();
                    break;


                case GameState.Controls:
                    spriteBatch.Begin();

                    if (singlePlayer)
                    {

                        title = "Controls";
                        stringOne = "Keyboard";
                        stringTwo = "Controller";
                        stringThree = "Back";
                        menuFont = spriteFont;
                        startHeight = 100;

                        // Draw the string twice to create a drop shadow, first colored black
                        // and offset one pixel to the bottom right, then again in white at the
                        // intended position. This makes text easier to read over the background.
                        spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2) - 3,
                            startHeight - 3), Color.Black);
                        spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2),
                            startHeight), Color.White);

                        if (controlMenuState == 0)
                        {
                            stringOneColor = Color.Green;
                        }
                        else if (controlMenuState == 1)
                        {
                            stringTwoColor = Color.Green;
                        }
                        else
                        {
                            stringThreeColor = Color.Green;
                        }
                        spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2) - 2,
                            startHeight + titleFont.MeasureString(title).Y - 2), Color.Black);
                        spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2),
                            startHeight + titleFont.MeasureString(title).Y), stringOneColor);

                        spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2) - 2,
                            startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                        spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2),
                            startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y), stringTwoColor);

                        spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2) - 2,
                            startHeight + titleFont.MeasureString(title).Y + 2 * menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                        spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2),
                            startHeight + titleFont.MeasureString(title).Y + 2 * menuFont.MeasureString(stringOne).Y), stringThreeColor);
                    }
                    else
                    {
                        title = "Controls";
                        stringOne = "Player1: Keyboard\nPlayer2: Controller\n";
                        stringTwo = "Player1: Controller\nPlayer2: Controller\n";
                        stringThree = "Back";
                        menuFont = spriteFont;
                        startHeight = 100;

                        // Draw the string twice to create a drop shadow, first colored black
                        // and offset one pixel to the bottom right, then again in white at the
                        // intended position. This makes text easier to read over the background.
                        spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2) - 3,
                            startHeight - 3), Color.Black);
                        spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2),
                            startHeight), Color.White);

                        if (controlMenuState == 0)
                        {
                            stringOneColor = Color.Green;
                        }
                        else if (controlMenuState == 1)
                        {
                            stringTwoColor = Color.Green;
                        }
                        else
                        {
                            stringThreeColor = Color.Green;
                        }
                        spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2) - 2,
                            startHeight + titleFont.MeasureString(title).Y - 2), Color.Black);
                        spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2),
                            startHeight + titleFont.MeasureString(title).Y), stringOneColor);

                        spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2) - 2,
                            startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                        spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2),
                            startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y), stringTwoColor);

                        spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2) - 2,
                            startHeight + titleFont.MeasureString(title).Y + 2 * menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                        spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2),
                            startHeight + titleFont.MeasureString(title).Y + 2 * menuFont.MeasureString(stringOne).Y), stringThreeColor);
                    }

                    spriteBatch.End();
                    break;

                case GameState.InGame:
                    spriteBatch.Begin();

                    TimeSpan ts = timer.Elapsed;

                    // Format and display the TimeSpan value. 
                    string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    spriteBatch.DrawString(spriteFont, elapsedTime, new Vector2(8, 8), Color.Black);
                    spriteBatch.DrawString(spriteFont, elapsedTime, new Vector2(10, 10), Color.White);
                    float flyScale = 0.13f;
                    float spriteWidth = fly.Width * flyScale;
                    Vector2 flyPos = new Vector2(GraphicsDevice.Viewport.Width - spriteWidth, 0);
                    
                    spriteBatch.Draw(fly, flyPos, null, Color.White, 0f, Vector2.Zero, flyScale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    break;
                case GameState.Settings:
                    spriteBatch.Begin();

                     title = "Controls";

                        if (fullscreen)
                            stringOne = "Fullscreen: On";
                        else
                            stringOne = "Fullscreen: Off";

                        stringTwo = "--";
                        stringThree = "Back";
                        menuFont = spriteFont;
                        startHeight = 100;

                    // Draw the string twice to create a drop shadow, first colored black
                    // and offset one pixel to the bottom right, then again in white at the
                    // intended position. This makes text easier to read over the background.
                    spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2) - 3,
                        startHeight-3), Color.Black);
                    spriteBatch.DrawString(titleFont, title, new Vector2((GraphicsDevice.Viewport.Width / 2) - (titleFont.MeasureString(title).X / 2),
                        startHeight), Color.White);

                    if (settingsMenuState == 0)
                    {
                        stringOneColor = Color.Green;
                    }
                    else if (settingsMenuState == 1)
                    {
                        stringTwoColor = Color.Green;
                    }
                    else
                    {
                        stringThreeColor = Color.Green;
                    }
                    spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2) - 2,
                        startHeight + titleFont.MeasureString(title).Y - 2), Color.Black);
                    spriteBatch.DrawString(menuFont, stringOne, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringOne).X / 2),
                        startHeight + titleFont.MeasureString(title).Y), stringOneColor);

                    spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2) - 2,
                        startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                    spriteBatch.DrawString(menuFont, stringTwo, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringTwo).X / 2),
                        startHeight + titleFont.MeasureString(title).Y + menuFont.MeasureString(stringOne).Y), stringTwoColor);

                    spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2) - 2,
                        startHeight + titleFont.MeasureString(title).Y + 2 *  menuFont.MeasureString(stringOne).Y - 2), Color.Black);
                    spriteBatch.DrawString(menuFont, stringThree, new Vector2((GraphicsDevice.Viewport.Width / 2) - (menuFont.MeasureString(stringThree).X / 2),
                        startHeight + titleFont.MeasureString(title).Y + 2* menuFont.MeasureString(stringOne).Y), stringThreeColor);

                    spriteBatch.End();
                    break;
                case GameState.End:

                    break;
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (EtchTheOwlGame game = new EtchTheOwlGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
