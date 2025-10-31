using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace BackRoomMap
{
    public class Game : GameWindow
    {
        Shader roomShader, lightSourceShader;
        Camera camera;
        Texture wallTex, floorTex;

        // Meshes 3  types
        Mesh floorCeilingMesh, wallMesh, decorativeMesh;

        // Set scene
        const float ROOM_SIZE = 10f;
        bool lightOn = true;
        Vector3 lightPos = new Vector3(0f, 4f, 0f);

        bool firstMove = true;

        public Game(GameWindowSettings g, NativeWindowSettings n) : base(g, n) { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.DepthTest);

            // Disable Culling to see the inside of the room walls.
            GL.Disable(EnableCap.CullFace);

            // Initialize shaders
            roomShader = new Shader("Shaders/vertex.glsl", "Shaders/fragment.glsl");
            lightSourceShader = new Shader("Shaders/vertex.glsl", "Shaders/lightsource.frag");

            // Initialize camera: Positioned in the center of the room, 1.6f up.
            camera = new Camera(new Vector3(0, 1.6f, 0));

            // Create 3 distinct meshes
            floorCeilingMesh = Mesh.CreateYQuad();
            wallMesh = Mesh.CreateZQuad();
            decorativeMesh = Mesh.CreateCube(); // Used for pillar and interactable

            // Load textures (Ensure 'Assets/wall.png' and 'Assets/floor.png' exist)
            wallTex = new Texture("Assets/wall.png");
            floorTex = new Texture("Assets/floor.png");

            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            roomShader.Dispose();
            lightSourceShader.Dispose();
            floorCeilingMesh.Dispose();
            wallMesh.Dispose();
            decorativeMesh.Dispose();
            wallTex.Dispose();
            floorTex.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            var ks = KeyboardState;
            if (ks.IsKeyDown(Keys.Escape)) Close();

            
            camera.ProcessKeyboard(ks, (float)e.Time);

            Vector3 pos = camera.Position;
            float halfRoom = ROOM_SIZE / 2f;
            float padding = 0.5f;

            // Collision: Keep player inside the room bounds (-4.5 to 4.5)
            if (pos.X < -halfRoom + padding) pos.X = -halfRoom + padding;
            if (pos.X > halfRoom - padding) pos.X = halfRoom - padding;
            if (pos.Z < -halfRoom + padding) pos.Z = -halfRoom + padding;
            if (pos.Z > halfRoom - padding) pos.Z = halfRoom - padding;

            float eyeHeight = 1.6f;
            float ceilingLimit = ROOM_SIZE - padding;

            if (pos.Y < eyeHeight) pos.Y = eyeHeight;
            if (pos.Y > ceilingLimit) pos.Y = ceilingLimit;

            camera.Position = pos;

            // Tab and E interaction logic is the same as the previous response
            if (ks.IsKeyPressed(Keys.Tab))
            {
                CursorState = CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;
                if (CursorState == CursorState.Normal) firstMove = true;
            }
            if (ks.IsKeyPressed(Keys.E))
            {
                lightOn = !lightOn;
                Console.WriteLine($"Light is now: {(lightOn ? "ON" : "OFF")}");
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            //  Only process rotation if the Right Mouse Button is down.
            if (MouseState.IsButtonDown(MouseButton.Right))
            {
                // Lock the cursor to the window only while holding RMB for rotation
                CursorState = CursorState.Grabbed;

                if (firstMove)
                {
                    firstMove = false;
                    return;
                }
                camera.ProcessMouseDelta(e.DeltaX, e.DeltaY);
            }
            else
            {
                // If RMB is not held, release the cursor and reset the flag.
                CursorState = CursorState.Normal;
                firstMove = true;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            roomShader.Use();

            // Setup Camera Matrices (Projection & View)
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), Size.X / (float)Size.Y, 0.1f, 100f);
            roomShader.SetMatrix4("projection", projection);
            roomShader.SetMatrix4("view", camera.GetViewMatrix());
            roomShader.SetVector3("viewPos", camera.Position);

            // Setup Lighting Uniforms 
           
            roomShader.SetFloat("shininess", 32.0f);
            roomShader.SetInt("lightOn", lightOn ? 1 : 0);
            roomShader.SetVector3("light.position", lightPos);

            if (lightOn)
            {
                roomShader.SetVector3("light.ambient", new Vector3(0.4f));
                roomShader.SetVector3("light.diffuse", new Vector3(0.5f));
                roomShader.SetVector3("light.specular", new Vector3(1.0f));
            }
            else
            {
                roomShader.SetVector3("light.ambient", new Vector3(0.05f));
                roomShader.SetVector3("light.diffuse", new Vector3(0.0f));
                roomShader.SetVector3("light.specular", new Vector3(0.0f));
            }

            // Render Objects (Geometry & Texturing)

            // floor (-Y plane, texture floor.png)
            floorTex.Use(TextureUnit.Texture0);
            var floorModel = Matrix4.CreateScale(ROOM_SIZE, 1f, ROOM_SIZE) * Matrix4.CreateTranslation(0, 0, 0); // Position Y=0
            roomShader.SetMatrix4("model", floorModel);
            floorCeilingMesh.Render();

            // ceiling (+Y plane, texture floor.png)
            // Rotate 180 degrees to flip the normal downward
            floorTex.Use(TextureUnit.Texture0);
            var ceilingModel = Matrix4.CreateScale(ROOM_SIZE, 1f, ROOM_SIZE) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(180f)) *
                               Matrix4.CreateTranslation(0, ROOM_SIZE, 0); // Position Y=10
            roomShader.SetMatrix4("model", ceilingModel);
            floorCeilingMesh.Render();

            // walls (+X, -X, +Z, -Z, texture wall.png)
            wallTex.Use(TextureUnit.Texture0);

            // Wall 1: Back Wall (-Z plane, Quad faces +Z)
            var backWallModel = Matrix4.CreateScale(ROOM_SIZE, ROOM_SIZE, 1f) * Matrix4.CreateTranslation(0, ROOM_SIZE / 2f, -ROOM_SIZE / 2f);
            roomShader.SetMatrix4("model", backWallModel);
            wallMesh.Render();

            // Wall 2: Front Wall (+Z plane, needs 180 rotation to face -Z)
            var frontWallModel = Matrix4.CreateScale(ROOM_SIZE, ROOM_SIZE, 1f) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(180f)) *
                                 Matrix4.CreateTranslation(0, ROOM_SIZE / 2f, ROOM_SIZE / 2f);
            roomShader.SetMatrix4("model", frontWallModel);
            wallMesh.Render();

            // Wall 3: Left Wall (-X plane, needs -90 rotation to face +X)
            var leftWallModel = Matrix4.CreateScale(ROOM_SIZE, ROOM_SIZE, 1f) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-90f)) *
                                 Matrix4.CreateTranslation(-ROOM_SIZE / 2f, ROOM_SIZE / 2f, 0);
            roomShader.SetMatrix4("model", leftWallModel);
            wallMesh.Render();

            // Wall 4: Right Wall (+X plane, needs +90 rotation to face -X)
            var rightWallModel = Matrix4.CreateScale(ROOM_SIZE, ROOM_SIZE, 1f) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90f)) *
                                 Matrix4.CreateTranslation(ROOM_SIZE / 2f, ROOM_SIZE / 2f, 0);
            roomShader.SetMatrix4("model", rightWallModel);
            wallMesh.Render();

            // object 2: Decorative Pillar (using decorativeMesh) 
            wallTex.Use(TextureUnit.Texture0); // Reuse wall texture
            var pillarModel = Matrix4.CreateScale(0.5f, 5f, 0.5f) *
                              Matrix4.CreateTranslation(3f, 2.5f, -3f);
            roomShader.SetMatrix4("model", pillarModel);
            decorativeMesh.Render();

            // object3: Interactable (Light Indicator) 
            //Rendering the light indicator is the same as the previous response
            var indicatorModel = Matrix4.CreateScale(0.3f) * Matrix4.CreateTranslation(lightPos);
            roomShader.SetMatrix4("model", indicatorModel);
            roomShader.SetVector3("light.ambient", lightOn ? new Vector3(0.0f, 0.2f, 0.0f) : new Vector3(0.2f, 0.0f, 0.0f));
            roomShader.SetVector3("light.diffuse", new Vector3(0f));
            decorativeMesh.Render();

            lightSourceShader.Use();
            lightSourceShader.SetMatrix4("projection", projection);
            lightSourceShader.SetMatrix4("view", camera.GetViewMatrix());
            lightSourceShader.SetMatrix4("model", indicatorModel);
            lightSourceShader.SetVector3("solidColor", lightOn ? new Vector3(1f, 1f, 0.9f) : new Vector3(0.2f, 0.2f, 0.2f));
            decorativeMesh.Render();

            SwapBuffers();
        }
    }
}
