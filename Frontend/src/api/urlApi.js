import { request } from "../lib/http";
import { API_BASE_URL } from "../lib/env";

export async function createUrl({ shortUrlCode, longUrl, description }) {
  return request("/url/create", {
    method: "POST",
    body: JSON.stringify({
      ShortUrlCode: shortUrlCode,
      LongUrl: longUrl,
      Description: description,
    }),
  });
}

export async function getUserUrls() {
  return request("/url/getAllUrlsByUserId", { method: "GET" });
}

export async function getUrlById(id) {
  return request(`/url/getUrlById/${id}`, { method: "GET" });
}

export async function deleteUrl(id) {
  return request(`/url/delete/${id}`, { method: "DELETE" });
}

export function buildRedirectUrl(shortCode) {
  return `${API_BASE_URL}/url/${encodeURIComponent(shortCode)}`;
}
