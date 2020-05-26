using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class GridTypeConverter : TypeConverterOf<Grid> { }

    [TypeConverter( typeof( GridTypeConverter ) )]
    public class Grid
    {

        #region Constructors

        public Grid()
        {
            IsXGrid = true;
            IsYGrid = true;
            IsY2Grid = false;

            GridThickness = 1.0f;
            GridPattern = DashStyle.Dash;
            GridColor = Color.LightGray;
        }


        public Grid( Grid grid )
        {
            IsXGrid = grid.IsXGrid;
            IsYGrid = grid.IsYGrid;
            IsY2Grid = grid.IsY2Grid;

            GridThickness = grid.GridThickness;
            GridPattern = grid.GridPattern;
            GridColor = grid.GridColor;
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public Grid Clone() => new Grid( this );


        public void Draw( Graphics g, ChartArea ca, XAxis xa, YAxis ya, Y2Axis y2a )
        {
            if ( !IsXGrid && !IsYGrid && !IsY2Grid ) return;

            Pen aPen;

            // Create vertical gridlines.
            int beg, end;

            var xmin = Math.Min( xa.Min, xa.Max );
            var xmax = Math.Max( xa.Min, xa.Max );
            var xtick = xa.Tick;

            // Create horizontal gridlines.
            if ( IsXGrid ) 
            {
                aPen = new Pen( GridColor, GridThickness ) { DashStyle = GridPattern };

                beg = ( int ) Math.Truncate( xmin / xtick );

                if ( beg * xtick <= xmin ) beg++;

                end = ( int ) Math.Truncate( xmax / xtick );

                if ( end * xtick >= xmax ) end--;

                for ( var i = beg; i <= end; i++ )
                {
                    var x = i * xtick;

                    x = ca.PlotRect.X + ( x - xmin ) * ca.PlotRect.Width / ( xmax - xmin );

                    g.DrawLine( aPen, new PointF( x, ca.PlotRect.Top ), new PointF( x, ca.PlotRect.Bottom ) );
                }

                aPen.Dispose();
            }

            var ymin = Math.Min( ya.Min, ya.Max );
            var ymax = Math.Max( ya.Min, ya.Max );
            var ytick = ya.Tick;

            // Create vertical gridlines.
            if ( IsYGrid ) 
            {
                aPen = new Pen( GridColor, GridThickness ) { DashStyle = GridPattern };

                beg = ( int ) Math.Truncate( ymin / ytick );

                if ( beg * ytick <= ymin ) beg++;

                end = ( int ) Math.Truncate( ymax / ytick );

                if ( end * ytick >= ymax ) end--;

                for ( var i = beg; i <= end; i++ )
                {
                    var y = ca.PlotRect.Bottom - ( i * ytick - ymin ) * ca.PlotRect.Height / ( ymax - ymin );

                    g.DrawLine( aPen, new PointF( ca.PlotRect.Left, y ), new PointF( ca.PlotRect.Right, y ) );
                }

                aPen.Dispose();
            }

            if ( IsY2Grid ) 
            {
                ymin = Math.Min( y2a.Min, y2a.Max );
                ymax = Math.Max( y2a.Min, y2a.Max );
                ytick = y2a.Tick;

                aPen = new Pen( GridColor, GridThickness ) { DashStyle = GridPattern };

                beg = ( int ) Math.Truncate( ymin / ytick );

                if ( ( beg * ytick ) <= ymin ) beg++;

                end = ( int ) Math.Truncate( ymax / ytick );

                if ( ( end * ytick ) >= ymax ) end--;

                for ( var i = beg; i <= end; i++ )
                {
                    var y = ca.PlotRect.Bottom - ( i * ytick - ymin ) * ca.PlotRect.Height / ( ymax - ymin );

                    g.DrawLine( aPen, new PointF( ca.PlotRect.Left, y ), new PointF( ca.PlotRect.Right, y ) );
                }

                aPen.Dispose();
            }
        }


        public void FromXml( XmlReader reader )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );
            var dashStyleConverter = TypeDescriptor.GetConverter( typeof( DashStyle ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "gridcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) GridColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "gridpattern" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( dashStyleConverter.IsValid( text ) ) GridPattern = ( DashStyle ) dashStyleConverter.ConvertFromString( text );
                }

                text = reader.GetAttribute( "gridthickness" );

                if ( !string.IsNullOrEmpty( text ) ) GridThickness = float.Parse( text, provider );

                text = reader.GetAttribute( "isxgrid" );

                if ( !string.IsNullOrEmpty( text ) ) IsXGrid = Convert.ToBoolean( text );

                text = reader.GetAttribute( "isygrid" );

                if ( !string.IsNullOrEmpty( text ) ) IsYGrid = Convert.ToBoolean( text ); 

                text = reader.GetAttribute( "isy2grid" );

                if ( !string.IsNullOrEmpty( text ) ) IsY2Grid = Convert.ToBoolean( text ); 
            }
        }


        public void ToXml( XmlWriter writer )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );
            var dashStyleConverter = TypeDescriptor.GetConverter( typeof( DashStyle ) );

            writer.WriteStartElement( GetType().Name.ToLower() );

            writer.WriteAttributeString( "gridcolor", colorConverter.ConvertToString( null, cultureInfo, GridColor ) );
            writer.WriteAttributeString( "gridpattern", dashStyleConverter.ConvertToString( GridPattern ) );
            writer.WriteAttributeString( "gridthickness", GridThickness.ToString( provider ) );
            writer.WriteAttributeString( "isxgrid", IsXGrid.ToString().ToLower() );
            writer.WriteAttributeString( "isygrid", IsYGrid.ToString().ToLower() );
            writer.WriteAttributeString( "isy2grid", IsY2Grid.ToString().ToLower() );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [DisplayName( "IsXGrid" ), 
        Description( "Indicates whether the X grid is shown." ),
        Category( "Appearance" )]
        public bool IsXGrid { get; set; }

        [Browsable( true )]
        [DisplayName( "IsYGrid" ), 
        Description( "Indicates whether the Y grid is shown." ),
        Category( "Appearance" )]
        public bool IsYGrid { get; set; }

        [Browsable( true )]
        [DisplayName( "IsY2Grid" ), 
        Description( "Indicates whether the Y2 grid is shown." ),
        Category( "Appearance" )]
        public bool IsY2Grid { get; set; }

        [Browsable( true )]
        [DisplayName( "LinesPattern" ), 
        Description( "Sets the line pattern for the grid lines." ),
        Category( "Appearance" )]
        public DashStyle GridPattern { get; set; }

        [Browsable( true )]
        [DisplayName( "LinesThickness" ), 
        Description( "Sets the thickness for the grid lines." ),
        Category( "Appearance" )]
        public float GridThickness { get; set; }

        [Browsable( true )]
        [DisplayName( "LinesColor" ), 
        Description( "The color used to display the grid lines." ),
        Category( "Appearance" )]
        public Color GridColor { get; set; }

        #endregion

    }
}
