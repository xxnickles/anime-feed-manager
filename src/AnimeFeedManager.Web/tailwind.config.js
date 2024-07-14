/** @type {import('tailwindcss').Config} */

const defaultTheme = require('tailwindcss/defaultTheme')
module.exports = {
    content: ["./**/*.{razor,cshtml}", "../AnimeFeedManager.Web.BlazorComponents/**/*.{razor,cshtml}"],
    theme:{
      extend: {
          fontFamily: {
              'sans': ['"SF Mono"', ...defaultTheme.fontFamily.sans],
              'logo' : ['"Prettier Script"', 'sans-serif']
          }
      }  
    },
    daisyui: {
        themes: ["nord", "dim"],
        darkTheme: "dim"        
    },
    plugins: [
        require('@tailwindcss/typography'),
        require("daisyui"),
    ],
    safelist: [
        'stroke-success',
        'stroke-error',
        'stroke-warning',
        'stroke-info'
    ]
}

