# RaceDay - Event Management System

## Overview
RaceDay is a full-stack web-based event management system for the South African road running, walking, and cycling community.

## Roles
- **Organiser**: Create, edit, and delete events, manage categories, capture results, and view all event enrolments.
- **Participant**: Create an account, browse events, enter an event by selecting a category, view their own enrolments, and track their personal results.

---

## Part 1 - Planning and Database
- **ERD**: `ERD.drawio.png`
- **API Endpoint Plan**: `RaceDay API Endpoint Plan.txt`
- **SQL Database Script**: `RaceDay_Database.sql`
- **CI/CD Workflow**: `.github/workflows/ci.yml` (Green Build)

## Part 2 - RESTful API
- Built with **C# ASP.NET Core 8** and **Entity Framework Core**.
- Connected to SQL Server.
- JWT Authentication (Register/Login).
- Role-based Authorization (Organiser / Participant).
- Full CRUD for Events, Categories, Enrolments, and Results.
- Swagger UI documentation available at `/swagger`.

## Part 3 - MVC Web Application & Docker
- Built with **ASP.NET Core MVC**.
- Consumes the Part 2 API (No direct database access).
- Session-based authentication.
- Role-specific interfaces (Organiser Dashboard vs. Participant Enrolment).
- **Docker**: Containerised using a `Dockerfile` and `.dockerignore`.

---

## Docker Setup

The application is containerised using Docker.

### To build the Docker image:
```bash
docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .
