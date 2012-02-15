using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class TilingView : UIButton, IImageUpdated
	{
		[Export("layerClass")]
		public static Class LayerClass ()
		{
			return new Class (typeof(CATiledLayer));
		}

		public TilingView (Image image, SizeF size) : base(new RectangleF (0, 0, size.Width, size.Height))
		{
			Layer.MasksToBounds = false;
			Layer.BorderColor = UIColor.LightGray.CGColor;
			Layer.BorderWidth = 0.5F;
			Layer.BackgroundColor = UIColor.FromRGBA(1.0f, 1.0f, 1.0f, 1.0F).CGColor;
			Layer.CornerRadius = 6;
			Layer.ShadowColor = UIColor.Gray.CGColor;
			Layer.ShadowOpacity = 0.5f;
			Layer.ShadowRadius = 0.5f;
			Layer.ShadowOffset = new SizeF(-0.5f, 0.5f);
			
			photoSize = new SizeF (this.Frame.Width - 6, this.Frame.Height - 6);
			
			//var tiledLayer = (CATiledLayer)this.Layer;
			//tiledLayer.LevelsOfDetail = 4;
			
			if (image == null)
			{
				_Photo = GetTakePhotoImage();			
				this.SetImage (_Photo, UIControlState.Normal);
			}
			else
				_Image = image;
			//	Update (image);
		}
		
		public UIView ParentHolder {get;set;}

		public Image _Image { get; set; }
		private Image drawedImage;
		public RectangleF ParentFrame { get;set; }
		
		public MyAnnotation Annotation { get; set; }

		private UIImage _Photo;

		public override void Draw (RectangleF rect)
		{
			var context = UIGraphics.GetCurrentContext ();
			
			// get the scale from the context by getting the current transform matrix, then asking for
			// its "a" component, which is one of the two scale components. We could also ask for "d".
			// This assumes (safely) that the view is being scaled equally in both dimensions.
			var scale = context.GetCTM ().xx;
			// .a			// http://developer.apple.com/library/ios/#documentation/GraphicsImaging/Reference/CGAffineTransform/Reference/reference.html#//apple_ref/doc/c_ref/CGAffineTransform
			var tiledLayer = (CATiledLayer)this.Layer;
			var tileSize = tiledLayer.TileSize;
			
			tileSize.Width /= scale;
			tileSize.Height /= scale;
			
			base.Draw (rect);							

			//context.SetAllowsAntialiasing (true);
			//context.SetShouldAntialias (true);
			
			if (_Photo == null) 
			{
				float [] colors = new float [] {
					221.0f/255, 221.0f/255, 221.0f/255, 0.99f,
					213.1f/255, 213.1f/255, 221.1f/255, 0.99f
				};				
				
				context.DrawImage(rect, UIImageUtils.MakeEmptyWithGradient(photoSize, colors).CGImage);
				
				Update(_Image);
			}
			else
			{
				//_Photo.Draw (new RectangleF(new PointF(0, 0), photoSize), CGBlendMode.Normal, 1.0f);
			}
		}

		private SizeF photoSize;

		public void Update (Image image)
		{
			if (image != drawedImage)
			{			   
				_Photo = null;
				drawedImage = image;
				
				UIImage filePic = ImageStore.RequestFullPicture(image.Id, image.UserId, SizeDB.Size75, this);
				if (filePic !=  null)
				{
					var resImg = UIImageUtils.resizeImage (filePic, photoSize);
					resImg = GraphicsII.RemoveSharpEdges(resImg, photoSize.Width, 4);
										
					BeginInvokeOnMainThread (() =>
					{
						_Photo = resImg;
						this.SetImage(_Photo, UIControlState.Normal);
						SetNeedsDisplay ();
					});
				}				
			}
		}
		
		public UIImage GetTakePhotoImage()
		{
			var resImg = UIImageUtils.drawEmptyImage(photoSize);
			
			resImg = UIImageUtils.overlayImage(resImg, UIImageUtils.FromFile("Images/Home/New2/icone_photo.png", new SizeF(26, 26)),
                          new RectangleF(new PointF(0, 0), photoSize),
                          new RectangleF(new PointF((photoSize.Width - 26)/2, (photoSize.Height - 26)/2),
                          new SizeF(26, 26)), photoSize);
			
			return GraphicsII.RemoveSharpEdges(resImg, photoSize.Width, 4);
		}

		#region IImageUpdated implementation
		public void UpdatedImage (long id, long userid, SizeDB sizeDB)
		{
			if (_Image != null && _Image.Id != id)
				return;
			
			UIImage filePic = ImageStore.GetLocalFullPicture(id, userid, SizeDB.Size75);
			if (filePic != null)
			{			
				var resImg = UIImageUtils.resizeImage (filePic, photoSize);
				resImg = GraphicsII.RemoveSharpEdges(resImg, photoSize.Width, 4);
									
				BeginInvokeOnMainThread (() =>
				{
					_Photo = resImg;
					this.SetImage(_Photo, UIControlState.Normal);
					SetNeedsDisplay ();
				});
			}
			else
			{
				var resImg = GetTakePhotoImage();
				
				BeginInvokeOnMainThread (() =>
				{
					_Photo = resImg;
					this.SetImage (_Photo, UIControlState.Normal);						
					SetNeedsDisplay();
				});
			}				
		}
		#endregion
}
}
