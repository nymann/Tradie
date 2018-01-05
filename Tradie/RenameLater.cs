using System.Collections.Generic;
using SharpDX;

namespace Tradie
{
    public class RenameLater
    {
        public IEnumerable<Item> Items { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int TextSize { get; set; }
        public int ImageSize { get; set; }
        public int Spacing { get; set; }
        public Color TextColor { get; set; }
        public bool LeftAlignment { get; set; }
        public bool Ascending { get; set; }

        public RenameLater()
        {

        }

        public RenameLater(IEnumerable<Item> items, int x, int y, int textSize, int imageSize, int extraBit, Color textColor, bool leftAlignment, bool ascending)
        {
            Items = items;
            X = x;
            Y = y;
            TextSize = textSize;
            ImageSize = imageSize;
            Spacing = extraBit;
            TextColor = textColor;
            LeftAlignment = leftAlignment;
            Ascending = ascending;
        }
    }
}
