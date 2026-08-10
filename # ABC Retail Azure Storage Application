# ABC Retail Azure Storage Application

ABC Retail Azure Storage Application is an ASP.NET Core MVC web application developed to demonstrate the use of various Microsoft Azure Storage Services in a retail environment.

The application is designed to address the storage, scalability, reliability, and processing challenges faced by ABC Retail as the company continues to grow.

## Project Overview

ABC Retail currently experiences challenges with its traditional on-premises infrastructure, including slow access to product images, difficulties handling increasing transaction volumes, unreliable message processing, and inefficient storage of application files.

This project provides a cloud-based solution using Microsoft Azure services.

## Technologies Used

- C#
- ASP.NET Core MVC
- Microsoft Azure
- Azure Table Storage
- Azure Blob Storage
- Azure Queue Storage
- Azure Files
- Azure App Service
- Bootstrap
- HTML
- CSS

## Azure Services

### Azure Table Storage

Azure Table Storage is used to store structured application data, including:

- Customer profiles
- Product information

### Azure Blob Storage

Azure Blob Storage is used to store:

- Product images
- Multimedia content

The application allows product images to be uploaded and displayed directly from Azure Blob Storage.

### Azure Queue Storage

Azure Queue Storage is used to support asynchronous processing of:

- Customer orders
- Inventory updates
- Order processing messages

### Azure Files

Azure Files is used to store application-related files such as:

- Log files
- Order processing logs
- Inventory logs

### Azure App Service

The completed ASP.NET Core MVC application will be deployed to Azure App Service so that it can be accessed online through a public URL.

## Main Features

- Create, view, edit, and delete customer profiles
- Create, view, edit, and delete products
- Upload product images to Azure Blob Storage
- Display uploaded product images within the website
- Submit order-processing messages to Azure Queue Storage
- Submit inventory-processing messages to Azure Queue Storage
- Store application log files in Azure Files
- View and download files stored in Azure Files
- Deploy the application to Azure App Service

## Project Architecture

```text
ASP.NET Core MVC Web Application
            |
            |
    ---------------------
    |        |          |
    v        v          v
 Azure    Azure       Azure
 Tables   Blobs       Queues
    |        |          |
Customers  Product    Orders
Products   Images     Inventory
    |
    |
 Azure Files
    |
 Log Files

            |
            v
    Azure App Service

Assignment Requirements

The application will demonstrate the following:

Customer and product information stored in Azure Table Storage
Images and multimedia stored in Azure Blob Storage
Order and inventory transactions stored in Azure Queue Storage
Log files stored using Azure Files
Appropriate web controls for uploading, downloading, and displaying Azure Storage data
Successful deployment to Azure App Service
