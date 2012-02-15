using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class ContentReporter : UIAlertView
	{
		private Image _image;
        private UITextField oTxtInput;

		public ContentReporter ( string sTitle, string sMessage, string sCancel, params string[] aOtherButtons ) 
			: base( sTitle, sMessage, null, sCancel, aOtherButtons )
        {
            this.KeyboardType = UIKeyboardType.ASCIICapable;
            this.KeyboardReturnType = UIReturnKeyType.Done;
            this.InputFieldTextAlignment = UITextAlignment.Left;
            //this.InputFieldCapitalization = UITextAutocapitalizationType.AllCharacters;
			this.InputFieldCapitalization = UITextAutocapitalizationType.Sentences;
            this.InputFieldAutocorrection = UITextAutocorrectionType.No;			
            this.InputFieldIsSecure = false;
            this.InputFieldPlaceholder = "";
            this.Presented += delegate
            {
                this.oTxtInput.BecomeFirstResponder ();
                this.Transform = CGAffineTransform.MakeTranslation ( 0, -100 );
            };
        }
        
        public string EnteredText
        {
            get
            {
                return this.oTxtInput.Text;
            }
        }
		
		public Image SelectedImage
		{
			set
			{
				_image = value;
			}
		}
        
        public UIKeyboardType KeyboardType
        {
            get;
            set;
        }
        
        public UIReturnKeyType KeyboardReturnType
        {
            get;
            set;
        }
        
        public UITextAlignment InputFieldTextAlignment
        {
            get;
            set;
        }
        
        public UITextAutocapitalizationType InputFieldCapitalization
        {
            get;
            set;
        }
        
        public UITextAutocorrectionType InputFieldAutocorrection
        {
            get;
            set;
        }
        
        public bool InputFieldIsSecure
        {
            get;
            set;
        }
        
        public string InputFieldPlaceholder
        {
            get;
            set;
        }
        
        public override void Show ()
        {
            base.Show ();
			            
            this.oTxtInput = new UITextField ( new System.Drawing.RectangleF ( 12f, 75f, 260f, 25f ) );
            this.oTxtInput.BackgroundColor = UIColor.White;
            this.oTxtInput.UserInteractionEnabled = true;
            this.oTxtInput.KeyboardType = this.KeyboardType;
            this.oTxtInput.ReturnKeyType = this.KeyboardReturnType;
            this.oTxtInput.TextAlignment = this.InputFieldTextAlignment;
            this.oTxtInput.AutocapitalizationType = this.InputFieldCapitalization;
            this.oTxtInput.AutocorrectionType = this.InputFieldAutocorrection;
            this.oTxtInput.SecureTextEntry = this.InputFieldIsSecure;
            this.oTxtInput.Placeholder = this.InputFieldPlaceholder;
            
            this.Frame = new RectangleF ( this.Frame.X, this.Frame.Y, this.Frame.Size.Width, this.Frame.Size.Height + this.oTxtInput.Bounds.Height + 20 );
            
            this.fInitialHeight = this.Bounds.Height;

            this.AddSubview ( this.oTxtInput );
            this.Superview.SetNeedsLayout ();
            this.SetNeedsLayout ();
            this.fInitialY = this.Frame.Y;
        }
        private float fInitialHeight;
        private float fInitialY;
        
        public override void LayoutSubviews ()
        {
            base.LayoutSubviews ();
            this.Frame = new RectangleF ( this.Frame.X, this.fInitialY - 80, this.Frame.Size.Width, this.fInitialHeight );
            foreach ( UIView oSubView in this.Subviews )
            {
                if ( oSubView is UITextField )
                {
                    oSubView.Frame = new RectangleF ( oSubView.Frame.X, this.Bounds.Height - oSubView.Frame.Height - 65, oSubView.Frame.Width, oSubView.Frame.Height );
                    continue;
                }
                if ( oSubView is UIControl )
                {
                    oSubView.Frame = new RectangleF ( oSubView.Frame.X, this.Bounds.Height - oSubView.Frame.Height - 20, oSubView.Frame.Width, oSubView.Frame.Height );
                }
            }
        }
		
		public override void DismissWithClickedButtonIndex (int index, bool animated)
		{			
			if (index == 1)
			{
				String txt = oTxtInput.Text;
				Action act = ()=>
				{								
					AppDelegateIPhone.AIphone.ReportServ.ReportContent(_image, txt);
					//http://storage.21offserver.com/json/syncreply/ReportImage?ImageId={0} 				
				};
				
				AppDelegateIPhone.ShowRealLoading(AppDelegateIPhone.AIphone.MainWnd, "Reporting image", null, act);				
			}
			
			base.DismissWithClickedButtonIndex(index, animated);
		}
    }
}

