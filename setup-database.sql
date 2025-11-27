-- =====================================================
-- DemoGeoServer Database Setup Script
-- =====================================================
-- Run this script as postgres user
-- Command: psql -U postgres -f setup-database.sql
-- =====================================================

-- 1. Create database if not exists
SELECT 'Creating database...' AS status;

DROP DATABASE IF EXISTS "DemoGeoServer";
CREATE DATABASE "DemoGeoServer"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
  LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Connect to the new database
\c DemoGeoServer

SELECT 'Connected to DemoGeoServer database' AS status;

-- 2. Create users table
SELECT 'Creating users table...' AS status;

CREATE TABLE IF NOT EXISTS public.users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255),
    role VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_users_username ON public.users(username);
CREATE INDEX IF NOT EXISTS idx_users_email ON public.users(email);

SELECT 'Users table created successfully' AS status;

-- 3. Create refresh_tokens table
SELECT 'Creating refresh_tokens table...' AS status;

CREATE TABLE IF NOT EXISTS public.refresh_tokens (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    token TEXT NOT NULL,
    expiry_date TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT refresh_tokens_user_id_fkey 
        FOREIGN KEY (user_id) 
        REFERENCES public.users(id) 
        ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON public.refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON public.refresh_tokens(token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expiry ON public.refresh_tokens(expiry_date);

SELECT 'Refresh tokens table created successfully' AS status;

-- 4. Insert sample data (optional - comment out if not needed)
SELECT 'Inserting sample data...' AS status;

-- Sample admin user (password: password123)
-- Password hash for BCrypt: $2a$11$... (you should generate this)
INSERT INTO public.users (username, email, password_hash, role, created_at)
VALUES 
    ('testadmin', 'admin@test.com', '$2a$11$dummyhash', 'Admin', NOW()),
    ('testuser', 'user@test.com', '$2a$11$dummyhash', 'User', NOW())
ON CONFLICT (username) DO NOTHING;

SELECT 'Sample data inserted' AS status;

-- 5. Verify tables
SELECT 'Verifying database setup...' AS status;

SELECT 
    'users' AS table_name,
    COUNT(*) AS record_count
FROM public.users
UNION ALL
SELECT 
    'refresh_tokens' AS table_name,
    COUNT(*) AS record_count
FROM public.refresh_tokens;

-- 6. Display table structures
SELECT 'Table structures:' AS status;

\d public.users
\d public.refresh_tokens

-- 7. Grant permissions (if using different user)
-- GRANT ALL PRIVILEGES ON DATABASE "DemoGeoServer" TO your_app_user;
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_app_user;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO your_app_user;

SELECT 'Database setup completed successfully!' AS status;
SELECT 'You can now run your application with connection string:' AS info;
SELECT 'Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=DemoGeoServer' AS connection_string;
