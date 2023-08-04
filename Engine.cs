using Raylib_cs;

using rat.Primitives;
using rat.Constants;

using Point = rat.Primitives.Point;
using Size = rat.Primitives.Size;
using Color = rat.Primitives.Color;

namespace rat
{
    public class Engine
    {
        private int m_FPS;

        private bool m_Fullscreen;

        private bool m_Exit = false;
        public bool Running => !m_Exit;
        public bool ShouldExit => m_Exit;

        private GlyphSet m_GameSet;
        public GlyphSet GameSet => m_GameSet;

        private GlyphSet m_UISet;
        public GlyphSet UISet => m_UISet;

        private Map m_Map;
        public Map Map { get => m_Map; set => m_Map = value; }

        private Cursor m_Cursor;
        public Cursor Cursor { get => m_Cursor; set => m_Cursor = value; }

        private Actor m_Player;
        public Actor Player { get => m_Player; set => m_Player = value; }

        private Population m_Population;
        public Population Population => m_Population;

        private MapScreen m_MapScreen;
        public MapScreen MapScreen => m_MapScreen;

        private MessageScreen m_MessageScreen;
        public MessageScreen MessageScreen => m_MessageScreen;

        private bool m_PlayerActed;

        private bool m_ShowLog;
        private bool m_ShowControls;

        private bool m_ActionSelect;
        private Action m_CurrentAction;

        private DateTime m_LastInputTime;
        private DateTime m_LastUpdateTime;
        private int m_LastSummonTurn;

        private DateTime m_ThisFrame = DateTime.Now;
        private DateTime m_LastFrame = DateTime.Now;

        private TimeSpan m_DeltaTime;

        private List<ConfigFlags> m_CurrentFlags;

        public void RecalculateFlags()
        {
            var flags = m_CurrentFlags
                .Cast<ConfigFlags>()
                .Aggregate((ConfigFlags)0, (s, f) => s | f);

            Raylib.SetWindowState(flags);
        }

        public void AddFlag(in ConfigFlags flag)
        {
            m_CurrentFlags.Add(flag);
            RecalculateFlags();
        }

        public void RemoveFlag(in ConfigFlags flag)
        {
            m_CurrentFlags.Remove(flag);
            RecalculateFlags();
        }

        public Engine(int seed)
        {
            Globals.Reseed(seed);
        }

        public void CalculateDeltaTime()
        {
            m_LastFrame = m_ThisFrame;
            m_ThisFrame = DateTime.Now;
            m_DeltaTime = m_ThisFrame - m_LastFrame;

            if (m_DeltaTime.Milliseconds != 0)
                m_FPS = 1000 / m_DeltaTime.Milliseconds;
        }

        public void ToggleFullscreen()
        {
            m_Fullscreen = !m_Fullscreen;

            if (m_Fullscreen)
            {
                if (!m_CurrentFlags.Contains(ConfigFlags.FLAG_FULLSCREEN_MODE))
                    AddFlag(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
            else
            {
                if (m_CurrentFlags.Contains(ConfigFlags.FLAG_FULLSCREEN_MODE))
                    RemoveFlag(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
        }

        public void SetLastInput() => m_LastInputTime = DateTime.Now;
        public bool AllowInput => (DateTime.Now - m_LastInputTime).Milliseconds > Settings.MinimumInputTime;

        public void SetLastUpdate() => m_LastUpdateTime = DateTime.Now;
        public bool AllowUpdate => (DateTime.Now - m_LastUpdateTime).Milliseconds > Settings.MinimumUpdateTime;

        public void SetLastSummon() => m_LastSummonTurn = Globals.CurrentTurn;
        public bool AllowSummon => Globals.CurrentTurn - m_LastSummonTurn > Settings.MinimumSummonTime;

        public int RandomSummonAmount => Globals.Generator.Next(Settings.Population.MinimumSummonEnemies, Settings.Population.MaximumSummonEnemies);

        public int MaximumSummons => Settings.Population.MaximumTotalEnemies - Population.TotalActors;

        public int NextSummonCount => System.Math.Min(RandomSummonAmount, MaximumSummons);

        public virtual void Initialize()
        {
            Size windowSize = Screens.WindowSize * GlyphSets.DefaultMapGlyphs.glyphSize;

            Raylib.InitWindow(windowSize.width, windowSize.height, "RATEngine");

            m_CurrentFlags = new List<ConfigFlags>
            {
                ConfigFlags.FLAG_WINDOW_ALWAYS_RUN,
                ConfigFlags.FLAG_WINDOW_UNDECORATED,
                //ConfigFlags.FLAG_WINDOW_TOPMOST,
                ConfigFlags.FLAG_VSYNC_HINT
            };

            RecalculateFlags();

            Raylib.DisableCursor();

            m_UISet = new GlyphSet(GlyphSets.DefaultUIGlyphs);
            m_GameSet = new GlyphSet(GlyphSets.DefaultMapGlyphs);

            m_Map = new Map(Settings.MapSize, Settings.BorderSize);

            m_Map.Generate(Settings.MapGeneration.TunnelHeavy.fillPercent);
            m_Map.Smooth(Settings.MapGeneration.TunnelHeavy.iterations, Settings.MapGeneration.TunnelHeavy.threshold);

            m_Map.Populate();

            m_Map.CloseHoles();

            m_Cursor = new Cursor(Point.Zero, Size.One, Map);

            m_MapScreen = new MapScreen("Map Screen", Screens.MapDisplay, Map, Cursor);
            m_MessageScreen = new MessageScreen("Message Screen", new Rect(Screens.MessagesTooltip, Size.Zero), Cursor);

            m_Player = new Actor
            (
                Map, "Jenkins", "A spry lad clad in armor and blade",
                Glyphs.ASCII.Player,
                1, 10.0f, 5.0f, 7.5f, 0.50f, 0.75f, false, false
            );

            int totalEnemies = Globals.Generator.Next(Settings.Population.MinimumInitialEnemies, Settings.Population.MaximumInitialEnemies);

            m_Population = new Population(Player, totalEnemies);
        }

        public virtual void Input()
        {
            bool heldInput = AllowInput;

            if (Cursor.Input())
                return;

            if (MapScreen.Input())
                return;

            if (MessageScreen.Input())
                return;

            if (m_ActionSelect)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))
                {
                    m_CurrentAction = Action.MoveTo;
                    m_ActionSelect = false;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_TWO))
                {
                    m_CurrentAction = Action.LookAt;
                    m_ActionSelect = false;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_THREE))
                {
                    m_CurrentAction = Action.Attack;
                    m_ActionSelect = false;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_FOUR))
                {
                    m_CurrentAction = Action.Push;
                    m_ActionSelect = false;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_FIVE))
                {
                    m_CurrentAction = Action.Mine;
                    m_ActionSelect = false;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) || Raylib.IsKeyPressed(KeyboardKey.KEY_TAB))
                {
                    m_CurrentAction = Action.None;
                    m_ActionSelect = false;
                }
            }
            else
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
                {
                    if (!m_ActionSelect)
                        m_Exit = true;
                    else
                    {
                        m_CurrentAction = Action.None;
                        m_ActionSelect = false;
                    }
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_TAB))
                {
                    if (!m_ActionSelect)
                    {
                        m_ActionSelect = true;
                        m_CurrentAction = Action.None;
                    }
                    else m_ActionSelect = false;
                }
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F1))
            {
                m_ShowControls = !m_ShowControls;
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_F2))
            {
                ToggleFullscreen();
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_F3))
            {
                m_Map.RevealMap(true);
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_F4))
            {
                m_Map.RecalculateIndices();
            }

            if (m_Player.Alive && !m_PlayerActed)
            {
                m_PlayerActed = true;

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
                {
                    m_Player.Stance = Stance.Prone;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_X))
                {
                    m_Player.Stance = Stance.Erect;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
                {
                    m_Player.Stance = Stance.Crouch;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
                {
                    if (m_Cursor.Cell != null)
                        m_Player.Act(m_Cursor.Cell.Position, Action.LookAt, false);
                }
                else
                {
                    if (heldInput)
                    {
                        int x_input = 0;
                        int y_input = 0;

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_D) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_6) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_3) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_9)) { x_input = 1; }
                        else if (Raylib.IsKeyDown(KeyboardKey.KEY_A) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_4) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_7) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_1)) { x_input = -1; }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_S) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_2) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_3) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_1)) { y_input = 1; }
                        else if (Raylib.IsKeyDown(KeyboardKey.KEY_W) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_8) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_9) || Raylib.IsKeyDown(KeyboardKey.KEY_KP_7)) { y_input = -1; }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_5))
                        {
                            m_Player.Act(Point.Zero, Action.Wait, true);
                            m_CurrentAction = Action.None;

                            SetLastInput();
                            return;
                        }
                        else if (x_input != 0 || y_input != 0)
                        {
                            Coord actPosition = new Coord(x_input, y_input, 0);

                            if (m_CurrentAction != Action.None)
                            {
                                m_Player.Act(actPosition, m_CurrentAction, true);
                                m_CurrentAction = Action.None;
                            }
                            else m_Player.Act(actPosition, true);

                            if (Settings.CursorLook && m_Cursor.Cell != null) m_Player.Act(m_Cursor.Cell.Position, Action.LookAt, false);

                            SetLastInput();
                            return;
                        }
                        else m_PlayerActed = false;
                    }
                    else m_PlayerActed = false;
                }
            }
        }

		public virtual void Update()
        {
            Cursor.Update(MapScreen.Position, m_GameSet.GlyphSize);

            MapScreen.Update();

            if (m_Player.Alive)
            {
                double view_distance = 0.0;
                double view_span = 0.0;

                switch (m_Player.Stance)
                {
                    case Stance.Erect:
                        view_distance = 16.0;
                        view_span = 135.0;
                        break;
                    case Stance.Crouch:
                        view_distance = 8.0;
                        view_span = 360.0;
                        break;
                    case Stance.Prone:
                        view_distance = 32.0;
                        view_span = 45;
                        break;
                }

                var fov = m_Map.CalculateFOV(m_Player.Position, view_distance, m_Player.Angle.Degrees, view_span);

                foreach (var pos in fov)
                {
                    var cell = m_Map[pos];
                    if (cell == null) continue;

                    foreach (var corpse in cell.Corpses)
                        if (corpse != null)
                            corpse.Observed = true;
                }

                m_Map.RevealMap(fov);
            }
            else if (m_Player.Dead)
            {
                if (AllowUpdate)
                {
                    m_PlayerActed = true;

                    m_Map.RevealMap();
                }
            }

            if (m_PlayerActed)
            {
                Population.CollectDead();

                foreach (var living_actor in Population.Living)
                    if (living_actor != null)
                        living_actor.Update(this);

                m_PlayerActed = false;

                SetLastUpdate();

                Globals.CurrentTurn++;

                if (AllowSummon)
                {
                    var summonCount = NextSummonCount;

                    if (summonCount > 0) Population.SummonEnemies(summonCount);
                }
            }

            m_MessageScreen.Update();
        }

        public virtual void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Colors.Black);

            MapScreen.Draw(GameSet);
            MessageScreen.Draw(UISet);
            m_Cursor.Draw(Map, GameSet);

            string text =
                $"{(m_Player != null ? m_Player.Name : "???")}:\n" +
                $"{(m_Player != null ? m_Player.Position : "???")}:\n" +
                $"Health: {(m_Player != null ? (int)System.Math.Ceiling(m_Player.CurrentHealth) : "(?.?)")}/{(m_Player != null ? (int)System.Math.Ceiling(m_Player.MaxHealth) : "(?.?)")}\n" +
                $"Stance: {(m_Player != null ? m_Player.Stance : "???")}";

            DrawLabel(text, Screens.InfoToolip, Size.One, Alignments.UpperLeft, Colors.White);

            DrawLabel($"FPS: {m_FPS}", Screens.FooterBar.position, Size.One, Alignments.LowerLeft, Colors.White);

            DrawFixedLabel(Settings.WindowTitle, Screens.TitleBar, Size.One, Alignments.Centered, Colors.White);

            if (m_ShowControls)
                DrawLabel(Text.Controls, Screens.ControlsTooltip, Size.One, Alignments.UpperRight, Colors.White);
            DrawLabel("    F1: Controls    ", Screens.ControlsTooltip, Size.One, Alignments.UpperRight, Colors.White);

            if (m_ActionSelect)
                DrawLabel(Text.Actions, Screens.LeftSideBar.position + new Point(0, Screens.LeftSideBar.size.height / 2), Size.One, Alignments.LeftCentered, Colors.White);

            Raylib.EndDrawing();
        }

        public virtual void Close() => Raylib.CloseWindow();

        public void DrawRect(in Rect transform, in Color color, in Size scale_by, bool fill = false)
        {
            Rect rec = new Rect(transform.position * scale_by, transform.size * scale_by);

            if (fill) Raylib.DrawRectangleRec(rec, color);
            else Raylib.DrawRectangleLinesEx(rec, 1.0f, color);
        }

        public void DrawRect(in Point position, in Size size, in Color color, in Size scale_by, bool fill = false)
        {
            Rect rec = new Rect(position * scale_by, size * scale_by);

            if (fill) Raylib.DrawRectangleRec(rec, color);
            else Raylib.DrawRectangleLinesEx(rec, 1.0f, color);
        }

        public void DrawRect(int x, int y, int width, int height, in Color color, in Size scale_by, bool fill = false)
        {
            if (fill) Raylib.DrawRectangle(x * (int)scale_by.width, y * (int)scale_by.height, width * (int)scale_by.width, height * (int)scale_by.height, color);
            else Raylib.DrawRectangleLines(x * (int)scale_by.width, y * (int)scale_by.height, width * (int)scale_by.width, height * (int)scale_by.height, color);
        }

        public void DrawText(in string text, in Point position, in TextAlignment alignment, in Color color)
        {
            if (text == "") return;

            int numLines = text == "" ? 0 : 1;

            int maxWidth = 0;
            int currWidth = 0;

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\n':
                        numLines++;
                        maxWidth = System.Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    case '\t':
                        currWidth += currWidth % 4 > 0 ? currWidth % 4 : 4;
                        break;
                    case '\v':
                        numLines += numLines % 4 > 0 ? numLines % 4 : 4;
                        numLines++;
                        maxWidth = System.Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    default:
                        currWidth++;
                        break;
                }
            }

            maxWidth = System.Math.Max(maxWidth, currWidth);

            Point startPosition = position;

            if (alignment.horizontal == HorizontalAlignment.Center) startPosition.x -= maxWidth / 2;
            else if (alignment.horizontal == HorizontalAlignment.Right) startPosition.x -= maxWidth;

            if (alignment.vertical == VerticalAlignment.Center) startPosition.y -= numLines / 2;
            else if (alignment.vertical == VerticalAlignment.Lower) startPosition.y -= numLines - 1;

            Point carriagePosition = startPosition;

            foreach (var c in text)
            {
                switch (c)
                {
                    case ' ':
                        carriagePosition.x++;
                        break;
                    case '\n':
                        carriagePosition.y++;
                        carriagePosition.x = startPosition.x;
                        break;
                    case '\t':
                        carriagePosition.x += carriagePosition.x % 4 > 0 ? carriagePosition.x % 4 : 4;
                        break;
                    case '\v':
                        carriagePosition.y += carriagePosition.y % 4 > 0 ? carriagePosition.y % 4 : 4;
                        carriagePosition.x = startPosition.x;
                        break;
                    default:
                        DrawRect(carriagePosition, Size.One, Colors.Black, UISet.GlyphSize);
                        UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

		public void DrawLabel(in string text, in Point position, in Size padding, in TextAlignment alignment, in Color color)
        {
            if (text == "") return;

            int numLines = text == "" ? 0 : 1;

            int maxWidth = 0;
            int currWidth = 0;

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\n':
                        numLines++;
                        maxWidth = System.Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    case '\t':
                        currWidth += currWidth % 4 > 0 ? currWidth % 4 : 4;
                        break;
                    case '\v':
                        numLines += numLines % 4 > 0 ? numLines % 4 : 4;
                        numLines++;
                        maxWidth = System.Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    default:
                        currWidth++;
                        break;
                }
            }

            maxWidth = System.Math.Max(maxWidth, currWidth);

            Size labelSize = new Size(maxWidth + (padding.width * 2), numLines + (padding.height * 2));

            Point startPosition = position;

            if (alignment.horizontal == HorizontalAlignment.Center) startPosition.x -= labelSize.width / 2;
            else if (alignment.horizontal == HorizontalAlignment.Right) startPosition.x -= labelSize.width;

            if (alignment.vertical == VerticalAlignment.Center) startPosition.y -= labelSize.height / 2;
            else if (alignment.vertical == VerticalAlignment.Lower) startPosition.y -= labelSize.height;

            DrawRect(startPosition, labelSize, Colors.Black, UISet.GlyphSize, true);

            DrawRect(startPosition, labelSize, color, UISet.GlyphSize, false);

            startPosition += padding;

            Point carriagePosition = startPosition;

            foreach (var c in text)
            {
                switch (c)
                {
                    case ' ':
                        carriagePosition.x++;
                        break;
                    case '\n':
                        carriagePosition.y++;
                        carriagePosition.x = startPosition.x;
                        break;
                    case '\t':
                        carriagePosition.x += carriagePosition.x % 4 > 0 ? carriagePosition.x % 4 : 4;
                        break;
                    case '\v':
                        carriagePosition.y += carriagePosition.y % 4 > 0 ? carriagePosition.y % 4 : 4;
                        carriagePosition.x = startPosition.x;
                        break;
                    default:
                        DrawRect(carriagePosition, Size.One, Colors.Black, UISet.GlyphSize);
                        UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

		public void DrawFixedLabel(in string text, in Rect rect, in Size padding, in TextAlignment alignment, in Color color)
        {
            if (text == "") return;

            DrawRect(rect, Colors.Black, UISet.GlyphSize, true);

            DrawRect(rect, color, UISet.GlyphSize, false);

            int numLines = text == "" ? 0 : 1;

            int maxWidth = 0;
            int currWidth = 0;

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\n':
                        numLines++;
                        maxWidth = System.Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    case '\t':
                        currWidth += currWidth % 4 > 0 ? currWidth % 4 : 4;
                        break;
                    case '\v':
                        numLines += numLines % 4 > 0 ? numLines % 4 : 4;
                        numLines++;
                        maxWidth = System.Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    default:
                        currWidth++;
                        break;
                }
            }

            maxWidth = System.Math.Max(maxWidth, currWidth);

            Size labelSize = new Size(maxWidth + (padding.width * 2), numLines + (padding.height * 2));
            Size delta = rect.size - labelSize;

            Point startPosition = rect.position;

            if (alignment.horizontal == HorizontalAlignment.Center) startPosition.x += delta.width / 2;
            else if (alignment.horizontal == HorizontalAlignment.Right) startPosition.x += delta.width;

            if (alignment.vertical == VerticalAlignment.Center) startPosition.y += delta.height / 2;
            else if (alignment.vertical == VerticalAlignment.Lower) startPosition.y += delta.height;

            startPosition += padding;

            Point carriagePosition = startPosition;

            foreach (var c in text)
            {
                switch (c)
                {
                    case ' ':
                        carriagePosition.x++;
                        break;
                    case '\n':
                        carriagePosition.y++;
                        carriagePosition.x = startPosition.x;
                        break;
                    case '\t':
                        carriagePosition.x += carriagePosition.x % 4 > 0 ? carriagePosition.x % 4 : 4;
                        break;
                    case '\v':
                        carriagePosition.y += carriagePosition.y % 4 > 0 ? carriagePosition.y % 4 : 4;
                        carriagePosition.x = startPosition.x;
                        break;
                    default:
                        DrawRect(carriagePosition, Size.One, Colors.Black, UISet.GlyphSize);
                        UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }
    }
}