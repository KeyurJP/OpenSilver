using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

#if MIGRATION
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace System.ComponentModel.Composition.Hosting
{
    /// <summary>
    /// Discovers attributed parts in a XAP file, and provides methods for asynchronously
    /// downloading XAP files.
    /// </summary>
    public class DeploymentCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
    {
        static class State
        {
            public const int Created = 0;
            public const int Initialized = 1000;
            public const int DownloadStarted = 2000;
            public const int DownloadCompleted = 3000;
            public const int DownloadCancelled = 4000;
        }

        private volatile bool _isDisposed = false;
        private Uri _uri = null;
        private int _state = State.Created;
        private AggregateCatalog _catalogCollection = new AggregateCatalog();
        private WebClient _webClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentCatalog"/> 
        /// class using assemblies in the current XAP.
        /// </summary>
        public DeploymentCatalog()
        {
            this.DiscoverParts(Package.CurrentAssemblies);
            this._state = State.Initialized;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentCatalog"/>
        /// class using the XAP file at the specified relative URI.
        /// </summary>
        /// <param name="uriRelative">The URI of the XAP file.</param>
        public DeploymentCatalog(string uriRelative)
        {
            if (uriRelative.Contains(".xap"))
                uriRelative = uriRelative.Replace(".xap", ".dll");
            this._uri = new Uri(uriRelative, UriKind.Relative);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentCatalog"/>
        /// class using the XAP file at the specified URI.
        /// </summary>
        /// <param name="uri">The URI of the XAP file.</param>
        public DeploymentCatalog(Uri uri)
        {
            this._uri = uri;
        }

        /// <summary>
        /// Occurs when the contents of the <see cref="DeploymentCatalog"/>
        /// have changed.
        /// </summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

        /// <summary>
        /// Occurs when the contents of the <see cref="DeploymentCatalog"/>
        /// are changing.
        /// </summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        /// <summary>
        /// Occurs when the XAP file has finished downloading, or there has been an error.
        /// </summary>		
        public event EventHandler<AsyncCompletedEventArgs> DownloadCompleted;

        /// <summary>
        /// Occurs when the download progress of the XAP file changes.
        /// </summary>		
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Gets all the parts contained in the catalog.
        /// </summary>
        /// <returns>
        /// A query enumerating all the parts contained in the catalog.
        /// </returns>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                this.ThrowIfDisposed();
                return this._catalogCollection.Parts;
            }
        }

        /// <summary>
        /// Gets the URI for the XAP file.
        /// </summary>
        public Uri Uri
        {
            get
            {
                this.ThrowIfDisposed();
                return this._uri;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="assemblies">
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="DeploymentCatalog"/> has been disposed of.
        /// </exception>
        private void DiscoverParts(IEnumerable<Assembly> assemblies)
        {
            this.ThrowIfDisposed();

            var addedDefinitions = new List<ComposablePartDefinition>();
            var addedCatalogs = new Dictionary<string, ComposablePartCatalog>();
            foreach (var assembly in assemblies)
            {
                if (addedCatalogs.ContainsKey(assembly.FullName))
                {
                    // Nothing to do because the assembly has already been added.
                    continue;
                }

                var catalog = new AssemblyCatalog(assembly);
                addedDefinitions.AddRange(catalog.Parts);
                addedCatalogs.Add(assembly.FullName, catalog);
            }

            // Generate notifications
            using (var atomicComposition = new AtomicComposition())
            {
                var changingArgs = new ComposablePartCatalogChangeEventArgs(addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), atomicComposition);
                this.OnChanging(changingArgs);

                foreach (var item in addedCatalogs)
                {
                    this._catalogCollection.Catalogs.Add(item.Value);
                }
                atomicComposition.Complete();
            }

            var changedArgs = new ComposablePartCatalogChangeEventArgs(addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), null);
            this.OnChanged(changedArgs);
        }

        /// <summary>
        /// Gets the export definitions that match the constraint expressed by the specified
        /// definition.
        /// </summary>
        /// <param name="definition">
        /// The conditions of the <see cref="ExportDefinition"/> objects to be returned.
        /// </param>
        /// <returns>
        /// A collection of <see cref="Tuple{T1, T2}"/> objects containing the <see cref="ExportDefinition"/>
        /// objects and their associated <see cref="ComposablePartDefinition"/>
        /// objects for objects that match the constraint specified by definition.
        /// </returns>
        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            this.ThrowIfDisposed();
            return this._catalogCollection.GetExports(definition);
        }

        /// <summary>
        /// Cancels the XAP file download in progress.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The method is called before <see cref="DeploymentCatalog.DownloadAsync"/>
        /// or after the <see cref="DeploymentCatalog.DownloadCompleted"/> event has occurred.
        /// </exception>
        [OpenSilver.NotImplemented]
        public void CancelAsync()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// Begins downloading the XAP file associated with the <see cref="DeploymentCatalog"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This method is called more than once, or after the <see cref="DeploymentCatalog.CancelAsync"/>
        /// method.
        /// </exception>
        public void DownloadAsync()
        {
            Exception error = null;
            // Possible valid current states are DownloadStarted and DownloadCancelled.
            int currentState = Interlocked.CompareExchange(ref this._state, State.DownloadCompleted, State.DownloadStarted);

            {
                try
                {
                    var assemblies = Package.LoadPackagedAssembliesInternal(this.Uri);
                    this.DiscoverParts(assemblies);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            this.OnDownloadCompleted(new AsyncCompletedEventArgs(error, false, this));
        }



        /// <summary>
        /// Raises the <see cref="DeploymentCatalog.Changed"/>
        /// event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="ComposablePartCatalogChangeEventArgs"/>
        /// object that contains the event data.
        /// </param>
        protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changedEvent = this.Changed;
            if (changedEvent != null)
            {
                changedEvent(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeploymentCatalog.Changing"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="ComposablePartCatalogChangeEventArgs"/> object that contains the event data.
        /// </param>
        protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changingEvent = this.Changing;
            if (changingEvent != null)
            {
                changingEvent(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeploymentCatalog.DownloadCompleted"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="AsyncCompletedEventArgs"/> object that contains the event data.
        /// </param>
		protected virtual void OnDownloadCompleted(AsyncCompletedEventArgs e)
        {
            EventHandler<AsyncCompletedEventArgs> downloadCompletedEvent = this.DownloadCompleted;
            if (downloadCompletedEvent != null)
            {
                downloadCompletedEvent(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeploymentCatalog.DownloadProgressChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="DownloadProgressChangedEventArgs"/> object that contains the event data.
        /// </param>
		protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            EventHandler<DownloadProgressChangedEventArgs> downloadProgressChangedEvent = this.DownloadProgressChanged;
            if (downloadProgressChangedEvent != null)
            {
                downloadProgressChangedEvent(this, e);
            }
        }


        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DeploymentCatalog"/>
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged
        /// resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (!this._isDisposed)
                    {
                        AggregateCatalog catalog = null;
                        try
                        {
                            catalog = this._catalogCollection;
                            this._catalogCollection = null;
                            this._isDisposed = true;
                        }
                        finally
                        {
                            if (catalog != null)
                            {
                                catalog.Dispose();
                            }
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException("Cannot access a disposed object.");
            }
        }

    }
}