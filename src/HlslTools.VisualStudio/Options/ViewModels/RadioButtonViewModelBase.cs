namespace HlslTools.VisualStudio.Options.ViewModels
{
    internal abstract class RadioButtonViewModelBase : NotifyPropertyChangedBase
    {
        private readonly OptionsPreviewViewModelBase _info;
        internal readonly string Preview;
        private bool _isChecked;

        public string Description { get; private set; }
        public string GroupName { get; private set; }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }

            set
            {
                SetProperty(ref _isChecked, value);

                if (_isChecked)
                {
                    SetOptionAndUpdatePreview(_info, Preview);
                }
            }
        }

        protected RadioButtonViewModelBase(string description, string preview, OptionsPreviewViewModelBase info, bool isChecked, string group)
        {
            Description = description;
            this.Preview = preview;
            _info = info;
            this.GroupName = group;

            SetProperty(ref _isChecked, isChecked);
        }

        internal abstract void SetOptionAndUpdatePreview(OptionsPreviewViewModelBase info, string preview);
    }

    internal sealed class RadioButtonViewModel<TOption> : RadioButtonViewModelBase
    {
        private readonly Option<TOption> _option;
        private readonly TOption _value;

        public RadioButtonViewModel(string preview, string group, TOption value, Option<TOption> option, OptionsPreviewViewModelBase info)
            : base(option.Name, preview, info, isChecked: option.Value.Equals(value), group: group)
        {
            _value = value;
            _option = option;
        }

        internal override void SetOptionAndUpdatePreview(OptionsPreviewViewModelBase info, string preview)
        {
            info.SetOptionAndUpdatePreview(_value, _option, preview);
        }
    }
}