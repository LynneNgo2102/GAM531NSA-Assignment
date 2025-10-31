
using OpenTK.Windowing.Desktop;


namespace EndlessHallway
{
    public static class Program
    {
        public static void Main()
        {
            var native = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "Endless Hallway - Midterm",
            };


            var game = new Game(GameWindowSettings.Default, native);
            game.Run();
        }
    }
}