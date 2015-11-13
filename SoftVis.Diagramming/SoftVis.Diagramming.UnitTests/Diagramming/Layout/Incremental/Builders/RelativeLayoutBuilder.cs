﻿using Codartis.SoftVis.Diagramming.Layout.Incremental.Relative;
using Codartis.SoftVis.Diagramming.Layout.Incremental.Relative.Logic;

namespace Codartis.SoftVis.Diagramming.UnitTests.Diagramming.Layout.Incremental.Builders
{
    internal class RelativeLayoutBuilder : BuilderBase
    {
        public LayeredGraphBuilder LayeredGraphBuilder { get; }
        public LowLevelLayoutGraphBuilder LowLevelLayoutGraphBuilder { get; }
        private readonly LayoutVertexLayers _layers;
        public RelativeLayout RelativeLayout { get; }

        public RelativeLayoutBuilder()
        {
            LayeredGraphBuilder = new LayeredGraphBuilder();
            LowLevelLayoutGraphBuilder = new LowLevelLayoutGraphBuilder(LayeredGraphBuilder);
            _layers = new LayoutVertexLayers();
            RelativeLayout = new RelativeLayout(LayeredGraphBuilder.Graph, LowLevelLayoutGraphBuilder.Graph, _layers);
        }

        public void SetUpGraphs(params string[] pathSpecification)
        {
            LayeredGraphBuilder.SetUp(pathSpecification);
            LowLevelLayoutGraphBuilder.SetUp(pathSpecification);
        }

        public void SetUpLayers(params string[] layerSpecifications)
        {
            var i = 0;
            foreach (var layerSpecification in layerSpecifications)
            {
                foreach (var vertexName in layerSpecification.Split(','))
                {
                    AddVertexToLayer(vertexName, i);
                }
                i++;
            }
        }

        private void AddVertexToLayer(string vertexName, int i)
        {
            var vertex = LowLevelLayoutGraphBuilder.GetVertex(vertexName);
            var relativeLocation = new RelativeLocation(i, _layers.GetLayer(i).Count);
            _layers.AddVertex(vertex, relativeLocation);
        }
    }
}