import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import { LoadingProvider } from './context/LoadingContext'
import { ComparadorProvider } from './context/ComparadorContext'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <LoadingProvider>
        <ComparadorProvider>
          <App />
        </ComparadorProvider>
      </LoadingProvider>
    </BrowserRouter>
  </React.StrictMode>,
)