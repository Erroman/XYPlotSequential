using System.ComponentModel;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class XAxisTypeConverter : TypeConverterOf<XAxis> { }

    [TypeConverter( typeof( XAxisTypeConverter ) )]
    public class XAxis : Axis
    {
        
        #region Constructors

        public XAxis()
        {
            Min = -5f;
            Max = 5f;
            Tick = 2.5f;
        }


        public XAxis( XAxis axis ) : base( axis )
        {
        }

        #endregion

        #region Public methods

        public new XAxis Clone() => new XAxis( this );


        public override void FromXml( XmlReader reader )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "xmin" );

                if ( !string.IsNullOrEmpty( text ) ) Min = float.Parse( text, provider );

                text = reader.GetAttribute( "xmax" );

                if ( !string.IsNullOrEmpty( text ) ) Max = float.Parse( text, provider );

                text = reader.GetAttribute( "xtick" );

                if ( !string.IsNullOrEmpty( text ) ) Tick = float.Parse( text, provider );
            }

            base.FromXml( reader );
        }


        public override void ToXml( XmlWriter writer )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            // FIXME: xaxes -> xaxis
            //writer.WriteStartElement( GetType().Name.ToLower() );
            writer.WriteStartElement( "xaxes" );

            writer.WriteAttributeString( "xmin", Min.ToString( provider ) );
            writer.WriteAttributeString( "xmax", Max.ToString( provider ) );
            writer.WriteAttributeString( "xtick", Tick.ToString( provider ) );

            base.ToXml( writer );

            writer.WriteEndElement();
        }

        #endregion

    }
}
