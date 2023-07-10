using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;
using rat.Constants;
using rat.Random;
using System.Reflection.Metadata;
using System.Collections.Specialized;

namespace rat
{
    public class Map
    {
		private bool m_Generating;

		private bool[] m_Solids;
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
			return (int)(coord.z * m_Bounds.Area + coord.y * m_Bounds.width + coord.x);
		}

		private void ConstrainToScreen()
		{
            if (m_Position.x < 0)
                m_Position.x = 0;
            else if (m_Position.x > m_Bounds.width - Screens.MapDisplay.size.width)
                m_Position.x = m_Bounds.width - Screens.MapDisplay.size.width;

            if (m_Position.y < 0)
                m_Position.y = 0;
            else if (m_Position.y > m_Bounds.height - Screens.MapDisplay.size.height)
                m_Position.y = m_Bounds.height - Screens.MapDisplay.size.height;
        }

		private void ShadowCast(in Coord origin, List<Coord> fov, int row, double start, double end, in Octant octant, double radius)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Bounds.Area && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Coord currentPosition = new Coord((long)(origin.x + deltaX * octant.x + deltaY * octant.dx), (long)(origin.y + deltaX * octant.y + deltaY * octant.dy), origin.z);
                    Coord delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = Maths.Normalize(deltaX, deltaY);

                    Cell? cell = this[currentPosition];

                    if (cell != null)
                    {
                        if (deltaRadius <= radius)
                            fov.Add(cell.Position);

                        if (blocked)
                        {
                            if (cell.Opaque)
                                newStart = rightSlope;
                            else
                            {
                                blocked = false;
                                start = newStart;
                            }
                        }
                        else if (cell.Opaque && distance < radius)
                        {
                            blocked = true;

                            ShadowCast(origin, fov, (int)distance + 1, start, leftSlope, octant, radius);

                            newStart = rightSlope;
                        }
                    }
                }
            }
        }

		private void ShadowCast(in Coord origin, List<Coord> fov, int row, double start, double end, in Octant octant, double radius, double angle, double span)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Bounds.Area && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Coord currentPosition = new Coord((long)(origin.x + deltaX * octant.x + deltaY * octant.dx), (long)(origin.y + deltaX * octant.y + deltaY * octant.dy), origin.z);
                    Coord delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = Maths.Normalize(deltaX, deltaY);
                    double at2 = Math.Abs(angle - Maths.Atan2(delta.y, delta.x));

                    Cell? cell = this[currentPosition];

                    if (cell != null)
                    {
                        if (deltaRadius <= radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                            fov.Add(cell.Position);

                        if (blocked)
                        {
                            if (cell.Opaque)
                                newStart = rightSlope;
                            else
                            {
                                blocked = false;
                                start = newStart;
                            }
                        }
                        else if (cell.Opaque && distance < radius)
                        {
                            blocked = true;

                            ShadowCast(origin, fov, (int)distance + 1, start, leftSlope, octant, radius, angle, span);

                            newStart = rightSlope;
                        }
                    }
                }
            }
        }

		public Map(in Bounds bounds, in Bounds border)
		{
            m_Position = Point.Zero;

            m_Bounds = bounds;
            m_Border = border;

            m_Solids = new bool[m_Bounds.Volume];
            m_Cells = new Cell[m_Bounds.Volume];
		}

        public void Move(in Point position, bool offset = true)
		{
            if (offset) { m_Position += position; }
            else { m_Position = position; }

            ConstrainToScreen();
        }

        public void Generate(float fillPercent = 0.5f)
		{
            m_Generating = true;

            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord coord = new Coord(x, y, z);

                        m_Solids[Index(coord)] = WithinBounds(coord) ? Globals.Generator.NextBool(fillPercent) : true;
                    }
        }

        public void Smooth(int iterations = 5, int threshold = 4)
		{
            bool[] smoothed = m_Solids;

            for (ulong i = 0; i < (ulong)iterations; i++)
            {
                for (long z = 0; z < m_Bounds.depth; z++)
                    for (long y = 0; y < m_Bounds.height; y++)
                        for (long x = 0; x < m_Bounds.width; x++)
                        {
                            Coord coord = new Coord(x, y, z);

                            smoothed[Index(coord)] = WithinBounds(coord) ? Automatize(coord, threshold) : true;
                        }

                (smoothed, m_Solids) = (m_Solids, smoothed);
            }
        }

		public bool Automatize(in Coord position, int threshold)
		{
            bool originalState = m_Solids[Index(position)];

            int neighbours = 0;

            bool isFlat = m_Bounds.depth == 1;

            for (long offset_z = isFlat ? 1 : -1; offset_z < 2; offset_z++)
                for (long offset_y = -1; offset_y < 2; offset_y++)
                    for (long offset_x = -1; offset_x < 2; offset_x++)
                        if (offset_x != 0 || offset_y != 0 || (offset_z != 0 || isFlat))
                            neighbours += IsSolid(position + new Coord(offset_x, offset_y, isFlat ? 0 : offset_z), m_Solids) ? 1 : 0;

            return neighbours > threshold ? true : neighbours < threshold ? false : originalState;
        }

        public void Populate()
		{
            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord coord = new Coord(x, y, z);

                        bool solid = IsSolid(coord, m_Solids);

                        if (this[coord] != null)
                            m_Cells[Index(coord)].Reinitialize(coord, solid, solid);
                        else
                            m_Cells[Index(coord)] = new Cell(coord, this, solid, solid);
                    }

            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord coord = new Coord(x, y, z);

                        m_Cells[Index(coord)].GenerateNeighbourhood();
                    }

            RecalculateIndices();

            m_Generating = false;
        }

        public void Regenerate(float fillPercent = 0.5f, int iterations = 5, int threshold = 4)
		{
            Generate(fillPercent);
            Smooth(iterations, threshold);
            Populate();
        }

        public void RecalculateIndices()
		{
            foreach (var cell in m_Cells)
                if (cell != null)
                {
                    cell.Dirty = true;
                    cell.Update();
                }
        }

		public Cell? FindOpen(float checkPercent = 0.5f)
		{
            int maxChecks = (int)(m_Bounds.Volume * checkPercent);
            int checks = 0;

            while (checks < maxChecks)
            {
                long x = Globals.Generator.NextInt(0, (int)m_Bounds.width - 1);
                long y = Globals.Generator.NextInt(0, (int)m_Bounds.height - 1);
                long z = Globals.Generator.NextInt(0, (int)m_Bounds.depth - 1);

                Coord randomPosition = new Coord(x, y, z);

                Cell? randomCell = this[randomPosition];

                if (randomCell != null)
                {
                    if (randomCell.Open)
                        if (randomCell.Vacant)
                            return randomCell;
                }
                else checks++;
            }

            throw new Exception("No open cells!");
        }

		public bool IsSolid(in Coord position, in bool[] solids)
		{
			return IsValid(position) ?
				m_Solids[Index(position)] : true;
		}

        /// <summary>
        /// Reset the fog of war
        /// </summary>
		public void ResetSeen()
        {
            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord position = new Coord(x, y, z);

                        Cell? cell = this[position];
                        if (cell == null) continue;

                        cell.Seen = false;
                    }
        }

        /// <summary>
        /// Reveal the entire map
        /// </summary>
        public void RevealMap(bool onlyFog = true)
        {
            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord position = new Coord(x, y, z);

                        Cell? cell = this[position];
                        if (cell == null) continue;

                        cell.Explored = true;

                        if (!onlyFog)
                            cell.Seen = true;
                    }
        }

        public void RevealMap(in List<Coord> fov, bool resetSeen = true)
        {
            if (resetSeen) ResetSeen();

            foreach (Coord position in fov)
            {
                Cell? cell = this[position];
                if (cell == null) continue;

                cell.Seen = true;
                cell.Explored = true;
            }
        }

        public void CenterOn(in Coord position)
        {
            if (!IsValid(position))
                return;

            m_Position.x = position.x - Screens.MapDisplay.size.width / 2;
            m_Position.y = position.y - Screens.MapDisplay.size.height / 2;

            ConstrainToScreen();
        }

        public void Draw(in GlyphSet glyphSet, long drawDepth, in Point offset)
		{
            if (drawDepth < 0 || drawDepth > m_Bounds.depth)
                return;

            for (long y = m_Position.y; y < m_Position.y + Screens.MapDisplay.size.height; y++)
                for (long x = m_Position.x; x < m_Position.x + Screens.MapDisplay.size.width; x++)
                {
                    Coord cellCoord = new Coord(x, y, drawDepth);

                    Cell? cell = this[cellCoord];

                    if (cell != null)
                        cell.Draw(glyphSet, m_Position - offset, true);
                    else glyphSet.DrawGlyph(m_Solids[Index(cellCoord)] ? Glyphs.ASCII.Wall : Glyphs.ASCII.Floor, m_Position + cellCoord + offset);
                }
        }

		public virtual void Update()
        {
            foreach (var cell in m_Cells)
                if (cell != null) cell.Update();
        }

        public bool IsValid(in Coord position)
        {
            bool isFlat = m_Bounds.depth == 1;
            return position.x >= 0 && position.y >= 0 && position.x < m_Bounds.width && position.y < m_Bounds.height && position.z >= 0 && (position.z < m_Bounds.depth || isFlat);
        }

        public bool WithinBounds(in Coord position)
        {
            bool isFlat = m_Bounds.depth == 1;
            return position.x >= m_Border.width && position.y >= m_Border.height && position.x < m_Bounds.width - m_Border.width && position.y < m_Bounds.height - m_Border.height && (position.z >= m_Border.depth || isFlat) && (position.z < m_Bounds.depth - m_Border.depth || isFlat);
        }

        public bool IsGenerating() => m_Generating;

        public List<Coord> CalculateFOV(in Coord origin, double viewDistance)
        {
            List<Coord> fov = new List<Coord>();

            viewDistance = Math.Max(1, viewDistance);

            Cell? cell = this[origin];
            if (cell != null)
            {
                var neighbours = GetNeighbourhood(origin);

                foreach (var neighbour in neighbours)
                    if (neighbour != null)
                        fov.Add(neighbour.Position);

                fov.Add(cell.Position);
            }

            for (int i = 0; i < Settings.Octants.Length; i++)
                ShadowCast(origin, fov, 1, 1.0, 0.0, Settings.Octants[i], viewDistance);

            return fov;
        }

        public List<Coord> CalculateFOV(in Coord origin, double viewDistance, double angle, double span)
        {
            List<Coord> fov = new List<Coord>();

            viewDistance = Math.Max(1, viewDistance);

            angle = (angle > 360.0 || angle < 0.0 ? Maths.WrapAround(angle, 360.0) : angle) * Maths.PercentOfCircle;
            span *= Maths.PercentOfCircle;

            Cell? cell = this[origin];

            if (cell != null)
            {
                var neighbours = GetNeighbourhood(origin);

                foreach (var neighbour in neighbours)
                    if (neighbour != null)
                        fov.Add(neighbour.Position);

                fov.Add(cell.Position);
            }

            for (int i = 0; i < Settings.Octants.Length; i++)
                ShadowCast(origin, fov, 1, 1.0, 0.0, Settings.Octants[i], viewDistance, angle, span);

            return fov;
        }

        public List<Coord> CalculateFOV(in Coord origin, double viewDistance, double angle, double span, double nudge)
        {
            List<Coord> fov = new List<Coord>();

            double radians = Maths.ToRadians(angle);

            Coord shiftedOrigin = nudge != 0 ?
			    new Coord( origin.x + (long)(Math.Cos(radians) * nudge), origin.y + (long)(Math.Sin(radians) * nudge), origin.z) :
                origin;

		    viewDistance = Math.Max(1, viewDistance);

		    angle = (angle > 360.0 || angle < 0.0 ? Maths.WrapAround(angle, 360.0) : angle) * Maths.PercentOfCircle;
		    span *= Maths.PercentOfCircle;

		    Cell? cell = this[shiftedOrigin];

		    if (cell != null)
		    {
			    var neighbours = GetNeighbourhood(shiftedOrigin);

			    foreach (var neighbour in neighbours)
				    if (neighbour != null)
                        fov.Add(neighbour.Position);

                fov.Add(cell.Position);
		    }

		    for (int i = 0; i < Settings.Octants.Length; i++)
                ShadowCast(shiftedOrigin, fov, 1, 1.0, 0.0, Settings.Octants[i], viewDistance, angle, span);

            return fov;
        }

        public struct NodeData
        {
	        public Coord position;
            public int distance;
            public int heuristic;

            public NodeData(in Coord pos, int dist, int h)
            {
                position = pos;
                distance = dist;
                heuristic = h;
            }

            /// <summary>
            /// Total cost by combining the distance and heuristic values
            /// </summary>
            public int Cost => distance + heuristic;
        };

        struct NodeDataComparator : IComparer<NodeData>
        {
            public int Compare(NodeData x, NodeData y)
            {
                if (x.Cost == y.Cost)
                    return 0;
                else if (x.Cost < y.Cost)
                    return -1;
                else return 1;
            }
        }
        public enum Distance
        {
	        Manhattan,
	        Chebyshev,
	        Octile,
	        Euclidean
        };

        public int CalculateHeuristic(in Coord current, in Coord destination, in Distance distance = Distance.Euclidean)
        {
	        int dx = Math.Abs((int)current.x - (int)destination.x);
	        int dy = Math.Abs((int)current.y - (int)destination.y);

	        switch (distance)
	        {
		        case Distance.Manhattan:
			        return dx + dy;
		        case Distance.Chebyshev:
			        return Math.Max(dx, dy);
		        case Distance.Octile:
			        return (int)(1.0 * (dx + dy) + (1.414 - 2.0 * 1.0) * Math.Min(dx, dy));
		        case Distance.Euclidean:
			        return (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                default:
                    return 0;
	        }
        }

        public Stack<Coord> CalculatePath(in Coord origin, in Coord destination)
        {
            PriorityQueue<NodeData, NodeDataComparator> frontier = new PriorityQueue<NodeData, NodeDataComparator>();

            frontier.Enqueue(new NodeData(origin, 0, CalculateHeuristic(origin, destination)), new NodeDataComparator());

            Dictionary<Coord, (Coord, int)> came_from = new Dictionary<Coord, (Coord, int)>();

            came_from[origin] = (origin, 0);

            while (frontier.Count > 0)
            {
                Coord currentPosition = frontier.Dequeue().position;

                if (currentPosition == destination)
                    break;

                var neighbourhood = GetNeighbourhood(currentPosition);

                foreach (var neighbor in neighbourhood)
			    {
                    if (neighbor != null)
                    {
                        Coord neighborPos = neighbor.Position;

                        bool solid = neighbor.Solid;
                        int newDistance = came_from[currentPosition].Item2 + 1; // Assuming constant distance for adjacent cells

                        if ((came_from[neighborPos] == came_from.Last().Value || newDistance < came_from[neighborPos].Item2) && !solid)
                        {
                            frontier.Enqueue(new NodeData(neighborPos, newDistance, CalculateHeuristic(neighborPos, destination)), new NodeDataComparator());
                            came_from[neighborPos] = (currentPosition, newDistance);
                        }
                    }
                }
            }

            Stack<Coord> path = new Stack<Coord>();

            Coord current = destination;

            if (came_from[destination] == came_from.Last().Value)
            {
                return path;
            }
            while (current != origin)
            {
                path.Push(current);
                current = came_from[current].Item1;
            }

            return path;
        }

        public List<Cell?> GetNeighbourhood(in Coord position)
        {
            Cell? cell = this[position];

            if (cell == null) throw new InvalidOperationException(nameof(position));

            return cell.Neighbours;
        }

        public byte GetNeighbourCount(in Coord position)
        {
            byte neighbourCount = 0;

            for (int y_offset = -1; y_offset <= 1; y_offset++)
                for (int x_offset = -1; x_offset <= 1; x_offset++)
                {
                    Point currentOffset = new Point(x_offset, y_offset);
                    Coord currentPosition = position - currentOffset;

                    if (currentPosition != position)
                    {
                        Cell? currentCell = this[currentPosition];

                        if (currentCell == null || currentCell.Solid)
                            neighbourCount++;
                    }
                }

            return neighbourCount;
        }
    }
}
