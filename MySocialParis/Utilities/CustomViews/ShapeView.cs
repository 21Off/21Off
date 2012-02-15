using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace MSP.Client
{
    public abstract class ShapeView : UIView
    {
        UIColor[] _colors;

        protected PointF _origin;

        public ShapeView (PointF origin)
        {
            BackgroundColor = UIColor.Clear;
            _origin = origin;
            _colors = new UIColor[] { UIColor.Red, UIColor.Blue, UIColor.Green, UIColor.Yellow, UIColor.Purple, UIColor.FromPatternImage (UIImage.FromFile ("images/around.png")) };
        }

        public override void Draw (RectangleF rect)
        {
            base.Draw (rect);
            
            CGContext gctx = UIGraphics.GetCurrentContext ();
            
            gctx.SetLineWidth (4);
            
            _colors[new Random ().Next (6)].SetFill ();
            
            UIColor.Black.SetStroke ();
            
            var path = new CGPath ();
            
            CreateShape (path, gctx);       
        }

        public abstract void CreateShape (CGPath path, CGContext gctx);
    }
	

}

