using Moq;
using NUnit.Framework;
using VehicleTracking.Infrastructure.Validation;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Domain.Entities;
using System;

namespace VehicleTracking.Tests.Unit.Infrastructure
{
    /// <summary>
    /// Tests for VehicleActiveRule validation
    /// </summary>
    [TestFixture]
    public class VehicleActiveRuleTests
    {
        private Mock<IVehicleRepository> _mockVehicleRepo;
        private VehicleActiveRule _rule;

        [SetUp]
        public void SetUp()
        {
            _mockVehicleRepo = new Mock<IVehicleRepository>();
            _rule = new VehicleActiveRule(_mockVehicleRepo.Object);
        }

        [Test]
        public void Validate_VehicleIsActive_ReturnsSuccess()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Name = "VAN-001", IsActive = true };
            var position = new GpsPosition 
            { 
                VehicleId = 1, 
                Latitude = 37.9838, 
                Longitude = 23.7275, 
                RecordedAt = DateTime.UtcNow 
            };

            _mockVehicleRepo.Setup(x => x.GetById(1)).Returns(vehicle);

            // Act
            var result = _rule.Validate(position);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_VehicleIsInactive_ReturnsFailure()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Name = "VAN-001", IsActive = false };
            var position = new GpsPosition 
            { 
                VehicleId = 1, 
                Latitude = 37.9838, 
                Longitude = 23.7275, 
                RecordedAt = DateTime.UtcNow 
            };

            _mockVehicleRepo.Setup(x => x.GetById(1)).Returns(vehicle);

            // Act
            var result = _rule.Validate(position);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("inactive"));
        }

        [Test]
        public void Validate_VehicleDoesNotExist_ReturnsSuccess()
        {
            // Arrange - Vehicle doesn't exist (VehicleExistsRule handles this)
            var position = new GpsPosition 
            { 
                VehicleId = 999, 
                Latitude = 37.9838, 
                Longitude = 23.7275, 
                RecordedAt = DateTime.UtcNow 
            };
            
            _mockVehicleRepo.Setup(x => x.GetById(999)).Returns((Vehicle)null);

            // Act
            var result = _rule.Validate(position);

            // Assert - Should pass because VehicleExistsRule will catch it
            Assert.IsTrue(result.IsValid);
        }
    }

    /// <summary>
    /// Tests for DuplicateDetectionRule validation
    /// </summary>
    [TestFixture]
    public class DuplicateDetectionRuleTests
    {
        private Mock<IGpsPositionRepository> _mockGpsRepo;
        private DuplicateDetectionRule _rule;

        [SetUp]
        public void SetUp()
        {
            _mockGpsRepo = new Mock<IGpsPositionRepository>();
            _rule = new DuplicateDetectionRule(_mockGpsRepo.Object);
        }

        [Test]
        public void Validate_PositionDoesNotExist_ReturnsSuccess()
        {
            // Arrange
            var position = new GpsPosition 
            { 
                VehicleId = 1, 
                Latitude = 37.9838, 
                Longitude = 23.7275, 
                RecordedAt = DateTime.UtcNow 
            };

            _mockGpsRepo.Setup(x => x.PositionExists(1, It.IsAny<DateTime>())).Returns(false);

            // Act
            var result = _rule.Validate(position);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_PositionAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var recordedAt = DateTime.UtcNow;
            var position = new GpsPosition 
            { 
                VehicleId = 1, 
                Latitude = 37.9838, 
                Longitude = 23.7275, 
                RecordedAt = recordedAt 
            };

            _mockGpsRepo.Setup(x => x.PositionExists(1, recordedAt)).Returns(true);

            // Act
            var result = _rule.Validate(position);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("already exists"));
        }
    }
}
