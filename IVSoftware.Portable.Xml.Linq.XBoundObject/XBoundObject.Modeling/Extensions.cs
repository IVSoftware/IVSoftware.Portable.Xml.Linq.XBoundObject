using IVSoftware.Portable.Threading;
using static IVSoftware.Portable.Threading.Extensions;
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
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;

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
            OriginModel = model ?? new XElement(nameof(StdFrameworkName.model));
            OriginModel.SetBoundAttributeValue(this, StdFrameworkName.context);
            OriginModel.Changing += (sender, e)
                => XObjectChange?.Invoke(sender, new XObjectChangedOrChangingEventArgs(e, true));
            OriginModel.Changed += (sender, e)
                => XObjectChange?.Invoke(sender, new XObjectChangedOrChangingEventArgs(e, false));
        }
        // Clone Constructor
        private ModelingContext(ModelingContext other, XElement localModel = null)
        {
            OriginModel = other.OriginModel;
            LocalModel = localModel ?? new XElement(nameof(StdFrameworkName.model));
            if (other.ModelAdded != null)
            {
                ModelAdded += other.ModelAdded;
            }
            foreach (var pi in PICache)
            {
                var value = pi.GetValue(other);
                pi.SetValue(this, value);
            }
        }
        public PropertyInfo[] PICache
        {
            get
            {
                if (_piCache is null)
                {
                    _piCache =
                        typeof(ModelingContext)
                        .GetProperties()
                        .Where(_ => _.CanRead && _.CanWrite)
                        .ToArray();
                }
                return _piCache;
            }
        }
        PropertyInfo[] _piCache = null;
        public ModelingContext Clone(XElement localModel = null)
            => new ModelingContext(this, localModel);
        public XElement OriginModel { get; }
        public XElement LocalModel { get; }
        public XElement TargetModel => LocalModel ?? OriginModel;
        public PropertyChangedDelegate PropertyChangedDelegate { get; set; } = null;
        public NotifyCollectionChangedDelegate NotifyCollectionChangedDelegate { get; set; } = null;
        public XObjectChangeDelegate XObjectChangeDelegate { get; set; } = null;

        public ModelingOption Options { get; set; } = 0;
        public event EventHandler<XObjectChangedOrChangingEventArgs> XObjectChange;

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
        public static XElement CreateModel(this object @this, ModelingContext context = null)
            => @this.CreateModel<SortOrderNOD>(context);

        public static XElement CreateModel<T>(this object @this, ModelingContext context = null) where T: Enum
        {
            if (@this is ModelingContext)
                throw new InvalidOperationException($"Can't create a model of a {nameof(ModelingContext)}.");
            // [Careful]
            // - Do not remove this, even though it's checked again in ModelDescendantsAndSelf.
            // - The issue is that it would end up null since context is not passed bu ref.
            context = context ?? new ModelingContext();
            foreach (var xel in @this.ModelDescendantsAndSelf(context))
            {
                context?.RaiseModelAdded(sender: @this, element: xel);
            }
            return context.TargetModel.SortAttributes<T>();
        }
        public static IEnumerable<XElement> ModelDescendantsAndSelf(this object @this, ModelingContext context = null)
        {
            if (@this is ModelingContext)
                throw new InvalidOperationException($"Can't create a model of a {nameof(ModelingContext)}.");
            var type = @this.GetType();
            context = context ?? new ModelingContext();
            if (!context.TargetModel.Ancestors().Any())
            {
                if(context.Options.HasFlag(ModelingOption.ShowFullNameForTypes))
                {
                    context.TargetModel.SetAttributeValue(
                        nameof(SortOrderNOD.name),
                        $"(Origin){type.ToTypeNameText()}");
                }
                else
                {
                    context.TargetModel.SetAttributeValue(
                        nameof(SortOrderNOD.name),
                        $"(Origin){type.ToShortTypeNameText()}");
                }
            }
            localDiscoverModel(@this, context.TargetModel);
            foreach (var xel in context.TargetModel.DescendantsAndSelf())
            {
                yield return xel;
            }

            void localDiscoverModel(object instance, XElement localModel, HashSet<object> visited = null)
            {
                visited = visited ?? new HashSet<object>();
                if (!instance.IsEnumOrValueTypeOrString() || context.Options.HasFlag(ModelingOption.IncludeValueTypeInstances))
                {
                    localModel.SetBoundAttributeValue(
                        instance,
                        name: nameof(instance),
                        instance.GetType().ToTypeNameForOptionText(context.Options).InSquareBrackets());
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
                            if (context.Options.HasFlag(ModelingOption.CachePropertyInfo))
                            {
                                member.SetBoundAttributeValue(
                                    name: SortOrderNOD.pi.ToString(),
                                    tag: pi,
                                    text: pi.PropertyType.ToTypeNameForOptionText(context.Options).InSquareBrackets()
                                );
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
                                        // Set the INSTANCE
                                        member.SetBoundAttributeValue(
                                            childInstance,
                                            nameof(instance),
                                            childType.ToTypeNameForOptionText(context.Options).InSquareBrackets());
                                    }
                                    else
                                    {
                                        // Set the RUNTIME PROPERTY TYPE...
                                        if (!Equals(pi.PropertyType, childType))
                                        {
                                            // ... but only of DIFFERENT from static property type.
                                            member.SetAttributeValue(
                                                nameof(SortOrderNOD.runtimetype),
                                                childType.ToTypeNameForOptionText(context.Options).InSquareBrackets());
                                        }
                                    }
                                    continue;
                                }
                                else
                                {
                                    member.SetBoundAttributeValue(
                                        childInstance,
                                        nameof(instance),
                                        childType.ToTypeNameForOptionText(context.Options).InSquareBrackets());
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
            context.ModelAdded += localOnModelAdded;

            void localOnModelAdded(object sender, ElementAvailableEventArgs e)
            {
                var modelAdded = e.Element;
                bool isOrigin = ReferenceEquals(modelAdded, context.OriginModel);
#if DEBUG
                var shallow = modelAdded.ToShallow();
                if (isOrigin)
                {   /* G T K */
                }
                else
                {   /* G T K */
                }
#endif
                if (!isOrigin && modelAdded.GetInstance() is object o)
                {
                    if (modelAdded.Attribute(SortOrderNOD.onpc.ToString()) is null)   // Check for refresh
                    {
                        if (onPC != null && o is INotifyPropertyChanged inpc)
                        {
                            PropertyChangedEventHandler handlerPC = (senderPC, ePC) =>
                            {
                                if (modelAdded.GetMember(ePC.PropertyName) is XElement member)
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
                            modelAdded.SetBoundAttributeValue(
                                tag: handlerPC,
                                SortOrderNOD.onpc,
                                text: StdFrameworkName.OnPC.InSquareBrackets());
                            inpc.PropertyChanged += handlerPC;
                        }
                    }
                    if (onCC != null && o is INotifyCollectionChanged incc)
                    {
                        if (modelAdded.Attribute(SortOrderNOD.oncc.ToString()) is null)   // Check for refresh
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
                                        var addedModel = newItem.CreateModel(context.Clone());
                                        modelAdded.Add(addedModel);
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
                                            modelAdded
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
                                    foreach (var removeModel in modelAdded.Elements().ToArray())
                                    {
                                        removeModel.Remove();
                                    }
                                }
                                // Call the delegate specified by the context.
                                onCC?.Invoke(modelAdded, eCC);
                            };
                            modelAdded.SetBoundAttributeValue(
                                tag: handlerCC,
                                SortOrderNOD.oncc,
                                text: StdFrameworkName.OnCC.InSquareBrackets());
                            incc.CollectionChanged += handlerCC;
                        }
                    }
                }
            };
            context.XObjectChange += (sender, e) =>
            {
                switch (e.ObjectChange)
                {
                    case XObjectChange.Remove:
                        switch (sender)
                        {
                            case XAttribute xattr:
                                if (Extensions.IsSorting)
                                {   /* G T K    N O O P */
                                }
                                else
                                {
                                    // These can also occur out of band.
                                    switch (xattr.Name.LocalName)
                                    {
                                        case nameof(SortOrderNOD.instance): localRemoveHandlers(xattr.Parent, SortOrderNOD.instance); break;
                                        case nameof(SortOrderNOD.onpc): localRemoveHandlers(xattr.Parent, SortOrderNOD.onpc); break;
                                        case nameof(SortOrderNOD.oncc): localRemoveHandlers(xattr.Parent, SortOrderNOD.oncc); break;
                                    }
                                }
                                break;
                            case XElement xel:
                                if (e.IsChanging)
                                {
                                    if (xel.GetInstance() is object o)
                                    {
                                        // From the last to the first, recursively
                                        // remove any xelements that exist below.
                                        foreach (var desc in xel.Elements().Reverse().ToArray())
                                        {
                                            Debug.Assert(xel.Parent != null, "For this to work, the xobject must still be parented.");
                                            Debug.WriteLineIf(true, $"250313 {desc.ToShallow()}");
                                            desc.Remove(); // [Careful] Do not expect a recursion event for ValueTypes under normal options config.
                                        }
                                        localRemoveHandlers(xel, SortOrderNOD.instance);
                                    }
                                }
                                else
                                {   /* G T K */
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                        void localRemoveHandlers(XElement xel, SortOrderNOD @case)
                        {
                            if (xel?.GetInstance() is object o)
                            {
                                if (Equals(@case, SortOrderNOD.instance) || Equals(@case, SortOrderNOD.onpc))
                                {
                                    if (o is INotifyPropertyChanged inpc)
                                    {
                                        if (xel.To<PropertyChangedEventHandler>() is PropertyChangedEventHandler handlerPC)
                                        {
                                            inpc.PropertyChanged -= handlerPC;
                                            xel.OnAwaited(new AwaitedEventArgs(args: $"Removing INPC Subscription"));
                                            xel.OnAwaited(new AwaitedEventArgs(args: $"{e.ObjectChange} {xel.ToShallow()}"));
                                        }
                                        else
                                        {
                                            // If this occurs out of band, we need to know that and respond!
                                            Debug.Fail("Expecting that inpc has a bound handler to unsubscribe.");
                                        }
                                    }
                                }
                                if (Equals(@case, SortOrderNOD.instance) || Equals(@case, SortOrderNOD.oncc))
                                {
                                    if (o is INotifyCollectionChanged incc)
                                    {
                                        if (xel.To<NotifyCollectionChangedEventHandler>() is NotifyCollectionChangedEventHandler handlerCC)
                                        {
                                            incc.CollectionChanged -= handlerCC;
                                            xel.OnAwaited(new AwaitedEventArgs(args: $"Removing INCC Subscription"));
                                            xel.OnAwaited(new AwaitedEventArgs(args: $"{e.ObjectChange} {xel.ToShallow()}"));
                                        }
                                        else
                                        {
                                            // If this occurs out of band, we need to know that and respond!
                                            Debug.Fail("Expecting that incc has a bound handler to unsubscribe.");
                                        }
                                    }
                                }
                            }
                        }
                }
            };
            // Show time
            model = @this.CreateModel(context);
            if(@this is IEnumerable collection && !(@this is string))
            {
                foreach (var item in collection.OfType<object>())
                {                    
                    var addedModel = item.CreateModel(context.Clone());
                    model.Add(addedModel);
                }
            }
            return @this;
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
                if (model.AncestorOfType<ModelingContext>() is ModelingContext context)
                {
                    newValue.CreateModel(context.Clone(model));
                }
            }
        }
        public static string ToTypeNameForOptionText(this Type @this, ModelingOption options)
            =>
            options.HasFlag(ModelingOption.ShowFullNameForTypes)
            ? @this.ToTypeNameText()
            : @this.ToShortTypeNameText();
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
