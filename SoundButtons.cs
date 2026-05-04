/*
HOW TO USE SoundButton:
    -in Game1.cs
        private SoundEffect _theSoundYouWant; 
        
        private Texture2D _textureWhenButtonNotClicked;
        private Texture2D _textureWhenButtonClicked;
        
        private Vector2 _soundButtonPosition = new Vector2(#, #);
        
        private SoundButton _buttonWithAudio;
        
        - in LoadContent()
            _theSoundYouWant = Content.Load<SoundEffect>("path/theSoundYouWant");       // as a .wav
            
            _textureWhenButtonNotClicked = Content.Load<Texture2D>("path/textureWhenButtonNotClicked"); 
            _textureWhenButtonClicked = Content.Load<Texture2D>("path/textureWhenButtonClicked"); 
            
            _buttonWithAudio = new SoundButton(_textureWhenButtonNotClicked,
            _textureWhenButtonClicked,
            _soundButtonPosition,
            _buttonClickSound);
            
        - in Update(GameTime gameTime)
            if (mState.LeftButton == ButtonState.Pressed)
                _buttonWithAudio.IsPressed(mState.X, mState.Y)
            else
                _buttonWithAudio.NotPressed();
                
        - in Draw(GameTime gameTime)
            _buttonWithAudio.Draw(_spriteBatch);
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Final;

public class SoundButtons : Buttons
{
    private SoundEffect _clickSound;
    private bool _wasPressed = false;

    public SoundButtons(Texture2D buttonTextureInput,
        Texture2D pressedButtonTextureInput,
        Vector2 buttonPositionInput,
        SoundEffect clickSound)
        : base(buttonTextureInput, pressedButtonTextureInput, buttonPositionInput)
    {
        _clickSound = clickSound;
    }

    public new void IsPressed(float mx, float my)
    {
        base.IsPressed(mx, my);

        if (buttonPressed && !_wasPressed)
            _clickSound?.Play();

        _wasPressed = buttonPressed;
    }

    public new void NotPressed()
    {
        base.NotPressed();
        _wasPressed = false;
    }
}