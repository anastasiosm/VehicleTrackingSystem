# 🚗 Vehicle Tracking & Route Visualization System

A real-time GPS tracking system for fleet management built with .NET Framework 4.6, ASP.NET Web API, Entity Framework 6, and vanilla JavaScript with Leaflet maps.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Database Schema](#database-schema)
- [Architectural & Modeling Decisions](#architectural--modeling-decisions)
- [Assumptions Made](#assumptions-made)
- [What I Would Improve With More Time](#what-i-would-improve-with-more-time)
- [Troubleshooting](#troubleshooting)
- [Documentation Structure](#documentation-structure)]

---

## 🎯 Overview

This system simulates and tracks GPS-equipped vehicles in real-time, similar to IoT and fleet management solutions. Vehicles send periodic location data, which is stored and visualized on an interactive map. Users can view vehicle positions, inspect historical routes, and calculate traveled distances.

### Key Capabilities:
- **Real-time tracking** of 100+ vehicles
- **Route visualization** with distance calculations
- **Automated data generation** for testing and demo purposes
- **Single Page Application** with no page reloads
- **RESTful API** following best practices

---

## ✨ Features

### Frontend (Single Page Application)
- ✅ **Vehicle Grid View**
  - Display all vehicles with current status (active/inactive)
  - Show last known position and timestamp
  - Auto-refresh every 30 seconds (configurable)
  - Click vehicle to view details

- ✅ **Live Map**
  - Display all vehicle positions with color-coded markers
  - Green markers: Active vehicles
  - Red markers: Inactive vehicles
  - Interactive popups with vehicle info

- ✅ **Vehicle Detail Modal**
  - DateTime range filter
  - Route visualization (blue polyline connecting GPS points)
  - Start marker (green) and end marker (red)
  - Calculate total traveled distance (Haversine formula)
  - Display data points count and time range

### Backend (Web API)
- ✅ **GPS Data Ingestion** 
  - Single position submission
  - Batch position submission (optimized)
  - **Multi-layer Validation:**
    - Domain: Coordinate range validation (lat: [-90, 90], lon: [-180, 180])
    - Application: Vehicle existence, active status, geographic bounds (Athens)
    - Infrastructure: Duplicate detection
  - Business rules: Inactive vehicles reject new positions

- ✅ **Data Querying**
  - List all vehicles
  - Get vehicle with last known position
  - Get GPS positions by time range
  - Calculate route distance (Haversine formula)

- ✅ **Service Layer**
  - `GpsService` - GPS data ingestion and querying
  - `VehicleService` - Vehicle management
  - `RouteCalculationService` - Distance calculations (separated for SRP)
  - `GeographicalService` - Haversine distance, bounding box checks

### Data Generator (Console App)
- ✅ **Automated GPS Generation**
  - Generates realistic movement within 50m radius
  - Configurable interval (default: 30 seconds)
  - Configurable positions per vehicle (default: 5)
  - Ensures positions stay within Athens bounding box
  - **Polly retry policies** with exponential backoff (2s, 4s, 8s)
  - Graceful error handling and detailed logging
  - Parallel processing for multiple vehicles

### Testing (Unit Tests)
- ✅ **25+ Unit Tests** with NUnit + Moq
  - Domain: Coordinates validation, value object behavior (8 tests)
  - Application: RouteCalculationService, business logic (9 tests)
  - Infrastructure: Validation rules (Strategy Pattern) (6+ tests)
  - Test coverage: Domain invariants, edge cases, mocking

---

## 📦 Prerequisites

### Required Software

1. **Visual Studio 2019 or 2022**
   - Workloads: .NET desktop development, ASP.NET and web development
   - [Download VS 2022 Community (Free)](https://visualstudio.microsoft.com/downloads/)

2. **SQL Server 2019+ Express Edition** (or LocalDB)
   - [Download SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - Or use LocalDB (included with Visual Studio)

3. **.NET Framework 4.6.1 Developer Pack**
   - [Download .NET Framework 4.6.1](https://dotnet.microsoft.com/download/dotnet-framework/net461)

4. **Git** (for cloning the repository)
   - [Download Git](https://git-scm.com/downloads)

### Optional Tools
- **SQL Server Management Studio (SSMS)** - For database inspection
- **Postman** - For API testing

---

## 🚀 Installation & Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/anastasiosm/VehicleTrackingSystem.git
cd VehicleTrackingSystem
```

### Step 2: Open Solution in Visual Studio

```bash
# Open the solution file
VehicleTrackingSystem.sln
```

Or from Visual Studio:
- **File** → **Open** → **Project/Solution**
- Navigate to `VehicleTrackingSystem.sln`

### Step 3: Restore NuGet Packages

Visual Studio should automatically restore packages. If not:

1. Right-click **Solution** → **Restore NuGet Packages**
2. Or use Package Manager Console:
   ```powershell
   Update-Package -reinstall
   ```

### Step 4: Configure Database Connection

#### Option A: SQL Server Express (Recommended)

Edit: `src\VehicleTracking.Web\Web.config`

```xml
<connectionStrings>
  <add name="VehicleTrackingDb" 
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=VehicleTrackingDb;Integrated Security=True;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

#### Option B: LocalDB (Lightweight)

```xml
<connectionStrings>
  <add name="VehicleTrackingDb" 
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=VehicleTrackingDb;Integrated Security=True;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Step 5: Initialize Database

The database will be **created automatically** on first run, including:
- Database schema (Vehicles, GpsPositions tables)
- Seed data (100 vehicles)
- Sample GPS positions

**No manual migration needed!** Just run the Web API (see next section).

### Step 6: Build Solution

```
Build → Rebuild Solution
```

**Expected result:**
```
========== Rebuild All: 5 succeeded, 0 failed, 0 skipped ==========
```

---

## ▶️ Running the Application

### Quick Start (Recommended)

#### 1. **Start Web API First**

- Set **VehicleTracking.Web** as startup project (Right-click → Set as Startup Project)
- Press **F5** (Debug) or **Ctrl+F5** (Run without debugging)
- Browser will open at: `http://localhost:5000`

**What happens:**
- Database creates automatically (if first run)
- 100 vehicles are seeded
- Web API starts
- Frontend (index.html) loads in browser

**Verify API is working:**
```bash
# Open these URLs in browser
http://localhost:5000/api/vehicles
http://localhost:5000/api/vehicles/with-last-positions
```

You should see JSON data! ✅

#### 2. **Start Data Generator (Optional but Recommended)**

**While Web API is running**, start the console app:

**Method A: Multiple Startup Projects**
1. Right-click **Solution** → **Properties**
2. Select **Multiple startup projects**
3. Set both to **Start**:
   - VehicleTracking.Web: **Start**
   - VehicleTracking.DataGenerator: **Start**
4. Click **OK**
5. Press **F5** - Both projects start together! 🎉

**Method B: Manual Start**
1. Keep Web API running
2. Right-click **VehicleTracking.DataGenerator** → **Debug** → **Start New Instance**

**Console Output:**
```
==============================================
Vehicle Tracking - GPS Data Generator
==============================================

Configuration:
  API Base URL: http://localhost:5000
  Interval: 30 seconds
  Positions per vehicle: 5
  Movement radius: 50 meters

Press CTRL+C to stop the generator...

[14:30:15] Starting iteration #1...
  ✓ Generated positions for 100 vehicles
  ✓ Total positions submitted: 500
  ✓ Failed submissions: 0

Next iteration in 30 seconds...
```

---

## 📡 API Endpoints

### Vehicles

#### `GET /api/vehicles`
Get all vehicles with basic info.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "VAN-001",
      "isActive": true,
      "lastPositionTimestamp": "2026-01-06T14:30:00Z"
    }
  ]
}
```

#### `GET /api/vehicles/with-last-positions`
Get all vehicles with their last known GPS position (optimized for map display).

---

### GPS Positions

#### `POST /api/gps/position`
Submit a single GPS position.

**Request:**
```json
{
  "vehicleId": 1,
  "latitude": 37.9838,
  "longitude": 23.7275,
  "recordedAt": "2026-01-06T14:30:00Z"
}
```

**Validations:**
- Vehicle must exist and be active
- Coordinates must be within Athens bounding box (37.9-38.1, 23.6-23.8)
- No duplicate (VehicleId, RecordedAt) pairs

#### `POST /api/gps/positions/batch`
Submit multiple positions for a vehicle (recommended for performance).

**Request:**
```json
{
  "vehicleId": 1,
  "positions": [
    {
      "latitude": 37.9838,
      "longitude": 23.7275,
      "recordedAt": "2026-01-06T14:30:00Z"
    }
  ]
}
```

#### `GET /api/gps/vehicle/{vehicleId}/route?from={datetime}&to={datetime}`
Get vehicle route with calculated distance.

**Response:**
```json
{
  "success": true,
  "data": {
    "vehicleId": 1,
    "vehicleName": "VAN-001",
    "totalDistanceMeters": 1234.56,
    "positionCount": 50
  }
}
```

---

## ⚙️ Configuration

### Web API Configuration

**File:** `src\VehicleTracking.Web\Web.config`

```xml
<connectionStrings>
  <add name="VehicleTrackingDb" connectionString="..." />
</connectionStrings>
```

### Data Generator Configuration

**File:** `src\VehicleTracking.DataGenerator\App.config`

```xml
<appSettings>
  <add key="ApiBaseUrl" value="http://localhost:5000" />
  <add key="IntervalSeconds" value="30" />
  <add key="PositionsPerVehicle" value="5" />
  <add key="RadiusMeters" value="50" />
</appSettings>
```

**Customization Examples:**
- **Faster updates:** `IntervalSeconds = 10`
- **More positions:** `PositionsPerVehicle = 10`
- **Larger movement:** `RadiusMeters = 100`

---

## 🗄️ Database Schema

### Tables

#### **Vehicles**
```sql
CREATE TABLE Vehicles (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedDate DATETIME NOT NULL
);
```

#### **GpsPositions**
```sql
CREATE TABLE GpsPositions (
    Id BIGINT PRIMARY KEY IDENTITY,
    VehicleId INT NOT NULL FOREIGN KEY REFERENCES Vehicles(Id),
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL,
    RecordedAt DATETIME NOT NULL,
    CONSTRAINT UQ_Vehicle_RecordedAt UNIQUE (VehicleId, RecordedAt)
);

-- Indexes
CREATE INDEX IX_VehicleId ON GpsPositions(VehicleId);
CREATE INDEX IX_RecordedAt ON GpsPositions(RecordedAt);
```

### Seed Data
- **100 vehicles** (VAN-001 to BUS-100)
- **50 sample GPS positions** (first 10 vehicles)

---

## 🏗️ Architectural & Modeling Decisions

### **1. Clean Architecture**
Separated into Domain, Application, Infrastructure, Persistence, Web layers for testability and maintainability.

### **2. Strategy Pattern for Validation**
Separate validation rules (VehicleExists, VehicleActive, GeographicBounds, DuplicateDetection) for Open/Closed Principle.

### **3. Value Objects**
`Coordinates` as readonly struct with validation enforces domain invariants.

### **4. Repository Pattern**
Abstracted data access behind interfaces for testability.

### **5. Batch API**
Reduces HTTP overhead by submitting multiple positions in one request.

### **6. Exception Hierarchy**
Domain exceptions for business rules, Application exceptions for use case failures.

### **7. Polly Retry Policies**
Exponential backoff (2s, 4s, 8s) for transient HTTP failures.

---

## 🤔 Assumptions Made

1. **Athens-only** - Hardcoded bounding box (37.9-38.1°N, 23.6-23.8°E)
2. **UTC timestamps** - No timezone complexity
3. **No authentication** - Demo environment
4. **Single vehicle type** - No Car/Truck/Bus differentiation
5. **Polling over WebSockets** - Simpler implementation
6. **In-memory caching** - No Redis needed for demo
7. **LocalDB sufficient** - Not production scale

---

## ⏰ What I Would Improve With More Time

### Short-Term (1-2 days)
- Integration tests with real database
- SignalR for real-time updates
- Frontend enhancements (search, dark mode, CSV export)

### Medium-Term (1 week)
- JWT authentication & RBAC
- Redis distributed cache
- Application Insights monitoring
- Multi-region support

### Long-Term (2+ weeks)
- Event Sourcing for audit trail
- CQRS + MediatR (when 20+ use cases)
- Microservices architecture
- Docker + Kubernetes
- Advanced ML features (predictive routing, anomaly detection)

---

## 🐛 Troubleshooting

### Common Issues

**Problem:** Database connection errors  
**Solution:** Verify SQL Server is running and connection string is correct in `Web.config`

**Problem:** API returns 500 errors  
**Solution:** Check `src\VehicleTracking.Web\App_Data\logs\web.log` for detailed error messages

**Problem:** Data Generator fails to connect  
**Solution:** Ensure Web API is running first and `ApiBaseUrl` in `App.config` is correct

**Problem:** Tests not appearing in Test Explorer  
**Solution:** Clean & Rebuild solution, restart Visual Studio

### Enable Detailed Logging

Edit `Web.config`:
```xml
<system.diagnostics>
  <trace autoflush="true">
    <listeners>
      <add name="textWriterTraceListener" type="System.Diagnostics.TextWriterTraceListener" initializeData="C:\logs\app.log" />
    </listeners>
  </trace>
</system.diagnostics>
```

---

## Documentation Structure

For more detailed technical information, please refer to the following documents in the `/docs` folder:

- **[DATABASE.md](./docs/DATABASE.md)**: Details on SQL schema, Entity Relationship Diagram, and EF6 mapping configurations.
- **[BACKEND_API.md](./docs/BACKEND_API.md)**: Complete list of REST endpoints, DTO structures, and architectural layering rules.
- **[GPS_GENERATOR.md](./docs/GPS_GENERATOR.md)**: In-depth look at the High Performance Simulator, including parallel processing logic and movement algorithms.
- **[FRONTEND.md](./docs/FRONTEND.md)**: Overview of the Single Page Application, Leaflet.js integration, and real-time polling implementation.


**Enjoy tracking! 🚗💨**

