# High Performance GPS Simulator

## Overview

The GPS Simulator is a standalone .NET Console Application designed to mimic the behavior of a fleet of IoT devices. It focuses on high performance, realistic movement patterns, and robust error handling.

## Key Architectural Patterns

### 1. Orchestration vs. Logic (IPositionSimulator)
The simulator logic is decoupled from the data submission flow.
- **`GpsDataGenerator`**: Acts as the orchestrator. It fetches vehicle states, coordinates the simulation tasks, and aggregates results.
- **`AthensPositionSimulator`**: Contains the pure logic for path generation, boundary enforcement, and speed calculation.

### 2. Parallel Processing (Concurrency)
To ensure scalability, the simulator processes vehicles in parallel using the **Task-based Asynchronous Pattern (TAP)**.
```csharp
var tasks = vehicles.Select(v => ProcessSingleVehicleAsync(v));
var results = await Task.WhenAll(tasks);
```
This allows the system to handle hundreds of vehicles simultaneously without sequential blocking, maximizing throughput.

### 3. Resilience & Error Isolation
The system is designed to be robust against partial failures:
- **Isolated Tasks**: Each vehicle is processed in its own asynchronous task. If one vehicle fails (e.g., due to invalid data or a specific API error), it **does not stop** the simulation for the remaining vehicles.
- **Detailed Diagnostics**: Every failure is captured with a full StackTrace and contextual data (Vehicle ID) using Serilog, ensuring that issues can be debugged without interrupting the service.

### 4. Dependency Injection (Autofac)
Every component is injected via interfaces, making the system highly modular and testable.
- `IVehicleApiClient`: Handles HTTP communication.
- `IPositionSimulator`: Handles path calculation.
- `IBoundingBoxProvider`: Defines geographic limits.
- `ILogger`: Provides structured telemetry.

## Movement Logic

The simulator uses a "Random Walk within Bounds" algorithm:
1. **Starting Point**: If a vehicle has a last known position, it starts from there. Otherwise, it uses a default starting point (Athens city center).
2. **Path Generation**: Generates `N` positions per iteration.
3. **Speed Calculation**: Realistic distance-based speed calculation (km/h) based on the time interval between readings.
4. **Boundary Enforcement**: If a vehicle exceeds the Athens Bounding Box, it is automatically redirected towards the center to prevent "lost" vehicles.

## Structured Logging (Serilog)

Instead of plain text, the simulator records data in a structured format:
```csharp
_logger.Information("Iteration #{Iteration} completed. Vehicles: {Processed}", iteration, count);
```
This enables real-time monitoring and advanced filtering based on properties like `VehicleId` or `ProcessedCount`.

## Configuration

Settings are managed via `App.config`:
- `ApiBaseUrl`: Target Web API address.
- `IntervalSeconds`: Delay between simulation bursts.
- `PositionsPerVehicle`: Number of GPS points generated per burst.
- `RadiusMeters`: Maximum movement distance per point.
