using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class Block {
		public string Value;
		public RectangleF Bounds;
		public UIFont Font;
		public UILineBreakMode? LineBreakMode;
		public UIColor TextColor;
		public BlockType Type = BlockType.Text;
		
		public string Tag;
		
		public object CallObject;
	}
}
