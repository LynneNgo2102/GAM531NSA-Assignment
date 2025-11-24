
using OpenTK.Windowing.Desktop;


namespace BackRoomMap
{
    public static class Program
    {
        public static void Main()
        {
            var native = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "Assignment 9",
            };


            var game = new Game(GameWindowSettings.Default, native);
            game.Run();
        }
    }
}