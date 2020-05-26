using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace TinyJson
{
    // Really simple JSON parser in ~300 lines
    // - Attempts to parse JSON files with minimal GC allocation
    // - Nice and simple "[1,2,3]".FromJson<List<int>>() API
    // - Classes and structs can be parsed too!
    //      class Foo { public int Value; }
    //      "{\"Value\":10}".FromJson<Foo>()
    // - Can parse JSON without type information into Dictionary<string,object> and List<object> e.g.
    //      "[1,2,3]".FromJson<object>().GetType() == typeof(List<object>)
    //      "{\"Value\":10}".FromJson<object>().GetType() == typeof(Dictionary<string,object>)
    // - No JIT Emit support to support AOT compilation on iOS
    // - Attempts are made to NOT throw an exception if the JSON is corrupted or invalid: returns null instead.
    // - Only public fields and property setters on classes/structs will be written to
    //
    // Limitations:
    // - No JIT Emit support to parse structures quickly
    // - Limited to parsing <2GB JSON files (due to int.MaxValue)
    // - Parsing of abstract classes or interfaces is NOT supported and will throw an exception.
    public static class JSONParser
    {
        [ThreadStatic]
        static Stack<List<string>> splitArrayPool;
        [ThreadStatic]
        static StringBuilder stringBuilder;
        [ThreadStatic]
        static Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache;
        [ThreadStatic]
        static Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache;

        public static T FromJson<T>( this string json )
        {
            // Initialize, if needed, the ThreadStatic variables.
            if ( propertyInfoCache == null ) propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

            if ( fieldInfoCache == null ) fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();

            if ( stringBuilder == null ) stringBuilder = new StringBuilder();

            if ( splitArrayPool == null ) splitArrayPool = new Stack<List<string>>();

            //Remove all whitespace not within strings to make parsing simpler
            stringBuilder.Length = 0;

            for ( int n = 0; n < json.Length; n++ )
            {
                char c = json[n];

                if ( c == '\'' )
                {
                    n = AppendUntilStringEnd( true, n, json ); 
                    continue;
                }

                if ( char.IsWhiteSpace(c) ) continue;

                stringBuilder.Append(c);
            }

            // Parse the thing!
            return ( T ) ParseValue( typeof( T ), stringBuilder.ToString() );
        }

        static int AppendUntilStringEnd( bool appendEscapeCharacter, int startIdx, string json )
        {
            stringBuilder.Append( json[ startIdx ] );

            for ( int n = startIdx + 1; n < json.Length; n++ )
            {
                if ( json[n] == '\\' )
                {
                    if ( appendEscapeCharacter ) stringBuilder.Append( json[n] );

                    stringBuilder.Append( json[ n + 1 ] );

                    // Skip next character as it is escaped.
                    n++;
                }
                else if ( json[n] == '\'' )
                {
                    stringBuilder.Append( json[n] );

                    return n;
                }
                else
                {
                    stringBuilder.Append( json[n] );
                }
            }

            return json.Length - 1;
        }

        // Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings.
        static List<string> Split( string json )
        {
            var splitArray = splitArrayPool.Count > 0 ? splitArrayPool.Pop() : new List<string>();

            splitArray.Clear();

            if ( json.Length == 2 ) return splitArray;

            int parseDepth = 0;

            stringBuilder.Length = 0;

            for ( int n = 1; n < json.Length - 1; n++ )
            {
                switch ( json[n] )
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '\'':
                        n = AppendUntilStringEnd( true, n, json );
                        continue;
                    case ',':
                    case ':':
                        if ( parseDepth == 0 )
                        {
                            splitArray.Add( stringBuilder.ToString() );
                            stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }

                stringBuilder.Append( json[n] );
            }

            splitArray.Add( stringBuilder.ToString() );

            return splitArray;
        }

        internal static object ParseValue( Type type, string json )
        {
            if ( type == typeof( string ) )
            {
                if ( json.Length <= 2 ) return string.Empty;

                var parseStringBuilder = new StringBuilder( json.Length );

                for ( int n = 1; n < json.Length - 1; ++n )
                {
                    if ( json[n] == '\\' && n + 1 < json.Length - 1 )
                    {
                        int j = "'\\nrtbf/".IndexOf( json[ n + 1 ] );

                        if ( j >= 0 )
                        {
                            parseStringBuilder.Append( "'\\\n\r\t\b\f/"[j] );
                            ++n;
                            continue;
                        }

                        if ( json[ n + 1 ] == 'u' && n + 5 < json.Length - 1 )
                        {
                            if ( uint.TryParse( json.Substring( n + 2, 4 ), System.Globalization.NumberStyles.AllowHexSpecifier, null, out var c ) )
                            {
                                parseStringBuilder.Append( ( char ) c );
                                n += 5;
                                continue;
                            }
                        }
                    }

                    parseStringBuilder.Append( json[n] );
                }

                return parseStringBuilder.ToString();
            }

            if ( type.IsPrimitive )
            {
                var result = Convert.ChangeType( json, type, System.Globalization.CultureInfo.InvariantCulture );

                return result;
            }

            if ( type == typeof( decimal ) )
            {
                decimal.TryParse( json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result );

                return result;
            }

            if ( json == "null" )
            {
                return null;
            }

            if ( type.IsEnum )
            {
                if ( json[0] == '\'' ) json = json.Substring( 1, json.Length - 2 );

                try
                {
                    return Enum.Parse( type, json, false );
                }
                catch
                {
                    return 0;
                }
            }

            if ( type.IsArray )
            {
                var arrayType = type.GetElementType();

                if ( json[0] != '[' || json[ json.Length - 1 ] != ']' ) return null;

                var elems = Split( json );

                var newArray = Array.CreateInstance( arrayType, elems.Count );

                for ( int n = 0; n < elems.Count; n++ ) newArray.SetValue( ParseValue( arrayType, elems[n] ), n );

                splitArrayPool.Push( elems );

                return newArray;
            }

            if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) )
            {
                var listType = type.GetGenericArguments()[0];

                if ( json[0] != '[' || json[ json.Length - 1 ] != ']' ) return null;

                var elems = Split( json );

                var list = ( IList ) type.GetConstructor( new[] { typeof( int ) } )?.Invoke( new object[] { elems.Count } );

                foreach ( var t in elems ) list?.Add( ParseValue( listType, t ) );

                splitArrayPool.Push( elems );

                return list;
            }

            if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
            {
                Type keyType, valueType;
                {
                    var args = type.GetGenericArguments();

                    keyType = args[0];
                    valueType = args[1];
                }

                // Refuse to parse dictionary keys that aren't of type string.
                if ( keyType != typeof( string ) ) return null;

                // Must be a valid dictionary element.
                if ( json[0] != '{' || json[ json.Length - 1 ] != '}' ) return null;

                // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON.
                var elems = Split( json );

                if ( elems.Count % 2 != 0 ) return null;

                var dictionary = ( IDictionary ) type.GetConstructor( new[] { typeof( int ) } )?.Invoke( new object[] { elems.Count / 2 } );

                for ( int n = 0; n < elems.Count; n += 2 )
                {
                    if ( elems[n].Length <= 2 ) continue;

                    var keyValue = elems[n].Substring( 1, elems[n].Length - 2 );

                    var val = ParseValue( valueType, elems[ n + 1 ] );

                    dictionary?.Add( keyValue, val );
                }

                return dictionary;
            }

            if ( type == typeof( object ) )
            {
                return ParseAnonymousValue( json );
            }

            if ( json[0] == '{' && json[ json.Length - 1 ] == '}' )
            {
                return ParseObject( type, json );
            }

            return null;
        }

        static object ParseAnonymousValue( string json )
        {
            if ( json.Length == 0 ) return null;

            if ( json[0] == '{' && json[ json.Length - 1 ] == '}' )
            {
                var elems = Split( json );

                if ( elems.Count % 2 != 0 ) return null;

                var dict = new Dictionary<string, object>( elems.Count / 2 );

                for ( int i = 0; i < elems.Count; i += 2 )
                    dict.Add( elems[i].Substring( 1, elems[i].Length - 2 ), ParseAnonymousValue( elems[ i + 1 ] ) );

                return dict;
            }

            if ( json[ 0 ] == '[' && json[ json.Length - 1 ] == ']' )
            {
                var items = Split( json );

                var finalList = new List<object>( items.Count );

                finalList.AddRange( items.ConvertAll( ParseAnonymousValue ) );

                return finalList;
            }

            if ( json[0] == '\'' && json[ json.Length - 1 ] == '\'' )
            {
                var str = json.Substring( 1, json.Length - 2 );

                return str.Replace( "\\", string.Empty );
            }

            if ( char.IsDigit( json[0] ) || json[0] == '-' )
            {
                if ( json.Contains( "." ) )
                {
                    double.TryParse( json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result );

                    return result;
                }
                else
                {
                    int.TryParse( json, out var result );

                    return result;
                }
            }

            if ( json == "true" ) return true;

            if ( json == "false" ) return false;

            // handles json == "null" as well as invalid JSON.
            return null;
        }

        static Dictionary<string, T> CreateMemberNameDictionary<T>( T[] members ) where T : MemberInfo
        {
            var nameToMember = new Dictionary<string, T>( StringComparer.OrdinalIgnoreCase );

            foreach ( T member in members )
            {
                //if ( member.IsDefined( typeof( IgnoreDataMemberAttribute ), true ) ) continue;

                var name = member.Name;

                //if ( member.IsDefined( typeof( DataMemberAttribute ), true ) )
                //{
                //    DataMemberAttribute dataMemberAttribute = ( DataMemberAttribute ) Attribute.GetCustomAttribute( member, typeof( DataMemberAttribute ), true );
                //
                //    if ( !string.IsNullOrEmpty( dataMemberAttribute.Name ) ) name = dataMemberAttribute.Name;
                //}

                nameToMember.Add( name, member );
            }

            return nameToMember;
        }

        static object ParseObject( Type type, string json )
        {
            var instance = FormatterServices.GetUninitializedObject( type );

            // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            var elems = Split( json );

            if ( elems.Count % 2 != 0 ) return instance;

            if ( !fieldInfoCache.TryGetValue( type, out var nameToField ) )
            {
                nameToField = CreateMemberNameDictionary( type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy ) );

                fieldInfoCache.Add( type, nameToField );
            }

            if ( !propertyInfoCache.TryGetValue( type, out var nameToProperty ) )
            {
                nameToProperty = CreateMemberNameDictionary( type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy ) );

                propertyInfoCache.Add( type, nameToProperty );
            }

            for ( int n = 0; n < elems.Count; n += 2 )
            {
                if ( elems[n].Length <= 2 ) continue;

                var key = elems[n].Substring( 1, elems[n].Length - 2 );

                var value = elems[ n + 1 ];

                if ( nameToField.TryGetValue( key, out var fieldInfo ) )
                {
                    fieldInfo.SetValue( instance, ParseValue( fieldInfo.FieldType, value ) );
                }
                else if ( nameToProperty.TryGetValue( key, out var propertyInfo ) )
                {
                    propertyInfo.SetValue( instance, ParseValue( propertyInfo.PropertyType, value ), null );
                }
            }

            return instance;
        }
    }
}
