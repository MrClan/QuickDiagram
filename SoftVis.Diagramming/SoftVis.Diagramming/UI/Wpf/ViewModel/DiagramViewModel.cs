﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Codartis.SoftVis.Diagramming;
using Codartis.SoftVis.Geometry;
using Codartis.SoftVis.Modeling;
using Codartis.SoftVis.Util;
using Codartis.SoftVis.Util.UI.Wpf.ViewModels;

namespace Codartis.SoftVis.UI.Wpf.ViewModel
{
    /// <summary>
    /// Top level view model of the diagram control.
    /// </summary>
    public class DiagramViewModel : DiagramViewModelBase, IDisposable
    {
        private Rect _diagramContentRect;

        public DiagramViewportViewModel DiagramViewportViewModel { get; }
        public RelatedEntityListBoxViewModel RelatedEntityListBoxViewModel { get; }
        public AutoHidePopupTextViewModel PopupTextViewModel { get; }

        public DiagramViewModel(IArrangedDiagram diagram, double minZoom, double maxZoom, double initialZoom)
            : base(diagram)
        {
            DiagramViewportViewModel = new DiagramViewportViewModel(diagram, minZoom, maxZoom, initialZoom);

            RelatedEntityListBoxViewModel = new RelatedEntityListBoxViewModel();
            RelatedEntityListBoxViewModel.ItemSelected += OnRelatedEntitySelected;

            PopupTextViewModel = new AutoHidePopupTextViewModel();

            SubscribeToDiagramEvents();
            SubscribeToViewportEvents();
        }

        public void Dispose()
        {
            RelatedEntityListBoxViewModel.ItemSelected -= OnRelatedEntitySelected;

            UnsubscribeFromDiagramEvents();
            UnsubscribeFromViewportEvents();

            DiagramViewportViewModel.Dispose();
        }

        public IEnumerable<DiagramNodeViewModel> DiagramNodeViewModels => DiagramViewportViewModel.DiagramNodeViewModels;
        public IEnumerable<DiagramConnectorViewModel> DiagramConnectorViewModelsModels => DiagramViewportViewModel.DiagramConnectorViewModels;

        public Rect DiagramContentRect
        {
            get { return _diagramContentRect; }
            set
            {
                _diagramContentRect = value;
                OnPropertyChanged();
            }
        }

        public void ZoomToContent() => DiagramViewportViewModel.ZoomToContent();
        public bool IsDiagramContentVisible() => DiagramViewportViewModel.IsDiagramContentVisible();

        public void ShowPopupMessage(string text, TimeSpan hideAfter = default(TimeSpan))
        {
            PopupTextViewModel.Text = text;
            PopupTextViewModel.AutoHideAfter = hideAfter;
            PopupTextViewModel.Show();
        }

        private void SubscribeToViewportEvents()
        {
            DiagramViewportViewModel.InputReceived += OnViewportInputReceived;
            DiagramViewportViewModel.ShowEntitySelectorRequested += OnShowRelatedEntitySelectorRequested;
            DiagramViewportViewModel.DiagramShapeRemoveRequested += OnDiagramShapeRemoveRequested;
        }

        private void UnsubscribeFromViewportEvents()
        {
            DiagramViewportViewModel.InputReceived -= OnViewportInputReceived;
            DiagramViewportViewModel.ShowEntitySelectorRequested -= OnShowRelatedEntitySelectorRequested;
            DiagramViewportViewModel.DiagramShapeRemoveRequested -= OnDiagramShapeRemoveRequested;
        }

        private void OnDiagramShapeRemoveRequested(DiagramShapeViewModelBase diagramShapeViewModel)
        {
            if (RelatedEntityListBoxViewModel.OwnerDiagramShape == diagramShapeViewModel)
                OnViewportInputReceived();
        }

        private void OnShowRelatedEntitySelectorRequested(ShowRelatedNodeButtonViewModel diagramNodeButtonViewModel, IEnumerable<IModelEntity> modelEntities)
        {
            DiagramViewportViewModel.PinDecoration();
            RelatedEntityListBoxViewModel.Show(diagramNodeButtonViewModel, modelEntities);
        }

        private void OnViewportInputReceived()
        {
            HideAllWidgets();
        }

        private void OnCleared()
        {
            HideAllWidgets();
            UpdateDiagramContentRect();
        }

        private void OnRelatedEntitySelected(IModelEntity selectedEntity)
        {
            Diagram.ShowItem(selectedEntity);

            var remainingEntities = RelatedEntityListBoxViewModel.Items.Except(selectedEntity.ToEnumerable()).ToList();

            if (remainingEntities.Any())
                RelatedEntityListBoxViewModel.Items = remainingEntities;
            else
                HideAllWidgets();
        }

        private void HideAllWidgets()
        {
            DiagramViewportViewModel.UnpinDecoration();
            RelatedEntityListBoxViewModel.Hide();
            PopupTextViewModel.Hide();
        }

        private void UpdateDiagramContentRect()
        {
            DiagramContentRect = Diagram.ContentRect.ToWpf();
        }

        private void OnDiagramShapeAdded(IDiagramShape diagramShape) => UpdateDiagramContentRect();
        private void OnDiagramShapeRemoved(IDiagramShape diagramShape) => UpdateDiagramContentRect();
        private void OnDiagramNodeSizeChanged(IDiagramNode diagramNode, Size2D oldSize, Size2D newSize) => UpdateDiagramContentRect();
        private void OnDiagramNodeTopLeftChanged(IDiagramNode diagramNode, Point2D oldTopLeft, Point2D newTopLeft) => UpdateDiagramContentRect();
        private void OnDiagramConnectorRouteChanged(IDiagramConnector diagramConnector, Route oldRoute, Route newRoute) => UpdateDiagramContentRect();

        private void SubscribeToDiagramEvents()
        {
            Diagram.ShapeAdded += OnDiagramShapeAdded;
            Diagram.ShapeRemoved += OnDiagramShapeRemoved;
            Diagram.DiagramCleared += OnCleared;
            Diagram.NodeSizeChanged += OnDiagramNodeSizeChanged;
            Diagram.NodeTopLeftChanged += OnDiagramNodeTopLeftChanged;
            Diagram.ConnectorRouteChanged += OnDiagramConnectorRouteChanged;
        }

        private void UnsubscribeFromDiagramEvents()
        {
            Diagram.ShapeAdded -= OnDiagramShapeAdded;
            Diagram.ShapeRemoved -= OnDiagramShapeRemoved;
            Diagram.DiagramCleared -= OnCleared;
            Diagram.NodeSizeChanged -= OnDiagramNodeSizeChanged;
            Diagram.NodeTopLeftChanged -= OnDiagramNodeTopLeftChanged;
            Diagram.ConnectorRouteChanged -= OnDiagramConnectorRouteChanged;
        }
    }
}
