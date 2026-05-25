using Cysharp.Threading.Tasks;
using Nox.CCK.Utils;

namespace Nox.Users {
	public interface IUserAPI {
		public ICurrentUser Current { get; }

		public UniTask<ICurrentUser> FetchCurrent();

		public UniTask<IUser> Fetch(Identifier identifier);

		public ISearchRequest MakeSearchRequest();

		public UniTask<ISearchResponse> Search(ISearchRequest request, string from = null);

		public UniTask<ISearchResponse> FetchFriends(uint offset = 0, uint limit = 50);

		public UniTask<IAuthToken> GetToken(string address);

		public UniTask<ICurrentUser> UpdateCurrent(IUpdateCurrentUserRequest request);

		public IUpdateCurrentUserRequest MakeUpdateCurrentRequest();
		
		/// <summary>
		/// Adds a user to the favorites list.
		/// </summary>
		/// <param name="identifier">Identifier of the user to add to favorites.</param>
		/// <returns></returns>
		public UniTask<IFavorites> AddFavorite(Identifier identifier);

		/// <summary>
		/// Removes a user from the favorites list.
		/// </summary>
		/// <param name="identifier">Identifier of the user to remove from favorites.</param>
		/// <returns></returns>
		public UniTask<IFavorites> RemoveFavorite(Identifier identifier);

		/// <summary>
		/// Gets the list of favorite user identifiers.
		/// </summary>
		/// <returns></returns>
		public UniTask<IFavorites> GetFavorites();
	}
}