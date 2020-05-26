using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml;

using SMath.Manager;


namespace JXCharts
{
    public class AxisTypeConverter : TypeConverterOf<Axis> { }

    [TypeConverter( typeof( AxisTypeConverter ) )]
    public class Axis
    {

        #region Private fields

        private int _minPhysicalLargeTickStep = 60;

        #endregion

        #region Public fields

        /// <summary>
        /// If LargeTickStep isn't specified, then a suitable value is
        /// calculated automatically. The value will be of the form
        /// m*10^e for some m in this set.
        /// </summary>
        public double[] Mantissas = { 1.0, 2.0, 5.0 };

        #endregion

        #region Constructors

        public Axis()
        {
            Visible = true;

            Min = -1f;
            Max = 1f;
            Tick = 0.5f;

            DecimalPlaces = 3;
            NumberFormat = NumberFormatEnum.General;
        }


        public Axis( Axis axis )
        {
            Visible = axis.Visible;

            Min = axis.Min;
            Max = axis.Max;
            Tick = axis.Tick;

            DecimalPlaces = axis.DecimalPlaces;
            NumberFormat = axis.NumberFormat;
        }

        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public virtual Axis Clone() => new Axis( this );


        /// <summary>
        /// Calculates the world spacing between large ticks, based on the physical
        /// axis length (parameter), world axis length, Mantissa values and 
        /// MinPhysicalLargeTickStep. A value such that at least two 
        /// </summary>
        /// <param name="physicalLength">physical length of the axis</param>
        /// <param name="shouldCullMiddle">Returns true if we were forced to make spacing of 
        /// large ticks too small in order to ensure that there are at least two of 
        /// them. The draw ticks method should not draw more than two large ticks if this
        /// returns true.</param>
        /// <returns>Large tick spacing</returns>
        /// <remarks>TODO: This can be optimised a bit.</remarks>
        public float DetermineLargeTickStep( float physicalLength, out bool shouldCullMiddle )
        {
            shouldCullMiddle = false;

            if ( float.IsNaN( Min ) || float.IsNaN( Max ) )
            {
                throw new Exception( "world extent of axis not set." );
            }

            // if the large tick has been explicitly set, then return this.
            //if ( !float.IsNaN( Tick ) )
            //{
            //    if ( Tick <= 0.0f )
            //    {
            //        throw new Exception( "can't have negative or zero tick step - reverse WorldMin WorldMax instead." );
            //    }
            //
            //    return Tick;
            //}

            // otherwise we need to calculate the large tick step ourselves.

            // adjust world max and min for offset and scale properties of axis.
            var adjustedMax = Max;
            var adjustedMin = Min;

            var range = Math.Abs( adjustedMax - adjustedMin );

            // if axis has zero world length, then return arbitrary number.
            if ( Math.Abs( adjustedMax - adjustedMin ) < float.Epsilon * 1000.0 ) 
            {
                return 1.0f;
            }

            double approxTickStep = _minPhysicalLargeTickStep / physicalLength * range;
            
            var exponent = Math.Floor( Math.Log10( approxTickStep ) );
            var mantissa = Math.Pow( 10.0, Math.Log10( approxTickStep ) - exponent );

            // determine next whole mantissa below the approx one.
            var mantissaIndex = Mantissas.Length - 1;

            for ( var i = 1; i < Mantissas.Length; ++i )
            {
                if ( mantissa < Mantissas[i] )
                {
                    mantissaIndex = i - 1;
                    break;
                }
            }

            // then choose next largest spacing. 
            mantissaIndex += 1;

            if ( mantissaIndex == Mantissas.Length )
            {
                mantissaIndex = 0;
                exponent += 1.0;
            }

            // now make sure that the returned value is such that at least two 
            // large tick marks will be displayed.
            var tickStep = Math.Pow( 10.0, exponent ) * Mantissas[ mantissaIndex ];

            var physicalStep = ( float ) ( ( tickStep / range ) * physicalLength );

            while ( physicalStep > physicalLength / 2 )
            {
                shouldCullMiddle = true;

                mantissaIndex -= 1;

                if ( mantissaIndex == -1 )
                {
                    mantissaIndex = Mantissas.Length - 1;
                    exponent -= 1.0;
                }

                tickStep = Math.Pow( 10.0, exponent ) * Mantissas[ mantissaIndex ];
                physicalStep = ( float ) ( ( tickStep / range ) * physicalLength );
            }

            // and we're done.
            return ( float ) ( Math.Pow( 10.0, exponent ) * Mantissas[ mantissaIndex ] );
        }
        

        public virtual void FromXml( XmlReader reader )
        {
            var numberFormatEnumConverter = TypeDescriptor.GetConverter( typeof( NumberFormatEnum ) );

            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            cultureInfo.TextInfo.ListSeparator = GlobalProfile.ArgumentsSeparatorStandard.ToString();

            if ( reader.HasAttributes )
            {
                var text = reader.GetAttribute( "visible" );

                if ( !string.IsNullOrEmpty( text ) ) Visible = Convert.ToBoolean( text );

                text = reader.GetAttribute( "decimalplaces" );

                if ( !string.IsNullOrEmpty( text ) ) DecimalPlaces = Convert.ToInt32( text );

                text = reader.GetAttribute( "numberformat" );

                if ( !string.IsNullOrEmpty( text ) )
                {
                    if ( numberFormatEnumConverter.IsValid( text ) )
                    {
                        NumberFormat = ( NumberFormatEnum ) numberFormatEnumConverter.ConvertFromString( null, cultureInfo, text );
                    }
                }                  
            }
        }


        public virtual void ToXml( XmlWriter writer )
        {
            var numberFormatEnumConverter = TypeDescriptor.GetConverter( typeof( NumberFormatEnum ) );

            writer.WriteAttributeString( "visible", Visible.ToString() );
            writer.WriteAttributeString( "decimalplaces", DecimalPlaces.ToString() );
            writer.WriteAttributeString( "numberformat", numberFormatEnumConverter.ConvertToString( NumberFormat ) );
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [Description( "Shows axis." ),
         Category( "Appearance" )]
        public bool Visible { get; set; }

        [Browsable( true )]
        [Description( "Sets the minimum limit for the axis." ),
         Category( "Appearance" )]
        public float Min { get; set; }

        [Browsable( true )]
        [Description( "Sets the maximum limit for the axis." ),
         Category( "Appearance" )]
        public float Max { get; set; }

        [Browsable( true )]
        [Description( "Sets the ticks for the axis." ),
         Category( "Appearance" )]
        public float Tick { get; set; }

        [Browsable( true )]
        [Description( "Number format." ),
         Category( "Appearance" )]
        public NumberFormatEnum NumberFormat { get; set; }

        [Browsable( true )]
        [Description( "Sets number of decimal places." ),
         Category( "Appearance" )]
        public int DecimalPlaces { get; set; }

        #endregion

    }
}
