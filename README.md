# Micro-Netflix 🎥

A distributed video streaming platform built with **.NET 8**, **Microservices**, and **Event-Driven Architecture**.

## 🚀 Features
*   **Microservices Architecture**: Separate API, Worker, and Frontend services.
*   **Event-Driven**: Uses **RabbitMQ** for asynchronous communication.
*   **Scalable Storage**: Uses **MinIO** (S3-compatible) for video storage.
*   **Background Processing**: Dedicated worker service for video transcoding simulation.
*   **Real-time Updates**: Frontend polls for status changes (Pending -> Completed).
*   **Video Streaming**: Stream processed videos directly in the browser.
*   **Modern UI**: Dark-themed, responsive frontend.

## 🛠️ Tech Stack
*   **Backend**: .NET 8 (C#), ASP.NET Core, Worker Service
*   **Frontend**: HTML5, CSS3, JavaScript, Nginx
*   **Database**: PostgreSQL (Entity Framework Core)
*   **Message Broker**: RabbitMQ (MassTransit)
*   **Object Storage**: MinIO
*   **Containerization**: Docker & Docker Compose

## 🏗️ Architecture
The system consists of the following containers:
1.  **API**: Receives uploads and serves streams.
2.  **Worker**: Processes videos in the background.
3.  **Frontend**: User interface.
4.  **Postgres**: Stores metadata.
5.  **RabbitMQ**: Handles messaging.
6.  **MinIO**: Stores video files.

## 🏃‍♂️ Getting Started

### Prerequisites
*   Docker Desktop

### Run Locally
1.  Clone the repository:
    ```bash
    git clone https://github.com/yourusername/micro-netflix.git
    ```
2.  Navigate to the project folder:
    ```bash
    cd micro-netflix
    ```
3.  Start the application:
    ```bash
    docker-compose up -d --build
    ```
4.  Open your browser:
    *   **Frontend**: [http://localhost:8080](http://localhost:8080)
    *   **API Swagger**: [http://localhost:5001/swagger](http://localhost:5001/swagger)
    *   **MinIO Console**: [http://localhost:9001](http://localhost:9001)

## 🧪 How to Use
1.  Go to the **Frontend** (`http://localhost:8080`).
2.  Click **"Choose File"** and upload a video.
3.  Wait for the status to change from **Pending** (Orange) to **Completed** (Green).
4.  Click **"▶ Play"** to watch your video!

## ☁️ Deployment
This project is containerized and ready for deployment on platforms like **Render**, **AWS**, or **Azure**.

---
*Built by Vansh Taneja*
