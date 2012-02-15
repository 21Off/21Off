using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace MSP.Client
{
    public class SquareView : ShapeView
    {
        public SquareView (PointF origin) : base(origin)
        {
        }

        public override void CreateShape (CGPath path, CGContext gctx)
        {
            path.AddRect (new RectangleF (_origin, new SizeF (UIScreen.MainScreen.Bounds.Width, 100)));
            gctx.AddPath (path);
            
            gctx.DrawPath (CGPathDrawingMode.FillStroke);
        }
        
    }	
}
