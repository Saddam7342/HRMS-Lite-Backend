# HRMS Web (Vite + React)

## Development

1. Start the API (default `http://localhost:5121` per `launchSettings.json`).
2. From this folder:

```bash
npm install
npm run dev
```

Open `http://localhost:5173`. The Vite dev server **proxies** `/api` to the backend, so you avoid CORS during local development.

## Production build

Set `VITE_API_URL` to your API origin (e.g. `https://api.yourcompany.com`) if the UI is hosted on another domain. Then:

```bash
npm run build
```

Serve the `dist/` folder from any static host. If the UI and API share the same origin, leave `VITE_API_URL` unset and route `/api` to the backend via your reverse proxy.

## Stack

- React 19 + TypeScript + Vite 8  
- Tailwind CSS v4  
- React Router  
- `fetch` client with JWT + refresh for all `/api/v1/*` routes  
