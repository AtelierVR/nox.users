using System;
using System.Linq;
using Nox.CCK.Utils;
using Nox.Users;

namespace Nox.CCK.Users {
	/// <summary>
	/// Search request for users, passed to <see cref="IUserAPI"/>'s search method
	/// (<c>GET /api/users?query=&amp;id=&amp;offset=&amp;limit=</c>).
	///
	/// An empty <see cref="Query"/> without <see cref="Ids"/> searches everything
	/// the server is allowed to return.
	/// </summary>
	public class SearchRequest : ISearchRequest, INoxObject {
		public string Query { get; set; } = null;

		public Identifier[] Ids { get; set; } = Array.Empty<Identifier>();

		public uint Offset { get; set; } = 0;

		public uint Limit { get; set; } = 0;

		public override string ToString() {
			var text = "";
			if (!string.IsNullOrEmpty(Query))
				text += (text.Length > 0 ? "&" : "") + $"query={Query}";
			if (Ids != null)
				text = Ids
					.Distinct()
					.Aggregate(text, (current, u) => current + (current.Length > 0 ? "&" : "") + $"id={u}");
			if (Offset > 0)
				text += (text.Length > 0 ? "&" : "") + $"offset={Offset}";
			if (Limit > 0)
				text += (text.Length > 0 ? "&" : "") + $"limit={Limit}";
			return string.IsNullOrEmpty(text) ? "" : "?" + text;
		}

		public static SearchRequest From(ISearchRequest request)
			=> new() {
				Query  = request.Query,
				Ids    = request.Ids,
				Offset = request.Offset,
				Limit  = request.Limit
			};
	}
}
