using System;
using Diese.Collections;
using Niddle;
using Fingear;
using Fingear.Controls;
using Fingear.MonoGame;
using Glyph.Core.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IInputStates = Fingear.MonoGame.IInputStates;

namespace Glyph.Engine
{
    public class GlyphGame : Game, IGlyphClient
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private Vector2 _lastWindowSize;
        private bool _resizing;
        private IControl _toggleFullscreen;
        public GlyphEngine Engine { get; }
        public Resolution Resolution { get; }
        public virtual bool IsFocus => IsActive && System.Windows.Forms.Form.ActiveForm?.Handle == Window.Handle;
        IInputStates IInputClient.States { get; } = new InputStates();
        Matrix IDrawClient.ResolutionMatrix => Resolution.TransformationMatrix;
        RenderTarget2D IDrawClient.DefaultRenderTarget => null;
        GraphicsDevice IDrawClient.GraphicsDevice => GraphicsDevice;

        public GlyphGame(Action<IDependencyRegistry> dependencyConfigurator = null)
        {
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;
            IsMouseVisible = false;

            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferMultiSampling = true
            };
            _graphicsDeviceManager.ApplyChanges();

            Resolution = new Resolution();
            _lastWindowSize = Resolution.WindowSize;

            Engine = new GlyphEngine(Content, dependencyConfigurator);
            Engine.Stopped += OnEngineStopped;
            Engine.FocusedClient = this;

            Engine.ControlManager.Plan(new ControlLayer("Window controls", ControlLayerTag.Debug)).RegisterMany(new []
            {
                _toggleFullscreen = new Control("Toogle fullscreen", InputSystem.Instance.Keyboard[Keys.F12])
            });
        }

        protected override void Initialize()
        {
            Engine.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            Engine.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            Engine.BeginUpdate(gameTime);

            base.Update(gameTime);

            Engine.HandleInput();

            if (!IsActive)
                return;
            
            if (_toggleFullscreen.IsActive())
                ToggleFullscreen();

            Engine.Update();
        }

        public void ToggleFullscreen()
        {
            if (_resizing)
                return;

            _resizing = true;

            if (_graphicsDeviceManager.IsFullScreen)
            {
                _graphicsDeviceManager.IsFullScreen = false;
                Window.IsBorderless = false;
                _graphicsDeviceManager.PreferredBackBufferWidth = (int)_lastWindowSize.X;
                _graphicsDeviceManager.PreferredBackBufferHeight = (int)_lastWindowSize.Y;
                _graphicsDeviceManager.ApplyChanges();
                Resolution.WindowSize = _lastWindowSize;
            }
            else
            {
                _lastWindowSize = Resolution.WindowSize;

                int maxWidth = 0;
                int maxHeight = 0;
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                    if (dm.Width >= maxWidth && dm.Height >= maxHeight
                        && dm.Width <= VirtualResolution.Size.X && dm.Height <= VirtualResolution.Size.Y)
                    {
                        maxWidth = dm.Width;
                        maxHeight = dm.Height;
                    }

                _graphicsDeviceManager.IsFullScreen = true;
                Window.IsBorderless = true;
                Window.Position = new Point(0, 0);
                _graphicsDeviceManager.PreferredBackBufferWidth = maxWidth;
                _graphicsDeviceManager.PreferredBackBufferHeight = maxHeight;
                _graphicsDeviceManager.ApplyChanges();
                Resolution.WindowSize = new Vector2(maxWidth, maxHeight);
            }

            _resizing = false;
        }

        protected override bool BeginDraw()
        {
            Engine.BeginDraw();
            return base.BeginDraw();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Engine.Draw(this);
        }

        protected override void EndDraw()
        {
            base.EndDraw();
            Engine.EndDraw();
        }

        protected override void BeginRun()
        {
            base.BeginRun();
            Engine.Start();
        }

        protected override void EndRun()
        {
            Engine.Stop();
            base.EndRun();
        }

        public void OnEngineStopped()
        {
            Exit();
        }

        private void OnClientSizeChanged(object sender, EventArgs args)
        {
            if (_resizing)
                return;

            _resizing = true;

            _graphicsDeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Size.X;
            _graphicsDeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Size.Y;
            _graphicsDeviceManager.ApplyChanges();
            Resolution.WindowSize = Window.ClientBounds.Size.ToVector2();

            _resizing = false;
        }
    }
}