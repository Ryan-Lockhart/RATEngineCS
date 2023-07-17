using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;

namespace rat
{
    public class Cell
    {
        public struct Attributes
        {
            private bool m_Solidity;
            private bool m_Opacity;
            private bool m_Explored;
            private bool m_Seen;
            private bool m_Bloodied;

            public Attributes(bool solidity, bool opacity, bool explored, bool seen, bool bloodied)
            {
                m_Solidity = solidity;
                m_Opacity = opacity;
                m_Explored = explored;
                m_Seen = seen;
                m_Bloodied = bloodied;
            }

            public bool Solidity { get => m_Solidity; set => m_Solidity = value; }
            public bool Opacity { get => m_Opacity; set => m_Opacity = value; }
            public bool Explored { get => m_Explored; set => m_Explored = value; }
            public bool Seen { get => m_Seen; set => m_Seen = value; }
            public bool Bloodied { get => m_Bloodied; set => m_Bloodied = value; }
        }

        public enum CellState
        {
            None,
            Wall,
            Obstacle,
            Floor,
            Overhang
        }

        private Coord m_Position;
        private Map? m_Parent;
        private Actor? m_Occupant;

        private List<Actor?> m_Corpses;
        private List<Cell?> m_Neighbours;

        private int m_Index;
        private bool m_Dirty;

        private bool m_Solid;
        private bool m_Opaque;
        private bool m_Bloody;

        private bool m_Seen;
        private bool m_Explored;

        public Coord Position { get => m_Position; set => m_Position = value; }

        public Map? Parent { get => m_Parent; set => m_Parent = value; }
        public bool Orphan => Parent == null;

        public Actor? Occupant { get => m_Occupant; set => m_Occupant = value; }
        public bool Vacant => Occupant == null;
        public bool Occupied => Occupant != null;

        public List<Actor?> Corpses { get => m_Corpses; set => m_Corpses = value; }

        public List<Cell?> Neighbours { get => m_Neighbours; set => m_Neighbours = value; }

        public int Index { get => m_Index; set => m_Index = value; }

        public bool Dirty { get => m_Dirty; set => m_Dirty = value; }
        public bool Clean => !Dirty;

        public bool Solid
        {
            get => m_Solid;
            set
            {
                if (m_Solid != value)
                {
                    m_Dirty = true;

                    foreach (var neighbour in m_Neighbours)
                        if (neighbour != null) neighbour.Dirty = true;
                }

                m_Solid = value;
            }
        }

        public bool Open => !Solid;

        public bool Opaque
        {
            get => m_Opaque;
            set
            {
                if (m_Opaque != value)
                    m_Dirty = true;

                m_Opaque = value;
            }
        }
        public bool Transperant => !Opaque;

        public bool Bloody { get => m_Bloody; set => m_Bloody = value; }
        public bool Unbloodied => !Bloody;

        public bool Seen { get => m_Seen; set => m_Seen = value; }
        public bool Unseen => !Seen;

        public bool Explored { get => m_Explored; set => m_Explored = value; }
        public bool Unexplored => !Explored;

        protected void SetIndex()
        {
            m_Index = 0x00;

            if (m_Parent == null) return;

            var neighbourhood = m_Parent.GetNeighbourhood(m_Position);

            if ((neighbourhood[0] != null ? neighbourhood[0].Solid ? true : false : true) &&
                (neighbourhood[1] != null ? neighbourhood[1].Solid ? true : false : true) &&
                (neighbourhood[3] != null ? neighbourhood[3].Solid ? true : false : true)) m_Index += 8;

            if ((neighbourhood[1] != null ? neighbourhood[1].Solid ? true : false : true) &&
                (neighbourhood[2] != null ? neighbourhood[2].Solid ? true : false : true) &&
                (neighbourhood[4] != null ? neighbourhood[4].Solid ? true : false : true)) m_Index += 4;

            if ((neighbourhood[4] != null ? neighbourhood[4].Solid ? true : false : true) &&
                (neighbourhood[6] != null ? neighbourhood[6].Solid ? true : false : true) &&
                (neighbourhood[7] != null ? neighbourhood[7].Solid ? true : false : true)) m_Index += 2;

            if ((neighbourhood[3] != null ? neighbourhood[3].Solid ? true : false : true) &&
                (neighbourhood[5] != null ? neighbourhood[5].Solid ? true : false : true) &&
                (neighbourhood[6] != null ? neighbourhood[6].Solid ? true : false : true)) m_Index += 1;

            if (m_Solid)
            {
                if (m_Index == 0x00)
                {
                    m_Solid = false;
                    m_Opaque = false;
                }
                else if (m_Index == 0x0F)
                    m_Index = Globals.Generator.NextBool(0.5) ? 0x1F : 0x3F;
                else if (m_Index == 0x00)
                    m_Index = Globals.Generator.NextBool(0.5) ? 0x10 : 0x30;
                else m_Index += Globals.Generator.NextBool(0.75) ? 0x10 : 0x30;
            }
            else
            {
                if (m_Index == 0x0F)
                    m_Index = Globals.Generator.NextBool(0.5) ? 0x0F : 0x2F;
                else m_Index += Globals.Generator.NextBool(0.75) ? 0x00 : 0x20;
            }

            if (m_Index > 0x4F) throw new Exception("Invalid cell index!");
        }

        public CellState State
        {
            get
            {
                if (Solid)
                {
                    if (Opaque) return CellState.Wall;
                    else return CellState.Obstacle;
                }
                else
                {
                    if (Opaque) return CellState.Overhang;
                    else return CellState.Floor;
                }
            }
        }

        public Cell(in Coord pos, Map? parent, bool solid = false, bool opaque = false)
		{
            m_Position = pos;
            m_Parent = parent;
            m_Solid = solid;
            m_Opaque = opaque;

            m_Corpses = new List<Actor?>();
            m_Neighbours = new List<Cell?>(8);
		}

        public void Reinitialize(in Coord pos, bool solid = false, bool opaque = false)
		{
            m_Position = pos;
            m_Solid = solid;
            m_Opaque = opaque;
		}

        public Actor? Vacate()
        {

            if (Occupied)
            {
                Actor? temp = Occupant;
                Occupant = null;
                return temp;
            }
            else { return null; }
        }

        /// <summary>
        /// Attempt to occupy a cell
        /// </summary>
        /// <param name="occupier">The actor that will occupy the cell if it is empty</param>
        /// <returns>True if the occupant </returns>
        public bool Occupy(Actor? occupier)
        {
            if (occupier == null)
                return false;

            if (Vacant)
            {
                Occupant = occupier;
                return true;
            }
            else return false;
        }

        public void AddCorpse(Actor? what)
        {
            if (what == null) return;
            if (what.Alive) return;

            Corpses.Add(what);
        }

        public void ClearCorpses() => m_Corpses.Clear();

        public void GenerateNeighbourhood()
        {
            if (Parent == null) return;

            m_Neighbours = new List<Cell?>();

            for (int y_offset = -1; y_offset <= 1; y_offset++)
                for (int x_offset = -1; x_offset <= 1; x_offset++)
                    if (x_offset != 0 || y_offset != 0)
                        m_Neighbours.Add(Parent[Position + new Coord(x_offset, y_offset, 0)]);
        }

		public void Draw(in GlyphSet glyphSet, in Point screenPosition, bool drawOccupant = true)
        {
            if (!Explored) return;

            Point drawPosition = Position - screenPosition;

            glyphSet.DrawGlyph(Index, Constants.Glyphs.ASCII.GetGlyph(Solid, Seen, Bloody).color, drawPosition);

            if (Occupied && drawOccupant && Seen)
                Occupant.Draw(glyphSet, screenPosition);
            else if (Vacant && Seen)
                if (m_Corpses.Count > 0)
                    glyphSet.DrawGlyph(new Glyph(Constants.Characters.Corpse, Constants.Colors.White), drawPosition);
        }

        public virtual void Update()
        {
            if (Dirty)
            {
                SetIndex();
                Dirty = false;
            }
        }
    }
}
