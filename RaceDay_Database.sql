-- Create the database
CREATE DATABASE RaceDay;
GO

-- Use the database
USE RaceDay;
GO

-- Create User table
CREATE TABLE AppUser (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Create Event table
CREATE TABLE Event (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    MaxEntries INT NOT NULL DEFAULT 1000,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Upcoming',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrganiserId) REFERENCES AppUser(UserId)
);
GO

-- Create Category table
CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Distance DECIMAL(8,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaxCapacity INT NOT NULL DEFAULT 500,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EventId) REFERENCES Event(EventId)
);
GO

-- Create Enrolment table
CREATE TABLE Enrolment (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    RegisteredAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ParticipantId) REFERENCES AppUser(UserId),
    FOREIGN KEY (EventId) REFERENCES Event(EventId),
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
);
GO

-- Create Result table
CREATE TABLE Result (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL,
    FinishTime TIME NULL,
    Position INT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'DNS',
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EnrolmentId) REFERENCES Enrolment(EnrolmentId)
);
GO

-- Create EventImage table
CREATE TABLE EventImage (
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EventId) REFERENCES Event(EventId)
);
GO

-- Add sample data
INSERT INTO AppUser (FirstName, LastName, Email, PasswordHash, Role) VALUES 
('Thabo', 'Nkosi', 'thabo@comrades.co.za', 'hash123', 'Organiser'),
('Sarah', 'van der Merwe', 'sarah@twinoceans.co.za', 'hash456', 'Organiser'),
('John', 'Doe', 'john.doe@gmail.com', 'hash789', 'Participant'),
('Lisa', 'Naidoo', 'lisa.naidoo@gmail.com', 'hash101', 'Participant');
GO

INSERT INTO Event (OrganiserId, Title, Description, EventDate, Location, MaxEntries) VALUES 
(1, 'Comrades Marathon 2026', 'Ultramarathon from Pietermaritzburg to Durban', '2026-06-16', 'Pietermaritzburg to Durban', 25000),
(2, 'Two Oceans Ultra Marathon', '56km ultra marathon around Cape Town', '2026-04-10', 'Cape Town', 16000);
GO

INSERT INTO Category (EventId, Name, Distance, EntryFee, MaxCapacity) VALUES 
(1, 'Ultramarathon 87km', 87.00, 850.00, 15000),
(1, 'Ultramarathon 56km', 56.00, 650.00, 10000),
(2, 'Ultra Marathon 56km', 56.00, 750.00, 11000);
GO

INSERT INTO Enrolment (ParticipantId, EventId, CategoryId, Status) VALUES 
(3, 1, 1, 'Confirmed'),
(4, 2, 3, 'Confirmed');
GO

INSERT INTO Result (EnrolmentId, FinishTime, Position, Status) VALUES 
(1, '05:45:30', 150, 'Finished'),
(2, '03:20:15', 45, 'Finished');
GO

INSERT INTO EventImage (EventId, ImageUrl, IsPrimary) VALUES 
(1, 'https://racedaystorage.blob.core.windows.net/images/comrades.jpg', 1),
(2, 'https://racedaystorage.blob.core.windows.net/images/two-oceans.jpg', 1);
GO

PRINT 'RaceDay database created successfully!';