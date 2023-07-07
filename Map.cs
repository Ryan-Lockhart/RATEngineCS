using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;
using rat.Constants;
using System.Collections;

namespace rat
{
    public class Map
    {
		private bool m_Generating;

		private BitArray m_Solids;
		private Cell[] m_Cells;

		private Point m_Position;

		private Bounds m_Bounds;
		private Bounds m_Border;

        public Point Position { get => m_Position; private set => m_Position = value; }

        public Cell? this[in Coord coord]
        {
            get
            {
                if (!IsValid(coord)) { return null; }
                else
                {
                    try { return m_Cells[Index(coord)]; }
                    catch { return null; }
                }
            }
        }

        private int Index(in Coord coord)
		{
			return (int)(coord.z * m_Bounds.Area() + coord.y * m_Bounds.width + coord.x);
		}

		private void ConstrainToScreen()
		{
            if (m_Position.x < 0)
                m_Position.x = 0;
            else if (m_Position.x > m_Bounds.width - displayRect.size.Width)
                m_Position.x = m_Bounds.width - displayRect.size.Width;

            if (m_Position.y < 0)
                m_Position.y = 0;
            else if (m_Position.y > m_Bounds.height - displayRect.size.Height)
                m_Position.y = m_Bounds.height - displayRect.size.Height;
        }

		private void ShadowCast(in Coord origin, int row, double start, double end, in Octant octant, double radius);
		private void ShadowCastLimited(in Coord origin, int row, double start, double end, in Octant octant, double radius, double angle, double span);

		private void ShadowCast(in Coord origin, ref Coord[] fovVector, int row, double start, double end, in Octant octant, double radius);
		private void ShadowCastLimited(in Coord origin, ref Coord[] fovVector, int row, double start, double end, in Octant octant, double radius, double angle, double span);

		public Map(in Bounds bounds, in Bounds border)
		{

		}

        public void Move(in Point position, bool offset = true)
		{
            if (offset) { m_Position += position; }
            else { m_Position = position; }

            ConstrainToScreen();
        }

        public void Generate(float fillPercent = 0.5f)
		{

		}

        public void Smooth(int iterations = 5, int threshold = 4)
		{

		}

		public bool Automatize(in Coord position, int threshold)
		{

		}

        public void Populate()
		{

		}

        public void Regenerate(float fillPercent = 0.5f, int iterations = 5, int threshold = 4)
		{

		}

        public void RecalculateIndices()
		{

		}

		public Cell? FindOpen(float checkPercent = 0.5f)
		{

		}

		public bool IsSolid(in Coord position, in BitArray solids)
		{
			return IsValid(position) ?
				m_Solids[Index(position)] : true;
		}

		public void ResetSeen()
        {

        }

        public void RevealMap()
        {

        }

        public void CenterOn(in Coord position)
        {

        }

        public void Draw(in GlyphSet glyphSet, long drawDepth)
        {

        }

        public void Draw(in GlyphSet glyphSet, long drawDepth, in Point offset)
		{

		}

		public virtual void Update()
        {

        }

        public bool IsValid(in Coord position)
        {

        }

        public bool WithinBounds(in Coord position)
        {

        }

        public bool IsGenerating() => m_Generating;

        public void CalculateFOV(in Coord origin, double viewDistance)
        {

        }

        public void CalculateFOV(in Coord origin, double viewDistance, double angle, double span)
        {

        }

        public void CalculateFOV(in Coord origin, double viewDistance, double angle, double span, double nudge)
        {

        }

        public List<Coord> WithinFOV(in Coord origin, double viewDistance)
        {

        }

        public List<Coord> WithinFOV(in Coord origin, double viewDistance, double angle, double span)
        {

        }

        public List<Coord> WithinFOV(in Coord origin, double viewDistance, double angle, double span, double nudge)
        {

        }

        public Stack<Coord> CalculatePath(in Coord origin, in Coord destination)
        {

        }

        public void GetNeighbourhood(in Coord position, out List<Cell?> neighbourhood)
        {
            neighbourhood = new List<Cell?>();

            for (int y_offset = -1; y_offset <= 1;  y_offset++)
            {
                for (int x_offset = -1; x_offset <= 1; x_offset++)
                {
                    Coord currentPosition = position + new Coord(x_offset, y_offset, 0);

                    neighbourhood.Add(this[currentPosition]);
                }
            }
        }

        public List<Cell?> GetNeighbourhood(in Coord position, bool returnNulls)
        {

        }

        public byte GetNeighbourCount(in Coord position)
        {
            uint8_t neighbourCount(0);

            for (int y_offset = -1; y_offset <= 1; y_offset++)
                for (int x_offset = -1; x_offset <= 1; x_offset++)
                {
                    Coord currentPosition{ position.X + x_offset, position.Y + y_offset, position.Z };

                    if (currentPosition != position)
                        neighbourCount += IsValid(currentPosition) ? GetCell(currentPosition)->IsSolid() : 1;
                }

            return neighbourCount;
        }
    }
}
