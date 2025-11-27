# DemoGeoServer - Authentication API

## C?u trúc d? án (Clean Architecture)

```
??? DemoGeoServer.Domain        # Core Domain Layer
?   ??? Entities/
?       ??? User.cs
?       ??? RefreshToken.cs
?
??? DemoGeoServer.Application     # Application Layer (Use Cases & DTOs)
?   ??? DTOs/Auth/
?   ?   ??? LoginRequest.cs
?   ?   ??? LoginResponse.cs
?   ?   ??? RegisterRequest.cs
?   ? ??? RegisterResponse.cs
?   ?   ??? RefreshTokenRequest.cs
?   ??? Interfaces/
?    ??? IAuthService.cs
?    ??? ITokenService.cs
?       ??? IUserRepository.cs
?       ??? IRefreshTokenRepository.cs
?
??? DemoGeoServer.Infrastructure  # Infrastructure Layer (Implementation)
?   ??? Configuration/
?   ?   ??? JwtSettings.cs
?   ??? Data/
?   ?   ??? ApplicationDbContext.cs
?   ??? Repositories/
?   ?   ??? UserRepository.cs
?   ?   ??? RefreshTokenRepository.cs
?   ??? Services/
?       ??? AuthService.cs
?   ??? JwtTokenService.cs
?
??? DemoGeoServer.API       # Presentation Layer (API)
    ??? Controllers/
    ?   ??? AuthController.cs
    ?   ??? TestController.cs
    ??? Program.cs
```

## Database Schema (PostgreSQL)

### Users Table
```sql
CREATE TABLE public.users (
    id serial4 NOT NULL,
    username varchar(100) NOT NULL,
    email varchar(255) NOT NULL,
    created_at timestamp DEFAULT now() NOT NULL,
    password_hash varchar(255) NULL,
    role varchar(50) NULL,
    CONSTRAINT users_pkey PRIMARY KEY (id)
);
```

### RefreshTokens Table
```sql
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

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=123;Database=DemoGeoServer"
  },
  "JwtConfig": {
    "Issuer": "DemoGeoServer",
    "Audience": "DemoGeoServerUsers",
    "Key": "0c6830be50543343974d1bd75ba3ad129a681127db866a2249c26eaf73185ac9",
    "DurationInMinutes": 60
  }
}
```

## API Endpoints

### 1. Register (No Token)
**POST** `/api/auth/register`

**Request:**
```json
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

### 2. Login (Returns Token)
**POST** `/api/auth/login`

**Request:**
```json
{
  "username": "admin",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64_encoded_refresh_token",
  "expiresAt": "2025-01-26T10:00:00Z",
  "message": "Login successful"
}
```

### 3. Refresh Token
**POST** `/api/auth/refresh`

**Request:**
```json
{
  "refreshToken": "your_refresh_token_here"
}
```

**Response:**
```json
{
  "success": true,
  "token": "new_jwt_token",
  "refreshToken": "new_refresh_token",
  "expiresAt": "2025-01-26T11:00:00Z",
  "message": "Token refreshed successfully"
}
```

### 4. Logout
**POST** `/api/auth/logout/{userId}`

**Response:**
```json
{
  "message": "Logout successful"
}
```

## Cách ch?y d? án

### 1. Chu?n b? Database
??m b?o PostgreSQL ?ang ch?y và t?o database:
```sql
CREATE DATABASE DemoGeoServer;
```

Ch?y script t?o b?ng (xem file `setup-database.sql`)

### 2. C?p nh?t Connection String
S?a `appsettings.json` v?i thông tin PostgreSQL c?a b?n

### 3. Ch?y ?ng d?ng
```bash
cd DemoGeoServer
dotnet run
```

### 4. Test API

#### Cách 1: Dùng Swagger UI (Recommended)
M? trình duy?t: `http://localhost:5148/swagger`

**Swagger ?ã ???c c?u hình v?i JWT Authorization!**

**Steps:**
1. Register user t?i `POST /api/auth/register`
2. Login t?i `POST /api/auth/login` - Copy token t? response
3. Click nút **"Authorize"** (??) ? góc trên bên ph?i
4. Paste token vào ô "Value"
5. Click "Authorize" ? "Close"
6. Gi? b?n có th? test các protected endpoints!

?? **Chi ti?t:** Xem file `SWAGGER-QUICK-START.md`

#### Cách 2: Dùng HTTP File
M? file `DemoGeoServer.http` trong Visual Studio và test các endpoints

## Packages s? d?ng

- **Npgsql.EntityFrameworkCore.PostgreSQL** (8.0.10) - PostgreSQL provider cho EF Core
- **BCrypt.Net-Next** (4.0.3) - Password hashing
- **System.IdentityModel.Tokens.Jwt** (8.15.0) - JWT token generation
- **Microsoft.AspNetCore.Authentication.JwtBearer** (8.0.11) - JWT authentication

## Security Features

? Password hashing v?i BCrypt  
? JWT tokens v?i expiration  
? Refresh tokens (7 days validity)  
? Token validation  
? Secure token storage trong database  
? **Register không tr? token (ph?i login riêng)**

## Flow ??ng ký/??ng nh?p

### Register Flow:
1. Client g?i username, password, email
2. Server ki?m tra username/email ?ã t?n t?i ch?a
3. Hash password v?i BCrypt
4. L?u user vào database
5. **Tr? v? thông tin user (KHÔNG có token)**
6. Client ph?i g?i login ?? l?y token

### Login Flow:
1. Client g?i username và password
2. Server tìm user trong database
3. Verify password v?i BCrypt
4. **T?o JWT access token (60 phút) và refresh token (7 ngày)**
5. L?u refresh token vào database
6. **Tr? v? tokens**

### Refresh Token Flow:
1. Client g?i refresh token
2. Server validate refresh token t? database
3. Ki?m tra expiry date
4. T?o JWT access token m?i và refresh token m?i
5. Update refresh token trong database
6. Tr? v? tokens m?i

### Logout Flow:
1. Client g?i userId
2. Server xóa t?t c? refresh tokens c?a user
3. Client xóa tokens trong local storage

## Authentication Flow Diagram

```
???????????      ???????????
? Client  ?    ?  Server ?
???????????   ???????????
     ?         ?
     ?  1. POST /register      ?
     ?  (username, password, email) ?
     ??????????????????????????????>?
     ?     ? Hash password
     ?        ? Save to DB
     ?       ?
 ?  2. RegisterResponse         ?
     ?  (NO TOKEN)       ?
     ?<??????????????????????????????
     ?     ?
     ?  3. POST /login              ?
     ?  (username, password)        ?
     ??????????????????????????????>?
     ?        ? Verify password
     ?                   ? Generate tokens
     ?      ?
  ?  4. LoginResponse            ?
     ?  (WITH TOKEN + REFRESH)      ?
     ?<??????????????????????????????
     ? ?
     ?  5. Use token for requests   ?
     ?  Authorization: Bearer xxx   ?
     ??????????????????????????????>?
?          ?
