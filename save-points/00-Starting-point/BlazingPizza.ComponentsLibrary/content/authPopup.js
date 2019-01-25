(function () {
    var userInfoComponent;

    window.openLoginPopup = function (component) {
        if (userInfoComponent) {
            userInfoComponent.dispose();
        }

        userInfoComponent = component;
        var popup = window.open('user/signin?returnUrl=' + encodeURIComponent(location.href), 'loginWindow', 'width=600,height=600');

        // Poll to see if it's closed before completion
        var intervalHandle = setInterval(function () {
            if (popup.closed) {
                clearInterval(intervalHandle);
                onLoginPopupFinished({ isLoggedIn: false });
            }
        }, 250);
    };

    window.onLoginPopupFinished = function (userState) {
        if (userInfoComponent) {
            userInfoComponent.invokeMethod('OnSignInStateChanged', userState);
            userInfoComponent.dispose();
            userInfoComponent = null;
        }
    };
})();