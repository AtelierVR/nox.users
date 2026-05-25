using Nox.Search;

namespace api.nox.user.search {
	public class Search {
		private readonly IHandler _handler;

		internal Search() {
			_handler = Main.SearchAPI.Add(new SearchHandler());
		}

		internal void Dispose() {
			if (_handler == null) return;
			Main.SearchAPI.Remove(_handler.GetId());
		}
	}
}