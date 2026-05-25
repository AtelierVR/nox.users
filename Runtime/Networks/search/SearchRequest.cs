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

		public ISearchRequest SetQuery(string query) {
			this.query = query;
			return this;
		}

		public ISearchRequest SetIds(Identifier[] userIds) {
			ids = userIds;
			return this;
		}

		public ISearchRequest SetOffset(uint offset) {
			this.offset = offset;
			return this;
		}

		public ISearchRequest SetLimit(uint limit) {
			this.limit = limit;
			return this;
		}

		public string GetQuery()
			=> query;

		public Identifier[] GetIds()
			=> ids;

		public uint GetOffset()
			=> offset;

		public uint GetLimit()
			=> limit;

		public static SearchRequest FromBase(ISearchRequest request) {
			if (request is SearchRequest sr)
				return sr;
			var req = new SearchRequest {
				query  = request.GetQuery(),
				ids    = request.GetIds()?.Distinct().ToArray(),
				offset = request.GetOffset(),
				limit  = request.GetLimit()
			};
			return req;
		}
	}
}