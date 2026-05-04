using System;
using System.Drawing;
using System.Numerics;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using Gum.DataTypes;

namespace Final;

public class SideMenu
{
    // ================== fields ==================
    // --- menu visuals --- 
    // sound 
    //     check box for button sound to be on or off
    //     slider to control button sound 
    //     check box for background sound to be on or off 
    //     slider to control background sound 
    // radio button to change light or dark mode 
    
    private const float MinMenuHeight = 120f;
    private const float MaxMenuHeight = 3000f;
    
    // --- menu button ---
    private Button _menuButton;
    private float _menuButtonSize = 40;
    
    // --- menu background ---
    private float _menuWidth = 200f; 
    private float _menuHeight = 200f;  
    
    // --- resizing ---
    private ColoredRectangleRuntime _resizeHandle;
    private int _resizingHandleSize = 12;     
    
    private bool _isDragging = false;
    private Vector2 _dragStartMouse;
    private float _dragStartWidth;
    private float _dragStartHeight;
    private const float MinMenuWidth = 150f;
    private const float MaxMenuWidth = 5000f; 
    
    // --- container ---
    private ContainerRuntime _menuContainer;
    
    // --- other ---
    private float _padingFromEdge = 10;
    
    // --- button sound checkbox ---
    public static bool ButtonSoundEnabled = true;
    private CheckBox _soundCheckBox;
    
    // --- button sound volume slider ---
    public static float ButtonSoundVolume = 1f;
    
    // --- dark mode --
    public static bool IsDarkMode = true;

    
    // ================== Build plot ==================
    public void BuildMenu(float screenWidth, float screenHeight)
    {
        _menuContainer = new ContainerRuntime();
        _menuContainer.Width = _menuWidth + (_resizingHandleSize / 2);
        _menuContainer.Height = _menuHeight + (_resizingHandleSize / 2);
        _menuContainer.X = screenWidth - _padingFromEdge - _menuContainer.Width;
        _menuContainer.Y = _padingFromEdge + _menuButtonSize + _padingFromEdge;
        _menuContainer.AddToRoot();
        _menuContainer.Visible = false;
        
        RebuildMenuContents();
        BuildMenuToggle(screenWidth);
        
        //_menuButton.AddToRoot();
    }
    
    // ================== Rebuilding plot ==================
    private void RebuildMenuContents()
    {
        // --- menu container ---
        _menuContainer.Children.Clear();    // clear existing elements 
        _menuContainer.Width = _menuWidth + (_resizingHandleSize / 2);
        _menuContainer.Height = _menuHeight + (_resizingHandleSize / 2);
        
        // --- menu background ---
        var menuBackground = new ColoredRectangleRuntime();
        menuBackground.Width = _menuWidth;
        menuBackground.Height = _menuHeight;
        menuBackground.X = _menuContainer.Width - menuBackground.Width;
        menuBackground.Y = _menuContainer.Height - menuBackground.Height - (_resizingHandleSize /2);
        menuBackground.Color = new Microsoft.Xna.Framework.Color(255, 225, 168);
        _menuContainer.Children.Add(menuBackground);
        
        if (_soundCheckBox == null)
        {
            _soundCheckBox = new CheckBox();
            _soundCheckBox.IsChecked = ButtonSoundEnabled;
            _soundCheckBox.Checked += (s, e) => ButtonSoundEnabled = true;
            _soundCheckBox.Unchecked += (s, e) => ButtonSoundEnabled = false;
        }
        _soundCheckBox.Text = "Button Sound";
        _soundCheckBox.X = 10;
        _soundCheckBox.Y = 10;
        menuBackground.Children.Add(_soundCheckBox.Visual);
        
        /*
        // labels
        var buttonSoundLable = new TextRuntime();
        buttonSoundLable.Text = "Button Sound";
        buttonSoundLable.X = _menuWidth / 2f;
        buttonSoundLable.Y = _menuHeight / 2f;
        buttonSoundLable.Color = Microsoft.Xna.Framework.Color.Black;
        _menuContainer.Children.Add(buttonSoundLable);
        */
        
        // --- resizing ---
        _resizeHandle = new ColoredRectangleRuntime();
        _resizeHandle.Width = _resizingHandleSize;
        _resizeHandle.Height = _resizingHandleSize;
        _resizeHandle.Color = new Microsoft.Xna.Framework.Color(226, 109, 92);
        _resizeHandle.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _menuContainer.Children.Add(_resizeHandle);
        
        // --- button sound volume slider ---
        var volumeLabel = new TextRuntime();
        volumeLabel.Text = "Button Volume";
        volumeLabel.X = 10;
        volumeLabel.Y = 40;
        volumeLabel.Color = Microsoft.Xna.Framework.Color.Black;
        menuBackground.Children.Add(volumeLabel);

        var volumeSlider = new Slider();
        volumeSlider.Minimum = 0;
        volumeSlider.Maximum = 1;
        volumeSlider.Value = ButtonSoundVolume;
        volumeSlider.X = 10;
        volumeSlider.Y = 60;
        volumeSlider.Width = _menuWidth - 20;
        volumeSlider.ValueChanged += (s, e) => ButtonSoundVolume = (float)volumeSlider.Value;
        menuBackground.Children.Add(volumeSlider.Visual);
        
        var modeLabel = new TextRuntime();
        modeLabel.Text = "Theme";
        modeLabel.X = 10;
        modeLabel.Y = 100;
        modeLabel.Color = Microsoft.Xna.Framework.Color.Black;
        menuBackground.Children.Add(modeLabel);

        var darkButton = new RadioButton();
        darkButton.Text = "Dark";
        darkButton.X = 10;
        darkButton.Y = 120;
        darkButton.IsChecked = IsDarkMode;
        darkButton.Checked += (_, _) =>
        {
            IsDarkMode = true;
            PlayClickSound();
        };
        menuBackground.Children.Add(darkButton.Visual);

        var lightButton = new RadioButton();
        lightButton.Text = "Light";
        lightButton.X = 10;
        lightButton.Y = 145;
        lightButton.IsChecked = !IsDarkMode;
        lightButton.Checked += (_, _) =>
        {
            IsDarkMode = false;
            PlayClickSound();
        };
        menuBackground.Children.Add(lightButton.Visual);
    }
    
    // ================== Build Toggle button  ==================
    public void BuildMenuToggle(float screenWidth)
    {
        if (_menuButton != null) return;

        _menuButton = new Button();
        _menuButton.X = screenWidth - _padingFromEdge - _menuButtonSize;
        _menuButton.Y = _padingFromEdge + 50;
        _menuButton.Click += (s, e) =>
        {
            MenuToggle();
            //Game1._buttonClickSound.Play();
            if (SideMenu.ButtonSoundEnabled)
                Game1._buttonClickSound.Play(ButtonSoundVolume, 0f, 0f);
        };
    
        UIHelper.SetButtonTexture(_menuButton, Game1.menuButtonTexture, _menuButtonSize, _menuButtonSize);
    
        //_menuButton.AddToRoot();
        
        _menuContainer.Visible = false;
    }

    // ================== Updates as when resized or protein changes ==================
    public void Update()
    {
        if (!_menuContainer.Visible) return;

        var mouse = Mouse.GetState();
        var mousePos = new Vector2(mouse.X, mouse.Y);
        
        float handleScreenX = _menuContainer.X;
        float handleScreenY = _menuContainer.Y + _menuContainer.Height - _resizingHandleSize;

        var handleBounds = new Rectangle((int)handleScreenX, 
            (int)handleScreenY, 
            _resizingHandleSize, 
            _resizingHandleSize);

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            if (!_isDragging && handleBounds.Contains(mouse.X, mouse.Y))
            {
                _isDragging = true;
                _dragStartMouse = mousePos;
                _dragStartWidth = _menuWidth;
                _dragStartHeight = _menuHeight;
            }

            if (_isDragging)
            {
                float deltaX = _dragStartMouse.X - mousePos.X; // left = bigger
                float deltaY = mousePos.Y - _dragStartMouse.Y; // down = bigger

                float newWidth = Math.Clamp(
                    _dragStartWidth + deltaX,
                    MinMenuWidth,
                    MaxMenuWidth);

                float newHeight = Math.Clamp(
                    _dragStartHeight + deltaY,
                    MinMenuHeight,
                    MaxMenuHeight);

                if (Math.Abs(newWidth - _menuWidth) > 0.5f ||
                    Math.Abs(newHeight - _menuHeight) > 0.5f)
                {
                    float rightEdge = _menuContainer.X + _menuContainer.Width;
                    float topEdge = _menuContainer.Y;

                    _menuWidth = newWidth;
                    _menuHeight = newHeight;

                    RebuildMenuContents();

                    // keep top-right fixed
                    _menuContainer.X = rightEdge - _menuContainer.Width;
                    _menuContainer.Y = topEdge;
                }
            }
        }
        else
        {
            _isDragging = false;
        }
    }

    // ================== Allows Toggle ==================
    public void MenuToggle()
    {
        _menuContainer.Visible = !_menuContainer.Visible;
    }
    
    // ================== anchoring button to top right ==================
    public void OnWindowResize(float screenWidth, float screenHeight)
    {
        if (_menuContainer == null || _menuButton == null) return;
    
        // reposition container to stay in upper right
        _menuContainer.X = screenWidth - _padingFromEdge - _menuContainer.Width;
        _menuContainer.Y = _padingFromEdge + _menuButtonSize + _padingFromEdge + 50;
    
        // reposition button to match
        _menuButton.X = screenWidth - _padingFromEdge - _menuButtonSize;
        _menuButton.Y = _padingFromEdge + 50;
    }
    
    private void PlayClickSound()
    {
        if (ButtonSoundEnabled)
            Game1._buttonClickSound.Play(ButtonSoundVolume, 0f, 0f);
    }

}