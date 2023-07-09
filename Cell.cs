using rat.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rat
{
    public class Cell
    {
        public struct Attributes { bool solidity, opacity, explored, seen, bloodiness; };

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

        private byte m_Index;
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

        public byte Index { get => m_Index; set => m_Index = value; }

        public bool Dirty { get => m_Dirty; set => m_Dirty = value; }
        public bool Clean => !Dirty;

        public bool Solid { get => m_Solid; set => m_Solid = value; }
        public bool Open => !Solid;

        public bool Opaque { get => m_Opaque; set => m_Opaque = value; }
        public bool Transperant => !Opaque;

        public bool Bloody { get => m_Bloody; set => m_Bloody = value; }
        public bool Unbloodied => !Bloody;

        public bool Seen { get => m_Seen; set => m_Seen = value; }
        public bool Unseen => !Seen;

        public bool Explored { get => m_Explored; set => m_Explored = value; }
        public bool Unexplored => !Explored;

        protected void SetIndex()
        {

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
            Position = pos;
            Parent = parent;
            Solid = solid;
            Opaque = opaque;
		}

        public void Reinitialize(in Coord pos, bool solid = false, bool opaque = false)
		{
            Position = pos;
            Solid = solid;
            Opaque = opaque;
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
                    m_Neighbours.Add(Parent[Position + new Coord(x_offset, y_offset, 0)]);
        }

		public void Draw(in GlyphSet glyphSet, in Point screenPosition, bool drawOccupant = true)
        {
            if (!Explored) return;

            Point drawPosition = Position - screenPosition;

            glyphSet.DrawGlyph(Index, Constants.Glyphs.ASCII.GetGlyph(Solid, Seen, Bloody).color, drawPosition);

            if (Occupied && drawOccupant && Seen)
                Occupant.Draw(glyphSet, screenPosition);
            else if (Vacant)
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
