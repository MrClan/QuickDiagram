﻿using System.Collections.Generic;
using System.Threading;
using Codartis.SoftVis.Modeling;
using Codartis.SoftVis.Util;

namespace Codartis.SoftVis.Diagramming
{
    /// <summary>
    /// Defines diagram-related operations.
    /// </summary>
    public interface IDiagramService : IDiagramStore
    {
        void ShowModelNode(IModelNode modelNode);
        void ShowModelNodes(IEnumerable<IModelNode> modelNodes, CancellationToken cancellationToken, IIncrementalProgress progress);
        void HideModelNode(IModelNode modelNode);

        void ShowModelRelationship(IModelRelationship modelRelationship);
        void HideModelRelationship(IModelRelationship modelRelationship);
    }
}