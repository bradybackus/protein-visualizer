using Gum.DataTypes.Variables;
using Gum.Forms.Controls;
using MonoGameGum.GueDeriving;

namespace Final;

public class UIHelper
{
    public static void SetButtonTexture(Button button, Microsoft.Xna.Framework.Graphics.Texture2D texture, float width, float height)
    {
        button.Width = width;
        button.Height = height;
        button.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        button.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        button.Visual.Width = width;
        button.Visual.Height = height;

        var buttonVisual = (Gum.Forms.DefaultVisuals.V3.ButtonVisual)button.Visual;
        var background = buttonVisual.Background;
        background.Color = Microsoft.Xna.Framework.Color.Transparent;
        buttonVisual.TextInstance.Text = "";

        // override all states to keep background transparent
        buttonVisual.ButtonCategory.ResetAllStates();
        buttonVisual.States.Enabled.Apply = () => background.Color = Microsoft.Xna.Framework.Color.Transparent;
        buttonVisual.States.Highlighted.Apply = () => background.Color = Microsoft.Xna.Framework.Color.Transparent;
        buttonVisual.States.Pushed.Apply = () => background.Color = Microsoft.Xna.Framework.Color.Transparent;
        buttonVisual.States.Focused.Apply = () => background.Color = Microsoft.Xna.Framework.Color.Transparent;
        buttonVisual.States.HighlightedFocused.Apply = () => background.Color = Microsoft.Xna.Framework.Color.Transparent;

        var icon = new SpriteRuntime();
        icon.Texture = texture;
        icon.Width = width;
        icon.Height = height;
        icon.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        icon.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        button.Visual.Children.Add(icon);
    }
}