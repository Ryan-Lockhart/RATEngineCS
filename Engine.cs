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
        private bool m_Locked;

        private GlyphSet m_GameSet;
        private GlyphSet m_UISet;

        private Cursor m_Cursor;

        private Actor m_Player;
        private Map m_Map;

        private bool m_PlayerActed;

        private bool m_ShowLog;
        private bool m_ShowControls;

        private bool m_ActionSelect;
        private Action m_CurrentAction;
        private Coord m_ActionPosition;

        private List<Actor?> m_Actors;
        private List<Actor?> m_Living;
        private List<Actor?> m_Dead;

        private DateTime m_LastUpdateTime;
        private DateTime m_LastSummonTime;

        private List<ConfigFlags> m_CurrentFlags;

        public Engine(ulong seed)
        {
            Size windowSize = Screens.WindowSize * GlyphSets.DefaultMapGlyphs.glyphSize;

            Raylib.InitWindow((int)windowSize.width, (int)windowSize.height, "RATEngine");

            m_CurrentFlags = new List<ConfigFlags>
            {
                ConfigFlags.FLAG_WINDOW_ALWAYS_RUN,
                ConfigFlags.FLAG_WINDOW_UNDECORATED,
                ConfigFlags.FLAG_WINDOW_TOPMOST,
                ConfigFlags.FLAG_VSYNC_HINT
            };

            var flags = m_CurrentFlags
                .Cast<int>()
                .Aggregate(0, (s, f) => s | f);

            Raylib.SetWindowState((ConfigFlags)flags);
            Raylib.DisableCursor();

            m_GameSet = new GlyphSet(GlyphSets.DefaultMapGlyphs);
            m_UISet = new GlyphSet(GlyphSets.DefaultUIGlyphs);

            bool exit = false;

            DateTime lastFrame = DateTime.Now;
            DateTime thisFrame = DateTime.Now;

            TimeSpan deltaTime;

            m_Map = new Map(Settings.MapSize, Bounds.Sixteen);

            m_Cursor = new Cursor(m_Map, Point.Zero, Size.One);

            m_Map.Generate(0.4f);
            m_Map.Smooth(5, 4);

            m_Map.Populate();

            ulong currentID = 0;

            m_Player = new Actor(currentID, m_Map, "Jenkins", "A spry lad clad in armor and blade", Glyphs.ASCII.Player, 1, 10.0f, 5.0f, 7.5f, 0.50f, 0.75f, false, false);
            currentID++;

            m_Actors = new List<Actor?>();
            m_Living = new List<Actor?>();
            m_Dead = new List<Actor?>();

            m_Actors.Add(m_Player);
            m_Living.Add(m_Player);

            int totalEnemies = Globals.Generator.NextInt(Settings.MinimumEnemies, Settings.MaximumEnemies);

            SummonEnemies(ref currentID, totalEnemies);

            while (!exit)
            {
                lastFrame = thisFrame;
                thisFrame = DateTime.Now;
                deltaTime = thisFrame - lastFrame;

                m_FPS = deltaTime.Milliseconds;

                KeyboardKey keyPressed = KeyboardKey.KEY_NULL;

                do
                {
                    keyPressed = (KeyboardKey)Raylib.GetKeyPressed();

                    if (m_ActionSelect)
                    {
                        switch (keyPressed)
                        {
                            case KeyboardKey.KEY_KP_1:
                                m_CurrentAction = Action.MoveTo;
                                m_ActionSelect = false;
                                break;
                            case KeyboardKey.KEY_KP_2:
                                m_CurrentAction = Action.LookAt;
                                m_ActionSelect = false;
                                break;
                            case KeyboardKey.KEY_KP_3:
                                m_CurrentAction = Action.Attack;
                                m_ActionSelect = false;
                                break;
                            case KeyboardKey.KEY_KP_4:
                                m_CurrentAction = Action.Push;
                                m_ActionSelect = false;
                                break;
                            case KeyboardKey.KEY_KP_5:
                                m_CurrentAction = Action.Mine;
                                m_ActionSelect = false;
                                break;
                        }
                    }
                    else
                    {
                        switch (keyPressed)
                        {
                            case KeyboardKey.KEY_ESCAPE:
                                if (!m_ActionSelect)
                                    exit = true;
                                else
                                {
                                    m_CurrentAction = Action.None;
                                    m_ActionSelect = false;
                                }
                                break;
                            case KeyboardKey.KEY_BACKSPACE:
                                Globals.MessageLog.Clear();
                                break;
                            case KeyboardKey.KEY_TAB:
                                if (!m_ActionSelect)
                                {
                                    m_ActionSelect = true;
                                    m_CurrentAction = Action.None;
                                }
                                else m_ActionSelect = false;
                                break;
                            default:
                                Input(keyPressed);
                                break;
                        }
                    }
                }
                while (keyPressed != KeyboardKey.KEY_NULL);

                Update(deltaTime);
                Render(deltaTime);
            }

            Raylib.CloseWindow();
        }

        public void ToggleFullscreen() => m_Fullscreen = !m_Fullscreen;

        public virtual void Input(in KeyboardKey key)
        {
            int x_input = 0;
            int y_input = 0;

            if (m_Map != null)
            {
                if (m_Player != null && m_PlayerActed && m_Player.Alive)
                {
                    m_PlayerActed = true;

                    switch (key)
                    {
                        case KeyboardKey.KEY_Z:
                            m_Player.Stance = Stance.Prone;
                            break;
                        case KeyboardKey.KEY_X:
                            m_Player.Stance = Stance.Erect;
                            break;
                        case KeyboardKey.KEY_C:
                            m_Player.Stance = Stance.Crouch;
                            break;
                        case KeyboardKey.KEY_L:
                            if (m_Cursor != null && m_Cursor.Cell != null)
                                m_Player.Act(m_Cursor.Cell.Position, Action.LookAt, false);
                            break;
                        default:
                            m_PlayerActed = false;
                            break;
                    }
                }
                else
                {
                    switch (key)
                    {
                        case KeyboardKey.KEY_SPACE:
                            m_Locked = !m_Locked;
                            break;
                        case KeyboardKey.KEY_RIGHT:
                            m_Locked = false;
                            m_Map.Move(new Point(Settings.CameraSpeed, 0));
                            break;
                        case KeyboardKey.KEY_LEFT:
                            m_Locked = false;
                            m_Map.Move(new Point(-Settings.CameraSpeed, 0));
                            break;
                        case KeyboardKey.KEY_DOWN:
                            m_Locked = false;
                            m_Map.Move(new Point(0, -Settings.CameraSpeed));
                            break;
                        case KeyboardKey.KEY_UP:
                            m_Locked = false;
                            m_Map.Move(new Point(0, Settings.CameraSpeed));
                            break;
                        case KeyboardKey.KEY_F1:
                            m_ShowControls = !m_ShowControls;
                            break;
                        case KeyboardKey.KEY_F2:
                            ToggleFullscreen();
                            break;
                        case KeyboardKey.KEY_F3:
                            m_Map.RevealMap();
                            break;
                        case KeyboardKey.KEY_F4:
                            m_Map.RecalculateIndices();
                            break;
                        default:

                            if (m_Player != null && m_Player.Alive && !m_PlayerActed)
                            {
                                if (key == KeyboardKey.KEY_D || key == KeyboardKey.KEY_KP_6 || key == KeyboardKey.KEY_KP_3 || key == KeyboardKey.KEY_KP_9) { x_input = 1; }
                                else if (key == KeyboardKey.KEY_A || key == KeyboardKey.KEY_KP_4 || key == KeyboardKey.KEY_KP_7 || key == KeyboardKey.KEY_KP_1) { x_input = -1; }

                                if (key == KeyboardKey.KEY_S || key == KeyboardKey.KEY_KP_2 || key == KeyboardKey.KEY_KP_3 || key == KeyboardKey.KEY_KP_1) { y_input = 1; }
                                else if (key == KeyboardKey.KEY_W || key == KeyboardKey.KEY_KP_8 || key == KeyboardKey.KEY_KP_9 || key == KeyboardKey.KEY_KP_7) { y_input = -1; }

                                if (key == KeyboardKey.KEY_KP_5)
                                {
                                    m_PlayerActed = true;
                                    m_CurrentAction = Action.None;
                                    return;
                                }

                                if (x_input != 0 || y_input != 0)
                                {
                                    Coord actPosition = new Coord(x_input, y_input, 0);

                                    if (m_CurrentAction != Action.None)
                                    {
                                        m_Player.Act(actPosition, m_CurrentAction, true);
                                        m_CurrentAction = Action.None;
                                    }
                                    else m_Player.Act(actPosition, true);

                                    m_PlayerActed = true;
                                }
                            }

                            break;
                    }
                }
            }
        }

		public virtual void Update(in TimeSpan deltaTime)
        {
            if (m_Map == null) return;

            if (m_Player != null)
            {
                if (m_Player.Alive)
                {
                    if (m_Locked && m_Map != null && m_Player != null)
                        m_Map.CenterOn(m_Player.Position);

                    if (m_Map != null)
                    {
                        if (m_Player != null)
                        {
                            double view_distance = 0.0;
                            double view_span = 0.0;

                            switch (m_Player.Stance)
                            {
                                case Stance.Erect:
                                    view_distance = 32.0;
                                    view_span = 135.0;
                                    break;
                                case Stance.Crouch:
                                    view_distance = 16.0;
                                    view_span = 180.0;
                                    break;
                                case Stance.Prone:
                                    view_distance = 48.0;
                                    view_span = 33.75;
                                    break;
                            }

                            m_Map.CalculateFOV(m_Player.Position, view_distance, m_Player.Angle, view_span);
                        }
                    }
                }
                else if (m_Player.Dead)
                {
                    var now = DateTime.Now;

                    if ((ulong)(now - m_LastUpdateTime).Milliseconds > Settings.MinimumUpdateTime)
                    {
                        m_PlayerActed = true;
                        m_LastUpdateTime = now;

                        m_Map.RevealMap();
                    }
                }
            }

            if (m_Map != null && m_Cursor != null)
            {
                if (m_ShowLog)
                {
                    if (m_Cursor != null && (double)(m_Cursor.Transform.position - m_Map.Position).x < 96.0 * 0.925)
                        m_ShowLog = false;
                }
                else
                {
                    if (m_Cursor != null && (double)(m_Cursor.Transform.position - m_Map.Position).x > 96.0 * 0.975)
                        m_ShowLog = true;
                }
            }

            if (m_Map != null) m_Map.Update();

            if (m_PlayerActed)
            {
                CollectDead();

                foreach (var living_actor in m_Living)
                    if (living_actor != null)
                        living_actor.Update();

                m_PlayerActed = false;
            }

            while (Globals.MessageLog.Count > Settings.MaxMessages)
                Globals.MessageLog.Dequeue();

            if (m_Cursor != null)
                m_Cursor.Update(Screens.MapDisplay.position, m_GameSet.GlyphSize);
        }

        public virtual void Render(in TimeSpan deltaTime)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Colors.Black);

            if (m_Map != null)
            {
                m_Map.Draw(m_GameSet, 0, Screens.MapDisplay.position);

                Point mapPosition = m_Map.Position;

                string text = $"{(m_Player != null ? m_Player.Name : "???")}:\nHealth: {(m_Player != null ? (int)Math.Ceiling(m_Player.CurrentHealth) : "(?.?)")}/{(m_Player != null ? (int)Math.Ceiling(m_Player.MaxHealth) : "(?.?)")}\nStance: {(m_Player != null ? m_Player.Stance : "???")}\nCamera {(m_Locked ? "Locked" : "Unlocked")}";

                DrawLabel(text, new Point(0, 3), Size.One, Alignments.UpperLeft, Colors.White);
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
            else DrawLabel($"Message Log: ({Globals.MessageLog.Count})", new Point(128, Screens.MessageDisplay.position.y), Size.One, Alignments.RightCentered, Colors.White);

            if (m_Cursor != null)
                DrawCursor(m_Cursor);

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
                        maxWidth = Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    case '\t':
                        currWidth += currWidth % 4 > 0 ? currWidth % 4 : 4;
                        break;
                    case '\v':
                        numLines += numLines % 4 > 0 ? numLines % 4 : 4;
                        numLines++;
                        maxWidth = Math.Max(maxWidth, currWidth);
                        currWidth = 0;
                        break;
                    default:
                        currWidth++;
                        break;
                }
            }

            maxWidth = Math.Max(maxWidth, currWidth);

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
                        DrawRect(carriagePosition, Size.One, Colors.Black, m_UISet.GlyphSize);
                        m_UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

		public void DrawLabel(in string text, in Point position, in Size padding, in TextAlignment alignment, in Color color)
        {
            if (text == "") return;

            uint numLines = text == "" ? 0u : 1u;

            uint maxWidth = 0u;
            uint currWidth = 0u;

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\n':
                        numLines++;
                        maxWidth = Math.Max(maxWidth, currWidth);
                        currWidth = 0u;
                        break;
                    case '\t':
                        currWidth += currWidth % 4u > 0u ? currWidth % 4u : 4u;
                        break;
                    case '\v':
                        numLines += numLines % 4u > 0u ? numLines % 4u : 4u;
                        numLines++;
                        maxWidth = Math.Max(maxWidth, currWidth);
                        currWidth = 0u;
                        break;
                    default:
                        currWidth++;
                        break;
                }
            }

            maxWidth = Math.Max(maxWidth, currWidth);

            Point startPosition = position;

            if (alignment.horizontal == HorizontalAlignment.Center) startPosition.x -= maxWidth + (padding.width / 2) / 2u;
            else if (alignment.horizontal == HorizontalAlignment.Right) startPosition.x -= maxWidth + padding.height * 2;

            if (alignment.vertical == VerticalAlignment.Center) startPosition.y -= numLines + (padding.height / 2) / 2u;
            else if (alignment.vertical == VerticalAlignment.Lower) startPosition.y -= numLines + padding.height * 2;

            Size labelSize = new Size(maxWidth + padding.width * 2u, numLines + padding.height * 2u);

            DrawRect(startPosition, labelSize, Colors.Black, m_UISet.GlyphSize, true);

            DrawRect(startPosition, labelSize, color, m_UISet.GlyphSize, false);

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
                        DrawRect(carriagePosition, Size.One, Colors.Black, m_UISet.GlyphSize);
                        m_UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

		public void DrawFixedLabel(in string text, in Rect rect, in Size padding, in TextAlignment alignment, in Color color)
        {
            if (text == "") return;

            DrawRect(rect, Colors.Black, m_UISet.GlyphSize, true);

            DrawRect(rect, color, m_UISet.GlyphSize, false);

            Point startPosition = rect.position;

            uint numLines = text == "" ? 0u : 1u;

            uint maxWidth = 0u;
            uint currWidth = 0u;

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\n':
                        numLines++;
                        maxWidth = Math.Max(maxWidth, currWidth);
                        currWidth = 0u;
                        break;
                    case '\t':
                        currWidth += currWidth % 4u > 0u ? currWidth % 4u : 4u;
                        break;
                    case '\v':
                        numLines += numLines % 4u > 0u ? numLines % 4u : 4u;
                        numLines++;
                        maxWidth = Math.Max(maxWidth, currWidth);
                        currWidth = 0u;
                        break;
                    default:
                        currWidth++;
                        break;
                }
            }

            maxWidth = Math.Max(maxWidth, currWidth);

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
                        DrawRect(carriagePosition, Size.One, Colors.Black, m_UISet.GlyphSize);
                        m_UISet.DrawGlyph((byte)c, color, carriagePosition);
                        carriagePosition.x++;
                        break;
                }
            }
        }

		public void DrawCursor(in Cursor cursor, bool attached = false)
        {
            Rect cursorRect = cursor.Transform;

            Rect drawRect = new Rect(cursorRect.position - m_Map.Position + Screens.MapDisplay.position, cursorRect.size);

            DrawRect(drawRect, cursor.Color, m_GameSet.GlyphSize);

            TextAlignment alignment = cursor.Alignment;

            Point offset = new Point(
                alignment.horizontal == HorizontalAlignment.Right ? -1 : alignment.horizontal == HorizontalAlignment.Left ? 2 : 0,
                alignment.vertical == VerticalAlignment.Lower ? -1 : alignment.vertical == VerticalAlignment.Upper ? 2 : 0
            );

            Cell? cell = cursor.Cell;
            Actor? actor = cursor.Actor;

            string text = "";

            if (cell != null)
            {
                if (cell.Seen)
                {
                    if (actor != null)
                        text = $"{cursorRect.position}, {actor.Name}\n{actor.Description}";
                    else text = $"{cursorRect.position}, {(cell != null ? cell.State : " ??? ")}";

                    var corpses = cell.Corpses;

                    if (corpses.Count > 0)
                    {
                        text += "\n\nCorpses:";

                        if (Settings.UseCorpseLimit)
                        {
                            for (int i = 0; i < corpses.Count; i++)
                            {
                                Actor? corpse = corpses[i];
                                if (corpse != null) { text += "\n " + corpse.Name; }

                                if (i > Settings.CorpseLimit) break;
                            }

                            if (corpses.Count > Settings.CorpseLimit)
                                text += $"\n +{corpses.Count - Settings.CorpseLimit} more...";
                        }
                        else
                        {
                            foreach (var corpse in corpses)
                            {
                                if (corpse == null) continue;
                                text += "\n" + corpse.Name;
                            }
                        }                        
                    }
                }
                else if (cell.Explored)
                {
                    text = $"{cursorRect.position}, {(cell != null ? cell.State : "\"???\"")}";
                }
                else text = cursorRect.position + ", ???";
            }
            else
            {
                text = cursorRect.position + ", ???";
            }

            DrawLabel(
                text,
                attached ?
                    drawRect.position + (attached ? offset : Point.Zero) :
                    Screens.MapToUI(new Point(Screens.MapDisplay.position.x, Screens.FooterBar.position.y)),
                Size.One,
                attached ?
                    cursor.Alignment :
                    Alignments.LowerRight,
                Colors.White
            );
        }

        /// <summary>
        /// BRING OUT YER DEAD!
        /// </summary>
        public void CollectDead()
        {
            List<Actor?> the_living = new List<Actor?>();
            List<Actor?> the_dead = new List<Actor?>();

            foreach (var maybem_Living in m_Living)
            {
                if (maybem_Living == null) continue;

                if (maybem_Living.Alive) the_living.Add(maybem_Living);
                else the_dead.Add(maybem_Living);
            }

            if (Settings.AllowResurrection)
            {
                // Check for resurrection!
                foreach (var maybem_Dead in m_Dead)
                {
                    if (maybem_Dead == null) continue;

                    if (maybem_Dead.Dead) the_dead.Add(maybem_Dead);
                    else the_living.Add(maybem_Dead);
                }
            }

            m_Living = the_living;
            m_Dead = the_dead;
        }

        /// <summary>
        /// Fetch their fetid souls from the warp!
        /// </summary>
        public void SummonEnemies(ref ulong currentID, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                long next = Globals.Generator.NextBool(0.00666) ? 7 : Globals.Generator.NextBool(0.75) ? Globals.Generator.NextInt(0, Settings.MaximumEnemyTypes / 2) : Globals.Generator.NextInt(Settings.MaximumEnemyTypes / 2, Settings.MaximumEnemyTypes - 1);

                Actor? newlySpawned = null;

                switch (next)
                {
                    case 0:
                        newlySpawned = new Actor(currentID, m_Map, "Gremlin", "A dimunitive creature with a cunning disposition", new Glyph(Characters.Entity, Colors.BrightYellow), 1, 1.5f, 0.65f, 0.0f, 0.266f, 0.475f, true);
                    break;

                    case 1:
                        newlySpawned = new Actor(currentID, m_Map, "Goblin", "A dexterous and selfish humanoid", new Glyph(Characters.Entity, Colors.LightGreen), 1, 3.5f, 1.25f, 0.5f, 0.375f, 0.675f, true);
                        break;
                    case 2:
                        newlySpawned = new Actor(currentID, m_Map, "Ork", "A brutal and violent humanoid", new Glyph(Characters.Entity, Colors.BrightOrange), 1, 12.5f, 3.5f, 1.25f, 0.666f, 0.275f, true);
                        break;
                    case 3:
                        newlySpawned = new Actor(currentID, m_Map, "Troll", "A giant humaniod of great strength", new Glyph(Characters.Entity, Colors.BrightRed), 1, 25.0f, 12.5f, 2.5f, 0.125f, 0.114f, true);
                        break;
                    case 4:
                        newlySpawned = new Actor(currentID, m_Map, "Draugr", "An undead servant of a wraith", new Glyph(Characters.Entity, Colors.DarkMarble), 1, 7.5f, 2.5f, 5.0f, 0.675f, 0.221f, true);
                        break;
                    case 5:
                        newlySpawned = new Actor(currentID, m_Map, "Basilisk", "A large hexapedal reptile of terrible power", new Glyph(Characters.Entity, Colors.Intrite), 1, 17.5f, 7.5f, 3.75f, 0.425f, 0.321f, true);
                        break;
                    case 6:
                        newlySpawned = new Actor(currentID, m_Map, "Serpentman", "A slithering humanoid with superior agility", new Glyph(Characters.Entity, Colors.BrightBlue), 1, 17.5f, 7.5f, 3.75f, 0.425f, 0.321f, true);
                        break;
                    case 7:
                        newlySpawned = new Actor(currentID, m_Map, "Wraith", "An eldritch abomination! Woe upon thee...", new Glyph(Characters.Entity, Colors.BrightMagenta), 2, 125.0f, 75.0f, 30.0f, 0.75f, 0.975f, true);
                        break;
                }

                if (newlySpawned == null) continue;

                m_Actors.Add(newlySpawned);
                m_Living.Add(newlySpawned);

                currentID++;
            }
        }
    }
}