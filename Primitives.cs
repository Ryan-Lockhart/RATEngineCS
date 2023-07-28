using GoRogue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static rat.Map;

namespace rat
{

    namespace Primitives
    {
        public enum Distance
        {
            Manhattan,
            Chebyshev,
            Octile,
            Euclidean
        };

        public enum Cardinal
        {
            Northwest,
            North,
            Northeast,
            West,
            Central,
            East,
            Southwest,
            South,
            Southeast
        }

        /// <summary>
        /// Two-dimensional structure representing a position in space
        /// </summary>
        public struct Point
        {
            public int x, y;

            public Point(in Point point)
            {
                x = point.x;
                y = point.y;
            }

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static readonly Dictionary<Cardinal, Point> Directions = new Dictionary<Cardinal, Point>()
            {
                { Cardinal.Northwest, Northwest },
                { Cardinal.North, North },
                { Cardinal.Northeast, Northeast },
                { Cardinal.West, West },
                { Cardinal.Central, Zero },
                { Cardinal.East, East },
                { Cardinal.Southwest, Southwest },
                { Cardinal.South, South },
                { Cardinal.Southeast, Southeast },
            };

            public static readonly Point Zero = new Point(0, 0);

            private static readonly Point North = new Point(0, -1);
            private static readonly Point South = new Point(0, 1);

            private static readonly Point West = new Point(-1, 0);
            private static readonly Point East = new Point(1, 0);

            private static readonly Point Northwest = North + West;
            private static readonly Point Northeast = North + East;

            private static readonly Point Southwest = South + West;
            private static readonly Point Southeast = South + East;

            public static implicit operator System.Numerics.Vector2(in Point point) => new System.Numerics.Vector2(point.x, point.y);
            public static implicit operator Point(in System.Numerics.Vector2 vector) => new Point((int)vector.X, (int)vector.Y);

            public static Point operator +(in Point lhs, in Point rhs) => new Point(lhs.x + rhs.x, lhs.y + rhs.y);
            public static Point operator +(in Point lhs, in Size rhs) => new Point(lhs.x + rhs.width, lhs.y + rhs.height);

            public static Point operator -(in Point lhs, in Point rhs) => new Point(lhs.x - rhs.x, lhs.y - rhs.y);
            public static Point operator -(in Point lhs, in Size rhs) => new Point(lhs.x - rhs.width, lhs.y - rhs.height);

            public static Point operator *(in Point lhs, in Point rhs) => new Point(lhs.x * rhs.x, lhs.y * rhs.y);
            public static Point operator *(in Point lhs, in Size rhs) => new Point(lhs.x * rhs.width, lhs.y * rhs.height);
            public static Point operator *(in Point lhs, double scalar) => new Point((int)(lhs.x * scalar), (int)(lhs.y * scalar));

            public static Point operator /(in Point lhs, in Point rhs) => new Point(lhs.x / rhs.x, lhs.y / rhs.y);
            public static Point operator /(in Point lhs, in Size rhs) => new Point(lhs.x / rhs.width, lhs.y / rhs.height);
            public static Point operator /(in Point lhs, double scalar) => new Point((int)(lhs.x / scalar), (int)(lhs.y / scalar));

            public static bool operator ==(in Point lhs, in Point rhs) => lhs.x == rhs.x && lhs.y == rhs.y;
            public static bool operator !=(in Point lhs, in Point rhs) => lhs.x != rhs.x || lhs.y != rhs.y;

            public override bool Equals(object? obj) => obj is Point point && x == point.x && y == point.y;
            public override int GetHashCode() => HashCode.Combine(x, y);

            public static Point Absolute(in Point coord) => new Point(System.Math.Abs(coord.x), System.Math.Abs(coord.y));

            public static Point Direction(in Point origin, in Point target) => target - origin;

            public static Point AbsoluteDirection(in Point origin, in Point target) => Absolute(Direction(origin, target));

            public static double Distance(in Point origin, in Point target, in Distance distance = Primitives.Distance.Chebyshev)
            {
                Point delta = AbsoluteDirection(origin, target);

                return distance switch
                {
                    Primitives.Distance.Manhattan => delta.x + delta.y,
                    Primitives.Distance.Chebyshev => System.Math.Max(delta.x, delta.y),
                    Primitives.Distance.Octile => 1.0f * (delta.x + delta.y) + (1.414f - 2.0f * 1.0f) * System.Math.Min(delta.x, delta.y),
                    Primitives.Distance.Euclidean => System.Math.Sqrt(System.Math.Pow(delta.x, 2f) + System.Math.Pow(delta.y, 2f)),
                    _ => 0.0,
                };
            }

            public static explicit operator Coord(in Point p) => new Coord(p.x, p.y, 0);
            public static Coord ToCoord(in Point p, int z) => new Coord(p.x, p.y, z);

            public override string ToString() => new string($"({x}, {y})");
        }

        /// <summary>
        /// Three-dimensional structure representing a position in space
        /// </summary>
        /// <remarks>
        /// This structure has been deprecated, as 3D <see cref="rat.Map"/>s are no longer in use
        /// </remarks>
        public struct Coord : IComparable<Coord>
        {
            public int x, y, z;

            /// <summary>
            /// Construct a <see cref="Coord"/> with an existing <see cref="Coord"/>
            /// </summary>
            /// <param name="coord">The <see cref="Coord"/> to clone</param>
            public Coord(in Coord coord)
            {
                x = coord.x;
                y = coord.y;
                z = coord.z;
            }

            /// <summary>
            /// Construct a <see cref="Coord"/> with an <see cref="int"/> for the X, Y, and Z axes
            /// </summary>
            /// <param name="x">The width on the X axis</param>
            /// <param name="y">The height on the Y axis</param>
            /// <param name="z">The depth on the Z axis</param>
            public Coord(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            /// <summary>
            /// Construct a <see cref="Coord"/> with a <see cref="Point"/> position and depth on the Z axis
            /// </summary>
            /// <param name="position"></param>
            /// <param name="z">The position on the Z axis (depth)</param>
            public Coord(in Point position, int z)
            {
                x = position.x;
                y = position.y;
                this.z = z;
            }

            /// <summary>
            /// A <see cref="Coord"/> representing no direction
            /// </summary>
            /// <returns>
            /// (X, Y, Z) -> (0, 0, 0)
            /// </returns>
            public static readonly Coord Zero = new Coord(0, 0, 0);

            /// <summary>
            /// Unit <see cref="Coord"/> representing the Up direction
            /// </summary>
            /// <returns>
            /// (X, Y, Z) -> (0, 0, 1)
            /// </returns>
            public static readonly Coord Up = new Coord(0, 0, 1);
            /// <summary>
            /// Unit <see cref="Coord"/> representing the Down direction
            /// </summary>
            /// <returns>
            /// (X, Y, Z) -> (0, 0, -1)
            /// </returns>
            public static readonly Coord Down = new Coord(0, 0, -1);

            public static readonly Coord West = new Coord(-1, 0, 0);
            public static readonly Coord East = new Coord(1, 0, 0);

            public static readonly Coord North = new Coord(0, -1, 0);
            public static readonly Coord South = new Coord(0, 1, 0);

            public static readonly Coord Northwest = North + West;
            public static readonly Coord Northeast = North + East;

            public static readonly Coord Southwest = South + West;
            public static readonly Coord Southeast = South + East;

            public static implicit operator System.Numerics.Vector3(in Coord coord) => new System.Numerics.Vector3(coord.x, coord.y, coord.z);
            public static implicit operator Coord(in System.Numerics.Vector3 vector) => new Coord((int)vector.X, (int)vector.Y, (int)vector.Z);

            /// <summary>
            /// Add two <see cref="Coord"/>s together
            /// </summary>
            /// <param name="lhs">The <see cref="Coord"/> on the left hand side of the operation</param>
            /// <param name="rhs">The <see cref="Coord"/> on the right hand side of the operation</param>
            /// <returns>A <see cref="Coord"/> of </returns>
            public static Coord operator +(in Coord lhs, in Coord rhs) => new Coord(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
            public static Coord operator +(in Coord lhs, in Point rhs) => new Coord(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z);
            public static Coord operator +(in Coord lhs, in Size rhs) => new Coord(lhs.x + rhs.width, lhs.y + rhs.height, lhs.z);
            public static Coord operator +(in Coord lhs, in Bounds rhs) => new Coord(lhs.x + rhs.width, lhs.y + rhs.height, lhs.z + rhs.depth);

            public static Coord operator -(in Coord lhs, in Coord rhs) => new Coord(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
            public static Coord operator -(in Coord lhs, in Point rhs) => new Coord(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z);
            public static Coord operator -(in Coord lhs, in Size rhs) => new Coord(lhs.x - rhs.width, lhs.y - rhs.height, lhs.z);
            public static Coord operator -(in Coord lhs, in Bounds rhs) => new Coord(lhs.x - rhs.width, lhs.y - rhs.height, lhs.z - rhs.depth);

            public static Coord operator *(in Coord lhs, in Coord rhs) => new Coord(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
            public static Coord operator *(in Coord lhs, in Point rhs) => new Coord(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z);
            public static Coord operator *(in Coord lhs, in Size rhs) => new Coord(lhs.x * rhs.width, lhs.y * rhs.height, lhs.z);
            public static Coord operator *(in Coord lhs, in Bounds rhs) => new Coord(lhs.x * rhs.width, lhs.y * rhs.height, lhs.z * rhs.depth);
            public static Coord operator *(in Coord lhs, double scalar) => new Coord((int)(lhs.x * scalar), (int)(lhs.y * scalar), (int)(lhs.z * scalar));

            public static Coord operator /(in Coord lhs, in Coord rhs) => new Coord(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
            public static Coord operator /(in Coord lhs, in Point rhs) => new Coord(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z);
            public static Coord operator /(in Coord lhs, in Size rhs) => new Coord(lhs.x / rhs.width, lhs.y / rhs.height, lhs.z);
            public static Coord operator /(in Coord lhs, in Bounds rhs) => new Coord(lhs.x / rhs.width, lhs.y / rhs.height, lhs.z / rhs.depth);
            public static Coord operator /(in Coord lhs, double scalar) => new Coord((int)(lhs.x / scalar), (int)(lhs.y / scalar), (int)(lhs.z / scalar));

            public static bool operator ==(in Coord lhs, in Coord rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
            public static bool operator !=(in Coord lhs, in Coord rhs) => lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;

            public override bool Equals(object? obj) => obj is Coord coord && x == coord.x && y == coord.y && z == coord.z;
            public override int GetHashCode() => HashCode.Combine(x, y, z);

            public static Coord Absolute(in Coord coord) => new Coord(System.Math.Abs(coord.x), System.Math.Abs(coord.y), System.Math.Abs(coord.z));

            public static Coord Direction(in Coord origin, in Coord target) => target - origin;

            public static Coord AbsoluteDirection(in Coord origin, in Coord target) => Absolute(Direction(origin, target));

            public static double Distance(in Coord origin, in Coord target, in Distance distance = Primitives.Distance.Chebyshev)
            {
                Coord delta = AbsoluteDirection(origin, target);

                switch (distance)
                {
                    case Primitives.Distance.Manhattan:
                        return delta.x + delta.y;
                    case Primitives.Distance.Chebyshev:
                        return MathF.Max(delta.x, delta.y);
                    case Primitives.Distance.Octile:
                        return 1.0f * (delta.x + delta.y) + (1.414f - 2.0f * 1.0f) * System.Math.Min(delta.x, delta.y);
                    case Primitives.Distance.Euclidean:
                        return System.Math.Sqrt(System.Math.Pow(delta.x, 2f) + System.Math.Pow(delta.y, 2f));
                    default:
                        return 0f;
                }
            }

            public static implicit operator Point(in Coord c) => new Point(c.x, c.y);

            public override string ToString() => new string($"({x}, {y}, {z})");

            public int CompareTo(Coord other)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Two-dimensional structure representing an area in space
        /// </summary>
        public struct Size
        {
            public int width, height;

            public Size(in Size size)
            {
                width = size.width;
                height = size.height;
            }

            public Size(int width, int height)
            {
                this.width = width;
                this.height = height;
            }

            public static readonly Size Zero = new Size(0, 0);
            public static readonly Size One = new Size(1, 1);
            public static readonly Size Two = new Size(2, 2);
            public static readonly Size Three = new Size(3, 3);
            public static readonly Size Four = new Size(4, 4);
            public static readonly Size Eight = new Size(8, 8);
            public static readonly Size Sixteen = new Size(16, 16);
            public static readonly Size Thritytwo = new Size(32, 32);

            public int Area => width * height;

            public static Size operator +(in Size lhs, in Size rhs) => new Size(lhs.width + rhs.width, lhs.height + rhs.height);

            public static Size operator -(in Size lhs, in Size rhs) => new Size(lhs.width - rhs.width, lhs.height - rhs.height);

            public static Size operator *(in Size lhs, in Size rhs) => new Size(lhs.width * rhs.width, lhs.height * rhs.height);
            public static Size operator *(in Size lhs, double scalar) => new Size((int)(lhs.width * scalar), (int)(lhs.height * scalar));

            public static Size operator /(in Size lhs, in Size rhs) => new Size(lhs.width / rhs.width, lhs.height / rhs.height);
            public static Size operator /(in Size lhs, double scalar) => new Size((int)(lhs.width / scalar), (int)(lhs.height / scalar));

            public static bool operator ==(in Size lhs, in Size rhs) => lhs.width == rhs.width && lhs.height == rhs.height;
            public static bool operator !=(in Size lhs, in Size rhs) => lhs.width != rhs.width || lhs.height != rhs.height;

            public override bool Equals(object? obj) => obj is Size bounds && width == bounds.width && height == bounds.height;
            public override int GetHashCode() => HashCode.Combine(width, height);

            public static explicit operator Bounds(in Size s) => new Bounds(s.width, s.height, 0);

            public override string ToString() => new string($"({width}, {height})");
        }

        /// <summary>
        /// Three-dimensional structure representing a volume in space
        /// </summary>
        public struct Bounds
        {
            public int width, height, depth;

            public Bounds(in Bounds bounds)
            {
                width = bounds.width;
                height = bounds.height;
                depth = bounds.depth;
            }

            public Bounds(int width, int height, int depth)
            {
                this.width = width;
                this.height = height;
                this.depth = depth;
            }

            public static readonly Bounds Zero = new Bounds(0, 0, 0);
            public static readonly Bounds One = new Bounds(1, 1, 1);
            public static readonly Bounds Two = new Bounds(2, 2, 2);
            public static readonly Bounds Three = new Bounds(3, 3, 3);
            public static readonly Bounds Four = new Bounds(4, 4, 4);
            public static readonly Bounds Eight = new Bounds(8, 8, 8);
            public static readonly Bounds Sixteen = new Bounds(16, 16, 16);
            public static readonly Bounds Thritytwo = new Bounds(32, 32, 32);

            public int Area => width * height;
            public int Volume => width * height * depth;

            public static Bounds operator +(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width + rhs.width, lhs.height + rhs.height, lhs.depth + rhs.depth);
            public static Bounds operator +(in Bounds lhs, in Size rhs) => new Bounds(lhs.width + rhs.width, lhs.height + rhs.height, lhs.depth);

            public static Bounds operator -(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width - rhs.width, lhs.height - rhs.height, lhs.depth - rhs.depth);
            public static Bounds operator -(in Bounds lhs, in Size rhs) => new Bounds(lhs.width - rhs.width, lhs.height - rhs.height, lhs.depth);

            public static Bounds operator *(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width * rhs.width, lhs.height * rhs.height, lhs.depth * rhs.depth);
            public static Bounds operator *(in Bounds lhs, in Size rhs) => new Bounds(lhs.width * rhs.width, lhs.height * rhs.height, lhs.depth);
            public static Bounds operator *(in Bounds lhs, double scalar) => new Bounds((int)(lhs.width * scalar), (int)(lhs.height * scalar), (int)(lhs.depth * scalar));

            public static Bounds operator /(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width / rhs.width, lhs.height / rhs.height, lhs.depth / rhs.depth);
            public static Bounds operator /(in Bounds lhs, in Size rhs) => new Bounds(lhs.width / rhs.width, lhs.height / rhs.height, lhs.depth);
            public static Bounds operator /(in Bounds lhs, double scalar) => new Bounds((int)(lhs.width / scalar), (int)(lhs.height / scalar), (int)(lhs.depth / scalar));

            public static bool operator ==(in Bounds lhs, in Bounds rhs) => lhs.width == rhs.width && lhs.height == rhs.height && lhs.depth == rhs.depth;
            public static bool operator !=(in Bounds lhs, in Bounds rhs) => lhs.width != rhs.width || lhs.height != rhs.height || lhs.depth != rhs.depth;

            public override bool Equals(object? obj) => obj is Bounds bounds && width == bounds.width && height == bounds.height && depth == bounds.depth;
            public override int GetHashCode() => HashCode.Combine(width, height, depth);

            public static implicit operator Size(in Bounds b) => new Size(b.width, b.height);
            public static Bounds ToBounds(in Size s, int depth) => new Bounds(s.width, s.height, depth);

            public override string ToString() => new string($"({width}, {height}, {depth})");
        }

        /// <summary>
        /// Two-dimensional structure representing a rectangle
        /// </summary>
        public struct Rect
        {
            public Point position;
            public Size size;

            public Rect(in Rect rect)
            {
                position = rect.position;
                size = rect.size;
            }

            public Rect(in Point position, in Size size)
            {
                this.position = position;
                this.size = size;
            }

            public static implicit operator Raylib_cs.Rectangle(in Rect rect)
            {
                return new Raylib_cs.Rectangle(rect.position.x, rect.position.y, rect.size.width, rect.size.height);
            }

            public static explicit operator Cuboid(in Rect r) => new Cuboid((Coord)r.position, (Bounds)r.size);
        }

        /// <summary>
        /// Three-dimensional structure representing a cuboid
        /// </summary>
        public struct Cuboid
        {
            public Coord position;
            public Bounds size;

            public Cuboid(in Cuboid cuboid)
            {
                position = cuboid.position;
                size = cuboid.size;
            }

            public Cuboid(in Coord position, in Bounds size)
            {
                this.position = position;
                this.size = size;
            }

            public static implicit operator Rect(in Cuboid c) => new Rect(c.position, c.size);
        }

        /// <summary>
        /// Two-dimensional structure representing a position, size, and angular rotation
        /// </summary>
        public struct Transform2D
        {
            public Point position;
            public Size size;
            public double rotation;

            public Transform2D(in Point position, in Size size, double rotation)
            {
                this.position = position;
                this.size = size;
                this.rotation = rotation;
            }
        }

        public struct Angle
        {
            private double _angle;

            public Angle(double angle)
            {
                _angle = angle;
            }

            public double Degrees { get => _angle; set => _angle = value; }
            public double Radians { get => Math.ToRadians(_angle); set => _angle = Math.ToDegrees(value); }

            public Cardinal Direction => RoundTo(_angle, 45.0) switch
            {
                   0.0 => Cardinal.East,
                 -45.0 => Cardinal.Northeast,
                 -90.0 => Cardinal.North,
                -135.0 => Cardinal.Northwest,
                  45.0 => Cardinal.Southeast,
                  90.0 => Cardinal.South,
                 135.0 => Cardinal.Southwest,
                 180.0 => Cardinal.West,
                     _ => Cardinal.Central,
            };

            public static double RoundTo(double value, double to)
                => System.Math.Round(value / to) * to;
        }

        /// <summary>
        /// Four-dimensional structure representing a rotation in three-dimensional space
        /// </summary>
        public struct Quaternion
        {
            public double w, x, y, z;

            /// <summary>
            /// Default constructor (identity quaternion)
            /// </summary>
            public Quaternion()
            {
                w = 1;
                x = 0;
                y = 0;
                z = 0;
            }

            public Quaternion(in Quaternion quat)
            {
                w = quat.w;
                x = quat.x;
                y = quat.y;
                z = quat.z;
            }

            public Quaternion(double x, double y, double z)
            {
                //Precalculate sin/cos of x divided by two
                double sin_of_x_div_2 = System.Math.Sin(x / 2);
                double cos_of_x_div_2 = System.Math.Cos(x / 2);

                //Precalculate sin/cos of y divided by two
                double sin_of_y_div_2 = System.Math.Sin(y / 2);
                double cos_of_y_div_2 = System.Math.Cos(y / 2);

                //Precalculate sin/cos of z divided by two
                double sin_of_z_div_2 = System.Math.Sin(z / 2);
                double cos_of_z_div_2 = System.Math.Cos(z / 2);

                     w = cos_of_x_div_2 * cos_of_y_div_2 * cos_of_z_div_2 + sin_of_x_div_2 * sin_of_y_div_2 * sin_of_z_div_2;
                this.x = sin_of_x_div_2 * cos_of_y_div_2 * cos_of_z_div_2 - cos_of_x_div_2 * sin_of_y_div_2 * sin_of_z_div_2;
                this.y = cos_of_x_div_2 * sin_of_y_div_2 * cos_of_z_div_2 + sin_of_x_div_2 * cos_of_y_div_2 * sin_of_z_div_2;
                this.z = cos_of_x_div_2 * cos_of_y_div_2 * sin_of_z_div_2 - sin_of_x_div_2 * sin_of_y_div_2 * cos_of_z_div_2;
            }
        }

        /// <summary>
        /// Three-dimensional structure representing a position, size, and angular rotation
        /// </summary>
        public struct Transform3D
        {
            public Coord position;
            public Bounds size;
            //public Quaternion rotation;
            public double rotation;
        }

        /// <summary>
        /// Structure representing the 8-bit channels for RGBA colors
        /// </summary>
        public struct Color
        {
            public byte red, green, blue, alpha;

            public Color()
            {
                red = 0x00;
                green = 0x00;
                blue = 0x00;
                alpha = 0x00;
            }

            public Color(in Color color)
            {
                red = color.red;
                green = color.green;
                blue = color.blue;
                alpha = color.alpha;
            }

            public Color(byte red, byte green, byte blue, byte alpha = 0xFF)
            {
                this.red = red;
                this.green = green;
                this.blue = blue;
                this.alpha = alpha;
            }

            public Color(double red, double green, double blue, double alpha = 0.0)
            {
                this.red = (byte)System.Math.Max(0, System.Math.Min(255, System.Math.Floor(red * 256.0)));
                this.green = (byte)System.Math.Max(0, System.Math.Min(255, System.Math.Floor(green * 256.0)));
                this.blue = (byte)System.Math.Max(0, System.Math.Min(255, System.Math.Floor(blue * 256.0)));
                this.alpha = (byte)System.Math.Max(0, System.Math.Min(255, System.Math.Floor(alpha * 256.0)));
            }

            public static implicit operator Raylib_cs.Color(in Color color)
            {
                return new Raylib_cs.Color(color.red, color.green, color.blue, color.alpha);
            }
        }

        public struct Glyph
        {
            public int index;
            public Color color;

            public Glyph(in Glyph glyph)
            {
                index = glyph.index;
                color = glyph.color;
            }

            public Glyph(int index, in Color color)
            {
                this.index = index;
                this.color = color;
            }
        }

        public struct Octant
        {
            public int x, dx, y, dy;

            public Octant(in Octant octant)
            {
                x = octant.x;
                dx = octant.dx;
                y = octant.y;
                dy = octant.dy;
            }

            public Octant(int x, int dx, int y, int dy)
            {
                this.x = x;
                this.dx = dx;
                this.y = y;
                this.dy = dy;
            }
        }

        public struct Letter
        {
            public int index;
            public bool vowel, consonant, capitalized;

            public Letter(in Letter letter)
            {
                index = letter.index;
                vowel = letter.vowel;
                consonant = letter.consonant;
                capitalized = letter.capitalized;
            }

            public Letter(int index, bool vowel, bool consonant, bool capitalized = false)
            {
                this.index = index;
                this.vowel = vowel;
                this.consonant = consonant;
                this.capitalized = capitalized;
            }

            public static readonly Letter Null = new Letter(-1, false, false);

            public static explicit operator ColoredLetter(in Letter letter)
            {
                return new ColoredLetter(letter, new Color());
            }
        }

        public struct ColoredLetter
        {
            public Letter letter;
            public Color color;

            public ColoredLetter(in ColoredLetter coloredLetter)
            {
                letter = coloredLetter.letter;
                color = coloredLetter.color;
            }

            public ColoredLetter(in Letter letter, in Color color)
            {
                this.letter = letter;
                this.color = color;
            }

            public static explicit operator Letter(in ColoredLetter letter)
            {
                return letter.letter;
            }
        }

        public enum VerticalAlignment
	    {
		    Center,
		    Upper,
		    Lower
	    }

        public enum HorizontalAlignment
	    {
		    Center,
		    Left,
		    Right
	    }

        public struct TextAlignment
	    {
            public VerticalAlignment vertical;
            public HorizontalAlignment horizontal;

            public TextAlignment()
            {
                vertical = VerticalAlignment.Upper;
                horizontal = HorizontalAlignment.Left;
            }

            public TextAlignment(in TextAlignment textAlignment)
            {
                vertical = textAlignment.vertical;
                horizontal = textAlignment.horizontal;
            }

            public TextAlignment(in VerticalAlignment vertical, in HorizontalAlignment horizontal)
            {
                this.vertical = vertical;
                this.horizontal = horizontal;
            }
        }
    }
}
