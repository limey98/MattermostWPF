using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mattermost.Utils
{
    class Attached
    {
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(Attached), new PropertyMetadata(0.0, OnVerticalOffsetPropertyChanged));
        public static readonly DependencyProperty ScrollableHeightProperty = DependencyProperty.RegisterAttached("ScrollableHeight", typeof(double), typeof(Attached), new PropertyMetadata(0.0));
        static readonly DependencyProperty VerticalScrollbarProperty = DependencyProperty.RegisterAttached("VerticalScrollBar", typeof(ScrollBar), typeof(Attached), new PropertyMetadata(null));
        
        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }
        
        public static double GetScrollableHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(ScrollableHeightProperty);
        }

        public static void SetScrollableHeight(DependencyObject obj, double value)
        {
            obj.SetValue(ScrollableHeightProperty, value);
        }

        static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer sv = d as ScrollViewer;

            if (sv != null)
            {
                if (sv.GetValue(VerticalScrollbarProperty) == null)
                {
                    sv.LayoutUpdated += (s, ev) =>
                    {
                        if (sv.GetValue(VerticalScrollbarProperty) == null)
                        {
                            GetScrollbarsForScrollViewer(sv);
                        }

                        SetScrollableHeight(sv, sv.ScrollableHeight);
                    };

                    sv.SizeChanged += (s, ev) =>
                    {
                        SetScrollableHeight(sv, sv.ScrollableHeight);
                    };
                }
                else
                {
                    sv.ScrollToVerticalOffset((double)e.NewValue);
                }
            }
        }

        static void GetScrollbarsForScrollViewer(ScrollViewer sv)
        {
            ScrollBar scroll = sv.FindChild<ScrollBar>(s => s.Orientation == Orientation.Vertical);

            if (scroll != null)
            {
                sv.SetValue(VerticalScrollbarProperty, scroll);
                sv.ScrollToVerticalOffset(GetVerticalOffset(sv));

                scroll.ValueChanged += (s, e) => SetVerticalOffset(sv, e.NewValue);
            }
        }
    }
}
