// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MSP.Client {
	
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("RelationsViewController")]
	public partial class RelationsViewController {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private MonoTouch.UIKit.UIButton __mt_backBtn;
		
		private MonoTouch.UIKit.UILabel __mt_subTitleBtn;
		
		private MonoTouch.UIKit.UILabel __mt_titleBtn;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("view")]
		private MonoTouch.UIKit.UIView view {
			get {
				this.__mt_view = ((MonoTouch.UIKit.UIView)(this.GetNativeField("view")));
				return this.__mt_view;
			}
			set {
				this.__mt_view = value;
				this.SetNativeField("view", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("backBtn")]
		private MonoTouch.UIKit.UIButton backBtn {
			get {
				this.__mt_backBtn = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("backBtn")));
				return this.__mt_backBtn;
			}
			set {
				this.__mt_backBtn = value;
				this.SetNativeField("backBtn", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("subTitleBtn")]
		private MonoTouch.UIKit.UILabel subTitleBtn {
			get {
				this.__mt_subTitleBtn = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("subTitleBtn")));
				return this.__mt_subTitleBtn;
			}
			set {
				this.__mt_subTitleBtn = value;
				this.SetNativeField("subTitleBtn", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("titleBtn")]
		private MonoTouch.UIKit.UILabel titleBtn {
			get {
				this.__mt_titleBtn = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("titleBtn")));
				return this.__mt_titleBtn;
			}
			set {
				this.__mt_titleBtn = value;
				this.SetNativeField("titleBtn", value);
			}
		}
	}
}
