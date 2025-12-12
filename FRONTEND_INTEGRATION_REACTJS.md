# Frontend Integration Guide - Login with Email (ReactJS)

## Overview
API documentation for integrating login functionality with ReactJS frontend application.

## Base URL
```
http://localhost:5148
```

## Authentication Flow

```
???????????????
?   User      ?
???????????????
       ? 1. Enter email + password
       ?
???????????????????
?  Login Form     ?
???????????????????
       ? 2. POST /api/auth/login
  ?
???????????????????????????????????
? Backend API   ????????   Database   ?
???????????????????      ????????????????
       ? 3. Return JWT tokens
       ?
???????????????????
?  Store tokens   ?
?  in localStorage?
???????????????????
       ? 4. Navigate to dashboard
?
???????????????????
?  Protected Page ?
???????????????????
```

---

## API Endpoint

### Login
**Endpoint:** `POST /api/auth/login`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```typescript
{
  email: string;      // Required, valid email format
  password: string;   // Required, min 6 characters
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-12-09T10:30:00Z",
  "message": "Login successful"
}
```

**Error Responses:**

1. **Invalid Credentials (401 Unauthorized):**
```json
{
  "success": false,
  "token": null,
  "refreshToken": null,
  "expiresAt": null,
  "message": "Invalid email or password"
}
```

2. **Invalid Email Format (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "Invalid email format"
    ]
  }
}
```

3. **Missing Required Fields (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "Email is required"
    ],
    "Password": [
      "Password is required"
    ]
  }
}
```

---

## TypeScript Types

```typescript
// Request Types
export interface LoginRequest {
  email: string;
  password: string;
}

// Response Types
export interface LoginResponse {
  success: boolean;
  token: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  message: string;
}

export interface ValidationError {
  type: string;
  title: string;
  status: number;
  errors: {
    [key: string]: string[];
  };
}

// User Types (decoded from JWT)
export interface User {
  sub: string;      // User ID
  role: string;          // "Admin" or "User"
  exp: number;          // Token expiration (Unix timestamp)
  iss: string;     // Issuer: "DemoGeoServer"
  aud: string;          // Audience: "DemoGeoServerUsers"
}
```

---

## React Implementation

### 1. API Service (axios)

```typescript
// src/services/auth.service.ts
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5148/api';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  token: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  message: string;
}

class AuthService {
  async login(email: string, password: string): Promise<LoginResponse> {
    try {
  const response = await axios.post<LoginResponse>(
     `${API_BASE_URL}/auth/login`,
        { email, password }
   );
      
  if (response.data.success && response.data.token) {
        // Store tokens in localStorage
        localStorage.setItem('accessToken', response.data.token);
        localStorage.setItem('refreshToken', response.data.refreshToken || '');
        localStorage.setItem('tokenExpiry', response.data.expiresAt || '');
      }
    
      return response.data;
    } catch (error: any) {
    if (error.response?.data) {
        return error.response.data;
}
      throw error;
    }
  }

  logout(): void {
    localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiry');
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
}

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
 const expiry = localStorage.getItem('tokenExpiry');
    
    if (!token || !expiry) return false;
    
    return new Date(expiry) > new Date();
  }

  decodeToken(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
    .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
       .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      return null;
    }
  }

  getCurrentUser(): any {
  const token = this.getAccessToken();
    return token ? this.decodeToken(token) : null;
  }

  getUserRole(): string | null {
    const user = this.getCurrentUser();
    return user?.role || null;
  }

  isAdmin(): boolean {
  return this.getUserRole() === 'Admin';
}
}

export default new AuthService();
```

---

### 2. Login Component (with Formik & Yup)

```typescript
// src/components/Login.tsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import authService from '../services/auth.service';

// Validation Schema
const LoginSchema = Yup.object().shape({
  email: Yup.string()
    .email('Invalid email format')
    .required('Email is required'),
  password: Yup.string()
  .min(6, 'Password must be at least 6 characters')
    .required('Password is required'),
});

interface LoginFormValues {
  email: string;
  password: string;
}

const Login: React.FC = () => {
  const navigate = useNavigate();
  const [error, setError] = useState<string>('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (values: LoginFormValues) => {
    setLoading(true);
    setError('');

    try {
  const response = await authService.login(values.email, values.password);

      if (response.success) {
  // Login successful
   navigate('/dashboard');
      } else {
    // Login failed
setError(response.message || 'Login failed');
      }
    } catch (err: any) {
      setError('An error occurred. Please try again.');
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <h2>Login</h2>
      
   {error && <div className="alert alert-error">{error}</div>}

      <Formik
        initialValues={{ email: '', password: '' }}
        validationSchema={LoginSchema}
      onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
      <Form>
        <div className="form-group">
   <label htmlFor="email">Email</label>
      <Field
                type="email"
   name="email"
  id="email"
                className="form-control"
         placeholder="admin@demo.com"
   />
            <ErrorMessage name="email" component="div" className="error-text" />
        </div>

        <div className="form-group">
  <label htmlFor="password">Password</label>
    <Field
           type="password"
   name="password"
                id="password"
     className="form-control"
           placeholder="Enter your password"
      />
  <ErrorMessage name="password" component="div" className="error-text" />
            </div>

            <button
        type="submit"
        disabled={isSubmitting || loading}
   className="btn btn-primary"
            >
      {loading ? 'Logging in...' : 'Login'}
  </button>
          </Form>
        )}
      </Formik>
    </div>
);
};

export default Login;
```

---

### 3. Simple Login Component (without libraries)

```typescript
// src/components/SimpleLogin.tsx
import React, { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/auth.service';

const SimpleLogin: React.FC = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const validateEmail = (email: string): boolean => {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    // Client-side validation
    if (!email || !password) {
      setError('Email and password are required');
      return;
    }

    if (!validateEmail(email)) {
      setError('Invalid email format');
  return;
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters');
   return;
    }

    setLoading(true);

try {
  const response = await authService.login(email, password);

      if (response.success) {
        navigate('/dashboard');
      } else {
        setError(response.message || 'Login failed');
      }
    } catch (err) {
      setError('An error occurred. Please try again.');
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <h2>Login</h2>
   
      {error && (
        <div className="alert alert-error" role="alert">
          {error}
   </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
     <label htmlFor="email">Email</label>
          <input
  type="email"
            id="email"
         value={email}
  onChange={(e) => setEmail(e.target.value)}
            placeholder="admin@demo.com"
            className="form-control"
            disabled={loading}
          />
</div>

        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input
            type="password"
    id="password"
            value={password}
   onChange={(e) => setPassword(e.target.value)}
          placeholder="Enter your password"
    className="form-control"
            disabled={loading}
          />
        </div>

     <button type="submit" disabled={loading} className="btn btn-primary">
  {loading ? 'Logging in...' : 'Login'}
      </button>
      </form>
    </div>
  );
};

export default SimpleLogin;
```

---

### 4. Protected Route Component

```typescript
// src/components/ProtectedRoute.tsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import authService from '../services/auth.service';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAdmin?: boolean;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ 
  children, 
  requireAdmin = false 
}) => {
  const isAuthenticated = authService.isAuthenticated();
  const isAdmin = authService.isAdmin();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
}

  if (requireAdmin && !isAdmin) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
```

---

### 5. Axios Interceptor (Auto-attach JWT)

```typescript
// src/utils/axios.config.ts
import axios from 'axios';
import authService from '../services/auth.service';

const api = axios.create({
  baseURL: 'http://localhost:5148/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - attach JWT token
api.interceptors.request.use(
  (config) => {
    const token = authService.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
(error) => Promise.reject(error)
);

// Response interceptor - handle 401 errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      authService.logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

---

### 6. App Router Setup

```typescript
// src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Login from './components/Login';
import Dashboard from './pages/Dashboard';
import AdminPanel from './pages/AdminPanel';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
  <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
          <Dashboard />
            </ProtectedRoute>
     }
 />
        
<Route
          path="/admin"
          element={
            <ProtectedRoute requireAdmin>
  <AdminPanel />
      </ProtectedRoute>
 }
        />
        
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
```

---

## CSS Styling (Optional)

```css
/* src/styles/Login.css */
.login-container {
  max-width: 400px;
  margin: 100px auto;
  padding: 30px;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.login-container h2 {
  text-align: center;
  margin-bottom: 30px;
  color: #333;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 600;
  color: #555;
}

.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.form-control:focus {
  outline: none;
  border-color: #4CAF50;
}

.btn {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-primary {
  background-color: #4CAF50;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #45a049;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.alert {
  padding: 12px;
  margin-bottom: 20px;
  border-radius: 4px;
}

.alert-error {
  background-color: #f44336;
  color: white;
}

.error-text {
  color: #f44336;
font-size: 12px;
  margin-top: 5px;
}
```

---

## Installation & Setup

### 1. Install Dependencies

```bash
npm install axios react-router-dom
npm install --save-dev @types/react-router-dom

# Optional (for form validation)
npm install formik yup
npm install --save-dev @types/yup
```

### 2. Environment Variables

```bash
# .env
REACT_APP_API_BASE_URL=http://localhost:5148/api
```

### 3. Update API Base URL

```typescript
// src/services/auth.service.ts
const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:5148/api';
```

---

## Testing Examples

### Test Credentials

**Admin User:**
```json
{
  "email": "admin@demo.com",
"password": "password123"
}
```

**Regular User:**
```json
{
  "email": "testuser@demo.com",
  "password": "password123"
}
```

### Test Scenarios

```typescript
// 1. Successful login
authService.login('admin@demo.com', 'password123')
  .then(response => console.log(response));

// 2. Invalid email format
authService.login('not-an-email', 'password123')
  .then(response => console.log(response));

// 3. Wrong password
authService.login('admin@demo.com', 'wrongpassword')
  .then(response => console.log(response));

// 4. Non-existent email
authService.login('notexist@demo.com', 'password123')
  .then(response => console.log(response));
```

---

## Security Best Practices

1. ? **Never log sensitive data** (passwords, tokens)
2. ? **Use HTTPS in production**
3. ? **Store tokens in httpOnly cookies** (more secure than localStorage)
4. ? **Implement token refresh** mechanism
5. ? **Add CORS configuration** on backend
6. ? **Validate on both client and server side**
7. ? **Implement rate limiting** on login endpoint
8. ? **Add CSRF protection** for production

---

## Troubleshooting

### CORS Error
```typescript
// Backend (Program.cs) - Add CORS policy
builder.Services.AddCors(options =>
{
 options.AddPolicy("AllowReactApp",
        builder => builder
        .WithOrigins("http://localhost:3000")
          .AllowAnyMethod()
     .AllowAnyHeader());
});

app.UseCors("AllowReactApp");
```

### Token Expiry Handling
```typescript
// Check token expiry before each request
api.interceptors.request.use(async (config) => {
  const expiry = localStorage.getItem('tokenExpiry');
  
  if (expiry && new Date(expiry) <= new Date()) {
    // Token expired, refresh or logout
    await authService.refreshToken();
    // Or logout: authService.logout();
  }
  
  return config;
});
```

---

## Complete Example Project Structure

```
src/
??? components/
?   ??? Login.tsx
?   ??? ProtectedRoute.tsx
?   ??? Navbar.tsx
??? pages/
?   ??? Dashboard.tsx
?   ??? AdminPanel.tsx
??? services/
?   ??? auth.service.ts
?   ??? api.service.ts
??? utils/
?   ??? axios.config.ts
??? types/
?   ??? auth.types.ts
??? styles/
?   ??? Login.css
??? App.tsx
??? index.tsx
```

---

## Support

For issues or questions:
- Backend API: http://localhost:5148/swagger
- Test file: `DemoGeoServer/test-login-email.http`
- Documentation: `LOGIN_WITH_EMAIL.md`
