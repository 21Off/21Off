using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class UIUtils
	{
		public static void SetTitle(string text, string subTitle, UILabel titleTxt, UILabel worldTxt)
		{
			titleTxt.Text = text;
			if (worldTxt != null)
				worldTxt.Text = subTitle;			
			
			SizeF sizeF = GetTextSize(text, titleTxt);
			PointF point = titleTxt.Frame.Location;
			point.X = (UIScreen.MainScreen.Bounds.Width - sizeF.Width) / 2;
			
			titleTxt.Frame = new RectangleF(point, sizeF);
			
			if (worldTxt != null)
			{
				sizeF = GetTextSize(subTitle, worldTxt);
				point = worldTxt.Frame.Location;
				point.X = (UIScreen.MainScreen.Bounds.Width - sizeF.Width) / 2;
				point.Y = 25;
					
				worldTxt.Frame = new RectangleF(point, sizeF);
			}
		}

		public static SizeF GetTextSize (string text, UILabel label)
		{
			return new NSString (text).StringSize (label.Font, UIScreen.MainScreen.Bounds.Width, UILineBreakMode.CharacterWrap);
		}
	}
}
