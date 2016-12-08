using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ShaderTools.VisualStudio.Core.Options.Views
{
    internal abstract class OptionsControlBase : UserControl
    {
        protected void BindToOption(CheckBox checkBox, object source, string propertyName)
        {
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, new System.Windows.Data.Binding
            {
                Source = source,
                Path = new PropertyPath(propertyName)
            });
        }

        public virtual void Close()
        {
            
        }
    }
}