-- ============================================================
-- FurEver sample data
-- Run after 01_schema.sql. Idempotent: skips if pets exist.
-- Note: the Admin user is seeded by the app on startup
-- (BCrypt hash generated in C#).
-- ============================================================

USE FurEver;
GO

IF EXISTS (SELECT 1 FROM dbo.Pet)
BEGIN
    PRINT 'Seed skipped: Pet table already has data.';
    RETURN;
END

-- ---------- Pets ----------
SET IDENTITY_INSERT dbo.Pet ON;

INSERT INTO dbo.Pet
    (Pet_ID, Pet_Name, Species, Breed, Age, Gender, Color, Date_Arrived,
     Spayed_Neutered, Temperament, Special_Needs, Photo_URL, Status)
VALUES
    (1, 'Luna',    'Dog', 'Labrador Retriever',  '2 years',   'Female', 'Black',           '2025-11-02', 'Yes', 'Gentle, loves water',            NULL,                                   NULL, 'Available'),
    (2, 'Biscuit', 'Dog', 'Corgi Mix',           '1 year',    'Male',   'Tan and White',   '2026-01-15', 'Yes', 'Playful, food-motivated',        NULL,                                   NULL, 'Available'),
    (3, 'Mochi',   'Cat', 'Domestic Shorthair',  '8 months',  'Female', 'Calico',          '2026-02-20', 'Yes', 'Curious, lap cat in training',   NULL,                                   NULL, 'Available'),
    (4, 'Shadow',  'Cat', 'Bombay',              '4 years',   'Male',   'Black',           '2025-09-10', 'Yes', 'Reserved at first, then velcro', 'Needs daily thyroid medication',        NULL, 'Available'),
    (5, 'Clover',  'Rabbit', 'Holland Lop',      '1.5 years', 'Female', 'Grey',            '2026-03-05', 'No',  'Calm, litter-trained',           NULL,                                   NULL, 'Available'),
    (6, 'Pico',    'Bird', 'Cockatiel',          '3 years',   'Male',   'Grey and Yellow', '2026-04-01', 'No',  'Whistles back, sociable',        NULL,                                   NULL, 'Available'),
    (7, 'Maple',   'Dog', 'Golden Retriever',    '5 years',   'Female', 'Golden',          '2025-08-22', 'Yes', 'Patient, great with kids',       'Mild hip dysplasia — joint supplements', NULL, 'Available'),
    (8, 'Onyx',    'Dog', 'German Shepherd Mix', '3 years',   'Male',   'Black and Tan',   '2025-12-12', 'Yes', 'Loyal, needs an active home',    NULL,                                   NULL, 'Medical Hold'),
    (9, 'Pearl',   'Cat', 'Ragdoll',             '2 years',   'Female', 'Cream',           '2026-05-18', 'Yes', 'Relaxed, floppy, affectionate',  NULL,                                   NULL, 'Available'),
    (10,'Rusty',   'Dog', 'Beagle',              '6 years',   'Male',   'Tricolor',        '2025-07-30', 'Yes', 'Nose-driven, mellow indoors',    NULL,                                   NULL, 'Adopted');

SET IDENTITY_INSERT dbo.Pet OFF;

-- ---------- Veterinary visits ----------
SET IDENTITY_INSERT dbo.Veterinary_Visit ON;

INSERT INTO dbo.Veterinary_Visit
    (Visit_ID, Pet_ID, Visit_Date, Veterinarian_Name, Visit_Type, Weight, Temperature,
     Diagnosis, General_Notes, Procedure_Cost, Next_Visit_Date)
VALUES
    (1, 1, '2025-11-03', 'Dr. Amelia Reyes',  'Checkup',     26.40, 38.40, NULL,                          'Intake exam. Healthy, slightly underweight.',        45.00,  '2026-05-03'),
    (2, 1, '2025-11-10', 'Dr. Amelia Reyes',  'Vaccination', 26.90, 38.30, NULL,                          'Core vaccines administered.',                        80.00,  NULL),
    (3, 3, '2026-02-21', 'Dr. Ben Soriano',   'Checkup',     3.10,  38.60, NULL,                          'Intake exam. Clean bill of health.',                 45.00,  '2026-08-21'),
    (4, 3, '2026-03-01', 'Dr. Ben Soriano',   'Vaccination', 3.20,  38.50, NULL,                          'FVRCP first dose.',                                  60.00,  NULL),
    (5, 4, '2026-01-12', 'Dr. Amelia Reyes',  'Treatment',   5.40,  38.90, 'Hyperthyroidism',             'Started methimazole. Recheck bloodwork in 6 weeks.', 150.00, '2026-02-23'),
    (6, 7, '2025-09-01', 'Dr. Karen Uy',      'Surgery',     29.80, 38.20, 'Hip dysplasia (mild)',        'Arthroscopic evaluation. Conservative management.',  520.00, '2026-03-01'),
    (7, 8, '2026-05-28', 'Dr. Karen Uy',      'Treatment',   31.20, 39.10, 'Heartworm positive (early)',  'Began treatment protocol. Hold from adoption.',      310.00, '2026-07-28'),
    (8, 5, '2026-03-06', 'Dr. Ben Soriano',   'Checkup',     1.60,  38.70, NULL,                          'Intake exam. Teeth and nails trimmed.',              40.00,  NULL),
    (9, 9, '2026-05-19', 'Dr. Amelia Reyes',  'Vaccination', 4.30,  38.40, NULL,                          'Rabies and FVRCP boosters.',                         75.00,  NULL),
    (10, 2, '2026-01-16', 'Dr. Karen Uy',     'Checkup',     11.10, 38.50, NULL,                          'Intake exam. High energy, healthy.',                 45.00,  '2026-07-16');

SET IDENTITY_INSERT dbo.Veterinary_Visit OFF;

-- ---------- Vaccinations ----------
SET IDENTITY_INSERT dbo.Vaccination ON;

INSERT INTO dbo.Vaccination
    (Vaccination_ID, Visit_ID, Vaccine_Name, Date_Administered, Administered_By,
     Manufacturer, Next_Due_Date, Site, Reaction, Status, Cost)
VALUES
    (1, 2, 'Rabies',            '2025-11-10', 'Dr. Amelia Reyes', 'Zoetis',    '2026-11-10', 'Right hind leg',  NULL,                 'Completed', 25.00),
    (2, 2, 'DHPP',              '2025-11-10', 'Dr. Amelia Reyes', 'Merck',     '2026-11-10', 'Left shoulder',   'Mild soreness',      'Completed', 30.00),
    (3, 4, 'FVRCP (1st dose)',  '2026-03-01', 'Dr. Ben Soriano',  'Boehringer','2026-03-22', 'Right shoulder',  NULL,                 'Completed', 28.00),
    (4, 4, 'FVRCP (2nd dose)',  NULL,         NULL,               'Boehringer','2026-03-22', NULL,              NULL,                 'Scheduled', 28.00),
    (5, 9, 'Rabies',            '2026-05-19', 'Dr. Amelia Reyes', 'Zoetis',    '2027-05-19', 'Right hind leg',  NULL,                 'Completed', 25.00),
    (6, 9, 'FVRCP booster',     '2026-05-19', 'Dr. Amelia Reyes', 'Boehringer','2027-05-19', 'Left shoulder',   NULL,                 'Completed', 28.00),
    (7, 10, 'Bordetella',       NULL,         NULL,               'Zoetis',    '2026-02-10', NULL,              NULL,                 'Overdue',   22.00);

SET IDENTITY_INSERT dbo.Vaccination OFF;

PRINT 'FurEver seed data inserted.';
GO
