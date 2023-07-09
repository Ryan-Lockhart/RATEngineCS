using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;
using rat.Constants;
using rat.Random;

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

		private void ShadowCast(in Coord origin, int row, double start, double end, in Octant octant, double radius)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Bounds.Area() && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Coord currentPosition{ static_cast<coord_t>(origin.X + deltaX * octant.x + deltaY * octant.dx), static_cast<coord_t>(origin.Y + deltaX * octant.y + deltaY * octant.dy), origin.Z };
                    Coord delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = math::normalize(deltaX, deltaY);

                    Cell* cell = GetCell(currentPosition);

                    if (cell != nullptr)
                    {
                        bool opaque = cell->IsOpaque();

                        if (deltaRadius <= radius)
                        {
                            cell->Explore();
                            cell->See();
                        }

                        if (blocked)
                        {
                            if (opaque)
                                newStart = rightSlope;
                            else
                            {
                                blocked = false;
                                start = newStart;
                            }
                        }
                        else if (opaque && distance < radius)
                        {
                            blocked = true;

                            ShadowCast(origin, distance + 1, start, leftSlope, octant, radius);

                            newStart = rightSlope;
                        }
                    }
                }
            }
        }

		private void ShadowCastLimited(in Coord origin, int row, double start, double end, in Octant octant, double radius, double angle, double span)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Bounds.Area() && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Coord currentPosition{ static_cast<coord_t>(origin.X + deltaX * octant.x + deltaY * octant.dx), static_cast<coord_t>(origin.Y + deltaX * octant.y + deltaY * octant.dy), origin.Z };
                    Coord delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = math::normalize(deltaX, deltaY);
                    double at2 = abs(angle - math::atan2_agnostic((double)delta.Y, (double)delta.X));

                    Cell* cell = GetCell(currentPosition);

                    if (cell != nullptr)
                    {
                        bool opaque = cell->IsOpaque();

                        if (deltaRadius <= radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                        {
                            cell->Explore();
                            cell->See();
                        }

                        if (blocked)
                        {
                            if (opaque)
                                newStart = rightSlope;
                            else
                            {
                                blocked = false;
                                start = newStart;
                            }
                        }
                        else if (opaque && distance < radius)
                        {
                            blocked = true;

                            ShadowCastLimited(origin, distance + 1, start, leftSlope, octant, radius, angle, span);

                            newStart = rightSlope;
                        }
                    }
                }
            }
        }

		private void ShadowCast(in Coord origin, ref Coord[] fovVector, int row, double start, double end, in Octant octant, double radius)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Bounds.Area() && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Coord currentPosition{ static_cast<coord_t>(origin.X + deltaX * octant.x + deltaY * octant.dx), static_cast<coord_t>(origin.Y + deltaX * octant.y + deltaY * octant.dy), origin.Z };
                    Coord delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = math::normalize(deltaX, deltaY);

                    Cell* cell = GetCell(currentPosition);

                    if (cell != nullptr)
                    {
                        bool opaque = cell->IsOpaque();

                        if (deltaRadius <= radius)
                            fovVector.push_back(cell->GetPosition());

                        if (blocked)
                        {
                            if (opaque)
                                newStart = rightSlope;
                            else
                            {
                                blocked = false;
                                start = newStart;
                            }
                        }
                        else if (opaque && distance < radius)
                        {
                            blocked = true;

                            ShadowCast(origin, fovVector, distance + 1, start, leftSlope, octant, radius);

                            newStart = rightSlope;
                        }
                    }
                }
            }
        }

		private void ShadowCastLimited(in Coord origin, ref Coord[] fovVector, int row, double start, double end, in Octant octant, double radius, double angle, double span)
        {
            double newStart = 0;

            if (start < end)
                return;

            bool blocked = false;

            for (double distance = row; distance <= radius && distance < m_Bounds.Area() && !blocked; distance++)
            {
                double deltaY = -distance;

                for (double deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    Coord currentPosition{ static_cast<coord_t>(origin.X + deltaX * octant.x + deltaY * octant.dx), static_cast<coord_t>(origin.Y + deltaX * octant.y + deltaY * octant.dy), origin.Z };
                    Coord delta = currentPosition - origin;

                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!IsValid(currentPosition) || start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    double deltaRadius = math::normalize(deltaX, deltaY);
                    double at2 = abs(angle - math::atan2_agnostic((double)delta.Y, (double)delta.X));

                    Cell* cell = GetCell(currentPosition);

                    if (cell != nullptr)
                    {
                        bool opaque = cell->IsOpaque();

                        if (deltaRadius <= radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                            fovVector.push_back(cell->GetPosition());

                        if (blocked)
                        {
                            if (opaque)
                                newStart = rightSlope;
                            else
                            {
                                blocked = false;
                                start = newStart;
                            }
                        }
                        else if (opaque && distance < radius)
                        {
                            blocked = true;

                            ShadowCastLimited(origin, fovVector, distance + 1, start, leftSlope, octant, radius, angle, span);

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

            m_Solids = new BitArray((int)m_Bounds.Volume);
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
            BitArray smoothed = new BitArray(m_Solids);

            for (ulong i = 0; i < (ulong)iterations; i++)
            {
                for (long z = 0; z < m_Bounds.depth; z++)
                    for (long y = 0; y < m_Bounds.height; y++)
                        for (long x = 0; x < m_Bounds.width; x++)
                        {
                            Coord coord = new Coord(x, y, z);

                            smoothed[Index(coord)] = WithinBounds(coord) ? Automatize(coord, threshold) : true;
                        }

                BitArray temp = m_Solids;
                m_Solids = smoothed;
                smoothed = temp;
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

		public bool IsSolid(in Coord position, in BitArray solids)
		{
			return IsValid(position) ?
				m_Solids[Index(position)] : true;
		}

		public void ResetSeen()
        {
            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord position = new Coord(x, y, z);

                        this[position].Seen = false;
                    }
        }

        public void RevealMap()
        {
            for (long z = 0; z < m_Bounds.depth; z++)
                for (long y = 0; y < m_Bounds.height; y++)
                    for (long x = 0; x < m_Bounds.width; x++)
                    {
                        Coord position = new Coord(x, y, z);

                        this[position].Seen = true;
                        this[position].Explored = true;
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
            throw new NotImplementedException();
        }

        public bool WithinBounds(in Coord position)
        {
            throw new NotImplementedException();
        }

        public bool IsGenerating() => m_Generating;

        public void CalculateFOV(in Coord origin, double viewDistance)
        {
            ResetSeen();

            viewDistance = std::max<double>(1, viewDistance);

            Cell* cell = GetCell(origin);

            if (cell != nullptr)
            {
                cell->See();
                cell->Explore();
            }

            for (int i = 0; i < 8; i++)
            {
                ShadowCast(origin, 1, 1.0, 0.0, octants[i], viewDistance);
            }
        }

        public void CalculateFOV(in Coord origin, double viewDistance, double angle, double span)
        {
            ResetSeen();

            viewDistance = std::max<double>(1, viewDistance);

            angle = (angle > 360.0 || angle < 0.0 ? math::wrap_around(angle, 360.0) : angle) * math::deg_percent_of_circle;
            span *= math::deg_percent_of_circle;

            Cell* cell = GetCell(origin);

            if (cell != nullptr)
            {
                auto neighbours = GetNeighbourhood(origin, false);

                for (auto & neighbour : neighbours)
                    if (neighbour != nullptr)
                    {
                        neighbour->See();
                        neighbour->Explore();
                    }

                cell->See();
                cell->Explore();
            }

            for (int i = 0; i < 8; i++)
            {
                ShadowCastLimited(origin, 1, 1.0, 0.0, octants[i], viewDistance, angle, span);
            }
        }

        public void CalculateFOV(in Coord origin, double viewDistance, double angle, double span, double nudge)
        {
            ResetSeen();

		    Coord shiftedOrigin = nudge > 0 ?
			    Coord{
			    static_cast<coord_t>(origin.X + cos(math::deg_to_rad * angle) * -nudge),
				    origin.Y + static_cast<coord_t>(sin(math::deg_to_rad * angle) * -nudge),
				    origin.Z
		    } : origin;

		    viewDistance = std::max<double>(1, viewDistance);

		    angle = (angle > 360.0 || angle < 0.0 ? math::wrap_around(angle, 360.0) : angle) * math::deg_percent_of_circle;
		    span *= math::deg_percent_of_circle;

		    Cell* cell = GetCell(shiftedOrigin);

		    if (cell != nullptr)
		    {
			    auto neighbours = GetNeighbourhood(shiftedOrigin, false);

			    for (auto& neighbour : neighbours)
				    if (neighbour != nullptr)
				    {
					    neighbour->See();
					    neighbour->Explore();
				    }

			    cell->See();
			    cell->Explore();
		    }

		    for (int i = 0; i < 8; i++)
		    {
			    ShadowCastLimited(shiftedOrigin, 1, 1.0, 0.0, octants[i], viewDistance, angle, span);
		    }
        }

        public List<Coord> WithinFOV(in Coord origin, double viewDistance)
        {
            std::vector<Coord> fovVector;

            viewDistance = std::max<double>(1, viewDistance);

            Cell* cell = GetCell(origin);

            if (cell != nullptr)
            {
                auto neighbours = GetNeighbourhood(origin, false);

                for (auto & neighbour : neighbours)
                    if (neighbour != nullptr)
                        fovVector.push_back(neighbour->GetPosition());

                fovVector.push_back(cell->GetPosition());
            }

            for (int i = 0; i < 8; i++)
            {
                ShadowCast(origin, fovVector, 1, 1.0, 0.0, octants[i], viewDistance);
            }

            return fovVector;
        }

        public List<Coord> WithinFOV(in Coord origin, double viewDistance, double angle, double span)
        {
            std::vector<Coord> fovVector;

            viewDistance = std::max<double>(1, viewDistance);

            angle = (angle > 360.0 || angle < 0.0 ? math::wrap_around(angle, 360.0) : angle) * math::deg_percent_of_circle;
            span *= math::deg_percent_of_circle;

            Cell* cell = GetCell(origin);

            if (cell != nullptr)
            {
                auto neighbours = GetNeighbourhood(origin, false);

                for (auto & neighbour : neighbours)
                    if (neighbour != nullptr)
                        fovVector.push_back(neighbour->GetPosition());

                fovVector.push_back(cell->GetPosition());
            }

            for (int i = 0; i < 8; i++)
            {
                ShadowCastLimited(origin, fovVector, 1, 1.0, 0.0, octants[i], viewDistance, angle, span);
            }

            return fovVector;
        }

        public List<Coord> WithinFOV(in Coord origin, double viewDistance, double angle, double span, double nudge)
        {
            std::vector<Coord> fovVector;

		    Coord shiftedOrigin = nudge > 0 ?
			    Coord{
			    static_cast<coord_t>(origin.X + cos(math::deg_to_rad * angle) * -nudge),
				    origin.Y + static_cast<coord_t>(sin(math::deg_to_rad * angle) * -nudge),
				    origin.Z
		    } : origin;

		    viewDistance = std::max<double>(1, viewDistance);

		    angle = (angle > 360.0 || angle < 0.0 ? math::wrap_around(angle, 360.0) : angle) * math::deg_percent_of_circle;
		    span *= math::deg_percent_of_circle;

		    Cell* cell = GetCell(shiftedOrigin);

		    if (cell != nullptr)
		    {
			    auto neighbours = GetNeighbourhood(shiftedOrigin, false);

			    for (auto& neighbour : neighbours)
				    if (neighbour != nullptr)
					    fovVector.push_back(neighbour->GetPosition());

			    fovVector.push_back(cell->GetPosition());
		    }

		    for (int i = 0; i < 8; i++)
		    {
			    ShadowCastLimited(shiftedOrigin, fovVector, 1, 1.0, 0.0, octants[i], viewDistance, angle, span);
		    }

		    return fovVector;
        }

        public Stack<Coord> CalculatePath(in Coord origin, in Coord destination)
        {
            std::priority_queue<NodeData, std::vector<NodeData>, NodeDataComparator> frontier;
            frontier.push({ origin, 0, CalculateHeuristic(origin, destination) });

            std::unordered_map<Coord, std::pair<Coord, int>> came_from;
            came_from[origin] = std::make_pair(origin, 0);

            while (!frontier.empty())
            {
                Coord current = frontier.top().position;
                frontier.pop();

                if (current == destination)
                    break;

                auto & neighbourhood = GetNeighbourhood(current);

                for (const auto&neighborPtr : neighbourhood)
			    {
                    if (neighborPtr != nullptr)
                    {
                        Coord neighborPos = neighborPtr->GetPosition();

                        bool solid = neighborPtr->IsSolid();
                        int newDistance = came_from[current].second + 1; // Assuming constant distance for adjacent cells

                        if ((came_from.find(neighborPos) == came_from.end() || newDistance < came_from[neighborPos].second) && !solid)
                        {
                            int heuristic = CalculateHeuristic(neighborPos, destination);
                            frontier.push({ neighborPos, newDistance, heuristic });
                            came_from[neighborPos] = std::make_pair(current, newDistance);
                        }
                    }
                }
            }

            std::stack<Coord> path;

            Coord current = destination;
            if (came_from.find(destination) == came_from.end())
            {
                return path;
            }
            while (current != origin)
            {
                path.push(current);
                current = came_from[current].first;
            }

            return path;
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
