# Quick Start - ReactJS Login Integration

## ?? 5-Minute Setup

### Step 1: Install Dependencies
```bash
npm install axios react-router-dom
```

### Step 2: Create Auth Service

Create `src/services/auth.service.ts`:

```typescript
import axios from 'axios';

const API_URL = 'http://localhost:5148/api/auth';

export const login = async (email: string, password: string) => {
  const response = await axios.post(`${API_URL}/login`, { email, password });
  
  if (response.data.success) {
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
  }
  
  return response.data;
};

export const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
};

export const getToken = () => localStorage.getItem('token');

export const isAuthenticated = () => !!getToken();
```

### Step 3: Create Login Component

Create `src/components/Login.tsx`:

```typescript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../services/auth.service';

export default function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const response = await login(email, password);
      
      if (response.success) {
        navigate('/dashboard');
      } else {
        setError(response.message);
 }
 } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed');
    }
  };

  return (
    <div style={{ maxWidth: '400px', margin: '100px auto', padding: '20px' }}>
      <h2>Login</h2>
      
      {error && <div style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}
      
   <form onSubmit={handleSubmit}>
  <div style={{ marginBottom: '15px' }}>
  <label>Email:</label>
        <input
 type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="admin@demo.com"
       required
        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
          />
     </div>
        
        <div style={{ marginBottom: '15px' }}>
          <label>Password:</label>
          <input
            type="password"
        value={password}
          onChange={(e) => setPassword(e.target.value)}
  placeholder="password123"
        required
            style={{ width: '100%', padding: '8px', marginTop: '5px' }}
          />
        </div>
      
 <button type="submit" style={{ width: '100%', padding: '10px', backgroundColor: '#4CAF50', color: 'white', border: 'none', cursor: 'pointer' }}>
   Login
        </button>
      </form>
    </div>
  );
}
```

### Step 4: Setup Routes

Update `src/App.tsx`:

```typescript
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './components/Login';
import Dashboard from './pages/Dashboard';
import { isAuthenticated } from './services/auth.service';

function ProtectedRoute({ children }: { children: JSX.Element }) {
  return isAuthenticated() ? children : <Navigate to="/login" />;
}

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
        <Route path="/" element={<Navigate to="/dashboard" />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
```

### Step 5: Create Dashboard

Create `src/pages/Dashboard.tsx`:

```typescript
import { useNavigate } from 'react-router-dom';
import { logout } from '../services/auth.service';

export default function Dashboard() {
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div style={{ padding: '20px' }}>
      <h1>Dashboard</h1>
      <p>Welcome! You are logged in.</p>
    <button onClick={handleLogout} style={{ padding: '10px 20px', backgroundColor: '#f44336', color: 'white', border: 'none', cursor: 'pointer' }}>
        Logout
   </button>
    </div>
  );
}
```

---

## ?? Test It!

1. Start your backend: `dotnet run` (port 5148)
2. Start React app: `npm start` (port 3000)
3. Navigate to http://localhost:3000/login
4. Login with:
   - **Email:** `admin@demo.com`
   - **Password:** `password123`

---

## ?? API Request Format

```typescript
// What to send
POST http://localhost:5148/api/auth/login
Content-Type: application/json

{
  "email": "admin@demo.com",
  "password": "password123"
}

// What you get back (success)
{
  "success": true,
  "token": "eyJhbGc...",
  "refreshToken": "base64string...",
  "expiresAt": "2024-12-09T10:30:00Z",
  "message": "Login successful"
}

// What you get back (error)
{
  "success": false,
  "token": null,
  "refreshToken": null,
  "expiresAt": null,
  "message": "Invalid email or password"
}
```

---

## ?? Using Token for Protected Requests

```typescript
// src/services/api.service.ts
import axios from 'axios';
import { getToken } from './auth.service';

const api = axios.create({
  baseURL: 'http://localhost:5148/api',
});

// Add token to every request
api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
 config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;

// Usage
import api from './services/api.service';

// This request will automatically include Authorization header
api.get('/test/protected')
  .then(response => console.log(response.data));
```

---

## ? Test Credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@demo.com` | `password123` | Admin |
| `testuser@demo.com` | `password123` | User |

---

## ?? Common Issues

### CORS Error
**Problem:** `Access to XMLHttpRequest has been blocked by CORS policy`

**Solution:** Backend needs CORS configuration. Add to `Program.cs`:
```csharp
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

### Token Not Working
**Problem:** Protected routes return 401

**Solution:** Check if token is being sent:
```typescript
// Add console.log to verify
api.interceptors.request.use((config) => {
  const token = getToken();
  console.log('Token:', token); // Should see JWT token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

---

## ?? Next Steps

1. ? Add form validation (Formik + Yup)
2. ? Add loading states
3. ? Add error handling
4. ? Implement token refresh
5. ? Add remember me functionality
6. ? Style with CSS/Tailwind/Material-UI

See full documentation: `FRONTEND_INTEGRATION_REACTJS.md`

---

## ?? Pro Tips

### 1. JWT Decode (see user info)
```typescript
const decodeToken = (token: string) => {
  const payload = token.split('.')[1];
  return JSON.parse(atob(payload));
};

const user = decodeToken(getToken()!);
console.log('User ID:', user.sub);
console.log('Role:', user.role);
```

### 2. Auto-logout on Token Expiry
```typescript
setInterval(() => {
  const token = getToken();
  if (token) {
    const decoded = decodeToken(token);
const expired = Date.now() >= decoded.exp * 1000;
    
    if (expired) {
    logout();
      window.location.href = '/login';
    }
  }
}, 60000); // Check every minute
```

### 3. Redirect After Login
```typescript
// Store intended URL before login
const from = location.state?.from?.pathname || '/dashboard';
navigate(from, { replace: true });
```

---

## ?? Complete Example

```typescript
// Minimal working example
import { useState } from 'react';
import axios from 'axios';

function App() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [token, setToken] = useState('');

  const login = async () => {
    try {
      const { data } = await axios.post('http://localhost:5148/api/auth/login', {
        email,
        password
      });
      
      if (data.success) {
        setToken(data.token);
        alert('Login successful!');
      } else {
        alert(data.message);
      }
    } catch (error) {
      alert('Login failed');
    }
  };

  return (
    <div style={{ padding: 50 }}>
      <h1>Login</h1>
      <input 
        placeholder="Email" 
   value={email} 
        onChange={e => setEmail(e.target.value)} 
      />
      <input 
        type="password" 
  placeholder="Password" 
        value={password} 
  onChange={e => setPassword(e.target.value)} 
      />
      <button onClick={login}>Login</button>
 
      {token && <p>Token: {token.substring(0, 50)}...</p>}
    </div>
  );
}
```

That's it! You now have a working login system. ??
