using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts 
{
    public class SymbolStyleTypeConverter : TypeConverterOf<SymbolStyle> { }

    [TypeConverter( typeof( SymbolStyleTypeConverter ) )]
    public class SymbolStyle
    {

        #region Public enums

        public enum SymbolTypeEnum
        {
            Box = 0,
            Circle = 1,
            Cross = 2,
            Diamond = 3,
            Dot = 4,
            InvertedTriangle = 5,
            None = 6,
            OpenDiamond = 7,
            OpenInvertedTriangle = 8,
            OpenTriangle = 9,
            Square = 10,
            Star = 11,
            Triangle = 12,
            Plus = 13
        }

        #endregion

        #region Constructors

        public SymbolStyle()
        {
            AntiAlias = true;
            SymbolType = SymbolTypeEnum.None;
            SymbolSize = 8.0f;
            BorderColor = Color.Black;
            FillColor = Color.White;
            BorderThickness = 1f;
        }


        public SymbolStyle( SymbolStyle style )
        {
            AntiAlias = style.AntiAlias;
            SymbolType = style.SymbolType;
            SymbolSize = style.SymbolSize;
            BorderColor = style.BorderColor;
            FillColor = style.FillColor;
            BorderThickness = style.BorderThickness;
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public SymbolStyle Clone() => new SymbolStyle( this );


        public void DrawSymbol( Graphics g, PointF pt ) 
        {
            var aPen = new Pen( BorderColor, BorderThickness );
            var aBrush = new SolidBrush( FillColor );

            var x = pt.X;
            var y = pt.Y;
            var size = SymbolSize;
            var halfSize = size / 2.0f;

            var aRectangle = new RectangleF( x - halfSize, y - halfSize, size, size );

            switch ( SymbolType ) 
            {
                case SymbolTypeEnum.Square:

                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y - halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y + halfSize, x - halfSize, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y + halfSize, x - halfSize, y - halfSize );
                    break;

                case SymbolTypeEnum.OpenDiamond:

                    g.DrawLine( aPen, x, y - halfSize, x + halfSize, y );
                    g.DrawLine( aPen, x + halfSize, y, x, y + halfSize );
                    g.DrawLine( aPen, x, y + halfSize, x - halfSize, y );
                    g.DrawLine( aPen, x - halfSize, y, x, y - halfSize );
                    break;

                case SymbolTypeEnum.Circle:

                    g.DrawEllipse( aPen, x - halfSize, y - halfSize, size, size );
                    break;

                case SymbolTypeEnum.OpenTriangle:

                    g.DrawLine( aPen, x, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y + halfSize, x - halfSize, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y + halfSize, x, y - halfSize );
                    break;

                case SymbolTypeEnum.None:
                    break;

                case SymbolTypeEnum.Cross:

                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x - halfSize, y + halfSize );
                    break;

                case SymbolTypeEnum.Star:

                    g.DrawLine( aPen, x, y - halfSize, x, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y, x + halfSize, y );
                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x - halfSize, y + halfSize );
                    break;

                case SymbolTypeEnum.OpenInvertedTriangle:

                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y - halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x, y + halfSize );
                    g.DrawLine( aPen, x, y + halfSize, x - halfSize, y - halfSize );
                    break;

                case SymbolTypeEnum.Plus:

                    g.DrawLine( aPen, x, y - halfSize, x, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y, x + halfSize, y );
                    break;

                case SymbolTypeEnum.Dot:

                    g.FillEllipse( aBrush, aRectangle );
                    g.DrawEllipse( aPen, aRectangle );
                    break;

                case SymbolTypeEnum.Box:

                    g.FillRectangle( aBrush, aRectangle );
                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y - halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y + halfSize, x - halfSize, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y + halfSize, x - halfSize, y - halfSize );
                    break;

                case SymbolTypeEnum.Diamond:

                    var pta = new PointF[4];

                    pta[0].X = x;
                    pta[0].Y = y - halfSize;
                    pta[1].X = x + halfSize;
                    pta[1].Y = y;
                    pta[2].X = x;
                    pta[2].Y = y + halfSize;
                    pta[3].X = x - halfSize;
                    pta[3].Y = y;

                    g.FillPolygon( aBrush, pta );
                    g.DrawPolygon( aPen, pta );

                    break;

                case SymbolTypeEnum.InvertedTriangle:

                    var ptb = new PointF[ 3 ];

                    ptb[0].X = x - halfSize;
                    ptb[0].Y = y - halfSize;
                    ptb[1].X = x + halfSize;
                    ptb[1].Y = y - halfSize;
                    ptb[2].X = x;
                    ptb[2].Y = y + halfSize;

                    g.FillPolygon( aBrush, ptb );
                    g.DrawPolygon( aPen, ptb );
                    break;

                case SymbolTypeEnum.Triangle:

                    var ptc = new PointF[3];

                    ptc[0].X = x;
                    ptc[0].Y = y - halfSize;
                    ptc[1].X = x + halfSize;
                    ptc[1].Y = y + halfSize;
                    ptc[2].X = x - halfSize;
                    ptc[2].Y = y + halfSize;

                    g.FillPolygon( aBrush, ptc );
                    g.DrawPolygon( aPen, ptc );
                    break;
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
            var symbolTypeEnumConverter = TypeDescriptor.GetConverter( typeof( SymbolTypeEnum ) );

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "symbolantialias" );

                if ( !string.IsNullOrEmpty( text ) ) AntiAlias = Convert.ToBoolean( text );

                text = reader.GetAttribute( "symbolsize" );

                if ( !string.IsNullOrEmpty( text ) ) SymbolSize = float.Parse( text, provider );

                text = reader.GetAttribute( "symboltype" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( symbolTypeEnumConverter.IsValid( text ) ) SymbolType = ( SymbolTypeEnum ) symbolTypeEnumConverter.ConvertFromString( text );
                }

                text = reader.GetAttribute( "symbolborderthickness" );

                if ( !string.IsNullOrEmpty( text ) ) BorderThickness = float.Parse( text, provider );

                text = reader.GetAttribute( "symbolbordercolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) BorderColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
                }

                text = reader.GetAttribute( "symbolfillcolor" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( colorConverter.IsValid( text ) ) FillColor = ( Color ) colorConverter.ConvertFromString( null, cultureInfo, text );
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
            var symbolTypeEnumConverter = TypeDescriptor.GetConverter( typeof( SymbolTypeEnum ) );

            writer.WriteAttributeString( "symbolantialias", AntiAlias.ToString().ToLower() );
            writer.WriteAttributeString( "symbolsize", SymbolSize.ToString( provider ) );
            writer.WriteAttributeString( "symboltype", symbolTypeEnumConverter.ConvertToString( SymbolType ) );
            writer.WriteAttributeString( "symbolborderthickness", BorderThickness.ToString( provider ) );
            writer.WriteAttributeString( "symbolbordercolor", colorConverter.ConvertToString( null, cultureInfo, BorderColor ) );
            writer.WriteAttributeString( "symbolfillcolor", colorConverter.ConvertToString( null, cultureInfo, FillColor ) );
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [Category( "AntiAlias" )]
        public bool AntiAlias { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public float BorderThickness { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public Color BorderColor { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public Color FillColor { get; set; }

        [Browsable( true )]
        [DisplayName( "Size" ), 
        Category( "Appearance" )]
        public float SymbolSize { get; set; }

        [Browsable( true )]
        [DisplayName( "Type" ), 
        Category( "Appearance" )]
        public SymbolTypeEnum SymbolType { get; set; }

        #endregion

    }
}
