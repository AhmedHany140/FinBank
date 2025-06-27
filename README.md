# 💳 FinBankDMO - Digital Banking System

## 🧾 Overview

**FinBankDMO** is a full-featured digital banking system designed to simulate real-world banking functionalities. The system includes account management, secure transactions, interest calculations, email OTP verification, and automatic scheduling powered by Hangfire.

---

## 🚀 Features

- ✅ Account Creation with AES-256 Encryption
- 🔐 Secure Transactions (Transfer / Deposit / Withdraw)
- 💰 Interest Management System (Daily, Monthly, Yearly)
- 🕒 Automatic Scheduling via Hangfire
- 📧 Email Verification via OTP (Gmail SMTP)
- 🔔 Real-time Notifications using MediatR Events
- 📊 Paginated data for performance optimization
- 🧾 Audit Logging for all sensitive operations
- 👤 Role & Policy Based Authorization (with Claims support)
- 🧭 Clean Architecture with strict separation of concerns
- 💫 Mapping using **Mapperly** (build-time generated mapping)
- 🔍 Logging using **Serilog**
- 🧠 Auto **Cache Filters** & **Behaviors** (to optimize performance)
- ❗ Auto **Exception Filters** & **Behaviors** (for centralized error handling)
- ⚠️ Global Exception Middleware for uncaught errors

---

## 🛠️ Tech Stack

| Layer           | Tools/Frameworks |
|----------------|------------------|
| **Backend**    | ASP.NET Core Web API, MediatR, Entity Framework Core |
| **Database**   | SQL Server |
| **Scheduling** | Hangfire |
| **Auth**       | ASP.NET Core Identity + JWT + Roles & Policies |
| **Email**      | MailKit + Gmail App Password |
| **Security**   | AES Encryption (using `System.Security.Cryptography`) |
| **Logging**    | Serilog + Custom Audit Logging |
| **Architecture** | Clean Architecture + Unit of Work + Repository Pattern |

---

flowchart TD
    UI[Client Request: GET /api/bankaccounts/{id}] -->|Authorize| AuthCheck[Policy: IsAccountOwner]
    AuthCheck -->|✅| CacheCheck[Cache Filter Behavior]
    CacheCheck -->|⛔ Not Found in Cache| Mediator[Send MediatR Query]
    Mediator -->|Pipeline| Behaviors[Exception + Caching Behavior]
    Behaviors --> Handler[BankAccountQueryHandler]
    Handler --> UoW[Unit of Work]
    UoW --> Repo[BankAccount Repository]
    Repo --> Db[(SQL Server)]
    Handler -->|Return Encrypted Result| Encryption[Decrypt with AES]
    Encryption -->|Return| CacheSet[Store in Cache]
    CacheSet --> UI

    subgraph "Filters"
        GlobalEx[Global Exception Middleware]
        AutoExc[Auto Exception Filter]
        AutoCach[Auto Cache Filter]
    end

    UI --> GlobalEx
    GlobalEx --> AutoExc
    GlobalEx --> AutoCach


## 🧪 Run Locally

```bash
# Clone the repo
git clone https://github.com/yourname/FinBankDMO.git

# Open the solution in Visual Studio

# Configure your appsettings.json:
- ConnectionStrings
- EmailSettings (SMTP, App Password)
- Encryption (Key, IV)

# Apply migrations
dotnet ef database update

# Run the application
dotnet run
