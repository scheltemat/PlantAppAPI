# 🌿 PlantAppAPI

Backend server for **Perennial**, a plant care application. This API is built with **.NET 9.0** and **Microsoft SQL Server**, and integrates with the [Permapeople API](https://permapeople.org/knowledgebase/api-docs.html) to provide detailed plant data.

---

## 🚀 Getting Started

### ✅ Prerequisites

Make sure you have the following installed:

- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download)
- [Microsoft SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

---

### 📦 Installation & Setup

1. **Install dependencies**
   ```bash
   dotnet install package
   ```
2. **Run database migrations**
   ```bash
   dotnet ef migrations
   ```
3. **Update the database schema**
   ```bash
   dotnet ef database update
   ```
4. **Start the server**
   ```bash
   dotnet watch run
   ```

## ⚙️ Environment Setup

Before running the app, create a `.env` file in the root directory with the following variables:

---

### 🔑 JWT Authentication

| Variable                 | Description                                                                         |
| ------------------------ | ----------------------------------------------------------------------------------- |
| `JWT_SECRET_KEY`         | Secret key used to sign JWT tokens (recommended to be a 256 bit hexadecimal string) |
| `JWT_ISSUER`             | Issuer for the JWT i.e. http://localhost:5000                                       |
| `JWT_AUDIENCE`           | Audience for the JWT i.e. http://localhost:4200                                     |
| `JWT_EXPIRATION_MINUTES` | Token expiration duration in minutes                                                |

---

### 🌱 Permapeople API Integration

- you will need to register for these:

| Variable                 | Description                              |
| ------------------------ | ---------------------------------------- |
| `PERMAPEOPLE_KEY_ID`     | API Key ID for accessing Permapeople API |
| `PERMAPEOPLE_KEY_SECRET` | Secret key for Permapeople API           |

---

### 📧 SMTP Email Settings

| Variable          | Description                                                                                                              |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `SMTP_HOST`       | SMTP server host (e.g., smtp.gmail.com)                                                                                  |
| `SMTP_PORT`       | SMTP server port (typically 587 for TLS)                                                                                 |
| `SMTP_USERNAME`   | Email address used to authenticate                                                                                       |
| `SMTP_PASSWORD`   | Password or app-specific password (must be created at [Google App Passwords](https://myaccount.google.com/apppasswords)) |
| `SMTP_FROM_EMAIL` | Email address that will appear as the sender                                                                             |
| `SMTP_FROM_NAME`  | Display name of the sender in the email                                                                                  |

---

### 🗄️ Database Configuration

Make sure your `appsettings.json` contains:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PlantAppDb;Trusted_Connection=True;",
  "ProductionConnection": "Your production database connection string here"
}
```

## 🔗 API Integration

This app integrates with [Permapeople](https://permapeople.org/) to retrieve plant data, providing rich information for plant care and management.

## 🛠️ Tech Stack

- .NET 9.0
- Microsoft SQL Server
- Entity Framework Core
- Permapeople API
