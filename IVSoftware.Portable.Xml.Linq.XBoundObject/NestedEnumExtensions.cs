using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject
{
    /// <summary>
    /// Specifies constraints for discovering related enum types during lookup.
    /// This enumeration supports bitwise combinations of its values.
    /// </summary>
    [Flags]
    public enum DiscoveryScope
    {
        /// <summary>
        /// Searches all loaded assemblies in the application domain.
        /// This is the broadest scope, allowing discovery across all assemblies.
        /// </summary>
        AllAppDomainAssemblies = 0x0,

        /// <summary>
        /// Restricts the search to the assembly that defines the specified enum type.
        /// This reduces lookup overhead and prevents conflicts with enums of the same name in other assemblies.
        /// </summary>
        ConstrainToAssembly = 0x1,

        /// <summary>
        /// Restricts the search to enums within the same namespace as the specified type.
        /// This is useful for maintaining logical grouping of related enums while avoiding namespace collisions.
        /// </summary>
        ConstrainToNamespace = 0x2
    }



    public static class NestedEnumExtensions
    {
        /// <summary>
        /// Retrieves all descendant enum values related to the specified enum type.
        /// This method identifies hierarchical relationships among "flat" enum groups
        /// by searching for other enums that share names with the current enum values.
        /// </summary>
        /// <param name="type">
        /// The root enum type from which to discover descendant enum values.
        /// </param>
        /// <param name="options">
        /// A bitwise combination of <see cref="DiscoveryScope"/> flags that control 
        /// the lookup behavior:
        /// <list type="bullet">
        ///   <item><see cref="DiscoveryScope.AllAppDomainAssemblies"/> – Searches all loaded assemblies in the application domain.</item>
        ///   <item><see cref="DiscoveryScope.ConstrainToAssembly"/> – Limits the search to the assembly containing <paramref name="type"/>.</item>
        ///   <item><see cref="DiscoveryScope.ConstrainToNamespace"/> – Limits the search to enums within the same namespace as <paramref name="type"/>.</item>
        /// </list>
        /// If multiple flags are combined, the search is progressively constrained.
        /// The default value is <see cref="DiscoveryScope.ConstrainToAssembly"/> | <see cref="DiscoveryScope.ConstrainToNamespace"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{Enum}"/> containing the discovered descendant 
        /// enum values, including indirect relationships.
        /// </returns>
        /// <exception cref="AmbiguousMatchException">
        /// Thrown when multiple enum types share the same name, causing ambiguity.
        /// To resolve, refine the <see cref="DiscoveryScope"/> constraints.
        /// </exception>
        public static IEnumerable<Enum> Descendants(
             this Type type,
             DiscoveryScope options = DiscoveryScope.ConstrainToAssembly | DiscoveryScope.ConstrainToNamespace)
        {
             var types = 
                options.HasFlag(DiscoveryScope.ConstrainToAssembly)
                ? type
                    .Assembly
                    .GetTypes()
                    .Where(_=>_.IsEnum)
                    .ToArray()
                : AppDomain
                    .CurrentDomain
                    .GetAssemblies().SelectMany(_ => _.GetTypes())
                    .Where(_=>_.IsEnum)
                    .ToArray();

            if (options.HasFlag(DiscoveryScope.ConstrainToNamespace))
            {
                types = 
                    types
                    .Where(_ => _.Namespace == type.Namespace)
                    .ToArray();
            }
            foreach (var value in localDescendants(type))
            {
                yield return value;
            }
            IEnumerable<Enum> localDescendants(Type currentType)
            {
                foreach (Enum value in currentType.GetEnumValues())
                {
                    yield return value;
                    var matches =
                        types
                        .Where(_ => _.Name == $"{value}")
                        .ToArray();
                    switch (matches.Length)
                    {
                        case 0:
                            break;
                        case 1:
                            foreach (var childValue in localDescendants(matches[0]))
                            {
                                yield return childValue;
                            }
                            break;
                        default:
                            throw new AmbiguousMatchException(
                                $"Multiple matches found for {value.ToFullKey()}. Try reducing {nameof(DiscoveryScope)}.");
                    }
                }
            }
        }

        /// <summary>
        /// Constructs a hierarchical XML representation of an enum and its related enums,
        /// effectively mapping "flat" enum structures into a nested format.
        /// </summary>
        /// <param name="type">
        /// The root enum type from which to build the hierarchy.
        /// </param>
        /// <param name="options">
        /// A bitwise combination of <see cref="DiscoveryScope"/> flags that determine the scope of the lookup:
        /// <list type="bullet">
        ///   <item><see cref="DiscoveryScope.AllAppDomainAssemblies"/> – Searches all loaded assemblies.</item>
        ///   <item><see cref="DiscoveryScope.ConstrainToAssembly"/> – Limits search to the root enum’s assembly.</item>
        ///   <item><see cref="DiscoveryScope.ConstrainToNamespace"/> – Limits search to the root enum’s namespace.</item>
        /// </list>
        /// The default value is <see cref="DiscoveryScope.ConstrainToAssembly"/> | <see cref="DiscoveryScope.ConstrainToNamespace"/>, 
        /// ensuring that only enums within the same assembly and namespace are considered.
        /// </param>
        /// <param name="root">
        /// The name of the root XML element. Defaults to `"root"`.
        /// </param>
        /// <returns>
        /// An <see cref="XElement"/> representing the hierarchical structure of enums.
        /// The XML output nests related enums within their respective parent values.
        /// </returns>
        /// <exception cref="AmbiguousMatchException">
        /// Thrown when multiple enums share the same name, causing ambiguity.
        /// Consider refining <see cref="DiscoveryScope"/> to resolve conflicts.
        /// </exception>
        /// <remarks>
        /// This method discovers relationships between enums based on **naming conventions**.
        /// If an enum value matches the name of another enum type, that type is treated as a child in the hierarchy.
        public static XElement BuildNestedEnum(
             this Type type,
             DiscoveryScope options = DiscoveryScope.ConstrainToAssembly | DiscoveryScope.ConstrainToNamespace,
             string root = "root")
        {
            var types =
               options.HasFlag(DiscoveryScope.ConstrainToAssembly)
               ? type
                   .Assembly
                   .GetTypes()
                   .Where(_ => _.IsEnum)
                   .ToArray()
               : AppDomain
                   .CurrentDomain
                   .GetAssemblies().SelectMany(_ => _.GetTypes())
                   .Where(_ => _.IsEnum)
                   .ToArray();

            if (options.HasFlag(DiscoveryScope.ConstrainToNamespace))
            {
                types =
                    types
                    .Where(_ => _.Namespace == type.Namespace)
                    .ToArray();
            }
            var xroot = new XElement(root);
            var x2id2x = new DualKeyLookup();
            xroot.SetBoundAttributeValue(x2id2x);

            foreach ((Enum value, XElement xel) in localDescendants(type, xroot))
            {
                x2id2x[value] = xel;
            }
            return xroot;

            IEnumerable<(Enum value, XElement xel)> localDescendants(Type currentType, XElement xCurrent)
            {
                foreach (Enum value in currentType.GetEnumValues())
                {
                    var xnode = new XElement("node");
                    xnode.SetBoundAttributeValue(value, name: "id");
                    xCurrent.Add(xnode); // Attach to the current XML tree
                    yield return (value, xnode);

                    var matches =
                        types
                        .Where(_ => _.Name == $"{value}")
                        .ToArray();
                    switch (matches.Length)
                    {
                        case 0:
                            break;
                        case 1:
                            foreach (var childValue in localDescendants(matches[0], xnode))
                            {
                                yield return childValue;
                            }
                            break;
                        default:
                            throw new AmbiguousMatchException(
                                $"Multiple matches found for {value.ToFullKey()}. Try reducing {nameof(DiscoveryScope)}.");
                    }
                }
            }
        }

        /// <summary>
        /// Generates a fully qualified string representation of an enum value,
        /// including its type name and value.
        /// </summary>
        /// <param name="this">
        /// The enum instance to convert.
        /// </param>
        /// <returns>
        /// A string in the format `"EnumType.EnumValue"`, representing the fully qualified name of the enum.
        /// </returns>
        public static string ToFullKey(this Enum @this) =>
            $"{@this.GetType().Name}.{@this}";
    }
}
