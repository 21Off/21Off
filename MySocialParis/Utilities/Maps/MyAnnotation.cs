using System;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{	
	public class CalloutMapAnnotation : MKAnnotation
	{
		public CalloutMapAnnotation(CLLocationCoordinate2D coord, string title, string subtitle)
		{
			_title = title;
			_subtitle = subtitle;
			Coordinate = coord;
		}
		
		public MyAnnotation ParentAnnotation {get;set;}
		
		private CLLocationCoordinate2D _coordinate;
		public override CLLocationCoordinate2D Coordinate {
			get { return _coordinate; }
			set { _coordinate = value; }
		}
		
		private string _title, _subtitle;
		
		public override string Title {
			get { return _title; }
		}		
		
		public override string Subtitle {
			get { return _subtitle; }
		}			
		
		public void SetTitle(string title)
		{
			_title = title;
		}
		
		public void SetSubtitle(string subtitle)
		{
			_subtitle = subtitle;
		}
	}
	
	public class MyAnnotation : MKAnnotation
	{
		public Action<MyAnnotation> OnAnnotationClicked;
		public TilingView TV {get;set;}
		public PinAnnotationView Pin { get;set; }
		public CalloutMapAnnotation AssociatedCalloutMapAnnotation {get;set;}
		public Image AssocImage {get;set;}
		
		private CLLocationCoordinate2D _coordinate;
		private string _title, _subtitle;			
		
		public override CLLocationCoordinate2D Coordinate {
			get { return _coordinate; }
			set { _coordinate = value; }
		}
		public override string Title {
			get { return _title; }
		}
		public override string Subtitle {
			get { return _subtitle; }
		}
		/// <summary>
		/// custom constructor
		/// </summary>
		public MyAnnotation (CLLocationCoordinate2D coord, string t, string s) : base()
		{
			_coordinate = coord;
			_title = t;
			_subtitle = s;
		}
		
		public void Click()
		{		
			if (OnAnnotationClicked != null)
				OnAnnotationClicked(this);	
		}
	}
}

