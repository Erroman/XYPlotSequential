using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

using SMath.Manager;


namespace JXCharts
{

    public class ChartArea
    {

        #region Private methods

        public void CalcPlotArea( Graphics g, XAxis xa, YAxis ya, Y2Axis y2a, Grid gd, XYLabel lb, Title tl )
        {
            var xOffset = 5f;
            var yOffset = 5f;

            var xSpacing = 2.0f;
            var ySpacing = 2.0f;

            var labelFontSize = g.MeasureString( "Z", lb.LabelFont );
            var titleFontSize = g.MeasureString( "Z", tl.TitleFont );

            if ( string.IsNullOrEmpty( tl.Text ) )
            {
                titleFontSize.Width = 8f;
                titleFontSize.Height = 8f;
            }

            var tickFontSize = g.MeasureString( "Z", lb.TickFont );
            var tickSpacing = 2f;

            var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };

            var numberFormatString = "";

            switch ( ya.NumberFormat )
            {
                case NumberFormatEnum.Exponential: { numberFormatString = string.Format( "{{0:e{0}}}", ya.DecimalPlaces ); break; }

                case NumberFormatEnum.FixedPoint: { numberFormatString = string.Format( "{{0:f{0}}}", ya.DecimalPlaces ); break; }

                default: { numberFormatString = string.Format( "{{0:0.{0}}}", "".PadRight( ya.DecimalPlaces, '#' ) ); break; }
            }

            var ymin = Math.Min( ya.Min, ya.Max );
            var ymax = Math.Max( ya.Min, ya.Max );
            var ytick = ya.Tick;

            var beg = ( int ) Math.Truncate( ymin / ytick );

            if ( beg * ytick < ymin ) beg++;

            var end = ( int ) Math.Truncate( ymax / ytick );

            if ( end * ytick > ymax ) end--;

            var yTickSize = g.MeasureString( string.Format( provider, numberFormatString, beg * ytick ), lb.TickFont );

            for ( var i = beg; i <= end; i++ )
            {
                var fY = i * ytick;

                var absfY = Math.Abs( fY );

                var exp = absfY > 0 ? ( int ) Math.Floor( Math.Log10( absfY ) ) : 0;

                string text;

                if ( ya.NumberFormat == NumberFormatEnum.General && Math.Abs( exp ) > 2 )
                {
                    fY = ( float ) ( fY / Math.Pow( 10, exp ) );

                    text = string.Format( provider, numberFormatString, fY ) + "⋅10" + exp.ToString().Replace( "-", "–" );
                }
                else
                {
                    text = string.Format( provider, numberFormatString, fY );
                }

                var tempSize = g.MeasureString( text, lb.TickFont );

                if ( yTickSize.Width < tempSize.Width )
                {
                    yTickSize = tempSize;
                }
            }

            var leftMargin = xOffset + xSpacing;

            leftMargin += ya.Visible ? yTickSize.Width + tickSpacing + ( string.IsNullOrEmpty( lb.YLabel ) ? 0 : labelFontSize.Height ) : 0;

            var rightMargin = xOffset + ( xa.Visible ? 2 : 1 ) * xSpacing;

            var topMargin = yOffset + ySpacing + ( string.IsNullOrEmpty( tl.Text ) ? 0 : yOffset + titleFontSize.Height );

            var bottomMargin = yOffset + ySpacing;

            bottomMargin += xa.Visible ? yOffset + tickSpacing + tickFontSize.Height + ( string.IsNullOrEmpty( lb.XLabel ) ? 0 : labelFontSize.Height ) : 0;

            if ( !y2a.IsY2Axis )
            {
                // Define the plot area with one Y axis:
                var plotX = ChartRect.X + ( int ) leftMargin;
                var plotY = ChartRect.Y + ( int ) topMargin;
                var plotWidth = ChartRect.Width - ( int ) leftMargin - ( int ) rightMargin;
                var plotHeight = ChartRect.Height - ( int ) topMargin - ( int ) bottomMargin;

                PlotRect = new Rectangle( plotX, plotY, plotWidth, plotHeight );
            }
            else
            {
                switch ( y2a.NumberFormat )
                {
                    case NumberFormatEnum.Exponential: { numberFormatString = string.Format( "{{0:e{0}}}", y2a.DecimalPlaces ); break; }

                    case NumberFormatEnum.FixedPoint: { numberFormatString = string.Format( "{{0:f{0}}}", y2a.DecimalPlaces ); break; }

                    default: { numberFormatString = string.Format( "{{0:0.{0}}}", "".PadRight( y2a.DecimalPlaces, '#' ) ); break; }
                }

                ymin = Math.Min( y2a.Min, y2a.Max );
                ymax = Math.Max( y2a.Min, y2a.Max );
                ytick = y2a.Tick;

                // Define the plot area with Y and Y2 axes:
                beg = ( int ) Math.Truncate( ymin / ytick );

                if ( ( beg * ytick ) < ymin ) beg++;

                end = ( int ) Math.Truncate( ymax / ytick );

                if ( end * ytick > ymax ) end--;

                var y2TickSize = g.MeasureString( string.Format( provider, numberFormatString, beg * ytick ), lb.TickFont );

                for ( var i = beg; i <= end; i++ )
                {
                    var fY = i * ytick;

                    var absfY = Math.Abs( fY );

                    var exp = absfY > 0 ? ( int ) Math.Floor( Math.Log10( absfY ) ) : 0;

                    string text;

                    if ( y2a.NumberFormat == NumberFormatEnum.General && Math.Abs( exp ) > 2 )
                    {
                        fY = ( float ) ( fY / Math.Pow( 10, exp ) );

                        text = string.Format( provider, numberFormatString, fY ) + "⋅10" + exp.ToString().Replace( "-", "–" );
                    }
                    else
                    {
                        text = string.Format( provider, numberFormatString, fY );
                    }

                    var tempSize2 = g.MeasureString( text, lb.TickFont );

                    if ( y2TickSize.Width < tempSize2.Width )
                    {
                        y2TickSize = tempSize2;
                    }
                }

                rightMargin = xOffset + xSpacing + y2TickSize.Width + tickSpacing + ( string.IsNullOrEmpty( lb.Y2Label ) ? 0 : labelFontSize.Height );

                var plotX = ChartRect.X + ( int ) leftMargin;
                var plotY = ChartRect.Y + ( int ) topMargin;
                var plotWidth = ChartRect.Width - ( int ) leftMargin - ( int ) rightMargin;
                var plotHeight = ChartRect.Height - ( int ) topMargin - ( int ) bottomMargin;

                PlotRect = new Rectangle( plotX, plotY, plotWidth, plotHeight );
            }
        }


        private void DrawXAxis( Graphics g, ChartStyle cs, XAxis xa, YAxis ya, XYLabel lb )
        {
            var p1 = Point2D( new PointF( xa.Min, ya.Min ), xa, ya );
            var p2 = Point2D( new PointF( xa.Max, ya.Min ), xa, ya );

            var aPen = new Pen( Color.Black, 1f );

            g.DrawLine( aPen, p1, p2 );

            // Floating point format.
            //var provider = new NumberFormatInfo { NumberDecimalSeparator = GlobalProfile.DecimalSymbolStandard.ToString() };
            var provider = new NumberFormatInfo { NumberDecimalSeparator = "," };

            string numberFormatString;

            switch ( xa.NumberFormat )
            {
                case NumberFormatEnum.Exponential: { numberFormatString = string.Format( "{{0:e{0}}}", xa.DecimalPlaces ); break; }

                case NumberFormatEnum.FixedPoint: { numberFormatString = string.Format( "{{0:f{0}}}", xa.DecimalPlaces ); break; }

                default: { numberFormatString = string.Format( "{{0:0.{0}}}", "".PadRight( xa.DecimalPlaces, '#' ) ); break; }
            }

            var aBrush = new SolidBrush( lb.TickFontColor );

            var xmin = Math.Min( xa.Min, xa.Max );
            var xmax = Math.Max( xa.Min, xa.Max );
            var xtick = xa.Tick;

            var beg = ( int ) Math.Truncate( xmin / xtick );

            if ( ( beg * xtick ) <= xmin ) beg++;

            var end = ( int ) Math.Truncate( xmax / xtick );

            if ( ( end * xtick ) >= xmax ) end--;

            for ( var i = beg; i <= end; i++ )
            {
                var fX = i * xtick;

                var yAxisPoint = Point2D( new PointF( fX, ya.Min ), xa, ya );

                g.DrawLine( aPen, yAxisPoint, new PointF( yAxisPoint.X, yAxisPoint.Y - 5f ) );

                yAxisPoint = Point2D( new PointF( fX, ya.Max ), xa, ya );

                g.DrawLine( aPen, yAxisPoint, new PointF( yAxisPoint.X, yAxisPoint.Y + 5f ) );
            }

            aPen.Dispose();

            beg = ( int ) Math.Truncate( xmin / xtick );

            if ( ( beg * xtick ) < xmin ) beg++;

            end = ( int ) Math.Truncate( xmax / xtick );

            if ( ( end * xtick ) > xmax ) end--;

            var sfmtFar = new StringFormat { Alignment = StringAlignment.Far };

            var sfmtNear = new StringFormat { Alignment = StringAlignment.Near };

            for ( var i = beg; i <= end; i++ )
            {
                var fX = i * xtick;

                var yAxisPoint = Point2D( new PointF( fX, ya.Min ), xa, ya );

                var absfX = Math.Abs( fX );

                var exp = absfX > 0 ? ( int ) Math.Floor( Math.Log10( absfX ) ) : 0;

                var exps = exp.ToString().Replace( "-", "–" );

                if ( xa.NumberFormat == NumberFormatEnum.General && Math.Abs( exp ) > 2 )
                {
                    fX = ( float ) ( fX / Math.Pow( 10, exp ) );

                    var text = string.Format( provider, numberFormatString, fX ) + "⋅10";

                    var sizeXTick = g.MeasureString( text + exps, lb.TickFont );

                    g.DrawString( text, lb.TickFont, aBrush,
                        new PointF( yAxisPoint.X - sizeXTick.Width / 2f, yAxisPoint.Y + 7f ), sfmtNear );

                    g.DrawString( exps, new Font( lb.TickFont.FontFamily, lb.TickFont.Size - 1 ), aBrush,
                        new PointF( yAxisPoint.X + sizeXTick.Width / 2f, yAxisPoint.Y + 2f ), sfmtFar );
                }
                else
                {
                    var text = string.Format( provider, numberFormatString, fX );

                    var sizeXTick = g.MeasureString( text, lb.TickFont );

                    g.DrawString( text, lb.TickFont, aBrush,
                        new PointF( yAxisPoint.X + sizeXTick.Width / 2f, yAxisPoint.Y + 8f ), sfmtFar );
                }
            }

            aBrush.Dispose();
        }


        private void DrawYAxis( Graphics g, ChartStyle cs, XAxis xa, YAxis ya, Y2Axis y2a, XYLabel lb )
        {
            var p1 = Point2D( new PointF( xa.Min, ya.Min ), xa, ya );
            var p2 = Point2D( new PointF( xa.Min, ya.Max ), xa, ya );

            var aPen = new Pen( Color.Black, 1f );

            g.DrawLine( aPen, p1, p2 );

            var provider = new NumberFormatInfo { NumberDecimalSeparator = "," };

            string numberFormatString;

            switch ( ya.NumberFormat )
            {
                case NumberFormatEnum.Exponential: { numberFormatString = string.Format( "{{0:e{0}}}", ya.DecimalPlaces ); break; }

                case NumberFormatEnum.FixedPoint: { numberFormatString = string.Format( "{{0:f{0}}}", ya.DecimalPlaces ); break; }

                default: { numberFormatString = string.Format( "{{0:0.{0}}}", "".PadRight( ya.DecimalPlaces, '#' ) ); break; }
            }

            var ymin = Math.Min( ya.Min, ya.Max );
            var ymax = Math.Max( ya.Min, ya.Max );
            var ytick = ya.Tick;

            var beg = ( int ) Math.Truncate( ymin / ytick );

            if ( ( beg * ytick ) <= ymin ) beg++;

            var end = ( int ) Math.Truncate( ymax / ytick );

            if ( ( end * ytick ) >= ymax ) end--;

            for ( var i = beg; i <= end; i++ )
            {
                var fY = i * ytick;

                var xAxisPoint = Point2D( new PointF( xa.Min, fY ), xa, ya );

                g.DrawLine( aPen, xAxisPoint, new PointF( xAxisPoint.X + 5f, xAxisPoint.Y ) );

                if ( !y2a.IsY2Axis )
                {
                    xAxisPoint = Point2D( new PointF( xa.Max, fY ), xa, ya );

                    g.DrawLine( aPen, xAxisPoint, new PointF( xAxisPoint.X - 5f, xAxisPoint.Y ) );
                }
            }

            aPen.Dispose();

            var aBrush = new SolidBrush( lb.TickFontColor );

            beg = ( int ) Math.Truncate( ymin / ytick );

            if ( ( beg * ytick ) < ymin ) beg++;

            end = ( int ) Math.Truncate( ymax / ytick );

            if ( ( end * ytick ) > ymax ) end--;

            var tickFontSize = g.MeasureString( "Z", lb.TickFont );

            var sfmtFar = new StringFormat { Alignment = StringAlignment.Far };

            var sfmtNear = new StringFormat { Alignment = StringAlignment.Near };

            for ( var i = beg; i <= end; i++ )
            {
                var fY = i * ytick;

                var xAxisPoint = Point2D( new PointF( xa.Min, fY ), xa, ya );

                var absfY = Math.Abs( fY );

                var exp = absfY > 0 ? ( int ) Math.Floor( Math.Log10( absfY ) ) : 0;

                if ( ya.NumberFormat == NumberFormatEnum.General && Math.Abs( exp ) > 2 )
                {
                    fY = ( float ) ( fY / Math.Pow( 10, exp ) );

                    var text = string.Format( provider, numberFormatString, fY ) + "⋅10";

                    var exps = exp.ToString().Replace( "-", "–" );

                    var sz = g.MeasureString( exps, lb.TickFont );

                    g.DrawString( text, lb.TickFont, aBrush,
                        new PointF( xAxisPoint.X - sz.Width - 2f, xAxisPoint.Y - tickFontSize.Height / 2 ), sfmtFar );

                    g.DrawString( exps, new Font( lb.TickFont.FontFamily, lb.TickFont.Size - 1 ), aBrush,
                        new PointF( xAxisPoint.X - sz.Width - 4f, xAxisPoint.Y - tickFontSize.Height / 2 - 4f ), sfmtNear );
                }
                else
                {
                    var text = string.Format( provider, numberFormatString, fY );

                    g.DrawString( text, lb.TickFont, aBrush,
                        new PointF( xAxisPoint.X - 3f, xAxisPoint.Y - tickFontSize.Height / 2 ), sfmtFar );
                }
            }

            aBrush.Dispose();
        }


        private void DrawY2Axis( Graphics g, ChartStyle cs, XAxis xa, YAxis ya, Y2Axis y2a, XYLabel lb )
        {
            var p1 = Point2D( new PointF( xa.Max, y2a.Min ), xa, y2a );
            var p2 = Point2D( new PointF( xa.Max, y2a.Max ), xa, y2a );

            var aPen = new Pen( cs.PlotBorderColor, 1f );

            g.DrawLine( aPen, p1, p2 );

            var provider = new NumberFormatInfo { NumberDecimalSeparator = "," };

            string numberFormatString;

            switch ( y2a.NumberFormat )
            {
                case NumberFormatEnum.Exponential: { numberFormatString = string.Format( "{{0:e{0}}}", y2a.DecimalPlaces ); break; }

                case NumberFormatEnum.FixedPoint: { numberFormatString = string.Format( "{{0:f{0}}}", y2a.DecimalPlaces ); break; }

                default: { numberFormatString = string.Format( "{{0:0.{0}}}", "".PadRight( y2a.DecimalPlaces, '#' ) ); break; }
            }

            var ymin = Math.Min( y2a.Min, y2a.Max );
            var ymax = Math.Max( y2a.Min, y2a.Max );
            var ytick = y2a.Tick;

            var beg = ( int ) Math.Truncate( ymin / ytick );

            if ( ( beg * ytick ) <= ymin ) beg++;

            var end = ( int ) Math.Truncate( ymax / ytick );

            if ( ( end * ytick ) >= ymax ) end--;

            for ( var i = beg; i <= end; i++ )
            {
                var fY = i * ytick;

                var x2AxisPoint = Point2D( new PointF( xa.Max, fY ), xa, y2a );

                g.DrawLine( aPen, x2AxisPoint, new PointF( x2AxisPoint.X - 5f, x2AxisPoint.Y ) );
            }

            aPen.Dispose();

            var aBrush = new SolidBrush( lb.TickFontColor );

            beg = ( int ) Math.Truncate( ymin / ytick );

            if ( ( beg * ytick ) < ymin ) beg++;

            end = ( int ) Math.Truncate( ymax / ytick );

            if ( ( end * ytick ) > ymax ) end--;

            var tickFontSize = g.MeasureString( "Z", lb.TickFont );

            var sfmtFar = new StringFormat { Alignment = StringAlignment.Far };

            var sfmtNear = new StringFormat { Alignment = StringAlignment.Near };

            for ( var i = beg; i <= end; i++ )
            {
                var fY = i * ytick;

                var x2AxisPoint = Point2D( new PointF( xa.Max, fY ), xa, y2a );

                var absfY = Math.Abs( fY );

                var exp = absfY > 0 ? ( int ) Math.Floor( Math.Log10( absfY ) ) : 0;

                if ( y2a.NumberFormat == NumberFormatEnum.General && Math.Abs( exp ) > 2 )
                {
                    fY = ( float ) ( fY / Math.Pow( 10, exp ) );

                    var text = string.Format( provider, numberFormatString, fY ) + "⋅10";

                    var sz = g.MeasureString( text, lb.TickFont );

                    g.DrawString( text, lb.TickFont, aBrush,
                        new PointF( x2AxisPoint.X + sz.Width + 4f, x2AxisPoint.Y - tickFontSize.Height / 2 ), sfmtFar );

                    g.DrawString( exp.ToString().Replace( "-", "–" ), new Font( lb.TickFont.FontFamily, lb.TickFont.Size - 1 ), aBrush,
                        new PointF( x2AxisPoint.X + sz.Width + 2f, x2AxisPoint.Y - tickFontSize.Height / 2 - 4f ), sfmtNear );
                }
                else
                {
                    var text = string.Format( provider, numberFormatString, fY );

                    var sizeYTick = g.MeasureString( text, lb.TickFont );

                    g.DrawString( text, lb.TickFont, aBrush,
                        new PointF( x2AxisPoint.X + sizeYTick.Width + 3f, x2AxisPoint.Y - tickFontSize.Height / 2 ), sfmtFar );
                }
            }

            aBrush.Dispose();
        }


        private void DrawAxes( Graphics g, ChartStyle cs, XAxis xa, YAxis ya, Y2Axis y2a, XYLabel lb )
        {
            if ( xa.Visible ) DrawXAxis( g, cs, xa, ya, lb );

            if ( ya.Visible ) DrawYAxis( g, cs, xa, ya, y2a, lb );

            if ( y2a.IsY2Axis && y2a.Visible )
            {
                DrawY2Axis( g, cs, xa, ya, y2a, lb );
            }
        }


        private void DrawBackground( Graphics g, ChartStyle cs )
        {
            if ( cs.PlotBackColor != Color.Transparent || cs.PlotBackColor != Color.Empty )
            {
                var aBrush = new SolidBrush( cs.PlotBackColor );

                g.FillRectangle( aBrush, PlotRect );

                aBrush.Dispose();
            }
        }


        private void DrawForeground( Graphics g, ChartStyle cs )
        {
            if ( cs.PlotBorderColor != Color.Transparent || cs.PlotBorderColor != Color.Empty )
            {
                var aPen = new Pen( cs.PlotBorderColor, 1f );

                g.DrawRectangle( aPen, PlotRect );

                aPen.Dispose();
            }
        }


        private void DrawLabels( Graphics g, Axis xa, Axis ya, Y2Axis y2a, XYLabel lb, Title tl )
        {
            var xOffset = 5f;
            var yOffset = 5f;

            // Add horizontal axis label.
            var sFormat = new StringFormat { Alignment = StringAlignment.Center };

            var aBrush = new SolidBrush( lb.LabelFontColor );

            if ( !string.IsNullOrEmpty( lb.XLabel ) && xa.Visible )
            {
                var sz = g.MeasureString( lb.XLabel, lb.LabelFont );

                g.DrawString( lb.XLabel, lb.LabelFont, aBrush,
                    new PointF( PlotRect.Left + PlotRect.Width / 2, ChartRect.Bottom - yOffset - sz.Height ), sFormat );
            }

            // Add y-axis label.
            if ( !string.IsNullOrEmpty( lb.YLabel ) && ya.Visible )
            {
                var gState = g.Save();

                g.TranslateTransform( xOffset, PlotRect.Top + PlotRect.Height / 2 );

                g.RotateTransform( -90 );

                g.DrawString( lb.YLabel, lb.LabelFont, aBrush, 0, 0, sFormat );

                g.Restore( gState );
            }

            // Add y2-axis label:
            if ( y2a.IsY2Axis && !string.IsNullOrEmpty( lb.Y2Label ) && y2a.Visible )
            {
                var sz = g.MeasureString( lb.Y2Label, lb.LabelFont );

                // Save the state of the current Graphics object
                var gState2 = g.Save();

                g.TranslateTransform( ChartRect.Right - xOffset - sz.Height, PlotRect.Top + PlotRect.Height / 2 );

                g.RotateTransform( -90 );

                g.DrawString( lb.Y2Label, lb.LabelFont, aBrush, 0, 0, sFormat );

                // Restore it:
                g.Restore( gState2 );
            }

            // Add title:
            aBrush = new SolidBrush( tl.TitleFontColor );

            if ( !string.IsNullOrEmpty( tl.Text ) )
            {
                g.DrawString( tl.Text, tl.TitleFont, aBrush,
                    new PointF( PlotRect.Left + PlotRect.Width / 2, yOffset ), sFormat );
            }

            aBrush.Dispose();
        }


        private void DrawAllSeries( Graphics g, XAxis xa, YAxis ya, Y2Axis y2a, List<Series> series )
        {
            var gs = g.Save();

            var clip = g.Clip;

            g.Clip = new Region( PlotRect );

            // FIXME: Sometimes exception about changed collection.
            foreach ( var ds in series )
            {
                var yaxis = ds.IsY2Data ? ( Axis ) y2a : ya;

                ds.DrawSeries( g, this, xa, yaxis );
            }

            g.Restore( gs );

            g.Clip = clip;

            g.SmoothingMode = SmoothingMode.None;
        }


        #endregion

        #region Public methods

        public override string ToString() => "(...)";


        public void Draw( Graphics g, ChartStyle cs, XAxis xa, YAxis ya, Y2Axis y2a, Grid gd, XYLabel lb, Title tl, List<Series> series )
        {
            DrawBackground( g, cs );

            gd.Draw( g, this, xa, ya, y2a );

            DrawAllSeries( g, xa, ya, y2a, series );

            DrawAxes( g, cs, xa, ya, y2a, lb );

            DrawForeground( g, cs );

            DrawLabels( g, xa, ya, y2a, lb, tl );
        }


        public PointF Point2D( PointF pt, Axis xa, Axis ya )
        {
            var aPoint = new PointF
            {
                X = PlotRect.X + ( pt.X - xa.Min ) * PlotRect.Width / ( xa.Max - xa.Min ),
                Y = PlotRect.Bottom - ( pt.Y - ya.Min ) * PlotRect.Height / ( ya.Max - ya.Min )
            };

            return aPoint;
        }

        #endregion

        #region Properties

        // The size for the chart area.
        public Rectangle ChartRect { get; set; }

        // The size for the plot area.
        public Rectangle PlotRect { get; private set; }

        #endregion

    }
}
