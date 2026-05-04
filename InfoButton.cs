
using System;
using Gum.Forms.Controls;
using MonoGameGum;

namespace Final;
 
public class InfoButton
{
    // ================== fields ==================
    private Button _button;
    private float _buttonSize = 40f;
    private float _padding = 10f;
    private Action _onPressed;

    private float _padingFromEdge = 10f;
    private float _menuButtonSize = 40f;
 
    // ================== constructor ==================
    public InfoButton(Action onPressed)
    {
        _onPressed = onPressed;
    }
 
    // ================== Build ==================
    public void Build(float screenWidth)
    {
        if (_button != null) return;
 
        _button = new Button();
        _button.X = screenWidth - _padingFromEdge - _menuButtonSize;
        _button.Y = _padding + 40f + _padding + 50f + 40f + _padding;
        _button.Click += (s, e) =>
        {
            _onPressed?.Invoke();
            if (SideMenu.ButtonSoundEnabled)
                Game1._buttonClickSound.Play();
        };
 
        UIHelper.SetButtonTexture(_button, Game1.infoButtonTexture, _buttonSize, _buttonSize);
        _button.AddToRoot();
    }
 
    // ================== Reposition on window resize ==================
    public void OnWindowResize(float screenWidth, float screenHeight)
    {
        if (_button == null) return;
 
        _button.X = screenWidth - _padingFromEdge - _menuButtonSize;
        _button.Y = _padding + 40f + _padding + 50f +  40f + _padding;
    }
}
