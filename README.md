# Megatlon Payments – Challenge .NET

API REST para registrar y consultar pagos. Arquitectura en capas, validaciones con **FluentValidation**, reglas de negocio **configurables** y **tests de integración**.

---

## Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core 8** + **SQLite**
- **FluentValidation**
- **xUnit + WebApplicationFactory** (tests)
- **Swagger** para exploración

---

## Arquitectura (solución)

```
 ├─ Megatlon.Payments.Api               → capa de presentación (controllers, Swagger)
 ├─ Megatlon.Payments.Application       → contratos, validadores, motor de reglas
 ├─ Megatlon.Payments.Domain            → entidades del dominio
 ├─ Megatlon.Payments.Infrastructure    → EF Core (DbContext, mappings, migrations)
 └─ Megatlon.Payments.Api.Tests         → tests de integración (xUnit)
```

---

## Carpetas / archivos clave

- `Infrastructure/Persistence/PaymentsDbContext.cs`  
  Mapeos, **idempotencia** (`Source + ExternalReference` única), **seed** (monedas/medios) y columna `CardBin` (nullable, 6 dígitos + CHECK).
- `Application/Validation/RegistrarPagoValidator.cs`  
  Validación de entrada.
- `Application/Rules/*`  
  **Motor de reglas** (`IPaymentRule`, `IPaymentRuleEngine` + reglas).
- `Api/Rules/PaymentRulesConfig.cs`  
  Lee reglas desde configuración.
- `Api/rulesconfiguration.json`  
  **Reglas configurables** (hot reload).
- `Api/Middleware/RequestResponseLoggingMiddleware.cs`  
  Log JSON de **REQ/RESP** (una línea por cada uno) en `logs/pagos.txt` dentro de la carpeta destino donde se vuelca el compilado (por ejemplo Megatlon.Payments.Api\bin\Debug\net8.0\logs\).

---

## Requisitos

- Visual Studio 2022 o `dotnet SDK 8.0+`
- **SQLite** (embebido, sin servidor)

---

## Puesta en marcha

1. **Clonar & restaurar**
   ```bash
   git clone <repo>
   cd <repo>
   dotnet restore
   ```

2. **Base incluida en el repo**  
   Se versiona **`app.db`** (esquema + seed).  

3. **Connection string** (ya incluida)
   ```json
   // src/Megatlon.Payments.Api/appsettings.json
   {
     "ConnectionStrings": { "Default": "Data Source=app.db" }
   }
   ```

4. **Reglas configurables**  
   `src/Megatlon.Payments.Api/rulesconfiguration.json` (hot reload):
   ```json
   {
     "PaymentRules": {
       "CASH": {
         "Min": 3000000000,
         "Max": null,
         "AllowedCurrencies": [ "ARS", "USD", "UYU" ]
       },
       "CARD": {
         "Min": 3000000000,
         "Max": 20000000000,
         "AllowedCurrencies": [ "ARS", "USD" ],
         "AllowedBins": [ "411111", "522222" ]
       },
       "CHEQUE": {
         "Min": 3000000000,
         "Max": 5000000000,
         "AllowedCurrencies": [ "ARS" ]
       }
     }
   }
   ```
   > En propiedades de `app.db` y `rulesconfiguration.json`: **Copy to Output Directory → Copy if newer**.

5. **Correr la API**
   ```bash
   dotnet run --project src/Megatlon.Payments.Api
   ```
   - Swagger: `https://localhost:<puerto>/swagger`

   > En `Program.cs` se ejecuta `db.Database.Migrate()` al iniciar para aplicar migraciones pendientes automáticamente.

---

## Endpoints

### POST `/api/pagos`
Registra un pago.

**Request ejemplo**
```json
{
  "cliente": { "nombre": "Juan Perez", "email": "juan@megatlon.com" },
  "monto": 3000000000,
  "fechaPago": "2025-08-31T15:00:00Z",
  "medioPagoCode": "CASH",        // CASH | CARD | CHEQUE
  "monedaISOCode": "ARS",         // ARS | USD | UYU
  "externalReference": "op-100",
  "source": "megatlon",
  "cardBin": "411111"             // opcional
}
```

**200 OK**
```json
{ "success": true, "message": "Pago registrado correctamente", "data": { "pagoId": "..." }, "errors": null }
```

**200 OK (idempotencia)**
```json
{ "success": true, "message": "Pago ya registrado", "data": { "pagoId": "..." }, "errors": null }
```

**400 BadRequest (validaciones/reglas)**
```json
{ "success": false, "message": "Reglas de negocio no satisfechas", "data": null, "errors": ["..."] }
```

---

### GET `/api/pagos?source={source}&externalReference={externalReference}`
Consulta un pago por clave de idempotencia.  
Incluye datos de cliente, medio y moneda.

---

## Reglas de negocio (engine flexible)

- **Interfaz**: `IPaymentRule`
- **Engine**: `IPaymentRuleEngine` (ejecuta todas las reglas registradas)
- **Reglas incluidas**:
  - `MinMaxAmountRule`
  - `AllowedCurrenciesRule`
  - `AllowedBinsRule`
- **Provider**: `IPaymentRulesConfig` (implementado por `PaymentRulesConfig`) que lee `rulesconfiguration.json`.

**Agregar una regla nueva**
1. Crear `sealed class NuevaRegla : IPaymentRule` (en *Application/Rules*).
2. Registrar en `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IPaymentRule, NuevaRegla>();
   ```
3. Si requiere configuración, extender `IPaymentRulesConfig` y su implementación.

---

## Seed de datos

**Monedas**
- ARS · “Peso Argentino”
- USD · “Dólar”
- UYU · “Peso Uruguayo”

**Medios de pago**
- CASH · Efectivo
- CARD · Tarjeta
- CHEQUE · Cheque

---

## Logging

Middleware `RequestResponseLoggingMiddleware` → escribe **una línea** por request y otra por response en `logs/pagos.txt`, separadas por:
```
------------------------------
```

Formato:
```
  [UTC yyyy-MM-dd HH:mm:ss.fffZ] METHOD PATH?query
  REQ:{...}
  RESP:{status}:{...}
------------------------------
```

---

## Idempotencia

- Índice **único** en `Pago` por `(Source, ExternalReference)`.
- Verificación lógica en controller + constraint en DB.

---

## Tests de integración

Proyecto: `tests/Megatlon.Payments.Api.Tests` (xUnit).

- Base **SQLite en memoria** (migraciones aplicadas en TestServer).
- Reglas **stub** (no dependen del JSON real).
- Casos:
  - ✅ Happy path (CASH, ARS, mínimo aceptado)
  - ❌ Falla por mínimo (CASH)
  - ❌ Falla por máximo (CARD)
  - ❌ Falla por moneda no habilitada (CHEQUE + USD)
  - 🔁 Idempotencia (mismo `Source + ExternalReference`)

**Ejecutar**
```bash
dotnet test
# por clase/método:
dotnet test --filter "FullyQualifiedName~PagosTests"
dotnet test --filter "Name~Idempotencia_MismoSourceYExternalReference"
```

---

## Notas SQLite

- En modo WAL, SQLite crea `app.db-wal` y `app.db-shm` (no versionar).
- Para volcar y truncar WAL:
  ```csharp
  db.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(TRUNCATE);");
  ```

---

## Comandos EF útiles

**PMC**
```
Add-Migration <Nombre> -Project Megatlon.Payments.Infrastructure -StartupProject Megatlon.Payments.Api
Update-Database -Project Megatlon.Payments.Infrastructure -StartupProject Megatlon.Payments.Api
```

**CLI**
```bash
dotnet ef migrations add <Nombre>   --project src/Megatlon.Payments.Infrastructure   --startup-project src/Megatlon.Payments.Api

dotnet ef database update   --project src/Megatlon.Payments.Infrastructure   --startup-project src/Megatlon.Payments.Api
```

---

## Estado del challenge

- [x] API .NET 8 con Swagger  
- [x] EF Core + SQLite (seed incluido)  
- [x] Idempotencia (Source + ExternalReference)
- [x] Validaciones (FluentValidation)  
- [x] Motor de reglas **configurable** con *hot reload*  
- [x] Logging de REQ/RESP  
- [x] **Tests de integración** (5 escenarios clave)

> Cualquier ajuste fino (nuevas reglas, más validaciones o reportes), se puede incorporar sin afectar la arquitectura actual.
