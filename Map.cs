using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Metadata;
using System.Collections.Specialized;

using rat.Primitives;
using rat.Constants;
using System.Numerics;
using Raylib_cs;

namespace rat
{
    public struct WorldGenerationSettings
    {
        public float fillPercent;
        public int iterations;
        public int threshold;

        public WorldGenerationSettings(float fillPercent, int iterations, int threshold)
        {
            this.fillPercent = fillPercent;
            this.iterations = iterations;
            this.threshold = threshold;
        }
    }

    public class Map
    {
		private bool m_Generating;

		private BitArray m_Solids;
		private Cell[] m_Cells;

        public List<Cell> Cells => m_Cells.ToList();

		private Point m_Position;

		private Size m_Size;
		private Size m_Border;

        private bool m_Revealed;

        public Point Position { get => m_Position; private set => m_Position = value; }
        public bool Revealed => m_Revealed;

        public Cell? this[in Point position]
        {
            get
            {
                if (!IsValid(position)) { return null; }
                else
                {
                    try { return m_Cells[Index(position)]; }
                    catch { return null; }
                }
            }
        }

        private Cell this[int index]
        {
            get
            {
                try { return m_Cells[index]; }
                catch { throw; }
            }
        }

        private int Index(in Point position)
		{
			return position.y * m_Size.width + position.x;
        }

        private int Index(int x, int y)
        {
            return y * m_Size.height + x;
        }

        private void ConstrainToScreen()
		{
            var delta = Screens.MapDisplay.size - m_Size;

            bool center_width = delta.width > 0;
            bool center_height = delta.height > 0;

            if (center_width)
                m_Position.x = -delta.width / 2;
            else if (m_Position.x < 0)
                m_Position.x = 0;
            else if (m_Position.x > m_Size.width - Screens.MapDisplay.size.width)
                m_Position.x = m_Size.width - Screens.MapDisplay.size.width;


            if (center_height)
                m_Position.y = -delta.height / 2;
            else if (m_Position.y < 0)
                m_Position.y = 0;
            else if (m_Position.y > m_Size.height - Screens.MapDisplay.size.height)
                m_Position.y = m_Size.height - Screens.MapDisplay.size.height;
        }

		private void ShadowCast(in Point origin, List<Point> fov, int row, double start, double end, in Octant octant, double radius)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Size.Area && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Point currentPosition = new Point((int)(origin.x + deltaX * octant.x + deltaY * octant.dx), (int)(origin.y + deltaX * octant.y + deltaY * octant.dy));

                    double leftSlope = (deltaX - 0.5) / (deltaY + 0.5);
                    double rightSlope = (deltaX + 0.5) / (deltaY - 0.5);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = Math.Normalize(deltaX, deltaY);

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

		private void ShadowCast(in Point origin, List<Point> fov, int row, double start, double end, in Octant octant, double radius, double angle, double span)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Size.Area && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Point currentPosition = new Point((int)(origin.x + deltaX * octant.x + deltaY * octant.dx), (int)(origin.y + deltaX * octant.y + deltaY * octant.dy));
                    Point delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5) / (deltaY + 0.5);
                    double rightSlope = (deltaX + 0.5) / (deltaY - 0.5);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = Math.Normalize(deltaX, deltaY);
                    double at2 = System.Math.Abs(angle - Math.Atan2(delta.y, delta.x));

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

		public Map(in Size size, in Size border)
		{
            m_Position = Point.Zero;

            m_Size = size;
            m_Border = border;

            m_Solids = new BitArray(m_Size.Area, false);
            m_Cells = new Cell[m_Size.Area];
        }

        public Map(in Size size, in Size border, float fillPercent, int iterations, int threshold)
        {
            m_Position = Point.Zero;

            m_Size = size;
            m_Border = border;

            m_Solids = new BitArray(m_Size.Area, false);
            m_Cells = new Cell[m_Size.Area];

            Generate(fillPercent);
            Smooth(iterations, threshold);
            Populate();
        }

        public Map(in Size size, in Size border, in WorldGenerationSettings settings)
        {
            m_Position = Point.Zero;

            m_Size = size;
            m_Border = border;

            m_Solids = new BitArray(m_Size.Area, false);
            m_Cells = new Cell[m_Size.Area];

            Generate(settings.fillPercent);
            Smooth(settings.iterations, settings.threshold);
            Populate();
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

            for (int y = 0; y < m_Size.height; y++)
                for (int x = 0; x < m_Size.width; x++)
                {
                    Point position = new Point(x, y);

                    m_Solids[Index(position)] = WithinBounds(position) ? Globals.Generator.NextBool(fillPercent) : true;
                }
        }

        public void Smooth(int iterations = 5, int threshold = 4)
		{
            BitArray smoothed = new BitArray(m_Solids);

            for (int i = 0; i < iterations; i++)
            {
                for (int y = 0; y < m_Size.height; y++)
                    for (int x = 0; x < m_Size.width; x++)
                    {
                        Point position = new Point(x, y);

                        smoothed[Index(position)] = WithinBounds(position) ? Automatize(position, threshold, m_Solids) : true;
                    }

                (smoothed, m_Solids) = (m_Solids, smoothed);
            }
        }

		public bool Automatize(in Point position, int threshold, in BitArray solids)
		{
            bool originalState = m_Solids[Index(position)];

            int neighbours = 0;

            for (int offset_y = -1; offset_y < 2; offset_y++)
                for (int offset_x = -1; offset_x < 2; offset_x++)
                    if (offset_x != 0 || offset_y != 0)
                        neighbours += IsSolid(position + new Point(offset_x, offset_y), solids) ? 1 : 0;

            return neighbours > threshold ? true : neighbours < threshold ? false : originalState;
        }

        public void Populate()
        {
            for (int y = 0; y < m_Size.height; y++)
                for (int x = 0; x < m_Size.width; x++)
                {
                    Point position = new Point(x, y);

                    bool solid = IsSolid(position, m_Solids);

                    if (this[position] != null)
                        m_Cells[Index(position)].Reinitialize(position, solid, solid);
                    else
                        m_Cells[Index(position)] = new Cell(position, this, solid, solid);
                }

            for (int y = 0; y < m_Size.height; y++)
                for (int x = 0; x < m_Size.width; x++)
                {
                    Point position = new Point(x, y);

                    m_Cells[Index(position)].GenerateNeighbourhood();
                }

            RecalculateIndices();

            m_Generating = false;
        }

        public void CloseHoles()
        {
            var partitioner = new Partitioner(this);

            var largestArea = partitioner.LargestArea;

            foreach (var area in partitioner.Areas)
                if (area != largestArea)
                    foreach (var cell in area.Cells)
                        cell.Solid = true;
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
            int maxChecks = (int)(m_Size.Area * checkPercent);
            int checks = 0;

            while (checks < maxChecks)
            {
                int x = Globals.Generator.Next(0, m_Size.width - 1);
                int y = Globals.Generator.Next(0, m_Size.height - 1);

                Point randomPosition = new Point(x, y);

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

        public bool IsSolid(in Point position)
        {
            return IsValid(position) ?
                m_Solids[Index(position)] : true;
        }

        public bool IsSolid(in Point position, in BitArray solids)
		{
			return IsValid(position) ?
				solids[Index(position)] : true;
		}

        /// <summary>
        /// Reset the fog of war
        /// </summary>
		public void ResetSeen()
        {
            for (int y = 0; y < m_Size.height; y++)
                for (int x = 0; x < m_Size.width; x++)
                    this[Index(x, y)].Seen = false;
        }

        /// <summary>
        /// Reveal the entire map
        /// </summary>
        public void RevealMap(bool onlyFog = true)
        {
            if (m_Revealed) return;

            for (int y = 0; y < m_Size.height; y++)
                for (int x = 0; x < m_Size.width; x++)
                {
                    Cell cell = this[Index(x, y)];

                    cell.Explored = true;

                    if (!onlyFog)
                        cell.Seen = true;
                }

            m_Revealed = true;
        }

        public void RevealMap(in List<Point> fov, bool resetSeen = true)
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

        public void CenterOn(in Point position)
        {
            if (!IsValid(position))
                return;

            m_Position.x = position.x - Screens.MapDisplay.size.width / 2;
            m_Position.y = position.y - Screens.MapDisplay.size.height / 2;

            ConstrainToScreen();
        }

        public void Draw(in GlyphSet glyphSet, in Point offset)
		{
            for (int y = m_Position.y; y < m_Position.y + Screens.MapDisplay.size.height; y++)
                for (int x = m_Position.x; x < m_Position.x + Screens.MapDisplay.size.width; x++)
                {
                    Point currentPosition = new Point(x, y);

                    if (!IsValid(currentPosition)) continue;

                    int index = Index(currentPosition);

                    if (m_Generating)
                        glyphSet.DrawGlyph(m_Solids[index] ? Glyphs.ASCII.Wall : Glyphs.ASCII.Floor, Position + currentPosition - offset);
                    else
                    {
                        Cell? cell = this[currentPosition];

                        if (cell != null)
                            cell.Draw(glyphSet, m_Position - offset, true);
                        else glyphSet.DrawGlyph(m_Solids[index] ? Glyphs.ASCII.Wall : Glyphs.ASCII.Floor, Position + currentPosition - offset);
                    }
                }
        }

		public virtual void Update()
        {
            foreach (var cell in m_Cells)
                if (cell != null) cell.Update();
        }

        public bool IsValid(in Point position)
            => position.x >= 0 && position.y >= 0 && position.x < m_Size.width && position.y < m_Size.height;

        public bool WithinBounds(in Point position)
            => position.x >= m_Border.width && position.y >= m_Border.height && position.x < m_Size.width - m_Border.width && position.y < m_Size.height - m_Border.height;

        public bool IsGenerating() => m_Generating;

        public List<Point> CalculateFOV(in Point origin, double viewDistance)
        {
            List<Point> fov = new List<Point>();

            viewDistance = System.Math.Max(1, viewDistance);

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

        public List<Point> CalculateFOV(in Point origin, double viewDistance, double angle, double span)
        {
            List<Point> fov = new List<Point>();

            viewDistance = System.Math.Max(1, viewDistance);

            angle = (angle > 360.0 || angle < 0.0 ? Math.WrapAround(angle, 360.0) : angle) * Math.PercentOfCircle;
            span *= Math.PercentOfCircle;

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

        public List<Point> CalculateFOV(in Point origin, double viewDistance, double angle, double span, double nudge)
        {
            List<Point> fov = new List<Point>();

            double radians = Math.ToRadians(angle);

            Point shiftedOrigin = nudge != 0 ?
			    new Point( origin.x + (int)(System.Math.Cos(radians) * nudge), origin.y + (int)(System.Math.Sin(radians) * nudge)) :
                origin;

		    viewDistance = System.Math.Max(1, viewDistance);

		    angle = (angle > 360.0 || angle < 0.0 ? Math.WrapAround(angle, 360.0) : angle) * Math.PercentOfCircle;
		    span *= Math.PercentOfCircle;

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

        public Stack<Point>? CalculatePath(in Point origin, in Point destination)
        {
            PriorityQueue<Point, double> frontier = new PriorityQueue<Point, double>();

            double initial_distance = Point.Distance(origin, destination);

            frontier.Enqueue(origin, initial_distance);

            Dictionary<Point, Point> came_from = new Dictionary<Point, Point>();
            Dictionary<Point, double> cost_so_far = new Dictionary<Point, double>();

            came_from[origin] = origin;
            cost_so_far[origin] = initial_distance;

            while (frontier.Count > 0)
            {
                Point currentPosition = frontier.Dequeue();

                if (currentPosition == destination)
                    break;

                var neighbourhood = GetNeighbourhood(currentPosition);

                foreach (var neighbor in neighbourhood)
                {
                    if (neighbor != null)
                    {
                        if (neighbor.Solid) continue;

                        Point neighborPos = neighbor.Position;
                        double new_cost = Point.Distance(neighborPos, destination);

                        if (neighborPos != destination && neighbor.Occupied) continue;

                        if (!came_from.ContainsKey(neighbor.Position) || new_cost < cost_so_far[neighborPos])
                        {
                            frontier.Enqueue(neighborPos, new_cost);
                            came_from[neighborPos] = currentPosition;
                            cost_so_far[neighborPos] = new_cost;
                        }
                    }
                }

                if (frontier.Count <= 0)
                    return null;
            }

            Stack<Point> path = new Stack<Point>();

            Point current = destination;

            while (current != origin)
            {
                path.Push(current);
                current = came_from[current];
            }

            return path;
        }

        public List<Cell?> GetNeighbourhood(in Point position)
        {
            Cell? cell = this[position];

            if (cell == null) throw new InvalidOperationException(nameof(position));

            return cell.Neighbours;
        }

        public byte GetNeighbourCount(in Point position)
        {
            byte neighbourCount = 0;

            for (int y_offset = -1; y_offset <= 1; y_offset++)
                for (int x_offset = -1; x_offset <= 1; x_offset++)
                {
                    Point currentOffset = new Point(x_offset, y_offset);
                    Point currentPosition = position - currentOffset;

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
