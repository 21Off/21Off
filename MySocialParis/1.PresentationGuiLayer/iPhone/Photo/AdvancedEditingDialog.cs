using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Threading;
using System.Drawing;

namespace MSP.Client
{
	public class AdvancedEditingDialog : DialogViewController {
		
		// This is our subclass of the fixed-size Source that allows editing
		public class EditingSource : DialogViewController.Source {
			public EditingSource (DialogViewController dvc) : base (dvc) {}
			
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				/*
				var cell = base.GetCell(tableView, indexPath);
				var reuse = cell.ReuseIdentifier;
				
				if (reuse == "LoadMoreElement")
					return true;
				*/	
				
				int rows = tableView.NumberOfRowsInSection(0);
				if (rows - 1 == indexPath.Row)
					return false;
					
				// Trivial implementation: we let all rows be editable, regardless of section or row
				return true;
			}
			
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				// trivial implementation: show a delete button always
				return UITableViewCellEditingStyle.Delete;
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				//
				// In this method, we need to actually carry out the request
				//
				var section = Container.Root [indexPath.Section];
				var element = section [indexPath.Row];
				section.Remove (element);
			}
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{			
			return new EditingSource (this);
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		}
		
		public AdvancedEditingDialog (RootElement root, bool pushing) : base (root, pushing)
		{
		}
	}	
}
