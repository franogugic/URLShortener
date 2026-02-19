# URL Shortener - Full Stack Production Project

### 🔗 [LIVE DEMO](http://116.203.122.236)
**Status**: The live demo is currently unavailable due to the expiration of the hosting plan on Hetzner VPS. However, the codebase is fully functional and can be run locally by following the installation steps below.

### Demo Account
A preconfigured demo user is available for testing application features:

| Field | Value |
|------|------|
| Username | `demo` |
| Password | `demo123` |

>⚠️ This account is intended for demonstration purposes only.

---

## Overview
A full\-stack URL shortening service with authentication, caching and rate limiting. The frontend is a React (Vite) SPA served by Nginx, and the backend is a .NET API (C#) backed by MySQL and Redis. The system is containerized and orchestrated with Docker Compose for development and production.

## Tech Stack
- Frontend: React (Vite) + Nginx (JavaScript, npm)
- Backend: .NET API (C\#) using ASP\.NET Core Identity and EF Core
- Database: MySQL 8\.0
- Cache: Redis
- Orchestration: Docker, Docker Compose

## Key Features
1. Authentication and security
    - ASP\.NET Core Identity for user management
    - Cookie\-based authentication with Data Protection keys persisted in a Docker volume
    - BCrypt password hashing
    - Rate limiting middleware to protect against brute\-force attacks

2. Performance and caching
    - Redis cache\-aside for O(1) redirect lookups and reduced MySQL I/O
    - Entity Framework Core with code\-first migrations applied at API startup

3. Infrastructure
    - Docker networking for internal service discovery via service names
    - Nginx reverse proxy configured with `try_files` to support React SPA routes
    - Docker volumes for persistent MySQL data and Data Protection keys

## Deployment Workflow
Manual pipeline used to deploy to Ubuntu VPS:
1. Sync source code with `rsync`.
2. Build multi\-stage Docker images (separate build and runtime layers).
3. Start services with `docker-compose`.

## Local Development

Prerequisites:
- Docker & Docker Compose installed
- Node.js & npm (for frontend development)

Start the full stack locally:
```bash
docker-compose up -d --build



