const API_BASE_URL = "/api";

export async function httpGet<T>(url: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`);

  if (!response.ok) {
    throw new Error("API request failed");
  }

  return response.json();
}
