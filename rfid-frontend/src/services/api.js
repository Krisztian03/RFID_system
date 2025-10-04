import axios from 'axios';

const API_URL = 'http://localhost:5062/api';

// Axios instance JWT token-nel
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Automatikus token hozzáadása minden kéréshez
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Ha 401 (Unauthorized) jön, kijelentkeztetjük
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth API
export const authAPI = {
  login: (email, password) => 
    api.post('/auth/login', { email, password }),
  
  register: (data) => 
    api.post('/auth/register', data),
  
  getCurrentUser: () => 
    api.get('/auth/me'),
};

// Employee API
export const employeeAPI = {
  getAll: () => 
    api.get('/employee'),
  
  getById: (id) => 
    api.get(`/employee/${id}`),
  
  update: (id, data) => 
    api.put(`/employee/${id}`, data),
  
  delete: (id) => 
    api.delete(`/employee/${id}`),
};

// Access Logs API
export const accessAPI = {
  getEmployeeLogs: (employeeId, from, to) => 
    api.get(`/access/logs/${employeeId}`, {
      params: { from, to }
    }),
  
  getSummary: (employeeId, from, to) => 
    api.get(`/admin/summary/${employeeId}`, {
      params: { from, to }
    }),
};

// Admin API
export const adminAPI = {
  getAllEmployees: () => 
    api.get('/admin/employees'),
  
  getSummary: (employeeId, from, to) => 
    api.get(`/admin/summary/${employeeId}`, {
      params: { from, to }
    }),
};

export default api;