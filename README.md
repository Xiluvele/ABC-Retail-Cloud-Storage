# ABC Retail – Azure Cloud Storage and Functions

## Project Overview

ABC Retail is an ASP.NET Core MVC web application developed to demonstrate how Microsoft Azure services can be integrated into a web application.

The system manages customers, products, orders and inventory while using Azure cloud services for storing data, images, queue messages and log files. Azure Functions are also used to handle some of the application's processing and storage operations.

This project was developed as part of my cloud development coursework and focuses on building a cloud-based application using different Azure services.

## Technologies Used

- ASP.NET Core MVC
- C#
- .NET 8
- Azure Functions
- Azure Table Storage
- Azure Blob Storage
- Azure Queue Storage
- Azure Files
- Azure App Service
- HTML
- CSS
- Bootstrap
- GitHub Actions

## Azure Services

### Azure Table Storage

Azure Table Storage is used to store customer and product information. The application can create, retrieve, update and delete records stored in Azure Tables.

### Azure Blob Storage

Azure Blob Storage is used to store product images. When a product image is uploaded, the image is stored in the `product-images` blob container and the application uses the stored image reference to display it.

### Azure Queue Storage

Azure Queue Storage is used for order and inventory processing.

The application uses the following queues:

- `order-processing` – stores messages related to customer orders.
- `inventory-processing` – stores messages related to inventory transactions.

### Azure Files

Azure Files is used to store log files generated when orders and inventory transactions are processed. These files can also be viewed and downloaded through the web application.

### Azure Functions

A separate Azure Functions project is included in the solution to handle cloud operations.

The following functions are implemented:

- `StoreCustomer` – stores customer information in Azure Table Storage.
- `UploadProductImage` – uploads product images to Azure Blob Storage.
- `ProcessOrder` – processes order information and sends messages to Azure Queue Storage.
- `ProcessInventory` – processes inventory transactions and sends messages to Azure Queue Storage.
- `UploadFile` – stores transaction log files in Azure Files.

## Application Features

The ABC Retail application allows users to:

- Manage customer information.
- Create and manage products.
- Upload and display product images.
- Process customer orders.
- Submit inventory transactions.
- Store order and inventory messages in Azure Queue Storage.
- Store transaction logs in Azure Files.
- View and download stored log files.

## Project Structure

The solution contains two main projects:

```text
ABCRetail
│
├── ABCRetail.csproj
├── ABCRetail.sln
│
├── Controllers
├── Models
├── Services
├── Views
│
└── ABCRetail.Functions
    ├── ABCRetail.Functions.csproj
    ├── StoreCustomer.cs
    ├── UploadProductImage.cs
    ├── ProcessOrder.cs
    ├── ProcessInventory.cs
    └── UploadFile.cs
