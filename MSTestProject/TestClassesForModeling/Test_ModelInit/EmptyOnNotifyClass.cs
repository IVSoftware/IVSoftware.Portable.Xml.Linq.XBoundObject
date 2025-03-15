using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoundObjectMSTest.TestClassesForModeling.Test_ModelInit
{
    class EmptyOnNotifyClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
