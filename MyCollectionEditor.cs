using System;
using System.ComponentModel.Design;
using System.Windows.Forms;


namespace XYPlotPlugin 
{
    public class MyCollectionEditor : CollectionEditor
    {
        private CollectionForm _form;

        // Define a static event to expose the inner PropertyGrid's
        // PropertyValueChanged event args...
        public delegate void MyPropertyValueChangedEventHandler( object sender, PropertyValueChangedEventArgs e );
        public static event MyPropertyValueChangedEventHandler MyPropertyValueChanged;

        // Inherit the default constructor from the standard
        // Collection Editor...
        public MyCollectionEditor( Type type ) : base( type ) {}

        // Override this method in order to access the containing user controls
        // from the default Collection Editor form or to add new ones...
        protected override CollectionForm CreateCollectionForm() 
        {
            // Getting the default layout of the Collection Editor...
            _form = base.CreateCollectionForm();

            _form.Load += Form_Load;
            _form.FormClosing += Form_FormClosing;

            var tlpLayout = _form.Controls[0] as TableLayoutPanel;

            // Get a reference to the inner PropertyGrid and hook
            // an event handler to it.
            if ( tlpLayout?.Controls[5] is PropertyGrid ) 
            {
                var propertyGrid = ( PropertyGrid ) tlpLayout.Controls[5];

                propertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
            }

            return _form;
        }

        private void Form_Load( object sender, EventArgs e )
        {
            try
            {
                _form.Size = GlobalConfig.Settings.FormCollection.Size;
                _form.Location = GlobalConfig.Settings.FormCollection.Location;
                _form.WindowState = GlobalConfig.Settings.FormCollection.State;
            }
            catch
            {
            }
        }

        private void Form_FormClosing( object sender, FormClosingEventArgs e )
        {
            try
            {
                GlobalConfig.Settings.FormCollection.State = _form.WindowState;

                if ( _form.WindowState == FormWindowState.Normal )
                {
                    GlobalConfig.Settings.FormCollection.Size = _form.Size;
                    GlobalConfig.Settings.FormCollection.Location = _form.Location;
                }
                else
                {
                    GlobalConfig.Settings.FormCollection.Size = _form.RestoreBounds.Size;
                    GlobalConfig.Settings.FormCollection.Location = _form.RestoreBounds.Location;
                }
            }
            catch { }

            GlobalConfig.Settings.Save();
        }

        void propertyGrid_PropertyValueChanged( object sender, PropertyValueChangedEventArgs e )
        {
            // Fire our customized collection event...
            MyPropertyValueChanged?.Invoke( this, e );
        }
    }
}
