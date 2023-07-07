using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;
using rat.Primitives;
using rat.Constants;
using Color = rat.Primitives.Color;

namespace rat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Raylib.InitWindow((int)Screens.WindowSize.width * (int)GlyphSets.DefaultMapGlyphs.glyphSize.width, (int)Screens.WindowSize.height * (int)GlyphSets.DefaultMapGlyphs.glyphSize.height, "Hello World");
            Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            GlyphSet glyphSet = new GlyphSet(GlyphSets.DefaultUIGlyphs);

            Point playerPosition = new Point(0, 0);

            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
                    playerPosition.y--;
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                    playerPosition.y++;
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_D))
                    playerPosition.x++;
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
                    playerPosition.x--;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Colors.Black);

                Raylib.DrawText(DateTime.Now.Ticks.ToString(), 12, 12, 20, Colors.White);

                glyphSet.DrawGlyph(playerPosition, Glyphs.ASCII.Player);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
