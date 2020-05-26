using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class Y2AxisTypeConverter : TypeConverterOf<Y2Axis> { }

    [TypeConverter( typeof( Y2AxisTypeConverter ) )]
    public class Y2Axis : Axis
    {

        #region Constructors

        public Y2Axis() 
        {
        }


        public Y2Axis( Y2Axis axis ) : base( axis )
        {
            IsY2Axis = axis.IsY2Axis;
        }

        #endregion

        #region Public methods

        public new Y2Axis Clone() => new Y2Axis ( this );


        public override void FromXml( XmlReader reader )
        {            
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "isy2axis" );

                if ( !string.IsNullOrEmpty( text ) ) IsY2Axis = Convert.ToBoolean( text );

                text = reader.GetAttribute( "y2min" );

                if ( !string.IsNullOrEmpty( text ) ) Min = float.Parse( text, provider );

                text = reader.GetAttribute( "y2max" );

                if ( !string.IsNullOrEmpty( text ) ) Max = float.Parse( text, provider );

                text = reader.GetAttribute( "y2tick" );

                if ( !string.IsNullOrEmpty( text ) ) Tick = float.Parse( text, provider );
            }

            base.FromXml( reader );
        }


        public override void ToXml( XmlWriter writer )
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            // FIXME: y2xes -> y2axis
            //writer.WriteStartElement( GetType().Name.ToLower() );
            writer.WriteStartElement( "y2axes" );
            
            writer.WriteAttributeString( "isy2axis", IsY2Axis.ToString().ToLower() );
            writer.WriteAttributeString( "y2min", Min.ToString( provider ) );
            writer.WriteAttributeString( "y2max", Max.ToString( provider ) );
            writer.WriteAttributeString( "y2tick", Tick.ToString( provider ) );

            base.ToXml( writer );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [Description( "Indicates whether the chart has the Y2 axis." ),
        Category( "Appearance" )]
        public bool IsY2Axis { get; set; }

        #endregion

    }
}
