using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    [Flags]
    public enum DiscoveryOption
    {
        IncludeValueTypeInstances = 0x01,
    }

    public delegate void PropertyChangedDelegate(object sender, PropertyChangedEventArgs e);
    public delegate void NotifyCollectionChangedDelegate(object sender, NotifyCollectionChangedEventArgs e);
    public delegate void XObjectChangeDelegate(object sender, XObjectChangeEventArgs e);
    public class DiscoveryContext
    {
        public XElement OriginModel { get; set; } = new XElement("model");
        public PropertyChangedDelegate PropertyChangedDelegate { get; set; } = null;
        public NotifyCollectionChangedDelegate NotifyCollectionChangedDelegate { get; set; } = null;
        public XObjectChangeDelegate XObjectChangeDelegate { get; set; } = null;

        public DiscoveryOption Options { get; set; } = 0;

        internal void RaiseElementAvailable(object sender, XElement element)
        {
            ElementAvailable?.Invoke(sender, new ElementAvailableEventArgs(element));
        }
        public event EventHandler<ElementAvailableEventArgs> ElementAvailable;
    }
    public class ElementAvailableEventArgs : EventArgs
    {
        public ElementAvailableEventArgs(XElement xel) => Element = xel;

        public XElement Element { get; }
    }
    public static class ModelingExtensions
    {
        public static XElement CreateModel(this object @this, DiscoveryContext context = null)
        {
            context = context ?? new DiscoveryContext();
            foreach (var xel in @this.ModelDescendantsAndSelf(context))
            {
                context?.RaiseElementAvailable(sender: @this, element: xel);
            }
            return context.OriginModel;
        }
        public static IEnumerable<XElement> ModelDescendantsAndSelf(this object @this, DiscoveryContext context = null)
        {
            XElement model = context?.OriginModel ?? new XElement(nameof(model));
            localDiscoverModel(@this, model);
            foreach (var xel in model.DescendantsAndSelf())
            {
                yield return xel;
            }

            void localDiscoverModel(object instance, XElement localModel, HashSet<object> visited = null)
            {
                visited = visited ?? new HashSet<object>();
                localModel.SetBoundAttributeValue(instance, name: nameof(instance));
                localRunRecursiveDiscovery(localModel);

                void localRunRecursiveDiscovery(XElement currentElement)
                {
                    object localInstance;
                    localInstance = (currentElement.Attribute(nameof(instance)) as XBoundAttribute)?.Tag;
                    if (localInstance != null && visited.Add(localInstance))
                    {
                        var pis = localInstance.GetType().GetProperties()
                            .Where(pi =>
                                pi.GetCustomAttribute<IgnoreNODAttribute>() is null &&
                                pi.GetIndexParameters().Length == 0);
                        foreach (var pi in pis.ToArray())
                        {
                            XElement member = new XElement(nameof(member));
                            member.SetAttributeValue(nameof(pi.Name).ToLower(), pi.Name);
                            currentElement.Add(member);
                            if (pi.GetValue(localInstance) is object childInstance)
                            {
                                if (childInstance is string ||
                                    childInstance is Enum ||
                                    childInstance is ValueType)
                                {
                                    if (context?.Options.HasFlag(DiscoveryOption.IncludeValueTypeInstances) == true)
                                    {
                                        member.SetBoundAttributeValue(childInstance, nameof(instance));
                                    }
                                    continue;
                                }
                                else
                                {
                                    member.SetBoundAttributeValue(childInstance, nameof(instance));
                                    if (childInstance is IEnumerable collection)
                                    {
                                        foreach (var item in collection)
                                        {
                                            var childModel = new XElement("model");
                                            localDiscoverModel(item, childModel, visited);
                                            member.Add(childModel);
                                        }
                                    }
                                    localRunRecursiveDiscovery(member);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attaches notification delegates to the descendants of the given object, 
        /// allowing property, collection, and object changes to be monitored. 
        /// This overload discards the model output.
        /// </summary>
        /// <typeparam name="T">The type of the object, which must have a parameterless constructor.</typeparam>
        /// <param name="this">The object whose descendants should be monitored.</param>
        /// <param name="onPC">The delegate to handle property change notifications (required).</param>
        /// <param name="onCC">The delegate to handle collection change notifications (optional).</param>
        /// <param name="onXO">The delegate to handle object change notifications (optional).</param>
        /// <returns>The same instance of <typeparamref name="T"/> for fluent chaining.</returns>
        public static T WithNotifyOnDescendants<T>(
            this T @this,
            PropertyChangedDelegate onPC,
            NotifyCollectionChangedDelegate onCC = null,
            XObjectChangeDelegate onXO = null)
            => @this.WithNotifyOnDescendants(out XElement _, onPC, onCC, onXO);

        /// <summary>
        /// Attaches notification delegates to the descendants of the given object,
        /// allowing for property changes, collection changes, and object changes to be monitored.
        /// </summary>
        /// <typeparam name="T">The type of the object, which must have a parameterless constructor.</typeparam>
        /// <param name="this">The object whose descendants should be monitored.</param>
        /// <param name="model">The XElement representing the model, returned as an output parameter.</param>
        /// <param name="onPC">The delegate to handle property change notifications (required).</param>
        /// <param name="onCC">The delegate to handle collection change notifications (optional).</param>
        /// <param name="onXO">The delegate to handle object change notifications (optional).</param>
        /// <returns>The same instance of <typeparamref name="T"/> for fluent chaining.</returns>
        public static T WithNotifyOnDescendants<T>(
            this T @this,
            out XElement model,
            PropertyChangedDelegate onPC,
            NotifyCollectionChangedDelegate onCC = null,
            XObjectChangeDelegate onXO = null)
        {
            throw new NotImplementedException();
            return @this;
        }
    }
}
