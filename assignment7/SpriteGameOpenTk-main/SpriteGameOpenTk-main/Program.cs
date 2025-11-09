// File: Program.cs
//
// Updated to add: Jump + Run, state machine, physics-like vertical motion,
// animation rows selection based on actual sprite PNG dimensions,
// model transform updated per-frame, and clean separation of concerns.

using OpenTK.Graphics.OpenGL4;                       // OpenGL API
using OpenTK.Windowing.Common;                       // Frame events (OnLoad/OnUpdate/OnRender)
using OpenTK.Windowing.Desktop;                      // GameWindow/NativeWindowSettings
using OpenTK.Windowing.GraphicsLibraryFramework;     // Keyboard state
using OpenTK.Mathematics;                            // Matrix4, Vector types
using System;
using System.IO;
using ImageSharp = SixLabors.ImageSharp.Image;       // Alias for brevity
using SixLabors.ImageSharp.PixelFormats;             // Rgba32 pixel type

namespace OpenTK_Sprite_Animation
{
    public class SpriteAnimationGame : GameWindow
    {
        private Character _character;                 // Handles animation state + UV selection
        private int _shaderProgram;                   // Linked GLSL program
        private int _vao, _vbo;                       // Geometry
        private int _texture;                         // Sprite sheet

        public SpriteAnimationGame()
            : base(
                new GameWindowSettings(),
                new NativeWindowSettings { Size = (800, 600), Title = "Sprite Animation" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 0f);            // Transparent background (A=0)
            GL.Enable(EnableCap.Blend);               // Enable alpha blending
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shaderProgram = CreateShaderProgram();   // Compile + link
            _texture = LoadTexture("Sprite_Character.png"); // Upload sprite sheet

            // Quad vertices: [pos.x, pos.y, uv.x, uv.y], centered model space
            float w = 64f, h = 128f;                  // half-size used for quad geometry
            float[] vertices =
            {
                -w, -h, 0f, 0f,
                 w, -h, 1f, 0f,
                 w,  h, 1f, 1f,
                -w,  h, 0f, 1f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Attribute 0: vec2 position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            // Attribute 1: vec2 texcoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.UseProgram(_shaderProgram);

            // Bind sampler to texture unit 0 (WHY: avoid undefined default binding)
            int texLoc = GL.GetUniformLocation(_shaderProgram, "uTexture");
            GL.Uniform1(texLoc, 0);

            // Orthographic projection (pixel coordinates 0..800, 0..600)
            // IMPORTANT: positional args to avoid API-name mismatch across OpenTK versions.
            int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(0, 800, 0, 600, -1, 1);
            GL.UniformMatrix4(projLoc, false, ref ortho);

            // Character will manage the model transform itself; start at (400,300)
            _character = new Character(_shaderProgram, 400f, 300f, _texture, GetLoadedTextureSize("Sprite_Character.png"));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Read keyboard state -> forward to character
            var kb = KeyboardState;

            // We'll pass booleans: left/right/run/jump (space/up)
            bool left = kb.IsKeyDown(Keys.Left);
            bool right = kb.IsKeyDown(Keys.Right);
            bool run = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
            bool jumpPressed = kb.IsKeyDown(Keys.Space) || kb.IsKeyDown(Keys.Up);

            _character.Update((float)e.Time, left, right, run, jumpPressed);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Bind texture and VAO, then draw
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.BindVertexArray(_vao);

            _character.Render();

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            // Free GPU resources
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            base.OnUnload();
        }

        // --- Shader creation utilities ---------------------------------------------------------

        private int CreateShaderProgram()
        {
            // Vertex Shader: transforms positions, flips V in UVs (image origin vs GL origin)
            string vs = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 projection;
uniform mat4 model;
void main() {
    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
    vTexCoord = vec2(aTexCoord.x, 1.0 - aTexCoord.y); // flip V so PNGs read intuitively
}";

            // Fragment Shader: samples sub-rect of the sheet using uOffset/uSize
            string fs = @"
#version 330 core
in vec2 vTexCoord;
out vec4 color;
uniform sampler2D uTexture; // bound to texture unit 0
uniform vec2 uOffset;       // normalized UV start (0..1)
uniform vec2 uSize;         // normalized UV size  (0..1)
void main() {
    vec2 uv = uOffset + vTexCoord * uSize;
    color = texture(uTexture, uv);
}";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            CheckShaderCompile(v, "VERTEX");

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            CheckShaderCompile(f, "FRAGMENT");

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            CheckProgramLink(p);

            GL.DetachShader(p, v);
            GL.DetachShader(p, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }

        private static void CheckShaderCompile(int shader, string stage)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0)
                throw new Exception($"{stage} SHADER COMPILE ERROR:\n{GL.GetShaderInfoLog(shader)}");
        }

        private static void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0)
                throw new Exception($"PROGRAM LINK ERROR:\n{GL.GetProgramInfoLog(program)}");
        }

        // --- Texture loading ------------------------------------------------------------------

        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}", path);

            using var img = ImageSharp.Load<Rgba32>(path); // decode to RGBA8

            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            // Copy raw pixels to managed buffer then upload
            var pixels = new byte[4 * img.Width * img.Height];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Nearest: prevents bleeding between adjacent frames on the atlas
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Clamp: avoid wrap artifacts at frame borders
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return tex;
        }

        // Helper: read image dimensions (width,height) for Character to compute UVs correctly
        private (int width, int height) GetLoadedTextureSize(string path)
        {
            using var img = ImageSharp.Load(path);
            return (img.Width, img.Height);
        }
    }

    // --- Character state machine + animator -----------------------------------------------
    public enum CharacterState { Idle, Walk, Run, Jump }
    public enum Facing { Right, Left }

    public class Character
    {
        private readonly int _shader;      // Program containing uOffset/uSize
        private readonly int _texture;     // texture id (for reference)
        private readonly float _sheetW;    // sheet pixel width
        private readonly float _sheetH;    // sheet pixel height

        // Position & physics
        public Vector2 Position;
        private float _vy;                 // vertical velocity (pixels/sec)
        private const float Gravity = -1200f;   // pixels per second^2 (downwards)
        private const float JumpVel = 520f;     // initial jump velocity (pixels/sec)

        // Movement speeds (pixels/sec)
        private const float WalkSpeed = 120f;
        private const float RunSpeed = 260f;

        // Animation timing
        private float _timer;              // accumulated time for stepping
        private int _frame;                // current frame index
        private float _frameTimeWalk = 0.15f;
        private float _frameTimeRun = 0.08f;

        // Facing & state
        private Facing _facing = Facing.Right;
        private CharacterState _state = CharacterState.Idle;
        private bool _grounded = true;     // simple grounded flag: true when on or above baseY

        // Sprite layout guesses (will be adjusted to the loaded image)
        private readonly int _cols = 4;    // frames per row (commonly 4)
        private readonly float FrameW;     // pixel width of a frame
        private readonly float FrameH;     // pixel height of a frame
        private readonly float Gap;        // horizontal spacing between frames (pixels)
        private readonly int _rowsAvailable; // computed from sheetH/FrameH

        // Where to place the quad in world-space (we will translate model)
        private readonly int _modelLoc;
        private readonly int _offsetLoc;
        private readonly int _sizeLoc;

        // Ground Y (simple floor). This keeps the character anchored visually.
        private readonly float _groundY = 300f; // this is the baseline center Y used in your original code

        public Character(int shaderProgram, float startX, float startY, int textureId, (int width, int height) sheetPixels)
        {
            _shader = shaderProgram;
            _texture = textureId;
            Position = new Vector2(startX, startY);
            _vy = 0f;

            _sheetW = sheetPixels.width;
            _sheetH = sheetPixels.height;

            // Heuristic: try to infer frame sizes from known / provided PNG layout.
            // The PNG provided to me had approx 4 columns and 2 rows with
            // frame widths ~77px and frame height ~127px; compute robust defaults:
            FrameH = _sheetH / 2f; // assume two rows (top/bottom)
            FrameW = _sheetW / 4f; // assume 4 columns

            // Compute gap by evenly distributing remaining space between frames:
            // total width = 4 * FrameW + 3 * Gap  => Gap = (sheetW - 4*FrameW) / 3
            Gap = (_sheetW - _cols * FrameW) / Math.Max(1, (_cols - 1));

            // number of rows available in sheet (floor)
            _rowsAvailable = Math.Max(1, (int)Math.Floor(_sheetH / FrameH));

            // Uniform locations
            _modelLoc = GL.GetUniformLocation(_shader, "model");
            _offsetLoc = GL.GetUniformLocation(_shader, "uOffset");
            _sizeLoc = GL.GetUniformLocation(_shader, "uSize");

            // start in idle frame
            _state = CharacterState.Idle;
            _frame = 0;
            UpdateSpriteUniformsForFrame(_frame, 0); // default top row 0
            // Ensure character starts resting on ground baseline
            Position.Y = startY;
            _grounded = true;
        }

        /// <summary>
        /// Main update called each frame. left/right/run/jump come from keyboard state.
        /// </summary>
        public void Update(float delta, bool left, bool right, bool run, bool jumpPressed)
        {
            // --- Determine horizontal direction & facing
            float dir = 0f;
            if (left) dir = -1f;
            else if (right) dir = 1f;

            if (dir > 0) _facing = Facing.Right;
            else if (dir < 0) _facing = Facing.Left;

            // --- Movement & state transitions
            // Jump input: only trigger when grounded
            bool tryJump = jumpPressed && _grounded;
            if (tryJump)
            {
                _vy = JumpVel;
                _grounded = false;
                _state = CharacterState.Jump;
                // Set jump sprite row if available (row index e.g. 2)
                ChooseJumpFrame();
            }
            else if (!_grounded)
            {
                _state = CharacterState.Jump; // maintain mid-air
            }
            else if (Math.Abs(dir) > 0.001f)
            {
                _state = run ? CharacterState.Run : CharacterState.Walk;
            }
            else
            {
                _state = CharacterState.Idle;
            }

            // --- Horizontal movement
            float speed = _state == CharacterState.Run ? RunSpeed : WalkSpeed;
            Position.X += dir * speed * delta;

            // --- Vertical physics (simple)
            if (!_grounded)
            {
                // integrate vy with gravity (downwards)
                _vy += Gravity * delta;
                Position.Y += _vy * delta;

                // If we reached or passed baseline ground, land
                if (Position.Y <= _groundY)
                {
                    Position.Y = _groundY;
                    _vy = 0f;
                    _grounded = true;
                    // after landing, change to Idle/Walk depending on dir
                    if (Math.Abs(dir) > 0.001f)
                        _state = run ? CharacterState.Run : CharacterState.Walk;
                    else
                        _state = CharacterState.Idle;
                }
            }

            // --- Animation timing and frame selection
            switch (_state)
            {
                case CharacterState.Idle:
                    // keep last frame visible or set specific idle frame
                    // We'll set frame 0 of facing row
                    _frame = 0;
                    UpdateSpriteUniformsForFrame(_frame, GetRowForState(_state, _facing));
                    break;

                case CharacterState.Walk:
                    _timer += delta;
                    if (_timer >= _frameTimeWalk)
                    {
                        _timer -= _frameTimeWalk;
                        _frame = (_frame + 1) % _cols;
                    }
                    UpdateSpriteUniformsForFrame(_frame, GetRowForState(_state, _facing));
                    break;

                case CharacterState.Run:
                    _timer += delta;
                    if (_timer >= _frameTimeRun)
                    {
                        _timer -= _frameTimeRun;
                        _frame = (_frame + 1) % _cols;
                    }
                    UpdateSpriteUniformsForFrame(_frame, GetRowForState(_state, _facing));
                    break;

                case CharacterState.Jump:
                    // Jump typically uses a single frame (e.g., last frame of row).
                    // We pick a reasonable single-frame index (middle or last) so it looks like a jump pose.
                    int jumpFrameIdx = Math.Min(_cols - 1, 1); // prefer frame 1 or last if not enough frames
                    UpdateSpriteUniformsForFrame(jumpFrameIdx, GetRowForState(_state, _facing));
                    break;
            }
        }

        /// <summary>
        /// Render: set model transform (translate to Position) then draw.
        /// </summary>
        public void Render()
        {
            // Build model matrix (translate to Position; Z=0)
            Matrix4 model = Matrix4.CreateTranslation(Position.X, Position.Y, 0f);

            GL.UseProgram(_shader);
            GL.UniformMatrix4(_modelLoc, false, ref model);

            // Draw quad (VAO must be bound externally)
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        private void ChooseJumpFrame()
        {
            // If the sheet contains a dedicated jump row, prefer to use it; otherwise, we fallback.
            // We'll attempt to pick row index 2 (i.e., third row) for jump animations.
            // GetRowForState will clamp to available rows.
            UpdateSpriteUniformsForFrame(0, GetRowForState(CharacterState.Jump, _facing));
        }

        /// <summary>
        /// Map (col,row) to normalized UVs and upload to shader.
        /// This function handles fallbacks when requested row is not present.
        /// </summary>
        private void UpdateSpriteUniformsForFrame(int col, int row)
        {
            // Clamp column
            col = Math.Clamp(col, 0, _cols - 1);

            // Determine available rows from sheet size; if requested row >= available, fallback to 0 or facing rows
            int rowCountAvailable = _rowsAvailable;
            if (row >= rowCountAvailable) row = 0;

            // Compute start X in pixels: start of column = col*(FrameW + Gap)
            float startX = col * (FrameW + Gap);
            float startY = row * FrameH;

            // Convert to normalized UV (0..1)
            float u = startX / _sheetW;
            float v = startY / _sheetH;
            float w = FrameW / _sheetW;
            float h = FrameH / _sheetH;

            GL.UseProgram(_shader);
            GL.Uniform2(_offsetLoc, u, v);
            GL.Uniform2(_sizeLoc, w, h);
        }

        /// <summary>
        /// Determines which row index to use for a given state and facing direction.
        /// You can customize row mapping according to your atlas layout.
        /// By default: row 0 = facing right, row 1 = facing left.
        /// If you have extra rows: row 2/3 could be jump rows (right/left).
        /// </summary>
        private int GetRowForState(CharacterState state, Facing facing)
        {
            // Default layout assumptions:
            // row 0: right-facing walk/idle
            // row 1: left-facing walk/idle
            // row 2: right-facing jump (optional)
            // row 3: left-facing jump (optional)
            int baseRowForFacing = facing == Facing.Right ? 0 : 1;

            return state switch
            {
                CharacterState.Idle => baseRowForFacing,
                CharacterState.Walk => baseRowForFacing,
                CharacterState.Run => baseRowForFacing, // assume same row as walk but faster timing
                CharacterState.Jump =>
                    // if jump rows exist, prefer rows 2/3; otherwise fallback to facing row
                    (_rowsAvailable >= 4) ? (facing == Facing.Right ? 2 : 3) :
                    (_rowsAvailable >= 3) ? 2 : baseRowForFacing,
                _ => baseRowForFacing,
            };
        }
    }

    // --- Entry point ---------------------------------------------------------------------------
    internal class Program
    {
        private static void Main()
        {
            using var game = new SpriteAnimationGame(); // Ensures Dispose/OnUnload is called
            game.Run();                                  // Game loop: Load -> (Update/Render)* -> Unload
        }
    }
}
