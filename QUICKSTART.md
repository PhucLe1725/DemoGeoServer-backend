# Quick Start Guide

## ?? Ch?y ?ng d?ng trong 5 b??c

### B??c 1: T?o Database
```sql
-- K?t n?i vào PostgreSQL
psql -U postgres

-- T?o database
CREATE DATABASE DemoGeoServer;

-- K?t n?i vào database v?a t?o
\c DemoGeoServer

-- T?o b?ng users
CREATE TABLE public.users (
    id serial4 NOT NULL,
    username varchar(100) NOT NULL,
    email varchar(255) NOT NULL,
    created_at timestamp DEFAULT now() NOT NULL,
    password_hash varchar(255) NULL,
    role varchar(50) NULL,
    CONSTRAINT users_pkey PRIMARY KEY (id)
);

-- T?o b?ng refresh_tokens
CREATE TABLE public.refresh_tokens (
    id serial4 NOT NULL,
    user_id serial4 NOT NULL,
    token text NOT NULL,
    expiry_date timestamp NOT NULL,
created_at timestamp DEFAULT now() NULL,
    updated_at timestamp DEFAULT now() NULL,
    CONSTRAINT refresh_tokens_user_id_fkey FOREIGN KEY (user_id) 
        REFERENCES public.users(id) ON DELETE CASCADE
);
```

### B??c 2: C?u hình Connection String
M? file `appsettings.json` và c?p nh?t:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=DemoGeoServer"
  }
}
```

### B??c 3: Ch?y ?ng d?ng
```bash
cd DemoGeoServer
dotnet run
```

?ng d?ng s? ch?y t?i: `http://localhost:5148`

### B??c 4: Test API v?i Swagger
M? trình duy?t và truy c?p: `http://localhost:5148/swagger`

### B??c 5: Test Authentication Flow

#### 5.1 Register User (Không tr? token)
```http
POST http://localhost:5148/api/auth/register
Content-Type: application/json

{
  "username": "admin",
  "password": "password123",
  "email": "admin@demo.com",
  "role": "Admin"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful. Please login to continue.",
  "userId": 1,
  "username": "admin",
  "email": "admin@demo.com"
}
```

#### 5.2 Login (Tr? token)
```http
POST http://localhost:5148/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGci...",
  "refreshToken": "AbC123...",
  "expiresAt": "2025-01-26T11:00:00Z",
  "message": "Login successful"
}
```

**L?u l?i token t? response!**

#### 5.3 Test Protected Endpoint
```http
GET http://localhost:5148/api/test/protected
Authorization: Bearer YOUR_TOKEN_HERE
```

**Response:**
```json
{
  "message": "This is a protected endpoint - authentication required",
  "userId": "1",
  "username": "admin",
  "email": "admin@demo.com"
}
```

#### 5.4 Test Admin Endpoint
```http
GET http://localhost:5148/api/test/admin
Authorization: Bearer YOUR_TOKEN_HERE
```

## ?? Test Scenarios

### ? Scenario 1: ??ng ký user m?i
1. G?i `/api/auth/register`
2. Ki?m tra database có record m?i
3. Password ?ã ???c hash
4. **Không nh?n ???c JWT token**
5. Ph?i login ?? l?y token

### ? Scenario 2: ??ng nh?p
1. Sau khi register, g?i `/api/auth/login` v?i username/password
2. **Nh?n ???c access token và refresh token**
3. Refresh token ???c l?u vào database

### ? Scenario 3: Truy c?p protected endpoint
1. G?i `/api/test/protected` **KHÔNG** có token ? 401 Unauthorized
2. G?i `/api/test/protected` **CÓ** token ? 200 OK

### ? Scenario 4: Refresh token
1. ??i access token h?t h?n (ho?c gi? l?p)
2. G?i `/api/auth/refresh` v?i refresh token
3. Nh?n ???c tokens m?i

### ? Scenario 5: Logout
1. G?i `/api/auth/logout/{userId}`
2. Refresh tokens b? xóa kh?i database
3. Không th? refresh token n?a

## ?? Ki?m tra Database

```sql
-- Xem t?t c? users
SELECT * FROM users;

-- Xem refresh tokens
SELECT rt.*, u.username 
FROM refresh_tokens rt
JOIN users u ON rt.user_id = u.id;

-- Ki?m tra password ?ã hash
SELECT id, username, 
       LEFT(password_hash, 20) as password_preview 
FROM users;
```

## ?? API Endpoints Summary

| Method | Endpoint | Auth Required | Returns Token | Description |
|--------|----------|---------------|---------------|-------------|
| POST | `/api/auth/register` | ? | ? | ??ng ký user m?i |
| POST | `/api/auth/login` | ? | ? | ??ng nh?p, nh?n token |
| POST | `/api/auth/refresh` | ? | ? | Refresh access token |
| POST | `/api/auth/logout/{userId}` | ? | ? | ??ng xu?t |
| GET | `/api/test/public` | ? | ? | Public endpoint |
| GET | `/api/test/protected` | ? | ? | Protected endpoint |
| GET | `/api/test/admin` | ? (Admin role) | ? | Admin only endpoint |

## ?? ?i?m Khác Bi?t Quan Tr?ng

### ? Register KHÔNG tr? token
- Ch? tr? v? thông tin user
- Client ph?i g?i login riêng

### ? Login M?I tr? token
- Tr? v? access token (60 phút)
- Tr? v? refresh token (7 ngày)

### T?i sao?
1. **B?o m?t**: Tách bi?t vi?c t?o tài kho?n và xác th?c
2. **Audit**: D? track hành vi ??ng ký vs ??ng nh?p
3. **Flexibility**: Có th? thêm email verification sau này
4. **Best Practice**: Follow OAuth2/OIDC standards

## ?? Next Steps

1. ? ?ã hoàn thành: Authentication c? b?n
2. ?? Có th? thêm:
   - Email verification sau khi register
   - Password reset
   - Two-factor authentication
   - Rate limiting
   - API logging
   - User management endpoints (CRUD)

## ?? Support
N?u g?p v?n ??, ki?m tra:
1. Build successful: `dotnet build`
2. Database connection: Test connection string
3. Logs: Xem console output khi ch?y app
