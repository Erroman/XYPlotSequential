using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XYPlotPlugin
{
    public static class Extensions
    {
        public static object GetPropItemValue( this object obj, string propName )
        {
            if ( new Regex( @"\w+\[\d+\]" ).IsMatch( propName ) )
            {
                var index = new object[] { int.Parse( Regex.Match( propName, @"(?<=\[)\d+" ).Value ) };

                var itemsInfo = obj?.GetType().GetProperty( propName.Split( '[' )[0] );

                if ( itemsInfo == null ) return null;

                var collection = itemsInfo.GetValue( obj, null );

                // note that there's no checking here that the object really
                // is a collection and thus really has the attribute
                var indexerName = collection.GetType().GetCustomAttributes( false ).OfType<DefaultMemberAttribute>().First().MemberName;

                var prop = collection.GetType().GetProperty( indexerName );

                obj = prop?.GetValue( collection, index );
            }
            else
            {
                var prop = obj?.GetType().GetProperty( propName );

                obj = prop?.GetValue( obj, null );
            }

            return obj;
        }

        public static object GetPropValue( this object obj, string propName )
        {
            return propName.Contains( "." ) ? propName.Split('.').Aggregate( obj, GetPropItemValue ) : GetPropItemValue( obj, propName );
        }
    }
}