﻿using Codartis.SoftVis.Modeling2;
using Codartis.SoftVis.Modeling2.Implementation;

namespace Codartis.SoftVis.TestHostApp.Modeling
{
    internal abstract class TestType : ImmutableModelNodeBase
    {
        public bool IsAbstract { get; }

        protected TestType(ModelItemId id, string displayName, string fullName, string description, ModelOrigin origin, bool isAbstract)
            : base(id, displayName, fullName, description, origin)
        {
            IsAbstract = isAbstract;
        }
    }
}