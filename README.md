# Freemarket FX Basket API



### Overview



This is a RESTful Web API for managing an online shopping basket.

It supports:



* Adding/removing items (single or bulk)
* Discounted items \& discount codes
* Shipping calculation (UK / other countries)
* VAT-inclusive and VAT-exclusive totals
* Health checks (liveness \& readiness)
* API versioning
* Logging via Serilog (Console)
* OpenAPI/Swagger documentation



The solution is built with .NET 9 and uses an in-memory store for simplicity.



### Project Structure



Basket.Api/

├── Controllers/      # API controllers

├── Dtos/             # Request \& Response DTOs

├── MemoryStore/      # In-memory basket storage

├── Models/           # Domain models (Basket, BasketLine, BasketTotals)

├── Services/         # Business logic

├── Mapping/          # DTO mappers

├── Program.cs        # App startup \& configuration

└── appsettings.json  # Configuration (Serilog, allowed hosts, etc.)



### Features



##### Feature		Endpoint Example

Create basket		 POST /api/v1/basket

Add item		 POST /api/v1/basket/{basketId}/items

Add multiple items	 POST /api/v1/basket/{basketId}/items/bulk

Remove item		 DELETE /api/v1/basket/{basketId}/items/{lineId}

Apply discount code	 POST /api/v1/basket/{basketId}/discount

Set shipping		 POST /api/v1/basket/{basketId}/shipping

Get totals		 GET /api/v1/basket/{basketId}/total?includeVat=true



* VAT is calculated at 20% (configurable in code).
* Shipping is free in the UK for baskets ≥ £50; £5 otherwise. Flat £15 for other countries.
* Discount codes only apply to non-discounted items.



### Setup \& Run



##### Prerequisites

* .NET 9 SDK
* Optional: Visual Studio 2026 / VS Code



##### 1\. Clone Repository



git clone https://github.com/gquinn1998/Freemarket-FX.git

cd Basket.Api



##### 2\. Restore Packages



dotnet restore



##### 3\. Build



dotnet build



##### 4\. Run



dotnet run



* API will be available at https://localhost:7214 (HTTPS) or http://localhost:5000 (HTTP)
* Swagger UI: https://localhost:7214/swagger/index.html



### Testing the API



Use Swagger UI or tools like Postman or curl:



\# Create a new basket

POST https://localhost:7214/api/v1/basket



\# Add an item

POST https://localhost:7214/api/v1/basket/{basketId}/items

{

&nbsp; "sku": "ITEM123",

&nbsp; "name": "Test Product",

&nbsp; "unitPrice": 10.50,

&nbsp; "quantity": 2

}



\# Get basket totals

GET https://localhost:7214/api/v1/basket/{basketId}/total?includeVat=true



### Configuration



* Serilog logging writes to the Console.
* Health checks available at:

&nbsp;	/health/live — liveness

&nbsp;	/health/ready — readiness

* Error handling: global /error endpoint (hidden from Swagger).











