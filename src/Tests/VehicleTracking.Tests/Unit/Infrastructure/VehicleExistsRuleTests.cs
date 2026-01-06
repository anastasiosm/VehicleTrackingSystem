using Moq;
using NUnit.Framework;
using VehicleTracking.Infrastructure.Validation;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Domain.Entities;
using System;

namespace VehicleTracking.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class VehicleExistsRuleTests
    {
        private Mock<IVehicleRepository> _mockVehicleRepo;
        private VehicleExistsRule _rule;

        [SetUp]
        public void SetUp()
        {
            _mockVehicleRepo = new Mock<IVehicleRepository>();
            _rule = new VehicleExistsRule(_mockVehicleRepo.Object);
        }

        [Test]
        public void Validate_VehicleExists_ReturnsSuccess()
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
            Assert.IsEmpty(result.Errors);
        }

        [Test]
        public void Validate_VehicleDoesNotExist_ReturnsFailure()
        {
            // Arrange
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

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.That(result.Errors[0], Does.Contain("Vehicle with ID 999 does not exist"));
        }
    }
}
