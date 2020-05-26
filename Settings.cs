using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using SMath.Manager;


namespace XYPlotPluginSeq
{
    public class FormSettings
    {

        #region Constructors

        public FormSettings()
        {
            Size = new Size( 360, 480 );
            Location = new Point( 0, 0 );
            State = FormWindowState.Normal;   
        }

        #endregion

        #region Properties

        public Size Size { get; set; }
        public Point Location { get; set; }
        public FormWindowState State { get; set; }

        #endregion

    }

    public static class GlobalConfig
    {
        public static Settings Settings = new Settings();
    }


    public class Settings
    {

        #region Private fields

        private string _configFileName = "XYPlotPlugin.config";

        #endregion

        #region Constructors

        public Settings()
        {
            FormFormatSettings = new FormSettings();
            FormCollection = new FormSettings();

            Load();
        }

        #endregion

        #region Private methods

        private void SerializeProperty( XmlWriter writer, PropertyInfo info, ref string path )
        {
            writer.WriteStartElement( info.Name );

            try
            {
                var value = this.GetPropValue( path );

                var props = value.GetType().GetProperties().Where( p => p.CanRead && p.CanWrite ).ToList();

                if ( props.Any() )
                {
                    foreach ( var prop in props )
                    {
                        var name = path + "." + prop.Name;

                        SerializeProperty( writer, prop, ref name );
                    }
                }
                else
                {
                    var cultureInfo = new CultureInfo( "" )
                    {
                        NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
                    };

                    var converter = TypeDescriptor.GetConverter( value.GetType() );

                    var str = converter.ConvertToString( null, cultureInfo, value );

                    if ( str != null )
                    {
                        writer.WriteString( str );
                    }
                }
            }
            catch { }
            finally
            {
                writer.WriteEndElement();
            }
        }


        private void DeserializeProperty( XmlReader reader, PropertyInfo info, ref object result, ref string path )
        {
            var props = info.PropertyType.GetProperties().Where( p => p.CanRead && p.CanWrite ).ToList();

            if ( props.Any() )
            {
                reader.MoveToElement();

                if ( !reader.IsEmptyElement )
                {
                    var tag = reader.Name;

                    while ( reader.Read() )
                    {
                        reader.MoveToContent();

                        if ( reader.NodeType == XmlNodeType.Element )
                        {
                            if ( props.Select( p => p.Name ).Contains( reader.Name ) )
                            {
                                var prop = props.Find( p => p.Name == reader.Name );

                                var name = path + "." + prop.Name;

                                try
                                {
                                    var obj = this.GetPropValue( name );

                                    DeserializeProperty( reader, prop, ref obj, ref name );

                                    prop.SetValue( result, obj, null );
                                }
                                catch { }
                            }
                        }

                        if ( reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals( tag ) ) break;
                    }
                }
            }
            else
            {
                var cultureInfo = new CultureInfo( "" )
                {
                    NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
                };

                var converter = TypeDescriptor.GetConverter( result.GetType() );

                result = converter.ConvertFromString( null, cultureInfo, reader.ReadString() );
            }
        }
                

        private void FromXml( XmlReader reader )
        {
            reader.MoveToElement();

            // <Settings>...</Settings>
            if ( !reader.IsEmptyElement )
            {
                var tag = reader.Name;

                var props = GetType().GetProperties().Where( p => p.CanRead && p.CanWrite ).ToList();

                while ( reader.Read() )
                {
                    reader.MoveToContent();

                    if ( reader.NodeType == XmlNodeType.Element )
                    {
                        if ( props.Select( p => p.Name ).Contains( reader.Name ) )
                        {
                            var prop = props.Find( p => p.Name == reader.Name );

                            var path = prop.Name;

                            try
                            {
                                var result = this.GetPropValue( path );

                                DeserializeProperty( reader, prop, ref result, ref path );

                                prop.SetValue( this, result, null );
                            }
                            catch { }
                        }
                    }

                    if ( reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals( tag ) ) break;
                }
            }
        }


        private void ToXml( XmlWriter writer )
        {
            writer.WriteStartElement( GetType().Name );

            var props = GetType().GetProperties().Where( p => p.CanRead && p.CanWrite ).ToList();

            try
            {
                if ( props.Any() )
                {
                    foreach ( var prop in props )
                    {
                        var path = prop.Name;

                        SerializeProperty( writer, prop, ref path );
                    }
                }
            }
            catch { }
            finally
            {
                writer.WriteEndElement();
            }
        }

        #endregion

        #region Public methods

        public void Load()
        {
            var fileName = Path.GetDirectoryName( new Uri( Assembly.GetExecutingAssembly().CodeBase ).LocalPath );

            fileName = Path.Combine( fileName, _configFileName );

            if ( File.Exists( fileName ) )
            {
                try
                {
                    using ( var fs = new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                    {
                        using ( var reader = XmlReader.Create( fs ) )
                        {
                            while ( reader.Read() )
                            {
                                FromXml( reader );
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                Save();
            }
        }


        public void Save()
        {
            var fileName = Path.GetDirectoryName( new Uri( Assembly.GetExecutingAssembly().CodeBase ).LocalPath );

            try
            {
                fileName = Path.Combine( fileName, _configFileName );

                using ( var fs = new FileStream( fileName, FileMode.Create ) )
                {
                    using ( var writer = new XmlTextWriter( fs, Encoding.UTF8 ) )
                    {
                        writer.Formatting = Formatting.Indented;
                        writer.Indentation = 4;
                        writer.IndentChar = ' ';

                        ToXml( writer );

                        writer.Flush();
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Properties

        public bool Debug { get; set; }

        public FormSettings FormFormatSettings { get; set; }

        public FormSettings FormCollection { get; set; }

        #endregion

    }
}
