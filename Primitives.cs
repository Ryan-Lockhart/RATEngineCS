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

            public static implicit operator System.Numerics.Vector2(Point point) => new System.Numerics.Vector2(point.x, point.y);

            public static Point operator +(Point lhs, Point rhs) => new Point(lhs.x + rhs.x, lhs.y + rhs.y);

            public static Point operator -(Point lhs, Point rhs) => new Point(lhs.x - rhs.x, lhs.y - rhs.y);

            public static Point operator *(Point lhs, Point rhs) => new Point(lhs.x * rhs.x, lhs.y * rhs.y);
            public static Point operator *(Point lhs, double scalar) => new Point((long)(lhs.x * scalar), (long)(lhs.y * scalar));

            public static Point operator /(Point lhs, Point rhs) => new Point(lhs.x / rhs.x, lhs.y / rhs.y);
            public static Point operator /(Point lhs, double scalar) => new Point((long)(lhs.x / scalar), (long)(lhs.y / scalar));

            public static Point operator *(Point a, Size b) => new Point(a.x * b.width, a.y * b.height);
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

            public static implicit operator System.Numerics.Vector3(Coord coord)
            {
                return new System.Numerics.Vector3(coord.x, coord.y, coord.z);
            }

            public static Coord operator +(Coord lhs, Coord rhs) => new Coord(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);

            public static Coord operator -(Coord lhs, Coord rhs) => new Coord(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z + rhs.z);

            public static Coord operator *(Coord lhs, Coord rhs) => new Coord(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z + rhs.z);
            public static Coord operator *(Coord lhs, double scalar) => new Coord((long)(lhs.x * scalar), (long)(lhs.y * scalar), (long)(lhs.z * scalar));

            public static Coord operator /(Coord lhs, Coord rhs) => new Coord(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
            public static Coord operator /(Coord lhs, double scalar) => new Coord((long)(lhs.x / scalar), (long)(lhs.y / scalar), (long)(lhs.z * scalar));
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

            public uint Area()
            {
                return width * height;
            }

            public static Size operator +(Size lhs, Size rhs) => new Size(lhs.width + rhs.width, lhs.height + rhs.height);

            public static Size operator -(Size lhs, Size rhs) => new Size(lhs.width - rhs.width, lhs.height - rhs.height);

            public static Size operator *(Size lhs, Size rhs) => new Size(lhs.width * rhs.width, lhs.height * rhs.height);
            public static Size operator *(Size lhs, double scalar) => new Size((uint)(lhs.width * scalar), (uint)(lhs.height * scalar));

            public static Size operator /(Size lhs, Size rhs) => new Size(lhs.width / rhs.width, lhs.height / rhs.height);
            public static Size operator /(Size lhs, double scalar) => new Size((uint)(lhs.width / scalar), (uint)(lhs.height / scalar));
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

            public uint Area()
            {
                return width * height;
            }

            public uint Volume()
            {
                return width * height * depth;
            }

            public static Bounds operator +(Bounds lhs, Bounds rhs) => new Bounds(lhs.width + rhs.width, lhs.height + rhs.height, lhs.depth + rhs.depth);

            public static Bounds operator -(Bounds lhs, Bounds rhs) => new Bounds(lhs.width - rhs.width, lhs.height - rhs.height, lhs.depth + rhs.depth);

            public static Bounds operator *(Bounds lhs, Bounds rhs) => new Bounds(lhs.width * rhs.width, lhs.height * rhs.height, lhs.depth + rhs.depth);
            public static Bounds operator *(Bounds lhs, double scalar) => new Bounds((uint)(lhs.width * scalar), (uint)(lhs.height * scalar), (uint)(lhs.depth * scalar));

            public static Bounds operator /(Bounds lhs, Bounds rhs) => new Bounds(lhs.width / rhs.width, lhs.height / rhs.height, lhs.depth / rhs.depth);
            public static Bounds operator /(Bounds lhs, double scalar) => new Bounds((uint)(lhs.width / scalar), (uint)(lhs.height / scalar), (uint)(lhs.depth * scalar));
        }

        /// <summary>
        /// Two-dimensional structure representing a rectangle
        /// </summary>
        public struct Rect
        {
            public Point position;
            public Size size;

            public Rect(Point position, Size size)
            {
                this.position = position;
                this.size = size;
            }

            public static implicit operator Raylib_cs.Rectangle(Rect rect)
            {
                return new Raylib_cs.Rectangle(rect.position.x, rect.position.y, rect.size.width, rect.size.height);
            }
        }

        /// <summary>
        /// Three-dimensional structure representing a cuboid
        /// </summary>
        public struct Cuboid
        {
            public Coord position;
            public Bounds bounds;

            public Cuboid(Coord position, Bounds bounds)
            {
                this.position = position;
                this.bounds = bounds;
            }
        }

        /// <summary>
        /// Two-dimensional structure representing a position, size, and angular rotation
        /// </summary>
        public struct Transform2D
        {
            public Point position;
            public Size size;
            public double rotation;

            public Transform2D(Point position, Size size, double rotation)
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

            public static implicit operator Raylib_cs.Color(Color color)
            {
                return new Raylib_cs.Color(color.red, color.green, color.blue, color.alpha);
            }
        }

        public struct Glyph
        {
            public byte index;
            public Color color;

            public Glyph(byte index, Color color)
            {
                this.index = index;
                this.color = color;
            }
        }

        public struct Cell
        {
            public Coord position;

            public Glyph foreground, background;

            public Cell(Coord position, Glyph foreground, Glyph background)
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

            public static explicit operator ColoredLetter(Letter letter)
            {
                return new ColoredLetter(letter, new Color());
            }
        }

        public struct ColoredLetter
        {
            public Letter letter;
            public Color color;

            public ColoredLetter(Letter letter, Color color)
            {
                this.letter = letter;
                this.color = color;
            }

            public static explicit operator Letter(ColoredLetter letter)
            {
                return letter.letter;
            }
        }
    }
}
