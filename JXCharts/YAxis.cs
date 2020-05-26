using System.ComponentModel;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class YAxisTypeConverter : TypeConverterOf<YAxis> { }

    [TypeConverter( typeof( YAxisTypeConverter ) )]
    public class YAxis : Axis
    {

        #region Constructors

        public YAxis() 
        {
        }


        public YAxis( YAxis axis ) : base( axis )
        {
        }

        #endregion

        #region Public methods

        public new YAxis Clone() => new YAxis( this );


        public override void FromXml( XmlReader reader )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "ymin" );

                if ( !string.IsNullOrEmpty( text ) ) Min = float.Parse( text, provider );

                text = reader.GetAttribute( "ymax" );

                if ( !string.IsNullOrEmpty( text ) ) Max = float.Parse( text, provider );

                text = reader.GetAttribute( "ytick" );

                if ( !string.IsNullOrEmpty( text ) ) Tick = float.Parse( text, provider );
            }

            base.FromXml( reader );
        }


        public override void ToXml( XmlWriter writer )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            // FIXME: yxes -> yaxis
            //writer.WriteStartElement( GetType().Name.ToLower() );
            writer.WriteStartElement( "yaxes" );

            writer.WriteAttributeString( "ymin", Min.ToString( provider ) );
            writer.WriteAttributeString( "ymax", Max.ToString( provider ) );
            writer.WriteAttributeString( "ytick", Tick.ToString( provider ) );
            
            base.ToXml( writer );

            writer.WriteEndElement();
        }

        #endregion

    }
}
