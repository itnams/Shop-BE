CREATE TABLE Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserName VARCHAR(255),
    Password VARCHAR(255),
    Role VARCHAR(255)
);

CREATE TABLE ProductCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(255),
)

CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(255),
    Description NVARCHAR(MAX),
    Price DECIMAL,
    OldPrice DECIMAL,
    CategoryId INT,
    PromotionId INT,
)

CREATE TABLE Cart(
    CartId INT IDENTITY(1,1) PRIMARY KEY, 
    UserId INT,
)

CREATE TABLE CartItems(
    CartItemId INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT,
    ProductId INT,
    Quantity Int,
)

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    OrderDate NVARCHAR(MAX),
    TotalAmount DECIMAL,
    Status NVARCHAR(MAX),
    Address NVARCHAR(MAX),
    PaymentMethods NVARCHAR(MAX)
)

CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT,
    ProductId Int,
    Quantity Int,
    Price DECIMAL,
)

CREATE TABLE ProductImages (
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT,
    ImagePath VARCHAR(MAX)
)

CREATE TABLE Promotions (
    PromotionId INT IDENTITY(1,1) PRIMARY KEY,
    PromotionName NVARCHAR(MAX),
    Discount DECIMAL,
    StartDate NVARCHAR(MAX),
    EndDate NVARCHAR(MAX),
    Image NVARCHAR(Max)
)

CREATE TABLE Reviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId Int,
    UserId Int,
    Rating Int,
    Comment NVARCHAR(255),
    ReviewDate NVARCHAR(MAX)
)

INSERT INTO Customer (UserName, Password, Role)
VALUES ('admin','Admin1234','Admin'),('itnams12','12345678','Customer');

INSERT INTO ProductCategories(CategoryName)
VALUES ('Túi Xách'),('Ba lô'),('Ví');

INSERT INTO Cart(UserId)
VALUES (1),(2),(3),(4);