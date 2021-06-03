using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Model
{
    public interface IClonable<T>
    {
        public T Clone();
    }
}
