E-Commerce Platform (ASP.NET, MongoDB, RabbitMQ, MCP)
Overview

This project is a backend e-commerce platform built with ASP.NET (C#).
It uses MongoDB for data persistence, RabbitMQ for asynchronous messaging between services, and an MCP (Model Context Protocol) server to expose tools to an LLM so the system can dynamically decide which backend operation to execute based on user requests.

The architecture is designed to be scalable, extensible, and suitable for modern AI-assisted workflows.

Key Features

🛒 Core e-commerce functionality (users, products, orders)

📦 MongoDB as the primary database

🔁 RabbitMQ for event-driven and cross-service communication

🤖 MCP server integration for LLM tool exposure

⚙️ ASP.NET backend written in C#

🔌 Extensible tool-based execution model for AI agents

Tech Stack
Backend Framework: ASP.NET (.NET)
Language: C#
Database: MongoDB
Message Broker: RabbitMQ
AI Integration: MCP Server (Model Context Protocol)
Architecture: Service-oriented / Event-driven
