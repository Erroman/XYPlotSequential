using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class XYLabelTypeConverter : TypeConverterOf<XYLabel> { }

    [TypeConverter( typeof( XYLabelTypeConverter ) )]
    public class XYLabel
    {

        #region Constructors

        public XYLabel()
        {
            XLabel = "x";
            YLabel = "y";
            Y2Label = "y2";

            LabelFont = new Font( "Arial", 10, FontStyle.Regular );
            LabelFontColor = Color.Black;

            TickFont = new Font( "Arial", 8, FontStyle.Regular );
            TickFontColor = Color.Black;
        }


        public XYLabel( XYLabel label )
        {
            XLabel = label.XLabel;
            YLabel = label.YLabel;
            Y2Label = label.Y2Label;

            LabelFont = new Font( label.LabelFont, label.LabelFont.Style );
            LabelFontColor = label.LabelFontColor;

            TickFont = new Font( label.TickFont, label.TickFont.Style );
            TickFontColor = label.TickFontColor;
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public XYLabel Clone() => new XYLabel( this );


        public void FromXml( XmlReader reader )
        {
            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            var fontConverter = TypeDescriptor.GetConverter( typeof( Font ) );
            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "labelfont" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( fontConverter.IsValid( text ) ) LabelFont = ( Font ) fontConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "labelfontcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) LabelFontColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "tickfont" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( fontConverter.IsValid( text ) ) TickFont = ( Font ) fontConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "tickfontcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) TickFontColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                XLabel = "";
                YLabel = "";
                Y2Label = "";

                text = reader.GetAttribute( "xlabel" );

                if ( !string.IsNullOrEmpty( text ) ) XLabel = text;

                text = reader.GetAttribute( "ylabel" );

                if ( !string.IsNullOrEmpty( text ) ) YLabel = text;

                text = reader.GetAttribute( "y2label" );

                if ( !string.IsNullOrEmpty( text ) ) Y2Label = text;
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

            writer.WriteStartElement( GetType().Name.ToLower() );

            writer.WriteAttributeString( "labelfont", fontConverter.ConvertToString( null, cultureInfo, LabelFont ) );
            writer.WriteAttributeString( "labelfontcolor", colorConverter.ConvertToString( null, cultureInfo, LabelFontColor ) );
            writer.WriteAttributeString( "tickfont", fontConverter.ConvertToString( null, cultureInfo, TickFont ) );
            writer.WriteAttributeString( "tickfontcolor", colorConverter.ConvertToString( null, cultureInfo, TickFontColor ) );
            writer.WriteAttributeString( "xlabel", XLabel );
            writer.WriteAttributeString( "ylabel", YLabel );
            writer.WriteAttributeString( "y2label", Y2Label );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [Description( "Creates a label for the X axis." ),
         Category( "Appearance" )]
        public string XLabel { get; set; }

        [Browsable( true )]
        [Description( "Creates a label for the Y axis." ),
         Category( "Appearance" )]
        public string YLabel { get; set; }

        [Browsable( true )]
        [Description( "Creates a label for the Y2 axis." ),
         Category( "Appearance" )]
        public string Y2Label { get; set; }

        [Browsable( true )]
        [Description( "The font used to display the axis labels." ),
         Category( "Appearance" )]
        public Font LabelFont { get; set; }

        [Browsable( true )]
        [Description( "Sets the color of the axis labels." ),
         Category( "Appearance" )]
        public Color LabelFontColor { get; set; }

        [Browsable( true )]
        [Description( "The font used to display the tick labels." ),
         Category( "Appearance" )]
        public Font TickFont { get; set; }

        [Browsable( true )]
        [Description( "Sets the color of the tick labels." ),
         Category( "Appearance" )]
        public Color TickFontColor { get; set; }

        #endregion

    }
}
