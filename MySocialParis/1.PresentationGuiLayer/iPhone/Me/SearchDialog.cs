using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public abstract class SearchDialog : DialogViewController {
		protected SearchMirrorElement SearchMirror;
		
		public SearchDialog () : base (new RootElement("Search"))
		{
			EnableSearch = true;
			Style = UITableViewStyle.Plain;			
		}

		public override void SearchButtonClicked (string text)
		{
			Save ();
			ActivateController (new SearchView (text) {  });
		}
		
		public override void OnSearchTextChanged (string text)
		{
			base.OnSearchTextChanged (text);
			SearchMirror.Text = text;
			TableView.SetNeedsDisplay ();
		}

		public abstract SearchMirrorElement MakeMirror ();
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad();
			
			InitSearch ();	
		}	
		
		void InitSearch ()
		{
			SearchMirror = MakeMirror ();
			var entries = new Section () {
				//SearchMirror
			};
			
			PopulateSearch (entries);
			
			Root = new RootElement (Locale.GetText ("Search")){
				entries,
			};
						
			StartSearch ();
			PerformFilter ("");
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			InitSearch ();
		}
		
		public string GetItemText (NSIndexPath indexPath)
		{
			var element = Root [0][indexPath.Row];
			
			if (element is SearchMirrorElement)
				return ((SearchMirrorElement) element).Text;
			else if (element is StringElement){
				return ((StringElement) element).Caption;
			} else
				throw new Exception ("Unknown item in SearchDialog");
		}
		
		public abstract void PopulateSearch (Section entries);
		
		public virtual void Save () {}
	}
}
