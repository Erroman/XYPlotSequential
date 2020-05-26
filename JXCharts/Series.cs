using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml;


namespace JXCharts 
{
    public delegate void Action<T1, T2, T3, T4, T5>( T1 p1, T2 p2, T3 p3, T4 p4, T5 p5 );

    public struct Line2D
    {
        public PointD P1;
        public PointD P2;
    }

    public class Series
    {

        #region Constructors

        public Series()
        {
            SeriesName = "";
            LineStyle = new LineStyle();
            SymbolStyle = new SymbolStyle();
        }


        public Series( Series series )
        {
            IsY2Data = series.IsY2Data;
            SeriesName = series.SeriesName;
            LineStyle = series.LineStyle.Clone();
            SymbolStyle = series.SymbolStyle.Clone();
        }

        #endregion

        #region Private methods

        private void DrawSymbol( Graphics g, PointD point, ChartArea ca, Axis xa, Axis ya )
        {
            var p = point;

            if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) return;
            if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) return;

            var ymin = ya.Min;
            var ymax = ya.Max;

            if ( ymin > ymax )
            {
                var t = ymin;
                ymin = ymax;
                ymax = t;
            }

            var xmin = xa.Min;
            var xmax = xa.Max;

            if ( xmin > xmax )
            {
                var t = xmin;
                xmin = xmax;
                xmax = t;
            }

            if ( ( p.X > xmax ) || ( p.X < xmin ) ) return;
            if ( ( p.Y > ymax ) || ( p.Y < ymin ) ) return;

            // Transform the current point from user scale units to
            // screen coordinates.
            var xy = new PointF( ( float ) p.X, ( float ) p.Y );

            xy = ca.Point2D( xy, xa, ya );

            p.X = xy.X;
            p.Y = xy.Y;

            SymbolStyle.DrawSymbol( g, new PointF( ( float ) p.X, ( float ) p.Y ) );
        }


        protected void DrawSymbols( Graphics g, List<PointD> points, ChartArea ca, Axis xa, Axis ya )
        {
            if ( points == null ) return;

            foreach ( var point in points )
            {
                DrawSymbol( g, point, ca, xa, ya );
            }
        }

        #endregion

        #region Public methods

        public virtual Series Clone() => new Series( this );


        public virtual void Clear()
        {
        }


        public virtual void AddData<T>( List<T> data )
        {
        }


        public override string ToString() => "Trace";


        public virtual void DrawSeries( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
        }


        public void FromXml( XmlReader reader )
        {
            if ( reader.HasAttributes )
            {
                SeriesName = "";

                var text = reader.GetAttribute( "seriesname" );

                if ( !string.IsNullOrEmpty( text ) ) SeriesName = text;

                text = reader.GetAttribute( "isy2data" );

                if ( !string.IsNullOrEmpty( text ) ) IsY2Data = Convert.ToBoolean( text );
            }

            LineStyle.FromXml( reader );
            SymbolStyle.FromXml( reader );
        }


        public void ToXml( XmlWriter writer )
        {
            writer.WriteAttributeString( "seriesname", SeriesName );
            writer.WriteAttributeString( "isy2data", IsY2Data.ToString().ToLower() );

            LineStyle.ToXml( writer );
            SymbolStyle.ToXml( writer );
        }

        #endregion

        #region Properties

        [Browsable( true )]
        [Category( "Appearance" )]
        public bool IsY2Data { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public LineStyle LineStyle { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public SymbolStyle SymbolStyle { get; set; }

        [Browsable( true )]
        [DisplayName( "Name" ),
         Category( "Appearance" )]
        public string SeriesName { get; set; }

        #endregion

    }


    public class DataSeries<T> : Series
    {

        #region Constructors

        public DataSeries()
        {
            Items = new List<T>();
        }

        
        public DataSeries( DataSeries<T> ds ): base( ds )
        {
            Items = new List<T>();

            Items.AddRange( ds.Items );
        }

        #endregion

        #region Public methods

        public override Series Clone()
        {
            return new DataSeries<T>( this );
        }

        public override void Clear()
        {
            Items.Clear();
        }

        public void AddData( List<T> data )
        {
            Items.AddRange( data );
        }

        #endregion

        #region Properties

        protected List<T> Items { get; set; }

        #endregion

    }


    public class PointSeries : DataSeries<PointD>
    {
        
        #region Constructors

        public PointSeries()
        {
        }

        public PointSeries( PointSeries series ) : base( series )
        {          
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method just handles the case where one or more of the coordinates are outrageous,
        /// or GDI+ threw an exception.  This method attempts to correct the outrageous coordinates by
        /// interpolating them to a point (along the original line) that lies at the edge of the ChartRect
        /// so that GDI+ will handle it properly.  GDI+ will throw an exception, or just plot the data
        /// incorrectly if the coordinates are too large (empirically, this appears to be when the
        /// coordinate value is greater than 5,000,000 or less than -5,000,000).  Although you typically
        /// would not see coordinates like this, if you repeatedly zoom in on a ZedGraphControl, eventually
        /// all your points will be way outside the bounds of the plot.
        /// </summary>
        private Line2D InterpolatePoint( Graphics g, ChartArea ca, Pen pen, float lastX, float lastY, float tmpX, float tmpY )
        {
            try
            {
                RectangleF chartRect = ca.PlotRect;

                // try to interpolate values
                var lastIn = chartRect.Contains( lastX, lastY );
                var curIn = chartRect.Contains( tmpX, tmpY );

                // If both points are outside the ChartRect, make a new point that is on the LastX/Y
                // side of the ChartRect, and fall through to the code that handles lastIn == true
                if ( !lastIn )
                {
                    float newX, newY;

                    if ( Math.Abs( lastX ) > Math.Abs( lastY ) )
                    {
                        newX = lastX < 0 ? chartRect.Left : chartRect.Right;
                        newY = lastY + ( tmpY - lastY ) * ( newX - lastX ) / ( tmpX - lastX );
                    }
                    else
                    {
                        newY = lastY < 0 ? chartRect.Top : chartRect.Bottom;
                        newX = lastX + ( tmpX - lastX ) * ( newY - lastY ) / ( tmpY - lastY );
                    }

                    lastX = newX;
                    lastY = newY;
                }

                if ( !curIn )
                {
                    float newX, newY;

                    if ( Math.Abs( tmpX ) > Math.Abs( tmpY ) )
                    {
                        newX = tmpX < 0 ? chartRect.Left : chartRect.Right;
                        newY = tmpY + ( lastY - tmpY ) * ( newX - tmpX ) / ( lastX - tmpX );
                    }
                    else
                    {
                        newY = tmpY < 0 ? chartRect.Top : chartRect.Bottom;
                        newX = tmpX + ( lastX - tmpX ) * ( newY - tmpY ) / ( lastY - tmpY );
                    }

                    tmpX = newX;
                    tmpY = newY;
                }

                //g.DrawLine( pen, lastX, lastY, tmpX, tmpY );
            }
            catch { }

            return new Line2D { P1 = new PointD( lastX, lastY ), P2 = new PointD( tmpX, tmpY ) };
        }


        private void DrawCurve( Graphics g, List<PointD> points, ChartArea ca, Axis xa, Axis ya )
        {
            if ( points == null ) return;

            if ( points.Count == 0 ) return;

            var aPen = new Pen( LineStyle.LineColor, LineStyle.Thickness ) { DashStyle = LineStyle.Pattern };

            bool isOut = true;
            bool lastBad = true;

            float tmpX, tmpY;
            float lastX = float.MaxValue;
            float lastY = float.MaxValue;

            int minX = ca.PlotRect.Left;
            int maxX = ca.PlotRect.Right;
            int minY = ca.PlotRect.Top;
            int maxY = ca.PlotRect.Bottom;

            float curX, curY;

            var lines = new List<Line2D?>();

            foreach ( var curPt in points )
            {
                curX = ( float ) curPt.X;
                curY = ( float ) curPt.Y;

                if ( float.IsNaN( curX ) || float.IsNaN( curY ) || float.IsInfinity( curX ) || float.IsInfinity( curY ) )
                {
                    if ( !lastBad ) lines.Add( null );

                    lastBad = true;
                }
                else
                {
                    // Transform the current point from user scale units to
                    // screen coordinates.
                    var xy = new PointF( curX, curY );

                    xy = ca.Point2D( xy, xa, ya );

                    tmpX = xy.X;
                    tmpY = xy.Y;
                    
                    isOut = ( tmpX < minX && lastX < minX ) || ( tmpX > maxX && lastX > maxX ) ||
                            ( tmpY < minY && lastY < minY ) || ( tmpY > maxY && lastY > maxY );

                    if ( !lastBad )
                    {
                        try
                        {
                            // GDI+ plots the data wrong and/or throws an exception for
                            // outrageous coordinates, so we do a sanity check here
                            if ( lastX > 5000000 || lastX < -5000000 ||
                                    lastY > 5000000 || lastY < -5000000 ||
                                    tmpX > 5000000 || tmpX < -5000000 ||
                                    tmpY > 5000000 || tmpY < -5000000 )
                            {
                                var line = InterpolatePoint( g, ca, aPen, lastX, lastY, tmpX, tmpY );
                                lines.Add( line );
                            }
                            else if ( !isOut )
                            {
                                //g.DrawLine( aPen, lastX, lastY, tmpX, tmpY );

                                var line = new Line2D { P1 = new PointD( lastX, lastY ), P2 = new PointD( tmpX, tmpY ) };
                                lines.Add( line );
                            }
                        }
                        catch
                        {
                            var line = InterpolatePoint( g, ca, aPen, lastX, lastY, tmpX, tmpY );
                            lines.Add( line );
                        }
                    }

                    lastX = tmpX;
                    lastY = tmpY;
                    lastBad = false;                    
                }
            }

            var last = new PointF( float.NaN, float.NaN );
            var path = new List<PointF>();

            foreach ( var line in lines )
            {
                if ( line == null )
                {
                    last = new PointF( float.NaN, float.NaN );

                    path.Add( last );

                    continue;
                }

                var p1 = new PointF( ( float ) ( ( Line2D ) line ).P1.X, ( float ) ( ( Line2D ) line ).P1.Y );
                var p2 = new PointF( ( float ) ( ( Line2D ) line ).P2.X, ( float ) ( ( Line2D ) line ).P2.Y );

                if ( last != p1 || !path.Any() ) path.Add( p1 );

                path.Add( p2 );

                last = p2;
            }

            if ( path.Count > 1 ) g.DrawLines( aPen, path.ToArray() );

            aPen.Dispose();
        }


        private void DrawLines( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {            
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            DrawCurve( g, Items, ca, xa, ya );

            g.SmoothingMode = SymbolStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            DrawSymbols( g, Items, ca, xa, ya );
        }


        private void DrawSpline( Graphics g, List<PointD> points, ChartArea ca, Axis xa, Axis ya )
        {
            if ( points == null ) return;

            var aPen = new Pen( LineStyle.LineColor, LineStyle.Thickness ) { DashStyle = LineStyle.Pattern };

            var pts = new List<PointF>();

            foreach ( var point in points )
            {
                var p = point;

                if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) continue;
                if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) continue;

                // Transform the current point from user scale units to
                // screen coordinates.
                var xy = new PointF( ( float ) p.X, ( float ) p.Y );

                xy = ca.Point2D( xy, xa, ya );

                p.X = xy.X;
                p.Y = xy.Y;

                pts.Add( new PointF( ( float ) p.X, ( float ) p.Y ) );
            }

            if ( pts.Count > 1 )
            {
                g.DrawCurve( aPen, pts.ToArray() );
            }

            aPen.Dispose();
        }


        private void DrawSplines( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            DrawSpline( g, Items, ca, xa, ya );

            g.SmoothingMode = SymbolStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            DrawSymbols( g, Items, ca, xa, ya );
        }

        #endregion

        #region Public methods

        public override Series Clone() => new PointSeries( this );


        public override void DrawSeries( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            if ( !LineStyle.Visible ) return;

            if ( LineStyle.PlotMethod == LineStyle.PlotLinesMethodEnum.Lines )
            {
                DrawLines( g, ca, xa, ya );
            }

            else if ( LineStyle.PlotMethod == LineStyle.PlotLinesMethodEnum.Splines )
            {
                DrawSplines( g, ca, xa, ya );
            }
        }

        #endregion

    }


    public class PolylineSeries : DataSeries<PointD[]>
    {

        #region Constructors

        public PolylineSeries()
        {
        }

        public PolylineSeries( PolylineSeries series ) : base( series )
        {
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method just handles the case where one or more of the coordinates are outrageous,
        /// or GDI+ threw an exception.  This method attempts to correct the outrageous coordinates by
        /// interpolating them to a point (along the original line) that lies at the edge of the ChartRect
        /// so that GDI+ will handle it properly.  GDI+ will throw an exception, or just plot the data
        /// incorrectly if the coordinates are too large (empirically, this appears to be when the
        /// coordinate value is greater than 5,000,000 or less than -5,000,000).  Although you typically
        /// would not see coordinates like this, if you repeatedly zoom in on a ZedGraphControl, eventually
        /// all your points will be way outside the bounds of the plot.
        /// </summary>
        private Line2D InterpolatePoint( Graphics g, ChartArea ca, Pen pen, float lastX, float lastY, float tmpX, float tmpY )
        {
            try
            {
                RectangleF chartRect = ca.PlotRect;

                // try to interpolate values
                var lastIn = chartRect.Contains( lastX, lastY );
                var curIn = chartRect.Contains( tmpX, tmpY );

                // If both points are outside the ChartRect, make a new point that is on the LastX/Y
                // side of the ChartRect, and fall through to the code that handles lastIn == true
                if ( !lastIn )
                {
                    float newX, newY;

                    if ( Math.Abs( lastX ) > Math.Abs( lastY ) )
                    {
                        newX = lastX < 0 ? chartRect.Left : chartRect.Right;
                        newY = lastY + ( tmpY - lastY ) * ( newX - lastX ) / ( tmpX - lastX );
                    }
                    else
                    {
                        newY = lastY < 0 ? chartRect.Top : chartRect.Bottom;
                        newX = lastX + ( tmpX - lastX ) * ( newY - lastY ) / ( tmpY - lastY );
                    }

                    lastX = newX;
                    lastY = newY;
                }

                if ( !curIn )
                {
                    float newX, newY;

                    if ( Math.Abs( tmpX ) > Math.Abs( tmpY ) )
                    {
                        newX = tmpX < 0 ? chartRect.Left : chartRect.Right;
                        newY = tmpY + ( lastY - tmpY ) * ( newX - tmpX ) / ( lastX - tmpX );
                    }
                    else
                    {
                        newY = tmpY < 0 ? chartRect.Top : chartRect.Bottom;
                        newX = tmpX + ( lastX - tmpX ) * ( newY - tmpY ) / ( lastY - tmpY );
                    }

                    tmpX = newX;
                    tmpY = newY;
                }

                //g.DrawLine( pen, lastX, lastY, tmpX, tmpY );
            }
            catch { }

            return new Line2D { P1 = new PointD( lastX, lastY ), P2 = new PointD( tmpX, tmpY ) };
        }


        private void DrawCurve( Graphics g, PointD[] points, ChartArea ca, Axis xa, Axis ya )
        {
            if ( points == null ) return;

            if ( points.Length == 0 ) return;

            var aPen = new Pen( LineStyle.LineColor, LineStyle.Thickness ) { DashStyle = LineStyle.Pattern };

            bool isOut = true;
            bool lastBad = true;

            float tmpX, tmpY;
            float lastX = float.MaxValue;
            float lastY = float.MaxValue;

            int minX = ca.PlotRect.Left;
            int maxX = ca.PlotRect.Right;
            int minY = ca.PlotRect.Top;
            int maxY = ca.PlotRect.Bottom;

            float curX, curY;

            var lines = new List<Line2D?>();

            foreach ( var curPt in points )
            {
                curX = ( float ) curPt.X;
                curY = ( float ) curPt.Y;

                if ( float.IsNaN( curX ) || float.IsNaN( curY ) || float.IsInfinity( curX ) || float.IsInfinity( curY ) )
                {
                    if ( !lastBad ) lines.Add( null );

                    lastBad = true;
                }
                else
                {
                    // Transform the current point from user scale units to
                    // screen coordinates.
                    var xy = new PointF( curX, curY );

                    xy = ca.Point2D( xy, xa, ya );

                    tmpX = xy.X;
                    tmpY = xy.Y;

                    isOut = ( tmpX < minX && lastX < minX ) || ( tmpX > maxX && lastX > maxX ) ||
                            ( tmpY < minY && lastY < minY ) || ( tmpY > maxY && lastY > maxY );

                    if ( !lastBad )
                    {
                        try
                        {
                            // GDI+ plots the data wrong and/or throws an exception for
                            // outrageous coordinates, so we do a sanity check here
                            if ( lastX > 5000000 || lastX < -5000000 ||
                                 lastY > 5000000 || lastY < -5000000 ||
                                 tmpX > 5000000 || tmpX < -5000000 ||
                                 tmpY > 5000000 || tmpY < -5000000 )
                            {
                                var line = InterpolatePoint( g, ca, aPen, lastX, lastY, tmpX, tmpY );

                                lines.Add( line );
                            }
                            else if ( !isOut )
                            {
                                //g.DrawLine( aPen, lastX, lastY, tmpX, tmpY );

                                var line = new Line2D { P1 = new PointD( lastX, lastY ), P2 = new PointD( tmpX, tmpY ) };
                                lines.Add( line );
                            }
                        }
                        catch
                        {
                            var line = InterpolatePoint( g, ca, aPen, lastX, lastY, tmpX, tmpY );
                            lines.Add( line );
                        }
                    }

                    lastX = tmpX;
                    lastY = tmpY;
                    lastBad = false;
                }
            }

            var last = new PointF( float.NaN, float.NaN );
            var path = new List<PointF>();

            foreach ( var line in lines )
            {
                if ( line == null )
                {
                    last = new PointF( float.NaN, float.NaN );

                    path.Add( last );

                    continue;
                }

                var p1 = new PointF( ( float ) ( ( Line2D ) line ).P1.X, ( float ) ( ( Line2D ) line ).P1.Y );
                var p2 = new PointF( ( float ) ( ( Line2D ) line ).P2.X, ( float ) ( ( Line2D ) line ).P2.Y );

                if ( last != p1 || !path.Any() ) path.Add( p1 );

                path.Add( p2 );

                last = p2;
            }

            if ( path.Count > 1 ) g.DrawLines( aPen, path.ToArray() );

            aPen.Dispose();
        }


        private void DrawLines( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            foreach ( var points in Items )
            {
                DrawCurve( g, points, ca, xa, ya );
            }

            g.SmoothingMode = SymbolStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            foreach ( var points in Items )
            {
                DrawSymbols( g, points.ToList(), ca, xa, ya );
            }
        }


        private void DrawSpline( Graphics g, PointD[] points, ChartArea ca, Axis xa, Axis ya )
        {
            if ( points == null ) return;

            var aPen = new Pen( LineStyle.LineColor, LineStyle.Thickness ) { DashStyle = LineStyle.Pattern };

            var pts = new List<PointF>();

            foreach ( var point in points )
            {
                var p = point;

                if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) continue;

                if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) continue;

                // Transform the current point from user scale units to
                // screen coordinates.
                var xy = new PointF( ( float ) p.X, ( float ) p.Y );

                xy = ca.Point2D( xy, xa, ya );

                p.X = xy.X;
                p.Y = xy.Y;

                pts.Add( new PointF( ( float ) p.X, ( float ) p.Y ) );
            }

            if ( pts.Count > 1 )
            {
                g.DrawCurve( aPen, pts.ToArray() );
            }

            aPen.Dispose();
        }


        private void DrawSplines( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            foreach ( var points in Items )
            {
                DrawSpline( g, points, ca, xa, ya );
            }

            g.SmoothingMode = SymbolStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            foreach ( var points in Items )
            {
                DrawSymbols( g, points.ToList(), ca, xa, ya );
            }
        }

        #endregion

        #region Public methods

        public override Series Clone() => new PolylineSeries( this );


        public override void DrawSeries( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            if ( !LineStyle.Visible ) return;

            if ( LineStyle.PlotMethod == LineStyle.PlotLinesMethodEnum.Lines )
            {
                DrawLines( g, ca, xa, ya );
            }
            else if ( LineStyle.PlotMethod == LineStyle.PlotLinesMethodEnum.Splines )
            {
                DrawSplines( g, ca, xa, ya );
            }
        }

        #endregion

    }


    public class LineSeries : DataSeries<Line2D>
    {

        #region Constructors

        public LineSeries()
        {
        }

        public LineSeries( LineSeries series ) : base( series )
        {
        }

        #endregion

        #region Private methods

        private void DrawLine( Graphics g, Line2D line, ChartArea ca, Axis xa, Axis ya )
        {
            var aPen = new Pen( LineStyle.LineColor, LineStyle.Thickness ) { DashStyle = LineStyle.Pattern };

            var p1 = line.P1;

            if ( double.IsNaN( p1.X ) || double.IsNaN( p1.Y ) ) return;

            if ( double.IsInfinity( p1.X ) || double.IsInfinity( p1.Y ) ) return;

            var p2 = line.P2;

            if ( double.IsNaN( p2.X ) || double.IsNaN( p2.Y ) ) return;

            if ( double.IsInfinity( p2.X ) || double.IsInfinity( p2.Y ) ) return;

            var xy1 = new PointF( ( float ) p1.X, ( float ) p1.Y );
            var xy2 = new PointF( ( float ) p2.X, ( float ) p2.Y );

            xy1 = ca.Point2D( xy1, xa, ya );
            xy2 = ca.Point2D( xy2, xa, ya );

            g.DrawLine( aPen, xy1, xy2 );

            aPen.Dispose();
        }


        private void DrawLines( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            foreach ( var line in Items )
            {
                DrawLine( g, line, ca, xa, ya );
            }
        }

        #endregion

        #region Public methods

        public override Series Clone() => new LineSeries( this );


        public override void DrawSeries( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            if ( LineStyle.Visible ) DrawLines( g, ca, xa, ya );
        }

        #endregion

    }


    public class LabelSeries : DataSeries<TextLabel>
    {

        #region Constructors

        public LabelSeries()
        {
            LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Labels;
        }


        public LabelSeries( LabelSeries series ) : base( series )
        {
            LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Labels;
        }

        #endregion

        #region Private methods

        private void DrawLabel( Graphics g, ChartArea ca, Axis xa, Axis ya, TextLabel label )
        {
            if ( label == null ) return;

            var p = label.Location;

            if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) return;
            if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) return;

            var ymin = ya.Min;
            var ymax = ya.Max;

            if ( ymin > ymax )
            {
                var t = ymin;
                ymin = ymax;
                ymax = t;
            }

            var xmin = xa.Min;
            var xmax = xa.Max;

            if ( xmin > xmax )
            {
                var t = xmin;
                xmin = xmax;
                xmax = t;
            }

            if ( ( p.X > xmax ) || ( p.X < xmin ) ) return;
            if ( ( p.Y > ymax ) || ( p.Y < ymin ) ) return;

            // Transform the current point from user scale units to
            // screen coordinates.
            var xy = new PointF( ( float ) p.X, ( float ) p.Y );

            xy = ca.Point2D( xy, xa, ya );

            // "+", ".", "*", "o", "x"
            if ( label.IsSymbol )
            {
                var aPen = new Pen( label.IsColorManual ? label.Color : SymbolStyle.BorderColor, 1f );

                var aBrush = new SolidBrush( label.IsColorManual ? label.Color : SymbolStyle.FillColor );

                var x = xy.X;
                var y = xy.Y;

                var size = label.IsSizeManual ? label.Size : SymbolStyle.SymbolSize;

                var halfSize = size / 2.0f;

                var aRectangle = new RectangleF( x - halfSize, y - halfSize, size, size );

                if ( label.Text.Equals( "+" ) )
                {
                    g.DrawLine( aPen, x, y - halfSize, x, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y, x + halfSize, y );
                }
                else if ( label.Text.Equals( "." ) )
                {
                    g.FillEllipse( aBrush, aRectangle );
                    g.DrawEllipse( aPen, aRectangle );
                }
                else if ( label.Text.Equals( "*" ) )
                {
                    g.DrawLine( aPen, x, y - halfSize, x, y + halfSize );
                    g.DrawLine( aPen, x - halfSize, y, x + halfSize, y );
                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x - halfSize, y + halfSize );
                }
                else if ( label.Text.Equals( "o" ) )
                {
                    g.DrawEllipse( aPen, x - halfSize, y - halfSize, size, size );
                }
                else if ( label.Text.Equals( "x" ) )
                {
                    g.DrawLine( aPen, x - halfSize, y - halfSize, x + halfSize, y + halfSize );
                    g.DrawLine( aPen, x + halfSize, y - halfSize, x - halfSize, y + halfSize );
                }

                aPen.Dispose();
                aBrush.Dispose();
            }
            else
            {
                var aBrush = new SolidBrush( label.IsColorManual ? label.Color : LineStyle.LineColor );

                g.DrawString( label.Text, new Font( "Courier", label.Size ), aBrush, xy );

                aBrush.Dispose();
            }
        }


        private void DrawLabels( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            foreach ( var label in Items )
            {                
                DrawLabel( g, ca, xa, ya, label );
            }

            var symPoints = Items.Where( x => !x.IsSymbol ).Select( x => x.Location ).ToList();

            g.SmoothingMode = SymbolStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            DrawSymbols( g, symPoints, ca, xa, ya );
        }

        #endregion

        #region Public methods

        public override Series Clone()
        {
            return new LabelSeries( this );
        }


        public override void DrawSeries( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            if ( !LineStyle.Visible ) return;

            if ( LineStyle.PlotMethod == LineStyle.PlotLinesMethodEnum.Labels )
            {
                DrawLabels( g, ca, xa, ya );
            }
        }

        #endregion

    }
    

    public class ShapeSeries : DataSeries<Shape>
    {

        #region Constructors

        public ShapeSeries()
        {
            LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Shapes;
        }


        public ShapeSeries( ShapeSeries series ) : base( series )
        {
            LineStyle.PlotMethod = LineStyle.PlotLinesMethodEnum.Shapes;
        }

        #endregion

        #region Private methods

        private void DrawLineShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p1 = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );
            var p2 = new PointF( ( float ) mat.unit[ 2, 0 ].obj.ToDouble(), ( float ) mat.unit[ 3, 0 ].obj.ToDouble() );

            if ( double.IsNaN( p1.X ) || double.IsNaN( p1.Y ) ) return;
            if ( double.IsInfinity( p1.X ) || double.IsInfinity( p1.Y ) ) return;

            if ( double.IsNaN( p2.X ) || double.IsNaN( p2.Y ) ) return;
            if ( double.IsInfinity( p2.X ) || double.IsInfinity( p2.Y ) ) return;

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawLine( aPen, p1, p2 );

            aPen.Dispose();
        }


        private void DrawRectangleShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );

            var w = ( float ) mat.unit[ 2, 0 ].obj.ToDouble();
            var h = ( float ) mat.unit[ 3, 0 ].obj.ToDouble();

            if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) return;
            if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) return;

            var p1 = new PointF( p.X, p.Y );
            var p2 = new PointF( p.X + w, p.Y + h );

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            if ( shape.IsFillColorManual && shape.FillColor != Color.Empty )
            {
                var aBrush = new SolidBrush( shape.FillColor );

                g.FillRectangle( aBrush, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y );

                aBrush.Dispose();
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawRectangle( aPen, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y );

            aPen.Dispose();
        }


        private GraphicsPath RoundedRect( RectangleF bounds, int radius )
        {
            var diameter = radius * 2;
            var size = new Size( diameter, diameter );
            var arc = new RectangleF( bounds.Location, size );
            var path = new GraphicsPath();

            if ( radius == 0 )
            {
                path.AddRectangle( bounds );
                return path;
            }

            // top left arc  
            path.AddArc( arc, 180, 90 );

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc( arc, 270, 90 );

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc( arc, 0, 90 );

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc( arc, 90, 90 );

            path.CloseFigure();

            return path;
        }


        private void DrawRoundedRectangle( Graphics graphics, Pen pen, RectangleF bounds, int cornerRadius )
        {
            using ( var path = RoundedRect( bounds, cornerRadius ) )
            {
                graphics.DrawPath( pen, path );
            }
        }


        private void FillRoundedRectangle( Graphics graphics, Brush brush, RectangleF bounds, int cornerRadius )
        {
            using ( var path = RoundedRect( bounds, cornerRadius ) )
            {
                graphics.FillPath( brush, path );
            }
        }


        private void DrawRoundedRectangleShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );

            var w = ( float ) mat.unit[ 2, 0 ].obj.ToDouble();
            var h = ( float ) mat.unit[ 3, 0 ].obj.ToDouble();

            var r = ( float ) mat.unit[ 4, 0 ].obj.ToDouble();

            if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) return;
            if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) return;

            var p1 = new PointF( p.X, p.Y );
            var p2 = new PointF( p.X + w, p.Y + h );

            var d1 = Math.Abs( p2.X - p1.X );

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            var d2 = Math.Abs( p2.X - p1.X );

            if ( shape.IsFillColorManual && shape.FillColor != Color.Empty )
            {
                var aBrush = new SolidBrush( shape.FillColor );

                FillRoundedRectangle( g, aBrush, new RectangleF( p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y ), ( int ) ( r * d2 / d1 ) );

                aBrush.Dispose();
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            DrawRoundedRectangle( g, aPen, new RectangleF( p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y ), ( int ) ( r * d2 / d1 ) );

            aPen.Dispose();
        }


        private void DrawCircleShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );

            var r = ( float ) mat.unit[ 2, 0 ].obj.ToDouble();

            var p1 = new PointF( p.X - r, p.Y - r );
            var p2 = new PointF( p.X + r, p.Y + r );

            if ( double.IsNaN( p1.X ) || double.IsNaN( p1.Y ) ) return;
            if ( double.IsInfinity( p1.X ) || double.IsInfinity( p1.Y ) ) return;

            if ( double.IsNaN( p2.X ) || double.IsNaN( p2.Y ) ) return;
            if ( double.IsInfinity( p2.X ) || double.IsInfinity( p2.Y ) ) return;

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            if ( shape.IsFillColorManual && shape.FillColor != Color.Empty )
            {
                var aBrush = new SolidBrush( shape.FillColor );

                g.FillEllipse( aBrush, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y );

                aBrush.Dispose();
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawEllipse( aPen, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y );

            aPen.Dispose();
        }


        private void DrawEllipseShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );

            var rx = ( float ) mat.unit[ 2, 0 ].obj.ToDouble();
            var ry = ( float ) mat.unit[ 3, 0 ].obj.ToDouble();

            var p1 = new PointF( p.X - rx, p.Y - ry );
            var p2 = new PointF( p.X + rx, p.Y + ry );

            if ( double.IsNaN( p1.X ) || double.IsNaN( p1.Y ) ) return;
            if ( double.IsInfinity( p1.X ) || double.IsInfinity( p1.Y ) ) return;

            if ( double.IsNaN( p2.X ) || double.IsNaN( p2.Y ) ) return;
            if ( double.IsInfinity( p2.X ) || double.IsInfinity( p2.Y ) ) return;

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            if ( shape.IsFillColorManual && shape.FillColor != Color.Empty )
            {
                var aBrush = new SolidBrush( shape.FillColor );

                g.FillEllipse( aBrush, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y );

                aBrush.Dispose();
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            if ( width == 0 || color == Color.Empty ) return;

            g.DrawEllipse( aPen, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y );

            aPen.Dispose();
        }


        private void DrawArcShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );

            var rx = ( float ) mat.unit[ 2, 0 ].obj.ToDouble();
            var ry = ( float ) mat.unit[ 3, 0 ].obj.ToDouble();

            var p1 = new PointF( p.X - rx, p.Y - ry );
            var p2 = new PointF( p.X + rx, p.Y + ry );

            if ( double.IsNaN( p1.X ) || double.IsNaN( p1.Y ) ) return;
            if ( double.IsInfinity( p1.X ) || double.IsInfinity( p1.Y ) ) return;

            if ( double.IsNaN( p2.X ) || double.IsNaN( p2.Y ) ) return;
            if ( double.IsInfinity( p2.X ) || double.IsInfinity( p2.Y ) ) return;

            var startAngle = -( float ) ( mat.unit[ 4, 0 ].obj.ToDouble() / Math.PI * 180f );
            var sweepAngle = -( float ) ( mat.unit[ 5, 0 ].obj.ToDouble() / Math.PI * 180f );

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawArc( aPen, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y, startAngle, sweepAngle );

            aPen.Dispose();
        }


        private void DrawPolygonShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var rows = mat.unit.GetLength( 0 );

            var pp = new List<PointF>();

            for ( var n = 0; n < rows; n++ )
            {
                var p = new PointF( ( float ) mat.unit[ n, 0 ].obj.ToDouble(), ( float ) mat.unit[ n, 1 ].obj.ToDouble() );

                if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) continue;
                if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) continue;

                // Transform the current point from user scale units to
                // screen coordinates.
                p = ca.Point2D( p, xa, ya );

                pp.Add( p );
            }

            if ( shape.IsFillColorManual && shape.FillColor != Color.Empty )
            {
                var aBrush = new SolidBrush( shape.FillColor );

                g.FillPolygon( aBrush, pp.ToArray() );

                aBrush.Dispose();
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawPolygon( aPen, pp.ToArray() );

            aPen.Dispose();
        }


        private void DrawPieShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var p = new PointF( ( float ) mat.unit[ 0, 0 ].obj.ToDouble(), ( float ) mat.unit[ 1, 0 ].obj.ToDouble() );

            var rx = ( float ) mat.unit[ 2, 0 ].obj.ToDouble();
            var ry = ( float ) mat.unit[ 3, 0 ].obj.ToDouble();

            var p1 = new PointF( p.X - rx, p.Y - ry );
            var p2 = new PointF( p.X + rx, p.Y + ry );

            if ( double.IsNaN( p1.X ) || double.IsNaN( p1.Y ) ) return;
            if ( double.IsInfinity( p1.X ) || double.IsInfinity( p1.Y ) ) return;

            if ( double.IsNaN( p2.X ) || double.IsNaN( p2.Y ) ) return;
            if ( double.IsInfinity( p2.X ) || double.IsInfinity( p2.Y ) ) return;

            var startAngle = -( float ) ( mat.unit[ 4, 0 ].obj.ToDouble() / Math.PI * 180f );
            var sweepAngle = -( float ) ( mat.unit[ 5, 0 ].obj.ToDouble() / Math.PI * 180f );

            // Transform the current point from user scale units to
            // screen coordinates.
            p1 = ca.Point2D( p1, xa, ya );
            p2 = ca.Point2D( p2, xa, ya );

            if ( shape.IsFillColorManual && shape.FillColor != Color.Empty )
            {
                var aBrush = new SolidBrush( shape.FillColor );

                g.FillPie( aBrush, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y, startAngle, sweepAngle );

                aBrush.Dispose();
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawPie( aPen, p1.X, p2.Y, p2.X - p1.X, p1.Y - p2.Y, startAngle, sweepAngle );

            aPen.Dispose();
        }


        private void DrawPolylineShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var rows = mat.unit.GetLength( 0 );

            var pp = new List<PointF>();

            for ( var n = 0; n < rows; n++ )
            {
                var p = new PointF( ( float ) mat.unit[ n, 0 ].obj.ToDouble(), ( float ) mat.unit[ n, 1 ].obj.ToDouble() );

                if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) continue;
                if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) continue;

                // Transform the current point from user scale units to
                // screen coordinates.
                p = ca.Point2D( p, xa, ya );

                pp.Add( p );
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawLines( aPen, pp.ToArray() );

            aPen.Dispose();
        }


        private void DrawSplineShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var rows = mat.unit.GetLength( 0 );

            var pp = new List<PointF>();

            for ( var n = 0; n < rows; n++ )
            {
                var p = new PointF( ( float ) mat.unit[ n, 0 ].obj.ToDouble(), ( float ) mat.unit[ n, 1 ].obj.ToDouble() );

                if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) continue;
                if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) continue;

                // Transform the current point from user scale units to
                // screen coordinates.
                p = ca.Point2D( p, xa, ya );

                pp.Add( p );
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawCurve( aPen, pp.ToArray() );

            aPen.Dispose();
        }


        private void DrawBezierShape( Graphics g, Shape shape, ChartArea ca, Axis xa, Axis ya )
        {
            // TODO: Check ranges: [xmin, xmax], [ymin, ymax].
            var mat = shape.Data;

            if ( mat == null ) return;

            var rows = mat.unit.GetLength( 0 );

            var pp = new List<PointF>();

            for ( var n = 0; n < rows; n++ )
            {
                var p = new PointF( ( float ) mat.unit[ n, 0 ].obj.ToDouble(), ( float ) mat.unit[ n, 1 ].obj.ToDouble() );

                if ( double.IsNaN( p.X ) || double.IsNaN( p.Y ) ) continue;
                if ( double.IsInfinity( p.X ) || double.IsInfinity( p.Y ) ) continue;

                // Transform the current point from user scale units to
                // screen coordinates.
                p = ca.Point2D( p, xa, ya );

                pp.Add( p );
            }

            var color = shape.IsLineColorManual ? shape.LineColor : LineStyle.LineColor;
            var width = shape.IsLineWidthManual ? shape.LineWidth : LineStyle.Thickness;

            if ( width == 0 || color == Color.Empty ) return;

            var aPen = new Pen( color, width ) { DashStyle = shape.LineStyle };

            g.DrawBeziers( aPen, pp.ToArray() );

            aPen.Dispose();
        }


        private void DrawShapes( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            g.SmoothingMode = LineStyle.AntiAlias ? SmoothingMode.AntiAlias : SmoothingMode.None;

            var drawActions = new Dictionary<EnShapeType, Action<Graphics, Shape, ChartArea, Axis, Axis>>
            {
                { EnShapeType.None, ( p1, p2, p3, p4, p5 ) => {} },
                { EnShapeType.Line, DrawLineShape },
                { EnShapeType.Rectangle, DrawRectangleShape },
                { EnShapeType.RoundedRectangle, DrawRoundedRectangleShape },
                { EnShapeType.Circle, DrawCircleShape },
                { EnShapeType.Ellipse, DrawEllipseShape },
                { EnShapeType.Arc, DrawArcShape },
                { EnShapeType.Polygon, DrawPolygonShape },
                { EnShapeType.Pie, DrawPieShape },
                { EnShapeType.Polyline, DrawPolylineShape },
                { EnShapeType.Spline, DrawSplineShape },
                { EnShapeType.Bezier, DrawBezierShape }
            };

            foreach ( var shape in Items )
            {
                if ( shape == null ) continue;

                if ( drawActions.Keys.Contains( shape.Type ) )
                {
                    try
                    {
                        drawActions[ shape.Type ]( g, shape, ca, xa, ya );
                    }
                    catch { }
                }
            }
        }

        #endregion

        #region Public methods

        public override Series Clone()
        {
            return new ShapeSeries( this );
        }


        public override void DrawSeries( Graphics g, ChartArea ca, Axis xa, Axis ya )
        {
            if ( !LineStyle.Visible ) return;

            if ( LineStyle.PlotMethod == LineStyle.PlotLinesMethodEnum.Shapes )
            {
                DrawShapes( g, ca, xa, ya );
            }
        }

        #endregion

    }
}
