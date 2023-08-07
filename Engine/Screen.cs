using rat.Constants;
using rat.Primitives;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public abstract class Screen
    {
        private string m_Name;

        private Rect m_Transform;

        private Cursor m_Cursor;

        protected Screen(string name, Rect transform, Cursor cursor)
        {
            m_Name = name;
            m_Transform = transform;
            m_Cursor = cursor;
        }

        protected Screen(string name, Point position, Size size, Cursor cursor)
        {
            m_Name = name;
            m_Transform = new Rect(position, size);
            m_Cursor = cursor;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            protected set
            {
                m_Name = value;
            }
        }

        public Rect Transform
        {
            get
            {
                return m_Transform;
            }
            protected set
            {
                m_Transform = value;
            }
        }

        public Point Position
        {
            get
            {
                return m_Transform.position;
            }
            protected set
            {
                m_Transform.position = value;
            }
        }

        public Size Size
        {
            get
            {
                return m_Transform.size;
            }
            protected set
            {
                m_Transform.size = value;
            }
        }

        public Cursor Cursor
        {
            get
            {
                return m_Cursor;
            }
            protected set
            {
                m_Cursor = value;
            }
        }

        public abstract void Update();
        public abstract bool Input();
        public abstract void Draw(in GlyphSet glyphs);
    }

    public class MapScreen : Screen
    {
        private Map m_Map;
        private bool m_Locked = true;

        public MapScreen(string name, Rect transform, Map map, Cursor cursor) : base(name, transform, cursor)
        {
            m_Map = map;
        }

        public MapScreen(string name, Point position, Size size, Map map, Cursor cursor) : base(name, position, size, cursor)
        {
            m_Map = map;
        }

        public override void Draw(in GlyphSet glyphs)
        {
            m_Map.Draw(glyphs, Position);
        }

        public override bool Input()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                m_Locked = !m_Locked;
            }

            if (!m_Locked)
            {
                int x_input = 0;
                int y_input = 0;

                if (Globals.Engine.AllowInput)
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) x_input = Settings.CameraSpeed;
                    else if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) x_input = -Settings.CameraSpeed;

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) y_input = -Settings.CameraSpeed;
                    else if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) y_input = Settings.CameraSpeed;

                    if (x_input != 0 || y_input != 0)
                    {
                        m_Map.Move(new Point(x_input, y_input), true);
                        Globals.Engine.SetLastInput();
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Update()
        {
            m_Map.Update();
            if (m_Locked)
                m_Map.CenterOn(Globals.Engine.Player.Position);
        }
    }

    public class MessageScreen : Screen
    {
        protected MessageLog m_Log;
        public MessageLog MessageLog
        {
            get
            {
                return m_Log;
            }
            protected set
            {
                m_Log = value;
            }
        }

        protected bool m_ShowLog;

        public MessageScreen(string name, Rect transform, Cursor cursor)
            : base(name, transform, cursor)
        {
            m_Log = new MessageLog();
        }

        public MessageScreen(string name, Point position, Size size, Cursor cursor)
            : base(name, position, size, cursor)
        {
            m_Log = new MessageLog();
        }

        public override void Draw(in GlyphSet glyphs)
        {
            if (m_ShowLog && !m_Log.Empty)
            {
                string messages = "";

                for (int i = 0; i < m_Log.Messages.Count; i++)
                {
                    string message = m_Log.Messages[i];

                    if (message == "") continue;

                    if (i != 0 && i != m_Log.Messages.Count)
                        messages += "\n\n";

                    messages += message;
                }

                messages += "\n\n\n";

                Globals.Engine.DrawLabel(messages, Transform.position, Size.One, Alignments.LowerCentered, Colors.White);
            }
            
            Globals.Engine.DrawLabel($"Message Log: ({m_Log.LogSize})", Screens.MessagesTooltip, Size.One, Alignments.LowerCentered, Colors.White);
        }

        public override bool Input()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
            {
                m_Log.ClearLog();
            }

            if (m_ShowLog)
            {
                if (Cursor.ScreenPosition.y < (Screens.ScreenScale * 48.0) * 0.95)
                    m_ShowLog = false;
            }
            else
            {
                if (Cursor.ScreenPosition.y > (Screens.ScreenScale * 48.0) * 0.95)
                    m_ShowLog = true;
            }

            return false;
        }

        public override void Update()
        {
            m_Log.TrimLog();
        }
    }
}
