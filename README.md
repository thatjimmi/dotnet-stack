# dotnet-stack

## Overview

`dotnet-stack` is a .NET application. It's designed to showcase the integration of .NET with industry-standard technologies including Nginx as a high-performance web server, Redis for caching, PostgreSQL as a relational database, and leveraging Docker for containerization.

## Features

- **.NET**: Framework for building web apps.
- **Nginx**: A high-performance HTTP server and reverse proxy.
- **Redis**: An in-memory data structure store, used as cache.
- **PostgreSQL**: A powerful, open-source object-relational database system.
- **Docker**: Containerization for consistent deployment and scalability.

## Prerequisites

- .NET 7
- Docker
- Docker Compose

## Getting Started

To get started with `dotnet-stack`, clone this repository to your local machine.

```sh
git clone https://github.com/yourusername/dotnet-stack.git
cd dotnet-stack
```

## Installation
1. Ensure Docker and Docker Compose are installed on your system.
2. Build and run the containers:

```sh
docker-compose up --build
```

### Running Migrations with Entity Framework
Before the application can run, the database schema needs to be created. Here's how to run EF migrations within the Docker environment:

1. Ensure the PostgreSQL container is running as the migrations will need to access the database.
2. Run the following command to execute migrations:
```
docker-compose run --rm app dotnet ef database update
```
The app service should be the name of the service defined in your docker-compose.yml that runs the .NET  application. Replace app with the correct service name if it's different.

## Usage
After launching the containers, the web application will be accessible at http://localhost.

## Application Configuration
Details on configuring each part of the stack:

- Nginx: Configurations for reverse proxy, load balancing, logs, and caching are located in /Nginx/nginx.conf.

  
