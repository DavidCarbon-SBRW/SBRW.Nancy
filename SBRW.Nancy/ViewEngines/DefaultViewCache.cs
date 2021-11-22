﻿namespace SBRW.Nancy.ViewEngines
{
    using System;
    using System.Collections.Concurrent;
    using Configuration;

    /// <summary>
    /// Default implementation of <see cref="IViewCache"/>.
    /// </summary>
    /// <remarks>Supports expiring content if it is stale, through the <see cref="ViewConfiguration.RuntimeViewUpdates"/> setting.</remarks>
    public class DefaultViewCache : IViewCache
    {
        private readonly ConcurrentDictionary<ViewLocationResult, object> cache;
        private readonly ViewConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewCache"/> class.
        /// </summary>
        public DefaultViewCache(INancyEnvironment environment)
        {
            this.cache = new ConcurrentDictionary<ViewLocationResult, object>();
            this.configuration = environment.GetValue<ViewConfiguration>();
        }

        /// <summary>
        /// Gets or adds a view from the cache.
        /// </summary>
        /// <typeparam name="TCompiledView">The type of the cached view instance.</typeparam>
        /// <param name="viewLocationResult">A <see cref="ViewLocationResult"/> instance that describes the view that is being added or retrieved from the cache.</param>
        /// <param name="valueFactory">A function that produces the value that should be added to the cache in case it does not already exist.</param>
        /// <returns>An instance of the type specified by the <typeparamref name="TCompiledView"/> type.</returns>
        public TCompiledView GetOrAdd<TCompiledView>(ViewLocationResult viewLocationResult, Func<ViewLocationResult, TCompiledView> valueFactory)
        {
            if (this.configuration.RuntimeViewUpdates)
            {
                if (viewLocationResult.IsStale())
                {
                    object old;
                    this.cache.TryRemove(viewLocationResult, out old);
                }
            }

            return (TCompiledView)this.cache.GetOrAdd(viewLocationResult, x => valueFactory(x));
        }
    }
}
