Food Ordering System

Student Information

Name: Dilek Shakir

Faculty Number: 2301321079

Specialty: Software Engineering (СИ)

Course: 3rd year

Project Description

Food Ordering System is a software system for ordering food from restaurants.

The system consists of two connected parts:

Backend – ASP.NET Core Web API

Frontend – ASP.NET Core Razor Pages

The backend provides RESTful services for managing users, restaurants, menu items, orders and order items.

The frontend provides a web interface through which users can view, create, edit and delete data.

The application uses a SQL Server LocalDB database.

Technologies

Backend

C#

ASP.NET Core Web API

.NET 8

Entity Framework Core

SQL Server LocalDB

REST API

JWT Authentication

Swagger / OpenAPI

Frontend

ASP.NET Core Razor Pages

C#

HTML

CSS

Bootstrap

JavaScript

Database

Microsoft SQL Server LocalDB

System Architecture

The system follows a client-server architecture.

+---------------------------+
|     Web Frontend          |
| ASP.NET Core Razor Pages  |
+------------+--------------+
             |
             | HTTP / REST
             |
             v
+---------------------------+
|      Backend API          |
| ASP.NET Core Web API      |
+------------+--------------+
             |
             | Entity Framework Core
             |
             v
+---------------------------+
|       SQL Server          |
|        LocalDB            |
+---------------------------+

The frontend communicates with the backend through HTTP requests.

The backend communicates with the SQL Server database using Entity Framework Core.

Main Features

The system provides CRUD operations for the following entities:

Users

Restaurants

Menu Items

Orders

Order Items

Users

Users can be:

created

viewed

edited

deleted

searched by first name and last name

sorted

displayed using pagination

Restaurants

Restaurants can be:

created

viewed

edited

deleted

searched

sorted

displayed using pagination

Menu Items

Menu items can be:

created

viewed

edited

deleted

searched

sorted

displayed using pagination

Orders

Orders can be:

created

viewed

edited

deleted

searched

sorted

displayed using pagination

Order Items

Order items can be:

created

viewed

edited

deleted

searched by order and menu item

sorted

displayed using pagination

Database

The database contains five logically related tables:

Users

Restaurants

MenuItems

Orders

OrderItems

Database Relationships

Users
  |
  | 1 : N
  v
Orders
  |
  | 1 : N
  v
OrderItems
  ^
  |
  | N : 1
  |
MenuItems
  ^
  |
  | N : 1
  |
Restaurants

The relationships between the tables are implemented using foreign keys.

One user can have many orders.

One order can contain many order items.

One menu item can be used in many order items.

One restaurant can have many menu items.

Database Constraints

The database models contain:

Primary keys

Foreign keys

Required fields

Maximum text lengths

Numeric ranges

Decimal precision

Validation rules

Foreign key relationships prevent invalid references between related entities.

Backend API

The backend is implemented using ASP.NET Core Web API.

Main API controllers:

/api/Users
/api/Restaurants
/api/MenuItems
/api/Orders
/api/OrderItems
/api/Login

Each main entity supports CRUD operations.

The API uses asynchronous operations with async / await for database operations.

Authentication and Security

The system uses JWT authentication.

Users authenticate through the login endpoint:

POST /api/Login

After successful authentication, the API returns a JWT token.

Protected API endpoints require the following authorization header:

Authorization: Bearer <token>

Passwords are stored using password hashing instead of storing plain-text passwords.

The frontend stores the JWT token in the session and uses it when communicating with protected API endpoints.

Unauthorized users cannot access protected functionality.

Validation

Validation is implemented using ASP.NET Core Data Annotations.

Examples include:

Required fields

Maximum string lengths

Email validation

Minimum password length

Numeric ranges

Valid foreign key references

Invalid data is rejected by the application before being stored in the database.

The frontend also performs validation before sending data to the backend.

Search, Pagination and Sorting

The API supports searching using multiple criteria.

For example, users can be searched by first name and last name:

/api/Users?firstName=Dilek&lastName=Shakir

Order items can be searched by order and menu item:

/api/OrderItems?orderId=2&menuItemId=2

List endpoints support pagination:

?page=1&pageSize=5

Sorting is supported using the sortBy parameter:

?sortBy=name

The frontend provides controls for searching, sorting and pagination.

Error Handling

The backend uses global exception handling.

Unexpected server errors are returned using the standardized Problem Details format.

Example:

{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Възникна неочаквана грешка. Моля, опитайте отново.",
  "instance": "/api/Restaurants"
}

This provides a consistent format for API errors.

Project Structure

FoodOrderingSystem
│
├── FoodOrderingSystem.API
│   ├── Controllers
│   ├── Data
│   ├── DTOs
│   ├── Models
│   ├── Services
│   ├── Migrations
│   ├── Program.cs
│   └── appsettings.json
│
├── FoodOrderingSystem.Web
│   ├── Pages
│   ├── wwwroot
│   ├── Program.cs
│   └── appsettings.json
│
├── FoodOrderingSystem.sln
└── README.md

Installation

Requirements

The following software is required:

Visual Studio 2022

.NET 8 SDK

SQL Server LocalDB

Step 1 – Open the Solution

Open the following file in Visual Studio 2022:

FoodOrderingSystem.sln

Step 2 – Restore NuGet Packages

Visual Studio will restore the required NuGet packages automatically.

The backend uses packages for:

Entity Framework Core

SQL Server

JWT authentication

Password hashing

Swagger / OpenAPI

Step 3 – Configure the Database

The application uses SQL Server LocalDB.

The connection string is:

Server=(localdb)\MSSQLLocalDB;Database=FoodOrderingSystemDb;Trusted_Connection=True;TrustServerCertificate=True;

Step 4 – Create the Database

Open:

Tools → NuGet Package Manager → Package Manager Console

Run:

Update-Database

This creates the database and the required tables.

How to Run

The solution contains two projects:

FoodOrderingSystem.API
FoodOrderingSystem.Web

Both projects should be configured as startup projects.

The API provides the backend REST services.

The Web project provides the user interface.

API

The backend API runs on:

https://localhost:7297

Swagger is available at:

https://localhost:7297/swagger

Swagger can be used to view and test the REST API endpoints.

Web Application

The frontend application runs on:

https://localhost:7029

Open the Web application in a browser to use the system.

Login

The application provides a login page.

Users can log in using their email address and password.

After successful login, the user receives access to protected functionality such as:

Menu Items

Orders

Order Items

Users

The Logout functionality removes the authentication session and returns the user to the Login page.

API Endpoints

Authentication

POST /api/Login

Users

GET    /api/Users
GET    /api/Users/{id}
POST   /api/Users
PUT    /api/Users/{id}
DELETE /api/Users/{id}

Restaurants

GET    /api/Restaurants
GET    /api/Restaurants/{id}
POST   /api/Restaurants
PUT    /api/Restaurants/{id}
DELETE /api/Restaurants/{id}

Menu Items

GET    /api/MenuItems
GET    /api/MenuItems/{id}
POST   /api/MenuItems
PUT    /api/MenuItems/{id}
DELETE /api/MenuItems/{id}

Orders

GET    /api/Orders
GET    /api/Orders/{id}
POST   /api/Orders
PUT    /api/Orders/{id}
DELETE /api/Orders/{id}

Order Items

GET    /api/OrderItems
GET    /api/OrderItems/{id}
POST   /api/OrderItems
PUT    /api/OrderItems/{id}
DELETE /api/OrderItems/{id}

CRUD Operations

The application implements full CRUD functionality for all database entities.

Create

New records can be created through the frontend and backend API.

Read

Records can be retrieved individually or as paginated lists.

Update

Existing records can be modified through the frontend and backend API.

Delete

Existing records can be deleted through the frontend and backend API.

CRUD functionality is implemented for:

Users

Restaurants

Menu Items

Orders

Order Items

Asynchronous Operations

Database operations are implemented using asynchronous programming.

The project uses:

async
await

with Entity Framework Core methods such as:

ToListAsync()
FirstOrDefaultAsync()
FindAsync()
AnyAsync()
SaveChangesAsync()

This prevents blocking during database operations.

RESTful Communication

The frontend communicates with the backend using HTTP requests.

The main HTTP methods used are:

GET
POST
PUT
DELETE

The API returns appropriate HTTP status codes such as:

200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
404 Not Found
500 Internal Server Error

The frontend processes the responses from the backend and displays appropriate messages to the user.

Frontend

The frontend is implemented using ASP.NET Core Razor Pages.

The application provides separate pages for managing:

Users

Restaurants

Menu Items

Orders

Order Items

Login

Logout

The interface provides forms for creating and editing records.

The application also provides:

Search

Sorting

Pagination

Validation messages

Success messages

Error messages

Authentication

Backend Services

The backend is organized into several components:

Controllers

Controllers handle HTTP requests and provide RESTful API endpoints.

Models

Models represent the database entities:

User
Restaurant
MenuItem
Order
OrderItem

DTOs

Data Transfer Objects are used for communication between the client and API.

Data

The ApplicationDbContext class manages database access through Entity Framework Core.

Services

Services contain supporting functionality such as password hashing and global exception handling.

Entity Relationships

The main entity relationships are:

Restaurant
    |
    | 1 : N
    v
MenuItem

User
    |
    | 1 : N
    v
Order
    |
    | 1 : N
    v
OrderItem
    ^
    |
    | N : 1
    |
MenuItem

These relationships allow the system to represent restaurants, their menu items, customers, orders and the individual items contained in each order.

Security Features

The application includes several security mechanisms:

JWT authentication

Authorization using [Authorize]

Anonymous access only where required, such as login and user registration

Password hashing

Session-based storage of the JWT token on the frontend

Validation of user input

Foreign key validation

HTTPS communication

Testing

The main functionality of the application has been tested through the frontend and Swagger API.

Tested functionality includes:

User CRUD operations

Restaurant CRUD operations

Menu Item CRUD operations

Order CRUD operations

Order Item CRUD operations

Login

Logout

JWT authentication

Search

Pagination

Sorting

Validation

Foreign key validation

Global exception handling

Database operations

Conclusion

The Food Ordering System demonstrates a complete web application consisting of a RESTful backend, web frontend and relational database.

The project implements:

REST API

CRUD operations

SQL Server database

Entity Framework Core

JWT authentication

Password hashing

Validation

Search

Pagination

Sorting

Global exception handling

Asynchronous database operations

Razor Pages frontend

Client-server communication

The system is designed as a university coursework project for the Software Engineering program.

Author

Dilek Shakir

Faculty Number: 2301321079

Specialty: Software Engineering (СИ)

Course: 3rd year