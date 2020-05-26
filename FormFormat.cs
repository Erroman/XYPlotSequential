using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using SMath.Manager;

using JXCharts;


namespace XYPlotPluginSeq 
{
    public partial class FormFormat : Form
    {

        #region Private fields

        private EnChartElement _element;
        private XYPlot _region;

        #endregion

        #region Constructors

        public FormFormat( XYPlot region, SessionProfile sessionProfile, EnChartElement element ) 
        {
            InitializeComponent();

            _region = region;
            _element = element;

            var cult = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = new CultureInfo( cult.IetfLanguageTag )
            {
                NumberFormat = { NumberDecimalSeparator = sessionProfile.DecimalSymbol.ToString() }
            };

            MyCollectionEditor.MyPropertyValueChanged += propertyGrid1_PropertyValueChanged;
        }

        #endregion

        #region Events handlers

        private void button2_Click( object sender, EventArgs e ) 
        {
            Close();
        }


        private void FormFormat_Load( object sender, EventArgs e )
        {
            var canv = _region.GetCanvas();

            try
            {
                Size = GlobalConfig.Settings.FormFormatSettings.Size;
                Location = GlobalConfig.Settings.FormFormatSettings.Location;
                WindowState = GlobalConfig.Settings.FormFormatSettings.State;

                propertyGrid1.BrowsableAttributes = new AttributeCollection( new CategoryAttribute( "Appearance" ) );

                switch ( _element )
                {
                    case EnChartElement.XAxis:
                        propertyGrid1.SelectedObject = canv.XAxis;
                        break;

                    case EnChartElement.YAxis:
                        propertyGrid1.SelectedObject = canv.YAxis;
                        break;

                    case EnChartElement.Y2Axis:
                        propertyGrid1.SelectedObject = canv.Y2Axis;
                        break;

                    case EnChartElement.Chart:
                        propertyGrid1.SelectedObject = canv;
                        break;

                    default:
                        propertyGrid1.SelectedObject = canv;
                        break;
                }
            }
            catch
            {
                propertyGrid1.SelectedObject = canv;
            }
        }


        private void FormFormat_FormClosing( object sender, FormClosingEventArgs e ) 
        {
            try 
            {
                GlobalConfig.Settings.FormFormatSettings.State = WindowState;

                if ( WindowState == FormWindowState.Normal ) 
                {
                    GlobalConfig.Settings.FormFormatSettings.Size = Size;
                    GlobalConfig.Settings.FormFormatSettings.Location = Location;
                } 
                else 
                {
                    GlobalConfig.Settings.FormFormatSettings.Size = RestoreBounds.Size;
                    GlobalConfig.Settings.FormFormatSettings.Location = RestoreBounds.Location;
                }
            }
            catch {}

            GlobalConfig.Settings.Save();
        }


        private void ExpandAllToolStripMenuItem_Click( object sender, EventArgs e ) 
        {
            propertyGrid1.ExpandAllGridItems();
        }


        private void CollapseAllToolStripMenuItem_Click( object sender, EventArgs e ) 
        {
            propertyGrid1.CollapseAllGridItems();
        }


        private void propertyGrid1_PropertyValueChanged( object s, PropertyValueChangedEventArgs e )
        {
            var names = new[] { "XMin", "XMax", "YMin", "YMax", "Y2Min", "Y2Max", "Points", "SourceType" };

            if ( names.Contains( e.ChangedItem.Label ) )
            {
                _region.RequestEvaluation( true );
                _region.Refresh();
                _region.Update();
            }
            else
            {
                _region.Refresh();
                _region.Update();                
            }
        }

        #endregion

    }
}
