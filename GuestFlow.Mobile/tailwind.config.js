/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./App.{js,jsx,ts,tsx}", "./src/**/*.{js,jsx,ts,tsx}"],
  theme: {
    extend: {
      colors: {
        primary: "#1976d2",
        secondary: "#9c27b0",
        background: "#f8f9fa",
      }
    },
  },
  plugins: [],
}
