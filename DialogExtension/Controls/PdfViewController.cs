
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using System.Drawing;

namespace NPhotoViewController
{
	public class PdfViewController : UIViewController
	{
		NSUrl Url;
		UIView ContentView;
		CGPDFDocument PdfDocument;
		CGPDFPage PdfPage;
		UIScrollView ScrollView;
		CATiledLayer TiledLayer;
		RectangleF PdfPageRect;

		public PdfViewController (NSUrl url) : base()
		{
			Url = url;
			View = new UIView ();
			View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
			View.AutosizesSubviews = true;
			
			PdfDocument = CGPDFDocument.FromUrl (Url.ToString ());
			
			// For demo purposes, show first page only.
			PdfPage = PdfDocument.GetPage (1);
			PdfPageRect = PdfPage.GetBoxRect (CGPDFBox.Crop);
			
			// Setup tiled layer.
			TiledLayer = new CATiledLayer ();
			TiledLayer.Delegate = new TiledLayerDelegate (this);
			TiledLayer.TileSize = new SizeF (1024f, 1024f);
			TiledLayer.LevelsOfDetail = 5;
			TiledLayer.LevelsOfDetailBias = 5;
			TiledLayer.Frame = PdfPageRect;
			
			ContentView = new UIView (PdfPageRect);
			ContentView.Layer.AddSublayer (TiledLayer);
			
			// Prepare scroll view.
			ScrollView = new UIScrollView (View.Frame);
			ScrollView.AutoresizingMask = View.AutoresizingMask;
			ScrollView.Delegate = new ScrollViewDelegate (this);
			ScrollView.ContentSize = PdfPageRect.Size;
			ScrollView.MaximumZoomScale = 10f;
			ScrollView.MinimumZoomScale = 1f;
			ScrollView.ScrollEnabled = true;
			ScrollView.AddSubview (ContentView);
			View.AddSubview (ScrollView);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			var page = PdfPage;
			var document = PdfDocument;
			var contentView = ContentView;
			var tiledLayer = TiledLayer;
			var scrollView = ScrollView;
			
			PdfPage = null;
			PdfDocument = null;
			ContentView = null;
			TiledLayer = null;
			ScrollView = null;
			
			scrollView.RemoveFromSuperview ();
			tiledLayer.RemoveFromSuperLayer ();
			page.Dispose ();
			document.Dispose ();
			contentView.Dispose ();
			scrollView.Dispose ();
			tiledLayer.Dispose ();
		}

		public class TiledLayerDelegate : CALayerDelegate
		{
			PdfViewController ParentController;

			public TiledLayerDelegate (PdfViewController parentController) : base()
			{
				ParentController = parentController;
			}

			public override void DrawLayer (CALayer layer, CGContext context)
			{
				context.SaveState ();
				context.SetRGBFillColor (1.0f, 1.0f, 1.0f, 1.0f);
				context.FillRect (context.GetClipBoundingBox ());
				context.TranslateCTM (0.0f, layer.Bounds.Size.Height);
				context.ScaleCTM (1.0f, -1.0f);
				context.ConcatCTM (ParentController.PdfPage.GetDrawingTransform (CGPDFBox.Crop, layer.Bounds, 0, true));
				context.DrawPDFPage (ParentController.PdfPage);
				context.RestoreState ();
			}
		}

		public class ScrollViewDelegate : UIScrollViewDelegate
		{
			PdfViewController ParentController;

			public ScrollViewDelegate (PdfViewController parentController) : base()
			{
				ParentController = parentController;
			}


			public override UIView ViewForZoomingInScrollView (UIScrollView scrollView)
			{
				return ParentController.ContentView;
			}
		}
	}
}
