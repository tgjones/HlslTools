using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ShaderTools.VisualStudio.Core.Options.ViewModels;

namespace ShaderTools.VisualStudio.Core.Options.Views
{
    internal partial class OptionsPreviewControl : OptionsControlBase
    {
        public OptionsPreviewViewModelBase ViewModel;

        public OptionsPreviewControl(Func<OptionsPreviewViewModelBase> createViewModel)
        {
            InitializeComponent();

            ViewModel = createViewModel();

            // Use the first item's preview.
            var firstItem = this.ViewModel.Items.OfType<CheckBoxOptionViewModel>().First();
            ViewModel.SetOptionAndUpdatePreview(firstItem.IsChecked, firstItem.Option, firstItem.GetPreview());

            DataContext = ViewModel;
        }

        private void Options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            var checkbox = listView.SelectedItem as CheckBoxOptionViewModel;
            if (checkbox != null)
            {
                ViewModel.UpdatePreview(checkbox.GetPreview());
            }

            var radioButton = listView.SelectedItem as RadioButtonViewModelBase;
            if (radioButton != null)
            {
                ViewModel.UpdatePreview(radioButton.Preview);
            }
        }

        private void Options_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                var listView = (ListView)sender;
                var checkBox = listView.SelectedItem as CheckBoxOptionViewModel;
                if (checkBox != null)
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                    e.Handled = true;
                }

                var radioButton = listView.SelectedItem as RadioButtonViewModelBase;
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                    e.Handled = true;
                }
            }
        }

        public override void Close()
        {
            base.Close();

            ViewModel?.Dispose();
        }
    }
}
