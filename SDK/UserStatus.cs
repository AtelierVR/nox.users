using System;
namespace Nox.Users {
	/// <summary>
	/// Represents the online status of a user.
	/// </summary>
	public enum UserStatus {
		/// <summary>
		/// Your location are visible to everyone,
		/// including non-friends.
		/// When you're received a join request,
		/// you will automatically accept it.
		/// </summary>
		EVENTS,
		/// <summary>
		/// Your location are visible to everyone,
		/// including non-friends.
		/// </summary>
		PUBLIC,
		/// <summary>
		/// Your location are visible to friends,
		/// but not to non-friends.
		/// When you're received a join request,
		/// you will automatically accept it.
		/// </summary>
		ONLINE_JOIN,
		/// <summary>
		/// Standard online status,
		/// Your location are visible to friends.
		/// </summary>
		ONLINE,
		/// <summary>
		/// Your location is hidden.
		/// </summary>
		BUSY,
		/// <summary>
		/// Your location is hidden,
		/// and you will not receive notifications.
		/// </summary>
		DO_NOT_DISTURB,
		/// <summary>
		/// Your location is hidden,
		/// and you will not receive notifications,
		/// and sensitive content will be hidden.
		/// </summary>
		STREAM,
		/// <summary>
		/// Your location is hidden.
		/// Is default when you are offline,
		/// but you can set it manually to hide your status without going offline.
		/// </summary>
		OFFLINE
	}

	/// <summary>
	/// Provides extension methods for the <see cref="UserStatus"/> enum,
	/// allowing for conversion between the enum values and their corresponding string representations.
	/// </summary>
	public static class UserStatusExtensions {
		/// <summary>
		/// Converts a <see cref="UserStatus"/> enum value to its corresponding string representation.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string ToString(this UserStatus status)
			=> status switch {
				UserStatus.EVENTS         => "events",
				UserStatus.PUBLIC         => "public",
				UserStatus.ONLINE_JOIN    => "online_join",
				UserStatus.ONLINE         => "online",
				UserStatus.BUSY           => "busy",
				UserStatus.DO_NOT_DISTURB => "do_not_disturb",
				UserStatus.STREAM         => "stream",
				UserStatus.OFFLINE        => "offline",
				_                         => throw new ArgumentOutOfRangeException(nameof(status), status, null)
			};

		/// <summary>
		/// Converts a string representation of a user status to its corresponding <see cref="UserStatus"/> enum value.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static UserStatus FromString(string status)
			=> status switch {
				"events"         => UserStatus.EVENTS,
				"public"         => UserStatus.PUBLIC,
				"online_join"    => UserStatus.ONLINE_JOIN,
				"online"         => UserStatus.ONLINE,
				"busy"           => UserStatus.BUSY,
				"do_not_disturb" => UserStatus.DO_NOT_DISTURB,
				"stream"         => UserStatus.STREAM,
				"offline"        => UserStatus.OFFLINE,
				_                => throw new ArgumentOutOfRangeException(nameof(status), status, null)
			};

	}
}