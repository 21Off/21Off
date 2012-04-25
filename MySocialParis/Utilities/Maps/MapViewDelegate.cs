using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class LocationMapViewDelegate : MKMapViewDelegate
	{
		
	}
	
	public class MapViewDelegate : MKMapViewDelegate
	{
		private MKAnnotationView selectedAnnotationView;
		private AnnotationBtnAssoc _Association;
		public event Action<Image> OnPhotoClicked;
		
		public MapViewDelegate () : base()
		{
		}
		
		public override void DidSelectAnnotationView (MKMapView mapView, MKAnnotationView view)
		{
			try
			{
				if (view.Annotation is MyAnnotation)
				{
					var myAnnotation = (MyAnnotation)view.Annotation;
					
					CalloutMapAnnotation calloutAnnotation = myAnnotation.AssociatedCalloutMapAnnotation;
					if (calloutAnnotation == null)
					{
						calloutAnnotation = new CalloutMapAnnotation(myAnnotation.Coordinate, myAnnotation.Title, "");
					}
					else
					{
						calloutAnnotation.Coordinate = myAnnotation.Coordinate;
					}
					
					myAnnotation.AssociatedCalloutMapAnnotation = calloutAnnotation;
					calloutAnnotation.ParentAnnotation = myAnnotation;
				
					mapView.AddAnnotation(calloutAnnotation);
					selectedAnnotationView = view;
				}
			}
			catch (Exception ex)
			{
				Util.LogException("DidSelectAnnotationView", ex);
			}
		}
		
		public override void DidDeselectAnnotationView (MKMapView mapView, MKAnnotationView view)
		{
			if (view.Annotation is MyAnnotation)
			{
				var myAnnotation = (MyAnnotation)view.Annotation;				
				if (myAnnotation.AssociatedCalloutMapAnnotation != null)
				{
					mapView.RemoveAnnotation(myAnnotation.AssociatedCalloutMapAnnotation);
				}
			}
		}
	
		/*
		/// <summary>
		/// When user moves the map, update lat,long text in label
		/// </summary>
		public override void RegionChanged (MKMapView mapView, bool animated)
		{
			Console.WriteLine ("Region did change Map Center " + mapView.CenterCoordinate.Latitude + ", " 
			                   + mapView.CenterCoordinate.Longitude);
			
			var a = new MyAnnotation (mapView.CenterCoordinate, "test", "test");
			mapView.AddAnnotationObject (a);
		}
		*/	
		
		/// <summary>
		/// Seems to work in the Simulator now
		/// </summary>
		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
		{
			try {
				if (annotation is CalloutMapAnnotation)
				{					
					var calloutAnnotation = (CalloutMapAnnotation)annotation;					
					var canv = mapView.DequeueReusableAnnotation("CalloutAnnotation");
					if (canv == null)
					{
						var calloutMapAnnotationView = new CalloutMapAnnotationView(calloutAnnotation, "CalloutAnnotation");
						
						var blocks = new List<Block>();
						var bounds = new RectangleF(0, 0, 320, 100);
						calloutMapAnnotationView.Frame = CalloutMapAnnotationView.GetSize(calloutAnnotation, blocks, bounds);
						
						UIButton gotoBtn = UIButton.FromType(UIButtonType.DetailDisclosure);
						float diam = 47 / 2;
						
						float width = calloutMapAnnotationView.Frame.Width;
						float height = calloutMapAnnotationView.Frame.Height;
						
						gotoBtn.Frame = new RectangleF(width - 40, (height - gotoBtn.Bounds.Height) / 2, diam * 2, 
							gotoBtn.Bounds.Height);
						
						//calloutMapAnnotationView.RightCalloutAccessoryView = gotoBtn;
						calloutMapAnnotationView.AddSubview(gotoBtn);
						
						calloutMapAnnotationView.OnAnnotationClicked += HandleCalloutMapAnnotationViewOnAnnotationClicked1;
						var association = new AnnotationBtnAssoc(gotoBtn, calloutAnnotation);
						association.OnPhotoClicked += this.OnPhotoClicked;
						
						_Association = association;						
						canv = calloutMapAnnotationView;
						//canv.DrawBorder(UIColor.Red);
					}					
					else
					{
						//var calloutMapAnnotationView = (canv as CalloutMapAnnotationView);
						//UIButton gotoBtn = UIButton.FromType(UIButtonType.DetailDisclosure);
						//calloutMapAnnotationView.RightCalloutAccessoryView = gotoBtn;
						
						_Association.CalloutAnnot = annotation as CalloutMapAnnotation;
						_Association.OnPhotoClicked -= OnPhotoClicked;
						_Association.OnPhotoClicked += OnPhotoClicked;
					}
					
					var calloutAnnot = (canv as CalloutMapAnnotationView);
					calloutAnnot.CanShowCallout = true;
					
					calloutAnnot.parentAnnotationView = selectedAnnotationView;
					calloutAnnot.mapView = mapView;					
					calloutAnnot.SetAnnotation(annotation);
					
					return canv;
				}
				
				if (annotation is MyAnnotation) 
				{					
					var myAn = (MyAnnotation)annotation;
					
					var anv = mapView.DequeueReusableAnnotation ("thislocation");
					if (anv == null) 
					{
						if (myAn.Title != null) {
							var pinanv = new PinAnnotationView (annotation, "thislocation");
							myAn.Pin = pinanv;
							pinanv.Image = Graphics.GetImgResource("position");
							pinanv.CanShowCallout = false;
							pinanv.UserInteractionEnabled = true;
							
							anv = pinanv;
						} else 
						{
							return null;
						}
					}					
					else
						anv.Annotation = annotation;
					
					return anv;
				}
				else
				{
					return null;
					/*
					var pinanv = new MKPinAnnotationView (annotation, "thislocation");
					pinanv.CanShowCallout = true;
					pinanv.PinColor = MKPinAnnotationColor.Green;
					pinanv.AnimatesDrop = true;
					pinanv.UserInteractionEnabled = true;
					return pinanv;
					*/

					//return base.GetViewForAnnotation(mapView, annotation);
				}
				
			} catch (Exception ex) {
				Util.LogException("GetViewForAnnotation Exception", ex);
				return null;
			}
		}

		void HandleCalloutMapAnnotationViewOnAnnotationClicked1 (CalloutMapAnnotation obj)
		{
			Util.Log("Clicked on map annotation view");
			if (_Association != null)
				_Association.ClickImage();
		}

		public MKCircle _circle = null;
		private MKCircleView _circleView = null;

		public override MKOverlayView GetViewForOverlay (MKMapView mapView, NSObject overlay)
		{
			if ((_circle != null) && (_circleView == null)) {
				_circleView = new MKCircleView (_circle);
				_circleView.FillColor = UIColor.Cyan;
			}
			return _circleView;
		}
	}
	
}
