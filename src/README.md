# Vehicle Tracking & Route Visualization System

## Table of Contents

1. [Overview](#overview)
2. [Architecture & Design Decisions](#architecture--design-decisions)
3. [Database Design](./docs/DATABASE.md)
4. [Backend API Design](./docs/BACKEND_API.md)
5. [High Performance GPS Simulator](./docs/GPS_GENERATOR.md)
6. [Frontend Architecture](./docs/FRONTEND.md)

---

## Overview

This project is an **Enterprise-Grade Vehicle Tracking System** designed to demonstrate modern software engineering principles in a .NET Framework environment.

**Key Highlights**:
- **Clean Architecture**: Separation of concerns into Domain, Application, Infrastructure, and Persistence layers.
- **Dependency Injection**: Fully decoupled components using **Autofac**.
- **Structured Logging**: Advanced telemetry and diagnostics using **Serilog**.
- **High Concurrency**: Asynchronous GPS data simulation with parallel processing.

**Purpose**: Store, process, and visualize real-time vehicle movement with geographic constraints.

**Technology Stack**:
- **Backend**: ASP.NET Web API 2 (.NET Framework 4.6.1)
- **DI Container**: Autofac
- **Logging**: Serilog (Console & Rolling Files)
- **ORM**: Entity Framework 6 (SQL Server)
- **Frontend**: SPA with HTML5/CSS3/Vanilla JS + **Leaflet.js**
- **Simulator**: .NET Console App with Task-based parallelism

---

## Key Features

### 1. Data Ingestion
- **Robust API**: RESTful endpoints for single and batch GPS data submission.
- **Validation**: Strict validation of vehicle status, geographic boundaries (Athens), and temporal integrity.
- **Performance**: Optimized batch processing to handle high-frequency data from multiple vehicles.

### 2. Real-time Visualization
- **Live Map**: Leaflet.js-based dashboard showing the latest known positions.
- **Route History**: Visualize historical paths with automatic distance calculations.
- **Polling System**: Smooth UI updates every 30 seconds to reflect the latest fleet movements.

### 3. Smart Simulation
- **High Concurrency**: Parallelized simulator processing hundreds of vehicles simultaneously.
- **Realistic Logic**: Random walk algorithm with boundary enforcement and automatic redirection.

## Quick API Reference

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/vehicles` | List all vehicles with last known status. |
| **GET** | `/api/vehicles/with-last-positions` | Detailed list including full position objects. |
| **POST** | `/api/gps/position` | Submit a single GPS reading. |
| **POST** | `/api/gps/positions/batch` | Optimized batch submission for a single vehicle. |
| **GET** | `/api/gps/vehicle/{id}/route` | Get historical route and total distance. |
| **GET** | `/api/gps/vehicle/{id}/last-position` | Get only the most recent coordinate. |

---

## Architecture & Design Decisions

### Clean Architecture (DDD Oriented)

The solution is split into distinct layers to ensure maintainability and testability:

- **`VehicleTracking.Domain`**: Core entities, value objects (Coordinates, BoundingBox), and business exceptions. Zero external dependencies.
- **`VehicleTracking.Application`**: Use cases, service interfaces, and application logic.
- **`VehicleTracking.Infrastructure`**: Implementation of cross-cutting concerns (Geographical calculations, GPS validation).
- **`VehicleTracking.Persistence`**: EF6 Data Context, Repositories, and Database Initializers.
- **`VehicleTracking.Web`**: The API entry point, Autofac setup, and SPA assets.

### Key Decisions & Trade-offs

**✓ Parallel Processing in Simulator**
- **Decision**: Used `Task.WhenAll` to process multiple vehicles simultaneously.
- **Benefit**: Can scale to hundreds of simulated devices without blocking the main loop.

**✓ Structured Logging (Serilog)**
- **Decision**: Replaced simple console output with Serilog templates (`{VehicleId}`).
- **Benefit**: Enables rich querying and analysis in tools like Seq or ElasticSearch.

**✓ Logic Extraction (IPositionSimulator)**
- **Decision**: Separated "Orchestration" (Generator) from "Movement Logic" (Simulator).
- **Benefit**: The Generator is now geographic-agnostic and 100% testable via mocks.

**✓ Unique Constraint on (VehicleId, RecordedAt)**
- **Rule**: One vehicle cannot have two different positions at the exact same time.
- **Enforcement**: Database-level unique index ensures data integrity.

---

## Performance Characteristics

- **Ingestion**: Supports batch GPS submissions for reduced HTTP overhead.
- **Querying**: Optimized composite indexes for time-series vehicle data.
- **Scalability**: Designed to handle ~300,000+ GPS records per day in a demonstration environment.

---

## Getting Started

### Prerequisites
- **IDE**: Visual Studio 2017 or newer.
- **Framework**: .NET Framework 4.6.1.
- **Database**: SQL Server Express or LocalDB (included with Visual Studio).
- **Browser**: Any modern web browser (Chrome, Edge, Firefox).

### Step-by-Step Execution

#### 1. Database Setup
The project uses **Entity Framework 6 Code First**. You don't need to manually run SQL scripts.
- Ensure your SQL LocalDB instance is running.
- The connection string in `VehicleTracking.Web/Web.config` is pre-configured for LocalDB.
- The database (`VehicleTrackingDB`) and its schema will be created automatically the first time you run the Web API.

#### 2. Running the Backend API
- Open `VehicleTrackingSystem.sln` in Visual Studio.
- Right-click on the **`VehicleTracking.Web`** project and select **Set as Startup Project**.
- Press **F5** or click **Start**.
- Visual Studio will launch IIS Express. Verify the API is running by navigating to `http://localhost:5000/api/vehicles`. You should see a JSON success response.

#### 3. Running the GPS Simulator
- Keep the Web API running.
- Right-click on the **`VehicleTracking.DataGenerator`** project -> **Debug** -> **Start New Instance**.
- A console window will appear. It will automatically:
  - Resolve dependencies via **Autofac**.
  - Read configuration from `App.config`.
  - Start generating and submitting GPS data in parallel for all active vehicles.
- You can monitor the submission status directly in the console or check the logs at `VehicleTracking.DataGenerator/bin/Debug/logs/generator.log`.

#### 4. Launching the Frontend
- The frontend is a Single Page Application (SPA).
- Navigate to the `VehicleTracking.Web` folder and open **`index.html`** in your browser.
- Alternatively, if the Web project is running, navigate to `http://localhost:5000/index.html`.
- You will see the vehicle grid and the live map. Data will refresh automatically every 30 seconds.

---

## Telemetry & Diagnostics (Logs)

The system uses **Serilog** for structured logging. If you encounter issues, check the following locations:
- **API Logs**: `src/VehicleTracking.Web/App_Data/logs/web.log`
- **Simulator Logs**: `src/VehicleTracking.DataGenerator/bin/Debug/logs/generator.log`

---

## Documentation Structure

For more detailed technical information, please refer to the following documents in the `/docs` folder:

- **[DATABASE.md](./docs/DATABASE.md)**: Details on SQL schema, Entity Relationship Diagram, and EF6 mapping configurations.
- **[BACKEND_API.md](./docs/BACKEND_API.md)**: Complete list of REST endpoints, DTO structures, and architectural layering rules.
- **[GPS_GENERATOR.md](./docs/GPS_GENERATOR.md)**: In-depth look at the High Performance Simulator, including parallel processing logic and movement algorithms.
- **[FRONTEND.md](./docs/FRONTEND.md)**: Overview of the Single Page Application, Leaflet.js integration, and real-time polling implementation.
