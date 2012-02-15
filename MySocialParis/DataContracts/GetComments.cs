using System.Collections.Generic;

namespace MSP.Client.DataContracts
{
    public class Comments
    {
        public int? ImageId { get; set; }
        public Comment Comment { get; set; }
	}
	
	public class CommentResponse
	{
		public Comment Comment {get;set;}
		public User User {get;set;}
	}	
	
    public class CommentsResponse
    {
        public CommentsResponse()
        {
            this.ResponseStatus = new ResponseStatus();
            this.Comments = new List<Comment>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<Comment> Comments { get; set; }
	}
}

