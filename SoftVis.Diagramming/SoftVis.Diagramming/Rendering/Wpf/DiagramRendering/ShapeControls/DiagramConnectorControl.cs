﻿using System.Windows;

namespace Codartis.SoftVis.Rendering.Wpf.DiagramRendering.ShapeControls
{
    public class DiagramConnectorControl : DiagramShapeControlBase
    {
        static DiagramConnectorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramConnectorControl), new FrameworkPropertyMetadata(typeof(DiagramConnectorControl)));
        }

        public static readonly DependencyProperty SourceRectProperty =
            DependencyProperty.Register("SourceRect", typeof(Rect), typeof(DiagramConnectorControl),
                new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty TargetRectProperty =
            DependencyProperty.Register("TargetRect", typeof(Rect), typeof(DiagramConnectorControl),
                new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty RoutePointsProperty =
            DependencyProperty.Register("RoutePoints", typeof(Point[]), typeof(DiagramConnectorControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public Rect SourceRect
        {
            get { return (Rect)GetValue(SourceRectProperty); }
            internal set { SetValue(SourceRectProperty, value); }
        }

        public Rect TargetRect
        {
            get { return (Rect)GetValue(TargetRectProperty); }
            internal set { SetValue(TargetRectProperty, value); }
        }

        public Point[] RoutePoints
        {
            get { return (Point[])GetValue(RoutePointsProperty); }
            set { SetValue(RoutePointsProperty, value); }
        }
    }
}