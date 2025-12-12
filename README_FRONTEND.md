# ?? Frontend Integration Documentation

## Overview
Complete documentation for integrating DemoGeoServer Authentication API with ReactJS frontend.

---

## ?? Documentation Files

| File | Description | Use Case |
|------|-------------|----------|
| `QUICKSTART_REACTJS.md` | ? 5-minute quick start | Start here for minimal setup |
| `FRONTEND_INTEGRATION_REACTJS.md` | ?? Complete guide | Full implementation with best practices |
| `DemoGeoServer_Auth_API.postman_collection.json` | ?? Postman collection | API testing |
| `test-login-email.http` | ?? VS Code REST Client | Quick API testing in VS Code |
| `LOGIN_WITH_EMAIL.md` | ?? API migration guide | Backend changes documentation |

---

## ?? Quick Links

### For Beginners
1. Start with `QUICKSTART_REACTJS.md`
2. Test API with `test-login-email.http` or Postman
3. See working examples in `QUICKSTART_REACTJS.md`

### For Production
1. Read full guide: `FRONTEND_INTEGRATION_REACTJS.md`
2. Implement all features:
 - Form validation (Formik + Yup)
   - Protected routes
   - Token refresh
   - Axios interceptors
3. Follow security best practices

---

## ?? API Endpoints Summary

### Base URL
```
http://localhost:5148/api
```

### Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register new user | ? |
| POST | `/auth/login` | **Login with email** | ? |
| POST | `/auth/refresh` | Refresh access token | ? |
| POST | `/auth/logout` | Logout (invalidate token) | ? |
| GET | `/test/public` | Public endpoint | ? |
| GET | `/test/protected` | Protected endpoint | ? |
| GET | `/test/check-role` | Check role claims | ? |
| GET | `/test/admin` | Admin-only endpoint | ? (Admin) |

---

## ?? Login API

### Request
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@demo.com",
  "password": "password123"
}
```

### Success Response (200 OK)
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwicm9sZSI6IkFkbWluIiwiZXhwIjoxNzY1Mjc0MTY0LCJpc3MiOiJEZW1vR2VvU2VydmVyIiwiYXVkIjoiRGVtb0dlb1NlcnZlclVzZXJzIn0...",
  "refreshToken": "base64-encoded-string...",
  "expiresAt": "2024-12-09T10:30:00Z",
  "message": "Login successful"
}
```

### Error Response (401 Unauthorized)
```json
{
  "success": false,
  "token": null,
  "refreshToken": null,
  "expiresAt": null,
  "message": "Invalid email or password"
}
```

### JWT Token Payload
```json
{
  "sub": "2",      // User ID
  "role": "Admin",         // User role
  "exp": 1765274164,          // Expiration timestamp
  "iss": "DemoGeoServer",    // Issuer
  "aud": "DemoGeoServerUsers"      // Audience
}
```

---

## ?? ReactJS Implementation

### Minimal Example (50 lines)
```typescript
import { useState } from 'react';
import axios from 'axios';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = async () => {
    try {
      const { data } = await axios.post(
        'http://localhost:5148/api/auth/login',
        { email, password }
      );

      if (data.success) {
        localStorage.setItem('token', data.token);
        alert('Login successful!');
      }
    } catch (error) {
   alert('Login failed');
    }
  };

  return (
    <div>
    <input 
      type="email"
        placeholder="Email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
      />
      <input 
    type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
    />
      <button onClick={handleLogin}>Login</button>
    </div>
  );
}
```

### Full Implementation
See `FRONTEND_INTEGRATION_REACTJS.md` for:
- ? Auth Service with TypeScript
- ? Login Component with validation
- ? Protected Routes
- ? Axios interceptors
- ? Token refresh logic
- ? Role-based access control

---

## ?? Test Credentials

| Email | Password | Role | Description |
|-------|----------|------|-------------|
| `admin@demo.com` | `password123` | Admin | Full access |
| `testuser@demo.com` | `password123` | User | Limited access |

---

## ?? Setup Instructions

### 1. Install Dependencies
```bash
npm install axios react-router-dom
```

### 2. Configure CORS (Backend)
Backend needs to allow requests from React app (port 3000):

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
  builder.WithOrigins("http://localhost:3000")
         .AllowAnyMethod()
    .AllowAnyHeader();
    });
});

app.UseCors();
```

### 3. Create Auth Service
```typescript
// src/services/auth.service.ts
import axios from 'axios';

const API_URL = 'http://localhost:5148/api';

export const login = async (email: string, password: string) => {
  const response = await axios.post(`${API_URL}/auth/login`, {
    email,
    password
  });
  
  if (response.data.success) {
    localStorage.setItem('token', response.data.token);
  }
  
  return response.data;
};
```

### 4. Use in Component
```typescript
import { login } from './services/auth.service';

const handleSubmit = async () => {
  const result = await login('admin@demo.com', 'password123');
  
  if (result.success) {
    // Navigate to dashboard
  }
};
```

---

## ?? Making Authenticated Requests

### Add Token to Headers
```typescript
import axios from 'axios';

const token = localStorage.getItem('token');

axios.get('http://localhost:5148/api/test/protected', {
  headers: {
    Authorization: `Bearer ${token}`
  }
});
```

### Using Axios Interceptor (Recommended)
```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5148/api'
});

// Automatically add token to all requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Usage - token automatically included
api.get('/test/protected');
api.get('/test/admin');
```

---

## ?? Complete Project Structure

```
my-react-app/
??? public/
??? src/
?   ??? components/
?   ?   ??? Login.tsx      # Login form
?   ? ??? ProtectedRoute.tsx     # Route guard
?   ?   ??? Navbar.tsx  # Navigation with logout
?   ??? pages/
?   ?   ??? Dashboard.tsx          # Protected page
?   ?   ??? AdminPanel.tsx         # Admin-only page
?   ??? services/
?   ?   ??? auth.service.ts        # Login, logout, token management
?   ?   ??? api.service.ts   # Axios instance with interceptors
?   ??? types/
?   ?   ??? auth.types.ts          # TypeScript interfaces
?   ??? utils/
?   ?   ??? axios.config.ts        # Axios configuration
? ??? App.tsx    # Main app with routes
?   ??? index.tsx
??? .env  # Environment variables
??? package.json
```

---

## ??? Security Best Practices

### ? DO
- Store tokens in httpOnly cookies (production)
- Validate email format on client side
- Use HTTPS in production
- Implement token refresh mechanism
- Add request timeout
- Rate limit login attempts
- Clear tokens on logout

### ? DON'T
- Log tokens or passwords
- Store sensitive data in localStorage (consider httpOnly cookies)
- Trust client-side validation only
- Expose API keys in frontend code
- Allow infinite login attempts

---

## ?? Troubleshooting

### CORS Error
**Problem:** `blocked by CORS policy`

**Solution:** Backend must enable CORS for React app origin
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => 
builder.WithOrigins("http://localhost:3000")
       .AllowAnyMethod()
.AllowAnyHeader()
    );
});
```

### 401 Unauthorized
**Problem:** Protected endpoints return 401

**Solution:** Ensure token is in Authorization header
```typescript
headers: {
  Authorization: `Bearer ${token}`  // Must include "Bearer " prefix
}
```

### Token Expired
**Problem:** Token expired, requests fail

**Solution:** Implement token refresh or re-login
```typescript
if (error.response?.status === 401) {
  // Token expired
  localStorage.removeItem('token');
  window.location.href = '/login';
}
```

---

## ?? Testing Tools

### 1. Postman
Import `DemoGeoServer_Auth_API.postman_collection.json`:
1. Open Postman
2. File ? Import
3. Select the JSON file
4. Test all endpoints

### 2. VS Code REST Client
Open `test-login-email.http` in VS Code:
1. Install "REST Client" extension
2. Click "Send Request" above each request
3. View response inline

### 3. Browser DevTools
```javascript
// Open Console (F12)
fetch('http://localhost:5148/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'admin@demo.com',
    password: 'password123'
  })
})
.then(r => r.json())
.then(console.log);
```

---

## ?? Learning Path

### Day 1: Basic Setup
1. ? Read `QUICKSTART_REACTJS.md`
2. ? Create minimal login component
3. ? Test with Postman/REST Client

### Day 2: Production Ready
1. ? Read `FRONTEND_INTEGRATION_REACTJS.md`
2. ? Add form validation
3. ? Implement protected routes
4. ? Add axios interceptors

### Day 3: Advanced Features
1. ? Token refresh mechanism
2. ? Role-based access control
3. ? Error handling & loading states
4. ? Unit tests

---

## ?? Support & Resources

### Documentation
- **Quick Start:** `QUICKSTART_REACTJS.md`
- **Full Guide:** `FRONTEND_INTEGRATION_REACTJS.md`
- **API Changes:** `LOGIN_WITH_EMAIL.md`
- **Backend Fix:** `FIX_COMPLETED.md`

### Testing
- **Postman:** `DemoGeoServer_Auth_API.postman_collection.json`
- **REST Client:** `test-login-email.http`

### Backend API
- Swagger: http://localhost:5148/swagger
- Base URL: http://localhost:5148/api

---

## ?? Quick Start Commands

```bash
# Clone/download project
git clone <your-repo>

# Install dependencies
npm install axios react-router-dom

# Start development server
npm start

# Build for production
npm run build
```

---

## ? Checklist

Frontend Setup:
- [ ] Install axios and react-router-dom
- [ ] Create auth service
- [ ] Create login component
- [ ] Setup protected routes
- [ ] Test login flow

Backend Setup:
- [ ] Backend running on port 5148
- [ ] CORS enabled for localhost:3000
- [ ] Test endpoints with Postman/REST Client

Testing:
- [ ] Login with admin@demo.com
- [ ] Login with testuser@demo.com
- [ ] Test invalid email format
- [ ] Test wrong password
- [ ] Test protected endpoints
- [ ] Test admin-only endpoint

---

**Ready to start? Begin with `QUICKSTART_REACTJS.md`** ??
