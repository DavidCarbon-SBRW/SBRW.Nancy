namespace SBRW.Nancy.Diagnostics
{
    using SBRW.Nancy.Bootstrapper;

    /// <summary>
    /// Defines the functionality for Nancy diagnostics.
    /// </summary>
    public interface IDiagnostics
    {
        /// <summary>
        /// Initializes diagnostics
        /// </summary>
        /// <param name="pipelines">Application pipelines</param>
        void Initialize(IPipelines pipelines);
    }
}