/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,cshtml}", "../AnimeFeedManager.Web.Client/**/*.{razor,cshtml}"],
    daisyui: {
        themes: ["emerald", "dim"],
        darkTheme: "dim",
        
    },
    plugins: [
        require('@tailwindcss/typography'),
        require("daisyui"),
    ],
}

