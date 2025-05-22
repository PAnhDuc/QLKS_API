USE BE;
GO

-- Bảng Roles: Lưu thông tin vai trò
CREATE TABLE Roles (
    role_id INT PRIMARY KEY IDENTITY(1,1),
    role_name NVARCHAR(50) NOT NULL UNIQUE,
    description NTEXT
);
GO

-- Bảng Permissions: Lưu thông tin quyền truy cập
CREATE TABLE Permissions (
    permission_id INT PRIMARY KEY IDENTITY(1,1),
    permission_name NVARCHAR(50) NOT NULL UNIQUE,
    description NTEXT
);
GO

-- Bảng Role_Permissions: Liên kết vai trò với quyền
CREATE TABLE Role_Permissions (
    role_id INT,
    permission_id INT,
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES Roles(role_id),
    FOREIGN KEY (permission_id) REFERENCES Permissions(permission_id)
);
GO

-- Bảng Users: Lưu thông tin người dùng
CREATE TABLE Users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    role_id INT,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(100),
    email NVARCHAR(100) UNIQUE,
    phone NVARCHAR(20),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (role_id) REFERENCES Roles(role_id)
);
GO

-- Bảng Customers: Lưu thông tin khách hàng
CREATE TABLE Customers (
    customer_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    full_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(100),
    phone NVARCHAR(20),
    address NTEXT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);
GO

-- Bảng Rooms: Lưu thông tin phòng
CREATE TABLE Rooms (
    room_id INT PRIMARY KEY IDENTITY(1,1),
    room_number NVARCHAR(10) NOT NULL UNIQUE,
    room_type NVARCHAR(50),
    price DECIMAL(10, 2) NOT NULL,
    status NVARCHAR(20) DEFAULT N'available',
    description NTEXT,
    CONSTRAINT CHK_Room_Status CHECK (status IN (N'available', N'occupied', N'maintenance'))
);
GO

-- Bảng Bookings: Lưu thông tin đặt phòng
CREATE TABLE Bookings (
    booking_id INT PRIMARY KEY IDENTITY(1,1),
    customer_id INT,
    room_id INT,
    check_in_date DATE NOT NULL,
    check_out_date DATE NOT NULL,
    status NVARCHAR(20) DEFAULT N'pending',
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id),
    FOREIGN KEY (room_id) REFERENCES Rooms(room_id),
    CONSTRAINT CHK_Booking_Status CHECK (status IN (N'pending', N'confirmed', N'cancelled', N'completed'))
);
GO

-- Bảng Services: Lưu thông tin dịch vụ
CREATE TABLE Services (
    service_id INT PRIMARY KEY IDENTITY(1,1),
    service_name NVARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    description NTEXT
);
GO

-- Bảng Invoices: Lưu thông tin hóa đơn
CREATE TABLE Invoices (
    invoice_id INT PRIMARY KEY IDENTITY(1,1),
    booking_id INT,
    customer_id INT,
    total_amount DECIMAL(10, 2) NOT NULL,
    status NVARCHAR(20) DEFAULT N'pending',
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (booking_id) REFERENCES Bookings(booking_id),
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id),
    CONSTRAINT CHK_Invoice_Status CHECK (status IN (N'pending', N'paid', N'cancelled'))
);
GO

-- Bảng Invoice_Services: Liên kết hóa đơn với dịch vụ
CREATE TABLE Invoice_Services (
    invoice_id INT,
    service_id INT,
    quantity INT DEFAULT 1,
    PRIMARY KEY (invoice_id, service_id),
    FOREIGN KEY (invoice_id) REFERENCES Invoices(invoice_id),
    FOREIGN KEY (service_id) REFERENCES Services(service_id)
);
GO

-- Bảng Reports: Lưu thông tin báo cáo
CREATE TABLE Reports (
    report_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    report_type NVARCHAR(20) NOT NULL,
    report_data NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    CONSTRAINT CHK_Report_Type CHECK (report_type IN (N'financial', N'booking', N'service_usage')),
    CONSTRAINT CHK_Valid_JSON CHECK (ISJSON(report_data) = 1)
);
GO

-- Bảng Invoice_Log: Lưu lịch sử thay đổi hóa đơn
CREATE TABLE Invoice_Log (
    log_id INT PRIMARY KEY IDENTITY(1,1),
    invoice_id INT,
    status NVARCHAR(20),
    changed_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (invoice_id) REFERENCES Invoices(invoice_id)
);
GO

-- Stored Procedure 1: CreateBooking
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateBooking]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CreateBooking];
GO

CREATE PROCEDURE CreateBooking
    @p_customer_id INT,
    @p_room_id INT,
    @p_check_in_date DATE,
    @p_check_out_date DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @room_status NVARCHAR(20);
    DECLARE @booking_conflict INT;

    -- Kiểm tra trạng thái phòng
    SELECT @room_status = status
    FROM Rooms
    WHERE room_id = @p_room_id;

    -- Kiểm tra trùng lặp đặt phòng
    SELECT @booking_conflict = COUNT(*)
    FROM Bookings
    WHERE room_id = @p_room_id
    AND status IN (N'pending', N'confirmed')
    AND (
        (@p_check_in_date BETWEEN check_in_date AND check_out_date)
        OR (@p_check_out_date BETWEEN check_in_date AND check_out_date)
        OR (check_in_date BETWEEN @p_check_in_date AND @p_check_out_date)
    );

    IF @room_status = N'available' AND @booking_conflict = 0
    BEGIN
        -- Tạo bản ghi đặt phòng
        INSERT INTO Bookings (customer_id, room_id, check_in_date, check_out_date, status)
        VALUES (@p_customer_id, @p_room_id, @p_check_in_date, @p_check_out_date, N'confirmed');

        -- Cập nhật trạng thái phòng
        UPDATE Rooms
        SET status = N'occupied'
        WHERE room_id = @p_room_id;

        SELECT N'Booking created successfully' AS message, SCOPE_IDENTITY() AS booking_id;
    END
    ELSE
    BEGIN
        THROW 50001, N'Room is not available or booking conflicts with existing booking', 1;
    END
END;
GO

-- Stored Procedure 2: GenerateMonthlyRevenueReport
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenerateMonthlyRevenueReport]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GenerateMonthlyRevenueReport];
GO

CREATE PROCEDURE GenerateMonthlyRevenueReport
    @p_user_id INT,
    @p_year INT,
    @p_month INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @report_data NVARCHAR(MAX);

    -- Tính doanh thu từ hóa đơn đã thanh toán
    SELECT @report_data = (
        SELECT 
            @p_year AS year,
            @p_month AS month,
            SUM(total_amount) AS total_revenue,
            COUNT(*) AS invoice_count
        FROM Invoices
        WHERE status = N'paid'
        AND YEAR(created_at) = @p_year
        AND MONTH(created_at) = @p_month
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    );

    -- Lưu báo cáo vào bảng Reports
    INSERT INTO Reports (user_id, report_type, report_data)
    VALUES (@p_user_id, N'financial', @report_data);

    SELECT N'Revenue report generated successfully' AS message, SCOPE_IDENTITY() AS report_id;
END;
GO

-- Trigger 1: AfterBookingInsert
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[AfterBookingInsert]'))
    DROP TRIGGER [dbo].[AfterBookingInsert];
GO

CREATE TRIGGER AfterBookingInsert
ON Bookings
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Rooms
    SET status = N'occupied'
    FROM Rooms r
    INNER JOIN inserted i ON r.room_id = i.room_id
    WHERE i.status = N'confirmed';
END;
GO

-- Trigger 2: AfterInvoiceUpdate
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[AfterInvoiceUpdate]'))
    DROP TRIGGER [dbo].[AfterInvoiceUpdate];
GO

CREATE TRIGGER AfterInvoiceUpdate
ON Invoices
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Invoice_Log (invoice_id, status)
    SELECT i.invoice_id, i.status
    FROM inserted i
    INNER JOIN deleted d ON i.invoice_id = d.invoice_id
    WHERE i.status = N'paid' AND d.status != N'paid';
END;
GO

-- Thêm dữ liệu mẫu cho bảng Roles với tiền tố N để hỗ trợ Unicode
INSERT INTO Roles (role_name, description) VALUES
(N'Admin', N'Quản trị viên'),
(N'Receptionist', N'Nhân viên lễ tân'),
(N'Accountant', N'Nhân viên kế toán'),
(N'Customer', N'Khách hàng'),
(N'Manager', N'Quản lý khách sạn');
GO