using System;

namespace ShaderTools.VisualStudio.Hlsl.Options.ViewModels
{
    internal sealed class Option<T>
    {
        private readonly Func<T> _getValue;
        private readonly Action<T> _setValue;

        public string Name { get; }

        public T Value
        {
            get { return _getValue(); }
            set { _setValue(value); }
        }

        public Option(string name, Func<T> getValue, Action<T> setValue)
        {
            Name = name;
            _getValue = getValue;
            _setValue = setValue;
        }
    }
}