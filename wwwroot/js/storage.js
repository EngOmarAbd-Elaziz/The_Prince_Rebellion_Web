// LocalStorage Interop Helper for The Prince Rebellion

window.storageHelper = {
    setItem: function (key, value) {
        try {
            localStorage.setItem(key, value);
            return true;
        } catch (e) {
            console.error("Error setting item in localStorage", e);
            return false;
        }
    },
    getItem: function (key) {
        try {
            return localStorage.getItem(key);
        } catch (e) {
            console.error("Error getting item from localStorage", e);
            return null;
        }
    },
    removeItem: function (key) {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (e) {
            return false;
        }
    },
    toggleFullscreen: function () {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen().catch(err => {
                console.warn(`Error trying to enable fullscreen: ${err.message}`);
            });
        } else {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            }
        }
    },
    scrollToElement: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.scrollIntoView({ behavior: 'smooth' });
        }
    }
};
