# A Matter of Taste — Claude Context

## What This System Is

A personal family recipe website built as a gift for Cheryl Charbonneau. Originally a printed cookbook started ~15 years ago, now a full web application. The site is public-facing with user accounts for saving favorites, and a single admin account for Cheryl to manage recipes.

Built by the same developer as the CharbTech Business System — follow all the same conventions, patterns, and architectural decisions used there unless this document says otherwise.

---

## Deployment Architecture

- Single Azure VM, IIS-hosted
- Single PostgreSQL database (no multi-tenant requirement)
- Domain: **www.amatteroftaste.us** is canonical. The apex (`amatteroftaste.us`) is bound in IIS
  and permanently redirects to `www` via the rewrite rule in `AMatterOfTaste.Api/web.config`.
  The Let's Encrypt cert (win-acme, renewal "[IIS] AMatterOfTaste, (any host)") covers both
  names automatically — it derives them from the site's bindings, so no cert config is needed
  when bindings change.
- Same Azure subscription and resource group patterns as CharbTech

---

## Tech Stack

### Backend
- .NET 8
- C#
- ASP.NET Web API
- Entity Framework Core
- IronPDF (licensed) — for cookbook PDF generation

### Frontend
- React
- TypeScript
- Tailwind CSS (this project only — not AG Grid, simpler UI needs)

### Database
- PostgreSQL 17
- Port 5332 (same as all CharbTech tenant VMs)
- Service name: postgresql-x64-17

### Infrastructure
- Azure VM, IIS
- E: drive for PostgreSQL data directory
- F: drive for backups

### External APIs
- Anthropic API (Claude) — recipe paste-and-parse feature, proxied through the .NET API (`/api/ai/*`)
- Pinterest — Save to Pinterest via URL scheme (no API key required)

---

## SQL Conventions

**These are mandatory. Never deviate.**

- All table names: **lowercase**
- All column names: **lowercase**
- **Never use quoted identifiers**
- Standard audit fields on all inserts:

```sql
createdbyid = 1,
modifiedbyid = 1,
createddate = now(),
modifieddate = now()
```

---

## Entity Conventions

### Base Class: `IntIdentityBase`
All entities inherit from `IntIdentityBase`:
- `Id`
- `CreatedById`
- `CreatedDate`
- `ModifiedById`
- `ModifiedDate`

**Always preserve this pattern.**

### Soft Deletes
All entities use `IsActive` (bool) for soft deletes. Never hard delete.

---

## Database Schema

### Core Tables

```sql
-- Recipe categories
create table recipecategory (
  id serial primary key,
  name varchar(100) not null,
  sortorder int not null default 0,
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now()
);

-- Recipes
create table recipe (
  id serial primary key,
  title varchar(255) not null,
  description text,
  notes text,
  story text,                        -- personal story/memory behind the recipe
  attribution varchar(255),          -- "From Joyce Poole" / "Nancy Charbonneau's Recipe"
  categoryid int not null references recipecategory(id),
  servings int,
  preptimeminutes int,
  cooktimeminutes int,
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now()
);

-- Structured ingredients (parsed from text)
create table recipeingredient (
  id serial primary key,
  recipeid int not null references recipe(id),
  sortorder int not null default 0,
  quantity varchar(50),              -- "2", "1/2", "¾"
  unit varchar(50),                  -- "cups", "tsp", "lbs"
  name varchar(255) not null,        -- "all-purpose flour"
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now()
);

-- Structured steps (parsed from text)
create table recipestep (
  id serial primary key,
  recipeid int not null references recipe(id),
  stepnumber int not null,
  instruction text not null,
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now()
);

-- Recipe photos
create table recipephoto (
  id serial primary key,
  recipeid int not null references recipe(id),
  filename varchar(255) not null,
  isprimary bool not null default false,
  sortorder int not null default 0,
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now()
);

-- User accounts (family/friends who save favorites)
create table appuser (
  id serial primary key,
  name varchar(150) not null,
  email varchar(255) not null unique,
  passwordhash varchar(500) not null,
  cookbookslug varchar(100) unique,  -- e.g. "jessie" → /cookbook/jessie
  isadmin bool not null default false,
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now()
);

-- User saved favorites
create table userfavorite (
  id serial primary key,
  userid int not null references appuser(id),
  recipeid int not null references recipe(id),
  isactive bool not null default true,
  createdbyid int not null default 1,
  modifiedbyid int not null default 1,
  createddate timestamp not null default now(),
  modifieddate timestamp not null default now(),
  unique(userid, recipeid)
);
```

---

## Authentication

- JWT bearer tokens
- Single admin account seeded at startup (Cheryl) — `isadmin = true`
- Public users can register with name + email + password
- Admin-only endpoints protected with `[Authorize(Roles = "Admin")]`
- Standard users protected with `[Authorize]`
- Passwords hashed with BCrypt

---

## API Structure

Follow CharbTech controller/service patterns:

```
/api/recipes          GET (public, paginated, filterable by category)
/api/recipes/{id}     GET (public)
/api/recipes          POST (admin only)
/api/recipes/{id}     PUT (admin only)
/api/recipes/{id}     DELETE (soft delete, admin only)

/api/categories       GET (public)
/api/categories       POST/PUT/DELETE (admin only)

/api/auth/register    POST (public)
/api/auth/login       POST (public)

/api/favorites        GET (authenticated user — their list)
/api/favorites        POST (authenticated — add recipe)
/api/favorites/{id}   DELETE (authenticated — remove)

/api/cookbook/{slug}  GET (public — shareable favorites list)

/api/pdf/cookbook     GET (authenticated — generate IronPDF cookbook)

/api/photos/upload    POST (admin only — multipart form)
/api/import/json      POST (admin only — bulk JSON import of legacy recipes)
```

---

## IronPDF — Cookbook Generation

- Generate a PDF of a user's favorite recipes
- Render an HTML template to PDF using IronPDF's `HtmlToPdf` renderer
- PDF layout: cover page → table of contents → one recipe per page
- Apply the site's majolica aesthetic: cream background, cobalt headers, lemon accents
- Include Cheryl's name and `amatteroftaste.us` in the footer
- Store generated PDFs temporarily, stream to client, then delete

```csharp
// Pattern to follow:
var renderer = new HtmlToPdf();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
return File(pdf.BinaryData, "application/pdf", "MyCookbook.pdf");
```

---

## AI Recipe Parser

- Called **server-side** via `POST /api/ai/parse-recipe` and `POST /api/ai/parse-notes` (admin-only)
- `AiParseService` uses the official Anthropic C# SDK (`Anthropic` NuGet package)
- API key comes from config `Anthropic:ApiKey` — set on the server as the `Anthropic__ApiKey` machine environment variable (never in git, never in the browser); IIS must be restarted (`iisreset`) after changing it
- Model: `claude-sonnet-5`
- User pastes raw recipe text → API proxies to Claude → structured JSON returned → pre-fills editor form
- Expected JSON response shape:

```json
{
  "title": "Sprite Pound Cake",
  "description": "Brief description",
  "servings": 12,
  "preptimeminutes": 20,
  "cooktimeminutes": 75,
  "ingredients": [
    { "quantity": "3", "unit": "cups", "name": "all-purpose flour" }
  ],
  "steps": [
    { "stepnumber": 1, "instruction": "Preheat oven to 325°F..." }
  ]
}
```

- Prompt the model to return **only valid JSON, no markdown, no preamble**
- Parse response safely with try/catch
- If parse fails, show error and leave fields for manual entry

---

## JSON Import

- Legacy data lives at `amatteroftaste.us` as a flat list
- Import endpoint accepts a JSON array of legacy recipes
- Map legacy fields to new schema:
  - `Title` → `recipe.title`
  - `Category` → look up or create `recipecategory`
  - `Description` → `recipe.description`
  - All existing text-block ingredients/steps → store in `recipe.notes` initially
  - Admin can then parse/split them individually via the AI parser
- Import is idempotent — skip duplicates by title match

---

## Pinterest Integration

- No API key required
- Use Pinterest's Save button URL scheme on recipe detail pages:
```
https://pinterest.com/pin/create/button/?url={pageUrl}&media={photoUrl}&description={title}
```
- Opens Pinterest in new tab, user saves from there

---

## Frontend Structure

```
/src
  /components
    /layout        Nav, Footer
    /recipes       RecipeCard, RecipeGrid, RecipeDetail
    /admin         RecipeEditor, IngredientRow, StepRow, AiParser
    /auth          LoginForm, RegisterForm
    /cookbook      MyCookbook, CookbookPage
  /pages
    Home, Browse, RecipeDetail, MyCookbook, Admin, Login, Register
  /api             Typed fetch wrappers for all endpoints
  /hooks           useRecipes, useFavorites, useAuth
  /types           Recipe, Ingredient, Step, User, Category
```

---

## Aesthetic / Design

The site uses a majolica Italian ceramic aesthetic:
- **Background**: white/cream (`#FDFCFA`)
- **Primary blue**: soft vintage china blue (`#4A6090`)
- **Accent**: terracotta (`#D4824A`)
- **Lemon yellow**: butter yellow (`#EDD98A` / `#F2C414`)
- **Nav/Admin rail**: aged parchment (`#D8CBAA`)
- **Fonts**: Pinyon Script (brand), Cinzel (headings/labels), EB Garamond (body)
- **Hero**: Full-width stock photo background (baking tablescape) with dark overlay and centered text

Do NOT use AG Grid — this is a content site, use simple Tailwind-styled tables and card grids.

---

## Solution Structure

```
AMatterOfTaste.sln
├── AMatterOfTaste.Api          (.NET 8 Web API)
│   ├── Controllers
│   ├── Services
│   ├── Models
│   │   ├── Entities
│   │   └── DTOs
│   ├── Data
│   │   └── AppDbContext.cs
│   └── Program.cs
└── amatteroftaste-web          (React TypeScript frontend)
    ├── src/
    ├── public/
    └── package.json
```

---

## Developer Notes

- One-man project, same developer as CharbTech
- Keep it simple and maintainable — this is a personal/family site
- Prefer practical, direct solutions over abstraction
- No over-engineering — straightforward EF Core queries, clean controllers
- When in doubt, match CharbTech patterns
- The AI parser is a nice-to-have — if it fails, manual entry still works perfectly
- Photo storage: local file system under IIS wwwroot/photos initially (same pattern as CharbTech documents)

---

## Seed Data

On first run, seed:
1. Admin user — Cheryl Charbonneau, admin@amatteroftaste.us, `isadmin = true`
2. All 18 recipe categories from the legacy site:
   Main Course, Dessert, Cookies, Breakfast, Side Dishes, Soups, Bread,
   Entertaining, Gifts, Appetizer, Salads, Beverages, Casserole, Dinner,
   Snacks, Condiments, Drinks, Other