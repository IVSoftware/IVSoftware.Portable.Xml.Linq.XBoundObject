using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq
{
    public enum ModifyMappingAction
    {
        IDtoXEL,
        XELtoID
    }

    public class BeforeModifyMappingCancelEventArgs : CancelEventArgs
    {
        public BeforeModifyMappingCancelEventArgs(Enum key, XElement oldXEL, XElement newXEL)
        {
            Action = ModifyMappingAction.IDtoXEL;
            OldID = key;
            OldXEL = oldXEL;
            NewXEL = newXEL;
        }

        public BeforeModifyMappingCancelEventArgs(XElement key, Enum oldID, Enum newID)
        {
            Action = ModifyMappingAction.XELtoID;
            NewXEL = key;
            OldID = oldID;
            NewID = newID;
        }
        public ModifyMappingAction Action { get; }
        public XElement OldXEL { get; }
        public XElement NewXEL { get; }
        public Enum OldID { get; }
        public Enum NewID { get; }
    }
}
