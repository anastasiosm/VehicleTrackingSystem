using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VehicleTracking.Application.Services;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Domain.ValueObjects;

namespace VehicleTracking.Tests.Unit.Application
{
    [TestFixture]
    public class RouteCalculationServiceTests
    {
        private Mock<IGeographicalService> _mockGeoService;
        private RouteCalculationService _service;

        [SetUp]
        public void SetUp()
        {
            _mockGeoService = new Mock<IGeographicalService>();
            _service = new RouteCalculationService(_mockGeoService.Object);
        }

        [Test]
        public void CalculateTotalDistance_NullPositions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _service.CalculateTotalDistance(null));
        }

        [Test]
        public void CalculateTotalDistance_EmptyList_ReturnsZero()
        {
            // Arrange
            var positions = new List<GpsPosition>();

            // Act
            var distance = _service.CalculateTotalDistance(positions);

            // Assert
            Assert.AreEqual(0, distance);
        }

        [Test]
        public void CalculateTotalDistance_SinglePosition_ReturnsZero()
        {
            // Arrange
            var positions = new List<GpsPosition>
            {
                new GpsPosition { Latitude = 37.9838, Longitude = 23.7275, RecordedAt = DateTime.UtcNow }
            };

            // Act
            var distance = _service.CalculateTotalDistance(positions);

            // Assert
            Assert.AreEqual(0, distance);
        }

        [Test]
        public void CalculateTotalDistance_TwoPositions_CallsGeoServiceOnce()
        {
            // Arrange
            var positions = new List<GpsPosition>
            {
                new GpsPosition { Latitude = 37.9838, Longitude = 23.7275, RecordedAt = DateTime.UtcNow },
                new GpsPosition { Latitude = 37.9840, Longitude = 23.7277, RecordedAt = DateTime.UtcNow.AddMinutes(1) }
            };

            _mockGeoService
                .Setup(x => x.CalculateDistance(It.IsAny<Coordinates>(), It.IsAny<Coordinates>()))
                .Returns(50.5);

            // Act
            var distance = _service.CalculateTotalDistance(positions);

            // Assert
            Assert.AreEqual(50.5, distance);
            _mockGeoService.Verify(x => x.CalculateDistance(It.IsAny<Coordinates>(), It.IsAny<Coordinates>()), Times.Once);
        }

        [Test]
        public void CalculateTotalDistance_ThreePositions_SumsTwoSegments()
        {
            // Arrange
            var positions = new List<GpsPosition>
            {
                new GpsPosition { Latitude = 37.9838, Longitude = 23.7275, RecordedAt = DateTime.UtcNow },
                new GpsPosition { Latitude = 37.9840, Longitude = 23.7277, RecordedAt = DateTime.UtcNow.AddMinutes(1) },
                new GpsPosition { Latitude = 37.9842, Longitude = 23.7279, RecordedAt = DateTime.UtcNow.AddMinutes(2) }
            };

            _mockGeoService
                .SetupSequence(x => x.CalculateDistance(It.IsAny<Coordinates>(), It.IsAny<Coordinates>()))
                .Returns(50.5)  // First segment
                .Returns(45.3); // Second segment

            // Act
            var distance = _service.CalculateTotalDistance(positions);

            // Assert
            Assert.AreEqual(95.8, distance, 0.001);
            _mockGeoService.Verify(x => x.CalculateDistance(It.IsAny<Coordinates>(), It.IsAny<Coordinates>()), Times.Exactly(2));
        }

        [Test]
        public void CalculateRouteStatistics_EmptyList_ReturnsZeroStatistics()
        {
            // Arrange
            var positions = new List<GpsPosition>();

            // Act
            var stats = _service.CalculateRouteStatistics(positions);

            // Assert
            Assert.AreEqual(0, stats.TotalDistanceMeters);
            Assert.AreEqual(0, stats.PositionCount);
            Assert.AreEqual(0, stats.DurationSeconds);
            Assert.AreEqual(0, stats.AverageSpeedMetersPerSecond);
        }

        [Test]
        public void CalculateRouteStatistics_ValidRoute_ReturnsCorrectStatistics()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var positions = new List<GpsPosition>
            {
                new GpsPosition { Latitude = 37.9838, Longitude = 23.7275, RecordedAt = startTime },
                new GpsPosition { Latitude = 37.9840, Longitude = 23.7277, RecordedAt = startTime.AddSeconds(10) },
                new GpsPosition { Latitude = 37.9842, Longitude = 23.7279, RecordedAt = startTime.AddSeconds(20) }
            };

            _mockGeoService
                .Setup(x => x.CalculateDistance(It.IsAny<Coordinates>(), It.IsAny<Coordinates>()))
                .Returns(50.0);

            // Act
            var stats = _service.CalculateRouteStatistics(positions);

            // Assert
            Assert.AreEqual(100.0, stats.TotalDistanceMeters); // 2 segments * 50m
            Assert.AreEqual(3, stats.PositionCount);
            Assert.AreEqual(20.0, stats.DurationSeconds);
            Assert.AreEqual(5.0, stats.AverageSpeedMetersPerSecond); // 100m / 20s
        }

        [Test]
        public void CalculateRouteStatistics_ZeroDuration_ReturnsZeroSpeed()
        {
            // Arrange - All positions at same time
            var time = DateTime.UtcNow;
            var positions = new List<GpsPosition>
            {
                new GpsPosition { Latitude = 37.9838, Longitude = 23.7275, RecordedAt = time },
                new GpsPosition { Latitude = 37.9840, Longitude = 23.7277, RecordedAt = time }
            };

            _mockGeoService
                .Setup(x => x.CalculateDistance(It.IsAny<Coordinates>(), It.IsAny<Coordinates>()))
                .Returns(50.0);

            // Act
            var stats = _service.CalculateRouteStatistics(positions);

            // Assert
            Assert.AreEqual(0, stats.AverageSpeedMetersPerSecond); // Avoid division by zero
        }
    }
}
