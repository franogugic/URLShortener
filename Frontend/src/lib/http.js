import { API_BASE_URL } from "./env";

export class ApiError extends Error {
  constructor(message, status, payload) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

function toApiError(status, payload) {
  const payloadMessage = payload?.message || payload?.error || payload?.title;
  const fallback = status === 429 ? "Too many requests. Please try again shortly." : "Request failed.";
  return new ApiError(payloadMessage || fallback, status, payload);
}

export async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {}),
    },
    ...options,
  });

  const contentType = response.headers.get("content-type") || "";
  const isJson = contentType.includes("application/json");
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const normalizedPayload = typeof payload === "string" ? { message: payload } : payload;
    throw toApiError(response.status, normalizedPayload);
  }

  return payload;
}
