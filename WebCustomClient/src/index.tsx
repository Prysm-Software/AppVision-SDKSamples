/**
 * Application entry point.
 *
 * Mounts the React root into the `#root` element of `public/index.html`, the page
 * that the web client loads in its iframe.
 * {@link React.StrictMode} is kept enabled to surface potential issues
 * during development (double-invokes effects in dev mode).
 */
import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
