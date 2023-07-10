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
        private readonly string m_Path;
        private readonly Size m_GlyphSize;
        private readonly Size m_AtlasSize;

        private readonly Rect[] m_Rects;

        private Texture2D m_Texture;

        public string Path => m_Path;

        public Size GlyphSize => m_GlyphSize;

        public Size AtlasSize => m_AtlasSize;

        public GlyphSet(GlyphSetInfo info)
        {
            m_Path = info.path;
            m_GlyphSize = info.glyphSize;
            m_AtlasSize = info.atlasSize;

            m_Rects = new Rect[m_AtlasSize.Area];

            for (int y = 0; y < m_AtlasSize.height; y++)
                for (int x = 0; x < m_AtlasSize.width; x++)
                    m_Rects[y * m_AtlasSize.width + x] = new Rect(new Point(x, y) * m_GlyphSize, m_GlyphSize);

            m_Texture = Raylib.LoadTexture(m_Path);
        }

        public GlyphSet(string path, Size glyphSize, Size atlasSize)
        {
            m_Path = path;
            m_GlyphSize = glyphSize;
            m_AtlasSize = atlasSize;

            m_Rects = new Rect[m_AtlasSize.Area];

            for (int y = 0; y < m_AtlasSize.height; y++)
                for (int x = 0; x < m_AtlasSize.width; x++)
                    m_Rects[y * m_AtlasSize.width + x] = new Rect(new Point(x, y) * m_GlyphSize, m_GlyphSize);

            m_Texture = Raylib.LoadTexture(m_Path);
        }

        public void Reload()
        {
            Raylib.UnloadTexture(m_Texture);
            m_Texture = Raylib.LoadTexture(m_Path);
        }

        public void Dispose()
        {
            Raylib.UnloadTexture(m_Texture);
        }

        public void DrawGlyph(byte index, in Color color, in Point position, bool useGrid = true)
        {
            Raylib.DrawTextureRec(m_Texture, GetRect(index), useGrid ? position * m_GlyphSize : position, color);
        }

        public void DrawGlyph(in Glyph glyph, in Point position, bool useGrid = true)
        {
            Raylib.DrawTextureRec(m_Texture, GetRect(glyph.index), useGrid ? position * m_GlyphSize : position, glyph.color);
        }

        public void DrawGlyph(in Glyph glyph, in Rect rect)
        {
            Raylib.DrawTexturePro(m_Texture, GetRect(glyph.index), rect, Point.Zero, 0.0f, glyph.color);
        }

        private Rect GetRect(uint index) => m_Rects[index];
    }
}
