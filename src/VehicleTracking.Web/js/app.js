// Configuration
const CONFIG = {
    apiBaseUrl: 'http://localhost:5000/api',
    pollingInterval: 30000, // 30 seconds
};

// State
let map = null;
let routeMap = null;
let vehicleMarkers = {};
let pollingTimer = null;
let currentVehicleId = null;

// Initialize application
document.addEventListener('DOMContentLoaded', function() {
    initializeMap();
    loadVehicles();
    startPolling();
    setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
    document.getElementById('refreshBtn').addEventListener('click', function() {
        loadVehicles();
    });

    document.getElementById('modalClose').addEventListener('click', function() {
        closeModal();
    });

    document.getElementById('applyFilter').addEventListener('click', function() {
        applyDateFilter();
    });

    // Close modal on outside click
    document.getElementById('vehicleModal').addEventListener('click', function(e) {
        if (e.target === this) {
            closeModal();
        }
    });
}

// Initialize main map
function initializeMap() {
    map = L.map('map').setView([37.9838, 23.7275], 12); // Centered on Athens

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '� OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);
}

// Load vehicles with their last positions
async function loadVehicles() {
    try {
        updateStatus('loading');
        
        const response = await fetch(`${CONFIG.apiBaseUrl}/vehicles/with-last-positions`);
        const result = await response.json();

        if (result.success && result.data) {
            displayVehicles(result.data);
            updateMapMarkers(result.data);
            updateHeader(result.data.length);
            updateStatus('active');
        } else {
            console.error('Failed to load vehicles');
            updateStatus('inactive');
        }
    } catch (error) {
        console.error('Error loading vehicles:', error);
        document.getElementById('vehicleGrid').innerHTML = 
            '<div class="loading">Error loading vehicles. Please check API connection.</div>';
        updateStatus('inactive');
    }
}

// Display vehicles in grid
function displayVehicles(data) {
    const grid = document.getElementById('vehicleGrid');
    
    if (!data || data.length === 0) {
        grid.innerHTML = '<div class="loading">No vehicles found</div>';
        return;
    }

    grid.innerHTML = data.map(item => {
        const vehicle = item.vehicle;
        const lastPos = item.lastPosition;
        
        const lastPosText = lastPos 
            ? `${new Date(lastPos.recordedAt).toLocaleString()}<br>
               Lat: ${lastPos.latitude.toFixed(6)}, Lon: ${lastPos.longitude.toFixed(6)}`
            : 'No position data';

        return `
            <div class="vehicle-card" onclick="openVehicleDetails(${vehicle.id}, '${vehicle.name}')">
                <div class="vehicle-card-header">
                    <span class="vehicle-name">${vehicle.name}</span>
                    <span class="vehicle-status ${vehicle.isActive ? 'active' : 'inactive'}">
                        ${vehicle.isActive ? 'Active' : 'Inactive'}
                    </span>
                </div>
                <div class="vehicle-info">
                    <div><strong>Last Position:</strong></div>
                    <div>${lastPosText}</div>
                </div>
            </div>
        `;
    }).join('');
}

// Update map markers
function updateMapMarkers(data) {
    // Clear existing markers
    Object.values(vehicleMarkers).forEach(marker => map.removeLayer(marker));
    vehicleMarkers = {};

    // Add new markers
    data.forEach(item => {
        const vehicle = item.vehicle;
        const lastPos = item.lastPosition;

        if (lastPos) {
            const markerColor = vehicle.isActive ? 'green' : 'red';
            
            const marker = L.circleMarker([lastPos.latitude, lastPos.longitude], {
                radius: 8,
                fillColor: markerColor,
                color: '#fff',
                weight: 2,
                opacity: 1,
                fillOpacity: 0.8
            }).addTo(map);

            marker.bindPopup(`
                <strong>${vehicle.name}</strong><br>
                Status: ${vehicle.isActive ? 'Active' : 'Inactive'}<br>
                Last Update: ${new Date(lastPos.recordedAt).toLocaleString()}<br>
                <a href="#" onclick="openVehicleDetails(${vehicle.id}, '${vehicle.name}'); return false;">View Details</a>
            `);

            vehicleMarkers[vehicle.id] = marker;
        }
    });
}

// Update header info
function updateHeader(vehicleCount) {
    document.getElementById('vehicleCount').textContent = `${vehicleCount} vehicles`;
    document.getElementById('lastUpdate').textContent = 
        `Last update: ${new Date().toLocaleTimeString()}`;
}

// Update status indicator
function updateStatus(status) {
    const indicator = document.getElementById('statusIndicator');
    indicator.className = `status-indicator ${status}`;
}

// Start polling
function startPolling() {
    pollingTimer = setInterval(() => {
        loadVehicles();
    }, CONFIG.pollingInterval);
}

// Open vehicle details modal
function openVehicleDetails(vehicleId, vehicleName) {
    currentVehicleId = vehicleId;
    
    document.getElementById('modalVehicleName').textContent = vehicleName;
    document.getElementById('vehicleModal').classList.add('active');

    // Initialize route map if not already done
    if (!routeMap) {
        routeMap = L.map('routeMap').setView([37.9838, 23.7275], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '� OpenStreetMap contributors',
            maxZoom: 19
        }).addTo(routeMap);
    }

    // Set default date range (last 24 hours)
    const now = new Date();
    const yesterday = new Date(now.getTime() - 24 * 60 * 60 * 1000);
    
    document.getElementById('dateTo').value = formatDateTimeLocal(now);
    document.getElementById('dateFrom').value = formatDateTimeLocal(yesterday);

    // Load initial route
    loadVehicleRoute(vehicleId, yesterday.toISOString(), now.toISOString());
}

// Close modal
function closeModal() {
    document.getElementById('vehicleModal').classList.remove('active');
    currentVehicleId = null;
}

// Apply date filter
function applyDateFilter() {
    if (!currentVehicleId) return;

    const fromInput = document.getElementById('dateFrom').value;
    const toInput = document.getElementById('dateTo').value;

    if (!fromInput || !toInput) {
        alert('Please select both from and to dates');
        return;
    }

    const from = new Date(fromInput).toISOString();
    const to = new Date(toInput).toISOString();

    loadVehicleRoute(currentVehicleId, from, to);
}

// Load vehicle route
async function loadVehicleRoute(vehicleId, from, to) {
    try {
        showModalLoading(true);

        const url = `${CONFIG.apiBaseUrl}/gps/vehicle/${vehicleId}/route?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`;
        const response = await fetch(url);
        const result = await response.json();

        if (result.success && result.data) {
            displayRoute(result.data);
        } else {
            alert('Failed to load route data');
        }
    } catch (error) {
        console.error('Error loading route:', error);
        alert('Error loading route data');
    } finally {
        showModalLoading(false);
    }
}

// Display route on map
function displayRoute(routeData) {
    // Clear existing layers
    routeMap.eachLayer(layer => {
        if (layer instanceof L.Polyline || layer instanceof L.CircleMarker) {
            routeMap.removeLayer(layer);
        }
    });

    const positions = routeData.positions;

    if (!positions || positions.length === 0) {
        document.getElementById('totalDistance').textContent = '0 m';
        document.getElementById('dataPoints').textContent = '0';
        document.getElementById('timeRange').textContent = 'No data';
        alert('No position data found for selected time range');
        return;
    }

    // Update info cards
    document.getElementById('totalDistance').textContent = 
        formatDistance(routeData.totalDistanceMeters);
    document.getElementById('dataPoints').textContent = positions.length;
    
    const firstTime = new Date(positions[0].recordedAt);
    const lastTime = new Date(positions[positions.length - 1].recordedAt);
    document.getElementById('timeRange').textContent = 
        `${firstTime.toLocaleString()} - ${lastTime.toLocaleString()}`;

    // Draw route line
    const latLngs = positions.map(p => [p.latitude, p.longitude]);
    
    const polyline = L.polyline(latLngs, {
        color: '#667eea',
        weight: 3,
        opacity: 0.7
    }).addTo(routeMap);

    // Add start marker (green)
    L.circleMarker([positions[0].latitude, positions[0].longitude], {
        radius: 8,
        fillColor: '#10b981',
        color: '#fff',
        weight: 2,
        opacity: 1,
        fillOpacity: 0.9
    }).addTo(routeMap).bindPopup(`
        <strong>Start</strong><br>
        ${new Date(positions[0].recordedAt).toLocaleString()}
    `);

    // Add end marker (red)
    L.circleMarker([positions[positions.length - 1].latitude, positions[positions.length - 1].longitude], {
        radius: 8,
        fillColor: '#ef4444',
        color: '#fff',
        weight: 2,
        opacity: 1,
        fillOpacity: 0.9
    }).addTo(routeMap).bindPopup(`
        <strong>End</strong><br>
        ${new Date(positions[positions.length - 1].recordedAt).toLocaleString()}
    `);

    // Fit map to route bounds
    routeMap.fitBounds(polyline.getBounds(), { padding: [50, 50] });

    // Invalidate size to fix potential rendering issues
    setTimeout(() => routeMap.invalidateSize(), 100);
}

// Helper: Format distance
function formatDistance(meters) {
    if (meters >= 1000) {
        return `${(meters / 1000).toFixed(2)} km`;
    }
    return `${meters.toFixed(0)} m`;
}

// Helper: Format datetime for input
function formatDateTimeLocal(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

// Helper: Show/hide modal loading
function showModalLoading(show) {
    const loader = document.getElementById('modalLoading');
    if (show) {
        loader.classList.add('active');
    } else {
        loader.classList.remove('active');
    }
}

// Make openVehicleDetails available globally
window.openVehicleDetails = openVehicleDetails;
