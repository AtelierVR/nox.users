using System;
using UnityEngine;

namespace Nox.Users {
	/// <summary>
	/// Represents the online status of a user.
	/// </summary>
	public enum UserStatus {
		/// <summary>
		/// No presence / unknown status.
		/// </summary>
		NONE,
		/// <summary>
		/// Your location are visible to everyone,
		/// including non-friends.
		/// When you're received a join request,
		/// you will automatically accept it.
		/// </summary>
		EVENT,
		/// <summary>
		/// Your location are visible to everyone,
		/// including non-friends.
		/// </summary>
		PUBLIC,
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
		DND,
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
				UserStatus.EVENT   => "event",
				UserStatus.PUBLIC  => "public",
				UserStatus.ONLINE  => "online",
				UserStatus.BUSY    => "busy",
				UserStatus.DND     => "dnd",
				UserStatus.STREAM  => "stream",
				UserStatus.OFFLINE => "offline",
				_    			   => null,
			};

		/// <summary>
		/// Converts a string representation of a user status to its corresponding <see cref="UserStatus"/> enum value.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static UserStatus FromString(string status)
			=> status switch {
				"event"   => UserStatus.EVENT,
				"public"  => UserStatus.PUBLIC,
				"online"  => UserStatus.ONLINE,
				"busy"    => UserStatus.BUSY,
				"dnd"     => UserStatus.DND,
				"stream"  => UserStatus.STREAM,
				"offline" => UserStatus.OFFLINE,
				_         => UserStatus.NONE
			};

		/// <summary>
		/// Converts a <see cref="UserStatus"/> to the colour used to display it (presence dot).
		/// <para>
		/// Tailwind 500 palette: `event` pink, `public` cyan, `online` green, `busy` orange,
		/// `dnd` red, `stream` violet, `offline` grey. Mirrors the web client's <c>COLOR</c> map.
		/// </para>
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		public static Color ToColor(this UserStatus status)
			=> status switch {
				UserStatus.EVENT   => new Color32(0xEC, 0x48, 0x99, 0xFF), // event   pink-500
				UserStatus.PUBLIC  => new Color32(0x06, 0xB6, 0xD4, 0xFF), // public  cyan-500
				UserStatus.ONLINE  => new Color32(0x22, 0xC5, 0x5E, 0xFF), // online  green-500
				UserStatus.BUSY    => new Color32(0xF9, 0x73, 0x16, 0xFF), // busy    orange-500
				UserStatus.DND     => new Color32(0xEF, 0x44, 0x44, 0xFF), // dnd     red-500
				UserStatus.STREAM  => new Color32(0x8B, 0x5C, 0xF6, 0xFF), // stream  violet-500
				UserStatus.OFFLINE => new Color32(0x6B, 0x72, 0x80, 0xFF), // offline gray-500
				_                  => new Color32(0x6B, 0x72, 0x80, 0xFF), // offline gray-500
			};

	}
}