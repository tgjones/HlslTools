using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace HlslTools.VisualStudio.Navigation.NavigateTo
{
    internal sealed class NavigateToItemDisplay : INavigateToItemDisplay2
    {
        private readonly Action _navigateCallback;

        public Icon Glyph { get; }
        public string Name { get; }
        public string AdditionalInformation { get; }
        public string Description { get; }
        public ReadOnlyCollection<DescriptionItem> DescriptionItems { get; }

        public NavigateToItemDisplay(Icon glyph, string name, string additionalInformation, string description, Action navigateCallback)
        {
            _navigateCallback = navigateCallback;
            Glyph = glyph;
            Name = name;
            AdditionalInformation = additionalInformation;
            Description = description;
            DescriptionItems = null;
        }

        public void NavigateTo()
        {
            _navigateCallback();
        }

        public int GetProvisionalViewingStatus()
        {
            return (int) __VSPROVISIONALVIEWINGSTATUS.PVS_Enabled;
        }

        public void PreviewItem()
        {
            _navigateCallback();
        }
    }
}