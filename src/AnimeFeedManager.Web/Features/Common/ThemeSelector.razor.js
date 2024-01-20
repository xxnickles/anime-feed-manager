const availableThemes = {
    'dark': 'dim',
    'light': 'emerald'
}

function getOppositeTheme(theme) {
    return theme === availableThemes['light'] ? availableThemes['dark'] : availableThemes['light'];
}

function themeDetector() {
    return {
        selectedTheme: availableThemes['light'],
        targetTheme: availableThemes['dark'],
        isDark: false,
        themeChanged(event) {
            this.selectedTheme = getOppositeTheme(this.selectedTheme);
            console.log("new theme", this.selectedTheme)
            localStorage.setItem('user-theme', this.selectedTheme);
        },
        init() {
            const matchMediaDark = window.matchMedia('(prefers-color-scheme: dark)');
            const matchMediaLight = window.matchMedia('(prefers-color-scheme: light)');

            if (!localStorage.getItem('user-theme')) {
                // set theme based on initial state
                this.targetTheme = matchMediaDark.matches ? availableThemes['light'] : availableThemes['dark'];
                this.isDark = matchMediaDark.matches;
                this.selectedTheme = getOppositeTheme(this.targetTheme);
            } else {
                this.selectedTheme = localStorage.getItem('user-theme');
                this.targetTheme = getOppositeTheme(this.selectedTheme);
                this.isDark = this.targetTheme === availableThemes['dark'];
                document.querySelector('html').setAttribute('data-theme', this.selectedTheme);
            }


            matchMediaDark.addEventListener('change', (e) => {

                if (e.matches) {
                    this.targetTheme = availableThemes['light'];
                    this.isDark = true;

                }
            });

            matchMediaLight.addEventListener('change', (e) => {
                if (e.matches) {
                    this.targetTheme = availableThemes['dark'];
                    this.isDark = false;
                }
            });
        }
    };
}