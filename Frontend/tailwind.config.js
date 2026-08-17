/** @type {import('tailwindcss').Config} */
export default {
  darkMode: "class",
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        page: "var(--am-page)",
        surface: "var(--am-surface)",
        "surface-2": "var(--am-surface-2)",
        line: "var(--am-line)",
        ink: "var(--am-ink)",
        "ink-2": "var(--am-ink-2)",
        "ink-3": "var(--am-ink-3)",
        input: "var(--am-input)",
        hover: "var(--am-hover)",
        // Nuevos tokens de marca y estado (se adaptan a claro/oscuro vía index.css)
        brand: "var(--am-brand)",
        "brand-hover": "var(--am-brand-hover)",
        "brand-soft": "var(--am-brand-soft)",
        success: "var(--am-success)",
        "success-soft": "var(--am-success-soft)",
      },
    },
  },
  plugins: [],
}