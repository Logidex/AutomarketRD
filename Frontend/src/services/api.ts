import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5217',
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: false,
});

// Interceptor para agregar el token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para manejar errores y leer el mensaje del backend
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Si el backend respondió con un error (4xx, 5xx)
    if (error.response) {
      // Lee el mensaje del backend
      const backendMessage = error.response.data?.mensaje || error.response.data?.message;
      
      // Si hay un mensaje personalizado del backend, úsalo
      if (backendMessage) {
        error.message = backendMessage;
      }
    }
    
    return Promise.reject(error);
  }
);

export default api;