# Frontend Design & Architecture

This section describes the design and responsibilities of the frontend application for the Vehicle Tracking & Route Visualization System.

The frontend is intentionally kept simple, clear, and functional, focusing on correct data visualization and interaction with the backend API.

---

## Overview

The frontend is a **Single Page Application (SPA)** implemented using:

- **HTML5 & CSS3**
- **Vanilla JavaScript** (ES6+)
- **Leaflet.js** for map visualization

Its primary responsibilities are:
- Displaying vehicles and their current status in a real-time grid.
- Visualizing GPS positions on an interactive map.
- Allowing route inspection and history tracking for individual vehicles.
- Periodically refreshing data via asynchronous polling.

---

## High-Level Architecture

```
+----------------------+
| Frontend SPA         |
|----------------------|
| - Vehicle Grid       |
| - Vehicle Detail     |
| - Map Visualization  |
+----------+-----------+
           |
           | HTTP (Polling & Queries)
           v
+----------------------+
| Backend API          |
+----------------------+
```

The frontend acts as a **pure presentation layer**, maintaining only ephemeral state needed for the current user session.

---

## Design Principles

✓ **Single Page Architecture**
- Smooth user experience without full page reloads.
- Updates happen in-place via DOM manipulation.

✓ **Stateless Communication**
- Each request to the API is independent.
- Authentication/Session management is out of scope for this demo.

✓ **Separation of Concerns**
- **Data Layer**: Fetch API for backend communication.
- **UI Layer**: Vanilla JS for rendering components.
- **Visualization Layer**: Leaflet.js for geographic rendering.

---

## Main Views & Components

### 1. Vehicle Grid View
The primary dashboard showing the entire fleet status.

#### Responsibilities
- Fetch and display the list of all vehicles.
- Show real-time status (Active/Inactive) using visual indicators.
- Display the last known GPS timestamp.
- Render markers for all vehicles on the map simultaneously.

#### Polling Strategy
The grid performs a **polling request every 30 seconds**.
1. **Fetch**: Calls `GET /api/vehicles/with-last-positions`.
2. **Update**: Refreshes the table rows and moves map markers to new coordinates.
3. **Transition**: Marker positions are updated smoothly using Leaflet's API.

---

### 2. Vehicle Detail & Route View
Displayed when a user selects a specific vehicle to view its history.

#### Responsibilities
- Filter positions by date and time range.
- Visualize the historical path as a polyline.
- Calculate and display the total distance travelled.

#### Interaction Flow
1. User selects a vehicle from the grid.
2. Frontend requests route data: `GET /api/gps/vehicle/{id}/route`.
3. API returns a list of coordinates and pre-calculated statistics.
4. Leaflet draws a **blue polyline** connecting the points chronologically.
5. Map bounds are automatically adjusted to fit the entire route.

---

## Map Integration with Leaflet

The geographic visualization is the core component of the UI. It is implemented in four distinct stages:

### 1. Initialization
- **Container**: The HTML contains a `<div id="map"></div>` with a fixed height.
- **Tiles**: We load **OpenStreetMap** tiles via the Leaflet API.
- **View**: The map is initialized with a view centered on Athens (e.g., zoom level 12).

### 2. Marker Creation & Persistence
To avoid UI flickering and memory leaks, we use a **persistence strategy** for markers:
- A global JavaScript object `markers` acts as a dictionary, using `vehicleId` as the key.
- For every vehicle received from the API:
    1. If a marker exists in the dictionary, we update its position using `marker.setLatLng()`.
    2. If no marker exists, we create a new one using `L.marker()`, add it to the map, and store its reference.

### 3. Popups & Interaction
Each marker is configured with a dynamic popup that provides immediate context:
- Vehicle Name and Current Status.
- Last seen timestamp (formatted for the user's locale).
- A shortcut link to "View Route" for that specific vehicle.

### 4. Route Visualization (Polylines)
When a specific route is requested:
- The system fetches a chronological list of coordinates.
- A `L.polyline` is generated with a distinct color (e.g., blue).
- **Auto-Fit**: We use `map.fitBounds(polyline.getBounds())` to ensure the entire route is immediately visible to the user without manual panning.

---

## API Interaction Reference

The frontend communicates with the backend exclusively via the following endpoints:

| Method | Endpoint | Purpose |
|:--- |:--- |:--- |
| **GET** | `/api/vehicles/with-last-positions` | Global update for grid and map markers. |
| **GET** | `/api/gps/vehicle/{id}/route` | Fetch historical polyline data and distance. |
| **GET** | `/api/gps/vehicle/{id}/last-position` | Immediate update for a single selected vehicle. |

---

## Distance Calculation

To ensure a **Single Source of Truth**, all geographic calculations (including total distance) are performed on the **Backend** within the `GeographicalService`.

The Frontend receives the `totalDistanceMeters` as part of the route response and formats it for the user:
- Values > 1000m are displayed in **kilometers** (e.g., 5.24 km).
- Smaller values are displayed in **meters** (e.g., 450 m).

While not currently used for calculation, the frontend documentation includes the **Haversine Formula** reference for potential client-side filtering features:

```
R = 6371 km (Earth's radius)
Δlat = lat2 - lat1
...
```

---

## Performance Considerations

- **Marker Persistence**: Markers are updated in-place rather than destroyed and recreated to avoid flickering and reduce memory pressure.
- **Lightweight Payloads**: The frontend requests specific DTOs from the Application Layer, ensuring only necessary data is transferred over the network.
- **Efficient Rendering**: Uses CSS Flexbox and Grid for responsive layouts without heavy framework dependencies.

---

## Summary

The frontend is designed to be **simple but robust**. By leveraging Leaflet.js and modern Vanilla JS, it provides a high-performance visualization tool that scales well for fleets of 50-200 vehicles, keeping complexity low and maintainability high.
