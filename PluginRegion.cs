using System;
using System.Windows.Forms;

using SMath.Manager;
using SMath.Controls;
using SMath.Drawing;

using JXCharts;

using XYPlotPluginSeq.Properties;

namespace XYPlotPluginSeq
{
    public class PluginRegion : IPluginCustomRegion
    {

        #region Public methods

        public RegionBase CreateNew( SessionProfile sessionProfile ) => new XYPlotSeq( sessionProfile );


        public MenuButton[] GetContextMenuItems( MenuContext context )
        {
            var mbFormat = new MenuButton( context.SessionProfile.CurrentLanguage.Abbr == "RUS" ? "Формат..." : "Format..." )
            {
                Action = args => ( ( XYPlotSeq ) args.CurrentRegion ).ShowFormatDialog( EnChartElement.Chart )
            };

            return new[] { mbFormat };
        }


        public MenuButton[] GetMenuItems( SessionProfile sessionProfile )
        {
            var xyPlotSeq = new MenuButton( sessionProfile.CurrentLanguage.Abbr == "RUS" ? "X-Y График" : "X-Y Plot Seq Marching" )
            {
                Behavior = MenuButtonBehavior.DisableWhenInsideRegion,
                Icon = Graphics.Specifics.BitmapFromNativeImage( Resources.menuIcon ),
                Action = args => args.CurrentRegion = CreateNew( args.SessionProfile )
            };

            var plotMenu = new MenuButton( sessionProfile.CurrentLanguage.StringsGUI[ 151 ] );

            plotMenu.AppendChild( xyPlotSeq );

            return new[] { plotMenu };
        }


        public void Initialize() => DragAndDropFileTypes = new DragAndDropFileType[0];


        public void Dispose()
        {
        }

        #endregion

        #region Properties

        public string TagName { get; } = "xyplotSeq";

        public Type RegionType => typeof( XYPlotSeq );

        public DragAndDropFileType[] DragAndDropFileTypes { get; private set; }

        public string[] SupportedClipboardFormats => new[] { DataFormats.UnicodeText };

        #endregion

    }
}
