using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Gum.Forms;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Microsoft.Xna.Framework.Audio;
using MonoGameGum;
using Button = Gum.Forms.Controls.Button;
using Dialog = NativeFileDialogSharp.Dialog;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Label = Gum.Forms.Controls.Label;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using Window = Gum.Forms.Window;
using WindowVisual = Gum.Forms.DefaultVisuals.V3.WindowVisual;


namespace Final;

public class Game1 : Game
{
    // --- monogame ---
    private GumService GumUI => GumService.Default;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // --- fonts ---
    private SpriteFont _regularFont;
    private SpriteFont _boldFont;

    // --- 3D models ---
    private Microsoft.Xna.Framework.Graphics.Model _sphere;
    private Microsoft.Xna.Framework.Graphics.Model _bond;

    // --- camera & input ---
    private Cam _camera;
    private MouseClicker _clicker;
    private SelectionManager _selectManager;
    private KeyboardState _previousKState;
    private MouseState _previousMouseState;
    private Viewport _croppedViewport;

    // --- protein models ---
    public static int CurrentProtein = 0;
    public List<Model> Models;
    public string[] exampleModels = new string[4];
    private Dictionary<int, string> _savePrevFilePath;
    private float _atomScaler = 5f;

    // --- static references ---
    public static Game1 Instance;
    public static List<Model> SharedModels => Instance.Models;

    // --- audio ---
    public static SoundEffect _buttonClickSound;

    // --- ui - textures ---
    public static Texture2D plotButtonTexture;
    public static Texture2D menuButtonTexture;
    public static Texture2D resetButtonTexture;
    public static Texture2D backboneButtonTexture;
    public static Texture2D infoButtonTexture;

    // --- ui - components ---
    private RamachandranPlot _ramaPlot;
    private SideMenu _sideMenu;
    private ControlPanel _controlPanel;
    private const int MenuBarHeight = 30;
    private FilterHelper _filterHelper;

    public Game1()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        _graphics.PreferMultiSampling = true;
    }

    private void OnResize(object sender, EventArgs e)
    {
        // Pass the viewport minus the menu bar so the camera's aspect ratio stays correct
        _camera.Resize(_graphics, Window);
        _controlPanel.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        _sideMenu.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        _ramaPlot.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        //_infoButton.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
    }

    protected override void Initialize()
    {
        _camera = new Cam(GraphicsDevice);
        _clicker = new MouseClicker(_camera);
        _selectManager = new SelectionManager();
        //GumUI.Initialize(this);
        _savePrevFilePath = new Dictionary<int, string>();
        //GumService.Default.Initialize(GraphicsDevice); 
        
        // Create Models list, starting with 4 examples already loaded
        Models = new List<Model>();
        exampleModels = ["2wj7.cif", "1AL1.cif", "1GFL.cif", "9mar.cif"];
        foreach (string exampleProteinName in exampleModels)
        {
            string filePath = Path.Combine(Content.RootDirectory, exampleProteinName);
            List<Residue> proteinData = DataReader.ReadFile(filePath);
            Models.Add(new Model(exampleProteinName, proteinData));
        }
        Models[CurrentProtein].Shown = true;
        
        
        // GUI Elements
        InitializeGUI();
        _controlPanel = new ControlPanel(
            onToggleBackbone: () => Models[CurrentProtein].BackboneOnly = !Models[CurrentProtein].BackboneOnly,
            onResetModel: () =>
            {
                bool backboneOnly = Models[CurrentProtein].BackboneOnly;
                string filePath = CurrentProtein <= 3
                    ? Path.Combine(Content.RootDirectory, exampleModels[CurrentProtein])
                    : _savePrevFilePath[CurrentProtein];
                List<Residue> proteinData = DataReader.ReadFile(filePath);
                Models[CurrentProtein] = new Model(Path.GetFileNameWithoutExtension(filePath), proteinData);
                Models[CurrentProtein].Shown = true;
                Models[CurrentProtein].BackboneOnly = backboneOnly;
            },
            onToggleRamaPlot: () => _ramaPlot.Toggle(),
            onToggleMenu: () => _sideMenu.MenuToggle(),
            onFilterChanged: (filter) => { },
            onResetCamera: () =>_camera.ResetCamera(),
            onShowInfo: () => CreateWelcomeWindow(),
            onSwitchProtein: new Action<int> (i =>
            {
                CurrentProtein = i;
                foreach (var m in Models) m.Shown = false;
                Models[i].Shown = true;
            }), 
            _filterHelper = new FilterHelper(Models, CurrentProtein)
            );
        //_controlPanel.Build();
        //CreateWelcomeWindow();
        

        base.Initialize();
    }
    

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // fonts
        _regularFont = Content.Load<SpriteFont>("fonts/SourceSansPro-Bold");
        _boldFont =  Content.Load<SpriteFont>("fonts/SourceSansPro-Black");

        // 3D model elements
        _sphere = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/lowpolysphere");
        _bond = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/cylinder");

        // audio
        _buttonClickSound = Content.Load<SoundEffect>("audio/buttonPressSound");

        // button texture 
        plotButtonTexture = Content.Load<Texture2D>("images/plotButton");
        menuButtonTexture = Content.Load<Texture2D>("images/menuButton");
        resetButtonTexture = Content.Load<Texture2D>("images/resetButton");
        backboneButtonTexture = Content.Load<Texture2D>("images/backboneButton");
        infoButtonTexture = Content.Load<Texture2D>("images/infoButton");
        
        _controlPanel.Build(Window.ClientBounds.Width, Window.ClientBounds.Height);
        
        // for spacing buttons 
        float plotButtonY = 110;

        // plot 
        List<(float phi, float psi, int residueId)> angles = RamachandranCalculator.GetAngles(Models[0].ProteinData);
        _ramaPlot = new RamachandranPlot(angles);
        _ramaPlot.BuildPlot(Window.ClientBounds.Width, Window.ClientBounds.Height, plotButtonY);
        
        
        // menu
        _sideMenu = new SideMenu();
        _sideMenu.BuildMenu(Window.ClientBounds.Width, Window.ClientBounds.Height);
        CreateWelcomeWindow(); // move it here, last thing

        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        KeyboardState kState = Keyboard.GetState();
        MouseState mState = Mouse.GetState();

        if (_selectManager.SelectedAtom is not null)
        {
            Console.WriteLine(_selectManager.SelectedAtom.Residue.AAType);
        }
        
        _selectManager.Update(mState, _previousMouseState, _clicker.GetRay(_croppedViewport), Models[CurrentProtein], _clicker);
        _camera.UpdateCamera(gameTime, kState, mState);
        Models[CurrentProtein].UpdateAtomData(gameTime);
        HandleModelInput(kState);
        GumUI.Update(gameTime);

        if (kState.IsKeyDown(Keys.B) && _previousKState.IsKeyUp(Keys.B))
        {
            Models[CurrentProtein].BackboneOnly = !Models[CurrentProtein].BackboneOnly;
        }

        if (kState.IsKeyDown(Keys.I) && _previousKState.IsKeyUp(Keys.I))
        {
            string filePath = GetFile();
            Console.WriteLine($"Got file path: '{filePath}'"); // add thi
            
            if (File.Exists(filePath))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                List<Residue> proteinData = DataReader.ReadFile(filePath);
                Model model = new Model(fileName, proteinData);
                Models.Add(model);
                
                _savePrevFilePath[Models.Count - 1] = filePath;
            }
            else
            {
                // TODO: Output warning screen that the file was not imported
            }
        }

        if (kState.IsKeyDown(Keys.R) && _previousKState.IsKeyUp(Keys.R))
        {
            bool backboneOnly = Models[CurrentProtein].BackboneOnly; // preserve UI state
    
            string filePath = CurrentProtein <= 3
                ? Path.Combine(Content.RootDirectory, exampleModels[CurrentProtein])
                : _savePrevFilePath[CurrentProtein];

            List<Residue> proteinData = DataReader.ReadFile(filePath);
            Models[CurrentProtein] = new Model(Path.GetFileNameWithoutExtension(filePath), proteinData);
            Models[CurrentProtein].Shown = true;
            Models[CurrentProtein].BackboneOnly = backboneOnly;
        }

        if (kState.IsKeyDown(Keys.P) && _previousKState.IsKeyUp(Keys.P))
        {
            Model model = Models[CurrentProtein];
            string filePath = CurrentProtein <= 3
                ? Path.Combine(Content.RootDirectory, exampleModels[CurrentProtein])
                : _savePrevFilePath[CurrentProtein];
            
            ExportFile(model, filePath);
        }
        
        _previousMouseState = mState;
        _previousKState = kState;
        
        // allows for plot resizing and updating when protein is changed 
        List<(float phi, float psi, int residueId)> angles = RamachandranCalculator.GetAngles(Models[CurrentProtein].ProteinData);
        _ramaPlot.UpdatePlot(angles);
        _ramaPlot.Update();
                
        // allows for menu resizing 
        _sideMenu.Update();
        
        _filterHelper.SetModel(CurrentProtein);
        GumService.Default.Update(gameTime);
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.Clear(SideMenu.IsDarkMode ? Color.Black : Color.White);

        // Restrict the 3D render area to below the menu bar
        var fullViewport = GraphicsDevice.Viewport;
        _croppedViewport = new Viewport(
            fullViewport.X,
            fullViewport.Y + MenuBarHeight,
            fullViewport.Width,
            Math.Max(1, fullViewport.Height - MenuBarHeight)
        );
        GraphicsDevice.Viewport = _croppedViewport;

        foreach (Model model in Models)
        {
            if (model.Shown)
            {
                model.DrawAtoms(_sphere, _camera.View, _camera.Projection, _atomScaler, _camera.LightDirection);
                model.DrawBonds(_bond, _camera.View, _camera.Projection, _camera.LightDirection);
            }
        }

        // Restore full viewport for GumUI so the menu bar renders correctly
        GraphicsDevice.Viewport = fullViewport;
        
        GumUI.Draw();
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        GumService.Default.Draw();
        
        base.Draw(gameTime);
        
        _spriteBatch.Begin();
        TextDisplayCurrentModels(_spriteBatch, _graphics.GraphicsDevice, _regularFont, _boldFont, 10f, 30, 100, _sideMenu);
        _spriteBatch.End();
    }
    
    private void HandleModelInput(KeyboardState kState)
    {
        Keys[] exKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
        for (int i = 0; i < exKeys.Length; i++)
        {
            if ((kState.IsKeyDown(exKeys[i]) && _previousKState.IsKeyUp(exKeys[i])) && Models.Count > i)
            {
                CurrentProtein = i;
                foreach (var m in Models) m.Shown = false;
                Models[i].Shown = true;
                _camera.ResetCamera();
                break;
            }
        }
    }

    public string GetFile()
    {
        if (System.OperatingSystem.IsWindows())
        {
            var result = Dialog.FileOpen("cif;pdb");
            if (result.IsOk)
            {
                string filePath = result.Path;
                return filePath;
            }
        }
        else if (System.OperatingSystem.IsMacOS())
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "osascript";
            process.StartInfo.Arguments = "-e \"choose file with prompt \\\"Select a CIF or PDB file\\\" of type {\\\"cif\\\", \\\"pdb\\\"}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
        
            // osascript returns "alias Mac:path:to:file" format, convert to unix path
            if (!string.IsNullOrEmpty(output))
            {
                string path = output.Replace("alias ", "").Replace(":", "/").TrimStart('/');
                path = "/" + path.Substring(path.IndexOf('/'));
                return path;
            }
        }
        return "";

        
        
    }

    public void ExportFile(Model model, string filePath)
    {
        string content = File.ReadAllText(filePath);
        string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        int index = Array.FindIndex(lines, f => f.Contains("_atom_site.pdbx_PDB_model_num ")) + 1;
        foreach (Residue residue in Models[CurrentProtein].ProteinData)
        {
            foreach (Atom atom in residue.Atoms)
            {
                Vector3 currPosition = atom.Position;

                string currLine = lines[index];
                string[] parts = currLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string oldX = parts[10];
                string oldY = parts[11];
                string oldZ = parts[12];

                string fX = currPosition.X.ToString("F3", CultureInfo.InvariantCulture);
                string fY = currPosition.Y.ToString("F3", CultureInfo.InvariantCulture);
                string fZ = currPosition.Z.ToString("F3", CultureInfo.InvariantCulture);

                int iX = currLine.IndexOf(oldX);
                currLine = currLine.Remove(iX, oldX.Length).Insert(iX, fX);

                int iY = currLine.IndexOf(oldY, iX + fX.Length);
                currLine = currLine.Remove(iY, oldY.Length).Insert(iY, fY);

                int iZ = currLine.IndexOf(oldZ, iY + fY.Length);
                currLine = currLine.Remove(iZ, oldZ.Length).Insert(iZ, fZ);

                lines[index] = currLine;

                index += 1;
            }
        }

        string outputPath = "";
        if (System.OperatingSystem.IsWindows())
        {
            var result = Dialog.FileSave("cif");
            if (result.IsOk)
            {
                outputPath = result.Path;
            }
        }
        else if (System.OperatingSystem.IsMacOS())
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "osascript";
            process.StartInfo.Arguments = "-e \"choose file name with prompt \\\"Save CIF File\\\" default name \\\"output.cif\\\"\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                // Convert Mac alias path (e.g. "alias Mac:Users:...") to unix path
                string path = output.Replace("alias ", "");
                string[] parts = path.Split(':');
                // parts[0] is the volume name (e.g. "Mac"), rest is the path
                outputPath = "/" + string.Join("/", parts, 1, parts.Length - 1);
            }
        }
       

        if (outputPath != "")
        {
            // Append .cif if the user didn't type it
            if (!outputPath.EndsWith(".cif", StringComparison.OrdinalIgnoreCase))
                outputPath = outputPath + ".cif";

            File.WriteAllText(outputPath, content);
        }
        else
        {
            //TODO: Output warning screen that new filepath was not accepted
        }
        
        
        
    }
    public void InitializeGUI()
    {
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);
        Window.AllowUserResizing = true;
        // This event is raised whenever a resize occurs, allowing
        // us to perform custom logic on a resize
        Window.ClientSizeChanged += HandleClientSizeChanged;
    }

    private void HandleClientSizeChanged(object sender, EventArgs e)
    {
        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges(); 
        
        GumUI.CanvasWidth = GraphicsDevice.Viewport.Width;
        GumUI.CanvasHeight = GraphicsDevice.Viewport.Height;
        
        _camera.Resize(_graphics, Window);
        _controlPanel.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        _sideMenu.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        _ramaPlot.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        
        GumUI.Root.UpdateLayout();
    }

    public void TextDisplayCurrentModels(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, SpriteFont regularFont, SpriteFont boldFont, 
        float spacing, int xPos, int yMidDisplacement, SideMenu sideMenu)
    {
        Color textColor;
        if (SideMenu.IsDarkMode)
        {
             textColor = Color.White;
        }
        else
        {
            textColor = Color.Black;
        }
        
        
        int numModels = Models.Count;
        float yDisplacement = numModels * spacing;
        float startingPos = graphicsDevice.Viewport.Height / 2f + yMidDisplacement - yDisplacement;

        float yPos = startingPos;
        int index = 1;
        foreach (var model in Models)
        {
            if (model.Shown)
            {
                _spriteBatch.DrawString(boldFont, $"{index}. {model.Name}", new Vector2(xPos, yPos), Color.Red);
            }
            else
            {
                _spriteBatch.DrawString(regularFont, $"{index}. {model.Name}", new Vector2(xPos, yPos), textColor);
            }
            
            yPos += spacing + regularFont.MeasureString(model.Name).Y;
            index += 1;
        }
    }

    public void CreateWelcomeWindow()
    {
        var welcomeWindow = new Window();
        welcomeWindow.Anchor(Gum.Wireframe.Anchor.Center);
        var windowVisual = (WindowVisual)welcomeWindow.Visual;
        windowVisual.BackgroundColor = Color.White;
        
        welcomeWindow.Width = 410;
        welcomeWindow.Height = 300;
        welcomeWindow.AddToRoot();
        
        var welcomeTitle = new Label();
        welcomeTitle.Text = "Welcome to Our Protein Visualizer!";
        welcomeTitle.Dock(Gum.Wireframe.Dock.Top);
        windowVisual.TitleBarInstance.AddChild(welcomeTitle);

        var instructionsText = new Label();
        instructionsText.Dock(Gum.Wireframe.Dock.Left);
        instructionsText.Text = "Instructions:\n" +
                           "Press \"I\" to Input a Model\n" +
                           "Press \"R\" to Reset the Current Model\n" +
                           "Press \"P\" to Export the Current Model\n" +
                           "Press \"B\" to Enable Backbone Only View\n \n" +
                           "To Rotate the Camera press the Arrow Keys\n" +
                           "To Move the presently shown Atoms,\nSelect and Drag the Model with your Mouse\n" +
                           "\nUse the Number Keys To Switch Between Models ";
        instructionsText.X = 5;
        instructionsText.Y = -10;
        welcomeWindow.AddChild(instructionsText);
        
        var closeButton = new Button();
        closeButton.Anchor(Gum.Wireframe.Anchor.Bottom);
        closeButton.Y = -10;
        closeButton.Text = "Close";
        welcomeWindow.AddChild(closeButton);
        closeButton.Click += (_, _) =>
        {
            welcomeWindow.RemoveFromRoot();
        };
    }
}
