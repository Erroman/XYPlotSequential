using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts 
{
    public class LineStyleTypeConverter : TypeConverterOf<LineStyle> { }

    [TypeConverter( typeof( LineStyleTypeConverter ) )]
    public class LineStyle 
    {

        #region Constructors

        public LineStyle()
        {
            AntiAlias = true;
            Visible = true;
            Pattern = DashStyle.Solid;
            LineColor = Color.Blue;
            Thickness = 1.0f;
            PlotMethod = PlotLinesMethodEnum.Lines;
        }


        public LineStyle( LineStyle style )
        {
            Visible = style.Visible;
            AntiAlias = style.AntiAlias;
            Pattern = style.Pattern;            
            LineColor = style.LineColor;            
            Thickness = style.Thickness;
            PlotMethod = style.PlotMethod;
        }

        #endregion

        #region Public enums

        public enum PlotLinesMethodEnum
        {
            Lines = 0,
            Splines,
            Labels,
            Shapes
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public LineStyle Clone() => new LineStyle( this );


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
            var plotLinesMethodEnumConverter = TypeDescriptor.GetConverter( typeof( PlotLinesMethodEnum ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "isvisible" );

                if ( !string.IsNullOrEmpty( text ) ) Visible = Convert.ToBoolean( text );

                text = reader.GetAttribute( "lineantialias" );

                if ( !string.IsNullOrEmpty( text ) ) AntiAlias = Convert.ToBoolean( text );

                text = reader.GetAttribute( "linecolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) LineColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "linethickness" );

                if ( !string.IsNullOrEmpty( text ) ) Thickness = float.Parse( text, provider );

                text = reader.GetAttribute( "linepattern" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( dashStyleConverter.IsValid( text ) ) Pattern = ( DashStyle ) dashStyleConverter.ConvertFromString( text );
                }

                text = reader.GetAttribute( "plotmethod" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    try
                    {
                        if ( plotLinesMethodEnumConverter.IsValid( text ) ) PlotMethod = ( PlotLinesMethodEnum ) plotLinesMethodEnumConverter.ConvertFromString( text );
                    }
                    catch
                    {
                        PlotMethod = PlotLinesMethodEnum.Lines;
                    }
                }
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
            var plotLinesMethodEnumConverter = TypeDescriptor.GetConverter( typeof( PlotLinesMethodEnum ) );

            writer.WriteAttributeString( "isvisible", Visible.ToString().ToLower() );            
            writer.WriteAttributeString( "plotmethod", plotLinesMethodEnumConverter.ConvertToString( PlotMethod ) );

            writer.WriteAttributeString( "lineantialias", AntiAlias.ToString().ToLower() );
            writer.WriteAttributeString( "linecolor", colorConverter.ConvertToString( null, cultureInfo, LineColor ) );
            writer.WriteAttributeString( "linethickness", Thickness.ToString( provider ) );
            writer.WriteAttributeString( "linepattern", dashStyleConverter.ConvertToString( Pattern ) );
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [Category( "Appearance" )]
        public bool Visible { get; set; }

        [Browsable( true )]
        [Category( "AntiAlias" )]
        public bool AntiAlias { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public PlotLinesMethodEnum PlotMethod { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public DashStyle Pattern { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public float Thickness { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public Color LineColor { get; set; }

        #endregion

    }
}
