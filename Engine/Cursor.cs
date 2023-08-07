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
    public enum Phase
    {
        Start,
        Active,
        Complete
    }

    public class Selection
    {
        private Cursor m_Cursor;

        private Point m_StartPosition;
        private Point m_EndPosition;

        private Rect m_Transform;

        private Phase m_Phase;

        private List<Cell?> m_Cells;
        private List<Actor?> m_Actors;

        public Cursor Cursor { get => m_Cursor; set => m_Cursor = value; }

        public Rect Transform { get => m_Transform; set => m_Transform = value; }

        public Phase Phase { get => m_Phase; set => m_Phase = value; }

        public List<Cell?> Cells { get => m_Cells; set => m_Cells = value; }
        public List<Actor?> Actors { get => m_Actors; set => m_Actors = value; }

        public Selection(Cursor cursor)
        {
            m_Cursor = cursor;

            m_Cells = new List<Cell?>();
            m_Actors = new List<Actor?>();

            m_Phase = Phase.Start;
        }

        private void CalculateSelection()
        {
            if (m_Cursor.Parent == null) return;
            if (m_EndPosition == m_StartPosition) return;

            Point direction = m_EndPosition - m_StartPosition;

            for (int y = m_StartPosition.y; direction.y > 0 ? y < m_EndPosition.y : y > m_EndPosition.y; y += direction.y)
                for (int x = m_StartPosition.x; direction.x > 0 ? x < m_EndPosition.y : x > m_EndPosition.x; x += direction.x)
                {
                    Point currentPosition = new Point(x, y);

                    Cell? cell = m_Cursor.Parent[currentPosition];
                }
        }

        public void Update()
        {
            if (m_Phase != Phase.Active) return;

            if (m_Cursor.Cell ==  null) return;

            m_EndPosition = m_Cursor.Cell.Position;

            CalculateSelection();
        }

        public void BeginSelection()
        {
            if (m_Cursor.Cell == null) return;

            m_StartPosition = m_Cursor.Cell.Position;

            m_Phase = Phase.Active;
        }
    }

    public class Cursor
    {
        private Map m_Parent;

        private Cell? m_Cell;
        private Actor? m_Actor;

        private Selection m_Selection;

        private Rect m_Transform;
        private Color m_Color;

        private TextAlignment m_Alignment;

        public event EventHandler OnCursorMoved;

        public event EventHandler OnCellFound;
        public event EventHandler OnActorFound;

        public event EventHandler OnSelectionStarted;
        public event EventHandler OnSelectionFinished;

        public Cursor(in Point position, in Size size, in Map map)
        {
            m_Parent = map;
            m_Transform = new Rect(position, size);

            m_Selection = new Selection(this);
        }

        public Cursor(in Rect transform, in Map map)
        {
            m_Parent = map;
            m_Transform = transform;

            m_Selection = new Selection(this);
        }

        public Map Parent => m_Parent;

        public Cell? Cell => m_Cell;

        public Actor? Actor => m_Actor;

        public Selection Selection { get => m_Selection; set => m_Selection = value; }

        public Rect Transform { get => m_Transform; set => m_Transform = value; }

        public Point ScreenPosition => Transform.position - Parent.Position;

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

            m_Cell = m_Parent[m_Transform.position];

            m_Actor = m_Cell != null ? m_Cell.Occupant : null;

            m_Color = m_Actor != null && m_Cell != null ? m_Cell.Seen ? m_Actor.Glyph.color : Colors.White : Colors.White;
        }

        public void Draw(in Map map, in GlyphSet glyphSet, bool attached = false)
        {
            Rect drawRect = new Rect(Transform.position - map.Position + Screens.MapDisplay.position, Transform.size);

            Globals.Engine.DrawRect(drawRect, Color, glyphSet.GlyphSize);

            Point offset = new Point(
                Alignment.horizontal == HorizontalAlignment.Right ? -1 : Alignment.horizontal == HorizontalAlignment.Left ? 2 : 0,
                Alignment.vertical == VerticalAlignment.Lower ? -1 : Alignment.vertical == VerticalAlignment.Upper ? 2 : 0
            );

            string text = "";

            if (Cell != null)
            {
                if (Cell.Seen)
                {
                    if (Actor != null)
                    {
                        text = $"{Transform.position}, {Actor.Name} the {Actor.Species.Name} \n{Actor.Species.Description}";

                        if (Actor != Globals.Engine.Player)
                        {
                            Relation? SeenToPlayer = Globals.Engine.Population.Relations[Actor, Globals.Engine.Player];
                            string SeenToPlayerString = SeenToPlayer != null ? SeenToPlayer.ToString() : $"The {Actor.Name} does not know {Globals.Engine.Player.Name}";

                            text += $"\n\n{SeenToPlayerString}";
                        }
                    }
                    else text = $"{Transform.position}, {(Cell != null ? Cell.State : " ??? ")}";
                }
                else if (Cell.Explored)
                {
                    text = $"{Transform.position}, {(Cell != null ? Cell.State : "\"???\"")}";
                }
                else text = Transform.position + ", ???";

                var corpses = Cell!.Corpses;

                if (corpses != null)
                {
                    if (corpses.Count > 0)
                    {
                        text += "\n\nCorpses:";

                        if (Settings.UseCorpseLimit)
                        {
                            int i = 0;

                            foreach (var corpse in corpses)
                            {
                                if (corpse == null || !corpse.Observed)
                                    continue;

                                i++;

                                var actor = corpse.Actor;
                                if (actor != null) { text += $"\n {actor.Name} the {actor.Species.Name}"; }

                                if (i > Settings.CorpseLimit) break;
                            }

                            if (corpses.Count > Settings.CorpseLimit)
                                text += $"\n +{corpses.Count - Settings.CorpseLimit} more...";
                        }
                        else
                        {
                            foreach (var corpse in corpses)
                            {
                                if (corpse == null || !corpse.Observed) continue;
                                var actor = corpse.Actor;
                                if (actor != null) { text += $"\n {actor.Name} the {actor.Species.Name}"; }
                            }
                        }
                    }
                }
            }
            else
            {
                text = Transform.position + ", ???";
            }

            Globals.Engine.DrawLabel(
                text,
                attached ?
                    Screens.UIToMap(drawRect.position + (attached ? offset : Point.Zero)) :
                    Screens.FixedTooltip,
                Size.One,
                attached ?
                    Alignment :
                    Alignments.LowerRight,
                Colors.White
            );
        }

        public bool Input()
        {
            if (Actor != null)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                    NameGenerator.Globals.Basic.Approve(Actor.Name);
                else if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                    NameGenerator.Globals.Basic.Reject(Actor.Name);
            }

            return false;
        }
    }
}
