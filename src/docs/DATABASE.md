# Database Design & Data Modeling

This section describes the database schema, design decisions, and assumptions made for the Vehicle Tracking & Route Visualization System.

The database is intentionally kept simple and minimal, focusing on correctness, performance for time-series data, and clarity rather than over-engineering.

---

## Overview

The system uses a relational SQL database (SQL Server / LocalDB) and consists of two core tables:

- `Vehicles`
- `GpsPositions`

The schema is designed to support:
- frequent inserts of GPS position data
- efficient querying by vehicle and time range
- retrieval of latest known vehicle positions
- enforcement of basic business rules at database level

---

## Entity Relationship Diagram (ERD)

### Conceptual Model

```
+----------------------+
|      Vehicles        |
+----------------------+
| PK  Id               |
|     Name             |
|     IsActive         |
|     CreatedDate      |
+----------+-----------+
           |
           | 1
           |
           | N
+----------v-----------+
|    GpsPositions      |
+----------------------+
| PK  Id               |
| FK  VehicleId        |
|     Latitude         |
|     Longitude        |
|     RecordedAt       |
+----------------------+

UNIQUE: (VehicleId, RecordedAt)
```

### Extended ERD (with Constraints & Indexes)

```
+------------------------------------------------+
|                  Vehicles                      |
+------------------------------------------------+
| PK  Id                INT IDENTITY             |
|     Name              NVARCHAR                 |
|     IsActive          BIT                      |
|     CreatedDate       DATETIME                 |
+------------------------------------------------+
| Indexes:                                       |
|  - PK_Id                               (CL)    |
+------------------------------------------------+

                         1
                         |
                         |
                         | FK_Vehicle_GPS
                         |
                         N

+------------------------------------------------+
|               GpsPositions                     |
+------------------------------------------------+
| PK  Id                BIGINT IDENTITY          |
| FK  VehicleId         INT                      |
|     Latitude          DECIMAL(9,6)             |
|     Longitude         DECIMAL(9,6)             |
|     RecordedAt        DATETIME                 |
+------------------------------------------------+
| Constraints:                                   |
|  - FK VehicleId -> Vehicles(Id)                |
|  - UNIQUE (VehicleId, RecordedAt)              |
|                                                |
| Indexes:                                       |
|  - IX_GpsPosition_VehicleId_RecordedAt (VehicleId, Time) |
+------------------------------------------------+

(CL = Clustered)
```

---

## Table: Vehicles

### Purpose

Represents a physical vehicle equipped with a GPS sensor. This table is relatively static compared to GPS position data.

### Columns

| Column | Type | Description |
|--------|------|-------------|
| Id | INT (PK) | Primary key |
| Name | NVARCHAR | Human-readable identifier |
| IsActive | BIT | Indicates whether the vehicle accepts new GPS data |
| CreatedDate | DATETIME | Creation timestamp |

### Design Notes

- Vehicles are not deleted from the database.
- The `IsActive` flag is used to enforce the rule that inactive vehicles should not accept new GPS positions.
- No unique constraint is enforced on `Name`, as it is not used as a technical identifier.
- Expected record count: ~100-200 vehicles

---

## Table: GpsPositions

### Purpose

Stores individual GPS position readings for vehicles. This is the main time-series data table and is expected to grow continuously.

### Columns

| Column | Type | Description |
|--------|------|-------------|
| Id | BIGINT (PK) | Primary key |
| VehicleId | INT (FK) | Reference to Vehicles |
| Latitude | DECIMAL(9,6) | GPS latitude |
| Longitude | DECIMAL(9,6) | GPS longitude |
| RecordedAt | DATETIME | Timestamp of the GPS reading |

### Constraints

**Foreign Key**: `VehicleId → Vehicles(Id)`
- Ensures referential integrity
- Prevents orphan GPS positions
- No cascade delete (preserves historical data)

**Unique Constraint**: `(VehicleId, RecordedAt)`
- Ensures that a vehicle cannot have more than one GPS position recorded at the same timestamp
- Protects against:
  - Retry requests
  - Concurrent inserts

### Design Rationale

**Why BIGINT for Id?**
- Supports very large number of records over time
- GPS data accumulates continuously

**Why DECIMAL for coordinates?**
- Avoids floating-point precision issues
- ~6 decimal places provides meter-level accuracy

**Why RecordedAt and not server timestamp?**
- Represents actual GPS measurement time
- Essential for chronological ordering

---

## Indexing Strategy

### Composite Index

```
IX_GpsPosition_VehicleId_RecordedAt (VehicleId, RecordedAt)
```

**Supports:**
- Retrieving GPS positions for a vehicle within a time range
- Ordering GPS positions chronologically
- Retrieving the latest known position of a vehicle

**Query Pattern:**
```sql
WHERE VehicleId = X 
AND RecordedAt BETWEEN @From AND @To
ORDER BY RecordedAt ASC
```

---

## Entity Framework 6 Mapping Decisions (Persistence Layer)

### Vehicle Entity
- Convention-based primary key (`Id`)
- Required fields: `Name`, `IsActive`, `CreatedDate`
- Navigation property: `ICollection<GpsPosition>`
- **Design Decision**: `Vehicle` is an **Aggregate Root** in the Domain layer.

### GpsPosition Entity
- Explicit foreign key (`VehicleId`)
- BIGINT primary key
- DECIMAL(9,6) precision for coordinates
- **Design Decision**: Represents **Time-Series** data with an immutable `RecordedAt` timestamp.

### Unique Constraint Mapping
The `(VehicleId, RecordedAt)` unique constraint is defined via the **Fluent API** in `VehicleTrackingContext` to prevent duplicate readings from IoT devices.

---

## Possible Future Improvements

### Performance Optimization
- Partition `GpsPositions` by date (year/month).
- Implement a caching layer (e.g., Redis) for latest known positions.

---

## Summary

The database design is **Clean**, **Robust**, and **Decoupled**, providing a solid foundation that scales effectively while remaining maintainable through the Persistence layer abstractions.