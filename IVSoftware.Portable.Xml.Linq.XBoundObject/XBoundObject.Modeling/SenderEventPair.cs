using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    public class SenderEventPair
    {
        public static implicit operator EventArgs(SenderEventPair @this)
            => @this.e;
        public SenderEventPair(object sender, EventArgs e)
        {
            this.sender = sender;
            SenderModel = sender as XElement;
            this.e = e;
            PropertyChangedEventArgs = e as PropertyChangedEventArgs;
            NotifyCollectionChangedEventArgs = e as NotifyCollectionChangedEventArgs;
            XObjectChangeEventArgs = e as XObjectChangeEventArgs;
            switch(e)
            {
                case PropertyChangingEventArgs eChanging:
                    PropertyName = eChanging.PropertyName;
                    break;
                case PropertyChangedEventArgs eChanged:
                    PropertyName = eChanged.PropertyName;
                    break;
            }
        }
        public object sender { get; }
        public EventArgs e { get; }
        public PropertyChangedEventArgs PropertyChangedEventArgs { get; }
        public NotifyCollectionChangedEventArgs NotifyCollectionChangedEventArgs { get; }
        public XObjectChangeEventArgs XObjectChangeEventArgs { get; }
        public string PropertyName { get; }
        public XElement SenderModel { get; }
        public XElement OriginModel
        {
            get
            {
                if (_originModel is null)
                {
                    _originModel =
                        SenderModel?
                        .AncestorsAndSelf()
                        .Last() ?? throw new NullReferenceException();
                }
                return _originModel;
            }
        }
        XElement _originModel = null;
        public override string ToString()
        {
            if(PropertyChangedEventArgs != null)
            {

            }
            else if(NotifyCollectionChangedEventArgs != null)
            {

            }
            else if(XObjectChangeEventArgs != null)
            {
                return $"[{sender.GetType().Name}.{XObjectChangeEventArgs.ObjectChange}] {sender}";
            }
            return base.ToString();
        }
    }
}
