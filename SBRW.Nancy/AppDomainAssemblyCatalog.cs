#if NETFRAMEWORK || (NET5_0_OR_GREATER && WINDOWS)
namespace SBRW.Nancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using SBRW.Nancy.Extensions;
    using SBRW.Nancy.Helpers;

    /// <summary>
    /// Default implementation of the <see cref="IAssemblyCatalog"/> interface, based on
    /// retrieving <see cref="Assembly"/> information from <see cref="AppDomain.CurrentDomain"/>.
    /// </summary>
    public class AppDomainAssemblyCatalog : IAssemblyCatalog
    {
        private static readonly AssemblyName NancyAssemblyName = typeof(INancyEngine).GetTypeInfo().Assembly.GetName();
        private readonly Lazy<IReadOnlyCollection<Assembly>> assemblies = new Lazy<IReadOnlyCollection<Assembly>>(GetAvailableAssemblies);

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public virtual IReadOnlyCollection<Assembly> GetAssemblies()
        {
            return this.assemblies.Value;
        }

        private static IReadOnlyCollection<Assembly> GetAvailableAssemblies()
        {
            var assemblies = GetLoadedNancyReferencingAssemblies();

            var loaded = LoadNancyReferencingAssemblies(assemblies);

            return assemblies.Union(loaded).ToArray();
        }

        private static List<Assembly> GetLoadedNancyReferencingAssemblies()
        {
            var assemblies = new List<Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !assembly.ReflectionOnly && assembly.IsReferencing(NancyAssemblyName))
                {
                    assemblies.Add(assembly);
                }
            }

            return assemblies;
        }

        private static IEnumerable<Assembly> LoadNancyReferencingAssemblies(IEnumerable<Assembly> loadedAssemblies)
        {
            var assemblies = new HashSet<Assembly>();

#if NETFRAMEWORK
            var inspectionAppDomain = CreateInspectionAppDomain();
            var inspectionProber = CreateRemoteReferenceProber(inspectionAppDomain);
#else
            var inspectionProber = new ProxyNancyReferenceProber();
#endif

            var loadedNancyReferencingAssemblyNames = loadedAssemblies.Select(assembly => assembly.GetName()).ToArray();

            foreach (var directory in GetAssemblyDirectories())
            {
                foreach (var assemblyPath in Directory.EnumerateFiles(directory, "*.dll"))
                {
                    var unloadedAssemblyName = SafeGetAssemblyName(assemblyPath);

                    if (unloadedAssemblyName == null)
                    {
                        continue;
                    }

                    if (!loadedNancyReferencingAssemblyNames.Any(loadedNancyReferencingAssemblyName => AssemblyName.ReferenceMatchesDefinition(loadedNancyReferencingAssemblyName, unloadedAssemblyName)))
                    {
                        if (inspectionProber.HasReference(unloadedAssemblyName, NancyAssemblyName))
                        {
                            var assembly = SafeLoadAssembly(AppDomain.CurrentDomain, unloadedAssemblyName);

                            if (assembly != null)
                            {
                                assemblies.Add(assembly);
                            }
                        }
                    }
                }
            }

#if NETFRAMEWORK
            AppDomain.Unload(inspectionAppDomain);
#endif

            return assemblies.ToArray();
        }

#if NETFRAMEWORK
        private static AppDomain CreateInspectionAppDomain()
        {
            var currentAppDomain = AppDomain.CurrentDomain;

            return AppDomain.CreateDomain("AppDomainAssemblyCatalog", currentAppDomain.Evidence,
                currentAppDomain.SetupInformation);
        }

        private static ProxyNancyReferenceProber CreateRemoteReferenceProber(AppDomain appDomain)
        {
            return (ProxyNancyReferenceProber)appDomain.CreateInstanceAndUnwrap(
                typeof(ProxyNancyReferenceProber).Assembly.FullName,
                typeof(ProxyNancyReferenceProber).FullName);
        }
#endif

#if !NETFRAMEWORK
        private static IEnumerable<string> GetPrivateBinPaths()
        {
            /* In .NET one can approximate PrivateBinPath by using additional probing paths or simply the base directory */
            return new[] { AppContext.BaseDirectory };
        }
#endif

        private static IEnumerable<string> GetAssemblyDirectories()
        {
#if NETFRAMEWORK
            var directories = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath != null
                ? AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                : new string[] { };
#else
            var directories = GetPrivateBinPaths();
#endif

            foreach (var directory in directories.Where(directory => !string.IsNullOrWhiteSpace(directory)))
            {
                yield return directory;
            }

#if NETFRAMEWORK
            if (AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe == null)
            {
                yield return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }
#else
            yield return AppContext.BaseDirectory;
#endif
        }

        private static AssemblyName SafeGetAssemblyName(string assemblyPath)
        {
            try
            {
                return AssemblyName.GetAssemblyName(assemblyPath);
            }
            catch
            {
                return null;
            }
        }

        private static Assembly SafeLoadAssembly(AppDomain domain, AssemblyName assemblyName)
        {
            try
            {
                return domain.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }
    }
}
#endif
