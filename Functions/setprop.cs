using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using SMath.Manager;
using SMath.Math;
using SMath.Math.Numeric;


namespace XYPlotPluginSeq.Functions 
{
    public class setprop : IFunction 
    {

        #region Private fields

        private Dictionary<string, string> _description = new Dictionary<string, string>
        {
            [ "ENG" ] = "set value of property.",
            [ "RUS" ] = "установить значение свойства.",
            [ "UKR" ] = "установить значение свойства.",
            [ "BEL" ] = "установить значение свойства."
        };

        #endregion

        #region Public fields

        public static int[] Arguments = { 2 };

        #endregion

        #region Constructors

        public setprop( int childCount ) => Info = new Term( "setprop", TermType.Function, childCount );

        #endregion

        #region Private methods

        private void Substitute( Store context, ref string result )
        {
            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            var pattern = new Regex( @"\{\s*\w+\s*\}", RegexOptions.Compiled );

            var block = result;

            result = string.Empty;

            var m = pattern.Match( block );

            while ( m.Success )
            {
                var tmp = block.Substring( m.Index, m.Length );

                try
                {
                    var value = Computation.NumericCalculation( new Entry( tmp.Trim( '{', '}' ) ), context );

                    if ( value.obj is TDouble )
                    {
                        var dbl = ( TDouble ) value.obj;

                        if ( dbl.isText && pattern.IsMatch( dbl.Text ) )
                        {
                            var txt = dbl.Text.Trim( '"' );

                            Substitute( context, ref txt );

                            tmp = txt;
                        }
                        else if ( dbl.isText )
                        {
                            tmp = dbl.Text.Trim( '"' );
                        }
                        else
                        {
                            tmp = dbl.ToDouble().ToString( cultureInfo );
                        }
                    }
                    else if ( value.obj is TFraction )
                    {
                        var dbl = ( TFraction ) value.obj;

                        tmp = dbl.ToDouble().ToString( cultureInfo );
                    }
                }
                catch {}

                block = block.Remove( m.Index, m.Length ).Insert( m.Index, tmp );

                result += block.Substring( 0, m.Index + tmp.Length );

                block = block.Substring( m.Index + tmp.Length );

                m = pattern.Match( block );
            }

            result += block;
        }


        private Entry SetPropertyValue( object self, PropertyInfo pinfo, object pvalue, BaseEntry newvalue, Store context )
        {
            var result = new Entry( "0" );

            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            var pattern = new Regex( @"\{\s*\w+\s*\}", RegexOptions.Compiled );

            var converter = TypeDescriptor.GetConverter( pvalue.GetType() );

            if ( newvalue is TComplex )
            {
            }
            else if ( newvalue is TMatrix )
            {
            }
            else if ( newvalue is TSystem )
            {
            }
            else if ( newvalue is TDouble )
            {
                var numericTypes = new List<Type> { typeof( byte ), typeof( short ), typeof( int ), typeof( float ), typeof( double ) };

                var dbl = ( TDouble ) newvalue;

                if ( dbl.isText )
                {
                    var tmp = dbl.Text.Trim( '"' );

                    if ( pvalue is string )
                    {
                        if ( pattern.IsMatch( tmp ) )
                        {
                            Substitute( context, ref tmp );

                            tmp = TermsConverter.DecodeText( tmp );
                        }

                        pvalue = converter.ConvertFromString( null, cultureInfo, tmp );

                        pinfo.SetValue( self, pvalue, null );

                        result = new Entry( "1" );
                    }

                    else if ( pvalue is Font || pvalue is Color )
                    {
                        if ( pattern.IsMatch( tmp ) )
                        {
                            Substitute( context, ref tmp );

                            tmp = TermsConverter.DecodeText( tmp );
                        }

                        pvalue = converter.ConvertFromString( null, cultureInfo, tmp );

                        pinfo.SetValue( self, pvalue, null );

                        result = new Entry( "1" );
                    }

                    else if ( numericTypes.Contains( pvalue.GetType() ) )
                    {
                        if ( pattern.IsMatch( tmp ) )
                        {
                            Substitute( context, ref tmp );

                            tmp = TermsConverter.DecodeText( tmp );
                        }

                        pvalue = converter.ConvertFromString( null, cultureInfo, tmp );

                        pinfo.SetValue( self, pvalue, null );

                        result = new Entry( "1" );
                    }

                    else
                    {
                        pvalue = converter.ConvertFromString( null, cultureInfo, tmp );

                        pinfo.SetValue( self, pvalue, null );

                        result = new Entry( "1" );
                    }
                }

                else 
                {
                    if ( numericTypes.Contains( pvalue.GetType() ) )
                    {
                        var text = newvalue.ToDouble().ToString( cultureInfo );

                        pvalue = converter.ConvertFromString( null, cultureInfo, text );

                        pinfo.SetValue( self, pvalue, null );

                        result = new Entry( "1" );
                    }

                    else if ( pvalue is bool )
                    {
                        var text = newvalue.ToDouble().ToString( cultureInfo ) == "0" ? "False" : "True";

                        pvalue = converter.ConvertFromString( null, cultureInfo, text );

                        pinfo.SetValue( self, pvalue, null );

                        result = new Entry( "1" );
                    }
                }
            }
            else
            {
                var text = newvalue.ToDouble().ToString( cultureInfo );

                pvalue = converter.ConvertFromString( null, cultureInfo, text );

                pinfo.SetValue( self, pvalue, null );

                result = new Entry( "1" );
            }

            return result;
        }

        #endregion

        #region Public methods

        public TermInfo GetTermInfo( string lang ) 
        {
            // The default language: ENG.
            var funcInfo = _description[ "ENG" ];

            // Translation, if it exists.
            foreach ( var pair in _description ) if ( pair.Key.Equals( lang ) ) funcInfo = pair.Value;

            var argsInfo = new List<ArgumentInfo>
            {
                new ArgumentInfo( ArgumentSections.String ),
                new ArgumentInfo( ArgumentSections.SymbolicExpression, true )
            };

            return new TermInfo( Info.Text, Info.Type, funcInfo, FunctionSections.Unknown, true, argsInfo.ToArray() );
        }


        public bool TryEvaluateExpression( Entry value, Store context, ref Entry result )
        {
            result = new Entry( "0" );            

            // Get object name.
            var arg1 = ( TDouble ) Computation.NumericCalculation( value.Items[0], context ).obj;
            
            var propname = TermsConverter.DecodeText( arg1.Text ).Trim( '"' );

            // Substitute variables from worksheet.
            var pattern = new Regex( @"\{\s*\w+\s*\}", RegexOptions.Compiled );

            if ( pattern.IsMatch( propname ) )
            {
                Substitute( context, ref propname );
            }

            // Find instances using name.
            var name = propname.Split( '.' ).FirstOrDefault();

            if ( string.IsNullOrEmpty( name ) ) return true;

            var items = new List<object>();

            var list = new Dictionary<string,string>
            {
                { "MapleTools", "MaplePlugin.MaplePlot" },
                { "XYPlotRegionSeq", "XYPlotPluginSeq.XYPlotSeq" },
                { "ZedGraphRegion", "ZedGraphPlugin.ZedGraph" }
            };

            // Find other types and instances.
            foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if ( list.Keys.All( k => !assembly.GetName().Name.Contains(k) ) ) continue;

                var type = assembly.GetType( list[ assembly.GetName().Name ] );

                var prop = type.GetField( "Instances", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public );

                var instances = ( List<object> ) prop?.GetValue( null );

                if ( instances == null ) continue;

                foreach ( var canv in instances )
                {
                    var prop1 = canv.GetType().GetProperty( "Name", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public );

                    var name1 = ( string ) prop1?.GetValue( canv, null );

                    if ( name1 != name ) continue;

                    items.Add( canv );
                }
            }

            // Get object value.
            var arg2 = Computation.NumericCalculation( value.Items[1], context );

            foreach ( var canv in items )
            {
                propname = propname.Replace( name + ".", "" );

                // TODO: Too complex.
                try
                {
                    var count = propname.Split( '.' ).Length;

                    var props = canv.GetType().GetProperties().Where( p => p.CanRead ).ToList();

                    var path = "";
                    var self = canv;

                    for ( var n = 0; n < count; n++ )
                    {
                        path = propname.Split( '.' )[n];

                        if ( props.All( x => x.Name != path ) ) { path = ""; break; }

                        if ( n == count - 1 ) break;

                        self = canv.GetPropValue( string.Join( ".", propname.Split( '.' ), 0, n + 1 ) );

                        props = props.Find( x => x.Name == path ).PropertyType.GetProperties().Where( p => p.CanRead ).ToList();
                    }

                    if ( !string.IsNullOrEmpty( path ) )
                    {
                        var pinfo = props.Find( x => x.Name == path );

                        try
                        {
                            var pvalue = canv.GetPropValue( propname );

                            result = SetPropertyValue( self, pinfo, pvalue, arg2.obj, context );
                        }
                        catch { }
                    }                
                }
                catch {}
            }

            return true;
        }


        public bool NumericEvaluation( Term value, TNumber[] args, ref TNumber result ) => true;

        #endregion

        #region Properties

        public Term Info { get; }

        #endregion
    
    }
}
