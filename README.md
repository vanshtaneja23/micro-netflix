# Micro-Netflix 🎥

A distributed video transcoding service built with **.NET 8**, **RabbitMQ**, **MinIO**, and **PostgreSQL**.

## 🚀 Architecture

The system follows a **Microservices** architecture:

1.  **API (`MicroNetflix.Api`)**:
    *   Accepts video uploads via `POST /videos/upload`.
    *   Stores raw video in **MinIO** (S3 compatible storage).
    *   Saves metadata to **PostgreSQL**.
    *   Publishes a `VideoUploadedEvent` to **RabbitMQ**.

2.  **Worker (`MicroNetflix.Worker`)**:
    *   Listens for `VideoUploadedEvent`.
    *   Downloads the video from MinIO.
    *   Simulates transcoding (processing).
    *   Uploads the "processed" video back to MinIO.

## 🛠️ Tech Stack

*   **Framework**: .NET 8 (ASP.NET Core, Worker Service)
*   **Language**: C#
*   **Database**: PostgreSQL (via EF Core)
*   **Messaging**: RabbitMQ (via MassTransit)
*   **Storage**: MinIO
*   **Containerization**: Docker & Docker Compose

## 🏃‍♂️ How to Run

### Prerequisites
*   Docker Desktop
*   .NET 8 SDK

### Steps

1.  **Start Everything**:
    Since you have Docker, you can run the entire stack (API, Worker, DB, Queue) with one command:
    ```bash
    docker-compose up -d --build
    ```

2.  **Test it**:
    *   The API will be available at `http://localhost:5000`
    *   Open Swagger: `http://localhost:5000/swagger`
    *   Upload a video file.
    *   Watch the Worker logs: `docker-compose logs -f worker`

## 🧪 Verification

You can use the included `verify.sh` script to test the flow automatically.

```bash
chmod +x verify.sh
./verify.sh
```
