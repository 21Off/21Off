using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using System.Threading;

namespace MSP.Client
{
	public class SearchKeyword : SearchDialog {
		
		public SearchKeyword()
		{
			TableView.BackgroundView = new UIImageView(UIImage.FromBundle("Images/Ver4/fond"));
			Style = UITableViewStyle.Plain;
		}
		
		public override SearchMirrorElement MakeMirror ()
		{
			return new SearchMirrorElement (Locale.GetText ("Search for keyword `{0}'"));
		}
		
		public void Reset()
		{
			if (Root.Count > 0)
				Root[0].Clear();
			
			FinishSearch();	
			
			//SearchMirror = MakeMirror ();
			var entries = new Section () {
				//SearchMirror
			};
						
			PopulateSearch (entries);
			
			Root = new RootElement (Locale.GetText ("Search")){
				entries,
			};
			
			ReloadData();
			//StartSearch ();
			//PerformFilter ("");
		}

		private List<Image> SearchImages ()
		{
			var images = new List<Image>();
			var keywResp = AppDelegateIPhone.AIphone.KeywServ.GetSimilarImages(SearchMirror.Text, LastTime.Ticks);
			var similarImages = keywResp.Images;
			
			foreach (SimilarImage similarImage in similarImages)
			{
				images.Add(similarImage.Image);				
			}
			
			LastTime = keywResp.Time;
			
			return images;
		}
		
		public override void SearchButtonClicked (string text)
		{	
			if (string.IsNullOrWhiteSpace(text))
				return;
			
			if (Root.Count > 0)
				Root[0].Clear();
			
			FinishSearch();	
			
			var lw = new LoadingView();
			lw.Show("Searching for " + text);
			
			NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (0.1), delegate {
				
				SearchMirror.Text = text;
				SearchedText = text;
				LastTime = DateTime.MaxValue;
				
				var entries = new Section () {
				};
				
				var images = SearchImages ();
				_foundImages = images;
				
				int rowCount = 4;
				for (int i = 0; i < images.Count; i += rowCount)
				{
					var imagesInfos = new List<ImageInfo>();
					for (int j = 0; j < rowCount; j++)
					{
						var imgInfo = new ImageInfo()
						{
							Img = (i + j < images.Count) ? images[i + j] : null,							
						};
						imagesInfos.Add(imgInfo);
					}
					
					entries.Add(new Images2Element(imagesInfos, i));
				}
				
				var more = new LoadMoreElement (lme => {
					// Launch a thread to do some work
					ThreadPool.QueueUserWorkItem (delegate {						
						SearchMoreKeywords(entries, lme);
					});
				});
				
				more.Height = 60;
				more.Image = Graphics.GetImgResource("more");				
				
				entries.Add(more);
				
				Root = new RootElement (Locale.GetText ("Search")){
					entries,
				};
				
				ReloadData();
				lw.Hide();
			});
		}
		
		private void SearchMoreKeywords(Section entries, LoadMoreElement lme)
		{
			try
			{
				var images = SearchImages ();
				_foundImages.AddRange(images);
				
				int rowCount = 4;
				var newElements = new List<Images2Element>();
				for (int i = 0; i < images.Count; i += rowCount)
				{
					var imagesInfos = new List<ImageInfo>();
					for (int j = 0; j < rowCount; j++)
					{
						var imgInfo = new ImageInfo()
						{
							Img = (i + j < images.Count) ? images[i + j] : null,							
						};
						imagesInfos.Add(imgInfo);
					}
					
					newElements.Add(new Images2Element(imagesInfos, i));
				}
				
				InvokeOnMainThread(()=>
				{
					lme.Animating = false;
					entries.Insert(entries.Count - 1, UITableViewRowAnimation.None, newElements.ToArray());					
				});								
			}
			catch (Exception ex)
			{
				Util.LogException("SearchMoreKeywords", ex);
			}
		}
		
		private DateTime LastTime;
		private List<Image> _foundImages = new List<Image>();
		public List<Image> FoundImages { get { return _foundImages; } }
		public string SearchedText {get;set;}
		
		public override void Selected (NSIndexPath indexPath)
		{

		}

		public override void PopulateSearch (Section entries)
		{
		
		}
	}
}
