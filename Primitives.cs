using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rat
{

    namespace Primitives
    {
        /// <summary>
        /// Two-dimensional structure representing a position in space
        /// </summary>
        public struct Point
        {
            public long x, y;

            public Point(long x, long y)
            {
                this.x = x;
                this.y = y;
            }

            public static readonly Point Zero = new Point(0, 0);

            public static readonly Point West = new Point(-1, 0);
            public static readonly Point East = new Point(1, 0);

            public static readonly Point North = new Point(0, -1);
            public static readonly Point South = new Point(0, 1);

            public static implicit operator System.Numerics.Vector2(in Point point) => new System.Numerics.Vector2(point.x, point.y);
            public static implicit operator Point(in System.Numerics.Vector2 vector) => new Point((long)vector.X, (long)vector.Y);

            public static Point operator +(in Point lhs, in Point rhs) => new Point(lhs.x + rhs.x, lhs.y + rhs.y);
            public static Point operator +(in Point lhs, in Size rhs) => new Point(lhs.x + rhs.width, lhs.y + rhs.height);

            public static Point operator -(in Point lhs, in Point rhs) => new Point(lhs.x - rhs.x, lhs.y - rhs.y);
            public static Point operator -(in Point lhs, in Size rhs) => new Point(lhs.x - rhs.width, lhs.y - rhs.height);

            public static Point operator *(in Point lhs, in Point rhs) => new Point(lhs.x * rhs.x, lhs.y * rhs.y);
            public static Point operator *(in Point lhs, in Size rhs) => new Point(lhs.x * rhs.width, lhs.y * rhs.height);
            public static Point operator *(in Point lhs, double scalar) => new Point((long)(lhs.x * scalar), (long)(lhs.y * scalar));

            public static Point operator /(in Point lhs, in Point rhs) => new Point(lhs.x / rhs.x, lhs.y / rhs.y);
            public static Point operator /(in Point lhs, in Size rhs) => new Point(lhs.x / rhs.width, lhs.y / rhs.height);
            public static Point operator /(in Point lhs, double scalar) => new Point((long)(lhs.x / scalar), (long)(lhs.y / scalar));

            public static bool operator ==(in Point lhs, in Point rhs) => lhs.x == rhs.x && lhs.y == rhs.y;
            public static bool operator !=(in Point lhs, in Point rhs) => lhs.x != rhs.x && lhs.y != rhs.y;

            public override bool Equals(object? obj) => obj is Point point && x == point.x && y == point.y;
            public override int GetHashCode() => HashCode.Combine(x, y);

            public static explicit operator Coord(in Point p) => new Coord(p.x, p.y, 0);
            public static Coord ToCoord(in Point p, long z) => new Coord(p.x, p.y, z);

            public override string ToString() => new string($"({x}, {y})");
        }

        /// <summary>
        /// Three-dimensional structure representing a position in space
        /// </summary>
        public struct Coord
        {
            public long x, y, z;

            public Coord(long x, long y, long z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public static readonly Coord Zero = new Coord(0, 0, 0);

            public static readonly Coord Up = new Coord(0, 0, 1);
            public static readonly Coord Down = new Coord(0, 0, -1);

            public static readonly Coord West = new Coord(-1, 0, 0);
            public static readonly Coord East = new Coord(1, 0, 0);

            public static readonly Coord North = new Coord(0, -1, 0);
            public static readonly Coord South = new Coord(0, 1, 0);

            public static implicit operator System.Numerics.Vector3(in Coord coord) => new System.Numerics.Vector3(coord.x, coord.y, coord.z);
            public static implicit operator Coord(in System.Numerics.Vector3 vector) => new Coord((long)vector.X, (long)vector.Y, (long)vector.Z);

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
            public static Coord operator *(in Coord lhs, double scalar) => new Coord((long)(lhs.x * scalar), (long)(lhs.y * scalar), (long)(lhs.z * scalar));

            public static Coord operator /(in Coord lhs, in Coord rhs) => new Coord(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
            public static Coord operator /(in Coord lhs, in Point rhs) => new Coord(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z);
            public static Coord operator /(in Coord lhs, in Size rhs) => new Coord(lhs.x / rhs.width, lhs.y / rhs.height, lhs.z);
            public static Coord operator /(in Coord lhs, in Bounds rhs) => new Coord(lhs.x / rhs.width, lhs.y / rhs.height, lhs.z / rhs.depth);
            public static Coord operator /(in Coord lhs, double scalar) => new Coord((long)(lhs.x / scalar), (long)(lhs.y / scalar), (long)(lhs.z / scalar));

            public static bool operator ==(in Coord lhs, in Coord rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
            public static bool operator !=(in Coord lhs, in Coord rhs) => lhs.x != rhs.x && lhs.y != rhs.y && lhs.z != rhs.z;

            public override bool Equals(object? obj) => obj is Coord coord && x == coord.x && y == coord.y && z == coord.z;
            public override int GetHashCode() => HashCode.Combine(x, y, z);

            public static implicit operator Point(in Coord c) => new Point(c.x, c.y);

            public override string ToString() => new string($"({x}, {y}, {z})");
        }

        /// <summary>
        /// Two-dimensional structure representing an area in space
        /// </summary>
        public struct Size
        {
            public uint width, height;

            public Size(uint width, uint height)
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

            public uint Area => width * height;

            public static Size operator +(in Size lhs, in Size rhs) => new Size(lhs.width + rhs.width, lhs.height + rhs.height);

            public static Size operator -(in Size lhs, in Size rhs) => new Size(lhs.width - rhs.width, lhs.height - rhs.height);

            public static Size operator *(in Size lhs, in Size rhs) => new Size(lhs.width * rhs.width, lhs.height * rhs.height);
            public static Size operator *(in Size lhs, double scalar) => new Size((uint)(lhs.width * scalar), (uint)(lhs.height * scalar));

            public static Size operator /(in Size lhs, in Size rhs) => new Size(lhs.width / rhs.width, lhs.height / rhs.height);
            public static Size operator /(in Size lhs, double scalar) => new Size((uint)(lhs.width / scalar), (uint)(lhs.height / scalar));

            public static bool operator ==(in Size lhs, in Size rhs) => lhs.width == rhs.width && lhs.height == rhs.height;
            public static bool operator !=(in Size lhs, in Size rhs) => lhs.width != rhs.width && lhs.height != rhs.height;

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
            public uint width, height, depth;

            public Bounds(uint width, uint height, uint depth)
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

            public uint Area => width * height;
            public uint Volume => width * height * depth;

            public static Bounds operator +(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width + rhs.width, lhs.height + rhs.height, lhs.depth + rhs.depth);
            public static Bounds operator +(in Bounds lhs, in Size rhs) => new Bounds(lhs.width + rhs.width, lhs.height + rhs.height, lhs.depth);

            public static Bounds operator -(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width - rhs.width, lhs.height - rhs.height, lhs.depth - rhs.depth);
            public static Bounds operator -(in Bounds lhs, in Size rhs) => new Bounds(lhs.width - rhs.width, lhs.height - rhs.height, lhs.depth);

            public static Bounds operator *(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width * rhs.width, lhs.height * rhs.height, lhs.depth * rhs.depth);
            public static Bounds operator *(in Bounds lhs, in Size rhs) => new Bounds(lhs.width * rhs.width, lhs.height * rhs.height, lhs.depth);
            public static Bounds operator *(in Bounds lhs, double scalar) => new Bounds((uint)(lhs.width * scalar), (uint)(lhs.height * scalar), (uint)(lhs.depth * scalar));

            public static Bounds operator /(in Bounds lhs, in Bounds rhs) => new Bounds(lhs.width / rhs.width, lhs.height / rhs.height, lhs.depth / rhs.depth);
            public static Bounds operator /(in Bounds lhs, in Size rhs) => new Bounds(lhs.width / rhs.width, lhs.height / rhs.height, lhs.depth);
            public static Bounds operator /(in Bounds lhs, double scalar) => new Bounds((uint)(lhs.width / scalar), (uint)(lhs.height / scalar), (uint)(lhs.depth / scalar));

            public static bool operator ==(in Bounds lhs, in Bounds rhs) => lhs.width == rhs.width && lhs.height == rhs.height && lhs.depth == rhs.depth;
            public static bool operator !=(in Bounds lhs, in Bounds rhs) => lhs.width != rhs.width && lhs.height != rhs.height && lhs.depth != rhs.depth;

            public override bool Equals(object? obj) => obj is Bounds bounds && width == bounds.width && height == bounds.height && depth == bounds.depth;
            public override int GetHashCode() => HashCode.Combine(width, height, depth);

            public static implicit operator Size(in Bounds b) => new Size(b.width, b.height);
            public static Bounds ToBounds(in Size s, uint depth) => new Bounds(s.width, s.height, depth);

            public override string ToString() => new string($"({width}, {height}, {depth})");
        }

        /// <summary>
        /// Two-dimensional structure representing a rectangle
        /// </summary>
        public struct Rect
        {
            public Point position;
            public Size size;

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

            public Quaternion(double x, double y, double z)
            {
                //Precalculate sin/cos of x divided by two
                double sin_of_x_div_2 = Math.Sin(x / 2);
                double cos_of_x_div_2 = Math.Cos(x / 2);

                //Precalculate sin/cos of y divided by two
                double sin_of_y_div_2 = Math.Sin(y / 2);
                double cos_of_y_div_2 = Math.Cos(y / 2);

                //Precalculate sin/cos of z divided by two
                double sin_of_z_div_2 = Math.Sin(z / 2);
                double cos_of_z_div_2 = Math.Cos(z / 2);

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

            public Color(byte red, byte green, byte blue, byte alpha = 0xFF)
            {
                this.red = red;
                this.green = green;
                this.blue = blue;
                this.alpha = alpha;
            }

            public Color(double red, double green, double blue, double alpha = 0.0)
            {
                this.red = (byte)Math.Max(0, Math.Min(255, Math.Floor(red * 256.0)));
                this.green = (byte)Math.Max(0, Math.Min(255, Math.Floor(green * 256.0)));
                this.blue = (byte)Math.Max(0, Math.Min(255, Math.Floor(blue * 256.0)));
                this.alpha = (byte)Math.Max(0, Math.Min(255, Math.Floor(alpha * 256.0)));
            }

            public static implicit operator Raylib_cs.Color(in Color color)
            {
                return new Raylib_cs.Color(color.red, color.green, color.blue, color.alpha);
            }
        }

        public struct Glyph
        {
            public byte index;
            public Color color;

            public Glyph(byte index, in Color color)
            {
                this.index = index;
                this.color = color;
            }
        }

        public struct Cell
        {
            public Coord position;

            public Glyph foreground, background;

            public Cell(in Coord position, in Glyph foreground, in Glyph background)
            {
                this.position = position;
                this.foreground = foreground;
                this.background = background;
            }
        }

        public struct Octant
        {
            public int x, dx, y, dy;

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
            public byte index;
            public bool vowel, consonant, capitalized;

            public Letter(byte index, bool vowel, bool consonant, bool capitalized = false)
            {
                this.index = index;
                this.vowel = vowel;
                this.consonant = consonant;
                this.capitalized = capitalized;
            }

            public static explicit operator ColoredLetter(in Letter letter)
            {
                return new ColoredLetter(letter, new Color());
            }
        }

        public struct ColoredLetter
        {
            public Letter letter;
            public Color color;

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

            public TextAlignment(in VerticalAlignment vertical, in HorizontalAlignment horizontal)
            {
                this.vertical = vertical;
                this.horizontal = horizontal;
            }
        }
    }
}
