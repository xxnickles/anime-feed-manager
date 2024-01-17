function themeDetector() {
    return {
        theme: 'dim',
        isDark: false,
        init() {
            const matchMediaDark = window.matchMedia('(prefers-color-scheme: dark)');
            const matchMediaLight = window.matchMedia('(prefers-color-scheme: light)');

            // set theme based on initial state
            this.theme = matchMediaDark.matches ? 'emerald' : 'dim';
            this.isDark = matchMediaDark.matches;

            matchMediaDark.addEventListener('change',(e) => {
                if (e.matches) {
                    this.theme = 'emerald';
                    this.isDark = true;
                }
            });

            matchMediaLight.addEventListener('change',(e) => {
                if (e.matches) {
                    this.theme = 'dim';
                    this.isDark = false;
                }
            });
        },
    };
}