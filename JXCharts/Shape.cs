using System.Drawing;
using System.Drawing.Drawing2D;

using SMath.Math.Numeric;


namespace JXCharts
{
    public enum EnShapeType
    {
        None = 0,
        Line,
        Rectangle,
        RoundedRectangle,
        Circle,
        Ellipse,
        Arc,
        Polygon,
        Pie,
        Polyline,
        Spline,
        Bezier
    }

    public class Shape
    {

        #region Constructors

        public Shape( EnShapeType type, TMatrix data )
        {
            Type = type;
            Data = data;

            IsLineColorManual = false;
            IsLineWidthManual = false;
            IsFillColorManual = false;

            LineStyle = DashStyle.Solid;
            LineWidth = 1;
            LineColor = Color.Black;

            FillColor = Color.Transparent;
        }

        #endregion

        #region Properties

        public bool IsLineColorManual { get; set; }

        public bool IsLineWidthManual { get; set; }

        public bool IsFillColorManual { get; set; }

        public EnShapeType Type { get; private set; }

        public DashStyle LineStyle { get; set; }

        public float LineWidth { get; set; }

        public TMatrix Data { get; private set; }

        public Color LineColor { get; set; }

        public Color FillColor { get; set; }

        #endregion

    }
}
