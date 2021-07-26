using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast;

namespace Example
{
    class CustomContext : ToastContext
    {
        public readonly string Value;

        public CustomContext(string value)
        {
            Value = value;
        }
    }
}
