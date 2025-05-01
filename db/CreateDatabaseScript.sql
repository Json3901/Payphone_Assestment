USE MASTER
GO

CREATE DATABASE dbPayPhoneChallenge
GO

USE dbPayPhoneChallenge
GO

-- Users Table
CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    UserTypeId INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    IsDisabled BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Wallets Table
CREATE TABLE Wallets (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(30) NOT NULL,
    Description NVARCHAR(100)  NULL,
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsDisabled BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_Wallets_UserId_Name UNIQUE (UserId, Name)
);

-- Transactions Table
CREATE TABLE Transactions (
    Id INT IDENTITY PRIMARY KEY,
    WalletId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    OperationType VARCHAR(8) NOT NULL CHECK (OperationType IN ('Credit', 'Debit')),
    OperationDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsDisabled BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
    FOREIGN KEY (WalletId) REFERENCES Wallets(Id),
);