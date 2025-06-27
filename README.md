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

## 🌐 System Workflow Visualization

### 🔄 End-to-End Request Flow
```mermaid
sequenceDiagram
    participant Client
    participant Middleware
    participant Filters
    participant Controller
    participant MediatR
    participant Repository
    participant Database
    
    Client->>Middleware: HTTP Request
    Middleware->>Middleware: Global Exception Handling
    Middleware->>Filters: Forward Request
    Filters->>Filters: Check Cache (GET requests)
    alt Cached Response Found
        Filters-->>Client: Return Cached Data
    else Not Cached
        Filters->>Controller: Forward Request
        Controller->>MediatR: Send Command/Query
        MediatR->>Repository: Process Business Logic
        Repository->>Database: Execute Query
        Database-->>Repository: Return Data
        Repository-->>MediatR: Return Result
        MediatR-->>Controller: Return Response
        Controller->>Filters: Return Response
        Filters->>Filters: Cache Response (if GET)
        Filters-->>Client: Return Fresh Data
    end
