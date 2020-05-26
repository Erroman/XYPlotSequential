 using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Design;

using SMath.Controls;
using SMath.Math;
using SMath.Manager;
using SMath.Drawing;

using XYPlotPluginSeq;


namespace JXCharts 
{
    public enum EnChartElement
    {
        None,
        XAxis,
        YAxis,
        Y2Axis,
        Chart    
    }

    class SeriesListTypeConverter : TypeConverter
    {
        public override bool CanConvertTo( ITypeDescriptorContext context, Type destType ) => destType == typeof( string );

        public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destType ) => "< List... >";
    }


    public class Chart2D : RegionEvaluable 
    {

        #region Private fields

        private string _name;
        private ArrayList _interactions;
        private KeyEventOptions _lastKeyEventArgs;

        #endregion

        #region Constructors

        public Chart2D( SessionProfile sessionProfile ) : base( sessionProfile ) 
        {
            Initialize();

            Size = new Size( 300, 200 );            
        }


        public Chart2D( Chart2D region ) : base( region )
        {            
            Initialize();

            Points = region.Points;
            Name = region.Name;

            PropertiesSource = new PropertiesSource( region.PropertiesSource.Index, region.PropertiesSource.SourceType );

            Title = region.Title.Clone();
            Grid = region.Grid.Clone();
            XAxis = region.XAxis.Clone();
            YAxis = region.YAxis.Clone();
            Y2Axis = region.Y2Axis.Clone();
            XYLabel = region.XYLabel.Clone();
            Legend = region.Legend.Clone();

            foreach ( var ds in region.Series )
            {
                Series.Add( ds.Clone() );
            }

            Size = new Size( region.Size.Width, region.Size.Height );
        }

        #endregion

        #region Private methods

        private void Initialize()
        {
            _name = "XYPlotModified";
            Points = 100;

            _lastKeyEventArgs = new KeyEventOptions();

            Font = new FontInfo( "Arial", 10, FontfaceStyle.Regular );

            PropertiesSource = new PropertiesSource( 1, PropertiesSource.SourceTypeEnum.PropertyGrid );

            ChartArea = new ChartArea { ChartRect = new Rectangle( new Point( 0, 0 ), Size ) };
            ChartStyle = new ChartStyle();
            Grid = new Grid();            
            Legend = new Legend();
            XAxis = new XAxis();
            YAxis = new YAxis();
            Y2Axis = new Y2Axis();
            XYLabel = new XYLabel();
            Title = new Title();

            Series = new List<Series>();

            _interactions = new ArrayList
            {
                new Interactions.HorizontalDrag(),
                new Interactions.VerticalDrag(),
                new Interactions.MouseZoom()
            };
        }


        public void SeriesFromXml( XmlReader reader )
        {
            reader.MoveToElement();

            // <traces>...</traces>
            if ( !reader.IsEmptyElement )
            {
                var name = reader.Name;

                while ( reader.Read() )
                {
                    reader.MoveToContent();

                    if ( reader.NodeType == XmlNodeType.Element )
                    {
                        if ( reader.Name == "trace" )
                        {
                            var ds = new Series();

                            ds.FromXml( reader );

                            Series.Add( ds );
                        }
                    }

                    if ( reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals( name ) ) break;
                }
            }
        }


        public void SeriesToXml( XmlWriter writer )
        {
            writer.WriteStartElement( "traces" );

            foreach ( var ds in Series )
            {
                writer.WriteStartElement( "trace" );
                ds.ToXml( writer );
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        #endregion

        #region Public methods

        public override RegionBase Clone() => new Chart2D( this );


        public void Refresh()
        {
            var g = System.Drawing.Graphics.FromImage( new Bitmap( 1, 1 ) );

            ChartArea.CalcPlotArea( g, XAxis, YAxis, Y2Axis, Grid, XYLabel, Title );
        }


        public void Update() => Invalidate();


        protected override void Resize()
        {
            var rect = new Rectangle( new Point( 0, 0 ), new Size( Size.Width, Size.Height ) );

            ChartArea.ChartRect = rect;

            Refresh();
        }


        /// <summary>
        /// Adds and interaction to the plotsurface that adds functionality that responds 
        /// to a set of mouse / keyboard events. 
        /// </summary>
        /// <param name="i">the interaction to add.</param>
        public void AddInteraction( Interactions.Interaction i ) => _interactions.Add(i);


        /// <summary>
        /// Remove a previously added interaction.
        /// </summary>
        /// <param name="i">interaction to remove</param>
        public void RemoveInteraction( Interactions.Interaction i ) => _interactions.Remove(i);


        public override void ToXml( StorageWriter storage, FileParsingContext parsingContext )
        {
            storage.Properties.SetInt32( "width", Size.Width );
            storage.Properties.SetInt32( "height", Size.Height );

            storage.Properties.SetInt32( "points", Points );
            storage.Properties.SetString( "name", Name );

            var writer = storage.GetXmlWriter();

            ChartStyle.ToXml( writer );
            PropertiesSource.ToXml( writer );
            Grid.ToXml( writer );
            XAxis.ToXml( writer );
            YAxis.ToXml( writer );
            Y2Axis.ToXml( writer );
            Title.ToXml( writer );
            XYLabel.ToXml( writer );
            Legend.ToXml( writer );
            SeriesToXml( writer );
        }


        public override void FromXml( StorageReader storage, FileParsingContext parsingContext )
        {
            Points = storage.Properties.GetInt32( "points", Points );
            Name = storage.Properties.GetString( "name", Name );

            var width = storage.Properties.GetInt32( "width", 200 );
            var height = storage.Properties.GetInt32( "height", 200 );

            var reader = storage.GetXmlReader();

            ChartStyle.FromXml( reader );
            
            reader.MoveToElement();

            // <xyplot>...<input>...</input></xyplot>
            if ( !reader.IsEmptyElement )
            {
                var name = reader.Name;

                while ( reader.Read() ) 
                {
                    reader.MoveToContent();

                    if ( reader.NodeType == XmlNodeType.Element )
                    {
                        if ( reader.Name == typeof( ChartStyle ).Name.ToLower() ) ChartStyle.FromXml( reader );

                        if ( reader.Name == typeof( PropertiesSource ).Name.ToLower() ) PropertiesSource.FromXml( reader );

                        if ( reader.Name == typeof( Grid ).Name.ToLower() ) Grid.FromXml( reader );

                        // FIXME: remove all *axes.
                        if ( reader.Name == typeof( XAxis ).Name.ToLower() || reader.Name == "xaxes" ) XAxis.FromXml( reader );

                        if ( reader.Name == typeof( YAxis ).Name.ToLower() || reader.Name == "yaxes" ) YAxis.FromXml( reader );

                        if ( reader.Name == typeof( Y2Axis ).Name.ToLower() || reader.Name == "y2axes" ) Y2Axis.FromXml( reader );

                        if ( reader.Name == typeof( Title ).Name.ToLower() || reader.Name == "title2d" ) Title.FromXml( reader );

                        if ( reader.Name == typeof( XYLabel ).Name.ToLower() ) XYLabel.FromXml( reader );

                        if ( reader.Name == typeof( Legend ).Name.ToLower() ) Legend.FromXml( reader );

                        if ( reader.Name == "traces" ) SeriesFromXml( reader );

                        // Special case.
                        if ( reader.Name == "input" ) break;
                    }

                    if ( reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals( name ) ) break;
                }
            }

            Size = new Size( width, height );
        }


        public override bool Copy( IClipboardManager clipboard ) => true;


        public override bool Paste( IClipboardManager clipboard ) => true;


        public override RegionSelectionStatus GetSelectionStatus() => throw new NotImplementedException();


        public void DoMouseDown( MouseEventOptions e )
        {
            var dirty = false;

            foreach ( Interactions.Interaction i in _interactions )
            {
                i.DoMouseDown( e, this );

                dirty |= i.DoMouseDown( e, this );
            }

            if ( dirty )
            {
                Update();
            }
        }


        public void DoMouseMove( MouseEventOptions e, RegionEvaluable ctr )
        {
            var dirty = false;

            foreach ( Interactions.Interaction i in _interactions )
            {
                i.DoMouseMove( e, ctr, _lastKeyEventArgs );

                dirty |= i.DoMouseMove( e, ctr, _lastKeyEventArgs );
            }

            if ( dirty )
            {
                Update();
            }
        }


        public void DoMouseUp( MouseEventOptions e, RegionEvaluable ctr )
        {
            var dirty = false;

            var localInteractions = ( ArrayList ) _interactions.Clone();

            foreach ( Interactions.Interaction i in localInteractions )
            {
                dirty |= i.DoMouseUp( e, ctr );
            }

            if ( dirty )
            {
                Update();
            }
        }


        public void DoMouseWheel( MouseEventOptions e, RegionEvaluable ctr, int wheelDelta )
        {
            var dirty = false;

            foreach ( Interactions.Interaction i in _interactions )
            {
                dirty |= i.DoMouseWheel( e, ctr, _lastKeyEventArgs, wheelDelta );
            }

            if ( dirty )
            {
                Refresh();
                Update();
            }
        }

        #endregion

        #region Events handlers

        public override void OnEvaluation( Store store )
        {
        }


        public override void OnKeyDown( KeyEventOptions e ) => _lastKeyEventArgs = e;


        public override void OnKeyUp( KeyEventOptions e ) => _lastKeyEventArgs = e;


        public override void OnMouseDown( MouseEventOptions e )
        {
            DoMouseDown(e);

            base.OnMouseDown(e);
        }


        public override void OnMouseMove( MouseEventOptions e ) 
        {
            DoMouseMove( e, this );

            base.OnMouseMove(e);
        }


        public override void OnMouseUp( MouseEventOptions e ) 
        {
            DoMouseUp( e, this );

            base.OnMouseUp(e);   
        }


        public override bool OnMouseWheel( MouseEventOptions e, int wheelDelta ) 
        {
            DoMouseWheel( e, this, wheelDelta );

            return base.OnMouseWheel( e, wheelDelta );
        }


        public override void OnPaint( PaintEventOptions e )
        {
            if ( InEvaluation ) return;

            var g = e.Graphics.Unwrap<System.Drawing.Graphics>();

            var posX = e.ClipRectangle.Location.X;
            var posY = e.ClipRectangle.Location.Y;

            var state = g.Save();

            g.TranslateTransform( posX, posY );

            if ( Focused && BackColor != Color.Transparent )
            {
                g.FillRectangle( new SolidBrush( BackColor ), ChartArea.ChartRect );
            }

            ChartArea.Draw( g, ChartStyle, XAxis, YAxis, Y2Axis, Grid, XYLabel, Title, Series );
            Legend.Draw( g, ChartArea, Series );

            g.Restore( state );
        }

        #endregion

        #region Properties

        public override Size Size
        {
            get => base.Size;
            set
            {
                var w = value.Width < 200 ? 200 : value.Width;
                var h = value.Height < 150 ? 150 : value.Height;

                base.Size = new Size( w, h );

                Refresh();
            }
        }

        [Browsable( true )]
        [Description( "Number of points." ),
        Category( "Appearance" )]
        public int Points { get; set; }

        [Browsable( true )]
        [Description( "Properties source data." ),
        DisplayName( "Properties source" ),
        Category( "Appearance" )]
        public PropertiesSource PropertiesSource { get; set; }

        [Browsable( true )]
        [Description( "Chart style." ),
         DisplayName( "Style" ),
         Category( "Appearance" )]
        public ChartStyle ChartStyle { get; set; }

        public ChartArea ChartArea { get; set; }

        [Browsable( true )]
        [DisplayName( "X-Axis" ),
        Category( "Appearance" )]
        public XAxis XAxis { get; set; }

        [Browsable( true )]
        [DisplayName( "Y-Axis" ),
        Category( "Appearance" )]
        public YAxis YAxis { get; set; }

        [Browsable( true )]
        [DisplayName( "Y2-Axis" ),
        Category( "Appearance" )]
        public Y2Axis Y2Axis { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public Grid Grid { get; set; }

        [Browsable( true )]
        [DisplayName( "Labels" ),
        Category( "Appearance" )]
        public XYLabel XYLabel { get; set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public Title Title { get; set; }

        [Browsable(true)]
        [Category("Appearance")]
        public string Name
        {
            get => _name;
            set
            {
                var provider = CodeDomProvider.CreateProvider( "C#" );

                if ( provider.IsValidIdentifier( value ) )
                {
                    _name = value;
                }
            }
        }

        [Browsable( true )]
        [DisplayName( "Traces" ), Category( "Appearance" )]
        [Editor( typeof( MyCollectionEditor ), typeof( UITypeEditor ) )]
        [TypeConverter( typeof( SeriesListTypeConverter ) )]
        public List<Series> Series { get; private set; }

        [Browsable( true )]
        [Category( "Appearance" )]
        public Legend Legend { get; set; }

        #endregion

        #region Class Interactions

        /// <summary>
        /// Encapsulates a number of separate "Interactions". An interaction is basically 
        /// a set of handlers for mouse and keyboard events that work together in a 
        /// specific way. 
        /// </summary>
        public abstract class Interactions
        {

            #region Class Interaction

            /// <summary>
            /// Base class for an interaction. All methods are virtual. Not abstract as not all interactions
            /// need to use all methods. Default functionality for each method is to do nothing. 
            /// </summary>
            public class Interaction
            {
                /// <summary>
                /// Handler for this interaction if a mouse down event is received.
                /// </summary>
                /// <param name="e">event args</param>
                /// <param name="ctr">reference to the control</param>
                /// <returns>true if plot surface needs refreshing.</returns>
                public virtual bool DoMouseDown( MouseEventOptions e, RegionEvaluable ctr ) { return false; }

                /// <summary>
                /// Handler for this interaction if a mouse up event is received.
                /// </summary>
                /// <param name="e">event args</param>
                /// <param name="ctr">reference to the control</param>
                /// <returns>true if plot surface needs refreshing.</returns>
                public virtual bool DoMouseUp( MouseEventOptions e, RegionEvaluable ctr ) { return false; }

                /// <summary>
                /// Handler for this interaction if a mouse move event is received.
                /// </summary>
                /// <param name="e">event args</param>
                /// <param name="ctr">reference to the control</param>
                /// <param name="lastKeyEventArgs"></param>
                /// <returns>true if plot surface needs refreshing.</returns>
                public virtual bool DoMouseMove( MouseEventOptions e, RegionEvaluable ctr, KeyEventOptions lastKeyEventArgs ) { return false; }

                /// <summary>
                /// Handler for this interaction if a mouse move event is received.
                /// </summary>
                /// <param name="e">event args</param>
                /// <param name="ctr">reference to the control</param>
                /// <param name="lastKeyEventArgs"></param>
                /// <param name="wheelDelta"></param>
                /// <returns>true if plot surface needs refreshing.</returns>
                public virtual bool DoMouseWheel( MouseEventOptions e, RegionEvaluable ctr, KeyEventOptions lastKeyEventArgs, int wheelDelta ) { return false; }

                /// <summary>
                /// Handler for this interaction if a mouse Leave event is received.
                /// </summary>
                /// <param name="e">event args</param>
                /// <param name="ctr">reference to the control</param>
                /// <returns>true if the plot surface needs refreshing.</returns>
                public virtual bool DoMouseLeave( EventArgs e, RegionEvaluable ctr ) { return false; }

                /// <summary>
                /// Handler for this interaction if a paint event is received.
                /// </summary>
                /// <param name="pe">paint event args</param>
                /// <param name="width"></param>
                /// <param name="height"></param>
                public virtual void DoPaint( PaintEventOptions pe, int width, int height ) { }
            }

            #endregion

            #region HorizontalDrag

            /// <summary>
            /// 
            /// </summary>
            public class HorizontalDrag : Interaction
            {

                #region Private fields

                private bool _dragInitiated;
                private PointF _lastPoint = new Point( -1, -1 );

                // this is the condition for an unset point
                private Point _unset = new Point( -1, -1 );

                #endregion

                /// <summary>
                /// 
                /// </summary>
                /// <param name="e"></param>
                /// <param name="ctr"></param>
                public override bool DoMouseDown( MouseEventOptions e, RegionEvaluable ctr )
                {
                    var chart = ( Chart2D ) ctr;

                    var reg = new Region( chart.ChartArea.PlotRect );

                    if ( reg.IsVisible( e.Location ) )
                    {
                        _dragInitiated = true;
                        _lastPoint = e.Location;
                    }

                    return false;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="e"></param>
                /// <param name="ctr"></param>
                /// <param name="lastKeyEventArgs"></param>
                public override bool DoMouseMove( MouseEventOptions e, RegionEvaluable ctr, KeyEventOptions lastKeyEventArgs )
                {
                    var chart = ( Chart2D ) ctr;

                    if ( e.Button == MouseInputButtons.Left && _dragInitiated && chart.PropertiesSource.SourceType != PropertiesSource.SourceTypeEnum.Sheet )
                    {
                        var scaleX = ( chart.XAxis.Max - chart.XAxis.Min ) / chart.Size.Width;

                        var diffX = scaleX * ( e.X - _lastPoint.X );

                        chart.XAxis.Min -= diffX;
                        chart.XAxis.Max -= diffX;

                        _lastPoint = e.Location;

                        return true;
                    }

                    return false;
                }


                /// <summary>
                /// 
                /// </summary>
                /// <param name="e"></param>
                /// <param name="ctr"></param>
                public override bool DoMouseUp( MouseEventOptions e, RegionEvaluable ctr )
                {
                    if ( e.Button == MouseInputButtons.Left && _dragInitiated )
                    {
                        _lastPoint = _unset;
                        _dragInitiated = false;
                    }

                    return false;
                }
            }

            #endregion

            #region VerticalDrag

            /// <summary>
            /// 
            /// </summary>
            public class VerticalDrag : Interaction
            {

                #region Private fields

                private bool _dragInitiated;
                private PointF _lastPoint = new PointF( -1, -1 );

                // this is the condition for an unset point
                private Point _unset = new Point( -1, -1 );

                #endregion

                /// <summary>
                /// 
                /// </summary>
                /// <param name="e"></param>
                /// <param name="ctr"></param>
                public override bool DoMouseDown( MouseEventOptions e, RegionEvaluable ctr )
                {
                    var chart = ( Chart2D ) ctr;

                    var reg = new Region( chart.ChartArea.PlotRect );

                    // Проверяем нахождение координат точки курсора в области графика.
                    if ( reg.IsVisible( e.Location ) )
                    {
                        _dragInitiated = true;
                        _lastPoint = e.Location;
                    }

                    return false;
                }


                /// <summary>
                /// 
                /// </summary>
                /// <param name="e"></param>
                /// <param name="ctr"></param>
                /// <param name="lastKeyEventArgs"></param>
                public override bool DoMouseMove( MouseEventOptions e, RegionEvaluable ctr, KeyEventOptions lastKeyEventArgs )
                {
                    var chart = ( Chart2D ) ctr;

                    if ( e.Button == MouseInputButtons.Left && _dragInitiated && chart.PropertiesSource.SourceType != PropertiesSource.SourceTypeEnum.Sheet )
                    {
                        var scaleY = ( chart.YAxis.Max - chart.YAxis.Min ) / chart.Size.Height;

                        var diffY = scaleY * ( e.Y - _lastPoint.Y );

                        chart.YAxis.Min += diffY;
                        chart.YAxis.Max += diffY;

                        _lastPoint = e.Location;

                        return true;
                    }

                    return false;
                }


                /// <summary>
                /// 
                /// </summary>
                /// <param name="e"></param>
                /// <param name="ctr"></param>
                public override bool DoMouseUp( MouseEventOptions e, RegionEvaluable ctr )
                {
                    if ( e.Button == MouseInputButtons.Left && _dragInitiated )
                    {
                        _lastPoint = _unset;
                        _dragInitiated = false;
                    }

                    return false;
                }
            }

            #endregion

            #region MouseZoom

            /// <summary>
            /// 
            /// </summary>
            public class MouseZoom : Interaction
            {
                public override bool DoMouseWheel( MouseEventOptions e, RegionEvaluable ctr, KeyEventOptions lastKeyEventArgs, int wheelDelta )
                {
                    var chart = ( Chart2D ) ctr;

                    var reg = new Region( chart.ChartArea.PlotRect );

                    if ( reg.IsVisible( e.Location ) && chart.PropertiesSource.SourceType != PropertiesSource.SourceTypeEnum.Sheet )
                    {
                        var scaleX = 0.1f * Math.Sign( wheelDelta ) * ( chart.XAxis.Max - chart.XAxis.Min );
                        var scaleY = 0.1f * Math.Sign( wheelDelta ) * ( chart.YAxis.Max - chart.YAxis.Min );

                        if ( lastKeyEventArgs.Shift && !lastKeyEventArgs.Control )
                        {
                            chart.XAxis.Min += scaleX;
                            chart.XAxis.Max -= scaleX;

                            chart.XAxis.Tick = chart.XAxis.DetermineLargeTickStep( chart.ChartArea.PlotRect.Width, out var shouldCullMiddle );
                        }
                        else if ( lastKeyEventArgs.Control && !lastKeyEventArgs.Shift )
                        {
                            chart.YAxis.Min += scaleY;
                            chart.YAxis.Max -= scaleY;

                            chart.YAxis.Tick = chart.YAxis.DetermineLargeTickStep( chart.ChartArea.PlotRect.Height, out var shouldCullMiddle );
                        }
                        else if ( !lastKeyEventArgs.Control && !lastKeyEventArgs.Shift )
                        {
                            chart.XAxis.Min += scaleX;
                            chart.XAxis.Max -= scaleX;

                            chart.XAxis.Tick = chart.XAxis.DetermineLargeTickStep( chart.ChartArea.PlotRect.Width, out var shouldCullMiddle );

                            chart.YAxis.Min += scaleY;
                            chart.YAxis.Max -= scaleY;

                            chart.YAxis.Tick = chart.YAxis.DetermineLargeTickStep( chart.ChartArea.PlotRect.Height, out shouldCullMiddle );
                        }

                        return true;
                    }
                    
                    return false;
                }
            }

            #endregion
        }

        #endregion

    }  
}
