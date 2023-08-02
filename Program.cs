namespace rat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Globals.Engine = new Engine(1337);

            Globals.Engine.Initialize();

            while (Globals.Engine.Running)
            {
                Globals.Engine.CalculateDeltaTime();

                Globals.Engine.Input();
                Globals.Engine.Update();
                Globals.Engine.Render();
            }

            Globals.Engine.Close();
        }
    }
}
