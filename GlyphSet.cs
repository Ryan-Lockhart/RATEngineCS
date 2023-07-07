using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rat.Primitives;
using Raylib_cs;

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

        public GlyphSet(GlyphSetInfo info)
        {
            _path = info.path;
            _glyphSize = info.glyphSize;
            _atlasSize = info.atlasSize;

            _rects = new Rect[_atlasSize.Area()];

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

            _rects = new Rect[_atlasSize.Area()];

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

        public void DrawGlyph(Point position, Glyph glyph, bool useGrid = true)
        {
            Raylib.DrawTextureRec(_texture, GetRect(glyph.index), useGrid ? position * _glyphSize : position, glyph.color);
        }

        private Rect GetRect(uint index) => _rects[index];
    }
}
