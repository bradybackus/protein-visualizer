using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace Final;

public class RamachandranPlot
{
    // ================== fields ==================
    // --- loading phi and psi ---
    private List<(float phi, float psi, int residueId)> _angles;
    
    // --- graph visuals --- 
    private float _outerPlotWidth = 320; // part of graph with axis labels
    private float _outerPlotHeight;
    
    private float _innerPlotSize;        // part with data 
    
    private float _dotSize = 4f;
    private float _innerPlotBorderThickness = 1f;
    private float _axisThickness = 1f;
    
    // --- resizing ---
    private ColoredRectangleRuntime _resizeHandle;
    private int _resizingHandleSize = 12;   // same as resizing handle for menu
    
    private bool _isDragging = false;
    private Vector2 _dragStartMouse;
    private float _dragStartWidth;
    private const float MinPlotWidth = 150f;
    private const float MaxPlotWidth = 2000f;
    
    // --- button ---
    private Button _plotButton;
    private float _plotButtonSize = 40f; 
    
    // --- other ---
    private ContainerRuntime _container;
    private float _padding = 10; 
    
    // ================== constructor ==================
    public RamachandranPlot(List<(float phi, float psi, int residueID)> angles)
    {
        // --- loading phi and psi angles and plot height ---
        _angles = angles;
        _outerPlotHeight = _outerPlotWidth * 0.9503f;
    }

    // ================== Build plot ==================
    public void BuildPlot(float screenWidth, float screenHeight, float plotButtonY)
    {
        _container = new ContainerRuntime();
        _container.Width = _outerPlotWidth + (_resizingHandleSize / 2);
        _container.Height = _outerPlotHeight + (_resizingHandleSize / 2);
        _container.X = screenWidth - _padding - _container.Width;
        //_container.Y = _padding + 40f + _padding + _plotButtonSize + _padding; // + height of menu button 
        _container.Y = plotButtonY;
        _container.AddToRoot();
        _container.Visible = false;
        
        RebuildPlotContents();
        //BuildToggle();
        //BuildToggle(_padding + 40f + _padding + _plotButtonSize + _padding); // below menu button
        BuildToggle(plotButtonY);
    }
    
    // ================== Rebuilding plot ==================
    // rebuilds plot at needed (like when it is resized, or the protein changes) 
    private void RebuildPlotContents()
    {
        _container.Children.Clear();    // clear existing elements 
        _container.Width = _outerPlotWidth + (_resizingHandleSize / 2);
        _container.Height = _outerPlotHeight + (_resizingHandleSize / 2);
                                             //+ _padding + _plotButtonSize;
        
        // --- graph visuals --- 
        // outer plot 
        var outerPlot = new ColoredRectangleRuntime();
        outerPlot.Width = _outerPlotWidth;
        outerPlot.Height = _outerPlotHeight;
        outerPlot.X = _container.Width - outerPlot.Width;
        //outerPlot.Y = _container.Height - outerPlot.Height - (_resizingHandleSize /2);
        outerPlot.Y = 0;
        outerPlot.Color = Microsoft.Xna.Framework.Color.White;
        _container.Children.Add(outerPlot);
        
        // inner plot border
        _innerPlotSize = _outerPlotWidth * 0.6635f;
        
        var _innerPlotBorder = new ColoredRectangleRuntime();
        _innerPlotBorder.Width = _innerPlotSize;
        _innerPlotBorder.Height = _innerPlotSize;
        _innerPlotBorder.X = (_outerPlotWidth * 0.2623f);
        _innerPlotBorder.Y = _outerPlotHeight * 0.0780f;
        _innerPlotBorder.Color = Microsoft.Xna.Framework.Color.Black;
        outerPlot.Children.Add(_innerPlotBorder);
        
        // inner plot 
        var innerPlot = new ColoredRectangleRuntime();
        innerPlot.Width = _innerPlotSize - 2 * _innerPlotBorderThickness;
        innerPlot.Height = _innerPlotSize - 2 * _innerPlotBorderThickness;
        innerPlot.Color = Microsoft.Xna.Framework.Color.White;
        innerPlot.Anchor(Gum.Wireframe.Anchor.Center);
        _innerPlotBorder.Children.Add(innerPlot);
        
        // vertical center line 
        var vLine = new ColoredRectangleRuntime();
        vLine.Width = _axisThickness;
        vLine.Height = _innerPlotSize - (_innerPlotBorderThickness * 2);
        vLine.Color = Microsoft.Xna.Framework.Color.Gray;
        vLine.Anchor(Gum.Wireframe.Anchor.Center);
        _innerPlotBorder.Children.Add(vLine);
        
        // horizontal center line 
        var hLine = new ColoredRectangleRuntime();
        hLine.Width = _innerPlotSize - (_innerPlotBorderThickness * 2);
        hLine.Height = _axisThickness;
        hLine.Color = Microsoft.Xna.Framework.Color.Gray;
        hLine.Anchor(Gum.Wireframe.Anchor.Center);
        _innerPlotBorder.Children.Add(hLine);
        
        // axis label
        var phiAxis = new TextRuntime();
        phiAxis.Text = "Phi";
        phiAxis.X = _innerPlotSize / 2f;
        phiAxis.Y = _outerPlotHeight * 0.7971f;
        phiAxis.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(phiAxis);
        
        var psiAxis = new TextRuntime();
        psiAxis.Text = "Psi";
        psiAxis.X = - _outerPlotWidth * 0.1881f;
        psiAxis.Y = _innerPlotSize / 2; 
        psiAxis.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(psiAxis);
        
        var xNeg180 = new TextRuntime();
        xNeg180.Text = "- 180";
        xNeg180.X = 0;
        xNeg180.Y = _outerPlotHeight * 0.7247f;
        xNeg180.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(xNeg180);
        
        var x0 = new TextRuntime();
        x0.Text = "0";
        x0.X = _innerPlotSize / 2;
        x0.Y = _outerPlotHeight * 0.7247f;
        x0.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(x0);
        
        var x180 = new TextRuntime();
        x180.Text = "180";
        x180.X = _innerPlotSize;
        x180.Y = _outerPlotHeight * 0.7247f;
        x180.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(x180);
        
        var yNeg180 = new TextRuntime();
        yNeg180.Text = "- 180";
        yNeg180.X = - _innerPlotSize * 0.17f;
        yNeg180.Y = _innerPlotSize;
        yNeg180.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(yNeg180);
        
        var y0 = new TextRuntime();
        y0.Text = "0";
        y0.X = - _innerPlotSize * 0.17f;
        y0.Y = _innerPlotSize / 2;
        y0.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(y0);
        
        var y180 = new TextRuntime();
        y180.Text = "180";
        y180.X = - _innerPlotSize * 0.17f;
        y180.Y = 0;
        y180.Color = Microsoft.Xna.Framework.Color.Black;
        _innerPlotBorder.Children.Add(y180);
        
        // individual points on plot 
        _dotSize = Math.Max(2f, _outerPlotWidth * 0.0114f);    // scales dot with the plot 
        foreach ((float phi, float psi, int residueID) in _angles)
        {
            if (float.IsNaN(phi) || float.IsNaN(psi)) continue;
            
            float x = ((phi + 180f) / 360f) * (_innerPlotSize - _dotSize);
            float y = (1f - (psi + 180f) / 360f) * (_innerPlotSize - _dotSize);

            var dot = new ColoredRectangleRuntime();
            dot.Width = _dotSize;
            dot.Height = _dotSize;
            dot.X = x;
            dot.Y = y;
            
            if ((phi > -120 && phi < -45 && psi > -75 && psi < -15) ||   // right handed alpha helix
                (phi > -180 && phi < -45 && psi > 90 && psi < 180) ||     // beta sheet
                (phi > 45 && phi < 90 && psi > 20 && psi < 60))           // left handed alpha helix
            {
                dot.Color = new Microsoft.Xna.Framework.Color(180, 186, 60); // green
            }
            else if ((phi > -140 && phi < -20 && psi > -100 && psi < 20) ||
                     (phi > -180 && phi < -30 && psi > 70 && psi < 180) ||
                     (phi > 30 && phi < 110 && psi > 0 && psi < 80))
            {
                dot.Color = new Microsoft.Xna.Framework.Color(255, 187, 61); // yellow 
            }
            else
            {
                dot.Color = new Microsoft.Xna.Framework.Color(226, 109, 92); // orange
            }
            _innerPlotBorder.Children.Add(dot);
        }
        
        // --- resizing ---
        _resizeHandle = new ColoredRectangleRuntime();
        _resizeHandle.Width = _resizingHandleSize;
        _resizeHandle.Height = _resizingHandleSize;
        _resizeHandle.Color = new Microsoft.Xna.Framework.Color(226, 109, 92);
        _resizeHandle.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _container.Children.Add(_resizeHandle);
    }
    
    // ================== Build Toggle button  ==================
    public void BuildToggle(float buttonTopY)
    {
        if (_plotButton != null) return;
        
        _plotButton = new Button();
        //_plotButton.X = _container.X + (_container.Width - _plotButtonSize);
        //_plotButton.Y = _padding + 40f + _padding;
        _plotButton.X = _container.X + (_container.Width - _plotButtonSize);
        _plotButton.Y = buttonTopY;
        _plotButton.Click += (s, e) =>
        {
            Toggle();
            //Game1._buttonClickSound.Play();
            if (SideMenu.ButtonSoundEnabled)
                Game1._buttonClickSound.Play(SideMenu.ButtonSoundVolume, 0f, 0f);
        };

        UIHelper.SetButtonTexture(_plotButton, Game1.plotButtonTexture, _plotButtonSize, _plotButtonSize);

        //_plotButton.AddToRoot();
        _container.Visible = false;
    }

    // ================== Updates as when resized or protein changes ==================
    public void Update()
    {
        if (!_container.Visible) return;

        var mouse = Mouse.GetState();
        var mousePos = new Vector2(mouse.X, mouse.Y);
        
        float handleScreenX = _container.X;
        float handleScreenY = _container.Y + _container.Height - _resizingHandleSize;

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
                _dragStartWidth = _outerPlotWidth;
            }

            if (_isDragging)
            {
                float deltaX = _dragStartMouse.X - mousePos.X; // move left = bigger

                float newWidth = Math.Clamp(
                    _dragStartWidth + deltaX * 0.75f,
                    MinPlotWidth,
                    MaxPlotWidth);

                if (Math.Abs(newWidth - _outerPlotWidth) > 0.5f)
                {
                    float rightEdge = _container.X + _container.Width;

                    _outerPlotWidth = newWidth;
                    _outerPlotHeight = _outerPlotWidth * 0.9503f;

                    RebuildPlotContents();

                    // keep right side fixed
                    _container.X = rightEdge - _container.Width;
                }
            }
        }
        else
        {
            _isDragging = false;
        }
    }

    // ================== Allows Toggle ==================
    public void Toggle()
    {
        _container.Visible = !_container.Visible;
    }
    
    // ================== Updates plot points position ==================
    public void UpdatePlot(List<(float phi, float psi, int residueID)> angles)
    {
        _angles = angles;
        _container.Children.Clear();
        RebuildPlotContents();
    }
    
    // ================== anchoring button to top right ==================
    public void OnWindowResize(float screenWidth, float screenHeight)
    {
        if (_container == null || _plotButton == null) return;
    
        // reposition container to stay in upper right
        _container.X = screenWidth - _padding - _container.Width;
        _container.Y = _padding + _plotButtonSize + _padding + 50;
    
        // reposition button to match
        _plotButton.X = _container.X + (_container.Width - _plotButtonSize);
        _plotButton.Y = _padding + 40f + _padding + 50; // + height of menu button
    }
}