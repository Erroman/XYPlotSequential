using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class LegendTypeConverter : TypeConverterOf<Legend> { }

    [TypeConverter( typeof( LegendTypeConverter ) )]
    public class Legend
    {
        
        #region Private fields

        private Rectangle _rect;

        #endregion

        #region Public enums

        public enum LegendPositionEnum
        {
            North,
            NorthWest,
            West,
            SouthWest,
            South,
            SouthEast,
            East,
            NorthEast
        }

        #endregion

        #region Constructors

        public Legend()
        {
            LegendPosition = LegendPositionEnum.NorthEast;
            TextColor = Color.Black;
            IsLegendVisible = false;
            IsBorderVisible = true;
            LegendBackColor = Color.White;
            LegendBorderColor = Color.Black;
            LegendFont = new Font( "Arial", 8, FontStyle.Regular );
            _rect = new Rectangle();
        }

        public Legend( Legend legend )
        {
            IsBorderVisible = legend.IsBorderVisible;
            IsLegendVisible = legend.IsLegendVisible;
            LegendBackColor = legend.LegendBackColor;
            LegendBorderColor = legend.LegendBorderColor;
            LegendPosition = legend.LegendPosition;
            TextColor = legend.TextColor;
            LegendFont = new Font( legend.LegendFont, legend.LegendFont.Style );
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public Legend Clone() => new Legend( this );


        public void Refresh( List<Series> dc, ChartArea ca )
        {
            var offSet = 10f;
            var spacing = 5f;
            var lineLength = 30.0f;
            var x = 0f;
            var y = 0f;
            var w = 3 * spacing + lineLength;
            var h = spacing;

            if ( dc.Any( ds => ds.LineStyle.Visible && ds.SeriesName.Length > 0 ) )
            {
                var labels = dc.Where( ds => ds.LineStyle.Visible && ds.SeriesName.Length > 0 ).Select( ds => ds.SeriesName ).ToList();

                var g = Graphics.FromHwnd( IntPtr.Zero );

                w += labels.Select( s => g.MeasureString( s, LegendFont ) ).Max( sz => sz.Width );

                h += ( g.MeasureString( "A", LegendFont ).Height + spacing ) * labels.Count;
            }

            switch ( LegendPosition )
            {
                case LegendPositionEnum.East:
                    x = ca.PlotRect.X + ca.PlotRect.Width - offSet - w;
                    y = ca.PlotRect.Y + ca.PlotRect.Height / 2 - h / 2;
                    break;

                case LegendPositionEnum.North:
                    x = ca.PlotRect.X + ca.PlotRect.Width / 2 - w / 2;
                    y = ca.PlotRect.Y + offSet;
                    break;

                case LegendPositionEnum.NorthEast:
                    x = ca.PlotRect.X + ca.PlotRect.Width - offSet - w;
                    y = ca.PlotRect.Y + offSet;
                    break;

                case LegendPositionEnum.NorthWest:
                    x = ca.PlotRect.X + offSet;
                    y = ca.PlotRect.Y + offSet;
                    break;

                case LegendPositionEnum.South:
                    x = ca.PlotRect.X + ca.PlotRect.Width / 2 - w / 2;
                    y = ca.PlotRect.Y + ca.PlotRect.Height - offSet - h;
                    break;

                case LegendPositionEnum.SouthEast:
                    x = ca.PlotRect.X + ca.PlotRect.Width - offSet - w;
                    y = ca.PlotRect.Y + ca.PlotRect.Height - offSet - h;
                    break;

                case LegendPositionEnum.SouthWest:
                    x = ca.PlotRect.X + offSet;
                    y = ca.PlotRect.Y + ca.PlotRect.Height - offSet - h;
                    break;

                case LegendPositionEnum.West:
                    x = ca.PlotRect.X + offSet;
                    y = ca.PlotRect.Y + ca.PlotRect.Height / 2 - h / 2;
                    break;
            }

            _rect = new Rectangle( ( int ) x, ( int ) y, ( int ) w, ( int ) h );
        }


        public void Draw( Graphics g, ChartArea ca, List<Series> series )
        {
            if ( !IsLegendVisible ) return;

            if ( !series.Any( ds => ds.LineStyle.Visible ) ) return;

            Refresh( series, ca );

            var spacing = 5f;
            var lineLength = 30.0f;
            var htextHeight = g.MeasureString( "A", LegendFont ).Height;

            var aPen = new Pen( LegendBorderColor, 1f );
            var aBrush = new SolidBrush( LegendBackColor );

            g.FillRectangle( aBrush, _rect );

            if ( IsBorderVisible )
            {
                g.DrawRectangle( aPen, _rect );
            }

            aBrush.Dispose();

            var xText = _rect.X + lineLength + 2 * spacing;
            var yText = _rect.Y + spacing;

            var xSymbol = _rect.X + spacing + lineLength / 2.0f;
            var ySymbol = yText + htextHeight / 2;

            var smooth = g.SmoothingMode;

            foreach ( var ds in series )
            {
                if ( !ds.LineStyle.Visible || ds.SeriesName.Length == 0 ) continue;

                g.SmoothingMode = ds.LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

                // Draw lines and symbols.
                aPen = new Pen( ds.LineStyle.LineColor, ds.LineStyle.Thickness ) { DashStyle = ds.LineStyle.Pattern };

                var ptStart = new PointF( _rect.X + spacing, ySymbol );
                var ptEnd = new PointF( _rect.X + spacing + lineLength, ySymbol );

                g.DrawLine( aPen, ptStart, ptEnd );

                g.SmoothingMode = ds.SymbolStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

                ds.SymbolStyle.DrawSymbol( g, new PointF( xSymbol, ySymbol ) );

                g.SmoothingMode = SmoothingMode.None;

                // Draw text.
                var sFormat = new StringFormat { Alignment = StringAlignment.Near };

                g.DrawString( ds.SeriesName, LegendFont, new SolidBrush( TextColor ), new PointF( xText, yText ), sFormat );

                yText += htextHeight + spacing;
                ySymbol = yText + htextHeight / 2;

                aPen.Dispose();
            }

            g.SmoothingMode = smooth;
        }


        public void FromXml( XmlReader reader )
        {
            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            var fontConverter = TypeDescriptor.GetConverter( typeof( Font ) );
            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );
            var legendPositionEnumConverter = TypeDescriptor.GetConverter( typeof( LegendPositionEnum ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "isbordervisible" );

                if ( !string.IsNullOrEmpty( text ) ) IsBorderVisible = Convert.ToBoolean( text );

                text = reader.GetAttribute( "islegendvisible" );

                if ( !string.IsNullOrEmpty( text ) ) IsLegendVisible = Convert.ToBoolean( text );

                text = reader.GetAttribute( "legendbackcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) LegendBackColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "legendbordercolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) LegendBorderColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "legendfont" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( fontConverter.IsValid( text ) ) LegendFont = ( Font ) fontConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "legendposition" );

                if ( !string.IsNullOrEmpty( text ) )
                    if ( legendPositionEnumConverter.IsValid( text ) )
                        LegendPosition = ( LegendPositionEnum ) legendPositionEnumConverter.ConvertFromString( text );

                text = reader.GetAttribute( "textcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) TextColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }
            }
        }


        public void ToXml( XmlWriter writer )
        {
            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            var fontConverter = TypeDescriptor.GetConverter( typeof( Font ) );
            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );
            var legendPositionEnumConverter = TypeDescriptor.GetConverter( typeof( LegendPositionEnum ) );

            writer.WriteStartElement( GetType().Name.ToLower() );

            writer.WriteAttributeString( "isbordervisible", IsBorderVisible.ToString().ToLower() );
            writer.WriteAttributeString( "islegendvisible", IsLegendVisible.ToString().ToLower() );
            writer.WriteAttributeString( "legendbackcolor", colorConverter.ConvertToString( null, cultureInfo, LegendBackColor ) );
            writer.WriteAttributeString( "legendbordercolor", colorConverter.ConvertToString( null, cultureInfo, LegendBorderColor ) );
            writer.WriteAttributeString( "legendfont", fontConverter.ConvertToString( null, cultureInfo, LegendFont ) );
            writer.WriteAttributeString( "legendposition", legendPositionEnumConverter.ConvertToString( LegendPosition ) );
            writer.WriteAttributeString( "textcolor", colorConverter.ConvertToString( null, cultureInfo, TextColor ) );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [DisplayName( "Font" ),
        Description( "Font used to display the legend text." ),
        Category( "Appearance" ) ]
        public Font LegendFont { get; set; }

        [Browsable( true )]
        [DisplayName( "BackColor" ), 
        Description( "Background color of the legend box." ),
        Category( "Appearance" ) ]
        public Color LegendBackColor { get; set; }

        [Browsable( true )]
        [DisplayName( "BorderColor" ), 
        Description( "The color of the legend box border." ),
        Category("Appearance")]
        public Color LegendBorderColor { get; set; }

        [Browsable( true )]
        [Description( "Indicates whether the legend border should be shown." ),
        Category("Appearance")]
        public bool IsBorderVisible { get; set; }

        [Browsable( true )]
        [DisplayName( "Position" ), 
        Description( "Specifies the legend position in the chart ." ),
        Category("Appearance")]
        public LegendPositionEnum LegendPosition { get; set; }

        [Browsable( true )]
        [Description( "Color of the legend text." ),
        Category("Appearance")]
        public Color TextColor { get; set; }

        [Browsable( true )]
        [Description( "Indicates whether the legend is shown in the chart." ),
        Category("Appearance")]
        public bool IsLegendVisible { get; set; }

        #endregion

    }
}
