using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using rat.Constants;
using rat.Primitives;

using Color = rat.Primitives.Color;

namespace rat
{
    public class Cursor
    {
        private Map? m_Parent;

        private Cell? m_Cell;
        private Actor? m_Actor;

        private Rect m_Transform;
        private Color m_Color;

        private TextAlignment m_Alignment;

        public Cursor(Map? parent, in Point position, in Size size)
        {
            m_Parent = parent;
            m_Transform = new Rect(position, size);
        }

        public Cursor(Map? parent, in Rect transform)
        {
            m_Parent = parent;
            m_Transform = transform;
        }

        public Map? Parent => m_Parent;

        public Cell? Cell => m_Cell;

        public Actor? Actor => m_Actor;

        public Rect Transform { get => m_Transform; set => m_Transform = value; }

        public Color Color { get => m_Color; set => m_Color = value; }

        public TextAlignment Alignment { get => m_Alignment; set => m_Alignment = value; }

        public void Update(in Point offset, in Size gridSize)
        {
            if (m_Parent == null) return;

            Point mousePosition = Raylib.GetMousePosition();

            Point gridPosition = mousePosition / gridSize;

            if (gridPosition.x > Screens.MapDisplay.size.width / 2)
                m_Alignment.horizontal = HorizontalAlignment.Right;
            else if (gridPosition.x < Screens.MapDisplay.size.width / 2)
                m_Alignment.horizontal = HorizontalAlignment.Left;

            if (gridPosition.y > Screens.MapDisplay.size.height / 2)
                m_Alignment.vertical = VerticalAlignment.Lower;
            else if (gridPosition.y < Screens.MapDisplay.size.height / 2)
                m_Alignment.vertical = VerticalAlignment.Upper;

            Point mapPosition = m_Parent.Position;

            m_Transform.position = gridPosition + mapPosition - offset;

            m_Cell = m_Parent[(Coord)m_Transform.position];

            m_Actor = m_Cell != null ? m_Cell.Occupant : null;

            m_Color = m_Actor != null && m_Cell != null ? m_Cell.Seen ? m_Actor.Glyph.color : Colors.White : Colors.White;
        }
    }
}
