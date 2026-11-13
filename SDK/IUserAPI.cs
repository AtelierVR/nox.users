using Cysharp.Threading.Tasks;
using Nox.CCK.Utils;

namespace Nox.Users {
	public interface IUserAPI {
		public ICurrentUser Current { get; }

		public UniTask<ICurrentUser> FetchCurrent();

		public UniTask<IUser> Fetch(Identifier identifier);

		public UniTask<ISearchResponse> Search(ISearchRequest request, string from = null);

		public UniTask<ISearchResponse> FetchFriends(uint offset = 0, uint limit = 50);

		public void WatchPresence();

		public void UnwatchPresence();

		public UniTask<IAuthToken> GetToken(string address);

		public UniTask<ICurrentUser> UpdateCurrent(IUpdateCurrentUserRequest request);

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

		/// <summary>
		/// Follows a user, or sends a follow request when the target requires approval.
		/// </summary>
		/// <param name="identifier">Identifier of the user to follow.</param>
		/// <returns><c>true</c> when the action succeeded.</returns>
		public UniTask<bool> Follow(Identifier identifier);

		/// <summary>
		/// Removes an existing follow or cancels a pending follow request.
		/// </summary>
		/// <param name="identifier">Identifier of the user to unfollow.</param>
		/// <returns><c>true</c> when the action succeeded.</returns>
		public UniTask<bool> Unfollow(Identifier identifier);

		/// <summary>
		/// Accepts or rejects the pending follow request of a user.
		/// </summary>
		/// <param name="identifier">Identifier of the user who requested to follow the current user.</param>
		/// <param name="accept"><c>true</c> to accept the request, <c>false</c> to reject it.</param>
		/// <returns><c>true</c> when the action succeeded.</returns>
		public UniTask<bool> RespondToRequest(Identifier identifier, bool accept);
	}
}
