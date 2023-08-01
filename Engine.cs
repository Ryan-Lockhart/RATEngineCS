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
        private bool m_Locked = true;
        private bool m_Exit = false;

        private bool m_PlayerActed;

        private bool m_ShowLog;
        private bool m_ShowControls;

        private bool m_ActionSelect;
        private Action m_CurrentAction;

        private DateTime m_LastInputTime;
        private DateTime m_LastUpdateTime;
        private DateTime m_LastSummonTime;

        private DateTime m_ThisFrame = DateTime.Now;
        private DateTime m_LastFrame = DateTime.Now;

        private TimeSpan m_DeltaTime;

        private List<ConfigFlags> m_CurrentFlags;

        /*
        private Point m_PathStart;
        private Point m_PathEnd;

        private bool m_PathStarted;

        private List<Point> m_Path = new List<Point>();

        private bool HasPath => m_Path.Count > 0;

        private void StartPath()
        {
            if (HasPath) m_Path.Clear();

            m_PathStart = m_Cursor.Transform.position;
            m_PathStarted = true;
        }

        private void EndPath()
        {
            m_PathEnd = m_Cursor.Transform.position;

            var path = m_Map.CalculatePath(m_PathStart, m_PathEnd);

            if (path == null)
            {
                m_PathStarted = false;
                return;
            }

            while (path.Count > 0) m_Path.Add(path.Pop());
            m_PathStarted = false;
        }

        private void DrawPath()
        {
            if (!HasPath) return;

            foreach (Point point in m_Path)
                DrawRect((point - m_Map.Position + Screens.MapDisplay.position) * m_GameSet.GlyphSize, m_GameSet.GlyphSize, Colors.BrightBlue, Size.One, true);

            DrawRect((m_PathStart - m_Map.Position + Screens.MapDisplay.position) * m_GameSet.GlyphSize, m_GameSet.GlyphSize, Colors.BrightGreen, Size.One, true);
            DrawRect((m_PathEnd - m_Map.Position + Screens.MapDisplay.position) * m_GameSet.GlyphSize, m_GameSet.GlyphSize, Colors.BrightRed, Size.One, true);
        }
        */

        private void RecalculateFlags()
        {
            var flags = m_CurrentFlags
                .Cast<ConfigFlags>()
                .Aggregate((ConfigFlags)0, (s, f) => s | f);

            Raylib.SetWindowState(flags);
        }

        private void AddFlag(in ConfigFlags flag)
        {
            m_CurrentFlags.Add(flag);
            RecalculateFlags();
        }

        private void RemoveFlag(in ConfigFlags flag)
        {
            m_CurrentFlags.Remove(flag);
            RecalculateFlags();
        }

        public Engine(int seed)
        {
            Globals.Reseed(seed);

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

            Globals.GameSet = new GlyphSet(GlyphSets.DefaultMapGlyphs);
            Globals.UISet = new GlyphSet(GlyphSets.DefaultUIGlyphs);

            Globals.Map = new Map(Settings.MapSize, Settings.BorderSize, Settings.MapGeneration.TunnelHeavy);

            Globals.Cursor = new Cursor(Point.Zero, Size.One);
            Globals.Player = new Actor("Jenkins", "A spry lad clad in armor and blade", Glyphs.ASCII.Player, 1, 10.0f, 5.0f, 7.5f, 0.50f, 0.75f, false, false);

            int totalEnemies = Globals.Generator.Next(Settings.Population.MinimumInitialEnemies, Settings.Population.MaximumInitialEnemies);

            SummonEnemies(totalEnemies);

            Globals.Relations = new RelationMatrix(Globals.Actors);

            while (!m_Exit)
            {
                CalculateDeltaTime();

                Input();
                Update();
                Render();
            }
            
            Raylib.CloseWindow();
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

        private void SetLastInput() => m_LastInputTime = DateTime.Now;
        private bool AllowInput => (DateTime.Now - m_LastInputTime).Milliseconds > Settings.MinimumInputTime;

        private void SetLastUpdate() => m_LastUpdateTime = DateTime.Now;
        private bool AllowUpdate => (DateTime.Now - m_LastUpdateTime).Milliseconds > Settings.MinimumUpdateTime;

        private void SetLastSummon() => m_LastSummonTime = DateTime.Now;
        private bool AllowSummon => (DateTime.Now - m_LastSummonTime).Milliseconds > Settings.MinimumSummonTime;

        private int RandomSummonAmount => Globals.Generator.Next(Settings.Population.MinimumSummonEnemies, Settings.Population.MaximumSummonEnemies);

        private int MaximumSummons => Settings.Population.MaximumTotalEnemies - Globals.TotalActors;

        private int NextSummonCount => System.Math.Min(RandomSummonAmount, MaximumSummons);

        public virtual void Input()
        {
            bool heldInput = AllowInput;

            Globals.Cursor!.Input();

            /*
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && !m_PathStarted)
                StartPath();
            else if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && m_PathStarted)
                EndPath();
            */

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
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
                {
                    Globals.ClearLog();
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

            if (!Globals.MapExists) return;

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                m_Locked = !m_Locked;
            }

            if (!m_Locked)
            {
                int x_input = 0;
                int y_input = 0;

                if (heldInput)
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) x_input = Settings.CameraSpeed;
                    else if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) x_input = -Settings.CameraSpeed;

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) y_input = -Settings.CameraSpeed;
                    else if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) y_input = Settings.CameraSpeed;

                    if (x_input != 0 || y_input != 0)
                    {
                        Globals.Map!.Move(new Point(x_input, y_input));
                        SetLastInput();
                        return;
                    }                    
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
                Globals.Map!.RevealMap();
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_F4))
            {
                Globals.Map!.RecalculateIndices();
            }

            if (Globals.PlayerExists && Globals.Player!.Alive && !m_PlayerActed)
            {
                m_PlayerActed = true;

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
                {
                    Globals.Player.Stance = Stance.Prone;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_X))
                {
                    Globals.Player.Stance = Stance.Erect;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
                {
                    Globals.Player.Stance = Stance.Crouch;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
                {
                    if (Globals.CursorExists && Globals.Cursor!.Cell != null)
                        Globals.Player.Act(Globals.Cursor.Cell.Position, Action.LookAt, false);
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
                            m_CurrentAction = Action.None;

                            SetLastInput();
                            return;
                        }
                        else if (x_input != 0 || y_input != 0)
                        {
                            Coord actPosition = new Coord(x_input, y_input, 0);

                            if (m_CurrentAction != Action.None)
                            {
                                Globals.Player.Act(actPosition, m_CurrentAction, true);
                                m_CurrentAction = Action.None;
                            }
                            else Globals.Player.Act(actPosition, true);

                            if (Settings.CursorLook && Globals.CursorExists && Globals.Cursor.Cell != null) Globals.Player.Act(Globals.Cursor.Cell.Position, Action.LookAt, false);

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
            if (Globals.Map == null) return;

            if (Globals.Player != null)
            {
                if (Globals.Player.Alive)
                {
                    if (m_Locked)
                        Globals.Map.CenterOn(Globals.Player.Position);

                    double view_distance = 0.0;
                    double view_span = 0.0;

                    switch (Globals.Player.Stance)
                    {
                        case Stance.Erect:
                            view_distance = 32.0;
                            view_span = 135.0;
                            break;
                        case Stance.Crouch:
                            view_distance = 16.0;
                            view_span = 150.0;
                            break;
                        case Stance.Prone:
                            view_distance = 48.0;
                            view_span = 33.75;
                            break;
                    }

                    var fov = Globals.Map.CalculateFOV(Globals.Player.Position, view_distance, Globals.Player.Angle.Degrees, view_span);

                    foreach (var pos in fov)
                    {
                        var cell = Globals.Map[pos];
                        if (cell == null) continue;
                        
                        foreach (var corpse in cell.Corpses)
                            if (corpse != null)
                                corpse.Observed = true;
                    }

                    Globals.Map.RevealMap(fov);
                }
                else if (Globals.Player.Dead)
                {
                    if (AllowUpdate)
                    {
                        m_PlayerActed = true;

                        Globals.Map.RevealMap(false);
                    }
                }
            }

            if (Globals.Map != null && Globals.CursorExists)
            {
                if (m_ShowLog)
                {
                    if (Globals.CursorExists && (Globals.Cursor!.Transform.position - Globals.Map.Position).x < (Screens.ScreenScale * 96.0) * 0.95)
                        m_ShowLog = false;
                }
                else
                {
                    if (Globals.CursorExists && (Globals.Cursor!.Transform.position - Globals.Map.Position).x > (Screens.ScreenScale * 96.0) * 0.95)
                        m_ShowLog = true;
                }
            }

            if (Globals.Map != null) Globals.Map.Update();

            if (m_PlayerActed)
            {
                CollectDead();

                foreach (var living_actor in Globals.Living)
                    if (living_actor != null)
                        living_actor.Update();

                m_PlayerActed = false;

                SetLastUpdate();

                Globals.CurrentTurn++;

                if (AllowSummon)
                {
                    var summonCount = NextSummonCount;

                    if (summonCount > 0) SummonEnemies(summonCount);
                }
            }

            Globals.TrimLog();

            if (Globals.CursorExists)
                Globals.Cursor!.Update(Screens.MapDisplay.position, Globals.GameSet!.GlyphSize);
        }

        public virtual void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Colors.Black);

            if (Globals.Map != null)
            {
                Globals.Map.Draw(Globals.GameSet!, 0, Screens.MapDisplay.position);

                string text =
                    $"{(Globals.Player != null ? Globals.Player.Name : "???")}:\n" +
                    $"{(Globals.Player != null ? Globals.Player.Position : "???")}:\n" +
                    $"Health: {(Globals.Player != null ? (int)System.Math.Ceiling(Globals.Player.CurrentHealth) : "(?.?)")}/{(Globals.Player != null ? (int)System.Math.Ceiling(Globals.Player.MaxHealth) : "(?.?)")}\n" +
                    $"Stance: {(Globals.Player != null ? Globals.Player.Stance : "???")}\n" +
                    $"Camera {(m_Locked ? "Locked" : "Unlocked")}";

                DrawLabel(text, Screens.InfoToolip, Size.One, Alignments.UpperLeft, Colors.White);
            }

            DrawLabel($"FPS: {m_FPS}", Screens.FooterBar.position, Size.One, Alignments.LowerLeft, Colors.White);

            DrawFixedLabel(Settings.WindowTitle, Screens.TitleBar, Size.One, Alignments.Centered, Colors.White);

            if (m_ShowLog)
            {
                string messages = "\nMessage Log: \n\n";

                foreach (var message in Globals.MessageLog)
                    if (message != "") { messages += message; }

                DrawFixedLabel(messages, Screens.MessageDisplay, Size.One, Alignments.UpperCentered, Colors.White);
            }
            else DrawLabel($"Message Log: ({Globals.MessageLog.Count})", Screens.LogTooltip, Size.One, Alignments.RightCentered, Colors.White);

            if (Globals.CursorExists)
                Globals.Cursor!.Draw(this, Globals.Map!, Globals.GameSet!);

            if (m_ShowControls)
            {
                string controls = "";
                controls += "\tMovement:\n";
                controls += "Manhattan: WASD\n";
                controls += "Chebyshev: Numpad\n\n";
                controls += "\tActions:\n";
                controls += "Attack:  Bump target\n";
                controls += "Mine:    Bumb terrain\n";
                controls += "Stand:   X\n";
                controls += "Crouch:  C\n";
                controls += "Prone:   Z\n";
                controls += "Look At: L\n";
                controls += "Wait:  KP5\n\n";
                controls += "\tCamera:\n";
                controls += "Move: Arrow keys\n";
                controls += "Lock: Spacebar\n\n";
                controls += "    F1: Controls ";

                DrawLabel(controls, Screens.FooterBar.position + new Point(Screens.FooterBar.size.width / 2, 0), Size.One, Alignments.LowerCentered, Colors.White);
            }
            else DrawLabel(" F1: Controls ", Screens.FooterBar.position + new Point(Screens.FooterBar.size.width / 2, 0), Size.One, Alignments.LowerCentered, Colors.White);

            if (m_ActionSelect)
            {
                string actions = "";
                actions += "Actions:\n\n";
                actions += "1:) Move To\n";
                actions += "2:) Look At\n";
                actions += "3:) Attack\n";
                actions += "4:) Push\n";
                actions += "5:) Mine\n";

                DrawLabel(actions, Screens.LeftSideBar.position + new Point(0, Screens.LeftSideBar.size.height / 2), Size.One, Alignments.LeftCentered, Colors.White);
            }

            //DrawPath();

            Raylib.EndDrawing();
        }

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
                        DrawRect(carriagePosition, Size.One, Colors.Black, Globals.UISet!.GlyphSize);
                        Globals.UISet.DrawGlyph((byte)c, color, carriagePosition);
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

            Point startPosition = position;

            if (alignment.horizontal == HorizontalAlignment.Center) startPosition.x -= maxWidth + (padding.width / 2) / 2;
            else if (alignment.horizontal == HorizontalAlignment.Right) startPosition.x -= maxWidth + padding.height * 2;

            if (alignment.vertical == VerticalAlignment.Center) startPosition.y -= numLines + (padding.height / 2) / 2;
            else if (alignment.vertical == VerticalAlignment.Lower) startPosition.y -= numLines + padding.height * 2;

            Size labelSize = new Size(maxWidth + padding.width * 2, numLines + padding.height * 2);

            DrawRect(startPosition, labelSize, Colors.Black, Globals.UISet!.GlyphSize, true);

            DrawRect(startPosition, labelSize, color, Globals.UISet!.GlyphSize, false);

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
                        DrawRect(carriagePosition, Size.One, Colors.Black, Globals.UISet.GlyphSize);
                        Globals.UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

		public void DrawFixedLabel(in string text, in Rect rect, in Size padding, in TextAlignment alignment, in Color color)
        {
            if (text == "") return;

            DrawRect(rect, Colors.Black, Globals.UISet!.GlyphSize, true);

            DrawRect(rect, color, Globals.UISet.GlyphSize, false);

            Point startPosition = rect.position;

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

            if (alignment.horizontal == HorizontalAlignment.Center) startPosition.x += (rect.size.width + (padding.width / 2) - maxWidth) / 2;
            else if (alignment.horizontal == HorizontalAlignment.Right) startPosition.x += rect.size.width + padding.width * 2;

            if (alignment.vertical == VerticalAlignment.Center) startPosition.y += (rect.size.height + (padding.height / 2) - numLines) / 2;
            else if (alignment.vertical == VerticalAlignment.Lower) startPosition.y += rect.size.height + padding.height * 2;

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
                        DrawRect(carriagePosition, Size.One, Colors.Black, Globals.UISet.GlyphSize);
                        Globals.UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

        /// <summary>
        /// BRING OUT YER DEAD!
        /// </summary>
        public void CollectDead()
        {
            List<Actor> the_living = new List<Actor>();
            List<Actor> the_dead = new List<Actor>();

            foreach (var maybe_living in Globals.Living)
            {
                if (maybe_living == null) continue;

                if (maybe_living.Alive) the_living.Add(maybe_living);
                else the_dead.Add(maybe_living);
            }

            if (Settings.AllowResurrection)
            {
                // Check for resurrection!
                foreach (var maybe_dead in Globals.Dead)
                {
                    if (maybe_dead == null) continue;

                    if (maybe_dead.Dead) the_dead.Add(maybe_dead);
                    else the_living.Add(maybe_dead);
                }
            }

            Globals.Living = the_living;
            Globals.Dead = the_dead;
        }

        /// <summary>
        /// Fetch their fetid souls from the warp!
        /// </summary>
        public void SummonEnemies(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                long next = Globals.Generator.NextBool(0.00666) ? 7 : Globals.Generator.NextBool(0.75) ? Globals.Generator.Next(0, Settings.Population.MaximumEnemyTypes / 2) : Globals.Generator.Next(Settings.Population.MaximumEnemyTypes / 2, Settings.Population.MaximumEnemyTypes - 1);

                Actor? newlySpawned = null;

                switch (next)
                {
                    case 0:
                        newlySpawned = new Actor("Gremlin", "A dimunitive creature with a cunning disposition", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightYellow), 1, 1.5f, 0.65f, 0.0f, 0.266f, 0.475f, true);
                        break;
                    case 1:
                        newlySpawned = new Actor("Goblin", "A dexterous and selfish humanoid", new Glyph(Characters.Entity[Cardinal.Central], Colors.LightGreen), 1, 3.5f, 1.25f, 0.5f, 0.375f, 0.675f, true);
                        break;
                    case 2:
                        newlySpawned = new Actor("Ork", "A brutal and violent humanoid", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightOrange), 1, 12.5f, 3.5f, 1.25f, 0.666f, 0.275f, true);
                        break;
                    case 3:
                        newlySpawned = new Actor("Troll", "A giant humaniod of great strength", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightRed), 1, 25.0f, 12.5f, 2.5f, 0.125f, 0.114f, true);
                        break;
                    case 4:
                        newlySpawned = new Actor("Draugr", "An undead servant of a wraith", new Glyph(Characters.Entity[Cardinal.Central], Colors.DarkMarble), 1, 7.5f, 2.5f, 5.0f, 0.675f, 0.221f, true);
                        break;
                    case 5:
                        newlySpawned = new Actor("Basilisk", "A large hexapedal reptile of terrible power", new Glyph(Characters.Entity[Cardinal.Central], Colors.Intrite), 1, 17.5f, 7.5f, 3.75f, 0.425f, 0.321f, true);
                        break;
                    case 6:
                        newlySpawned = new Actor("Serpentman", "A slithering humanoid with superior agility", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightBlue), 1, 17.5f, 7.5f, 3.75f, 0.425f, 0.321f, true);
                        break;
                    case 7:
                        newlySpawned = new Actor("Wraith", "An eldritch abomination! Woe upon thee...", new Glyph(Characters.Entity[Cardinal.Central], Colors.BrightMagenta), 2, 125.0f, 75.0f, 30.0f, 0.75f, 0.975f, true);
                        break;
                }

                if (newlySpawned == null) continue;
            }

            SetLastSummon();
        }
    }
}