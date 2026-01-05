# 🚗 Vehicle Tracking & Route Visualization System

A real-time GPS tracking system for fleet management built with .NET Framework 4.6, ASP.NET Web API, Entity Framework 6, and vanilla JavaScript with Leaflet maps.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Database Schema](#database-schema)
- [Troubleshooting](#troubleshooting)
- [Future Improvements](#future-improvements)

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
  - Validation: Athens bounding box (37.9-38.1°N, 23.6-23.8°E)
  - Duplicate detection (VehicleId + RecordedAt)
  - Business rules: Inactive vehicles reject new positions

- ✅ **Data Querying**
  - List all vehicles
  - Get vehicle with last known position
  - Get GPS positions by time range
  - Calculate route distance

### Data Generator (Console App)
- ✅ **Automated GPS Generation**
  - Generates realistic movement within 50m radius
  - Configurable interval (default: 30 seconds)
  - Configurable positions per vehicle (default: 5)
  - Ensures positions stay within Athens bounding box
  - Handles API failures gracefully

---

## 🛠️ Technology Stack

### Backend
- **.NET Framework 4.6**
- **ASP.NET Web API** - RESTful services
- **Entity Framework 6** - ORM and database access
- **SQL Server Express / LocalDB** - Database
- **Unity Container** - Dependency Injection
- **Newtonsoft.Json** - JSON serialization

### Frontend
- **HTML5 + CSS3** - Modern UI
- **Vanilla JavaScript** - No framework dependencies
- **Leaflet.js** - Interactive maps
- **OpenStreetMap** - Map tiles

### Architecture Patterns
- **Repository Pattern** - Data access abstraction
- **Service Layer** - Business logic separation
- **Dependency Injection** - Loose coupling
- **DTO Pattern** - Clean API contracts

---

## 🏗️ Architecture

### Data Flow

```
User Browser
    ↓ (HTTP)
Frontend (index.html + app.js)
    ↓ (AJAX/Fetch)
Web API Controllers
    ↓
Service Layer (GpsService)
    ↓
Repository Layer
    ↓
Entity Framework 6
    ↓
SQL Server Database
```

---

## 📦 Prerequisites

### Required Software

1. **Visual Studio 2019 or 2022**
   - Workloads: .NET desktop development, ASP.NET and web development
   - [Download VS 2022 Community (Free)](https://visualstudio.microsoft.com/downloads/)

2. **SQL Server 2019+ Express Edition** (or LocalDB)
   - [Download SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - Or use LocalDB (included with Visual Studio)

3. **.NET Framework 4.6 Developer Pack**
   - [Download .NET Framework 4.6](https://dotnet.microsoft.com/download/dotnet-framework/net46)

4. **Git** (for cloning the repository)
   - [Download Git](https://git-scm.com/downloads)

### Optional Tools

- **SQL Server Management Studio (SSMS)** - For database inspection
- **Postman** - For API testing

---

## 🚀 Installation & Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/VehicleTrackingSystem.git
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

Edit both files:
- `VehicleTracking.Web\Web.config`
- `VehicleTracking.Data\App.config`

Find the `<connectionStrings>` section and use:

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

#### Optional: Use Migrations (if you prefer manual control)

```powershell
# In Package Manager Console
PM> Enable-Migrations -ProjectName VehicleTracking.Data
PM> Add-Migration InitialCreate -ProjectName VehicleTracking.Data
PM> Update-Database -ProjectName VehicleTracking.Data -Verbose
```

### Step 6: Build Solution

```
Build → Rebuild Solution
```

**Expected result:**
```
========== Rebuild All: 4 succeeded, 0 failed, 0 skipped ==========
```

---

## ▶️ Running the Application

### Quick Start (Recommended)

#### 1. **Start Web API First**

- Set **VehicleTracking.Web** as startup project (Right-click → Set as Startup Project)
- Press **F5** (Debug) or **Ctrl+F5** (Run without debugging)
- Browser will open at: `http://localhost:56789`

**What happens:**
- Database creates automatically (if first run)
- 100 vehicles are seeded
- Web API starts on port 56789
- Frontend (index.html) loads in browser

**Verify API is working:**
```bash
# Open these URLs in browser
http://localhost:56789/api/vehicles
http://localhost:56789/api/vehicles/with-last-positions
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
  API Base URL: http://localhost:56789
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

## 🎮 Using the Application

### Frontend UI

#### **Main View**

1. **Left Panel - Vehicle Grid**
   - Lists all vehicles
   - Shows status badge (green=active, red=inactive)
   - Displays last known position and timestamp
   - **Click any vehicle** to open detail view

2. **Right Panel - Live Map**
   - Shows all vehicle positions
   - Color-coded markers:
     - 🟢 Green = Active vehicle
     - 🔴 Red = Inactive vehicle
   - **Click marker** for popup with vehicle info
   - Auto-refreshes every 30 seconds

3. **Header**
   - Vehicle count
   - Last update timestamp
   - Status indicator (pulsing green dot = active polling)
   - **Refresh button** - Manual refresh

#### **Vehicle Detail Modal**

Click any vehicle to open:

1. **Date Range Filter**
   - Default: Last 24 hours
   - Customize start/end datetime
   - Click **Apply Filter** to update

2. **Route Map**
   - Blue polyline connecting GPS points
   - 🟢 Green marker = Route start
   - 🔴 Red marker = Route end

3. **Statistics**
   - Total Distance (meters/kilometers)
   - Data Points count
   - Time range covered

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
      "lastPositionTimestamp": "2026-01-04T14:30:00Z",
      "lastLatitude": 37.9838,
      "lastLongitude": 23.7275
    }
  ]
}
```

#### `GET /api/vehicles/{id}`
Get specific vehicle details.

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
  "recordedAt": "2026-01-04T14:30:00Z"
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
      "recordedAt": "2026-01-04T14:30:00Z"
    },
    {
      "latitude": 37.9840,
      "longitude": 23.7277,
      "recordedAt": "2026-01-04T14:30:05Z"
    }
  ]
}
```

#### `GET /api/gps/vehicle/{vehicleId}/positions?from={datetime}&to={datetime}`
Get GPS positions for a vehicle within a time range.

**Query Parameters:**
- `from` (optional): Start datetime (ISO 8601)
- `to` (optional): End datetime (ISO 8601)
- Default: Last 24 hours

#### `GET /api/gps/vehicle/{vehicleId}/route?from={datetime}&to={datetime}`
Get vehicle route with calculated distance.

**Response:**
```json
{
  "success": true,
  "data": {
    "vehicleId": 1,
    "vehicleName": "VAN-001",
    "positions": [...],
    "totalDistanceMeters": 1234.56,
    "positionCount": 50
  }
}
```

#### `GET /api/gps/vehicle/{vehicleId}/last-position`
Get the most recent position for a vehicle.

---

## ⚙️ Configuration

### Web API Configuration

**File:** `VehicleTracking.Web\Web.config`

```xml
<!-- Database -->
<connectionStrings>
  <add name="VehicleTrackingDb" connectionString="..." />
</connectionStrings>

<!-- CORS (already enabled) -->
<httpProtocol>
  <customHeaders>
    <add name="Access-Control-Allow-Origin" value="*" />
  </customHeaders>
</httpProtocol>
```

### Data Generator Configuration

**File:** `VehicleTracking.DataGenerator\App.config`

```xml
<appSettings>
  <!-- API Base URL -->
  <add key="ApiBaseUrl" value="http://localhost:56789" />
  
  <!-- Interval between generation cycles (seconds) -->
  <add key="IntervalSeconds" value="30" />
  
  <!-- Number of positions generated per vehicle per cycle -->
  <add key="PositionsPerVehicle" value="5" />
  
  <!-- Maximum radius for movement (meters) -->
  <add key="RadiusMeters" value="50" />
</appSettings>
```

**Customization Examples:**

- **Faster updates:** `IntervalSeconds = 10`
- **More positions:** `PositionsPerVehicle = 10`
- **Larger movement:** `RadiusMeters = 100`

### Frontend Configuration

**File:** `VehicleTracking.Web\js\app.js`

```javascript
const CONFIG = {
    apiBaseUrl: 'http://localhost:56789/api',
    pollingInterval: 30000, // 30 seconds in milliseconds
};
```

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
CREATE UNIQUE INDEX IX_Vehicle_RecordedAt ON GpsPositions(VehicleId, RecordedAt);
```

### Seed Data

**Vehicles:** 100 pre-seeded vehicles with names like:
- VAN-001 to VAN-015
- TRUCK-016 to TRUCK-030
- CAR-031 to CAR-045
- BUS-046 to BUS-060
- etc.

**Initial GPS Positions:** First 10 vehicles have 5 sample positions each (50 total positions).

---

## 🎯 Future Improvements

### Short-term (Easy wins)
- [ ] Add vehicle search/filter in grid
- [ ] Export route data to CSV
- [ ] Configurable map center and zoom
- [ ] Dark mode toggle
- [ ] Vehicle status change from UI

### Medium-term (More features)
- [ ] Real-time updates with SignalR (instead of polling)
- [ ] Authentication and user roles
- [ ] Multiple bounding boxes (different cities)
- [ ] Geocoding (address display from coordinates)
- [ ] Speed calculation and alerts
- [ ] Geofencing and alerts

### Long-term (Architectural)
- [ ] Migrate to .NET 6/8
- [ ] Add Docker support
- [ ] Implement CQRS pattern
- [ ] Add Redis caching layer
- [ ] Implement message queue (RabbitMQ/Azure Service Bus)
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Microservices architecture

---


**Enjoy tracking! 🚗💨**

