const baseUrl = import.meta.env.VITE_API_BASE_URL;

if (!baseUrl) {
  throw new Error("VITE_API_BASE_URL is not set");
}

export const env = {
  apiBaseUrl: String(baseUrl).replace(/\/$/, ""),
  appTitle: String(import.meta.env.VITE_APP_TITLE || "SAUMI"),
};
