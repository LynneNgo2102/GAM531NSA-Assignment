// Program.cs
using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

// Replace this with the actual namespace containing your Game class.
// In the earlier snippets I used `GameEngine`.
using GameEngine;

namespace MyOpenTKGame
{
    public static class Program
    {
        /// <summary>
        /// Entry point. Accepts optional command-line args:
        ///   --width <int>    : window width (default 1280)
        ///   --height <int>   : window height (default 720)
        ///   --fullscreen     : start fullscreen
        /// Example: dotnet run -- --width 1366 --height 768 --fullscreen
        /// </summary>
        public static void Main(string[] args)
        {
            // Default window size
            int width = 1280;
            int height = 720;
            bool fullscreen = false;

            // Parse simple CLI args (robust enough for testing)
            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i].ToLowerInvariant();
                if (a == "--width" && i + 1 < args.Length && int.TryParse(args[i + 1], out var w))
                {
                    width = w;
                    i++;
                }
                else if (a == "--height" && i + 1 < args.Length && int.TryParse(args[i + 1], out var h))
                {
                    height = h;
                    i++;
                }
                else if (a == "--fullscreen")
                {
                    fullscreen = true;
                }
            }

            // Basic OpenTK window settings
            var gameWindowSettings = new GameWindowSettings
            {
                RenderFrequency = 0, // 0 = unlimited (use VSync or control manually)
                UpdateFrequency = 0  // 0 = same as render
            };

            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = new Vector2i(width, height),
                Title = "MyOpenTKGame - Collision Assignment",
                // If you have an icon file, set it here: Icon = ...
                // If using Mac or other platforms, additional settings might be needed
                APIVersion = new Version(3, 3), // OpenGL 3.3 core
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible,
                WindowState = fullscreen ? WindowState.Fullscreen : WindowState.Normal,
            };

            try
            {
                // Create and run your Game (assumes a Game class derived from GameWindow)
                // If your Game class namespace differs, update the 'using' above or fully qualify here.
                using (var game = new Game(gameWindowSettings, nativeWindowSettings))
                {
                    // Set VSync to reduce unnecessary CPU usage and limit framerate
                    game.VSync = VSyncMode.On;

                    // Run the game: this enters the update-render loop.
                    // argument is target update rate (0 -> let OS schedule)
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                // Helpful runtime debug output
                Console.WriteLine("Unhandled exception while running the game:");
                Console.WriteLine(ex);
                Environment.Exit(-1);
            }
        }
    }
}
