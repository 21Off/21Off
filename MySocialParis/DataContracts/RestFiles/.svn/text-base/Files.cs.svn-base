using System.ComponentModel;
using System.Runtime.Serialization;
using RestFiles.ServiceModel.Types;
using MSP.Client.DataContracts;

namespace RestFiles.ServiceModel.Operations
{
	[Description("GET the File or Directory info at {Path}\n"
               + "POST multipart/formdata to upload a new file to any {Path} in the /ReadWrite folder\n"
               + "PUT {TextContents} to replace the contents of a text file in the /ReadWrite folder\n")]
	[DataContract]
	public class Files
	{
		[DataMember]
		public string Path { get; set; }		

		[DataMember]
		public string TextContents { get; set; }

		[DataMember]
		public bool ForDownload { get; set; }
	}

	[DataContract]
	public class FilesResponse
	{
		[DataMember]
		public FolderResult Directory { get; set; }

		[DataMember]
		public FileResult File { get; set; }

		//Auto inject and serialize web service exceptions
		[DataMember] public ResponseStatus ResponseStatus { get; set; }
	}
}