# RaceDay – Event Management System

Event Management System for South African road running, walking, and cycling events.

---

## Overview
RaceDay is a full-stack web-based platform for managing road running, walking, and cycling events in South Africa. The system supports two roles:

- **Organiser** – create and manage events, categories, and results
- **Participant** – browse events, enter events, and track personal results

---

## Part 1 Deliverables
- `RaceDay_Database.sql` – SQL script that creates the database and seed data
- `ERD.drawio.png` – Entity Relationship Diagram (6 entities)
- `RaceDay API Endpoint Plan.txt` – RESTful API endpoint plan

---

## Part 2 – RaceDay API

The RaceDay RESTful API is built in **C# with ASP.NET Core 8** and connects to SQL Server using **Entity Framework Core**.

### Features
- JWT Authentication (register + login)
- Role-based access control (Organiser / Participant)
- Full CRUD for Events, Categories, Enrolments and Results
- Swagger UI documentation
- Entity Framework Core migrations

### API Endpoints
- `POST /api/auth/register` – Register new user
- `POST /api/auth/login` – Login and receive JWT
- `GET /api/events` – List all events
- `POST /api/events` – Create event (Organiser only)
- `GET /api/events/{id}/categories` – Get categories for event
- `POST /api/events/{eventId}/enrol` – Enrol in event (Participant)
- `GET /api/events/{eventId}/results` – Get event results

### How to Run the API
1. Open the solution in Visual Studio 2022
2. Restore NuGet packages
3. Update the connection string in `appsettings.json` if needed
4. Run `Update-Database` in Package Manager Console
5. Press F5 – Swagger opens at `https://localhost:7006/swagger`

### CI/CD Status
![CI](https://github.com/ST10492111/RaceDay-/actions/workflows/ci.yml/badge.svg)

### Video Presentation
[Part 2 – RaceDay API Walkthrough](YOUR_VIDEO_LINK_HERE)

---

## Author
Chelsea Van Ross – ST10492111
