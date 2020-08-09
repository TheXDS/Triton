using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TheXDS.Triton.Ui.Exceptions
{
    [Serializable]
    public class UnresolvableViewModelException : Exception
    {
        public UnresolvableViewModelException() { }
        public UnresolvableViewModelException(string message) : base(message) { }
        public UnresolvableViewModelException(string message, Exception inner) : base(message, inner) { }
        protected UnresolvableViewModelException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
