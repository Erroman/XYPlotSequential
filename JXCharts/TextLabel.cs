using System.Drawing;
using System.Linq;


namespace JXCharts
{
    public class TextLabel
    {

        #region Private fields

        private readonly string[] _symbols = { "+", ".", "*", "o", "x" };

        #endregion

        #region Constructors

        public TextLabel( string text, PointD location )
        {
            Text = text;
            Location = location;

            IsSymbol = _symbols.Contains( Text );
            IsSizeManual = false;
            IsColorManual = false;

            Size = 10;
            Color = Color.Black;
        }

        #endregion

        #region Properties

        public bool IsSizeManual { get; set; }

        public bool IsColorManual { get; set; }

        public bool IsSymbol { get; private set; }

        public PointD Location { get; set; }

        public string Text { get; private set; }

        public float Size { get; set; }

        public Color Color { get; set; }

        #endregion

    }
}