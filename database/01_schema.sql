IF DB_ID('FurEver') IS NULL
    CREATE DATABASE FurEver;
GO

USE FurEver;
GO

IF OBJECT_ID('dbo.Vaccination', 'U') IS NOT NULL DROP TABLE dbo.Vaccination;
IF OBJECT_ID('dbo.Veterinary_Visit', 'U') IS NOT NULL DROP TABLE dbo.Veterinary_Visit;
IF OBJECT_ID('dbo.Favorite', 'U') IS NOT NULL DROP TABLE dbo.Favorite;
IF OBJECT_ID('dbo.Adoption', 'U') IS NOT NULL DROP TABLE dbo.Adoption;
IF OBJECT_ID('dbo.Adopter', 'U') IS NOT NULL DROP TABLE dbo.Adopter;
IF OBJECT_ID('dbo.Pet', 'U') IS NOT NULL DROP TABLE dbo.Pet;
IF OBJECT_ID('dbo.Admin', 'U') IS NOT NULL DROP TABLE dbo.Admin;
GO

CREATE TABLE dbo.Pet (
    Pet_ID          INT IDENTITY(1,1) PRIMARY KEY,
    Pet_Name        NVARCHAR(50)  NOT NULL,
    Species         NVARCHAR(30)  NOT NULL,
    Breed           NVARCHAR(50)  NOT NULL,
    Age             NVARCHAR(20)  NOT NULL,
    Gender          NVARCHAR(10)  NOT NULL
        CONSTRAINT CK_Pet_Gender CHECK (Gender IN ('Male','Female')),
    Color           NVARCHAR(50)  NOT NULL,
    Date_Arrived    DATE          NOT NULL,
    Spayed_Neutered NVARCHAR(3)   NOT NULL
        CONSTRAINT CK_Pet_SpayedNeutered CHECK (Spayed_Neutered IN ('Yes','No')),
    Temperament     NVARCHAR(100) NOT NULL,
    Special_Needs   NVARCHAR(MAX) NULL,
    Photo_URL       NVARCHAR(255) NULL,
    Status          NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_Pet_Status CHECK (Status IN ('Available','Adopted','Reserved','Medical Hold'))
);

CREATE INDEX idx_pet_status       ON dbo.Pet(Status);
CREATE INDEX idx_pet_species      ON dbo.Pet(Species);
CREATE INDEX idx_pet_date_arrived ON dbo.Pet(Date_Arrived);
GO

CREATE TABLE dbo.Adopter (
    Adopter_ID       INT IDENTITY(1,1) PRIMARY KEY,
    Email            NVARCHAR(100) NOT NULL CONSTRAINT UQ_Adopter_Email UNIQUE,
    Password_Hash    NVARCHAR(255) NOT NULL,
    Full_Name        NVARCHAR(100) NOT NULL,
    Contact_No       NVARCHAR(15)  NOT NULL,
    Address          NVARCHAR(200) NOT NULL,
    Housing_Type     NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_Adopter_Housing CHECK (Housing_Type IN ('House','Apartment','Condo','Farm')),
    Has_Other_Pets   NVARCHAR(3)   NOT NULL
        CONSTRAINT CK_Adopter_OtherPets CHECK (Has_Other_Pets IN ('Yes','No')),
    Has_Children     NVARCHAR(3)   NOT NULL
        CONSTRAINT CK_Adopter_Children CHECK (Has_Children IN ('Yes','No')),
    Experience_Level NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_Adopter_Experience CHECK (Experience_Level IN ('First-time','Experienced'))
);
GO

CREATE TABLE dbo.Adoption (
    Adoption_ID      INT IDENTITY(1,1) PRIMARY KEY,
    Pet_ID           INT NOT NULL
        CONSTRAINT FK_Adoption_Pet REFERENCES dbo.Pet(Pet_ID) ON DELETE CASCADE,
    Adopter_ID       INT NOT NULL
        CONSTRAINT FK_Adoption_Adopter REFERENCES dbo.Adopter(Adopter_ID) ON DELETE CASCADE,
    Application_Date DATE NOT NULL CONSTRAINT DF_Adoption_AppDate DEFAULT (CAST(GETDATE() AS DATE)),
    Adoption_Date    DATE NULL,
    Adoption_Fee     DECIMAL(10,2) NULL,
    Contract_Signed  NVARCHAR(3) NOT NULL
        CONSTRAINT CK_Adoption_Contract CHECK (Contract_Signed IN ('Yes','No')),
    Status           NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Adoption_Status CHECK (Status IN ('Completed','Pending','Returned','Cancelled'))
);

CREATE INDEX idx_adoption_status           ON dbo.Adoption(Status);
CREATE INDEX idx_adoption_pet              ON dbo.Adoption(Pet_ID);
CREATE INDEX idx_adoption_adopter          ON dbo.Adoption(Adopter_ID);
CREATE INDEX idx_adoption_date             ON dbo.Adoption(Adoption_Date);
CREATE INDEX idx_adoption_application_date ON dbo.Adoption(Application_Date);
GO

CREATE TABLE dbo.Veterinary_Visit (
    Visit_ID          INT IDENTITY(1,1) PRIMARY KEY,
    Pet_ID            INT NOT NULL
        CONSTRAINT FK_VetVisit_Pet REFERENCES dbo.Pet(Pet_ID) ON DELETE CASCADE,
    Visit_Date        DATE          NOT NULL,
    Veterinarian_Name NVARCHAR(100) NOT NULL,
    Visit_Type        NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_VetVisit_Type CHECK (Visit_Type IN ('Checkup','Vaccination','Surgery','Treatment','Emergency')),
    Weight            DECIMAL(5,2)  NULL,
    Temperature       DECIMAL(4,2)  NULL,
    Diagnosis         NVARCHAR(MAX) NULL,
    General_Notes     NVARCHAR(MAX) NULL,
    Procedure_Cost    DECIMAL(10,2) NOT NULL,
    Next_Visit_Date   DATE          NULL
);

CREATE INDEX idx_vet_visit_pet ON dbo.Veterinary_Visit(Pet_ID);
GO

CREATE TABLE dbo.Vaccination (
    Vaccination_ID    INT IDENTITY(1,1) PRIMARY KEY,
    Visit_ID          INT NOT NULL
        CONSTRAINT FK_Vaccination_Visit REFERENCES dbo.Veterinary_Visit(Visit_ID) ON DELETE CASCADE,
    Vaccine_Name      NVARCHAR(100) NOT NULL,
    Date_Administered DATE          NULL,
    Administered_By   NVARCHAR(100) NULL,
    Manufacturer      NVARCHAR(50)  NULL,
    Next_Due_Date     DATE          NULL,
    Site              NVARCHAR(50)  NULL,
    Reaction          NVARCHAR(MAX) NULL,
    Status            NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_Vaccination_Status CHECK (Status IN ('Completed','Scheduled','Overdue')),
    Cost              DECIMAL(10,2) NULL
);

CREATE INDEX idx_vaccination_status   ON dbo.Vaccination(Status);
CREATE INDEX idx_vaccination_next_due ON dbo.Vaccination(Next_Due_Date);
GO

CREATE TABLE dbo.Favorite (
    Favorite_ID INT IDENTITY(1,1) PRIMARY KEY,
    Adopter_ID  INT NOT NULL
        CONSTRAINT FK_Favorite_Adopter REFERENCES dbo.Adopter(Adopter_ID) ON DELETE CASCADE,
    Pet_ID      INT NOT NULL
        CONSTRAINT FK_Favorite_Pet REFERENCES dbo.Pet(Pet_ID) ON DELETE CASCADE,
    Date_Added  DATETIME2 NOT NULL CONSTRAINT DF_Favorite_DateAdded DEFAULT (SYSDATETIME()),
    Notes       NVARCHAR(MAX) NULL,
    CONSTRAINT Unique_Favorite UNIQUE (Adopter_ID, Pet_ID)
);

CREATE INDEX idx_favorite_adopter ON dbo.Favorite(Adopter_ID);
CREATE INDEX idx_favorite_pet     ON dbo.Favorite(Pet_ID);
GO

CREATE TABLE dbo.Admin (
    Admin_ID      INT IDENTITY(1,1) PRIMARY KEY,
    Email         NVARCHAR(255) NOT NULL CONSTRAINT UQ_Admin_Email UNIQUE,
    Password_Hash NVARCHAR(255) NOT NULL,
    Full_Name     NVARCHAR(100) NOT NULL
);
GO

CREATE OR ALTER TRIGGER dbo.trg_adoption_validate_insert
ON dbo.Adoption
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Pet p ON p.Pet_ID = i.Pet_ID
        WHERE p.Status <> 'Available'
    )
    BEGIN
        THROW 50001, 'This pet is not available for adoption.', 1;
    END

    UPDATE a
    SET a.Adoption_Date   = ISNULL(a.Adoption_Date, CAST(GETDATE() AS DATE)),
        a.Contract_Signed = 'Yes'
    FROM dbo.Adoption a
    JOIN inserted i ON i.Adoption_ID = a.Adoption_ID
    WHERE i.Status = 'Completed';
END;
GO

EXEC sp_settriggerorder
    @triggername = 'dbo.trg_adoption_validate_insert',
    @order = 'First',
    @stmttype = 'INSERT';
GO

CREATE OR ALTER TRIGGER dbo.trg_adoption_after_insert
ON dbo.Adoption
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Status = CASE i.Status
                       WHEN 'Completed' THEN 'Adopted'
                       ELSE 'Reserved'
                   END
    FROM dbo.Pet p
    JOIN inserted i ON i.Pet_ID = p.Pet_ID
    WHERE i.Status IN ('Pending','Completed');
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_adoption_after_update
ON dbo.Adoption
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE a
    SET a.Adoption_Date   = ISNULL(a.Adoption_Date, CAST(GETDATE() AS DATE)),
        a.Contract_Signed = 'Yes'
    FROM dbo.Adoption a
    JOIN inserted i ON i.Adoption_ID = a.Adoption_ID
    JOIN deleted  d ON d.Adoption_ID = a.Adoption_ID
    WHERE i.Status = 'Completed' AND d.Status <> 'Completed';

    UPDATE p
    SET p.Status = CASE i.Status
                       WHEN 'Completed' THEN 'Adopted'
                       WHEN 'Pending'   THEN 'Reserved'
                       ELSE 'Available'
                   END
    FROM dbo.Pet p
    JOIN inserted i ON i.Pet_ID = p.Pet_ID
    JOIN deleted  d ON d.Adoption_ID = i.Adoption_ID
    WHERE i.Status <> d.Status;
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_adoption_after_delete
ON dbo.Adoption
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Status = 'Available'
    FROM dbo.Pet p
    JOIN deleted d ON d.Pet_ID = p.Pet_ID
    WHERE p.Status IN ('Reserved','Adopted');
END;
GO

CREATE OR ALTER TRIGGER dbo.trg_pet_cleanup_favorites
ON dbo.Pet
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DELETE f
    FROM dbo.Favorite f
    JOIN inserted i ON i.Pet_ID = f.Pet_ID
    JOIN deleted  d ON d.Pet_ID = i.Pet_ID
    WHERE i.Status = 'Adopted' AND d.Status <> 'Adopted';
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_get_available_pets_by_species
    @p_species NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM dbo.Pet
    WHERE Status = 'Available'
      AND Species = @p_species
    ORDER BY Date_Arrived DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_monthly_adoption_stats
    @p_year  INT,
    @p_month INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*)                                               AS Total_Adoptions,
        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END)  AS Completed,
        SUM(CASE WHEN Status = 'Pending'   THEN 1 ELSE 0 END)  AS Pending,
        SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END)  AS Cancelled,
        SUM(CASE WHEN Status = 'Returned'  THEN 1 ELSE 0 END)  AS Returned,
        ISNULL(SUM(CASE WHEN Status = 'Completed' THEN Adoption_Fee END), 0) AS Total_Revenue
    FROM dbo.Adoption
    WHERE YEAR(Application_Date)  = @p_year
      AND MONTH(Application_Date) = @p_month;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_update_overdue_vaccinations
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Vaccination
    SET Status = 'Overdue'
    WHERE Status = 'Scheduled'
      AND Next_Due_Date IS NOT NULL
      AND Next_Due_Date < CAST(GETDATE() AS DATE);
END;
GO

PRINT 'FurEver schema created successfully.';
GO
