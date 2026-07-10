import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    host: '0.0.0.0',
      proxy: {
        '/api': {
          target: 'http://backend-dev:8080',   // ← 容器名稱:容器內部埠
          changeOrigin: true,
          secure: false,
        },
      "/hubs": {
        target: "https://localhost:7124",
        changeOrigin: true,
        secure: false,
        ws: true, // ← WebSocket 支援
      },
    },
  },
});
