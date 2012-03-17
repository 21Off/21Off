using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class AnnotationBtnAssoc
	{
		public CalloutMapAnnotation CalloutAnnot {get;set;}
		
		public AnnotationBtnAssoc(UIButton button, CalloutMapAnnotation calloutAnnotation)
		{
			CalloutAnnot = calloutAnnotation;
			button.TouchDown += HandleButtonTouchUpInside;
		}

		void HandleButtonTouchUpInside (object sender, EventArgs e)
		{
			if (OnPhotoClicked != null)
			{
				if (CalloutAnnot == null || CalloutAnnot.ParentAnnotation == null)
				{
					return;
				}
				
				OnPhotoClicked(CalloutAnnot.ParentAnnotation.AssocImage);
			}
		}
		
		public void ClickImage()
		{
			if (OnPhotoClicked != null)
				OnPhotoClicked(CalloutAnnot.ParentAnnotation.AssocImage);
		}
		
		public event Action<Image> OnPhotoClicked;
	}
}
