GiftApi

GiftApi is a modern, modular .NET 8 Web API designed for managing users, vouchers, brands, categories, file uploads, and admin operations.
Built with clean architecture principles using CQRS, MediatR, EF Core, JWT Authentication, FluentValidation, and production-ready design patterns.

🚀 Features
🔐 Authentication & User Management

User registration & login

JWT access & refresh tokens

Email verification

Password reset flow

Get current user

Role-based authorization (User/Admin)

🎁 Voucher System

Create, update, delete & restore vouchers

Purchase vouchers

Redemption tracking

Voucher usage statistics

Bulk voucher import/upsert

🏪 Brand & Category Management

CRUD operations for brands & categories

Admin-only protected endpoints

📁 File Uploads

Upload files/images to ImageKit

Store and manage metadata

Retrieve uploaded file records

📊 Admin Panel & Statistics

Dashboard summary

Brand leaderboard

Voucher usage insights

🧪 Testing

NUnit + FluentAssertions

Unit & integration tests supported

🛠 Technologies Used

.NET 8 (C# 12)

ASP.NET Core Web API

Entity Framework Core (SQL Server)

MediatR (CQRS)

FluentValidation

JWT Authentication

Swagger / OpenAPI

ImageKit SDK

NUnit, FluentAssertions

📂 Project Architecture
GiftApi
│
├── Application        // CQRS Handlers, Commands, Queries, Validation
├── Domain             // Entities, Enums, Interfaces
├── Infrastructure     // EF Core, Repositories, ImageKit, Auth & JWT Services
└── WebApi             // Controllers, Middleware, DI, Startup configuration

📡 Endpoints Overview
Area	Controller	Examples
Auth	AuthController	POST /api/auth/register, POST /api/auth/login
User	UserController	GET /api/user/current, POST /api/user/refresh-token
Voucher	VoucherController	GET /api/voucher, POST /api/voucher/buy
Brand	BrandController	GET /api/brand, POST /api/brand/create
Category	CategoryController	GET /api/category, POST /api/category/create
File	FileController	POST /api/file/upload, GET /api/file
Manage	ManageController	POST /api/manage/create-brand, POST /api/manage/bulk-upsert-vouchers
Statistics	StatisticsController	GET /api/statistics/brand-leaderboard
Dashboard	DashboardController	GET /api/dashboard/summary
Health	HealthController	GET /api/health

Most management endpoints require Admin role.

⚙️ Getting Started
Prerequisites

.NET 8 SDK

SQL Server (local or cloud)

(Optional) Docker

▶️ Setup Instructions
1. Clone the repository
git clone https://github.com/yourusername/GiftApi.git

2. Navigate to the WebApi project
cd GiftApi/WebApi

3. Update the connection string

In appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=GiftApiDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

4. Apply migrations
dotnet ef database update

5. Run the API
dotnet run

6. Open Swagger UI
https://localhost:5001/swagger

🔑 Environment Variables
JWT
JwtSettings:Secret
JwtSettings:Issuer
JwtSettings:Audience
JwtSettings:AccessTokenLifetime
JwtSettings:RefreshTokenLifetime

ImageKit
ImageKit:PublicKey
ImageKit:PrivateKey
ImageKit:UrlEndpoint

🗂 Example Database Schema (Simplified ERD)
User (1) ──── (∞) VoucherPurchase ──── (∞) Voucher
        └──── (∞) RefreshToken

Brand (1) ──── (∞) Voucher
Category (1) ──── (∞) Voucher

FileStorage (1) ──── (1) User / Voucher / Brand

📜 License

This project is developed for portfolio/demo purposes.
You are free to use, modify, or reference it.

👤 Author

Mikheil Kharazishvili
GitHub: https://github.com/mkharazishvili95
