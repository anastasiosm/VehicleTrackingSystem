# Backend API Design & Architecture

This document describes the backend API architecture, endpoints, validation rules, and design decisions for the Vehicle Tracking & Route Visualization System.

---

## Architecture Overview

The Backend is built using **ASP.NET Web API 2** and follows a **Clean Architecture** approach with clear separation of concerns:

### Layer Structure

1. **Domain Layer** (`VehicleTracking.Domain`)
   - Core entities: `Vehicle`, `GpsPosition`
   - Value objects: `Coordinates`, `BoundingBox`
   - Domain exceptions: `InvalidCoordinatesException`, `VehicleNotFoundException`
   - Business rules and invariants

2. **Application Layer** (`VehicleTracking.Application`)
   - Service interfaces: `IGpsService`, `IVehicleService`, `IRouteCalculationService`
   - Data Transfer Objects (DTOs)
   - Application-specific exceptions
   - Use case orchestration

3. **Infrastructure Layer** (`VehicleTracking.Infrastructure`)
   - External service implementations
   - Geographical calculations (`IGeographicalService`)
   - Validation rules using Strategy Pattern
   - Cross-cutting concerns

4. **Persistence Layer** (`VehicleTracking.Persistence`)
   - Entity Framework 6 DbContext
   - Repository implementations
   - Database configuration and migrations

5. **Web Layer** (`VehicleTracking.Web`)
   - API Controllers
   - Autofac DI configuration
   - Request/Response models
   - Global exception handling

---

## Key Architectural Features

### 1. Dependency Injection (Autofac)
The entire API is decoupled using **Autofac** container:
- Controllers depend only on service interfaces
- Services are registered with appropriate lifetimes
- Easy unit testing through interface mocking

### 2. Structured Logging (Serilog)
Comprehensive logging with structured data:
- Request/response logging
- Validation failure details
- Performance metrics
- Error tracking with context

### 3. Multi-Layer Validation Strategy
Validation is implemented using the **Strategy Pattern** across multiple layers:

#### Domain Validation
- Coordinate range validation (lat: [-90, 90], lon: [-180, 180])
- Value object invariants

#### Application Validation
- Vehicle existence checks
- Active status verification
- Geographic bounds (Athens bounding box)

#### Infrastructure Validation
- Duplicate detection (VehicleId, RecordedAt)
- Database constraint enforcement

### 4. Exception Handling Strategy
Hierarchical exception handling:
- Domain exceptions for business rule violations
- Application exceptions for use case failures
- Infrastructure exceptions for external service issues
- Global exception filter for consistent API responses

---

## Complete API Reference

### Vehicle Endpoints

#### `GET /api/vehicles`
**Purpose**: List all vehicles with basic information and last known status.

**Response Format**:
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
  ],
  "message": null
}
```

**Use Cases**:
- Vehicle grid display
- Fleet status overview
- Administrative dashboards

---

#### `GET /api/vehicles/with-last-positions`
**Purpose**: Get all vehicles with their complete last known GPS position (optimized for map display).

**Response Format**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "VAN-001",
      "isActive": true,
      "lastPosition": {
        "latitude": 37.9838,
        "longitude": 23.7275,
        "recordedAt": "2026-01-06T14:30:00Z"
      }
    }
  ]
}
```

**Performance Notes**:
- Single query with LEFT JOIN for efficiency
- Optimized for real-time map updates

---

### GPS Position Endpoints

#### `POST /api/gps/position`
**Purpose**: Submit a single GPS position reading.

**Request Format**:
```json
{
  "vehicleId": 1,
  "latitude": 37.9838,
  "longitude": 23.7275,
  "recordedAt": "2026-01-06T14:30:00Z"
}
```

**Validation Rules**:
- Vehicle must exist and be active
- Coordinates within Athens bounds (37.9-38.1°N, 23.6-23.8°E)
- No duplicate (VehicleId, RecordedAt) combinations
- Valid coordinate ranges (lat: [-90, 90], lon: [-180, 180])

**Response Format**:
```json
{
  "success": true,
  "data": {
    "id": 12345,
    "message": "GPS position recorded successfully"
  }
}
```

**Error Responses**:
```json
{
  "success": false,
  "data": null,
  "message": "Vehicle VAN-001 is inactive and cannot accept new positions"
}
```

---

#### `POST /api/gps/positions/batch`
**Purpose**: Submit multiple GPS positions for a single vehicle (optimized for high-frequency data).

**Request Format**:
```json
{
  "vehicleId": 1,
  "positions": [
    {
      "latitude": 37.9838,
      "longitude": 23.7275,
      "recordedAt": "2026-01-06T14:30:00Z"
    },
    {
      "latitude": 37.9840,
      "longitude": 23.7277,
      "recordedAt": "2026-01-06T14:30:30Z"
    }
  ]
}
```

**Performance Benefits**:
- Reduced HTTP overhead
- Batch database operations
- Optimized for GPS simulators and IoT devices

**Response Format**:
```json
{
  "success": true,
  "data": {
    "processedCount": 2,
    "skippedCount": 0,
    "message": "Batch processed successfully"
  }
}
```

---

#### `GET /api/gps/vehicle/{vehicleId}/route?from={datetime}&to={datetime}`
**Purpose**: Get vehicle route with calculated distance and position data.

**Parameters**:
- `vehicleId`: Vehicle identifier
- `from`: Start datetime (ISO 8601 format)
- `to`: End datetime (ISO 8601 format)

**Response Format**:
```json
{
  "success": true,
  "data": {
    "vehicleId": 1,
    "vehicleName": "VAN-001",
    "positions": [
      {
        "latitude": 37.9838,
        "longitude": 23.7275,
        "recordedAt": "2026-01-06T14:30:00Z"
      }
    ],
    "totalDistanceMeters": 1234.56,
    "positionCount": 50,
    "timeRange": {
      "from": "2026-01-06T14:00:00Z",
      "to": "2026-01-06T15:00:00Z"
    }
  }
}
```

**Distance Calculation**:
- Uses Haversine formula for accuracy
- Calculated server-side for consistency
- Handles edge cases (single position, no positions)

---

#### `GET /api/gps/vehicle/{vehicleId}/last-position`
**Purpose**: Get only the most recent GPS position for a specific vehicle.

**Response Format**:
```json
{
  "success": true,
  "data": {
    "latitude": 37.9838,
    "longitude": 23.7275,
    "recordedAt": "2026-01-06T14:30:00Z"
  }
}
```

---

## Service Layer Architecture

### IGpsService
**Responsibilities**:
- GPS data ingestion and validation
- Position querying and filtering
- Batch processing optimization

**Key Methods**:
- `SubmitPositionAsync(GpsPositionDto)`
- `SubmitBatchPositionsAsync(BatchGpsPositionDto)`
- `GetVehicleRouteAsync(int vehicleId, DateTime from, DateTime to)`

### IVehicleService
**Responsibilities**:
- Vehicle management and querying
- Status tracking
- Last position retrieval

**Key Methods**:
- `GetAllVehiclesAsync()`
- `GetVehiclesWithLastPositionsAsync()`
- `GetVehicleByIdAsync(int id)`

### IRouteCalculationService
**Responsibilities**:
- Distance calculations using Haversine formula
- Route analysis and statistics
- Geographic computations

**Key Methods**:
- `CalculateRouteDistanceAsync(IEnumerable<GpsPosition>)`
- `CalculateDistanceBetweenPoints(Coordinates point1, Coordinates point2)`

### IGeographicalService
**Responsibilities**:
- Geographic utility functions
- Bounding box validation
- Coordinate system conversions

---

## Validation Strategy Details

### Strategy Pattern Implementation
Each validation rule is implemented as a separate strategy:

```csharp
public interface IGpsValidationRule
{
    Task<ValidationResult> ValidateAsync(GpsPositionDto position);
}
```

**Implemented Rules**:
- `VehicleExistsValidationRule`
- `VehicleActiveValidationRule`
- `GeographicBoundsValidationRule`
- `DuplicateDetectionValidationRule`

**Benefits**:
- Open/Closed Principle compliance
- Easy to add new validation rules
- Testable in isolation
- Configurable rule ordering

---

## Error Handling & Response Format

### Consistent Response Structure
All API responses follow a consistent format:

```json
{
  "success": boolean,
  "data": object | null,
  "message": string | null
}
```

### HTTP Status Codes
- `200 OK`: Successful operations
- `400 Bad Request`: Validation failures, malformed requests
- `404 Not Found`: Resource not found (vehicle, position)
- `409 Conflict`: Duplicate data, business rule violations
- `500 Internal Server Error`: Unexpected server errors

### Exception Mapping
- `ValidationException` → 400 Bad Request
- `VehicleNotFoundException` → 404 Not Found
- `DuplicatePositionException` → 409 Conflict
- `Exception` → 500 Internal Server Error

---

## Performance Considerations

### Database Optimization
- Composite indexes on (VehicleId, RecordedAt)
- Batch operations for multiple inserts
- Efficient JOIN queries for vehicle-position relationships

### Caching Strategy
- In-memory caching for vehicle lookup
- Last position caching for real-time updates
- Configurable cache expiration

### Async/Await Pattern
- All database operations are asynchronous using Entity Framework 6 async methods
- Non-blocking I/O for better scalability and responsiveness
- Proper Task-based asynchronous programming (TAP) implementation
- Controllers, services, and repositories all support async operations

---

## Security Considerations

### Input Validation
- All inputs validated at multiple layers
- SQL injection prevention through EF parameterization
- XSS protection through proper encoding

### Geographic Constraints
- Hardcoded Athens bounding box prevents data pollution
- Coordinate range validation prevents invalid GPS data

### Rate Limiting (Future Enhancement)
- API rate limiting per client
- Batch size limitations
- Throttling for high-frequency submissions

---

## Testing Strategy

### Unit Tests Coverage
- Domain entity validation (8+ tests)
- Service layer business logic (9+ tests)
- Validation rule strategies (6+ tests)
- Geographic calculations (5+ tests)

### Integration Tests (Recommended)
- End-to-end API testing
- Database integration testing
- Performance testing under load

---

## Future Enhancements

### Authentication & Authorization
- JWT token-based authentication
- Role-based access control (RBAC)
- API key management for IoT devices

### Advanced Features
- Real-time notifications (SignalR)
- Geofencing capabilities
- Predictive analytics
- Multi-tenant support

### Monitoring & Observability
- Application Performance Monitoring (APM)
- Health check endpoints
- Metrics collection and dashboards
- Distributed tracing

---

## Summary

The Backend API is designed with **Clean Architecture** principles, providing a robust, scalable, and maintainable foundation for vehicle tracking operations. The multi-layer validation, comprehensive error handling, and performance optimizations ensure reliable operation under various load conditions while maintaining code quality and testability.
