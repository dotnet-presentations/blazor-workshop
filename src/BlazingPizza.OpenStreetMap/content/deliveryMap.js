var tileUrl = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
var tileAttribution ='Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>';

// Global export
deliveryMap = {
    maps: {},
    initializeCore: function (id) {
        var map = deliveryMap.maps[id];
        if (map === undefined) {
            map = L.map(id);
            deliveryMap.maps[id] = map;
            L.tileLayer(tileUrl, { attribution: tileAttribution }).addTo(map);

            map.addedMarkers = [];
        }

        return map;
    },
    initialize: function (id) {
        deliveryMap.initializeCore(id);
    },
    setView: function (id, center, zoom) {
        var map = deliveryMap.initializeCore(id);

        map.setView([center.x, center.y], zoom);
    },
    setMarkers: function (id, markers) {
        var map = deliveryMap.initializeCore(id);

        if (map.addedMarkers.length !== markers.length) {
            // Markers have changed, so reset
            map.addedMarkers.forEach(marker => marker.removeFrom(map));
            map.addedMarkers = markers.map(m => {
                var marker = L.marker([m.y, m.x]).addTo(map);
                marker.bindPopup(m.description).openPopup();
                return marker;
            });

            var markersGroup = new L.featureGroup(map.addedMarkers);
            map.fitBounds(markersGroup.getBounds().pad(0.2));
        } else {
            // Same number of markers, so update
            for (var i = 0; i < markers.length; i++) {
                map.addedMarkers[i].setLatLng([markers[i].y, markers[i].x]);
            }
        }
    }
};
