using GoRogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;

namespace rat
{
    public class Partitioner
    {
        private List<Area> m_Areas;

        public List<Area> Areas => m_Areas;
        public int Size => m_Areas.Count;

        public Area LargestArea
        {
            get
            {
                Area largestArea = m_Areas.First();

                foreach (Area area in m_Areas)
                    if (largestArea.Size < area.Size)
                        largestArea = area;

                return largestArea;
            }
        }

        public Partitioner(in Map map)
        {
            m_Areas = new List<Area>();

            List<Cell> cells = new List<Cell>(map.Cells);
            List<Cell> openCells = new List<Cell>();

            foreach (var cell in cells)
                if (cell.Open)
                    openCells.Add(cell);

            while (openCells.Count > 0)
            {
                var randomCell = openCells[Globals.Generator.Next(0, openCells.Count)];

                if (randomCell == null) continue;
                if (randomCell.Solid) continue;

                var area = new Area(map, randomCell.Position);
                
                foreach (var cell in area.Cells)
                    openCells.Remove(cell);

                m_Areas.Add(area);
            }
        }
    }

    public class Area
    {
        private List<Cell> m_Cells;

        public List<Cell> Cells => m_Cells;
        public int Size => m_Cells.Count;

        /// <summary>
        /// Generate an <see cref="Area"/> by flood filling from a <see cref="Point"/> on the <see cref="Map"/>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="origin"></param>
        public Area(in Map map, Point origin)
        {
            var origin_cell = map[origin];

            if (origin_cell == null) throw new ArgumentNullException("Cell at origin of area is null!");
            if (origin_cell.Solid) throw new ArgumentException("Cell at origin of area is solid!");

            m_Cells = new List<Cell>();

            Queue<Cell?> frontier = new Queue<Cell?>();
            HashSet<Cell> visited = new HashSet<Cell>();

            frontier.Enqueue(origin_cell);
            
            while (frontier.Count > 0)
            {
                var cell = frontier.Dequeue();

                if (cell == null) continue;
                if (visited.Contains(cell)) continue;
                if (cell.Solid) continue;

                visited.Add(cell);
                m_Cells.Add(cell);

                var neighbourhood = map.GetNeighbourhood(cell.Position);

                foreach (var neighbour in neighbourhood)
                    frontier.Enqueue(neighbour);
            }
        }
    }
}
