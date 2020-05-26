#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using SMath.Manager;
using SMath.Math;
using SMath.Math.Numeric;

#endregion

namespace XYPlotPlugin
{
    public class Plugin: IPluginLowLevelEvaluationFast, IPluginMathNumericEvaluation
    {

        #region Private fields

        private static TermInfo[] _terms;
        private static List<IFunction> _functions;
        private readonly Func<Term, IFunction> _find = t => _functions.Find( f => f.Info.Equals(t) );

        #endregion

        #region Private methods

        private static void Log( string format, params object[] list )
        {
            if ( !GlobalConfig.Settings.Debug ) return;

            var text = string.Format( format, list );

            text = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {text}{Environment.NewLine}";

            try
            {
                File.AppendAllText( LogFile, text, Encoding.UTF8 );
            }
            catch { }
        }

        #endregion

        #region Internal methods

        internal static void LogInfo( string format, params object[] list ) => Log( $"[INFO ] {format}", list );

        internal static void LogError( string format, params object[] list ) => Log( $"[ERROR] {format}", list );

        #endregion

        #region Public methods

        public TermInfo[] GetTermsHandled( SessionProfile sessionProfile ) => _terms ?? ( _terms = _functions?.ConvertAll( f => f.GetTermInfo( sessionProfile.CurrentLanguage.Abbr ) ).ToArray() );

        public void Initialize()
        {
            var is64Bit = Marshal.SizeOf( typeof( IntPtr ) ) == 8;

            var version = ExecAssembly.GetName().Version;

            var bdate = new DateTime( 2000, 1, 1 ).AddDays( version.Build ).AddSeconds( 2 * version.Revision );

            var lines = new[]
            {
                $"OS: {Environment.OSVersion}", $".Net: {Environment.Version}",
                $"{(is64Bit ? "64" : "32")}-bit", $"{AssemblyTitle}, version {version}, {bdate:dd-MMM-yyyy HH:mm:ss}"
            };

            var method = "[Plugin.Initialize()]";

            foreach ( var line in lines ) LogInfo( $"{method} {line}" );

            // Adding functions happen automatically at runtime.
            _functions = new List<IFunction>();

            var types = Array.FindAll( ExecAssembly.GetTypes(), t => t.IsClass && typeof( IFunction ).IsAssignableFrom(t) );

            foreach ( var type in types )
            {
                var arguments = type.GetField( "Arguments", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public );

                try
                {
                    var args = ( int[] ) ( arguments?.GetValue( null ) ?? new int[0] );

                    var items = Array.ConvertAll( args, arg => ( IFunction ) Activator.CreateInstance( type, arg ) );

                    _functions.AddRange( items );

                    foreach ( var item in Array.ConvertAll( items, f => f.GetTermInfo( "ENG" ) ) ) LogInfo( $"{method} {item.Text}({item.ArgsCount}) - {item.Description}" );
                }
                catch ( Exception ex )
                {
                    LogError( $"{method} {ex.Message}" );
                }
            }

            LogInfo( $"{method} Successfully. {_functions.Count} function(s) loaded." );
        }

        public bool TryEvaluateExpression( Entry value, Store context, out Entry result ) =>
            ( result = null ) != null || ( _find( value.ToTerm() )?.TryEvaluateExpression( value, context, ref result ) ?? false );

        public bool NumericEvaluation( Term value, TNumber[] args, ref TNumber result ) => _find( value )?.NumericEvaluation( value, args, ref result ) ?? false;

        public void Dispose()
        {
            try
            {
                if ( File.Exists( LogFile ) ) File.Delete( LogFile );
            }
            catch { }
        }

        #endregion

        #region Properties

        internal static Assembly ExecAssembly => Assembly.GetExecutingAssembly();

        internal static string AssemblyDirectory => Path.GetDirectoryName( new Uri( ExecAssembly.CodeBase ).LocalPath );

        internal static string AssemblyFileName => Path.GetFileName( new Uri( ExecAssembly.CodeBase ).LocalPath );

        internal static string LogFile => Path.Combine( AssemblyDirectory, Path.GetFileNameWithoutExtension( AssemblyFileName ) + ".log" );

        internal static string AssemblyTitle => ExecAssembly.GetCustomAttributes( false ).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title 
            ?? Path.GetFileNameWithoutExtension( AssemblyFileName );

        #endregion

    }
}
