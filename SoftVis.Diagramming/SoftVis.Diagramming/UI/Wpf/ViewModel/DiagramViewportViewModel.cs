﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Codartis.SoftVis.Common;
using Codartis.SoftVis.Diagramming;
using Codartis.SoftVis.Modeling;
using Codartis.SoftVis.UI.Common;
using Codartis.SoftVis.UI.Extensibility;

namespace Codartis.SoftVis.UI.Wpf.ViewModel
{
    /// <summary>
    /// Tracks the change events of a diagram and creates/modifies diagram shape viewmodels accordingly.
    /// Also handles viewport transform (resize, pan and zoom) in a transitioned style (with a given transition speed).
    /// Also handles which shape has the focus and which one has the decorators (mini buttons).
    /// </summary>
    public class DiagramViewportViewModel : DiagramViewModelBase
    {
        private readonly Map<IDiagramShape, DiagramShapeViewModelBase> _diagramShapeToViewModelMap;
        private readonly DiagramShapeViewModelFactory _diagramShapeViewModelFactory;
        private readonly DiagramButtonCollectionViewModel _diagramButtonCollectionViewModel;

        private readonly Viewport _viewport;
        private double _viewportZoom;
        private TransitionedTransform _transitionedViewportTransform;

        /// <summary>The focused shape is the one that the user points to.</summary>
        private DiagramNodeViewModel _focusedDiagramNode;

        /// <summary>The decorated shape is the one that has the minibuttons attached.</summary>
        private DiagramNodeViewModel _decoratedDiagramNode;

        /// <summary>The decoration is pinned of the minibuttons stay visible even when focus is lost from a shape.</summary>
        private bool _isDecorationPinned;

        public ObservableCollection<DiagramShapeViewModelBase> DiagramShapeViewModels { get; }
        public double MinZoom { get; }
        public double MaxZoom { get; }

        public Viewport.ResizeCommand ViewportResizeCommand { get; }
        public Viewport.PanCommand ViewportPanCommand { get; }
        public Viewport.ZoomToContentCommand ViewportZoomToContentCommand { get; }
        public Viewport.ZoomCommand ViewportZoomCommand { get; }

        public event Action ViewportChanged;
        public event EntitySelectorRequestedEventHandler EntitySelectorRequested;

        public DiagramViewportViewModel(IReadOnlyModel readOnlyModel, IDiagram diagram, IDiagramBehaviourProvider diagramBehaviourProvider,
            double minZoom, double maxZoom, double initialZoom)
            : base(readOnlyModel, diagram)
        {
            _diagramShapeToViewModelMap = new Map<IDiagramShape, DiagramShapeViewModelBase>();
            _diagramShapeViewModelFactory = new DiagramShapeViewModelFactory(readOnlyModel, diagram);
            _diagramButtonCollectionViewModel = new DiagramButtonCollectionViewModel(readOnlyModel, diagram, diagramBehaviourProvider);

            _viewport = new Viewport(minZoom, maxZoom, initialZoom);
            _transitionedViewportTransform = TransitionedTransform.Identity;
            _focusedDiagramNode = null;
            _isDecorationPinned = false;
            _decoratedDiagramNode = null;

            DiagramShapeViewModels = new ObservableCollection<DiagramShapeViewModelBase>();
            MinZoom = minZoom;
            MaxZoom = maxZoom;
            ViewportResizeCommand = new Viewport.ResizeCommand(_viewport);
            ViewportPanCommand = new Viewport.PanCommand(_viewport);
            ViewportZoomToContentCommand = new Viewport.ZoomToContentCommand(_viewport);
            ViewportZoomCommand = new Viewport.ZoomCommand(_viewport);

            SubscribeToViewportEvents();
            SubscribeToDiagramEvents();
            SubscribeToDiagramButtonEvents();
            AddDiagram(diagram);
        }

        public ObservableCollection<DiagramButtonViewModelBase> DiagramButtonViewModels
            => _diagramButtonCollectionViewModel.DiagramButtonViewModels;

        public double ViewportZoom
        {
            get { return _viewportZoom; }
            set
            {
                _viewportZoom = value;
                OnPropertyChanged();
            }
        }

        public TransitionedTransform TransitionedViewportTransform
        {
            get { return _transitionedViewportTransform; }
            set
            {
                _transitionedViewportTransform = value;
                OnPropertyChanged();
            }
        }

        public DiagramNodeViewModel DecoratedDiagramNode
        {
            get { return _decoratedDiagramNode; }
            set
            {
                _decoratedDiagramNode = value;
                OnPropertyChanged();
            }
        }

        public void ZoomToContent(TransitionSpeed transitionSpeed = TransitionSpeed.Slow)
            => _viewport.ZoomToContent(transitionSpeed);

        public void PinDecoration()
        {
            _isDecorationPinned = true;
        }

        public void UnpinDecoration()
        {
            _isDecorationPinned = false;
            ChangeDecorationTo(_focusedDiagramNode);
        }

        private void SubscribeToViewportEvents()
        {
            _viewport.LinearZoomChanged += OnViewportLinearZoomChanged;
            _viewport.TransitionedTransformChanged += OnViewportTransitionedTransformChanged;
        }

        private void SubscribeToDiagramEvents()
        {
            Diagram.ShapeAdded += OnShapeAdded;
            Diagram.ShapeMoved += OnShapeMoved;
            Diagram.ShapeRemoved += OnShapeRemoved;
            Diagram.Cleared += OnDiagramCleared;
        }

        private void SubscribeToDiagramButtonEvents()
        {
            foreach (var showRelatedNodeButtonViewModel in DiagramButtonViewModels.OfType<ShowRelatedNodeButtonViewModel>())
                showRelatedNodeButtonViewModel.EntitySelectorRequested += OnEntitySelectorRequested;
        }

        private void AddDiagram(IDiagram diagram)
        {
            foreach (var diagramNode in diagram.Nodes)
                OnShapeAdded(null, diagramNode);

            foreach (var diagramConnector in diagram.Connectors)
                OnShapeAdded(null, diagramConnector);

            UpdateDiagramContentRect();
        }

        private void OnShapeAdded(object sender, IDiagramShape diagramShape)
        {
            var diagramShapeViewModel = _diagramShapeViewModelFactory.CreateViewModel(diagramShape);
            diagramShapeViewModel.GotFocus += OnShapeFocused;
            diagramShapeViewModel.LostFocus += OnShapeUnfocused;
            diagramShapeViewModel.RemoveRequested += OnShapeRemoveRequested;

            DiagramShapeViewModels.Add(diagramShapeViewModel);
            _diagramShapeToViewModelMap.Set(diagramShape, diagramShapeViewModel);

            UpdateDiagramContentRect();
            RaiseViewportChanged();
        }

        private void OnShapeMoved(object sender, IDiagramShape diagramShape)
        {
            var diagramShapeViewModel = _diagramShapeToViewModelMap.Get(diagramShape);
            diagramShapeViewModel.UpdateState();

            UpdateDiagramContentRect();
            RaiseViewportChanged();
        }

        private void OnShapeRemoved(object sender, IDiagramShape diagramShape)
        {
            var diagramShapeViewModel = _diagramShapeToViewModelMap.Get(diagramShape);
            if (diagramShapeViewModel == null)
                return;

            OnShapeUnfocused(diagramShapeViewModel);
            diagramShapeViewModel.GotFocus -= OnShapeFocused;
            diagramShapeViewModel.LostFocus -= OnShapeUnfocused;
            diagramShapeViewModel.RemoveRequested -= OnShapeRemoveRequested;

            DiagramShapeViewModels.Remove(diagramShapeViewModel);
            _diagramShapeToViewModelMap.Remove(diagramShape);

            UpdateDiagramContentRect();
            RaiseViewportChanged();
        }

        private void OnDiagramCleared(object sender, EventArgs e)
        {
            DiagramShapeViewModels.Clear();
            _diagramShapeToViewModelMap.Clear();

            UpdateDiagramContentRect();
            RaiseViewportChanged();
        }

        private void OnShapeRemoveRequested(IDiagramShape diagramShape)
        {
            Diagram.RemoveShape(diagramShape);
        }

        private void OnShapeFocused(FocusableViewModelBase focusableViewModel)
        {
            ChangeFocusTo(focusableViewModel as DiagramNodeViewModel);
        }

        private void OnShapeUnfocused(FocusableViewModelBase focusableViewModel)
        {
            ChangeFocusTo(null);
        }

        private void ChangeFocusTo(DiagramNodeViewModel diagramNodeViewModel)
        {
            _focusedDiagramNode = diagramNodeViewModel;

            if (!_isDecorationPinned)
                ChangeDecorationTo(diagramNodeViewModel);
        }

        private void ChangeDecorationTo(DiagramNodeViewModel newlyDecoratedDiagramNodeViewModel)
        {
            if (newlyDecoratedDiagramNodeViewModel == null)
                _diagramButtonCollectionViewModel.HideButtons();
            else
                _diagramButtonCollectionViewModel.AssignButtonsTo(newlyDecoratedDiagramNodeViewModel);

            DecoratedDiagramNode = newlyDecoratedDiagramNodeViewModel;
        }

        private void UpdateDiagramContentRect()
        {
            _viewport.UpdateContentRect(Diagram.ContentRect.ToWpf());
        }

        private void OnViewportLinearZoomChanged(double viewportZoom)
        {
            ViewportZoom = viewportZoom;
        }

        private void OnViewportTransitionedTransformChanged(TransitionedTransform transitionedTransform)
        {
            TransitionedViewportTransform = transitionedTransform;
            RaiseViewportChanged();
        }

        private void OnEntitySelectorRequested(Point attachPointInDiagramSpace,
            HandleOrientation handleOrientation, IEnumerable<IModelEntity> modelEntities)
        {
            var attachPointInScreenSpace = _viewport.ProjectFromDiagramSpaceToScreenSpace(attachPointInDiagramSpace);
            EntitySelectorRequested?.Invoke(attachPointInScreenSpace, handleOrientation, modelEntities);
        }

        private void RaiseViewportChanged()
        {
            ViewportChanged?.Invoke();
        }
    }
}
