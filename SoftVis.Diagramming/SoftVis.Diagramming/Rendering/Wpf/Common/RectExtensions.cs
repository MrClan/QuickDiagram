﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Codartis.SoftVis.Rendering.Wpf.Common
{
    public static class RectExtensions
    {
        public static Point GetCenter(this Rect rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        public static Rect Union(this IEnumerable<Rect> rectCollection)
        {
            var rectList = rectCollection as IList<Rect> ?? rectCollection.ToList();
            return rectList.Any()
                ? new Rect(
                    new Point(rectList.Select(i => i.Left).Min(), rectList.Select(i => i.Top).Min()),
                    new Point(rectList.Select(i => i.Right).Max(), rectList.Select(i => i.Bottom).Max()))
                : Rect.Empty;
        }
    }
}
