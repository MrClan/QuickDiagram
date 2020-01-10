﻿using System.Collections.Generic;

namespace Codartis.SoftVis.Diagramming.UnitTests.Diagramming.Layout.Incremental.Helpers
{
    internal class PathSpecification : VertexList
    {
        public PathSpecification(string[] vertexNames)
            :base(vertexNames)
        {
        }

        public IEnumerable<EdgeSpecification> ToEdgeSpecifications()
        {
            for (var i = 0; i < VertexNames.Length - 1; i++)
                yield return new EdgeSpecification(VertexNames[i], VertexNames[i + 1]);
        }

        public static PathSpecification Parse(string pathString)
        {
            return new PathSpecification(InternalParse(pathString));
        }
    }
}
