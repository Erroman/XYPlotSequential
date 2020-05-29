#region Using

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

using SMath.Manager;
using SMath.Controls;
using SMath.Math;
using SMath.Math.Numeric;

using JXCharts;

#endregion

namespace XYPlotPluginSeq 
{
    public class XYPlotSeq : RegionHolder<Chart2D>
    {

        #region Private fields

        private int _tracesCount;
        private Color[] _defLineColors;

        private byte[] _edgeTable = { 0, 9, 3, 10, 6, 15, 5, 12, 12, 5, 15, 6, 10, 3, 9, 0 };

        private int[,] _triTable =
        {
            { -1,  0,  0,  1,  1,  0,  0,  2,  2,  0,  0,  1,  1,  0,  0,  0 },
            { -1,  3,  1,  3,  2,  3,  2,  3,  3,  2,  1,  2,  3,  1,  3, -1 },
            { -1, -1, -1, -1, -1,  1, -1, -1, -1, -1,  2, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1,  2, -1, -1, -1, -1,  3, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
        };

        #endregion

        #region Public fields

        public static List<object> Instances = new List<object>();

        #endregion

        #region Constructors

        public XYPlotSeq(SessionProfile sessionProfile) : base(sessionProfile)
        {
            Initialize();
            Instances.Add(canv);
        }


        public XYPlotSeq(XYPlotSeq region) : base(region)
        {
            Initialize();

            DescriptionLocation = region.DescriptionLocation;
        }

        #endregion

        #region Private methods

        private void Initialize()
        {
            BackColor = Color.Transparent;
            Border = false;
            DescriptionLocation = ElementPosition.Top;

            math.BackColor = Color.Transparent;

            _tracesCount = 0;

            _defLineColors = new Color[6];

            _defLineColors[0] = Color.Blue;
            _defLineColors[1] = Color.Red;
            _defLineColors[2] = Color.Green;
            _defLineColors[3] = Color.Fuchsia;
            _defLineColors[4] = Color.DarkOrange;
            _defLineColors[5] = Color.SaddleBrown;
        }

        class Branch
        {
            Square startSquare;
            public Branch(Square startSquare) 
            {
                this.startSquare = startSquare;
            }
        };
        class Square 
        {
            int n; // №  квадрата в ряду по оси Х
            int m; //  №  квадрата в столбце вдоль оси Y

        };
        // (x,y) coordinates.
        private List<PointD> GetPoints( double dx, double dy, double xmin, double ymin, int n, int m )
        {
            double[,] c = { { 0, 1f }, { 1f, 1f }, { 1f, 0 }, { 0, 0 }, { .5f, .5f } };

            var p = new PointD( dx * n + xmin, dy * m + ymin );

            return Enumerable.Range( 0, 5 ).Select( k => new PointD( p.X + c[ k, 0 ] * dx, p.Y + c[ k, 1 ] * dy ) ).ToList();
        }


        // Получаем значения z в вершинах квадрата.
        private double[] GetValues( int n, int m, double[,] zvalues ) 
        {
            var vals = new double[5];

            vals[0] = zvalues[ n + 0, m + 1 ];
            vals[1] = zvalues[ n + 1, m + 1 ];
            vals[2] = zvalues[ n + 1, m + 0 ];
            vals[3] = zvalues[ n + 0, m + 0 ];

            // Значение в центре квадрата.
            vals[4] = ( vals[0] + vals[1] + vals[2] + vals[3] ) / 4;

            return vals;
        }


        // Формируем индекс, перебирая варианты расположения вершин относительно
        // уровня isolevel.
        private byte GetIndex( double[] vals, double isolevel ) 
        {
            byte indx = 0, N = 5;

            for ( byte n = 0; n < N - 1; n++ ) 
            {
                indx |= ( byte ) ( vals[n] < isolevel ? 1 << n : 0 );
            }

            if ( indx == 10 ) 
            {
                if ( vals[ N - 1 ] < isolevel ) indx = 5;
            }
            else if ( indx == 5 )  
            {
                if ( vals[ N - 1 ] < isolevel ) indx = 10;
            }

            return indx;
        }


        // Линейная интерполяция.
        private PointD VertexInterp( double level, PointD p1, PointD p2, double v1, double v2 ) 
        {
            var s = ( level - v1 ) / ( v2 - v1 );

            return new PointD( p1.X + s * ( p2.X - p1.X ), p1.Y + s * ( p2.Y - p1.Y ) );
        }


        // Формируем список коодринат вершин квадрата.
        private List<PointD> GetVertList( double level, List<PointD> pp, double[] vals ) 
        {
            var vlist = new List<PointD>(4)
            {
                VertexInterp( level, pp[0], pp[1], vals[0], vals[1] ),
                VertexInterp( level, pp[1], pp[2], vals[1], vals[2] ),
                VertexInterp( level, pp[2], pp[3], vals[2], vals[3] ),
                VertexInterp( level, pp[3], pp[0], vals[3], vals[0] )
            };

            return vlist;
        }


        private List<PointD> ImplicitPlot2d( double dx, double dy, double xmin, double ymin, int nx, int ny, double[,] zvalues, double isolevel = 0 ) 
        {
            var pp = new List<PointD>();

            for ( var n = 0; n < nx; n++ ) 
            {
                for ( var m = 0; m < ny; m++ ) 
                {
                    // Значение z в вершинах квадрата.
                    var vals = GetValues( n, m, zvalues );

                    // Классифицируем тип пересечения.
                    var indx = GetIndex( vals, isolevel );

                    // Пропускаем, если нет пересечения.
                    if ( _edgeTable[ indx ] == 0 ) continue;

                    // Текущий квадрат.
                    var xy = GetPoints( dx, dy, xmin, ymin, n, m );

                    // Получаем список точек для найденного квадрата.
                    var vlist = GetVertList( isolevel, xy, vals );

                    // Заполняем список точек кривой отрезками на основе 
                    // найденной конфигурации пересечения.
                    byte i = 0;
        //private int[,] _triTable =
        //{
        //    { -1,  0,  0,  1,  1,  0,  0,  2,  2,  0,  0,  1,  1,  0,  0,  0 },
        //    { -1,  3,  1,  3,  2,  3,  2,  3,  3,  2,  1,  2,  3,  1,  3, -1 },
        //    { -1, -1, -1, -1, -1,  1, -1, -1, -1, -1,  2, -1, -1, -1, -1, -1 },
        //    { -1, -1, -1, -1, -1,  2, -1, -1, -1, -1,  3, -1, -1, -1, -1, -1 },
        //    { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
        //};

                    while ( _triTable[ i, indx ] != -1 ) 
                    {
                        pp.Add( vlist[ _triTable[ i, indx ] ] );
                        pp.Add( vlist[ _triTable[ i + 1, indx ] ] );

                        i += 2;
                    }
                }
            }

            return pp;
        }
        private List<PointD> ImplicitPlot2dSeq(double dx, double dy, double xmin, double ymin, int nx, int ny, double[,] zvalues, double isolevel = 0)
        {
            var pp = new List<PointD>();

            for (var n = 0; n < nx; n++)
            {
                for (var m = 0; m < ny; m++)
                {
                    // Значение z в вершинах квадрата.
                    var vals = GetValues(n, m, zvalues);

                    // Классифицируем тип пересечения.
                    var indx = GetIndex(vals, isolevel);

                    // Пропускаем, если нет пересечения.
                    if (_edgeTable[indx] == 0) continue;
                    //Пересечение найдено! Включается алгоритм последо вательного поиска
                    // Текущий квадрат.
                    var xy = GetPoints(dx, dy, xmin, ymin, n, m);

                    // Получаем список точек для найденного квадрата.
                    var vlist = GetVertList(isolevel, xy, vals);

                    // Заполняем список точек кривой отрезками на основе 
                    // найденной конфигурации пересечения.
                    byte i = 0;
                    //private int[,] _triTable =
                    //{
                    //    { -1,  0,  0,  1,  1,  0,  0,  2,  2,  0,  0,  1,  1,  0,  0,  0 },
                    //    { -1,  3,  1,  3,  2,  3,  2,  3,  3,  2,  1,  2,  3,  1,  3, -1 },
                    //    { -1, -1, -1, -1, -1,  1, -1, -1, -1, -1,  2, -1, -1, -1, -1, -1 },
                    //    { -1, -1, -1, -1, -1,  2, -1, -1, -1, -1,  3, -1, -1, -1, -1, -1 },
                    //    { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
                    //};

                    while (_triTable[i, indx] != -1)
                    {
                        pp.Add(vlist[_triTable[i, indx]]);
                        pp.Add(vlist[_triTable[i + 1, indx]]);

                        i += 2;
                    }
                }
            }

            return pp;
        }

        private void AddLinePlot<T>( List<T> list )
        {
            _tracesCount++;

            var types = new[]
            {
                new { Type = typeof( PointD ), SeriesType = typeof( PointSeries ) },
                new { Type = typeof( PointD[] ), SeriesType = typeof( PolylineSeries ) },
                new { Type = typeof( Line2D ), SeriesType = typeof( LineSeries ) },
                new { Type = typeof( TextLabel ), SeriesType = typeof( LabelSeries ) },
                new { Type = typeof( Shape ), SeriesType = typeof( ShapeSeries ) },
            };

            var pair = types.FirstOrDefault( t => t.Type == typeof(T) );

            if ( pair == null ) return;

            var series = ( DataSeries<T> ) Activator.CreateInstance( pair.SeriesType );

            series.LineStyle.LineColor = _defLineColors[ ( _tracesCount - 1 ) % 6 ];

            series.AddData( list );

            if ( _tracesCount > canv.Series.Count )
            {
                canv.Series.Add( series );
                return;
            }

            var item = canv.Series[ _tracesCount - 1 ];

            series.LineStyle = item.LineStyle;
            series.SymbolStyle = item.SymbolStyle;
            series.IsY2Data = item.IsY2Data;
            series.SeriesName = item.SeriesName;

            canv.Series[ _tracesCount - 1 ] = series;

            if ( typeof(T) == typeof( Line2D ) )
            {
                series.LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Lines;
            }
            else if ( typeof(T) == typeof( TextLabel ) )
            {
                series.LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Labels;
            }
            else if ( typeof(T) == typeof( Shape ) )
            {
                series.LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Shapes;
            }
            else
            {
                switch ( series.LineStyle.PlotMethod )
                {
                    case LineStyle.PlotLinesMethodEnum.Lines:
                    case LineStyle.PlotLinesMethodEnum.Splines: break;
                    default:
                        series.LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Lines;
                        break;
                }
            }
        }


        private Shape BuildShape( TMatrix mat )
        {
            var rows = mat.unit.GetLength(0);

            if ( rows < 2 ) throw new Exception();

            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );

            // Shape type.
            var d = ( TDouble ) mat.unit[ 0, 0 ].obj;

            if ( !d.isText ) throw new Exception();

            var text = d.Text.Trim( '"' );

            if ( string.IsNullOrEmpty( text ) ) throw new Exception();

            var shapes = new[]
            {
                new { Name = "line", Type = EnShapeType.Line },
                new { Name = "rect", Type = EnShapeType.Rectangle },
                new { Name = "roundrect", Type = EnShapeType.RoundedRectangle },
                new { Name = "circle", Type = EnShapeType.Circle },
                new { Name = "ellipse", Type = EnShapeType.Ellipse },
                new { Name = "arc", Type = EnShapeType.Arc },
                new { Name = "polygon", Type = EnShapeType.Polygon },
                new { Name = "pie", Type = EnShapeType.Pie },
                new { Name = "polyline", Type = EnShapeType.Polyline },
                new { Name = "spline", Type = EnShapeType.Spline },
                new { Name = "bezier", Type = EnShapeType.Bezier },
            };

            var item = Array.Find( shapes, s => s.Name.Equals( text ) );

            var type = item?.Type ?? throw new Exception();

            // Shape data.
            var data = ( TMatrix ) mat.unit[ 1, 0 ].obj;

            var lineColor = Color.Empty;
            var fillColor = Color.Empty;

            var lineStyle = DashStyle.Solid;
            var lineWidth = 1f;

            var isLineWidthManual = false;
            var isLineColorManual = false;
            var isFillColorManual = false;

            if ( rows > 2 )
            {
                // Line color.
                d = ( TDouble ) mat.unit[ 2, 0 ].obj;

                try
                {
                    lineColor = d.isText ? ( Color ) colorConverter.ConvertFromString( d.Text.Trim( '"' ) ) : Color.FromArgb( ( int ) d.ToDouble() );
                }
                catch
                {
                    lineColor = Color.Empty;
                }

                isLineColorManual = true;

                if ( rows > 3 )
                {
                    // Line style.
                    d = ( TDouble ) mat.unit[ 3, 0 ].obj;

                    text = d.Text.Trim( '"' );

                    var styles = new[]
                    {
                        new { Name = "solid", Value = DashStyle.Solid },
                        new { Name = "dash", Value = DashStyle.Dash },
                        new { Name = "dot", Value = DashStyle.Dot },
                        new { Name = "dashdot", Value = DashStyle.DashDot },
                        new { Name = "dashdotdot", Value = DashStyle.DashDotDot },
                    };

                    var style = Array.Find( styles, s => s.Name.Equals( text ) );

                    lineStyle = style?.Value ?? DashStyle.Solid;

                    if ( rows > 4 )
                    {
                        // Line width.
                        lineWidth = ( float ) mat.unit[ 4, 0 ].obj.ToDouble();

                        isLineWidthManual = true;

                        if ( rows > 5 )
                        {
                            // Fill color.
                            d = ( TDouble ) mat.unit[ 5, 0 ].obj;

                            try
                            {
                                fillColor = d.isText ? ( Color ) colorConverter.ConvertFromString( d.Text.Trim( '"' ) ) : Color.FromArgb( ( int ) d.ToDouble() );
                            }
                            catch
                            {
                                fillColor = Color.Empty;
                            }

                            isFillColorManual = true;
                        }
                    }
                }
            }

            var shape = new Shape( type, data )
            {
                LineStyle = lineStyle,
                LineWidth = lineWidth,
                LineColor = lineColor,
                FillColor = fillColor,
                IsLineColorManual = isLineColorManual,
                IsFillColorManual = isFillColorManual,
                IsLineWidthManual = isLineWidthManual
            };

            return shape;
        }


        private void AddShapes( TMatrix mat )
        {
            var rows = mat.unit.GetLength(0);            

            var shapes = new List<Shape>();

            for ( var n = 0; n < rows; n++ )
            {
                var obj = mat.unit[ n, 0 ].obj;

                if ( !( obj is TMatrix ) ) continue;

                var shapemat = ( TMatrix ) obj;

                var r = shapemat.unit.GetLength(0);

                for ( var k = 0; k < r; k++ )
                {
                    var mobj = shapemat.unit[ k, 0 ].obj;

                    if ( !( mobj is TMatrix ) ) continue;

                    try
                    {
                        var shape = BuildShape( ( TMatrix ) mobj );

                        shapes.Add( shape );
                    }
                    catch {}
                }
            }

            AddLinePlot( shapes );
        }


        private void AddLabels( TMatrix mat )
        {
            var rows = mat.unit.GetLength(0);
            var cols = mat.unit.GetLength(1);

            var colorConverter = TypeDescriptor.GetConverter( typeof( Color ) );

            var labels = new List<TextLabel>();

            for ( var n = 0; n < rows; n++ )
            {
                var isSizeManual = false;
                var isColorManual = false;
                var text = string.Empty;
                var sz = 10f;
                var color = Color.Black;
                var loc = new PointD( 0, 0 );

                try
                {
                    loc.X = mat.unit[ n, 0 ].obj.ToDouble();
                    loc.Y = mat.unit[ n, 1 ].obj.ToDouble();

                    var d = ( TDouble ) mat.unit[ n, 2 ].obj;

                    text = d.isText ? ( ( TDouble ) mat.unit[ n, 2 ].obj ).Text.Trim( '"' ) : "";

                    if ( string.IsNullOrEmpty( text ) ) continue;

                    if ( cols > 3 )
                    {
                        sz = ( float ) mat.unit[ n, 3 ].obj.ToDouble();

                        isSizeManual = true;

                        if ( cols > 4 )
                        {
                            d = ( TDouble ) mat.unit[ n, 4 ].obj;

                            color = d.isText ? ( Color ) colorConverter.ConvertFromString( d.Text.Trim( '"' ) ) : Color.FromArgb( ( int ) d.ToDouble() );

                            isColorManual = true;
                        }
                    }
                }
                catch
                {
                    continue;
                }

                var label = new TextLabel( text, loc )
                {
                    Size = sz,
                    Color = color,
                    IsSizeManual = isSizeManual,
                    IsColorManual = isColorManual                        
                };

                labels.Add( label );
            }

            AddLinePlot( labels );
        }


        private void AddMatrix( TMatrix mat )
        {
            var rows = mat.unit.GetLength(0);

            var xy = new List<PointD>();

            for ( var n = 0; n < rows; n++ )
            {
                if ( ( mat.unit[ n, 0 ].obj is TComplex ) || ( mat.unit[ n, 1 ].obj is TComplex ) )
                {
                    ShowError( new Exception( "Complex numbers not allowed." ), Term.Empty );
                }

                xy.Add( new PointD( mat.unit[ n, 0 ].obj.ToDouble(), mat.unit[ n, 1 ].obj.ToDouble() ) );
            }

            var list = new List<PointD[]> { xy.ToArray() };

            AddLinePlot( list );
        }


        private void AddMatrixItems( TMatrix mat )
        {
            var rows = mat.unit.GetLength(0);

            var list = new List<PointD[]>();

            for ( var n = 0; n < rows; n++ )
            {
                var obj = mat.unit[ n, 0 ].obj;

                if ( !( obj is TMatrix ) ) continue;

                var m = ( TMatrix ) obj;

                var r = m.unit.GetLength(0);
                var c = m.unit.GetLength(1);

                if ( c == 2 )
                {
                    var xy = new List<PointD>();

                    for ( var k = 0; k < r; k++ )
                    {
                        xy.Add( new PointD( m.unit[ k, 0 ].obj.ToDouble(), m.unit[ k, 1 ].obj.ToDouble() ) );
                    }

                    list.Add( xy.ToArray() );
                }
            }

            AddLinePlot( list );
        }


        private void AddMatrixData( TMatrix mat )
        {
            var cols = mat.unit.GetLength(1);

            if ( cols == 1 ) 
            {
                var obj =  mat.unit[ 0, 0 ].obj;

                if ( obj is TMatrix )
                {
                    var m = ( TMatrix ) obj;

                    var c = m.unit.GetLength(1);

                    if ( c == 1 )
                    {
                        AddShapes( mat );
                    }
                    else if ( c == 2 )
                    {
                        AddMatrixItems( mat );
                    }
                }
            }

            else if ( cols == 2 )
            {
                AddMatrix( mat );
            } 

            else if ( cols > 2 && cols < 6 )
            {
                AddLabels( mat );
            }

            else 
            {
                ShowError( new Exception( "2-5 cols allowed." ), Term.Empty );
            }
        }


        private void AddImplicitFunctionData( Entry entry, Entry arg1, Entry arg2, Store store )
        {
            var N = canv.Points - 1;
            var M = canv.Points - 1;

            var dx = ( canv.XAxis.Max - canv.XAxis.Min ) / N;
            var dy = ( canv.YAxis.Max - canv.YAxis.Min ) / M;

            var xmin = canv.XAxis.Min;
            var ymin = canv.YAxis.Min;

            var zvalues = new double [ N + 1, M + 1 ];

            var eq = entry.ToTermsList();

            var eqlist = new List<Term>();

            for ( var n = 0; n <= N; n++ ) 
            {
                double x = n * dx + xmin;

                var xterms = new TDouble(x).ToTerms();

                for ( var m = 0; m <= M; m++ ) 
                {
                    eqlist.Clear();
                    
                    double y = m * dy + ymin;
                    
                    var yterms = new TDouble(y).ToTerms();

                    foreach ( var el in eq )
                    {
                        if ( el.Text.Equals( arg1.Text ) )
                        {
                            eqlist.AddRange( xterms );
                        }
                        else if ( el.Text.Equals( arg2.Text ) )
                        {
                            eqlist.AddRange( yterms );
                        }
                        else
                        {
                            eqlist.Add( el );
                        }
                    }                    
                    
                    var tmp = Entry.Create( eqlist );

                    var res = Computation.NumericCalculation( tmp, store );

                    if ( res.obj is TComplex ) 
                    {
                        ShowError( new Exception( "The type of result is complex." ), entry.ToTerm() );
                    }
                    else if ( res.obj is TMatrix )
                    {
                        ShowError( new Exception( "The type of result is matrix." ), entry.ToTerm() );
                    }
                    else if ( res.obj is TSystem )
                    {
                        ShowError( new Exception( "The type of result is system." ), entry.ToTerm() );                
                    }
                    else
                    {
                        zvalues[ n, m ] = res.obj.ToDouble();
                    }
                }
            }

            //var points = ImplicitPlot2d( dx, dy, xmin, ymin, N, M, zvalues );
            var points = ImplicitPlot2dSeq(dx, dy, xmin, ymin, N, M, zvalues);

            var list = new List<Line2D>( points.Count / 2 );

            for ( var n = 0; n < points.Count / 2; n++ )
            {
                var line = new Line2D { P1 = points[ 2 * n ], P2 = points[ 2 * n + 1 ] };

                list.Add( line );
            }

            AddLinePlot( list );
        }


        private void AddConstExprData( Entry entry, Store store )
        {
            var N = canv.Points;

            var xmin = canv.XAxis.Min;
            var xmax = canv.XAxis.Max;

            var xscale = ( xmax - xmin ) / ( N - 1 );

            var list = new List<PointD>(N);

            try
            {
                var tmp = Computation.Preprocessing( entry, store );

                var res = Computation.NumericCalculation( tmp, store );

                if ( res.obj is TComplex )
                {
                    ShowError( new Exception( "The type of result is complex." ), entry.ToTerm() );
                }
                else if ( res.obj is TMatrix )
                {
                    ShowError( new Exception( "The type of result is matrix." ), entry.ToTerm() );
                }
                else if ( res.obj is TSystem )
                {
                    ShowError( new Exception( "The type of result is system." ), entry.ToTerm() );
                }
                else if ( res.obj is TDouble )
                {
                    var dbl = ( TDouble ) res.obj;

                    if ( !dbl.isText )
                    {
                        for ( var n = 0; n < N; n++ )
                        {
                            var x = n * xscale + xmin;

                            list.Add( new PointD( x, dbl.ToDouble() ) );
                        }
                    }
                }
                else
                {
                    for ( var n = 0; n < N; n++ )
                    {
                        var x = n * xscale + xmin;

                        list.Add( new PointD( x, res.obj.ToDouble() ) );
                    }
                }
            }
            catch {}

            AddLinePlot( list );
        }


        private void AddFunctionData( Entry entry, Entry arg, Store store ) 
        {
            var N = canv.Points;

            var xmin = canv.XAxis.Min;
            var xmax = canv.XAxis.Max;

            var xscale = ( xmax - xmin ) / ( N - 1 );

            var list = new List<PointD>(N);

            var eqlist = new List<Term>();

            var eq = entry.ToTermsList();

            for ( var n = 0; n < N; n++ )
            {
                var x = n * xscale + xmin;

                eqlist.Clear();

                var value = new TDouble(x).ToTerms();

                foreach ( var el in eq )
                {
                    if ( el.Text.Equals( arg.Text ) )
                    {                            
                        eqlist.AddRange( value );
                    }
                    else
                    {
                        eqlist.Add( el );
                    }
                }

                try
                {
                    var tmp = Entry.Create( eqlist );

                    var res = Computation.NumericCalculation( tmp, store );

                    if ( res.obj is TComplex )
                    {
                        ShowError( new Exception( "The type of result is complex." ), entry.ToTerm() );
                    }
                    else if ( res.obj is TMatrix )
                    {
                        ShowError( new Exception( "The type of result is matrix." ), entry.ToTerm() );
                    }
                    else if ( res.obj is TSystem )
                    {
                        ShowError( new Exception( "The type of result is system." ), entry.ToTerm() );
                    }
                    else if ( res.obj is TDouble )
                    {
                        var dbl = ( TDouble ) res.obj;

                        list.Add( new PointD( x, dbl.isText ? double.NaN : dbl.ToDouble() ) );
                    }
                    else
                    {
                        list.Add( new PointD( x, res.obj.ToDouble() ) );
                    }
                }
                catch
                {
                    list.Add( new PointD( x, double.NaN ) );
                }
            }

            AddLinePlot( list );
        }


        private void AddSystemData( Entry entry, Store store ) 
        {
            for ( var n = 0; n < entry.ArgsCount - 2; n++ ) 
            {
                ParseInput( entry.Items[n], store );
            }        
        }


        private void ParseExpression( Entry entry, Store store )
        {
            var undefvars = new HashSet<string>( entry.ToTermsList().Where( t => t.Type == TermType.Operand && !store.IsDefined( t, false ) ).Select( t => t.Text ) ).ToList();

            if ( undefvars.Count == 0 )
            {
                var result = Computation.NumericCalculation( entry, store ).obj;

                if ( result is TSystem )
                {
                    AddSystemData( Entry.Create( result.ToTerms() ), store );
                }

                else if ( result is TMatrix )
                {
                    AddMatrixData( result as TMatrix );
                }

                else
                {
                    AddConstExprData( Entry.Create( result.ToTerms() ), store );
                }
            }

            else if ( undefvars.Count == 1 )
            {
                AddFunctionData( entry, new Entry( undefvars[0] ), store );
            }

            else if ( undefvars.Count == 2 )
            {
                if ( undefvars.Any( v => v == "t" ) )
                {
                    var arg1 = new Entry( "t" );
                    var arg2 = new Entry( undefvars.Find( v => v != "t" ) );

                    AddImplicitFunctionData( entry, arg1, arg2, store );
                }
                else if ( undefvars.Any( v => v == "x" ) )
                {
                    var arg1 = new Entry( "x" );
                    var arg2 = new Entry( undefvars.Find( v => v != "x" ) );

                     AddImplicitFunctionData( entry, arg1, arg2, store );
                }
                else
                {
                    var msg = string.Format( "Pair ({0},{1}) must have an explicit form.", undefvars[0], undefvars[1] );

                    ShowError( new Exception( msg ), Term.Empty );
                }
            }
            else
            {
                ShowError( new Exception( "Too many unknowns." ), Term.Empty );
            }
        }


        private void ParseOperator( Entry entry, Store store )
        {
            if ( entry.Text == Operators.Definition )
            {
                var lhs = entry.Items[0];

                if ( lhs.Type == TermType.Function )
                {
                    Computation.Preprocessing( entry, store );

                    ParseFunction( lhs, store );
                }

                else if ( lhs.Type == TermType.Operand )
                {
                    Computation.Preprocessing( entry, store );

                    var defs = Enumerable.Range( 0, store.Count ).Select( n => store[n] ).ToList();

                    var def = defs.Find( eq => eq.IsLocal && !eq.IsFunction && eq.Name.Equals( lhs.Text ) );

                    if ( def != null )
                    {
                        ParseInput( def.Result, store );
                    }
                }
            }
            else
            {
                ParseExpression( entry, store );
            }            
        }


        private void ParseOperand( Entry entry, Store store )
        {
            var defs = Enumerable.Range( 0, store.Count ).Select( n => store[n] ).ToList();

            var def = defs.Find( eq => eq.Name.Equals( entry.Text ) && !eq.IsFunction );

            // Operand, defined in store.
            if ( def != null )
            {
                ParseInput( def.Result, store );

                return;
            }

            def = defs.Find( eq => eq.Name.Equals( entry.Text ) && eq.IsFunction );

            // Function, defined in store.
            if ( def != null )
            {
                var args = def.Variables.Select( item => Entry.Create( new[] { item } ) ).ToArray();

                ParseFunction( new Entry( def.Name, TermType.Function, args ), store );                    
            }
            else
            {
                entry = Computation.Preprocessing( entry, store );

                ParseExpression( entry, store );
            }
        }


        private void ParseFunction( Entry entry, Store store )
        {
            var internals = typeof( SMath.Manager.Functions ).GetProperties( BindingFlags.Static | BindingFlags.Public ).Select( p =>
            {
                object obj = string.Empty;

                obj = p.GetValue( obj, null );

                return ( string ) obj;
            }).ToList();            

            if ( internals.Contains( entry.Text ) )
            {
                if ( entry.Text.Equals( SMath.Manager.Functions.Line ) )
                {
                    var eq = Computation.Preprocessing( entry, store );

                    ParseInput( eq, store );
                }

                else if ( entry.Text.Equals( SMath.Manager.Functions.Sys ) )
                {
                    AddSystemData( entry, store );
                }

                else 
                {
                    ParseExpression( entry, store );
                }
            }

            else if ( store.IsDefined( entry, false ) )
            {
                var defs = Enumerable.Range( 0, store.Count ).Select( n => store[n] ).ToList();

                var def = defs.Find( eq => eq.IsFunction && eq.Name.Equals( entry.Text ) && ( eq.Variables.Length == entry.ArgsCount ) );

                // Special case: result = sys().
                if ( def != null )
                {
                    if ( def.Result.Text == SMath.Manager.Functions.Sys )
                    {
                        AddSystemData( entry, store );

                        return;
                    }

                    var undefvars = new HashSet<string>( entry.Items.Where( t => t.Type == TermType.Operand && !store.IsDefined( t, false ) ).Select( t => t.Text ) )
                        .Select( x => new Entry(x) ).ToList();

                    switch ( undefvars.Count )
                    {
                        case 0:
                            ParseExpression( entry, store );
                            break;

                        case 1:
                            AddFunctionData( entry, undefvars[0], store );
                            break;

                        case 2:
                            var arg1 = undefvars[0];
                            var arg2 = undefvars[1];

                            AddImplicitFunctionData( entry, arg1, arg2, store );
                            break;

                        default:
                            ShowError( new Exception( "Too many unknowns: " + entry ), Term.Empty );
                            break;
                    }
                }
                else if ( entry.Text.Equals( "at" ) )
                {
                    entry = Computation.Preprocessing( entry, store );

                    ParseExpression( entry, store );
                }
                else
                {
                    entry = Computation.SymbolicCalculation( entry, store ).ToEntry();

                    ParseExpression( entry, store );
                }
            }

            else
            {
                ShowError( new Exception( "Function is undefined." ), entry.ToTerm() );
            }            
        }


        private void ParseInput( Entry entry, Store store ) 
        {
            switch ( entry.Type )
            {
                // Function.
                case TermType.Function: ParseFunction( entry, store ); break;

                // Identifier.
                case TermType.Operand: ParseOperand( entry, store ); break;

                // Operator.
                case TermType.Operator: ParseOperator( entry, store ); break;
            }
        }


        private void SetPropertiesFromSheet( Store store ) 
        {
            var list = new[]
            {
                new { Key = "XYPlot{0}XLimMin", Value = "XAxis.Min" },
                new { Key = "XYPlot{0}XLimMax", Value = "XAxis.Max" },
                new { Key = "XYPlot{0}XTick", Value = "XAxis.Tick" },

                new { Key = "XYPlot{0}YLimMin", Value = "YAxis.Min" },
                new { Key = "XYPlot{0}YLimMax", Value = "YAxis.Max" },
                new { Key = "XYPlot{0}YTick", Value = "YAxis.Tick" },

                new { Key = "XYPlot{0}Points", Value = "Points" },
            };

            var cultureInfo = new CultureInfo( "" )
            {
                NumberFormat = { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() }
            };

            // TODO: Too complex.
            foreach ( var pair in list )
            {
                try
                {
                    var entry = Computation.Preprocessing( new Entry( string.Format( pair.Key, GlobalProfile.DecimalSymbolStandard ) ), store );

                    var mat = ( TMatrix ) Computation.NumericCalculation( entry, store ).obj;

                    var count = pair.Value.Split( '.' ).Length;

                    var props = canv.GetType().GetProperties().Where( p => p.CanRead && p.CanWrite ).ToList();

                    var path = "";
                    var result = ( object ) canv;

                    for ( var n = 0; n < count; n++)
                    {
                        path = pair.Value.Split( '.' )[n];

                        if ( props.All( x => x.Name != path ) ) { path = ""; break; }

                        if ( n == count - 1 ) break;

                        result = canv.GetPropValue( string.Join( ".", pair.Value.Split( '.' ), 0, n + 1 ) );

                        props = props.Find( x => x.Name == path ).PropertyType.GetProperties().Where( p => p.CanRead && p.CanWrite ).ToList();
                    }

                    if ( !string.IsNullOrEmpty( path ) )
                    {
                        var prop = props.Find( x => x.Name == path );

                        try
                        {
                            var obj = canv.GetPropValue( pair.Value );

                            var converter = TypeDescriptor.GetConverter( obj.GetType() );

                            var text = mat.unit[ canv.PropertiesSource.Index - 1, 0 ].obj.ToDouble().ToString( cultureInfo );

                            obj = converter.ConvertFromString( null, cultureInfo, text );

                            prop.SetValue( result, obj, null );
                        }
                        catch { }                        
                    }
                }
                catch { }                
            }
        }

        #endregion

        #region Public methods

        public override RegionBase Clone()
        {
            var clone = new XYPlotSeq( this );

            Instances.Add( clone );

            return clone;
        }


        public void Refresh()
        {
            canv.Refresh();
        }


        public void Update()
        {
            Invalidate();
        }


        public void Evaluate( Store store )
        {
            _tracesCount = 0;

            var dc = canv.Series;

            try
            {
                // Set canv properties from the sheet.
                if ( canv.PropertiesSource.SourceType == PropertiesSource.SourceTypeEnum.Sheet )
                {
                    SetPropertiesFromSheet( store );
                    Refresh();
                }

                ParseInput( Entry.Create( Terms ), store );

                // Удаляем лишние графики.
                while ( _tracesCount < dc.Count ) 
                {
                    dc.Remove( dc.Last() );
                }
            } 
            catch ( Exception ex ) 
            {
                for ( var n = _tracesCount; n < dc.Count; n++ )
                {
                    dc[n].Clear();
                }

                ShowError( ex, Term.Empty );
            }
        }


        public void ShowFormatDialog( EnChartElement element )
        {
            var formFormat = new FormFormat( this, SessionProfile, element );

            if ( formFormat.ShowDialog() == DialogResult.OK )
            {
                Refresh();
                Update();
            }
        }


        public override void FromXml( StorageReader storage, FileParsingContext parsingContext )
        {
            canv.FromXml( storage, parsingContext );

            base.FromXml( storage, parsingContext );
        }


        public override void ToXml( StorageWriter storage, FileParsingContext parsingContext )
        {
            canv.ToXml( storage, parsingContext );

            base.ToXml( storage, parsingContext );
        }

        #endregion

        #region Events handlers

        public override void Dispose()
        {
            Instances.Remove( this );

            base.Dispose();
        }

        public override void OnEvaluation( Store store )
        {
            base.OnEvaluation( store );

            Evaluate( store );

            Update();
        }


        public override void OnKeyDown( KeyEventOptions e )
        {
            base.OnKeyDown(e);

            canv.OnKeyDown(e);
        }


        public override void OnKeyUp( KeyEventOptions e )
        {            
            canv.OnKeyUp(e);

            base.OnKeyUp(e);
        }


        public override void OnMouseUp( MouseEventOptions e ) 
        {
            base.OnMouseUp(e);

            if ( dblclick.Enabled )
            {
                OnDoubleClick(e);
            }

            dblclick.Enabled = true;
        }


        public void OnDoubleClick( MouseEventOptions e )
        {
            var reg = new Region( canv.ChartArea.ChartRect );

            if ( reg.IsVisible( e.Location ) )
            {
                var plt = canv.ChartArea.PlotRect;

                var xaxisreg = new Region( new Rectangle( new Point( plt.Left, plt.Top + plt.Height ), new Size( plt.Width, 20 ) ) );
                var yaxisreg = new Region( new Rectangle( new Point( plt.Left - 20, plt.Top ), new Size( 20, plt.Height ) ) );
                var y2axisreg = new Region( new Rectangle( new Point( plt.Left + plt.Width, plt.Top ), new Size( 20, plt.Height ) ) );

                var element = EnChartElement.None;

                if ( xaxisreg.IsVisible( e.Location ) )
                {
                    element = EnChartElement.XAxis;
                }
                else if ( yaxisreg.IsVisible( e.Location ) )
                {
                    element = EnChartElement.YAxis;
                }
                else if ( canv.Y2Axis.IsY2Axis && y2axisreg.IsVisible( e.Location ) )
                {
                    element = EnChartElement.Y2Axis;
                }

                ShowFormatDialog( element );
            }
        }


        public override void OnPaint( PaintEventOptions e )
        {
            DrawBase = false;

            math.BackColor = Color.Transparent;

            canv.Border = Focused;
            canv.BackColor = Color.Transparent;

            var g = e.Graphics.Unwrap<Graphics>();

            var smooth = g.SmoothingMode;

            g.SmoothingMode = SmoothingMode.None;

            base.OnPaint(e);

            g.SmoothingMode = smooth;
        }

        #endregion

    }
}
