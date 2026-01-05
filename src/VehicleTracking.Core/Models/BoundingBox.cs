namespace VehicleTracking.Core.Models
{
	/// <summary>
	/// Represents an immutable geographical bounding box defined by minimum and maximum latitude/longitude.
	/// Used to define geographical boundaries for validation (e.g., Athens city limits).
	/// Implemented as a readonly struct for value semantics and performance.
	/// </summary>
	public readonly struct BoundingBox
	{
		public double MinLat { get; }
		public double MaxLat { get; }
		public double MinLon { get; }
		public double MaxLon { get; }

		/// <summary>
		/// Creates a new BoundingBox with the specified boundaries.
		/// </summary>
		/// <param name="minLat">Minimum latitude (southern boundary)</param>
		/// <param name="maxLat">Maximum latitude (northern boundary)</param>
		/// <param name="minLon">Minimum longitude (western boundary)</param>
		/// <param name="maxLon">Maximum longitude (eastern boundary)</param>
		public BoundingBox(double minLat, double maxLat, double minLon, double maxLon)
		{
			MinLat = minLat;
			MaxLat = maxLat;
			MinLon = minLon;
			MaxLon = maxLon;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current BoundingBox.
		/// Uses value equality - two BoundingBoxes are equal if all their boundaries match.
		/// </summary>
		/// <param name="obj">The object to compare with the current BoundingBox</param>
		/// <returns>true if the objects are equal; otherwise, false</returns>
		public override bool Equals(object obj)
		{
			// Pattern matching: check type and cast in one step
			return obj is BoundingBox other && Equals(other);
		}

		/// <summary>
		/// Determines whether the specified BoundingBox is equal to the current BoundingBox.
		/// Strongly-typed version that avoids boxing and is faster than Equals(object).
		/// </summary>
		/// <param name="other">The BoundingBox to compare with the current instance</param>
		/// <returns>true if all boundary values are equal; otherwise, false</returns>
		public bool Equals(BoundingBox other)
		{
			// Compare all four boundary values using .Equals() for proper double comparison
			return MinLat.Equals(other.MinLat) &&
				   MaxLat.Equals(other.MaxLat) &&
				   MinLon.Equals(other.MinLon) &&
				   MaxLon.Equals(other.MaxLon);
		}

		/// <summary>
		/// Returns a hash code for this BoundingBox instance.
		/// Used when storing BoundingBox in hash-based collections (Dictionary, HashSet, etc).
		/// Two BoundingBoxes with the same boundary values will return the same hash code.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code</returns>
		public override int GetHashCode()
		{
			unchecked // Allow arithmetic overflow without exceptions (for performance)
			{
				// Combine hash codes of all four properties using prime numbers (17, 23)
				// This algorithm provides good distribution and reduces collisions
				var hash = 17; // Start with a non-zero prime number
				hash = hash * 23 + MinLat.GetHashCode(); // Mix in MinLat
				hash = hash * 23 + MaxLat.GetHashCode(); // Mix in MaxLat
				hash = hash * 23 + MinLon.GetHashCode(); // Mix in MinLon
				hash = hash * 23 + MaxLon.GetHashCode(); // Mix in MaxLon
				return hash;
			}
		}

		/// <summary>
		/// Determines whether two BoundingBox instances are equal.
		/// Enables the use of == operator for value comparison.
		/// </summary>
		/// <param name="left">The first BoundingBox to compare</param>
		/// <param name="right">The second BoundingBox to compare</param>
		/// <returns>true if the values are equal; otherwise, false</returns>
		public static bool operator ==(BoundingBox left, BoundingBox right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether two BoundingBox instances are not equal.
		/// Enables the use of != operator for value comparison.
		/// </summary>
		/// <param name="left">The first BoundingBox to compare</param>
		/// <param name="right">The second BoundingBox to compare</param>
		/// <returns>true if the values are not equal; otherwise, false</returns>
		public static bool operator !=(BoundingBox left, BoundingBox right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a string representation of the BoundingBox showing its boundaries.
		/// Format: "BoundingBox [Lat: min-max, Lon: min-max]"
		/// Values are formatted with 2 decimal places.
		/// </summary>
		/// <returns>A formatted string representation of the BoundingBox</returns>
		public override string ToString()
		{
			// F2 formats doubles with 2 decimal places (e.g., 37.90)
			return $"BoundingBox [Lat: {MinLat:F2}-{MaxLat:F2}, Lon: {MinLon:F2}-{MaxLon:F2}]";
		}
	}
}
