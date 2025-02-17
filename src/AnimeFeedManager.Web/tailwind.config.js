/** @type {import('tailwindcss').Config} */

module.exports = {
    content: ["./**/*.{razor,cshtml}", "../AnimeFeedManager.Web.BlazorComponents/**/*.{razor,cshtml}"],   
    safelist: [
        'stroke-success',
        'stroke-error',
        'stroke-warning',
        'stroke-info'
    ]
}

