using System;
using Cysharp.Threading.Tasks;
using Nox.CCK.Utils;

namespace Nox.Users {
	/// <summary>
	/// Represents a user in the system,
	/// providing access to their profile information,
	/// </summary>
	public interface IUser {
		/// <summary>
		/// Gets the unique identifier of the user.
		/// </summary>
		public uint Id { get; }

		/// <summary>
		/// Gets the username of the user.
		/// </summary>
		public string Username { get; }

		/// <summary>
		/// Gets the display name of the user.
		/// </summary>
		public string Display { get; }

		/// <summary>
		/// Gets the biography of the user.
		/// Can be null.
		/// </summary>
		public string Bio { get; }

		/// <summary>
		/// Gets the pronoun of the user.
		/// Can be null.
		/// </summary>
		public string Pronoun { get; }

		/// <summary>
		/// Gets the server of the user.
		/// </summary>
		public string Server { get; }

		/// <summary>
		/// Gets the tags associated with the user.
		/// A tags are `&lt;attributor&gt;:&lt;value&gt;`.
		/// If is a self-assigned by the user, a tag start by `usr:`.
		/// </summary>
		public string[] Tags { get; }

		/// <summary>
		/// Gets the URL of the user's thumbnail image.
		/// </summary>
		public string Thumbnail { get; }

		/// <summary>
		/// Gets the URL of the user's banner image.
		/// </summary>
		/// <returns></returns>
		public string Banner { get; }

		/// <summary>
		/// Gets the links associated with the user.
		/// </summary>
		public ILinkEntry[] Links { get; }

		/// <summary>
		/// Gets the relationship status of the user with respect to the current authenticated user.
		/// If the user is the current authenticated user, the relationship is null.
		/// Same if the user is not authenticated, the relationship is null.
		/// </summary>
		public IUserRelation Relations { get; }

		/// <summary>
		/// Gets the public key of the user.
		/// </summary>
		public byte[] Public { get; }

		/// <summary>
		/// Gets the number of followers the user has.
		/// If the number is -1, it means the followers count is hidden by the user.
		/// </summary>
		public int Followers { get; }

		/// <summary>
		/// Gets the number of users the user is following.
		/// If the number is -1, it means the following count is hidden by the
		/// </summary>
		public int Following { get; }

		/// <summary>
		/// Gets the presence information of the user.
		/// </summary>
		public IUserPresence Presence { get; }

		/// <summary>
		/// Gets the aliases of the user.
		/// </summary>
		public IUserAlias[] Aliases { get; }

		/// <summary>
		/// Gets the date and time when the user account was created.
		/// </summary>
		public DateTime CreatedAt { get; }

		/// <summary>
		/// Gets the identifier of the user.
		/// </summary>
		public Identifier Identifier { get; }

		/// <summary>
		/// Refreshes the user data by fetching the latest information from the server.
		/// </summary>
		/// <returns></returns>
		public UniTask<IUser> Refresh();
	}
}