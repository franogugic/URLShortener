# URL Shortener Service

## Opis projekta
URL Shortener je servis koji omogućuje korisnicima da skrate dugačke URL-ove, prate njihovu upotrebu i imaju pregled svojih skraćenih linkova. Projekt je implementiran u ASP.NET Core, koristi MySQL za pohranu podataka, Redis za caching i Nginx kao reverse proxy.

Projekt je građen prema **Clean / Onion Architecture** principima, s jasnom separacijom slojeva i testabilnom logikom.

---

## Tehnologije
- Backend: **ASP.NET Core Web API**
- Baza podataka: **MySQL**
- Caching: **Redis / IMemoryCache**
- Web server / reverse proxy: **Nginx**
- Auth: **Cookie-based**
- Docker: za lokalni razvoj i deployment

---

## Arhitektura

### Slojevi (Onion / Clean)
1. **Domain Layer**
    - Entiteti: `User`, `Url`
    - Business rules: generiranje short code, validacija URL-a, provjera custom code-a

2. **Application Layer**
    - Use cases / Services:
        - `CreateShortUrlService`
        - `GetUserUrlsService`
        - `UpdateUrlService`
        - `DeleteUrlService`
        - `LoginUserService`
        - `RegisterUserService`
    - DTOs za input i output

3. **Infrastructure Layer**
    - Database: MySQL + Entity Framework Core
    - Cache: Redis / IMemoryCache
    - Session / Cookie auth

4. **Presentation Layer**
    - REST API: Controllers za Auth, URL management i Redirect
    - Redirect endpoint koristi cache i click tracking

---

## DB struktura

### Users
| Polje         | Tip          | Napomena               |
|---------------|--------------|----------------------|
| id            | BIGINT PK    | auto increment        |
| username      | VARCHAR      | UNIQUE                |
| password_hash | VARCHAR      |                      |
| created_at    | TIMESTAMP    |                      |

### Urls
| Polje           | Tip          | Napomena               |
|-----------------|--------------|----------------------|
| id              | BIGINT PK    | auto increment        |
| short_code      | VARCHAR(50)  | UNIQUE                |
| long_url        | TEXT         |                      |
| description     | VARCHAR(255) | opcionalno            |
| user_id         | BIGINT FK    | references users.id   |
| created_at      | TIMESTAMP    |                      |
| clicks          | INT          | default 0             |

---

## Use caseovi

1. **Register**
    - Kreiranje novog korisnika
    - Endpoint: `POST /auth/register`

2. **Login**
    - Login korisnika, postavljanje session cookie-a
    - Endpoint: `POST /auth/login`

3. **Create short URL**
    - Unos long URL-a i opcionalno custom code / description
    - Endpoint: `POST /shorten`

4. **Get user URLs**
    - Dohvat svih skraćenih URL-ova korisnika
    - Endpoint: `GET /urls`

5. **Edit / Delete URL**
    - Promjena opisa ili custom code-a, brisanje URL-a
    - Endpoint: `PUT /urls/{id}`, `DELETE /urls/{id}`

6. **Click tracking**
    - Svaki redirect se broji u `clicks`
    - Integrirano u `GET /{shortCode}` endpoint

---

## Cross-cutting feature-i

- **Rate limiting / brute force protection**
    - Implementirano na middleware-u i Nginx-u
    - Štiti login, create URL i redirect endpoint
- **Caching**
    - Redis ili IMemoryCache za `shortCode → longUrl` mapping
    - HTTP cache headers opcionalno za browser / CDN
- **Security**
    - HttpOnly, Secure cookies
    - CSRF zaštita za POST requeste
    - HTTPS terminacija preko Nginx
- **Deployment**
    - Docker Compose: API + MySQL + Redis + Nginx

---

## Redirect flow

1. Korisnik klikne short URL: `/abc123`
2. Nginx provjeri cache (HTTP / Redis)
3. Ako miss → backend query MySQL
4. Click tracking (increment `clicks`)
5. HTTP redirect (301/302) na `longUrl`

---

## Opcionalne nadogradnje

- Analytics dashboard (top links, clicks over time)
- Expiry date / deactivation
- Custom short code validation / hints
- Rate limiting per user / per IP
- Logging & monitoring (Serilog / NLog / Grafana)

---

## Kako pokrenuti (lokalno)

1. Clone repozitorij
2. Pokreni Docker Compose (`docker-compose up`)
    - Servisi: API, MySQL, Redis, Nginx
3. Open API na `http://localhost:5000`
4. Testirati endpointove (Postman / Swagger)

---

## Zaključak

Projekt demonstrira:
- REST API s autentikacijom i user history
- URL shortening s custom code i caching
- Click tracking i rate limiting
- Clean / Onion arhitekturu za testabilnost i skalabilnost
- Integraciju Redis-a i Nginx-a za performanse i sigurnost  

