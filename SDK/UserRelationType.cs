using System;

namespace Nox.Users {
	/// <summary>
	/// Represents the type of relationship between users,
	/// such as following or friend requests.
	/// </summary>
	public enum UserRelationType {
		/// <summary>
		/// Represents a following relationship,
		/// where one user follows another user.
		/// </summary>
		FOLLOW,
		/// <summary>
		/// Represents a friend request relationship,
		/// where one user has sent a friend request to another user.
		/// </summary>
		REQUEST
	}

	/// <summary>
	/// Provides extension methods for the UserRelationType enum,
	/// allowing for conversion to and from string representations.
	/// </summary>
	public static class UserRelationTypeExtensions {
		/// <summary>
		/// Converts a UserRelationType enum value
		/// to its corresponding string representation.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string ToString(this UserRelationType type)
			=> type switch {
				UserRelationType.FOLLOW  => "follow",
				UserRelationType.REQUEST => "request",
				_                        => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};

		/// <summary>
		/// Converts a string representation of a user relation type
		/// to its corresponding UserRelationType enum value.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static UserRelationType FromString(string str)
			=> str switch {
				"follow"  => UserRelationType.FOLLOW,
				"request" => UserRelationType.REQUEST,
				_         => throw new ArgumentOutOfRangeException(nameof(str), str, null)
			};
	}
}