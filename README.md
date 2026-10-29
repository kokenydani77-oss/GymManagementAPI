# Gym Management API

A lightweight, robust backend REST API built with **.NET 8** and **Entity Framework Core** for gym membership and access management. The project demonstrates clean architecture patterns, relational data modeling, automated testing with **xUnit**, and automated API documentation via **Swagger / OpenAPI**.

---

## Key Features

- **Member Management (CRUD):** Complete lifecycle handling for gym members.
- **Pass Management:** Support for both duration-based (monthly/annual) and punch-card (occasional) passes with automatic expiration tracking.
- **Access Control & Check-in System:**
  - Automated validity check (expiration date & remaining visits).
  - Anti-passback / anti-duplicate check-in protection (prevents scanning in twice without checking out).
  - Real-time facility occupancy tracking (`/api/checkins/occupancy`).
- **Automated Testing:** Business-critical check-in constraints covered by unit tests using an in-memory database.

---

## Tech Stack & Architecture

- **Platform:** .NET 8 (C#)
- **Framework:** ASP.NET Core Web API
- **Object-Relational Mapping:** Entity Framework Core
- **Database:** SQLite
- **Testing:** xUnit, Microsoft.EntityFrameworkCore.InMemory
- **Documentation:** Swagger