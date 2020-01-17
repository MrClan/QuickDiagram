﻿using System.Threading;
using System.Threading.Tasks;
using Codartis.SoftVis.Modeling;
using Codartis.SoftVis.Util;

namespace Codartis.SoftVis.VisualStudioIntegration.Modeling
{
    /// <summary>
    /// Defines model operations for the application commands package.
    /// </summary>
    public interface IModelServices
    {
        /// <summary>
        /// A read-only view of the model.
        /// </summary>
        IReadOnlyModel Model { get; }

        /// <summary>
        /// Returns a value indicating whether the current symbol (the one under the caret in the active source code editor) can be added to the model.
        /// </summary>
        /// <returns>True if there is a symbol under the caret that can be added to the model.</returns>
        Task<bool> CurrentSymbolAvailableAsync();

        /// <summary>
        /// Adds the current Roslyn symbol (under the caret in the active source code editor) to the model.
        /// </summary>
        /// <returns>The model entity that corresponds to the current Roslyn symbol.</returns>
        Task<IRoslynBasedModelEntity> AddCurrentSymbolAsync();

        /// <summary>
        /// Explores related entities and adds them to the model.
        /// </summary>
        /// <param name="modelEntity">The starting model item.</param>
        /// <param name="entityRelationType">Optionally specifies what kind of relations should be explored. Null means all relations.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <param name="progress">Optional progress reporting object.</param>
        /// <param name="recursive">True means repeat exploring for related entities. Default is false.</param>
        Task ExtendModelWithRelatedEntitiesAsync(IModelEntity modelEntity, EntityRelationType? entityRelationType = null, 
            CancellationToken cancellationToken = default, IIncrementalProgress progress = null, bool recursive = false);

        /// <summary>
        /// Returns a value indicating whether a model entity has source code.
        /// </summary>
        /// <param name="modelEntity">A model entity.</param>
        /// <remarks>True if the model entity has source code, false otherwise.</remarks>
        Task<bool> HasSourceAsync(IModelEntity modelEntity);

        /// <summary>
        /// Shows the source in the host environment that corresponds to the given model entity.
        /// </summary>
        /// <param name="modelEntity">A model entity.</param>
        Task ShowSourceAsync(IModelEntity modelEntity);

        /// <summary>
        /// Updates the model from the current source code.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <param name="progress">Optional progress reporting object.</param>
        Task UpdateFromSourceAsync(CancellationToken cancellationToken = default, IIncrementalProgress progress = null);

        /// <summary>
        /// Clears the model.
        /// </summary>
        void Clear();
    }
}
