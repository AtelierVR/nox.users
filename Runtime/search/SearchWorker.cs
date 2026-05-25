using api.nox.user.network;
using Cysharp.Threading.Tasks;
using Nox.Search;

namespace api.nox.user.search {
	public class SearchWorker : IWorker {
		public string Title;
		public string ServerAddress;

		public string[] TitleArguments
			=> new[] { Title };

		public async UniTask<IResult> Fetch(IFetchOptions options) {
			if (string.IsNullOrEmpty(ServerAddress))
				return new SearchResult { Error = "Invalid server address." };
			var data = await Main.Instance.Network.Search(
				new SearchRequest {
					query  = options.Query,
					offset = options.Page * options.Limit,
					limit  = options.Limit
				}, ServerAddress
			);
			if (data == null) return new SearchResult { Error = "Error fetching users." };
			return new SearchResult {
				Response      = data,
				ServerAddress = ServerAddress,
				Error         = null
			};
		}
	}
}