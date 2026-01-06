using System.Data.Entity;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Persistence.Context
{
	public class VehicleTrackingContext : DbContext
	{
		public VehicleTrackingContext() : base("VehicleTrackingDb")
		{
		}

		public DbSet<Vehicle> Vehicles { get; set; }
		public DbSet<GpsPosition> GpsPositions { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// ===== Vehicle Configuration =====
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

			// ===== GpsPosition Configuration =====
			modelBuilder.Entity<GpsPosition>()
				.ToTable("GpsPositions")
				.HasKey(g => g.Id);

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.VehicleId)
				.IsRequired();

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.Latitude)
				.IsRequired();

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.Longitude)
				.IsRequired();

			modelBuilder.Entity<GpsPosition>()
				.Property(g => g.RecordedAt)
				.IsRequired();

			// ===== Relationship Configuration (Fluent API) =====
			modelBuilder.Entity<GpsPosition>()
				.HasRequired(g => g.Vehicle)        // GpsPosition requires a Vehicle
				.WithMany(v => v.GpsPositions)      // Vehicle can have many GpsPositions
				.HasForeignKey(g => g.VehicleId)
				.WillCascadeOnDelete(false);

			// ===== Indexes for Performance =====
			modelBuilder.Entity<GpsPosition>()
				.HasIndex(g => g.VehicleId)
				.HasName("IX_GpsPosition_VehicleId");

			modelBuilder.Entity<GpsPosition>()
				.HasIndex(g => g.RecordedAt)
				.HasName("IX_GpsPosition_RecordedAt");

			// Composite unique index to prevent duplicate positions for the same vehicle at the same time
			modelBuilder.Entity<GpsPosition>()
				.HasIndex(g => new { g.VehicleId, g.RecordedAt })
				.IsUnique()
				.HasName("IX_GpsPosition_VehicleId_RecordedAt");
		}
	}
}
