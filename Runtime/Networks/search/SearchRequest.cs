using System.Collections.Generic;
using System.Linq;
using Nox.CCK.Utils;
using Nox.Users;

namespace Nox.Users.Runtime.Networks {
	public class SearchRequest : ISearchRequest, INoxObject {
		internal string query;
		internal Identifier[] ids;
		internal uint offset;
		internal uint limit;

		public string ToParams() {
			var text = "";
			if (!string.IsNullOrEmpty(query))
				text += (text.Length > 0 ? "&" : "") + $"query={query}";
			foreach (var u in ids?.Distinct() ?? Enumerable.Empty<Identifier>())
				text += (text.Length > 0 ? "&" : "") + $"id={u}";
			if (offset > 0)
				text += (text.Length > 0 ? "&" : "") + $"offset={offset}";
			if (limit > 0)
				text += (text.Length > 0 ? "&" : "") + $"limit={limit}";
			return text;
		}

		public static SearchRequest From(Dictionary<string, object> data) {
			var req = new SearchRequest();
			if (data.TryGetValue("query", out var query) && query is string q)
				req.query = q;
			if (data.TryGetValue("ids", out var userIds) && userIds is Identifier[] u)
				req.ids = u?.Distinct().ToArray();
			if (data.TryGetValue("offset", out var offset) && offset is uint o)
				req.offset = o;
			if (data.TryGetValue("limit", out var limit) && limit is uint l)
				req.limit = l;
			return req;
		}

		public string Query {
			get => query;
			set => query = value;
		}

		public Identifier[] Ids {
			get => ids;
			set => ids = value;
		}

		public uint Offset {
			get => offset;
			set => offset = value;
		}

		public uint Limit {
			get => limit;
			set => limit = value;
		}

		public static SearchRequest FromBase(ISearchRequest request) {
			if (request is SearchRequest sr)
				return sr;
			var req = new SearchRequest {
				query  = request.Query,
				ids    = request.Ids?.Distinct().ToArray(),
				offset = request.Offset,
				limit  = request.Limit
			};
			return req;
		}
	}
}