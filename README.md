# QRHub API

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

A production-grade, highly scalable RESTful QR Code Generation and Management API built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, **PostgreSQL (Docker)**, and **JWT Bearer Authentication**.

---

## Table of Contents

- [Key Features](#key-features)
- [Architecture & Folder Structure](#architecture--folder-structure)
- [Tech Stack](#tech-stack)
- [Quick Start Guide](#quick-start-guide)
  - [1. Prerequisites](#1-prerequisites)
  - [2. Start PostgreSQL via Docker](#2-start-postgresql-via-docker)
  - [3. Run the Application](#3-run-the-application)
- [Interactive API Documentation](#interactive-api-documentation)
- [API Reference](#api-reference)
  - [Authentication Endpoints](#authentication-endpoints)
  - [QR Code CRUD Endpoints](#qr-code-crud-endpoints)
  - [Public Quick Generate Endpoints](#public-quick-generate-endpoints)
- [Sample Requests & Responses](#sample-requests--responses)
- [Database Migrations](#database-migrations)
- [Docker & Containerization](#docker--containerization)
- [Production Scaling Best Practices](#production-scaling-best-practices)

---

## Key Features

- **Production Clean Architecture**: Strictly decoupled into **Domain**, **Application**, **Infrastructure**, and **API** layers.
- **Dockerized PostgreSQL**: Zero manual PostgreSQL installation required; run with a single `docker compose up -d` command.
- **Automated Database Migrations**: Automatically applies pending EF Core migrations on application boot.
- **Secure Authentication**: Robust Email/Password authentication with salted **BCrypt** password hashing and **JWT Bearer** tokens.
- **Multi-Tenant User Isolation**: Users can only access, view, update, download, and delete their own QR codes.
- **Advanced QR Code Customization**:
  - Custom foreground and background colors (Hex codes).
  - Configurable Error Correction Levels (`L` ~7%, `M` ~15%, `Q` ~25%, `H` ~30%).
  - Configurable module resolution (`PixelsPerModule`).
  - Supports URLs, plain text, WiFi credentials, vCards, etc.
- **Dual Output Formats**: Returns both direct inline/downloadable PNG image streams (`image/png`) and Base64 Data URLs for frontend rendering.
- **Pagination & Search**: Full-text searching and sorting for user QR collections.
- **Modern Scalar API Documentation**: Interactive, high-performance API testing UI at `/scalar/v1`.
- **Global Error Handling**: Standardized `ApiResponse<T>` envelope for both success and error payloads.

---

## Architecture & Folder Structure

The project is structured according to **Clean Architecture / Onion Architecture** principles, separating business logic from infrastructure concerns:

```
QRHub/
├── QRHub.Domain/                      # Enterprise Core (POCO Entities, Enums)
│   ├── Entities/
│   │   ├── BaseEntity.cs              # Common Id, CreatedAtUtc, UpdatedAtUtc
│   │   ├── User.cs                    # User entity with navigation properties
│   │   └── QrCode.cs                  # QR Code entity with styling attributes
│   └── Enums/
│       └── QrErrorCorrectionLevel.cs  # L, M, Q, H error correction levels
│
├── QRHub.Application/                 # Business Logic & Use Cases
│   ├── Common/
│   │   ├── Exceptions/                # NotFound, Conflict, Unauthorized, BadRequest
│   │   ├── Interfaces/                # IApplicationDbContext, IJwtTokenGenerator,
│   │   │                              # IPasswordHasher, IQrCodeGenerator, ICurrentUserService
│   │   └── Models/                    # ApiResponse<T>, PagedResult<T>
│   ├── DTOs/
│   │   ├── Auth/                      # RegisterRequest, LoginRequest, AuthResponse, UserDto
│   │   └── QrCodes/                   # CreateQrCodeRequest, UpdateQrCodeRequest,
│   │                                  # QrCodeResponse, QrQueryParameters, QuickGenerateQrRequest
│   ├── Services/                      # AuthService, QrCodeService
│   └── DependencyInjection.cs         # Service layer DI registration
│
├── QRHub.Infrastructure/              # External Concerns (DB, Security, Generation)
│   ├── Authentication/
│   │   ├── JwtSettings.cs             # Strongly-typed JWT options
│   │   ├── JwtTokenGenerator.cs       # HMAC-SHA256 JWT generation
│   │   └── PasswordHasher.cs          # BCrypt.Net-Next enhanced hashing
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs    # EF Core DbContext
│   │   ├── Configurations/            # Fluent API configurations & database indexes
│   │   └── Migrations/                # EF Core PostgreSQL migration files
│   ├── Services/
│   │   ├── CurrentUserService.cs      # HttpContext accessor claims extraction
│   │   └── QrCodeGeneratorService.cs  # High-performance PNG byte generation
│   └── DependencyInjection.cs         # Infrastructure DI registration
│
├── QRHub.Api/                         # Entry Point & Presentation Layer
│   ├── Controllers/
│   │   ├── BaseApiController.cs       # Base controller with authenticated user helpers
│   │   ├── AuthController.cs          # Register, Login, Me endpoints
│   │   └── QrCodesController.cs       # QR Code CRUD, image download & quick-generate
│   ├── Extensions/
│   │   └── MigrationExtensions.cs     # Startup auto-migration runner
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs # Global error response formatter
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json               # Database connection string & JWT secret
│   ├── appsettings.Development.json
│   ├── Program.cs                     # Middleware pipeline & dependency wiring
│   └── QRHub.Api.http                 # Ready-to-use HTTP requests for VS Code/Rider
│
├── Dockerfile                         # Multi-stage production build container
├── docker-compose.yml                 # PostgreSQL 16 container definition
└── QRHub.sln                          # .NET solution file
```

---

## Tech Stack

| Component | Technology | Version |
| :--- | :--- | :--- |
| **Framework** | .NET / ASP.NET Core Web API | 10.0 |
| **Database** | PostgreSQL | 16 (Alpine) |
| **ORM** | Entity Framework Core + Npgsql | 10.0.13 |
| **Authentication** | JWT Bearer Tokens (`System.IdentityModel.Tokens.Jwt`) | 8.23.0 |
| **Password Hashing** | BCrypt.Net-Next | 4.2.0 |
| **QR Code Engine** | QRCoder (`PngByteQRCode`) | 1.8.1 |
| **API Docs** | Scalar API Reference UI | 2.18.0 |
| **Containerization**| Docker & Docker Compose | Latest |

---

## Quick Start Guide

### 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (must be running)

### 2. Environment Configuration

The application uses `.env` files for seamless configuration across Docker and local execution:

- **`.env`**: Contains your active local development settings (loaded automatically by `docker compose` and `dotnet run`).
- **`.env.local`**: Contains dummy / placeholder data for local overrides or testing without production values.
- **`.env.example`**: Version-controlled template for team onboarding.

```bash
# To initialize from template:
cp .env.example .env
```

### 3. Start PostgreSQL via Docker

Run the following command in the root project directory:

```bash
docker compose up -d postgres
```

Verify that the container is healthy:

```bash
docker ps --filter "name=qrhub-postgres"
```

> **Note**: PostgreSQL is mapped to port `5432` with username `postgres`, password `postgres`, and database `qrhub_db`.

### 4. Run the Application

Start the Web API:

```bash
dotnet run --project QRHub.Api
```

The application will automatically:
1. Load environment variables from `.env` and `.env.local`.
2. Connect to PostgreSQL in Docker.
3. Apply all pending EF Core migrations (creating tables and indexes).
4. Start listening on **`http://localhost:5143`**.

---

## Interactive API Documentation

Open your browser and navigate to:

👉 **[http://localhost:5143/scalar/v1](http://localhost:5143/scalar/v1)** (or simply **[http://localhost:5143](http://localhost:5143)**)

You will see the **Scalar API Documentation** with dark-mode, interactive request execution, code snippets in multiple languages, and Bearer token authorization.

---

## API Reference

### Standard Response Envelope

All JSON responses follow a standardized format:

```json
{
  "success": true,
  "message": "Operation description.",
  "data": { ... },
  "errors": null
}
```

---

### Authentication Endpoints

| Method | Endpoint | Auth | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Public | Register a new user with name, email, and password. |
| `POST` | `/api/auth/login` | Public | Authenticate user and receive a JWT Bearer token. |
| `GET` | `/api/auth/me` | Bearer Token | Retrieve profile of the currently logged-in user. |

---

### QR Code CRUD Endpoints

All endpoints below require the `Authorization: Bearer <token>` header:

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/qrcodes` | Create a customized QR code for the authenticated user. |
| `GET` | `/api/qrcodes` | List user's QR codes (with pagination, sorting, search). |
| `GET` | `/api/qrcodes/{id}` | Retrieve details of a specific QR code by ID. |
| `GET` | `/api/qrcodes/{id}/image` | Stream the raw PNG image file (`Content-Type: image/png`). |
| `PUT` | `/api/qrcodes/{id}` | Update title, content, colors, or resolution. |
| `DELETE`| `/api/qrcodes/{id}` | Delete a QR code belonging to the user. |

#### Query Parameters for `GET /api/qrcodes`

- `searchTerm` *(string, optional)*: Filter by title, content, or description.
- `pageNumber` *(int, default: 1)*: Page number (1-indexed).
- `pageSize` *(int, default: 10, max: 50)*: Number of items per page.
- `sortBy` *(string, default: "CreatedAtUtc")*: Field to sort by (`CreatedAtUtc`, `Title`, `UpdatedAtUtc`).
- `sortDescending` *(bool, default: true)*: Sort direction.

---

### Public Quick Generate Endpoints

Generate QR code images on the fly without saving to the database:

| Method | Endpoint | Auth | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/qrcodes/quick-generate` | Public | Generate PNG image from query params (embed in `<img>` tags). |
| `POST` | `/api/qrcodes/quick-generate` | Public | Generate PNG image from JSON request body. |

---

## Sample Requests & Responses

### 1. Register User

**Request:**
```bash
curl -X POST http://localhost:5143/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Krishna Gupta",
    "email": "krishna@example.com",
    "password": "SecurePassword123!"
  }'
```

**Response (HTTP 201 Created):**
```json
{
  "success": true,
  "message": "User registered successfully.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ...",
    "tokenType": "Bearer",
    "expiresAtUtc": "2026-10-15T10:38:06Z",
    "user": {
      "id": "eea5bf5f-32cb-4499-b8c0-3989e29d0c42",
      "name": "Krishna Gupta",
      "email": "krishna@example.com",
      "createdAtUtc": "2026-10-14T10:38:06Z"
    }
  },
  "errors": null
}
```

---

### 2. Login User

**Request:**
```bash
curl -X POST http://localhost:5143/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "krishna@example.com",
    "password": "SecurePassword123!"
  }'
```

**Response (HTTP 200 OK):**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ...",
    "tokenType": "Bearer",
    "expiresAtUtc": "2026-10-15T10:38:21Z",
    "user": {
      "id": "eea5bf5f-32cb-4499-b8c0-3989e29d0c42",
      "name": "Krishna Gupta",
      "email": "krishna@example.com",
      "createdAtUtc": "2026-10-14T10:38:06Z"
    }
  },
  "errors": null
}
```

---

### 3. Create a Custom QR Code

**Request:**
```bash
curl -X POST http://localhost:5143/api/qrcodes \
  -H "Authorization: Bearer <YOUR_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My GitHub Profile",
    "content": "https://github.com/krishnaa6268",
    "description": "Personal developer portfolio and projects",
    "foregroundColorHex": "#0A66C2",
    "backgroundColorHex": "#FFFFFF",
    "errorCorrectionLevel": "H",
    "pixelsPerModule": 20
  }'
```

**Response (HTTP 201 Created):**
```json
{
  "success": true,
  "message": "QR code created successfully.",
  "data": {
    "id": "d99aaad5-9050-49b0-bb33-a960aa9e5188",
    "userId": "eea5bf5f-32cb-4499-b8c0-3989e29d0c42",
    "title": "My GitHub Profile",
    "content": "https://github.com/krishnaa6268",
    "description": "Personal developer portfolio and projects",
    "foregroundColorHex": "#0A66C2",
    "backgroundColorHex": "#FFFFFF",
    "errorCorrectionLevel": "H",
    "pixelsPerModule": 20,
    "createdAtUtc": "2026-10-14T10:40:21Z",
    "updatedAtUtc": null,
    "downloadUrl": "/api/qrcodes/d99aaad5-9050-49b0-bb33-a960aa9e5188/image",
    "base64DataUrl": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUg..."
  },
  "errors": null
}
```

---

### 4. Direct Image Download / Display

You can embed the QR code directly into any website or HTML template:

```html
<img src="http://localhost:5143/api/qrcodes/d99aaad5-9050-49b0-bb33-a960aa9e5188/image" alt="QR Code" />
```

Or download with `curl`:

```bash
curl -O -J -H "Authorization: Bearer <YOUR_TOKEN>" \
  http://localhost:5143/api/qrcodes/d99aaad5-9050-49b0-bb33-a960aa9e5188/image
```

---

### 5. Quick Generate (No Auth / Inline Image)

Generate a branded QR code image on the fly:

```html
<img src="http://localhost:5143/api/qrcodes/quick-generate?content=https://example.com&foregroundColor=%230A66C2&backgroundColor=%23F3F4F6&pixelsPerModule=20" alt="Quick QR" />
```

---

## Database Migrations

The project uses EF Core migrations located in `QRHub.Infrastructure/Persistence/Migrations`.

Migrations are **applied automatically** when the application launches via `app.ApplyMigrations()`.

To create a new migration manually:

```bash
dotnet dotnet-ef migrations add <MigrationName> \
  --project QRHub.Infrastructure \
  --startup-project QRHub.Api \
  --output-dir Persistence/Migrations
```

To update the database manually:

```bash
dotnet dotnet-ef database update \
  --project QRHub.Infrastructure \
  --startup-project QRHub.Api
```

---

## Docker & Containerization

### Running with Docker Compose (Full Stack)

You can run both PostgreSQL and the API inside Docker:

```bash
docker compose --profile all up --build -d
```

### Stopping Containers

```bash
docker compose down
```

To wipe the database volume:

```bash
docker compose down -v
```

---

## Production Scaling Best Practices

When deploying QRHub to production environments (AWS ECS / EKS, Azure App Service / AKS, GCP Cloud Run), consider these architecture advantages already built into this codebase:

1. **Stateless API Design**: The API holds zero in-memory session state; tokens are stateless signed JWTs, allowing horizontal autoscaling behind any load balancer (Nginx, ALB, Cloudflare).
2. **Database Connection Pooling**: PostgreSQL connection pooling is handled by `Npgsql` (default pool size: 100 connections per instance).
3. **Optimized Database Indexes**: Includes composite B-tree index `(user_id, created_at_utc)` for sub-millisecond paginated queries even with millions of QR records.
4. **Caching Layer (Future Expansion)**: To scale read traffic further, you can inject a Redis distributed cache in `QrCodeService` using the existing `IQrCodeService` interface without modifying controller or domain code.
5. **Non-Root Docker Security**: The production Dockerfile builds on Alpine Linux and runs as an unprivileged `appuser`.