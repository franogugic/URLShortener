import { request } from "../lib/http";

export async function registerUser({ username, password }) {
  return request("/api/user/register", {
    method: "POST",
    body: JSON.stringify({ Username: username, Password: password }),
  });
}

export async function loginUser({ username, password }) {
  return request("/api/user/login", {
    method: "POST",
    body: JSON.stringify({ Username: username, Password: password }),
  });
}

export async function getMe() {
  return request("/api/user/me", { method: "GET" });
}

export async function logoutUser() {
  return request("/api/user/logout", { method: "POST" });
}
