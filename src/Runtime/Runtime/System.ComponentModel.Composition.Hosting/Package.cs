using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Resources;
using System.Windows;
using System.IO;
using System.Xml;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;

namespace System.ComponentModel.Composition.Hosting
{
    internal static class Package
    {

        /// <summary>
        ///     Retrieves The current list of assemblies for the application XAP load. Depends on the Deployment.Current property being setup and
        ///     so can only be accessed after the Application object has be completely constructed.
        ///     No caching occurs at this level.
        /// </summary>
        public static IEnumerable<Assembly> CurrentAssemblies
        {
            get
            {
                var assemblies = new List<Assembly>();

                // While this may seem like somewhat of a hack, walking the AssemblyParts in the active 
                // deployment object is the only way to get the list of assemblies loaded by the initial XAP. 
                foreach (AssemblyPart ap in Deployment.Current.Parts)
                {

                    if (ap.Assembly != null)
                    {
                        assemblies.Add(ap.Assembly);
                    }
                }

                return assemblies;
            }
        }

        internal static IEnumerable<Assembly> LoadPackagedAssembliesInternal(Uri uri)
        {
            var fileName = System.IO.Path.GetFileName(uri.ToString().Split('?')[0]);
            List<AssemblyCatalog> catalogsList = new List<AssemblyCatalog>();
            List<ComposablePartDefinition> partDefinitionsList = new List<ComposablePartDefinition>();

            foreach (Assembly assembly in GetAssemblies(Assembly.Load(fileName)))
            {
                yield return assembly;
            }            
        }

        private static IEnumerable<Assembly> GetAssemblies(Assembly assembly)
        {
            Assembly applicationAssembly = assembly;
            yield return applicationAssembly;
            AssemblyName[] referencedAssemblies = applicationAssembly.GetReferencedAssemblies();
            Assembly assemblyRef = null;
            for (int i = 0; i < referencedAssemblies.Length; ++i)
            {
                bool flag = false;
                try
                {
                    assemblyRef = Assembly.Load(referencedAssemblies[i]);
                }
                catch
                {
                    flag = true;
                }

                if (!flag)
                {
                    yield return assemblyRef;
                }
            }
        }

        public static IEnumerable<Assembly> LoadPackagedAssemblies(Stream packageStream)
        {
            List<Assembly> assemblies = new List<Assembly>();
            StreamResourceInfo packageStreamInfo = new StreamResourceInfo(packageStream, null);

            IEnumerable<AssemblyPart> parts = GetDeploymentParts(packageStreamInfo);

            foreach (AssemblyPart ap in parts)
            {
                StreamResourceInfo sri = Application.GetResourceStream(
                    packageStreamInfo, new Uri(ap.Source, UriKind.Relative));

                assemblies.Add(ap.Load(sri.Stream));
            }
            packageStream.Close();
            return assemblies;
        }

        /// <summary>
        ///     Only reads AssemblyParts and does not support external parts (aka Platform Extensions or TPEs).
        /// </summary>
        private static IEnumerable<AssemblyPart> GetDeploymentParts(StreamResourceInfo xapStreamInfo)
        {
            Uri manifestUri = new Uri("AppManifest.xaml", UriKind.Relative);
            StreamResourceInfo manifestStreamInfo = Application.GetResourceStream(xapStreamInfo, manifestUri);
            List<AssemblyPart> assemblyParts = new List<AssemblyPart>();
            if (manifestStreamInfo != null)
            {
                Stream manifestStream = manifestStreamInfo.Stream;
                using (XmlReader reader = XmlReader.Create(manifestStream))
                {
                    if (reader.ReadToFollowing("AssemblyPart"))
                    {
                        do
                        {
                            string source = reader.GetAttribute("Source");

                            if (source != null)
                            {
                                assemblyParts.Add(new AssemblyPart() { Source = source });
                            }
                        }
                        while (reader.ReadToNextSibling("AssemblyPart"));
                    }
                }
            }

            return assemblyParts;
        }
    }
}
