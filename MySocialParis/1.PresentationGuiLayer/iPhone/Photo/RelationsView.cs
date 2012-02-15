using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;

namespace MSP.Client
{
	public class RelationsView : BaseTimelineViewController
	{		
		public RelationsView(RootElement root, bool pushing): base(pushing)
		{
			//Style = UITableViewStyle.Grouped;
			
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			//this.TableView.BackgroundColor = new UIColor (0, 0, 0, 0);
			this.TableView.AllowsSelection = true;
			
			this.Root = root;
		}
		
		public override void ReloadTimeline ()
		{			
			System.Threading.ThreadPool.QueueUserWorkItem(o => DownloadTweets());
		}				

		void DownloadTweets ()
		{
			this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
		}
				
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource(this);
		}
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var element = Root[0].Elements [indexPath.Row];
		
			var sizable = element as UserElementII;
			if (sizable == null)
				return 60;
			
			return 40;
		}
	}
}
