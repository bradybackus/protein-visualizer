using System;
using System.Collections.Generic;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace Final;

public class ControlPanel
{
    private Panel _controlPanel;
    
    // Class-level fields so we can update their positions instead of rebuilding
    private ColoredRectangleRuntime _buttonPanel;
    private Button _menuButton;
    private Button _ramaButton;
    private Button _backboneButton;
    private Button _infoButton;
    private Button _resetButton;
    
    // callbacks to Game1
    private Action _onToggleBackbone;
    private Action _onResetModel;
    private Action _onToggleRamaPlot;
    private Action<string> _onFilterChanged;
    private Action _onToggleMenu;
    private Action _onResetCamera;
    private Action _onShowInfo;
    private Action<int> _onSwitchProtein;
    private FilterHelper _filterHelper;

    private float _buttonSize = 40f;
    private float _buttonPading = 10f;

    public ControlPanel(Action onToggleBackbone,
        Action onResetModel,
        Action onToggleRamaPlot,
        Action onToggleMenu,
        Action<string> onFilterChanged,
        Action onResetCamera,
        Action onShowInfo,
        Action<int> onSwitchProtein,
        FilterHelper filterHelper)
    {
        _onToggleBackbone = onToggleBackbone;
        _onResetModel = onResetModel;
        _onToggleRamaPlot = onToggleRamaPlot;
        _onToggleMenu = onToggleMenu;
        _onFilterChanged = onFilterChanged;
        _onResetCamera = onResetCamera;
        _onShowInfo = onShowInfo;
        _onSwitchProtein = onSwitchProtein;
        _filterHelper = filterHelper;
    }

    public void Build(float screenWidth, float screenHeight)
    {
        _controlPanel = new Panel();
        _controlPanel.AddToRoot();

        // filter box
        var filterBox = new ComboBox();
        filterBox.Text = "Filter";
        filterBox.Width = 110;
        filterBox.X = _buttonPading;
        filterBox.Y = _buttonPading;
        filterBox.Items.Add("No Filter");
        filterBox.Items.Add("Polar");
        filterBox.Items.Add("Nonpolar");
        filterBox.Items.Add("Acidic");
        filterBox.Items.Add("Basic");
        
        var filterButtonVisual = (ComboBoxVisual)filterBox.Visual;
        filterButtonVisual.BackgroundColor = new Color(226, 109, 92);
        filterButtonVisual.FocusedIndicatorColor = Color.White;
        filterBox.SelectionChanged += (_, _) =>
        {
            _onFilterChanged?.Invoke(filterBox.SelectedObject?.ToString() ?? "No Filter");
            _onResetModel.Invoke();
            String selection = filterBox.SelectedObject.ToString();
            _filterHelper.ApplyFilter(selection);
            PlayClickSound();
        };
        _controlPanel.AddChild(filterBox);

        // background behind buttons 
        _buttonPanel = new ColoredRectangleRuntime();
        _buttonPanel.Width = _buttonSize + _buttonPading * 2;
        _buttonPanel.Height = screenHeight; 
        _buttonPanel.X = screenWidth - _buttonPanel.Width;
        _buttonPanel.Y = 0;
        _buttonPanel.Color = new Color(43, 27, 29);
        _controlPanel.AddChild(_buttonPanel);
        
        // side menu button
        _menuButton = new Button();
        _menuButton.X = screenWidth - _buttonSize - _buttonPading;
        _menuButton.Y = _buttonPading;
        _menuButton.Width = _buttonSize;
        _menuButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(_menuButton, Game1.menuButtonTexture, _buttonSize, _buttonSize);
        _menuButton.Click += (_, _) =>
        {
            _onToggleMenu?.Invoke();
            PlayClickSound();
        }; 
        _controlPanel.AddChild(_menuButton);
        
        // plot button
        _ramaButton = new Button();
        _ramaButton.X = screenWidth - _buttonSize - _buttonPading;
        _ramaButton.Y = _menuButton.Y + _buttonSize + _buttonPading; 
        _ramaButton.Width = _buttonSize;
        _ramaButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(_ramaButton, Game1.plotButtonTexture, _buttonSize, _buttonSize);
        _ramaButton.Click += (_, _) =>
        {
            _onToggleRamaPlot?.Invoke();
            PlayClickSound();
        }; 
        _controlPanel.AddChild(_ramaButton);
        
        // backbone button
        _backboneButton = new Button();
        _backboneButton.X = screenWidth - _buttonSize - _buttonPading;
        _backboneButton.Y = _ramaButton.Y + _buttonSize + _buttonPading;
        _backboneButton.Width = _buttonSize;
        _backboneButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(_backboneButton, Game1.backboneButtonTexture, _buttonSize, _buttonSize);
        _backboneButton.Click += (_, _) =>
        {
            _onToggleBackbone?.Invoke();
            PlayClickSound();
        };
        _controlPanel.AddChild(_backboneButton);
        
        // info button
        _infoButton = new Button();
        _infoButton.X = screenWidth - _buttonSize - _buttonPading;  
        _infoButton.Y = _backboneButton.Y + _buttonSize + _buttonPading;
        _infoButton.Width = _buttonSize;   
        _infoButton.Height = _buttonSize;
        _infoButton.Click += (s, e) =>
        {
            _onShowInfo?.Invoke();
            PlayClickSound();
        }; 
        UIHelper.SetButtonTexture(_infoButton, Game1.infoButtonTexture, _buttonSize, _buttonSize);
        _controlPanel.AddChild(_infoButton);  

        // reset button
        _resetButton = new Button();
        _resetButton.X = screenWidth - _buttonSize - _buttonPading;
        _resetButton.Y = screenHeight - _buttonSize - _buttonPading;
        _resetButton.Width = _buttonSize;
        _resetButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(_resetButton, Game1.resetButtonTexture, _buttonSize, _buttonSize);
        _resetButton.Click += (_, _) =>
        {
            _onResetModel?.Invoke();
            PlayClickSound();
        }; 
        _controlPanel.AddChild(_resetButton);
    }

    public void OnWindowResize(float screenWidth, float screenHeight)
    {
        if (_controlPanel == null) return;

        _buttonPanel.Height = screenHeight;
        _buttonPanel.X = screenWidth - _buttonPanel.Width;

        float btnX = screenWidth - _buttonSize - _buttonPading;

        _menuButton.X = btnX;
        _ramaButton.X = btnX;
        _backboneButton.X = btnX;
        _infoButton.X = btnX;

        _resetButton.X = btnX;
        _resetButton.Y = screenHeight - _buttonSize - _buttonPading;
    }
    
    private void PlayClickSound()
    {
        if (SideMenu.ButtonSoundEnabled)
            Game1._buttonClickSound.Play(SideMenu.ButtonSoundVolume, 0f, 0f);
    }
}