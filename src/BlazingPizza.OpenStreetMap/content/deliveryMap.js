var tileUrl = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
var tileAttribution ='Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>';

// Global export
deliveryMap = {
    maps: { },
    initializeCore: function (id) {
        var map = deliveryMap.maps[id];
        if (map === undefined) {
            map = L.map(id);
            deliveryMap.maps[id] = map;
            L.tileLayer(tileUrl, {attribution: tileAttribution}).addTo(map);
        }

        return map;
    },
    initialize: function(id) {
        deliveryMap.initializeCore(id);
    },
    setView: function(id, center, zoom) {
        var map = deliveryMap.initializeCore(id);

        map.setView([center.x, center.y], zoom);
    },
    setMarkers: function(id, markers) {
        var map = deliveryMap.initializeCore(id);

        markers.forEach(function(m) {
            var marker = L.marker([m.x, m.y]).addTo(map);
            marker.bindPopup(m.description).openPopup();
        });
    },
    setDriverMarker: function(id, driver) {
        var map = deliveryMap.initializeCore(id);

        if (deliveryMap.driver === undefined) {
            var marker = L.marker([driver.x, driver.y]).addTo(map);
            marker.bindPopup(driver.description).openPopup();
            deliveryMap.driver = marker;
        } else {
            deliveryMap.driver.setLatLng([driver.x, driver.y]);
        }
    }
}