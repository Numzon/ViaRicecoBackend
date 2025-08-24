# Docker Orchestration Setup for ViaRiceco Backend API

This document describes the Docker orchestration setup for the ViaRiceco backend API.

## Files in this Directory

- `docker-compose.yml` - Main composition file with all backend services
- `docker-compose.override.yml` - Development overrides (automatically loaded)
- `docker-compose.prod.yml` - Production configuration
- `docker-compose.dcproj` - Visual Studio Docker Compose project
- `.dockerignore` - Files to exclude from Docker build context

## Services

### ViaRiceco API (viariceco.api)
- Built from `./ViaRiceco.Api/Dockerfile`
- Exposed on ports 5000:8080 and 5001:8081 (development)
- Connected to PostgreSQL and Redis services
- Configured with Seq logging
- Depends on all other services

### PostgreSQL Database (viariceco.database)
- PostgreSQL latest image
- Exposed on port 5432
- Development database: `viariceco`
- Development user: `viariceco_user`
- Uses persistent volumes for data storage

### Redis Cache (viariceco.redis)
- Redis latest image
- Exposed on port 6379
- Used for caching and session management
- Password protected in all environments

### Seq Logging (viariceco.seq)
- Datalust Seq for structured logging
- Web UI available at http://localhost:8081 (port 8081:80)
- Management interface on port 5341
- **Development**: No authentication required (SEQ_FIRSTRUN_NOAUTHENTICATION=true)
- **Production**: Password-based authentication with admin password from environment variable

## Usage

### Development
```bash
# Navigate to Backend directory
cd Backend

# Start all services
docker-compose up -d

# Start and rebuild
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Production
```bash
# Navigate to Backend directory
cd Backend

# Create production network first
docker network create viariceco-network

# Deploy with production config
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Environment Variables

Create a `.env` file in the Backend directory with the following variables for production:

```env
# Docker Configuration
DOCKER_REGISTRY=your-registry.com/
API_VERSION=v1.0.0

# Database Configuration
DATABASE_CONNECTION_STRING=Host=viariceco.database;Port=5432;Database=viariceco;Username=prod_user;Password=secure_password;
POSTGRES_DB=viariceco
POSTGRES_USER=prod_user
POSTGRES_PASSWORD=secure_password

# Redis Configuration
REDIS_CONNECTION_STRING=viariceco.redis:6379
REDIS_PASSWORD=secure_redis_password

# Seq Logging
SEQ_ADMIN_PASSWORD=secure_seq_password

# Note: Development environment uses no authentication (SEQ_FIRSTRUN_NOAUTHENTICATION=true)
# Production environment overrides this with password-based authentication for security
```

## Visual Studio Integration

The `docker-compose.dcproj` file allows Visual Studio to:
- Debug the application in containers
- Build and run with F5
- Attach debugger to containerized processes
- View container logs
- Set as startup project for container orchestration

## Development Database Initialization

To initialize the database with seed data, create scripts in `./database/init/` directory. These will be automatically executed when the PostgreSQL container starts.

## Networking

All services communicate through the `viariceco-network` Docker network, enabling service discovery by name. Services can reference each other using their full service names:
- `viariceco.api` - Main API service
- `viariceco.database` - PostgreSQL database
- `viariceco.redis` - Redis cache
- `viariceco.seq` - Seq logging service

## Build Context

The build context is set to the current directory (Backend), so all paths in Dockerfiles are relative to the Backend folder.
