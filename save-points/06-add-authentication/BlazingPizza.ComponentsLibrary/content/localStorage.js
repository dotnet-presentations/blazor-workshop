(function () {
    window.blazorLocalStorage = {
        get: key => key in localStorage ? JSON.parse(localStorage[key]) : null,
        set: (key, value) => { localStorage[key] = JSON.stringify(value); },
        delete: key => { delete localStorage[key]; }
    };
})();
