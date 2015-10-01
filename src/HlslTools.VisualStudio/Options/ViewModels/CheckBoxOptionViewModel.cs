namespace HlslTools.VisualStudio.Options.ViewModels
{
    internal class CheckBoxOptionViewModel : NotifyPropertyChangedBase
    {
        private readonly OptionsPreviewViewModelBase _info;

        internal string GetPreview()
        {
            return _isChecked ? _truePreview : _falsePreview;
        }

        private bool _isChecked;
        private readonly string _truePreview;
        private readonly string _falsePreview;

        public Option<bool> Option { get; }
        public string Description { get; set; }

        public CheckBoxOptionViewModel(Option<bool> option, string preview, OptionsPreviewViewModelBase info)
        {
            Option = option;
            Description = option.Name;
            _truePreview = preview;
            _falsePreview = preview;
            _info = info;
            SetProperty(ref _isChecked, option.Value);
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }

            set
            {
                SetProperty(ref _isChecked, value);
                _info.SetOptionAndUpdatePreview(_isChecked, Option, GetPreview());
            }
        }
    }
}