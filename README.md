# JWT Authentication and Authorization in .NET 8.0 Core

## Overview

This project implements user authentication and role-based authorization using JSON Web Tokens (JWT) in a .NET 8.0 Core web application. The goal is to provide secure access to API endpoints based on user roles without relying on built-in .NET functionalities.

## Features

- **JWT Token Generation and Validation**: Securely generate and validate JWT tokens for user authentication.
- **Role-Based Authorization**: Protect specific API endpoints to ensure they are accessible only to users with defined roles (e.g., Admin, User).
- **Custom Authentication Handler**: Implement a custom authentication handler for handling JWT authentication.
- **Middleware for Token Validation**: Middleware to validate JWT tokens in incoming requests.

## Key Components

### 1. JWT Token Generation and Validation

- **Token Generation**: A token is generated upon successful user login and is included in the response.
- **Token Validation**: The token is validated for each request to protected endpoints to ensure its integrity and authenticity.

### 2. Role-Based Authorization

- Roles such as Admin and User are defined and assigned to users.
- Specific API endpoints are protected using custom attributes that check for the necessary roles.

### 3. Configuration in Program.cs

The following configurations are used for setting up JWT authentication and role-based authorization:

- **Service Registration**: Register required service extentions including controllers, Swagger, database context, dependency injection, and JWT authentication.
- **Middleware Setup**: Configure the middleware pipeline to handle errors, seed the database, apply JWT authentication, and set up Swagger documentation.

### Endpoints

**POST** `/api/auth/login`  
This endpoint allows users to log in by providing their username and password. If the credentials are valid, a JWT token is returned.

#### Request

- **Method**: POST
- **URL**: `/api/auth/login`
- **Body**:

  ```json
  {
    "username": "yourusername",
    "password": "yourpassword"
  }
  ```

#### Response

- **Success (200 OK)**:

  ```json
  {
    "token": "your.jwt.token"
  }
  ```

- **Unauthorized (401 Unauthorized)**:

  ```json
  {
    "title": "Unauthorized",
    "status": 401
  }
  ```

---

**GET** `/api/auth/admin-endpoint`  
This endpoint is accessible only by users with the `Admin` role.

#### Response

- **Success (200 OK)**:

  ```json
  {
    "message": "Admin access granted."
  }
  ```

- **Unauthorized (401 Unauthorized, 403 Forbidden)**

---

**GET** `/api/auth/admin-or-user-endpoint`  
This endpoint is accessible by users with either the `Admin` or `User` role.

#### Response

- **Success (200 OK)**:

  ```json
  {
    "message": "Admin Or User access granted."
  }
  ```

- **Unauthorized (401 Unauthorized, 403 Forbidden)**

---

**GET** `/api/auth/user-endpoint`  
This endpoint is accessible only by users with the `User` role.

#### Response

- **Success (200 OK)**:

  ```json
  {
    "message": "User access granted."
  }
  ```

- **Unauthorized (401 Unauthorized, 403 Forbidden)**

### Configure the Environment

1. **Set the JWT Secret Key**  
    The application uses an environment variable for the JWT secret key. You can configure this using the `setx` command on Windows.

   - Run the following command in your command prompt:

     ```bash
     setx AppSettings__Token "your-secret-key"
     ```

   After setting it, restart your command prompt or system for the changes to take effect. This key will be used to sign and validate JWT tokens.

2. **Database Connection Strings**  
   Change your database connection in `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DevDB": "YourDatabaseConnectionString"
     }
   }
   ```

3. **Apply the database migrations**

   ```sh
   dotnet ef database update
   ```

### Usage

To run the project, use the following command: **dotnet run**

### Seeded User Credentials

The application comes with pre-seeded users for testing purposes. You can log in using the following credentials:

- **Username**: `admin`
- **Password**: `password`

- **Username**: `user`
- **Password**: `password`

You can use these credentials to access the different API endpoints after starting the application.

### API Documentation

The application uses Swagger for API documentation. Once the application is running, you can access the Swagger UI at the following address:

- **Swagger URL**: [http://localhost:5088/swagger/index.html](http://localhost:5088/swagger/index.html)

### Testing the API

You can test the API using Swagger or any API testing tool such as Postman.

#### Using Swagger

1. Run the application.
2. Open your browser and navigate to [http://localhost:5088/swagger/index.html](http://localhost:5088/swagger/index.html).
3. Use the Swagger UI to explore and test the API endpoints.
4. Open your browser and navigate to [Example swagger usage](https://drive.google.com/file/d/1zOx5n3BofZh5oyEAlYdue0L47kwckyqB/view?usp=sharing).

#### Using Postman or Other Tools

If you prefer to use Postman or other tools, follow these steps:

1. Open Postman (or any API tool).
2. Use the **POST** method to authenticate at the `/login` endpoint:

   - **URL**: `http://localhost:5088/api/auth/login`
   - **Body**:

     ```json
     {
       "username": "admin",
       "password": "password"
     }
     ```

3. Copy the returned JWT token from the response.

4. For protected endpoints (e.g., `GET /admin-endpoint`), set the `Authorization` header:

   - **Header**: `Authorization: Bearer your.jwt.token`

5. Send the request to the desired endpoint.
