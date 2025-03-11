using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    [Flags]
    public enum ModelingOption
    {
        CachePropertyInfo = 0x1,
        ShowFullNameForTypes = 0x2,
        IncludeValueTypeInstances = 0x4,
    }

    public delegate void PropertyChangedDelegate(object sender, PropertyChangedEventArgs e);
    public delegate void NotifyCollectionChangedDelegate(object sender, NotifyCollectionChangedEventArgs e);
    public delegate void XObjectChangeDelegate(object sender, XObjectChangeEventArgs e);
    public class ModelingContext
    {
        public ModelingContext(XElement model = null)
        {
            if(model != null)
            {
                OriginModel = model;
            }
        }

        public XElement OriginModel
        {
            get
            {
                if(_originModel is null)
                {
                    _originModel = new XElement($"{StdFrameworkName.model}");
                    OnOriginModelChanged();
                }
                return _originModel;
            }
            private set
            {
                if (!Equals(_originModel, value))
                {
                    _originModel = value;
                    OnOriginModelChanged();
                }
            }
        }
        XElement _originModel = null;
        protected virtual void OnOriginModelChanged()
        {
            // [Careful] Use backing store here to avoid activating the singleton.
            if(_originModel != null)
            {
                var trueOrigin = _originModel.AncestorsAndSelf().Last();
                if(trueOrigin.To<ModelingContext>() is null)
                {
                    _originModel.SetBoundAttributeValue(this, StdFrameworkName.context);
                }
            }
        }
        public PropertyChangedDelegate PropertyChangedDelegate { get; set; } = null;
        public NotifyCollectionChangedDelegate NotifyCollectionChangedDelegate { get; set; } = null;
        public XObjectChangeDelegate XObjectChangeDelegate { get; set; } = null;

        public ModelingOption Options { get; set; } = 0;

        internal void RaiseModelAdded(object sender, XElement element)
        {
            ModelAdded?.Invoke(sender, new ElementAvailableEventArgs(element));
        }
        public event EventHandler<ElementAvailableEventArgs> ModelAdded;
    }
    public class ElementAvailableEventArgs : EventArgs
    {
        public ElementAvailableEventArgs(XElement xel) => Element = xel;

        public XElement Element { get; }
    }
    public static class ModelingExtensions
    {
        public static XElement CreateModel(this object @this, ModelingContext context)
        {
            if (@this is ModelingContext)
                throw new InvalidOperationException($"Can't create a model of a {nameof(ModelingContext)}.");
            foreach (var xel in @this.ModelDescendantsAndSelf())
            {
                context?.RaiseModelAdded(sender: @this, element: xel);
            }
            return context.OriginModel;
        }
        public static IEnumerable<XElement> ModelDescendantsAndSelf(this object @this, ModelingContext context = null)
        {
            if (@this is ModelingContext)
                throw new InvalidOperationException($"Can't create a model of a {nameof(ModelingContext)}.");
            var type = @this.GetType();
            context = context ?? new ModelingContext();
            if (!context.OriginModel.Ancestors().Any())
            {
                if(context.Options.HasFlag(ModelingOption.ShowFullNameForTypes))
                {
                    context.OriginModel.SetAttributeValue(
                        nameof(SortOrderNOD.name),
                        $"(Origin){type.ToTypeNameText()}");
                }
                else
                {
                    context.OriginModel.SetAttributeValue(
                        nameof(SortOrderNOD.name),
                        $"(Origin){type.ToShortTypeNameText()}");
                }
            }
            localDiscoverModel(@this, context.OriginModel);
            foreach (var xel in context.OriginModel.DescendantsAndSelf())
            {
                yield return xel;
            }

            void localDiscoverModel(object instance, XElement localModel, HashSet<object> visited = null)
            {
                visited = visited ?? new HashSet<object>();
                if (context.Options.HasFlag(ModelingOption.ShowFullNameForTypes))
                {
                    localModel.SetBoundAttributeValue(
                        instance,
                        name: nameof(instance),
                        instance.GetType().ToTypeNameText().InSquareBrackets());
                }
                else
                {
                    localModel.SetBoundAttributeValue(
                        instance, 
                        name: nameof(instance),
                        instance.GetType().ToShortTypeNameText().InSquareBrackets());
                }
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
                            if(context.Options.HasFlag(ModelingOption.CachePropertyInfo))
                            {
                                if (context.Options.HasFlag(ModelingOption.ShowFullNameForTypes))
                                {
                                    member.SetBoundAttributeValue(
                                        name: SortOrderNOD.pi.ToString(),
                                        tag: pi,
                                        text: pi.PropertyType.ToTypeNameText().InSquareBrackets()
                                    );
                                }
                                else
                                {
                                    member.SetBoundAttributeValue(
                                        name: SortOrderNOD.pi.ToString(),
                                        tag: pi,
                                        text: pi.PropertyType.ToShortTypeNameText().InSquareBrackets()
                                    );
                                }
                            }
                            currentElement.Add(member);
                            if (pi.GetValue(localInstance) is object childInstance)
                            {
                                var childType = childInstance.GetType();
                                if (childInstance is string ||
                                    childInstance is Enum ||
                                    childInstance is ValueType)
                                {
                                    if (context?.Options.HasFlag(ModelingOption.IncludeValueTypeInstances) == true)
                                    {
                                        if (context.Options.HasFlag(ModelingOption.ShowFullNameForTypes))
                                        {
                                            member.SetBoundAttributeValue(
                                                childInstance, 
                                                nameof(instance),
                                                childType.ToTypeNameText().InSquareBrackets());
                                        }
                                        else
                                        {
                                            member.SetBoundAttributeValue(
                                                childInstance,
                                                nameof(instance),
                                                childType.ToShortTypeNameText().InSquareBrackets());
                                        }
                                    }
                                    continue;
                                }
                                else
                                {
                                    if (context.Options.HasFlag(ModelingOption.ShowFullNameForTypes))
                                    {
                                        member.SetBoundAttributeValue(
                                            childInstance, 
                                            nameof(instance), 
                                            childType.ToTypeNameText().InSquareBrackets());
                                    }
                                    else
                                    {
                                        member.SetBoundAttributeValue(
                                            childInstance,
                                            nameof(instance),
                                            childType.ToShortTypeNameText().InSquareBrackets());
                                    }
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
            XObjectChangeDelegate onXO = null,
            ModelingOption options = 0)
            => @this.WithNotifyOnDescendants(out XElement _, onPC, onCC, onXO, options);

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
            XObjectChangeDelegate onXO = null,
            ModelingOption options = 0)
        {
            var context = new ModelingContext()
            {
                PropertyChangedDelegate = onPC,
                NotifyCollectionChangedDelegate = onCC,
                XObjectChangeDelegate = onXO,               
                Options = options,
            };
            model = context.OriginModel;
            context.ModelAdded += (sender, e) =>
            {
                var modelAdd = e.Element;
#if DEBUG
                var shallow = modelAdd.ToShallow();
#endif
                if (modelAdd.Name != $"{StdFrameworkName.model}" &&
                    modelAdd.GetInstance() is object o)
                {
                    if (modelAdd.Attribute(SortOrderNOD.onpc.ToString()) is null)   // Check for refresh
                    {
                        if (onPC != null && o is INotifyPropertyChanged inpc)
                        {
                            PropertyChangedEventHandler handlerPC = (senderPC, ePC) =>
                            {
                                if (modelAdd.GetMember(ePC.PropertyName) is XElement member)
                                {
                                    PropertyInfo pi = member.To<PropertyInfo>();
                                    if (pi is null) pi = sender.GetType().GetProperty(ePC.PropertyName);
                                    if (pi != null)
                                    {
                                        if (pi.IsEnumOrValueTypeOrString())
                                        {   /* G T K */
                                            // [Careful]
                                            // We can only do this on PropertyType not on Instance Type.
                                            // That is, a property of type 'object' can go in and out
                                            // of being any other status. But if the property type is
                                            // fixed that way, then it is safe to ignore.
                                        }
                                        else
                                        {
                                            member.RefreshModel(newValue: pi.GetValue(o));
                                        }
                                    }
                                    onPC?.Invoke(member, ePC);
                                }
                            };
                            modelAdd.SetBoundAttributeValue(
                                tag: handlerPC,
                                SortOrderNOD.onpc,
                                text: StdFrameworkName.OnPC.InSquareBrackets());
                            inpc.PropertyChanged += handlerPC;
                        }
                    }
                    if (onCC != null && o is INotifyCollectionChanged incc)
                    {
                        if (modelAdd.Attribute(SortOrderNOD.oncc.ToString()) is null)   // Check for refresh
                        {
                            NotifyCollectionChangedEventHandler handlerCC = (senderCC, eCC) =>
                            {
                                switch (eCC.Action)
                                {
                                    case NotifyCollectionChangedAction.Add: onAdd(); break;
                                    case NotifyCollectionChangedAction.Remove: onRemove(); break;
                                    case NotifyCollectionChangedAction.Replace: onReplace(); break;
                                    case NotifyCollectionChangedAction.Reset: onReset(); break;
                                }

                                void onAdd()
                                {
                                    eCC.NewItems?
                                    .OfType<object>()
                                    .ToList()
                                    .ForEach(newItem =>
                                    {
                                        _ = newItem
                                            .WithNotifyOnDescendants(
                                                out XElement addedModel,
                                                onPC,
                                                onCC,
                                                onXO,
                                                options);
                                        modelAdd.Add(addedModel);
                                    });
                                }

                                void onRemove()
                                {
                                    eCC.OldItems?
                                    .OfType<object>()
                                    .ToList()
                                    .ForEach(_ =>
                                    {
                                        var removeModel =
                                            modelAdd
                                            .Elements()
                                            .First(desc => ReferenceEquals(_, desc.GetInstance()));
                                        removeModel.Remove();
                                    });
                                }
                                void onReplace()
                                {
                                    onRemove();
                                    onAdd();
                                }
                                void onReset()
                                {
                                    foreach (var removeModel in modelAdd.Elements().ToArray())
                                    {
                                        removeModel.Remove();
                                    }
                                }
                                // Call the delegate specified by the context.
                                onCC?.Invoke(modelAdd, eCC);
                            };
                            modelAdd.SetBoundAttributeValue(
                                tag: handlerCC,
                                SortOrderNOD.oncc,
                                text: StdFrameworkName.OnCC.InSquareBrackets());
                            incc.CollectionChanged += handlerCC;
                        }
                    }
                }
            };
            context.OriginModel.Changing += (sender, e)
                => onXObjectCommon(sender, new XObjectChangedOrChangingEventArgs(e, true));
            context.OriginModel.Changed += (sender, e) 
                => onXObjectCommon(sender, new XObjectChangedOrChangingEventArgs(e, false));
            // Show time
            model = @this.CreateModel(context);
            return @this;

            void onXObjectCommon(object sender, XObjectChangedOrChangingEventArgs e)
            {
                onXO?.Invoke(sender, e);
            }
        }
        public static void RefreshModel(this XElement model, object newValue)
        {
            var attrsB4 = model.Attributes().ToArray();
            // Perform an unconditional complete reset.
            foreach (var element in model.Elements().ToArray())
            {
                foreach (var desc in element.DescendantsAndSelf().ToArray())
                {
                    desc.Remove();
                }
            }
            foreach (var attr in model.Attributes().ToArray())
            {
                switch (attr.Name.LocalName)
                {
                    case nameof(SortOrderNOD.name):
                    case nameof(SortOrderNOD.pi):
                        break;
                    case nameof(SortOrderNOD.statusnod):
                    case nameof(SortOrderNOD.instance):
                    case nameof(SortOrderNOD.runtimetype):
                    case nameof(SortOrderNOD.onpc):
                    case nameof(SortOrderNOD.oncc):
                    case nameof(SortOrderNOD.notifyinfo):
                        attr.Remove();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (newValue != null)
            {
                
            }
        }


        public static string ToTypeNameText(this Type @this)
        {
            if (@this?.FullName == null) return "Unknown";

            var fullName = @this.FullName.Split('`').First(); // Remove generic type info
            int lastPlusIndex = fullName.LastIndexOf('+');

            if (lastPlusIndex < 0) return fullName; // No nested class, return as is

            int lastDotIndex = fullName.LastIndexOf('.', lastPlusIndex);
            return fullName.Remove(lastDotIndex + 1, lastPlusIndex - lastDotIndex);
        }
        public static string ToShortTypeNameText(this Type @this)
            => @this.ToTypeNameText().Split('.').Last();

        public static string InSquareBrackets(this string @this) => $"[{@this}]";
        public static string InSquareBrackets(this Enum @this) => $"[{@this.ToString()}]";

        public static T GetInstance<T>(this XElement @this, bool @throw = false)
        {
            if (@this.Attribute(nameof(SortOrderNOD.instance)) is XBoundAttribute xba)
            {
                if (xba.Tag is T instance)
                {
                    return instance;
                }
                else
                {
                    if (@throw) throw new NullReferenceException($"Expecting {nameof(XBoundAttribute)}.Tag is not null.");
                    else return default;
                }
            }
            else
            {
                if (@throw) throw new NullReferenceException($"Expecting {nameof(SortOrderNOD.instance)} is {nameof(XBoundAttribute)}");
                return default;
            }
        }
        public static object GetInstance(this XElement @this, bool @throw = false)
        {
            if (@this.Attribute(nameof(SortOrderNOD.instance)) is XBoundAttribute xba)
            {
                if (xba.Tag is object instance)
                {
                    return instance;
                }
                else
                {
                    if (@throw) throw new NullReferenceException($"Expecting {nameof(XBoundAttribute)}.Tag is not null.");
                    else return null;
                }
            }
            else
            {
                if (@throw) throw new NullReferenceException($"Expecting {nameof(SortOrderNOD.instance)} is {nameof(XBoundAttribute)}");
                return default;
            }
        }
        public static object GetMember(this XElement @this, string propertyName)
        => @this.Elements().FirstOrDefault(_ =>
            _
            .Attribute(nameof(SortOrderNOD.name))?
            .Value == propertyName);

        internal static bool IsEnumOrValueTypeOrString(this object @this)
            => @this is Enum || @this is ValueType || @this is string;
        internal static bool IsEnumOrValueTypeOrString(this Type @this)
            => @this.IsEnum || @this.IsValueType || Equals(@this, typeof(string));
    }
}
