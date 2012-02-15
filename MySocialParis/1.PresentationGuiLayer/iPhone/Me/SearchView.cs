using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Web;
using System.Drawing;
using MonoTouch.CoreLocation;

namespace MSP.Client
{
	public class SearchView : DialogViewController
	{
		public SearchView (string search) : base(null)
		{
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}
	}

	public class SearchMirrorElement : StyledStringElement
	{
		string text, format;

		public string Text {
			get { return text; }
			set {
				text = value;
				Caption = String.Format (format, text);
			}
		}

		public SearchMirrorElement (string format) : base("")
		{
			this.format = format;
			TextColor = UIColor.FromRGB (0.13f, 0.43f, 0.84f);
			Font = UIFont.BoldSystemFontOfSize (18);
		}

		public override bool Matches (string test)
		{
			return !String.IsNullOrEmpty (text);
		}
	}
}

