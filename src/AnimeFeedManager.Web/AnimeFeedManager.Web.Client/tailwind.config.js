/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,cshtml}", "../AnimeFeedManager.Web/**/*.{razor,cshtml}"],
  theme: {
    extend: {},
  },
    plugins: [require("daisyui")],
}

