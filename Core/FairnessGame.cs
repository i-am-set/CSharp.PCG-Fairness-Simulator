using Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Visualization;

public class FairnessGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private bool _isHeadless;

    // Simulation System
    private SimulationController _simController;
    private Dashboard _dashboard;
    private SpriteFont _mainFont;

    public FairnessGame(bool headless = false)
    {
        _isHeadless = headless;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        if (_isHeadless)
        {
            _graphics.PreferredBackBufferWidth = 1;
            _graphics.PreferredBackBufferHeight = 1;
        }
        else
        {
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _simController = new SimulationController();
        _dashboard = new Dashboard(_simController);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        if (!_isHeadless)
        {
            try
            {
                _mainFont = Content.Load<SpriteFont>("Fonts/Px437_IBM_BIOS");
            }
            catch
            {
                throw new System.Exception("Please create a SpriteFont named 'File' in the Content Pipeline.");
            }
            _dashboard.LoadContent(_mainFont, GraphicsDevice);
        }

        if (_isHeadless)
        {
            Console.WriteLine("Headless Mode: Starting Simulation...");
            _simController.StartSimulation();
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (!_isHeadless)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var kState = Keyboard.GetState();

            if (kState.IsKeyDown(Keys.Space) && !_simController.IsRunning)
            {
                _simController.StartSimulation();
            }

            if (kState.IsKeyDown(Keys.C) && _simController.IsRunning)
            {
                _simController.Cancel();
            }
        }
        else
        {
            if (!_simController.IsRunning && _simController.Progress >= 1.0)
            {
                Console.WriteLine("Headless Mode: Simulation Complete. Exiting.");
                Exit();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_isHeadless)
        {
            GraphicsDevice.Clear(Color.Black);
            return;
        }

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        _dashboard.Draw(_spriteBatch, GraphicsDevice.Viewport.Bounds);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}