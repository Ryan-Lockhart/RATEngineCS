using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;
using Raylib_cs;
using Color = rat.Primitives.Color;
using Point = rat.Primitives.Point;
using Size = rat.Primitives.Size;

namespace rat
{
    public struct GlyphSetInfo
    {
        public string path;
        public Size glyphSize;
        public Size atlasSize;

        public GlyphSetInfo(string path, Size glyphSize, Size atlasSize)
        {
            this.path = path;
            this.glyphSize = glyphSize;
            this.atlasSize = atlasSize;
        }
    }

    public class GlyphSet : IDisposable
    {
        private readonly string _path;
        private readonly Size _glyphSize;
        private readonly Size _atlasSize;

        private readonly Rect[] _rects;

        private Texture2D _texture;

        public string Path => _path;

        public Size GlyphSize => _glyphSize;

        public Size AtlasSize => _atlasSize;

        public GlyphSet(GlyphSetInfo info)
        {
            _path = info.path;
            _glyphSize = info.glyphSize;
            _atlasSize = info.atlasSize;

            _rects = new Rect[_atlasSize.Area];

            for (int y = 0; y < _atlasSize.height; y++)
                for (int x = 0; x < _atlasSize.width; x++)
                    _rects[y * _atlasSize.width + x] = new Rect(new Point(x, y) * _glyphSize, _glyphSize);

            _texture = Raylib.LoadTexture(_path);
        }

        public GlyphSet(string path, Size glyphSize, Size atlasSize)
        {
            _path = path;
            _glyphSize = glyphSize;
            _atlasSize = atlasSize;

            _rects = new Rect[_atlasSize.Area];

            for (int y = 0; y < _atlasSize.height; y++)
                for (int x = 0; x < _atlasSize.width; x++)
                    _rects[y * _atlasSize.width + x] = new Rect(new Point(x, y) * _glyphSize, _glyphSize);

            _texture = Raylib.LoadTexture(_path);
        }

        public void Reload()
        {
            Raylib.UnloadTexture(_texture);
            _texture = Raylib.LoadTexture(_path);
        }

        public void Dispose()
        {
            Raylib.UnloadTexture(_texture);
        }

        public void DrawGlyph(byte index, in Color color, in Point position, bool useGrid = true)
        {
            Raylib.DrawTextureRec(_texture, GetRect(index), useGrid ? position * _glyphSize : position, color);
        }

        public void DrawGlyph(in Glyph glyph, in Point position, bool useGrid = true)
        {
            Raylib.DrawTextureRec(_texture, GetRect(glyph.index), useGrid ? position * _glyphSize : position, glyph.color);
        }

        public void DrawGlyph(in Glyph glyph, in Rect rect)
        {
            Raylib.DrawTexturePro(_texture, GetRect(glyph.index), rect, Point.Zero, 0.0f, glyph.color);
        }

        private Rect GetRect(uint index) => _rects[index];
    }
}
