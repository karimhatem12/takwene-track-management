# Takwene Track Management

A small music-distribution back office: register artists, add their tracks, send tracks to DSPs
(Spotify, Apple Music, YouTube, Anghami) and follow each track from **Draft → Submitted → Distributed**.

- **Backend:** ASP.NET Core Web API on .NET 10, Clean Architecture, EF Core 10 + SQL Server LocalDB, JWT auth
- **Frontend:** Angular 21 single-page app: track list (artist and status filters), track detail (DSP statuses and
  history), an artists page, and popups to add and edit artists and tracks, send tracks to DSPs and move their status
- **Extras:** audit trail of every change (who / what / when), ProblemDetails errors, Swagger UI, unit tests

Design choices, AI usage and security notes are in [DECISIONS.md](DECISIONS.md).

---

## Contents

1. [Prerequisites](#1-prerequisites)
2. [Quick start](#2-quick-start)
3. [Database and migrations](#3-database-and-migrations)
4. [Getting a JWT](#4-getting-a-jwt)
5. [API reference](#5-api-reference)
6. [Running the tests](#6-running-the-tests)
7. [Project structure](#7-project-structure)
8. [Configuration and secrets](#8-configuration-and-secrets)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. Prerequisites

| Tool | Version | Check with |
|---|---|---|
| .NET SDK | **10.0.100 or newer** (pinned in `backend/global.json`) | `dotnet --version` |
| SQL Server **LocalDB** | any recent (installed with Visual Studio's "Data storage" workload or SQL Server Express) | `sqllocaldb info` |
| Node.js | **22.12 or newer** (Angular 21 requirement) | `node --version` |
| npm | 10+ | `npm --version` |

Optional:

- **Visual Studio 2026** (v18) or newer. Visual Studio 2022 *cannot* load .NET 10 projects (see [Troubleshooting](#9-troubleshooting)).
- **SQL Server Management Studio (SSMS)** to browse the database.

## 2. Quick start

Two terminals, from the repository root.

**Terminal 1: API**

```bash
cd backend
dotnet run --project src/TrackManagement.Api --launch-profile https
```

On the first start in Development the API:

1. creates the `TakweneTracks` database in LocalDB and applies all migrations,
2. seeds the sample data (4 DSPs, 4 artists, 10 tracks, 19 distributions),
3. creates a **demo user** (`demo@takwene.local`) for signing in.

Then it listens on **https://localhost:7142**, and Swagger UI is at **https://localhost:7142/swagger**.

> First time using HTTPS with .NET on this machine? Trust the dev certificate once:
> `dotnet dev-certs https --trust`

**Terminal 2: Angular app**

```bash
cd frontend
npm install
npm start
```

Open **http://localhost:4200**. A sign-in popup appears: sign in with the demo user, or register a new account
from the **Register** tab. The demo user's password is in
`backend/src/TrackManagement.Api/appsettings.Development.json` under `DemoUser:Password`.

In Visual Studio: open `backend/TrackManagement.slnx`, choose the **https** profile and press **Ctrl+F5**
(see [why not F5](#the-api-is-slow-or-hangs-on-404-errors-in-visual-studio)).

## 3. Database and migrations

### Connection

The API uses SQL Server **LocalDB**, no server install or password needed:

```
Server=(localdb)\MSSQLLocalDB;Database=TakweneTracks;Trusted_Connection=True;...
```

(`ConnectionStrings:DefaultConnection` in `backend/src/TrackManagement.Api/appsettings.json`.)

### Migrations

| Migration | What it does |
|---|---|
| `InitialCreate` | Tables, keys, unique indexes (artist email, track ISRC, one distribution per track+DSP) |
| `SeedSampleData` | 4 DSPs, 4 artists, 10 tracks across genres and all three statuses, 19 DSP distributions |
| `AddAuditTrail` | `CreatedAt/By`, `UpdatedAt/By` columns and the `AuditLogs` table |

In **Development** they are applied automatically when the API starts. To apply them by hand
(e.g. for a non-Development environment):

```bash
cd backend
dotnet tool restore
dotnet ef database update --project src/TrackManagement.Infrastructure --startup-project src/TrackManagement.Api
```

`dotnet tool restore` installs the pinned `dotnet-ef` version from `backend/dotnet-tools.json`.

Other useful commands (run from `backend/`):

```bash
# Start over: drop the database (it is recreated and reseeded on the next API start)
dotnet ef database drop --force --project src/TrackManagement.Infrastructure --startup-project src/TrackManagement.Api

# Add a migration after changing the model
dotnet ef migrations add <Name> --project src/TrackManagement.Infrastructure --startup-project src/TrackManagement.Api --output-dir Persistence/Migrations
```

### Opening the database in SSMS

1. Open SSMS → **Connect → Database Engine**.
2. **Server name:** type `(localdb)\MSSQLLocalDB` (LocalDB does *not* appear in the server browse list, and it
   is not your machine name such as `MY-PC` or `MY-PC\SQLEXPRESS`).
3. **Authentication:** Windows Authentication.
4. If SSMS 20+ shows an encryption option, set **Encrypt** to *Optional* or tick **Trust server certificate**.
5. Connect → **Databases → TakweneTracks → Tables**.

Alternative without SSMS: Visual Studio → **View → SQL Server Object Explorer** → `(localdb)\MSSQLLocalDB`.

Handy queries:

```sql
SELECT * FROM Tracks ORDER BY ReleaseDate DESC;
SELECT * FROM TrackDistributions WHERE TrackId = 1;
SELECT TOP 50 * FROM AuditLogs ORDER BY Id DESC;   -- who changed what
```

## 4. Getting a JWT

Every endpoint except `POST /api/auth/register` and `POST /api/auth/login` requires
`Authorization: Bearer <token>`. Tokens are valid for 60 minutes.

### Option A: Swagger UI (easiest)

1. Open https://localhost:7142/swagger.
2. Expand **POST /api/auth/login** → *Try it out* and send:
   ```json
   { "email": "demo@takwene.local", "password": "<DemoUser:Password from appsettings.Development.json>" }
   ```
   (or call **POST /api/auth/register** first with your own name, email and password).
3. Copy the `token` value from the response.
4. Click **Authorize** (top right), paste the token (without the word `Bearer`), click *Authorize*.
   All requests from Swagger now carry the token, and it is kept across page reloads.

### Option B: curl

```bash
# Register your own user (password: 8+ characters with at least one letter and one digit)…
curl -k -X POST https://localhost:7142/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Jane Reviewer","email":"jane@example.com","password":"Reviewer2026"}'

# …or sign in: both return { "token": "...", "expiresAt": "...", "user": { ... } }
curl -k -X POST https://localhost:7142/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"jane@example.com","password":"Reviewer2026"}'

# Use the token
curl -k https://localhost:7142/api/tracks -H "Authorization: Bearer <token>"
```

(`-k` accepts the local development certificate.)

### Option C: the Angular app

Sign in through the popup; the app stores the token for the browser tab and sends it on every API call.
Closing the tab, signing out, the token expiring or any `401` from the API brings the popup back.

## 5. API reference

Base URL: `https://localhost:7142/api`. Full, interactive documentation: Swagger UI.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/auth/register` | Create an account, returns a JWT *(anonymous, rate limited)* |
| POST | `/auth/login` | Sign in, returns a JWT *(anonymous, rate limited)* |
| GET | `/artists` | List artists, each with its track count |
| POST | `/artists` | Create an artist (`name`, unique `email`, 2-letter `country` code) |
| PUT | `/artists/{id}` | Edit an artist (same fields and rules as create) |
| GET | `/tracks?artistId=&genre=&status=` | List tracks; all filters optional and combinable |
| POST | `/tracks` | Create a track (starts as `Draft`) |
| GET | `/tracks/{id}` | Track details with each DSP distribution and its status |
| PUT | `/tracks/{id}` | Edit a track (`artistId`, `title`, `isrc`, `releaseDate`, `genre`); see the rules below |
| POST | `/tracks/{id}/distribute` | Send the track to DSPs: `{ "dspIds": [1, 4] }` |
| PATCH | `/tracks/{id}/status` | Move the status one step forward: `{ "status": "Submitted" }` |
| GET | `/tracks/{id}/history` | Audit history of the track and its distributions, newest first |
| GET | `/dsps` | List the DSPs a track can be sent to |
| GET | `/genres` | List the allowed genres |
| GET | `/audit-logs?entityName=&entityId=&userId=&action=&page=&pageSize=` | Search the whole audit trail (paged) |

DSP ids: `1` Spotify, `2` Apple Music, `3` YouTube, `4` Anghami.

### Business rules

- **Status only moves forward, one step at a time:** `Draft → Submitted → Distributed`.
- **Distributing** creates a `Pending` distribution per DSP and moves a `Draft` track to `Submitted`.
  A track can be sent to each DSP only once.
- A track can become **`Distributed`** only when it is `Live` on at least one DSP. `Live` and `Rejected` are
  reported by the DSPs, so there is no endpoint (or button) to set them.
- **Editing a track:** title, genre and release date can change at any time. The **artist and ISRC are locked once
  the track leaves `Draft`**, because the DSPs already know the recording by them (409 otherwise).
- **ISRC** must be 12 characters in the form `CC-XXX-YY-NNNNN`; dashes and spaces are optional
  (`EG-TKW-24-00001` and `EGTKW2400001` are both accepted and stored as `EGTKW2400001`). Unique per track.
- **Genre** must be one of the fixed list from `GET /api/genres` (Arabic Pop, Classical, Electronic, Hip Hop,
  Indie, Jazz, Khaleeji, Mahraganat, Pop, R&B, Rai, Rock, Shaabi, Tarab). Any case is accepted and stored in
  the list's spelling (`hip hop` → `Hip Hop`); anything else is a 400. This keeps the genre filter reliable.
- Artist **email** is unique (case-insensitive).

### Errors

All errors use [RFC 7807 ProblemDetails](https://www.rfc-editor.org/rfc/rfc7807):

| Status | When | Example |
|---|---|---|
| 400 | Invalid input; `errors` lists messages per field | `"Isrc": ["ISRC must look like 'CC-XXX-YY-NNNNN' (e.g. 'EG-TKW-24-00001')."]` |
| 401 | Missing, invalid or expired token; wrong email or password | |
| 404 | Unknown track id | `"Track with id '999' was not found."` |
| 409 | Duplicate data or a broken business rule | `"Track cannot be marked as Distributed until it is live on at least one DSP."` |
| 429 | More than 10 login/register attempts per minute from one IP | |
| 500 | Unexpected error: a generic message only; details go to the server log | |

### Audit trail

Every create, update and delete of artists, tracks and distributions is recorded automatically in the
`AuditLogs` table with the signed-in user, the time, and the changed fields as `{ "old": ..., "new": ... }`.
Successful registrations and logins are recorded too. Each audited row also has
`CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` columns. Read it through `GET /api/tracks/{id}/history`,
`GET /api/audit-logs`, or the **History** section of the track detail page.

The sample data is inserted by a migration, so seeded rows show `system` as their author and have no
history entries until someone changes them.

## 6. Running the tests

```bash
cd backend
dotnet test
```

57 unit tests (xUnit) cover:

- **Domain rules:** forward-only status transitions, the "Live on at least one DSP" rule, one distribution
  per DSP, Draft → Submitted on distribute, a Live/Rejected outcome is final, ISRC normalisation, which fields
  may be edited in which status, the fixed genre list
- **Validators:** ISRC format, required fields, email, country codes, password rules, and that create and edit
  use the same rules
- **Audit interceptor:** records who created a track, only the changed fields with old/new values,
  `system` when nobody is signed in, and never records users or password hashes

Frontend build check:

```bash
cd frontend
npm run build
```

## 7. Project structure

```
backend/
  TrackManagement.slnx
  src/
    TrackManagement.Domain/          Entities + business rules (no dependencies)
    TrackManagement.Application/     Use cases: services, DTOs, validators, interfaces (Common/Interfaces)
    TrackManagement.Infrastructure/  EF Core DbContext, migrations, seed data, audit interceptor, JWT, password hashing
    TrackManagement.Api/             Controllers, auth/CORS/rate-limit setup, error handling, Swagger
  tests/
    TrackManagement.Tests/           Unit tests
frontend/
  src/app/
    core/        Auth (service, interceptor, sign-in popup), API models, error handling
    features/    tracks (list, detail, add/edit form, send-to-DSPs popup), artists (list, add/edit form)
    shared/      Popup (dialog) shell, status badge, ISRC formatting
```

Dependencies point inwards: `Api → Application ← Infrastructure`, and everything depends on `Domain`.
More detail: [backend/README.md](backend/README.md) (projects, where to find each rule, request flow) and
[frontend/README.md](frontend/README.md).

## 8. Configuration and secrets

| Setting | Where | Notes |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `appsettings.json` | LocalDB by default |
| `Jwt:SigningKey` | `appsettings.Development.json` (development only) | **Empty in `appsettings.json` on purpose.** The API refuses to start without a key of at least 32 characters |
| `Jwt:ExpiryMinutes` | `appsettings.json` | 60 |
| `Cors:AllowedOrigins` | `appsettings.json` | Only `http://localhost:4200` may call the API from a browser |
| `DemoUser` | `appsettings.Development.json` | Created at startup **only in Development** |
| `apiUrl` | `frontend/src/environments/*.ts` | `https://localhost:7142/api` |

The key and the demo password in `appsettings.Development.json` are for local development only.
For any other environment, supply the signing key from a secret store or an environment variable, e.g.:

```bash
# PowerShell: $env:Jwt__SigningKey = "<at least 32 random characters>"
export Jwt__SigningKey="<at least 32 random characters>"
```

## 9. Troubleshooting

### `OptionsValidationException: Jwt:SigningKey is missing`

The API is not running in the **Development** environment, so `appsettings.Development.json` (which holds
the development key) is not loaded. Start it with the `https` or `http` launch profile (`--launch-profile https`
or the profile drop-down in Visual Studio), which sets `ASPNETCORE_ENVIRONMENT=Development`. Opening the
folder instead of `TrackManagement.slnx` in Visual Studio can also lose the profile.

### `The SDK 'Microsoft.NET.Sdk' specified could not be found` / projects not loaded in Visual Studio

Visual Studio 2022 (v17) does not support .NET 10. Open `backend/TrackManagement.slnx` in **Visual Studio
2026** (v18) or newer, or use the `dotnet` CLI.

### `A network-related or instance-specific error… (provider: SQL Network Interfaces, error: 50)`

LocalDB is not running or failed to start. Check and start it:

```bash
sqllocaldb info MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

If it says *"SQL Server process failed to start"* (Windows event log: *WaitForMultipleObjects returned error
code 575*), force-stop it and start again, and if that still fails, **restart Windows**: LocalDB sometimes
loses track of its own process:

```bash
sqllocaldb stop MSSQLLocalDB -k
sqllocaldb start MSSQLLocalDB
```

### The API is slow or hangs on 404 errors in Visual Studio

Not-found and business-rule errors are raised as exceptions and turned into ProblemDetails by a global
handler. Under the debugger (**F5**) Visual Studio may pause on them ("exception user-unhandled"), so the
request waits until you press *Continue*. Run with **Ctrl+F5** (no debugger), or untick *Break when this
exception type is user-unhandled* for these exceptions. Without the debugger such requests take milliseconds.

### Swagger shows "Failed to fetch" or the browser warns about the certificate

Run `dotnet dev-certs https --trust` once and restart the browser. Use the `https://localhost:7142/swagger`
address (not the `http` one).

### The Angular app says "Cannot reach the API"

The API is not running on `https://localhost:7142`, or the browser does not yet trust the development
certificate: open https://localhost:7142/swagger once in the same browser, then reload the app.

### Login returns `429 Too many requests`

More than 10 sign-in/register attempts in one minute from your IP. Wait a minute.

### Audit ids jump (e.g. 9 → 10009)

Normal SQL Server behaviour: identity values are reserved in blocks and unused ones are skipped after a
restart. Ids are unique, not gap-free.
