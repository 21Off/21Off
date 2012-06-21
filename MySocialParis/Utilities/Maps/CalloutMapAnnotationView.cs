using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using System.Collections.Generic;

namespace MSP.Client
{
	public class CalloutMapAnnotationView : MKAnnotationView, IReverseGeo
	{
		public MKAnnotationView parentAnnotationView;
		public MKMapView mapView;
		public RectangleF endFrame;		
		public Point offsetFromParent;
		public float contentHeight;
		
		public event Action<CalloutMapAnnotation> OnAnnotationClicked;
		
		private float _yShadowOffset;
		public float yShadowOffset
		{
			get
			{				
				return 6;
				/*
				if (_yShadowOffset == 0)
				{
					float osVersion = (float)Convert.ToDouble(UIDevice.CurrentDevice.SystemVersion);
					if (osVersion >= 3.2)
						_yShadowOffset = 6;
					else
						_yShadowOffset = -6;
				}
				return _yShadowOffset;
				*/
			}					
		}
		
		private UIView _contentView;
		public UIView contentView
		{
			get
			{
				if (_contentView == null)
				{
					_contentView = new UIView();
					_contentView.BackgroundColor = UIColor.Clear;
					_contentView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
					this.AddSubview(_contentView);
				}
				return _contentView;
			}
		}		
		
		const float CalloutMapAnnotationViewBottomShadowBufferSize = 6.0f;
		const float CalloutMapAnnotationViewContentHeightBuffer = 8.0f;
		const float CalloutMapAnnotationViewHeightAboveParent = 2.0f;
		
		public CalloutMapAnnotationView(NSObject annotation, string reuseidentifier) 
			: base(annotation, reuseidentifier)
		{			
			contentHeight = 20.0f;
			offsetFromParent = new Point(8, 0); // this works for MKPinAnnotationView		
			
			this.BackgroundColor = UIColor.Clear;
			this.Enabled = false;		
		}
		
		public void SetAnnotation(NSObject annotation)
		{
			this.Annotation = annotation;					
			
			if (this.Annotation is MKAnnotation)
			{
				ReverseGeocode((this.Annotation as MKAnnotation).Coordinate);
			}			
			
			PrepareFrameSize();
			PrepareOffset();
			PrepareContentFrame();
			
			SetNeedsDisplay();
		}
		
		public void ReverseGeocode (CLLocationCoordinate2D coord)
		{
			/*
			MKPlacemark placemark = ReverseGeocoder.ReverseGeocode(coord, this);
			if (placemark != null)
				SetPlacemark(placemark);
			*/
			
			string address = ReverseGeocoder.ReverseGeocode(coord, this);
			if (!string.IsNullOrWhiteSpace(address))
				SetAddress(address);
		}

		#region IReverseGeo implementation
		
		public void OnFoundAddress(CLLocationCoordinate2D coord, string address)
		{
			SetAddress(address);
		}
		
		public void HandleGeoCoderDelOnFailedWithError (MKReverseGeocoder arg1, NSError arg2)
		{
			string address = "No address available";						
			SetAddress(address);
			
			//Util.LogException("HandleGeoCoderDelOnFailedWithError", new Exception(arg2.ToString()));
		}

		public void HandleGeoCoderDelOnFoundWithPlacemark (MKReverseGeocoder arg1, MKPlacemark placemark)
		{
			SetPlacemark(placemark);
		}
		
		#endregion
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			
			if (OnAnnotationClicked != null)
				OnAnnotationClicked((CalloutMapAnnotation)this.Annotation);
		}		
		
		private void SetPlacemark(MKPlacemark placemark)
		{
			string address = placemark.SubThoroughfare + " " + placemark.Thoroughfare;
			SetAddress(address);			
		}
		
		private void SetAddress(string address)
		{
			if (string.IsNullOrWhiteSpace(address))
				address = "No address available";	
			
			if (this.Annotation == null)
				return;
			
			(this.Annotation as CalloutMapAnnotation).SetSubtitle(address);
			//Frame = GetSize(this.Annotation as MKAnnotation);
			
			InvokeOnMainThread(() => SetNeedsDisplay());
		}
		
		private void PrepareFrameSize()
		{
			if (mapView == null)
				return;
			
			RectangleF frame = this.Frame;
			
			float height = contentHeight + 
				CalloutMapAnnotationViewContentHeightBuffer + 
				CalloutMapAnnotationViewBottomShadowBufferSize - 
				offsetFromParent.Y;
			
			frame.Size = new SizeF(mapView.Frame.Size.Width, height);
		}
		
		private void PrepareOffset()
		{
			if (mapView == null)
				return;
			
			PointF parentOrigin = mapView.ConvertPointFromView(parentAnnotationView.Frame.Location, parentAnnotationView.Superview);
			float xOffset = mapView.Frame.Size.Width / 2 - (parentOrigin.X + offsetFromParent.X);
			
			//Add half our height plus half of the height of the annotation we are tied to so that our bottom lines up to its top
			//Then take into account its offset and the extra space needed for our drop shadow
			float yOffset = -(this.Frame.Size.Height / 2 + 
								parentAnnotationView.Frame.Size.Height / 2) + 
								offsetFromParent.Y + 
								CalloutMapAnnotationViewBottomShadowBufferSize;
			
			this.CenterOffset = new PointF(xOffset, yOffset);
		}	
		
		private void PrepareContentFrame()
		{
			RectangleF contentFrame = new RectangleF(this.Bounds.Location.X + 10, 
											 this.Bounds.Location.Y + 3, 
											 this.Bounds.Size.Width - 20, 
											 contentHeight);
		
			contentView.Frame = contentFrame;
		}
		
		private void adjustMapRegionIfNeeded()
		{
		}
		
		/*		
		private float xTransformForScale_scale {
			float xDistanceFromCenterToParent = this.endFrame.size.width / 2 - this.relativeParentXPosition;
			return (xDistanceFromCenterToParent * scale) - xDistanceFromCenterToParent;
		}
		
		private float yTransformForScale_scale {
			float yDistanceFromCenterToParent = (((this.endFrame.size.height) / 2) + self.offsetFromParent.y + CalloutMapAnnotationViewBottomShadowBufferSize + CalloutMapAnnotationViewHeightAboveParent);
			return yDistanceFromCenterToParent - yDistanceFromCenterToParent * scale;
		}
		*/		
		
		private void animateIn() 
		{
			/*
			//self.endFrame = self.frame;
			float scale = 0.001f;
						
			this.Transform = new CGAffineTransform(scale, 0.0f, 0.0f, scale, 
				this.xTransformForScale_scale, this.yTransformForScale_scale);
			
			UIView.BeginAnimations("animateIn");
			UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut);
			UIView.SetAnimationDuration(0.075);
			
			UIView.SetAnimationDidStopSelector(null);
			//[UIView setAnimationDidStopSelector:@selector(animateInStepTwo)];
			
			UIView.SetAnimationDelegate(this);
			scale = 1.1;
			this.Transform = new CGAffineTransform(scale, 0.0f, 0.0f, scale, 
				this.xTransformForScale_scale, this.yTransformForScale_scale);
			
			UIView.CommitAnimations();
			*/
		}
		
		public override void MovedToSuperview ()
		{
			base.MovedToSuperview ();			
			adjustMapRegionIfNeeded();
			
			this.animateIn();
		}
		
		public override void Draw (RectangleF rect)
		{
			//base.Draw (rect);
			
			float radius = 7.0f;
			float stroke = 1.0f;
						
			var context = UIGraphics.GetCurrentContext ();
			
			rect.Size.Width -= stroke + 14f;
			rect.Size.Height -= stroke + CalloutMapAnnotationViewHeightAboveParent
				- offsetFromParent.Y + CalloutMapAnnotationViewBottomShadowBufferSize;
			
			rect.Location.X += stroke / 2.0f + 7.0f;
			rect.Location.Y += stroke / 2.0f;
			
			SizeF size = new SizeF(rect.Size);
			size.Height -= 8;			

			var path = new CGPath ();
			path.MoveToPoint (rect.Location.X, rect.Location.Y + radius);
			path.AddLineToPoint(rect.Location.X, rect.Location.Y + size.Height - radius);									
			
			path.AddArc (rect.Location.X + radius, rect.Location.Y + size.Height - radius, 
			                    radius, (float)Math.PI, (float)Math.PI / 2.0f, true);
			/*
			 * 
			 * 
			 * 
			 */
			 
			path.AddArc(rect.Location.X + rect.Size.Width - radius, rect.Location.Y + size.Height - radius,
			            radius, (float)Math.PI / 2.0f, 0.0f, true);
						
			path.AddLineToPoint(rect.Location.X + size.Width, rect.Location.Y + radius);			
			path.AddArc(rect.Location.X + size.Width - radius, rect.Location.Y + radius,
			            radius, 0.0f, - (float)Math.PI / 2.0f, true);					
			
			path.AddLineToPoint(rect.Location.X + radius, rect.Location.Y);			
			
			path.AddArc(rect.Location.X + radius, rect.Location.Y + radius, radius,
			            - (float)Math.PI / 2.0f, (float)Math.PI, true);
			
			path.CloseSubpath ();
			
			float red, green, blue, alpha;
			UIColor.Black.GetRGBA(out red, out green, out blue, out alpha);
			UIColor color = UIColor.FromRGBA(red, green, blue, .6f);
			color.SetFill();
						
			var bounds = Bounds;
			var b = new SizeF(16, 8);
			context.MoveTo ((bounds.Width - b.Width) / 2 - 8, bounds.Height - 2 - b.Height);
			context.AddLineToPoint (bounds.Width/2 - 8, bounds.Height - 2);
			context.AddLineToPoint ((bounds.Width + b.Width) / 2 - 8, bounds.Height - b.Height - 2);
			context.ClosePath ();			
			
			context.AddPath(path);
			context.SaveState();
			context.SetShadowWithColor (new SizeF (0, yShadowOffset), 6, UIColor.FromWhiteAlpha(0.0f, 0.5f).CGColor);
			context.FillPath();
			context.RestoreState();					
			
			if (Annotation is CalloutMapAnnotation)
			{
				var annotation = (Annotation as CalloutMapAnnotation);
				blocks = new List<Block>();
				GetSize(annotation, blocks, bounds);
				
				foreach (Block block in blocks)
				{
					if (block.Type == BlockType.Text)
					{
						block.TextColor.SetColor();
						if (block.LineBreakMode.HasValue)
							DrawString (block.Value, block.Bounds, block.Font, block.LineBreakMode.Value, UITextAlignment.Left);
						else
							DrawString (block.Value, block.Bounds, block.Font);
					}
				}
			}			
		}
		
		List<Block> blocks = new List<Block>();
		
		public static RectangleF GetSize(MKAnnotation annotation, List<Block> blocks, RectangleF bounds)
		{			
			var size = new SizeF(0,0);
			
			float diam = 47 / 2;
			float width = size.Width + 2 * 3 + diam + 3;
			float height = size.Height + 8;			
			
			try
			{
				string title = annotation.Title;
				string subTitle = annotation.Subtitle;				
				
				if (title != null)
				{				
					float subTitleHeightPadY = 3;
					float dimWidth = 0;	
					
					using (var nss = new NSString (title)){
						UIFont font = UIFont.SystemFontOfSize(17);
						var dim = nss.StringSize (font, bounds.Width - diam - 3 * 3, UILineBreakMode.TailTruncation);
						var placement = new RectangleF(3, subTitleHeightPadY, dim.Width, dim.Height);						
						
						var block = new Block()
						{
							Font = font,
							Value = title,
							LineBreakMode = UILineBreakMode.TailTruncation,
							TextColor = UIColor.White,
							Bounds = placement,
						};
						
						blocks.Add(block);
						
						dimWidth = dim.Width;
						subTitleHeightPadY += dim.Height;
					}
					subTitleHeightPadY += 3;
					
					if (subTitle != null)
					{
						using (var nss = new NSString (subTitle)){
							UIFont font = UIFont.SystemFontOfSize(12);
							var dim = nss.StringSize (font, bounds.Width - diam - 3 * 3, UILineBreakMode.TailTruncation);						
							var placement = new RectangleF(3, subTitleHeightPadY, dim.Width, dim.Height);
							
							var block = new Block()
							{
								Font = font,
								Value = subTitle,
								LineBreakMode = UILineBreakMode.TailTruncation,
								TextColor = UIColor.White,
								Bounds = placement,
							};
						
							blocks.Add(block);
							
							dimWidth = Math.Max(dimWidth, dim.Width);
							subTitleHeightPadY += dim.Height;
						}
					}
					
					subTitleHeightPadY += 3;
					size = new SizeF(dimWidth, subTitleHeightPadY); 
				}
							
				width = size.Width + 2 * 3 + diam + 3;
				height = size.Height + 23;
			
			}
			catch (Exception ex)
			{
				Util.LogException("GetSize", ex);
			}
			return new RectangleF(5, 2, width, height);			
		}
	}
}
