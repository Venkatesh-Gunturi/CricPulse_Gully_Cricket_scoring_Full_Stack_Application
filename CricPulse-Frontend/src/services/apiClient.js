import axios from 'axios';

const apiClient = axios.create({
  baseURL:
  import.meta.env.VITE_API_BASE_URL ||
  'https://localhost:7066/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((request) => {
  if (!/^\/AccountProfile\/[0-9a-f-]+$/i.test(request.url ?? '')) {
    return request;
  }

  const storedUser =
    localStorage.getItem('user') ?? sessionStorage.getItem('user');
  const accessToken = storedUser ? JSON.parse(storedUser)?.accessToken : null;

  if (accessToken) {
    request.headers.set('Authorization', `Bearer ${accessToken}`);
  }

  return request;
});

export default apiClient;