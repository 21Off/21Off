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
	[MonoTouch.Foundation.Register("AuthentificationViewController")]
	public partial class AuthentificationViewController {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private MonoTouch.UIKit.UIButton __mt_okBtn;
		
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
		
		[MonoTouch.Foundation.Connect("okBtn")]
		private MonoTouch.UIKit.UIButton okBtn {
			get {
				this.__mt_okBtn = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("okBtn")));
				return this.__mt_okBtn;
			}
			set {
				this.__mt_okBtn = value;
				this.SetNativeField("okBtn", value);
			}
		}
	}
}