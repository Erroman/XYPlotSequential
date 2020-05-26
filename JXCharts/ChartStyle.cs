using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using System.Linq;

using SMath.Manager;


namespace JXCharts 
{
    public class TypeConverterOf<T> : TypeConverter
    {
        public override bool GetPropertiesSupported( ITypeDescriptorContext context ) => true;

        public override PropertyDescriptorCollection GetProperties( ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var props = TypeDescriptor.GetProperties( typeof(T) ).Cast<PropertyDescriptor>().ToList();

            props = props.Where( p => p.Attributes.OfType<BrowsableAttribute>().Any( a => a.Browsable ) ).ToList();

            return new PropertyDescriptorCollection( props.ToArray() );
        }
    }


    public enum NumberFormatEnum 
    {
        General = 0,
        Exponential,
        FixedPoint
    }

    public class ChartStyleConverter : TypeConverterOf<ChartStyle> { }

    [TypeConverter( typeof( ChartStyleConverter ) )]  
    public class ChartStyle
    {

        #region Constructors

        public ChartStyle()
        {
            ChartBackColor = Color.Transparent;
            ChartBorderColor = Color.Transparent;
            PlotBackColor = Color.White;
            PlotBorderColor = Color.Black;            
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public void FromXml( XmlReader reader )
        {
            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "backcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) PlotBackColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "bordercolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) PlotBorderColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
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

            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );

            writer.WriteStartElement( GetType().Name.ToLower() );

            writer.WriteAttributeString( "backcolor", colorConverter.ConvertToString( null, cultureInfo, PlotBackColor ) );
            writer.WriteAttributeString( "bordercolor", colorConverter.ConvertToString( null, cultureInfo, PlotBorderColor ) );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Browsable( false )]
        [Description( "The background color of the chart area." ),         
         Category( "Appearance" )]
        public Color ChartBackColor { get; set; }

        [Browsable( false )]
        [Description( "The border color of the chart area." ),         
         Category( "Appearance" )]
        public Color ChartBorderColor { get; set; }

        [Browsable( true )]
        [Description( "The background color of the plot area." ),
         DisplayName( "BackColor" ),
         Category( "Appearance" )]
        public Color PlotBackColor { get; set; }

        [Browsable( true )]
        [Description( "The border color of the plot area." ),
         DisplayName( "BorderColor" ),
         Category( "Appearance" )]
        public Color PlotBorderColor { get; set; }

        #endregion

    }
}
