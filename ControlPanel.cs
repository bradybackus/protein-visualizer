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
            //_filterHelper.ResetFilter();
            _onResetModel.Invoke();
            String selection = filterBox.SelectedObject.ToString();
            _filterHelper.ApplyFilter(selection);
            PlayClickSound();
        };
        _controlPanel.AddChild(filterBox);

        // background behind buttons 
        var buttonPanel = new ColoredRectangleRuntime();
        buttonPanel.Width = _buttonSize + _buttonPading * 2;
        buttonPanel.Height = screenHeight; 
        buttonPanel.X = screenWidth - buttonPanel.Width;
        buttonPanel.Y = 0;
        buttonPanel.Color = new Color(43, 27, 29);
        _controlPanel.AddChild(buttonPanel);
        
        // side menu button
        var menuButton = new Button();
        menuButton.X = screenWidth - _buttonSize - _buttonPading;;
        menuButton.Y = _buttonPading;
        menuButton.Width = _buttonSize;
        menuButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(menuButton, Game1.menuButtonTexture, _buttonSize, _buttonSize);
        menuButton.Click += (_, _) =>
        {
            _onToggleMenu?.Invoke();
            PlayClickSound();
        }; 
        _controlPanel.AddChild(menuButton);
        
        // plot button
        var ramaButton = new Button();
        ramaButton.X = screenWidth - _buttonSize - _buttonPading;
        ramaButton.Y = menuButton.Y + _buttonSize + _buttonPading; 
        ramaButton.Width = _buttonSize;
        ramaButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(ramaButton, Game1.plotButtonTexture, _buttonSize, _buttonSize);
        ramaButton.Click += (_, _) =>
        {
            _onToggleRamaPlot?.Invoke();
            PlayClickSound();
        }; 
        _controlPanel.AddChild(ramaButton);
        
        // backbone button
        var backboneButton = new Button();
        backboneButton.X = screenWidth - _buttonSize - _buttonPading;
        backboneButton.Y = ramaButton.Y + _buttonSize + _buttonPading;
        backboneButton.Width = _buttonSize;
        backboneButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(backboneButton, Game1.backboneButtonTexture, _buttonSize, _buttonSize);
        backboneButton.Click += (_, _) =>
        {
            _onToggleBackbone?.Invoke();
            PlayClickSound();
        };
        _controlPanel.AddChild(backboneButton);
        
        // info button
        var infoButton = new Button();
        infoButton.X = screenWidth - _buttonSize - _buttonPading;  
        infoButton.Y = backboneButton.Y + _buttonSize + _buttonPading;
        infoButton.Width = _buttonSize;   
        infoButton.Height = _buttonSize;
        infoButton.Click += (s, e) =>
        {
            _onShowInfo?.Invoke();
            PlayClickSound();
        }; 
        UIHelper.SetButtonTexture(infoButton, Game1.infoButtonTexture, _buttonSize, _buttonSize);
        _controlPanel.AddChild(infoButton);  

        // reset button
        var resetButton = new Button();
        resetButton.X = screenWidth - _buttonSize - _buttonPading;
        resetButton.Y = screenHeight - _buttonSize - _buttonPading;
        resetButton.Width = _buttonSize;
        resetButton.Height = _buttonSize;
        UIHelper.SetButtonTexture(resetButton, Game1.resetButtonTexture, _buttonSize, _buttonSize);
        resetButton.Click += (_, _) =>
        {
            _onResetModel?.Invoke();
            PlayClickSound();
        }; 
        _controlPanel.AddChild(resetButton);
        
        // radio buttons
    //     var stackPanel = new StackPanel();
    //     stackPanel.X = _buttonPading;
    //     stackPanel.Y = screenHeight - 40;
    //     var stackPanelVisual = stackPanel.Visual;
    //     stackPanelVisual.ChildrenLayout = Gum.Managers.ChildrenLayout.LeftToRightStack;
    //     _controlPanel.AddChild(stackPanel);
    //
    //     var proteinOneButton = new RadioButton();
    //     proteinOneButton.Text = "alphaB";
    //     proteinOneButton.Checked += (_, _) => 
    //     {
    //         Game1.CurrentProtein = 0;
    //         foreach (var m in Game1.SharedModels) m.Shown = false;
    //         Game1.SharedModels[0].Shown = true;
    //         _onResetCamera?.Invoke(); 
    //         PlayClickSound(); 
    //     };
    //     stackPanel.AddChild(proteinOneButton); 
    //
    //     var proteinTwoButton = new RadioButton();
    //     proteinTwoButton.Text = "GFP";
    //     proteinTwoButton.Checked += (_, _) =>
    //     {
    //         Game1.CurrentProtein = 2;
    //         foreach (var m in Game1.SharedModels) m.Shown = false;
    //         Game1.SharedModels[2].Shown = true;
    //         _onResetCamera?.Invoke(); 
    //         PlayClickSound(); 
    //     };    
    //     stackPanel.AddChild(proteinTwoButton); 
    //
    //     var proteinThreeButton = new RadioButton();
    //     proteinThreeButton.Text = "alpha1";
    //     proteinThreeButton.Checked += (_, _) =>         
    //     {
    //         Game1.CurrentProtein = 1;
    //         foreach (var m in Game1.SharedModels) m.Shown = false;
    //         Game1.SharedModels[1].Shown = true;
    //         _onResetCamera?.Invoke(); 
    //         PlayClickSound(); 
    //     }; 
    //     stackPanel.AddChild(proteinThreeButton); 
    //     
    //     var proteinFourButton = new RadioButton();
    //     proteinFourButton.Text = "9MAR";
    //     proteinFourButton.Checked += (_, _) =>        
    //     {
    //         Game1.CurrentProtein = 3;
    //         foreach (var m in Game1.SharedModels) m.Shown = false;
    //         Game1.SharedModels[3].Shown = true;
    //         _onResetCamera?.Invoke(); 
    //         PlayClickSound(); 
    //     }; 
    //     stackPanel.AddChild(proteinFourButton);
    //     
    }

    public void OnWindowResize(float screenWidth, float screenHeight)
    {
        _controlPanel.RemoveFromRoot();
        Build(screenWidth, screenHeight);
    }
    
    private void PlayClickSound()
    {
        if (SideMenu.ButtonSoundEnabled)
            Game1._buttonClickSound.Play(SideMenu.ButtonSoundVolume, 0f, 0f);
    }
}