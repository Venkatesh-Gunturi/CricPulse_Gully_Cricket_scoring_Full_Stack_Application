import apiClient from './apiClient';

export function resolveProfileImageUrl(url) {
  if (url?.startsWith('/api/AccountProfile/photos/')) {
    const apiUrl = new URL(apiClient.defaults.baseURL, window.location.origin);
    return new URL(url, apiUrl).href;
  }

  return url;
}