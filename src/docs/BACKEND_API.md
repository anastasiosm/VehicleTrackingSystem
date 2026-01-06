# Backend API Design

## Architecture

The Backend is built using **ASP.NET Web API 2** and follows a **Clean Architecture** approach:

### Layers
1. **Domain**: Entities (`Vehicle`, `GpsPosition`), Value Objects (`Coordinates`), and Core Exceptions.
2. **Application**: Business interfaces (`IGpsService`, `IVehicleService`) and Data Transfer Objects (DTOs).
3. **Infrastructure**: Implementations of external services (Geographical formulas, Validation rules).
4. **Persistence**: EF6 DbContext and Repository implementations.
5. **Web (Presentation)**: Controllers, Autofac configuration, and API models.

## Key Features

### 1. Inversion of Control (DI)
The entire API is decoupled using **Autofac**. Controllers never instantiate services directly; they depend on interfaces.

### 2. Structured Logging
Powered by **Serilog**. All API requests, validation failures, and internal errors are logged with contextual data for easier troubleshooting.

### 3. Validation Logic
- **Geographic Validation**: Ensures GPS coordinates fall within the allowed Athens Bounding Box.
- **State Validation**: Prevents data ingestion for inactive vehicles.
- **Temporal Integrity**: Ensures positions are recorded with valid UTC timestamps.

## API Endpoints (Summary)

- `GET /api/vehicles`: List all vehicles with last known status.
- `POST /api/gps/position`: Submit a single GPS reading.
- `POST /api/gps/positions/batch`: Submit multiple readings for a single vehicle (optimized for simulation).
- `GET /api/gps/vehicle/{id}/route`: Retrieve historical route data with distance calculations.

## Database Integration
Uses **Entity Framework 6** with a Repository pattern to abstract data access and ensure the Domain remains persistent-ignorant.
