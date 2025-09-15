E-Commerce Platform

A modern e-commerce backend built with .NET providing authentication, authorization, CRUD operations, RabbitMQ event-driven communication, and pagination support.

🚀 Features

Authentication & Authorization

User registration and login with JWT-based authentication.

Role-based access control (e.g., Admin, Customer).

CRUD Operations

Manage Products, Categories, Orders, Users.

Full Create, Read, Update, Delete support.

RabbitMQ Integration

Event-driven architecture for decoupled services.

Publishes domain events (e.g., OrderCreated, ProductUpdated).

Consumers listen and process messages asynchronously.

Pagination & Filtering

Efficient data retrieval with page size, page number, and filters.

Scalable Architecture

Layered design: Controller → Service → Repository → Database.

Configurable settings via appsettings.json.
