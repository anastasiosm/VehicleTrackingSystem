using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;
using VehicleTracking.Core.Entities;

namespace VehicleTracking.Data.Context
{
	public class VehicleTrackingContext : DbContext
	{
		public VehicleTrackingContext() : base("VehicleTrackingDb")
		{
			// Disable lazy loading for better performance control
			Configuration.LazyLoadingEnabled = false;
			Configuration.ProxyCreationEnabled = false;
		}

		public DbSet<Vehicle> Vehicles { get; set; }
		public DbSet<GpsPosition> GpsPositions { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Vehicle configuration
			modelBuilder.Entity<Vehicle>()
				.ToTable("Vehicles")
				.HasKey(v => v.Id);

			modelBuilder.Entity<Vehicle>()
				.Property(v => v.Name)
				.IsRequired()
				.HasMaxLength(100);

			modelBuilder.Entity<Vehicle>()
				.Property(v => v.IsActive)
				.IsRequired();

			modelBuilder.Entity<Vehicle>()
				.Property(v => v.CreatedDate)
				.IsRequired();

			// GpsPosition configuration
			modelBuilder.Entity<GpsPosition>()
				.ToTable("GpsPositions")
				.HasKey(g => g.Id);

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.Latitude)
				.IsRequired();

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.Longitude)
				.IsRequired();

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.RecordedAt)
				.IsRequired();

			// Relationship: Vehicle -> GpsPositions (1:Many)
			modelBuilder.Entity<GpsPosition>()
				.HasRequired(g => g.Vehicle)
				.WithMany(v => v.GpsPositions)
				.HasForeignKey(g => g.VehicleId)
				.WillCascadeOnDelete(false);

			// Unique constraint on (VehicleId, RecordedAt)
			modelBuilder.Entity<GpsPosition>()
				.HasIndex(g => new { g.VehicleId, g.RecordedAt })
				.IsUnique()
				.HasName("IX_Vehicle_RecordedAt");

			// Index for querying by VehicleId and RecordedAt range
			modelBuilder.Entity<GpsPosition>()
				.HasIndex(g => g.VehicleId)
				.HasName("IX_VehicleId");

			modelBuilder.Entity<GpsPosition>()
				.HasIndex(g => g.RecordedAt)
				.HasName("IX_RecordedAt");
		}
	}
}