using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class TitleTypeConverter : TypeConverterOf<Title> { }

    [TypeConverter( typeof( TitleTypeConverter ) )]
    public class Title
    {

        #region Constructors

        public Title()
        {
            Text = "";

            TitleFont = new Font( "Arial", 10, FontStyle.Regular );
            TitleFontColor = Color.Black;
        }


        public Title( Title title )
        {
            Text = title.Text;
            TitleFont = new Font( title.TitleFont, title.TitleFont.Style );
            TitleFontColor = title.TitleFontColor;
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public Title Clone() => new Title( this );


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
                Text = "";

                var tmp = reader.GetAttribute( "title" );

                if ( !string.IsNullOrEmpty( tmp ) ) Text = tmp;

                tmp = reader.GetAttribute( "titlefont" );

                if ( !string.IsNullOrEmpty( tmp ) )
                {
                    if ( fontConverter.IsValid( tmp ) ) TitleFont = ( Font ) fontConverter.ConvertFromString( null, cultureInfo, tmp );
                }

                tmp = reader.GetAttribute( "titlefontcolor" );

                if ( !string.IsNullOrEmpty( tmp ) )
                {
                    if ( colorConverter.IsValid( tmp ) ) TitleFontColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, tmp );
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

            // FIXME: title2d -> title
            //writer.WriteStartElement( GetType().Name.ToLower() );
            writer.WriteStartElement( "title2d" );

            writer.WriteAttributeString( "title", Text );
            writer.WriteAttributeString( "titlefont", fontConverter.ConvertToString( null, cultureInfo, TitleFont ) );
            writer.WriteAttributeString( "titlefontcolor", colorConverter.ConvertToString( null, cultureInfo, TitleFontColor ) );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [DisplayName( "Text" ), 
        Description( "Creates a title for the chart." ),
        Category( "Appearance" )]
        public string Text { get; set; }

        [Browsable( true )]
        [DisplayName( "Font" ), 
        Description( "The font used to display the title." ),
        Category( "Appearance" )]
        public Font TitleFont { get; set; }

        [Browsable( true )]
        [DisplayName( "FontColor" ), 
        Description( "Sets the color of the tile." ),
        Category( "Appearance" )]
        public Color TitleFontColor { get; set; }

        #endregion

    }
}
