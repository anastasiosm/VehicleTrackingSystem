using VehicleTracking.Domain.ValueObjects;
namespace VehicleTracking.Domain.ValueObjects
{
	/// <summary>
	/// Represents immutable geographical coordinates (latitude and longitude).
	/// Implemented as a readonly struct for value semantics and performance.
	/// </summary>
	public readonly struct Coordinates
	{
		public double Latitude { get; }
		public double Longitude { get; }

		/// <summary>
		/// Creates a new Coordinates instance with the specified latitude and longitude.
		/// </summary>
		/// <param name="latitude">The latitude coordinate (-90 to 90 degrees)</param>
		/// <param name="longitude">The longitude coordinate (-180 to 180 degrees)</param>
		public Coordinates(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current Coordinates.
		/// Uses value equality - two Coordinates are equal if they have the same Latitude and Longitude.
		/// </summary>
		/// <param name="obj">The object to compare with the current Coordinates</param>
		/// <returns>true if the objects are equal; otherwise, false</returns>
		public override bool Equals(object obj)
		{
			// Pattern matching: check if obj is a Coordinates and cast it in one step
			return obj is Coordinates other && Equals(other);
		}

		/// <summary>
		/// Determines whether the specified Coordinates is equal to the current Coordinates.
		/// Strongly-typed version that avoids boxing and is faster than Equals(object).
		/// </summary>
		/// <param name="other">The Coordinates to compare with the current instance</param>
		/// <returns>true if the Coordinates values are equal; otherwise, false</returns>
		public bool Equals(Coordinates other)
		{
			// Use .Equals() for doubles to properly handle special values like NaN
			return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
		}

		/// <summary>
		/// Returns a hash code for this Coordinates instance.
		/// Used when storing Coordinates in hash-based collections (Dictionary, HashSet, etc).
		/// Two Coordinates with the same values will return the same hash code.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code</returns>
		public override int GetHashCode()
		{
			unchecked // Allow integer overflow without throwing exceptions
			{
				// Use prime numbers (17 and 23) to distribute hash values evenly
				// This reduces hash collisions in hash-based collections
				var hash = 17; // Start with a prime number
				hash = hash * 23 + Latitude.GetHashCode();  // Combine Latitude's hash
				hash = hash * 23 + Longitude.GetHashCode(); // Combine Longitude's hash
				return hash;
			}
		}

		/// <summary>
		/// Determines whether two Coordinates instances are equal.
		/// Enables the use of == operator for value comparison.
		/// </summary>
		/// <param name="left">The first Coordinates to compare</param>
		/// <param name="right">The second Coordinates to compare</param>
		/// <returns>true if the values are equal; otherwise, false</returns>
		public static bool operator ==(Coordinates left, Coordinates right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether two Coordinates instances are not equal.
		/// Enables the use of != operator for value comparison.
		/// </summary>
		/// <param name="left">The first Coordinates to compare</param>
		/// <param name="right">The second Coordinates to compare</param>
		/// <returns>true if the values are not equal; otherwise, false</returns>
		public static bool operator !=(Coordinates left, Coordinates right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a string representation of the Coordinates in the format (Latitude, Longitude).
		/// Values are formatted with 6 decimal places for precision.
		/// </summary>
		/// <returns>A formatted string representation of the Coordinates</returns>
		public override string ToString()
		{
			// F6 formats doubles with 6 decimal places (e.g., 37.983800)
			return $"({Latitude:F6}, {Longitude:F6})";
		}
	}
}

