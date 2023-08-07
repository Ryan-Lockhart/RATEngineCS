namespace rat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #if DEBUG
                Directory.SetCurrentDirectory("C:\\dev\\cs\\RATEngine");
            #endif

            Globals.Engine = new Engine();

            Globals.Engine.Initialize();

            while (Globals.Engine.Running)
            {
                Globals.Engine.CalculateDeltaTime();

                Globals.Engine.Input();
                Globals.Engine.Update();
                Globals.Engine.Render();
            }

            Globals.Engine.Close();
            NameGenerator.Globals.Basic.Save();
        }
    }
}
