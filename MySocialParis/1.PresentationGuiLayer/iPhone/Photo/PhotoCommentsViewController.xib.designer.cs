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
	[MonoTouch.Foundation.Register("PhotoCommentsViewController")]
	public partial class PhotoCommentsViewController {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private MonoTouch.UIKit.UIView __mt_mainView;
		
		private MonoTouch.UIKit.UIButton __mt_backBtn;
		
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
		
		[MonoTouch.Foundation.Connect("mainView")]
		private MonoTouch.UIKit.UIView mainView {
			get {
				this.__mt_mainView = ((MonoTouch.UIKit.UIView)(this.GetNativeField("mainView")));
				return this.__mt_mainView;
			}
			set {
				this.__mt_mainView = value;
				this.SetNativeField("mainView", value);
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
	}
}
