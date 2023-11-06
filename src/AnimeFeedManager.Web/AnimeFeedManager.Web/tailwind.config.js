/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,cshtml}", "../AnimeFeedManager.Web.Client/**/*.{razor,cshtml}"],
  theme: {
    extend: {},
  },
    plugins: [require("daisyui")],
}

