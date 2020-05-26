using System;
using System.ComponentModel;
using System.Xml;

namespace XYPlotPluginSeq
{
    public class PropertiesSourceConverter : TypeConverter
    {
        public override bool GetPropertiesSupported( ITypeDescriptorContext context )
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties( ITypeDescriptorContext context, object value, Attribute[] attributes )
        {
            return TypeDescriptor.GetProperties( typeof( PropertiesSource ) );
        }
    }

    [TypeConverter( typeof( PropertiesSourceConverter ) )]
    public class PropertiesSource 
    {
        public enum SourceTypeEnum 
        {
            PropertyGrid = 0,
            Sheet = 1
        }

        #region Private fields

        private int _index;
        private SourceTypeEnum _srcType;

        #endregion

        #region Constructors

        public PropertiesSource( int index, SourceTypeEnum srcType ) 
        {
            _index = index;
            _srcType = srcType;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return "(...)";
        }

        public void FromXml( XmlReader reader )
        {
            var sourceTypeConverter = TypeDescriptor.GetConverter( typeof( SourceTypeEnum ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "index" );

                if ( !string.IsNullOrEmpty( text ) ) _index = int.Parse( text );

                text = reader.GetAttribute( "sourcetype" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( sourceTypeConverter.IsValid( text ) ) _srcType = ( SourceTypeEnum ) sourceTypeConverter.ConvertFromString( text );
                } 
            }
        }

        public void ToXml( XmlWriter writer )
        {
            var sourceTypeConverter = TypeDescriptor.GetConverter( typeof( SourceTypeEnum ) );

            writer.WriteStartElement( GetType().Name.ToLower() );

            writer.WriteAttributeString( "index", _index.ToString() );
            writer.WriteAttributeString( "sourcetype", sourceTypeConverter.ConvertToString( _srcType ) );

            writer.WriteEndElement();
        }

        #endregion

        #region Properties

        [Category( "Appearance" )]
        public int Index 
        {
            get { return _index; }
            set { _index = value; }
        }

        [Category( "Appearance" )]
        public SourceTypeEnum SourceType 
        {
            get { return _srcType; }
            set { _srcType = value; }
        }

        #endregion

    }
}