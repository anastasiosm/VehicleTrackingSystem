using System;
using NUnit.Framework;
using VehicleTracking.Domain.ValueObjects;
using VehicleTracking.Domain.Exceptions;

namespace VehicleTracking.Tests.Unit.Domain
{
    [TestFixture]
    public class CoordinatesTests
    {
        [Test]
        public void Constructor_ValidCoordinates_CreatesInstance()
        {
            // Arrange
            double latitude = 37.9838;
            double longitude = 23.7275;

            // Act
            var coordinates = new Coordinates(latitude, longitude);

            // Assert
            Assert.AreEqual(latitude, coordinates.Latitude);
            Assert.AreEqual(longitude, coordinates.Longitude);
        }

        [TestCase(-91, 23.7275, Description = "Latitude too low")]
        [TestCase(91, 23.7275, Description = "Latitude too high")]
        [TestCase(37.9838, -181, Description = "Longitude too low")]
        [TestCase(37.9838, 181, Description = "Longitude too high")]
        public void Constructor_InvalidCoordinates_ThrowsInvalidCoordinateException(double latitude, double longitude)
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidCoordinateException>(() => new Coordinates(latitude, longitude));
            
            Assert.AreEqual(latitude, ex.Latitude);
            Assert.AreEqual(longitude, ex.Longitude);
            Assert.That(ex.Message, Does.Contain("Invalid coordinates"));
        }

        [Test]
        public void Constructor_BoundaryValues_CreatesInstance()
        {
            // Act & Assert - Should NOT throw
            Assert.DoesNotThrow(() => new Coordinates(-90, -180));
            Assert.DoesNotThrow(() => new Coordinates(90, 180));
            Assert.DoesNotThrow(() => new Coordinates(0, 0));
        }

        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            // Arrange
            var coord1 = new Coordinates(37.9838, 23.7275);
            var coord2 = new Coordinates(37.9838, 23.7275);

            // Act & Assert
            Assert.IsTrue(coord1.Equals(coord2));
            Assert.IsTrue(coord1 == coord2);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            // Arrange
            var coord1 = new Coordinates(37.9838, 23.7275);
            var coord2 = new Coordinates(37.9840, 23.7277);

            // Act & Assert
            Assert.IsFalse(coord1.Equals(coord2));
            Assert.IsTrue(coord1 != coord2);
        }

        [Test]
        public void GetHashCode_SameCoordinates_ReturnsSameHash()
        {
            // Arrange
            var coord1 = new Coordinates(37.9838, 23.7275);
            var coord2 = new Coordinates(37.9838, 23.7275);

            // Act
            var hash1 = coord1.GetHashCode();
            var hash2 = coord2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void ToString_FormatsCorrectly()
        {
            // Arrange
            var coordinates = new Coordinates(37.9838, 23.7275);

            // Act
            var result = coordinates.ToString();

            // Assert
            Assert.AreEqual("(37.983800, 23.727500)", result);
        }
    }
}
