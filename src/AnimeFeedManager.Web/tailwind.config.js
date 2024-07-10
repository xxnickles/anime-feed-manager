/** @type {import('tailwindcss').Config} */

const defaultTheme = require('tailwindcss/defaultTheme')
module.exports = {
    content: ["./**/*.{razor,cshtml}", "../AnimeFeedManager.Web.BlazorComponents/**/*.{razor,cshtml}"],
    theme:{
      extend: {
          fontFamily: {
              'sans': ['"PT Sans"', ...defaultTheme.fontFamily.sans],
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

