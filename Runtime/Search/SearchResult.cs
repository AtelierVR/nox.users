using System;
using System.Linq;
using Nox.Search;
using Nox.Users.Runtime.Networks;

namespace Nox.Users.Runtime.Search {

	public class SearchResult : IResult {
		public string Error { get; internal set; }
		public SearchResponse Response;
		public string ServerAddress;

		public bool IsError
			=> !string.IsNullOrEmpty(Error);

		public bool HasNext()
			=> !IsError && Response.HasNext();

		public IResultData[] Data
			=> Response != null
				? Response.Items
					.Select(x => new SearchData { Reference = x })
					.Cast<IResultData>()
					.ToArray()
				: Array.Empty<IResultData>();
	}

}