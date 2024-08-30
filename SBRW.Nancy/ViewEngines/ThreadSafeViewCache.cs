namespace SBRW.Nancy.ViewEngines
{
    using System;
    using System.Collections.Concurrent;
    using Configuration;

    /// <summary>
    /// ThreadSafe implementation of <see cref="IViewCache"/>.
    /// </summary>
    /// <remarks>Solves a Memory problem when generating multiple views simultaneously (see https://github.com/NancyFx/Nancy/issues/2563).</remarks>
    public class ThreadSafeViewCache : IViewCache
    {
        private readonly ConcurrentDictionary<ViewLocationResult, object> _cache;
        private readonly object _cacheLock = new object();
        private readonly ViewConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeViewCache"/> class.
        /// </summary>
        public ThreadSafeViewCache(INancyEnvironment environment)
        {
            _cache = new ConcurrentDictionary<ViewLocationResult, object>();
            _configuration = environment.GetValue<ViewConfiguration>();
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
            if (_configuration.RuntimeViewUpdates)
            {
                if (viewLocationResult.IsStale())
                {
                    _cache.TryRemove(viewLocationResult, out var old);
                }
            }

            if (_cache.TryGetValue(viewLocationResult, out var value))
                return (TCompiledView)value;

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(viewLocationResult, out value))
                    return (TCompiledView)value;

                value = valueFactory(viewLocationResult);
                _cache.TryAdd(viewLocationResult, value);
            }

            return (TCompiledView)value;
        }
    }
}