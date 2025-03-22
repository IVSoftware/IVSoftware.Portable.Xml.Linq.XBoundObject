using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSTestProject
{
    enum DiscoveryDemo
    {
        Home,
        Scan,
        Settings,
    }

    enum Scan
    {
        QRCode,
        Barcode,
    }

    enum Settings
    {
        Apply,
        Cancel,
    }

    namespace Deep
    {
        /// <summary>
        /// By default, discovery WILL NOT reach this.
        /// </summary>
        enum Apply
        {
            All,
            Selected,
        }
    }
    enum NodeType { drive, folder, file, }

    enum NotFoundTypeForTest { na }
}
