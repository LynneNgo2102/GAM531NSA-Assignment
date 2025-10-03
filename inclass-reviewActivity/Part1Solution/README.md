# OpenTKReview

A minimal OpenTK 4.x project in C# (.NET 8) designed for learning 3D graphics programming. This repository provides a barebones setup that opens an 800x600 window with a black background, ready for students to extend with OpenGL rendering tasks, such as drawing shapes, gradients, or 3D scenes. Ideal for beginners exploring graphics APIs, it serves as a foundation for exercises in interactive 3D applications, inspired by classic graphics tutorials.

## Features
- Simple window setup using OpenTK's `GameWindow`.
- Closes on ESC key press.
- Lightweight and clean codebase, perfect for educational use.

## Setup
1. Clone the repository: `git clone https://github.com/yourusername/OpenTKReview.git`
2. Open in Visual Studio 2022 and ensure .NET 8 is installed.
3. Install OpenTK: `Install-Package OpenTK` via NuGet Package Manager.
4. Build and run to see an 800x600 window.

## Usage
- Extend `Game.cs` by overriding `OnRenderFrame` to add OpenGL rendering (e.g., draw a square or 3D landscape).
- Use as a starting point for graphics programming assignments, such as those involving pixel manipulation, coordinate systems, or shaders.

## Contributing
Feel free to fork, experiment, and submit pull requests with improvements or new features!

## License
MIT License - free to use, modify, and share.