(function () {
    const themeKey = "azubilog-theme";

    function applyTheme(theme) {
        const normalizedTheme = theme === "dark" ? "dark" : "light";
        document.documentElement.dataset.theme = normalizedTheme;
        document.documentElement.dataset.bsTheme = normalizedTheme;
    }

    window.azubiLogPreferences = {
        getTheme() {
            return localStorage.getItem(themeKey) || "light";
        },
        setTheme(theme) {
            const normalizedTheme = theme === "dark" ? "dark" : "light";
            localStorage.setItem(themeKey, normalizedTheme);
            applyTheme(normalizedTheme);
        },
        setCulture(culture) {
            const normalizedCulture = culture === "en-US" ? "en-US" : "de-DE";
            const cookieValue = `c=${normalizedCulture}|uic=${normalizedCulture}`;
            document.cookie = `AzubiLog.Culture=${normalizedCulture}; path=/; max-age=31536000; samesite=lax`;
            document.cookie = `.AspNetCore.Culture=${cookieValue}; path=/; max-age=31536000; samesite=lax`;
        }
    };

    applyTheme(window.azubiLogPreferences.getTheme());
})();
