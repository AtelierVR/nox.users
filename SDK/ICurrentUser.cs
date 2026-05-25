using System;
using Nox.CCK.Utils;

namespace Nox.Users {
	/// <summary>
	/// Represents the current authenticated user in the system,
	/// providing access to their email,
	/// home identifier,
	/// and avatar information.
	///
	/// The <see cref="ICurrentUser.Relation"/> is always <see cref="null"/>.
	/// </summary>
	public interface ICurrentUser : IUser {
		public string Email { get; }
		public bool IsEmailVerified { get; }

		public Identifier Home { get; }

		public Identifier Avatar { get; }
		
		public bool Is2FAEnabled { get; }
	}
}