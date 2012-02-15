using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class PinAnnotationView : MKAnnotationView
	{
		public PinAnnotationView (NSObject anotation, string reuseidentifier) : base(anotation, reuseidentifier)
		{			
		}
				
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			var touchPoint = ((UITouch)touches.AnyObject).LocationInView (this);
			//Console.WriteLine (touchPoint.ToString ());
			
			base.TouchesBegan (touches, evt);
			
			((MyAnnotation)this.Annotation).Click ();
		}
	}
}
