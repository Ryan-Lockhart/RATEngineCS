using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;

namespace rat
{
    namespace Constants
    {
        public class Settings
        {
            /// <summary>
            /// The maximum amount of messages that can be displayed in the message log
            /// </summary>
            public static readonly int MaxMessages = 21;

            /// <summary>
            /// Minimum time (in milliseconds) between AI updates when control flow is unblocked
            /// </summary>
            public static readonly ulong MinimumUpdateTime = 500;
            /// <summary>
            /// Minimum time (in milliseconds) between summon wave checks
            /// </summary>
            public static readonly ulong MinimumSummonTime = 30000;

            public static readonly string WindowTitle = "Dungeon Sandbox (RATEngineCS) v0.001 07/09/2023";

            public static readonly Bounds MapSize = new Bounds(256, 256, 1);

            /// <summary>
            /// Allows the controlled actor to occupy solid cells
            /// </summary>
            public static readonly bool AllowNoclip = false;
            /// <summary>
            /// Allows the resurrection of deceased actors
            /// </summary>
            public static readonly bool AllowResurrection = false;

            /// <summary>
            /// The number of tiles the "camera" will traverse with unlocked camera movement
            /// </summary>
            public static readonly int CameraSpeed = 5;

            public static bool UseCorpseLimit = true;
            public static int CorpseLimit = 1;

            /// <summary>
            /// The number of enemies that are available for generation
            /// </summary>
            public static readonly int MaximumEnemyTypes = 6;

            public static readonly int MinimumEnemies = 10;
            public static readonly int MaximumEnemies = 50;

            /// <summary>
            /// The eight octants used for shadowcasting
            /// </summary>
            public static readonly Octant[] Octants =
            {
                new Octant(0, 1, 1, 0),
                new Octant(1, 0, 0, 1),
                new Octant(0, -1, 1, 0),
                new Octant(-1, 0, 0, 1),
                new Octant(0, -1, -1, 0),
                new Octant(-1, 0, 0, -1),
                new Octant(0, 1, -1, 0),
                new Octant(1, 0, 0, -1)
            };
        }

        public class GlyphSets
        {
            public static readonly GlyphSetInfo DefaultUIGlyphs = new GlyphSetInfo("Assets\\Glyphs\\glyphs_12x12.png", new Size(12, 12), new Size(16, 16));
            public static readonly GlyphSetInfo DefaultMapGlyphs = new GlyphSetInfo("Assets\\Tilesets\\tileset_16x16.png", new Size(16, 16), new Size(16, 16));
        }

        public class Screens
        {
            /// <summary>
            /// Title Bar size in UI coordinate system
            /// </summary>
            public static readonly Rect TitleBar = new Rect(new Point(0, 0), new Size(128, 3));

            /// <summary>
            /// Footer Bar size in Map coordinate system
            /// </summary>
            public static readonly Rect MapDisplay = new Rect(new Point(0, 3), new Size(96, 42));

            /// <summary>
            /// Footer Bar size in UI coordinate system
            /// </summary>
            public static readonly Rect MessageDisplay = new Rect(new Point(80, 8), new Size(48, 48));

            /// <summary>
            /// Footer Bar size in UI coordinate system
            /// </summary>
            public static readonly Rect LeftSideBar = new Rect(new Point(0, 3), new Size(3, 64));

            /// <summary>
            /// Footer Bar size in UI coordinate system
            /// </summary>
            public static readonly Rect FooterBar = new Rect(new Point(0, 64), new Size(128, 3));

            /// <summary>
            /// Window size in Map coordinate system
            /// </summary>
            public static readonly Size WindowSize = new Size(96, 48);

            private static readonly double _UIToMap = 16.0 / 12.0;
            private static readonly double _MapToUI = 12.0 / 16.0;

            public static Point UIToMap(in Point position) => new Point((long)(position.x * _UIToMap), (long)(position.y * _UIToMap));
            public static Point MapToUI(in Point position) => new Point((long)(position.x * _MapToUI), (long)(position.y * _MapToUI));

            public static Size UIToMap(in Size size) => new Size((uint)(size.width * _UIToMap), (uint)(size.height * _UIToMap));
            public static Size MapToUI(in Size size) => new Size((uint)(size.width * _MapToUI), (uint)(size.height * _MapToUI));
        }

        /// <summary>
        /// An organizational class containitng constant indexes for use with glyphs
        /// </summary>
	    public class Characters
        {
            public static readonly byte Error = 0x58;

            public static readonly byte Empty = 0x00;
            public static readonly byte Wall = 0x4F;
            public static readonly byte Obstacle = 0x4E;
            public static readonly byte Floor = 0x4D;

            public static readonly byte Entity = 0x40;
            public static readonly byte Medkit = 0x49;
            public static readonly byte Glock = 0x4A;
            public static readonly byte Ladder = 0x4B;
            public static readonly byte Corpse = 0x4C;
        }

        /// <summary>
        /// An organizational class containitng constant colors 
        /// </summary>
        public class Colors
        {
            public static readonly Color Transperant = new Color(0, 0, 0, 0);

            public static readonly Color White = new Color(255, 255, 255, 255);
            public static readonly Color Black = new Color(0, 0, 0, 255);

            public static readonly Color LightGrey = new Color(192, 192, 192, 255);
            public static readonly Color Grey = new Color(128, 128, 128, 255);
            public static readonly Color DarkGrey = new Color(64, 64, 64, 255);

            public static readonly Color Marble = new Color(240, 232, 232, 255);
            public static readonly Color DarkMarble = new Color(200, 192, 192, 255);

            public static readonly Color LightIntrite = new Color(132, 124, 124, 255);
            public static readonly Color Intrite = new Color(112, 104, 104, 255);

            public static readonly Color LightCharcoal = new Color(60, 58, 58, 255);
            public static readonly Color Charcoal = new Color(40, 32, 32, 255);

            public static readonly Color BrightRed = new Color(255, 0, 0, 255);
            public static readonly Color LightRed = new Color(192, 0, 0, 255);
            public static readonly Color DarkRed = new Color(128, 0, 0, 255);

            public static readonly Color BrightGreen = new Color(0, 255, 0, 255);
            public static readonly Color LightGreen = new Color(0, 192, 0, 255);
            public static readonly Color DarkGreen = new Color(0, 128, 0, 255);

            public static readonly Color BrightBlue = new Color(0, 0, 255, 255);
            public static readonly Color LightBlue = new Color(0, 0, 192, 255);
            public static readonly Color DarkBlue = new Color(0, 0, 128, 255);

            public static readonly Color BrightCyan = new Color(0, 255, 255, 255);
            public static readonly Color LightCyan = new Color(0, 192, 192, 255);
            public static readonly Color DarkCyan = new Color(0, 128, 128, 255);

            public static readonly Color BrightMagenta = new Color(255, 0, 255, 255);
            public static readonly Color LightMagenta = new Color(192, 0, 192, 255);
            public static readonly Color DarkMagenta = new Color(128, 0, 128, 255);

            public static readonly Color BrightYellow = new Color(255, 255, 0, 255);
            public static readonly Color LightYellow = new Color(192, 192, 0, 255);
            public static readonly Color DarkYellow = new Color(128, 128, 0, 255);

            public static readonly Color BrightOrange = new Color(255, 94, 5, 255);
            public static readonly Color LightOrange = new Color(255, 165, 115, 255);
            public static readonly Color DarkOrange = new Color(200, 71, 0, 255);

            /// <summary>
            /// Constant colors that represent various real world materials
            /// </summary>
            public class Materials
            {
                public static readonly Color Blood = new Color(157, 34, 53, 255);
                public static readonly Color DarkBlood = new Color(137, 14, 33, 255);

                public static readonly Color Ebony = new Color(40, 44, 52, 255);
                public static readonly Color Ivory = new Color(255, 255, 240, 255);

                public static readonly Color Oak = new Color(120, 81, 45, 255);
                public static readonly Color Willow = new Color(168, 172, 155, 255);
                public static readonly Color Birch = new Color(234, 225, 216, 255);
            }

            /// <summary>
            /// Constant colors that represent various real world metals
            /// </summary>
            public class Metals
            {
                public static readonly Color Iron = new Color(58, 60, 64, 255);
                public static readonly Color Steel = new Color(113, 121, 126, 255);

                public static readonly Color Gold = new Color(255, 215, 0, 255);
                public static readonly Color Silver = new Color(192, 192, 192, 255);
                public static readonly Color Copper = new Color(184, 115, 51, 255);

                public static readonly Color Tin = new Color(145, 145, 145, 255);
                public static readonly Color Bronze = new Color(205, 127, 50, 255);

                public static readonly Color Zinc = new Color(186, 196, 200, 255);
                public static readonly Color Brass = new Color(181, 166, 66, 255);
            }
        }

        /// <summary>
        /// An organizational class containing constant glyphs for use with glyph sets
        /// </summary>
	    public class Glyphs
        {
            /// <summary>
            /// These glyphs are for use with a 256 character ASCII glyph set
            /// </summary>
            public class ASCII
            {
                public static readonly Glyph Empty = new Glyph(Characters.Empty, Colors.Transperant);

                public static Glyph GetGlyph(bool solid, bool seen, bool bloody)
                {
                    Glyph glyph = Empty;

                    glyph.index = solid ? Characters.Wall : Characters.Floor;

                    if (bloody)
                        glyph.color = seen ? Colors.Materials.Blood : Colors.Materials.DarkBlood;
                    else glyph.color = seen ? (solid ? Colors.Marble : Colors.LightCharcoal) : (solid ? Colors.DarkMarble : Colors.Charcoal);

                    return glyph;
                }

                public static readonly Glyph Error = new Glyph(Characters.Error, Colors.BrightRed);

                public static readonly Glyph Wall = new Glyph(Characters.Wall, Colors.Marble);
                public static readonly Glyph Floor = new Glyph(Characters.Floor, Colors.LightCharcoal);
                public static readonly Glyph Obstacle = new Glyph(Characters.Obstacle, Colors.LightIntrite);

                public static readonly Glyph Player = new Glyph(Characters.Entity, Colors.BrightGreen);
                public static readonly Glyph Enemy = new Glyph(Characters.Entity, Colors.BrightRed);
                public static readonly Glyph Ally = new Glyph(Characters.Entity, Colors.BrightCyan);
                public static readonly Glyph Neutral = new Glyph(Characters.Entity, Colors.BrightYellow);
            }

            /// <summary>
            /// These glyphs are for use with the Battle Graphics glyph set
            /// </summary>
            public class Battle
            {

            }
        }

	    /// <summary>
        /// An organizaitonal class containing the nine possible constant text alignments
        /// </summary>
	    public class Alignments
	    {
            public static readonly TextAlignment Centered = new TextAlignment(VerticalAlignment.Center, HorizontalAlignment.Center);
            public static readonly TextAlignment LeftCentered = new TextAlignment(VerticalAlignment.Center, HorizontalAlignment.Left);
            public static readonly TextAlignment RightCentered = new TextAlignment(VerticalAlignment.Center, HorizontalAlignment.Right);

            public static readonly TextAlignment UpperCentered = new TextAlignment(VerticalAlignment.Upper, HorizontalAlignment.Center);
            public static readonly TextAlignment UpperLeft = new TextAlignment(VerticalAlignment.Upper, HorizontalAlignment.Left);
            public static readonly TextAlignment UpperRight = new TextAlignment(VerticalAlignment.Upper, HorizontalAlignment.Right);

            public static readonly TextAlignment LowerCentered = new TextAlignment(VerticalAlignment.Lower, HorizontalAlignment.Center);
            public static readonly TextAlignment LowerLeft = new TextAlignment(VerticalAlignment.Lower, HorizontalAlignment.Left);
            public static readonly TextAlignment LowerRight = new TextAlignment(VerticalAlignment.Lower, HorizontalAlignment.Right);
	    }
    }
}