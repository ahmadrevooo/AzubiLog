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
        }
    };

    applyTheme(window.azubiLogPreferences.getTheme());
})();
