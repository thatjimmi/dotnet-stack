# dotnet-core-stack

## Overview

`dotnet-core-stack` is a .NET Core application. It's designed to showcase the integration of .NET Core with industry-standard technologies including Nginx as a high-performance web server, Redis for caching, PostgreSQL as a relational database, and leveraging Docker for containerization.

## Features

- **.NET Core**: A cross-platform .NET implementation for websites, servers, and console apps on Windows, Linux, and macOS.
- **Nginx**: A high-performance HTTP server and reverse proxy.
- **Redis**: An in-memory data structure store, used as cache.
- **PostgreSQL**: A powerful, open-source object-relational database system.
- **Docker**: Containerization for consistent deployment and scalability.

## Prerequisites

- .NET Core 3.1 or later
- Docker
- Docker Compose

## Getting Started

To get started with `dotnet-core-stack`, clone this repository to your local machine.

```sh
git clone https://github.com/yourusername/dotnet-core-stack.git
cd dotnet-core-stack
```

## Installation
1. Ensure Docker and Docker Compose are installed on your system.
2. Build and run the containers:

```sh
docker-compose up --build
```

### Running Migrations with Entity Framework Core
Before the application can run, the database schema needs to be created. Here's how to run EF Core migrations within the Docker environment:

1. Ensure the PostgreSQL container is running as the migrations will need to access the database.
2. Run the following command to execute migrations:
```
docker-compose run --rm app dotnet ef database update
```
The app service should be the name of the service defined in your docker-compose.yml that runs the .NET Core application. Replace app with the correct service name if it's different.

## Usage
After launching the containers, the web application will be accessible at http://localhost.

## Application Configuration
Details on configuring each part of the stack:

Nginx: Configurations for reverse proxy, load balancing, logs, and caching are located in /Nginx/nginx.conf.
PostgreSQL: Database connection strings and credentials are managed in appsettings.json. or enviroment variables in docker compose.
NET Core Application: Additional application settings can be modified in appsettings.Development.json and appsettings.Production.json.
