-- ============================================
-- ALOud Perfume Database Seeding Script
-- SQL Server (T-SQL)
-- ============================================

USE ALOud; -- Change to your database name
GO

-- ============================================
-- 1. INSERT BRANDS
-- ============================================
INSERT INTO Brands (Id, Name) VALUES
(NEWID(), 'Chanel'),
(NEWID(), 'Dior'),
(NEWID(), 'Tom Ford'),
(NEWID(), 'Creed'),
(NEWID(), 'Yves Saint Laurent'),
(NEWID(), 'Giorgio Armani'),
(NEWID(), 'Versace'),
(NEWID(), 'Prada'),
(NEWID(), 'Gucci'),
(NEWID(), 'Hermès'),
(NEWID(), 'Jo Malone'),
(NEWID(), 'Maison Francis Kurkdjian'),
(NEWID(), 'Byredo'),
(NEWID(), 'Le Labo'),
(NEWID(), 'Acqua di Parma');

-- ============================================
-- 2. INSERT FAMILIES
-- ============================================
INSERT INTO Families (Id, Name, Description) VALUES
(NEWID(), 'Floral', 'Fragrances dominated by the scent of flowers'),
(NEWID(), 'Oriental', 'Warm, exotic fragrances with spices and resins'),
(NEWID(), 'Woody', 'Fragrances based on wood notes like cedar and sandalwood'),
(NEWID(), 'Fresh', 'Light, clean scents with citrus and aquatic notes'),
(NEWID(), 'Chypre', 'Sophisticated fragrances with oakmoss, patchouli, and bergamot'),
(NEWID(), 'Gourmand', 'Sweet, edible scents like vanilla, caramel, and chocolate'),
(NEWID(), 'Fougère', 'Aromatic fragrances with lavender, coumarin, and oakmoss'),
(NEWID(), 'Aromatic', 'Herbal scents with sage, rosemary, and thyme');

-- ============================================
-- 3. INSERT NOTES
-- ============================================
INSERT INTO Notes (Id, Name, Category, Description) VALUES
-- Citrus Notes
(NEWID(), 'Bergamot', 'Citrus', 'Fresh, citrusy, slightly spicy'),
(NEWID(), 'Lemon', 'Citrus', 'Bright, zesty, energizing'),
(NEWID(), 'Orange', 'Citrus', 'Sweet, juicy, uplifting'),
(NEWID(), 'Grapefruit', 'Citrus', 'Tart, refreshing, slightly bitter'),
(NEWID(), 'Mandarin', 'Citrus', 'Sweet, tangy, light'),

-- Floral Notes
(NEWID(), 'Rose', 'Floral', 'Classic, romantic, elegant'),
(NEWID(), 'Jasmine', 'Floral', 'Rich, heady, exotic'),
(NEWID(), 'Lavender', 'Floral', 'Clean, calming, aromatic'),
(NEWID(), 'Iris', 'Floral', 'Powdery, elegant, soft'),
(NEWID(), 'Ylang-Ylang', 'Floral', 'Sweet, exotic, slightly fruity'),
(NEWID(), 'Tuberose', 'Floral', 'Intense, creamy, narcotic'),
(NEWID(), 'Magnolia', 'Floral', 'Fresh, green, lemony'),
(NEWID(), 'Orange Blossom', 'Floral', 'Sweet, fresh, honeyed'),

-- Woody Notes
(NEWID(), 'Sandalwood', 'Woody', 'Creamy, warm, smooth'),
(NEWID(), 'Cedar', 'Woody', 'Dry, pencil-like, aromatic'),
(NEWID(), 'Vetiver', 'Woody', 'Earthy, smoky, green'),
(NEWID(), 'Patchouli', 'Woody', 'Earthy, musky, slightly sweet'),
(NEWID(), 'Oud', 'Woody', 'Rich, animalic, resinous'),
(NEWID(), 'Oakmoss', 'Woody', 'Earthy, forest-like, mossy'),

-- Spicy Notes
(NEWID(), 'Black Pepper', 'Spicy', 'Sharp, spicy, vibrant'),
(NEWID(), 'Pink Pepper', 'Spicy', 'Fresh, fruity, mildly spicy'),
(NEWID(), 'Cardamom', 'Spicy', 'Warm, aromatic, slightly sweet'),
(NEWID(), 'Cinnamon', 'Spicy', 'Sweet, warm, comforting'),
(NEWID(), 'Clove', 'Spicy', 'Warm, spicy, slightly medicinal'),

-- Sweet/Gourmand Notes
(NEWID(), 'Vanilla', 'Gourmand', 'Sweet, creamy, comforting'),
(NEWID(), 'Tonka Bean', 'Gourmand', 'Sweet, warm, almond-like'),
(NEWID(), 'Caramel', 'Gourmand', 'Sweet, buttery, rich'),
(NEWID(), 'Honey', 'Gourmand', 'Sweet, rich, golden'),
(NEWID(), 'Chocolate', 'Gourmand', 'Rich, sweet, indulgent'),

-- Musk/Amber
(NEWID(), 'Musk', 'Musk', 'Clean, skin-like, soft'),
(NEWID(), 'Amber', 'Resin', 'Warm, sweet, powdery'),
(NEWID(), 'Ambergris', 'Animalic', 'Marine, sweet, musky'),

-- Fresh/Aquatic
(NEWID(), 'Sea Notes', 'Aquatic', 'Fresh, marine, ozonic'),
(NEWID(), 'Mint', 'Fresh', 'Cool, refreshing, crisp'),
(NEWID(), 'Green Notes', 'Fresh', 'Leafy, fresh-cut grass, natural'),

-- Fruity
(NEWID(), 'Apple', 'Fruity', 'Fresh, crisp, juicy'),
(NEWID(), 'Peach', 'Fruity', 'Sweet, juicy, velvety'),
(NEWID(), 'Blackcurrant', 'Fruity', 'Tart, fruity, slightly green'),
(NEWID(), 'Pear', 'Fruity', 'Fresh, sweet, delicate');

-- ============================================
-- 4. INSERT ACCORDS
-- ============================================
INSERT INTO Accords (Id, Name, Description) VALUES
(NEWID(), 'Citrus', 'Fresh and zesty citrus blend'),
(NEWID(), 'Floral', 'Bouquet of flowers'),
(NEWID(), 'Woody', 'Warm wood notes'),
(NEWID(), 'Spicy', 'Warm spices'),
(NEWID(), 'Sweet', 'Sweet and gourmand'),
(NEWID(), 'Fresh', 'Clean and airy'),
(NEWID(), 'Aquatic', 'Marine and watery'),
(NEWID(), 'Powdery', 'Soft and powdery'),
(NEWID(), 'Smoky', 'Incense and smoke'),
(NEWID(), 'Earthy', 'Soil and earth tones'),
(NEWID(), 'Green', 'Herbal and leafy'),
(NEWID(), 'Musky', 'Skin-like musk'),
(NEWID(), 'Amber', 'Warm amber resin'),
(NEWID(), 'Leather', 'Rich leather'),
(NEWID(), 'Animalic', 'Sensual animalic notes');

-- ============================================
-- 5. INSERT SEASONS
-- ============================================
INSERT INTO Seasons (Id, Name) VALUES
(NEWID(), 'Spring'),
(NEWID(), 'Summer'),
(NEWID(), 'Fall'),
(NEWID(), 'Winter');

-- ============================================
-- 6. INSERT OCCASIONS
-- ============================================
INSERT INTO Occasions (Id, Name) VALUES
(NEWID(), 'Daily Wear'),
(NEWID(), 'Office'),
(NEWID(), 'Evening'),
(NEWID(), 'Night Out'),
(NEWID(), 'Date Night'),
(NEWID(), 'Formal Event'),
(NEWID(), 'Casual'),
(NEWID(), 'Sport'),
(NEWID(), 'Wedding'),
(NEWID(), 'Business Meeting');

-- ============================================
-- 7. INSERT TAGS
-- ============================================
INSERT INTO Tags (Id, Name) VALUES
(NEWID(), 'Classic'),
(NEWID(), 'Modern'),
(NEWID(), 'Luxury'),
(NEWID(), 'Niche'),
(NEWID(), 'Designer'),
(NEWID(), 'Bestseller'),
(NEWID(), 'Iconic'),
(NEWID(), 'Sexy'),
(NEWID(), 'Sophisticated'),
(NEWID(), 'Youthful'),
(NEWID(), 'Mature'),
(NEWID(), 'Bold'),
(NEWID(), 'Subtle'),
(NEWID(), 'Romantic'),
(NEWID(), 'Fresh'),
(NEWID(), 'Warm'),
(NEWID(), 'Cool'),
(NEWID(), 'Intense'),
(NEWID(), 'Light'),
(NEWID(), 'Versatile');

-- ============================================
-- 8. INSERT PERFUMES WITH DETAILED INFORMATION
-- ============================================

-- Declare variables for IDs to use in relationships
DECLARE @ChanelId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Chanel');
DECLARE @DiorId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Dior');
DECLARE @TomFordId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Tom Ford');
DECLARE @CreedId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Creed');
DECLARE @YSLId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Yves Saint Laurent');
DECLARE @ArmaniId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Giorgio Armani');
DECLARE @VersaceId UNIQUEIDENTIFIER = (SELECT Id FROM Brands WHERE Name = 'Versace');

-- Chanel No. 5
DECLARE @Chanel5Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@Chanel5Id, 'Chanel No. 5', 'Strong', 'Long Lasting (8-12 hours)', 'Heavy', 'Female', 'High ($150-$300)', 
        'https://example.com/chanel5.jpg', GETDATE(), @ChanelId);

-- Dior Sauvage
DECLARE @SauvageId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@SauvageId, 'Dior Sauvage', 'Strong', 'Very Long Lasting (12+ hours)', 'Heavy', 'Male', 'High ($150-$300)', 
        'https://example.com/sauvage.jpg', GETDATE(), @DiorId);

-- Tom Ford Black Orchid
DECLARE @BlackOrchidId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@BlackOrchidId, 'Tom Ford Black Orchid', 'Very Strong', 'Very Long Lasting (12+ hours)', 'Enormous', 'Unisex', 'Luxury ($300+)', 
        'https://example.com/blackorchid.jpg', GETDATE(), @TomFordId);

-- Creed Aventus
DECLARE @AventusId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@AventusId, 'Creed Aventus', 'Strong', 'Very Long Lasting (12+ hours)', 'Heavy', 'Male', 'Luxury ($300+)', 
        'https://example.com/aventus.jpg', GETDATE(), @CreedId);

-- YSL Black Opium
DECLARE @BlackOpiumId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@BlackOpiumId, 'Yves Saint Laurent Black Opium', 'Strong', 'Long Lasting (8-12 hours)', 'Heavy', 'Female', 'High ($150-$300)', 
        'https://example.com/blackopium.jpg', GETDATE(), @YSLId);

-- Armani Acqua di Gio
DECLARE @AcquaDiGioId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@AcquaDiGioId, 'Giorgio Armani Acqua di Gio', 'Moderate', 'Moderate (4-8 hours)', 'Moderate', 'Male', 'Medium ($100-$150)', 
        'https://example.com/acquadigio.jpg', GETDATE(), @ArmaniId);

-- Chanel Coco Mademoiselle
DECLARE @CocoMademoiselleId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@CocoMademoiselleId, 'Chanel Coco Mademoiselle', 'Strong', 'Long Lasting (8-12 hours)', 'Heavy', 'Female', 'High ($150-$300)', 
        'https://example.com/cocomademoiselle.jpg', GETDATE(), @ChanelId);

-- Dior J\'adore
DECLARE @JadoreId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@JadoreId, 'Dior J''adore', 'Moderate', 'Long Lasting (8-12 hours)', 'Moderate', 'Female', 'High ($150-$300)', 
        'https://example.com/jadore.jpg', GETDATE(), @DiorId);

-- Tom Ford Oud Wood
DECLARE @OudWoodId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@OudWoodId, 'Tom Ford Oud Wood', 'Strong', 'Very Long Lasting (12+ hours)', 'Heavy', 'Unisex', 'Luxury ($300+)', 
        'https://example.com/oudwood.jpg', GETDATE(), @TomFordId);

-- Versace Eros
DECLARE @ErosId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Perfumes (Id, Name, Intensity, Longevity, Sillage, GenderProfile, PriceRange, ImageUrl, CreatedAt, BrandId)
VALUES (@ErosId, 'Versace Eros', 'Strong', 'Long Lasting (8-12 hours)', 'Heavy', 'Male', 'Medium ($100-$150)', 
        'https://example.com/eros.jpg', GETDATE(), @VersaceId);

-- ============================================
-- 9. INSERT PERFUME-FAMILY RELATIONSHIPS
-- ============================================

-- Get Family IDs
DECLARE @FloralFamilyId UNIQUEIDENTIFIER = (SELECT Id FROM Families WHERE Name = 'Floral');
DECLARE @OrientalFamilyId UNIQUEIDENTIFIER = (SELECT Id FROM Families WHERE Name = 'Oriental');
DECLARE @WoodyFamilyId UNIQUEIDENTIFIER = (SELECT Id FROM Families WHERE Name = 'Woody');
DECLARE @FreshFamilyId UNIQUEIDENTIFIER = (SELECT Id FROM Families WHERE Name = 'Fresh');
DECLARE @GourmandFamilyId UNIQUEIDENTIFIER = (SELECT Id FROM Families WHERE Name = 'Gourmand');

-- Chanel No. 5 - Floral
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES (@Chanel5Id, @FloralFamilyId);

-- Dior Sauvage - Fresh, Woody
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES 
    (@SauvageId, @FreshFamilyId),
    (@SauvageId, @WoodyFamilyId);

-- Tom Ford Black Orchid - Oriental, Floral
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES 
    (@BlackOrchidId, @OrientalFamilyId),
    (@BlackOrchidId, @FloralFamilyId);

-- Creed Aventus - Fresh, Fruity
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES (@AventusId, @FreshFamilyId);

-- YSL Black Opium - Oriental, Gourmand
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES 
    (@BlackOpiumId, @OrientalFamilyId),
    (@BlackOpiumId, @GourmandFamilyId);

-- Armani Acqua di Gio - Fresh
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES (@AcquaDiGioId, @FreshFamilyId);

-- Chanel Coco Mademoiselle - Floral, Oriental
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES 
    (@CocoMademoiselleId, @FloralFamilyId),
    (@CocoMademoiselleId, @OrientalFamilyId);

-- Dior J'adore - Floral
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES (@JadoreId, @FloralFamilyId);

-- Tom Ford Oud Wood - Woody, Oriental
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES 
    (@OudWoodId, @WoodyFamilyId),
    (@OudWoodId, @OrientalFamilyId);

-- Versace Eros - Fresh, Woody
INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES 
    (@ErosId, @FreshFamilyId),
    (@ErosId, @WoodyFamilyId);

-- ============================================
-- 10. INSERT PERFUME-NOTE RELATIONSHIPS
-- ============================================

-- Get Note IDs
DECLARE @BergamotId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Bergamot');
DECLARE @RoseId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Rose');
DECLARE @JasmineId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Jasmine');
DECLARE @SandalwoodId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Sandalwood');
DECLARE @VanillaId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Vanilla');
DECLARE @PatchouliId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Patchouli');
DECLARE @AmberId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Amber');
DECLARE @MuskId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Musk');
DECLARE @BlackPepperId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Black Pepper');
DECLARE @VetiverId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Vetiver');
DECLARE @LavanderId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Lavender');
DECLARE @OudId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Oud');
DECLARE @BlackcurrantId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Blackcurrant');
DECLARE @AppleId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Apple');
DECLARE @SeaNotesId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Sea Notes');
DECLARE @OrangeBlossomId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Orange Blossom');
DECLARE @TonkaBeanId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Tonka Bean');
DECLARE @YlangYlangId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Ylang-Ylang');
DECLARE @MintId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Mint');
DECLARE @LemonId UNIQUEIDENTIFIER = (SELECT Id FROM Notes WHERE Name = 'Lemon');

-- Chanel No. 5
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@Chanel5Id, @BergamotId, 'Top'),
    (@Chanel5Id, @JasmineId, 'Middle'),
    (@Chanel5Id, @RoseId, 'Middle'),
    (@Chanel5Id, @VanillaId, 'Base'),
    (@Chanel5Id, @SandalwoodId, 'Base');

-- Dior Sauvage
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@SauvageId, @BergamotId, 'Top'),
    (@SauvageId, @BlackPepperId, 'Top'),
    (@SauvageId, @LavanderId, 'Middle'),
    (@SauvageId, @AmberId, 'Base'),
    (@SauvageId, @VetiverId, 'Base');

-- Tom Ford Black Orchid
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@BlackOrchidId, @BergamotId, 'Top'),
    (@BlackOrchidId, @JasmineId, 'Middle'),
    (@BlackOrchidId, @YlangYlangId, 'Middle'),
    (@BlackOrchidId, @PatchouliId, 'Base'),
    (@BlackOrchidId, @VanillaId, 'Base');

-- Creed Aventus
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@AventusId, @BlackcurrantId, 'Top'),
    (@AventusId, @AppleId, 'Top'),
    (@AventusId, @BergamotId, 'Top'),
    (@AventusId, @RoseId, 'Middle'),
    (@AventusId, @MuskId, 'Base'),
    (@AventusId, @VanillaId, 'Base');

-- YSL Black Opium
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@BlackOpiumId, @BlackPepperId, 'Top'),
    (@BlackOpiumId, @OrangeBlossomId, 'Middle'),
    (@BlackOpiumId, @JasmineId, 'Middle'),
    (@BlackOpiumId, @VanillaId, 'Base'),
    (@BlackOpiumId, @PatchouliId, 'Base');

-- Armani Acqua di Gio
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@AcquaDiGioId, @BergamotId, 'Top'),
    (@AcquaDiGioId, @LemonId, 'Top'),
    (@AcquaDiGioId, @SeaNotesId, 'Middle'),
    (@AcquaDiGioId, @RoseId, 'Middle'),
    (@AcquaDiGioId, @MuskId, 'Base');

-- Chanel Coco Mademoiselle
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@CocoMademoiselleId, @BergamotId, 'Top'),
    (@CocoMademoiselleId, @OrangeBlossomId, 'Middle'),
    (@CocoMademoiselleId, @JasmineId, 'Middle'),
    (@CocoMademoiselleId, @RoseId, 'Middle'),
    (@CocoMademoiselleId, @PatchouliId, 'Base'),
    (@CocoMademoiselleId, @VanillaId, 'Base');

-- Dior J'adore
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@JadoreId, @BergamotId, 'Top'),
    (@JadoreId, @YlangYlangId, 'Middle'),
    (@JadoreId, @RoseId, 'Middle'),
    (@JadoreId, @JasmineId, 'Middle'),
    (@JadoreId, @SandalwoodId, 'Base');

-- Tom Ford Oud Wood
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@OudWoodId, @BergamotId, 'Top'),
    (@OudWoodId, @OudId, 'Middle'),
    (@OudWoodId, @SandalwoodId, 'Middle'),
    (@OudWoodId, @VetiverId, 'Base'),
    (@OudWoodId, @TonkaBeanId, 'Base');

-- Versace Eros
INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES 
    (@ErosId, @MintId, 'Top'),
    (@ErosId, @LemonId, 'Top'),
    (@ErosId, @AppleId, 'Top'),
    (@ErosId, @TonkaBeanId, 'Base'),
    (@ErosId, @VanillaId, 'Base');

-- ============================================
-- 11. INSERT PERFUME-ACCORD RELATIONSHIPS
-- ============================================

-- Get Accord IDs
DECLARE @CitrusAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Citrus');
DECLARE @FloralAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Floral');
DECLARE @WoodyAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Woody');
DECLARE @SpicyAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Spicy');
DECLARE @SweetAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Sweet');
DECLARE @FreshAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Fresh');
DECLARE @PowderyAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Powdery');
DECLARE @MuskAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Musky');
DECLARE @AmberAccordId UNIQUEIDENTIFIER = (SELECT Id FROM Accords WHERE Name = 'Amber');

-- Chanel No. 5
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@Chanel5Id, @FloralAccordId, 'Strong'),
    (@Chanel5Id, @PowderyAccordId, 'Medium'),
    (@Chanel5Id, @CitrusAccordId, 'Light');

-- Dior Sauvage
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@SauvageId, @FreshAccordId, 'Strong'),
    (@SauvageId, @SpicyAccordId, 'Medium'),
    (@SauvageId, @WoodyAccordId, 'Strong');

-- Tom Ford Black Orchid
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@BlackOrchidId, @FloralAccordId, 'Strong'),
    (@BlackOrchidId, @SweetAccordId, 'Strong'),
    (@BlackOrchidId, @WoodyAccordId, 'Medium');

-- Creed Aventus
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@AventusId, @FreshAccordId, 'Strong'),
    (@AventusId, @CitrusAccordId, 'Medium'),
    (@AventusId, @SweetAccordId, 'Light');

-- YSL Black Opium
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@BlackOpiumId, @SweetAccordId, 'Strong'),
    (@BlackOpiumId, @FloralAccordId, 'Medium'),
    (@BlackOpiumId, @SpicyAccordId, 'Light');

-- Armani Acqua di Gio
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@AcquaDiGioId, @FreshAccordId, 'Strong'),
    (@AcquaDiGioId, @CitrusAccordId, 'Medium'),
    (@AcquaDiGioId, @MuskAccordId, 'Light');

-- Chanel Coco Mademoiselle
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@CocoMademoiselleId, @FloralAccordId, 'Strong'),
    (@CocoMademoiselleId, @CitrusAccordId, 'Medium'),
    (@CocoMademoiselleId, @SweetAccordId, 'Medium');

-- Dior J'adore
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@JadoreId, @FloralAccordId, 'Strong'),
    (@JadoreId, @FreshAccordId, 'Light');

-- Tom Ford Oud Wood
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@OudWoodId, @WoodyAccordId, 'Strong'),
    (@OudWoodId, @SpicyAccordId, 'Medium'),
    (@OudWoodId, @AmberAccordId, 'Medium');

-- Versace Eros
INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES 
    (@ErosId, @FreshAccordId, 'Strong'),
    (@ErosId, @SweetAccordId, 'Medium'),
    (@ErosId, @CitrusAccordId, 'Light');

-- ============================================
-- 12. INSERT PERFUME-SEASON RELATIONSHIPS
-- ============================================

-- Get Season IDs
DECLARE @SpringId UNIQUEIDENTIFIER = (SELECT Id FROM Seasons WHERE Name = 'Spring');
DECLARE @SummerId UNIQUEIDENTIFIER = (SELECT Id FROM Seasons WHERE Name = 'Summer');
DECLARE @FallId UNIQUEIDENTIFIER = (SELECT Id FROM Seasons WHERE Name = 'Fall');
DECLARE @WinterId UNIQUEIDENTIFIER = (SELECT Id FROM Seasons WHERE Name = 'Winter');

-- Chanel No. 5 - All Seasons
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@Chanel5Id, @SpringId),
    (@Chanel5Id, @SummerId),
    (@Chanel5Id, @FallId),
    (@Chanel5Id, @WinterId);

-- Dior Sauvage - Spring, Summer, Fall
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@SauvageId, @SpringId),
    (@SauvageId, @SummerId),
    (@SauvageId, @FallId);

-- Tom Ford Black Orchid - Fall, Winter
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@BlackOrchidId, @FallId),
    (@BlackOrchidId, @WinterId);

-- Creed Aventus - Spring, Summer, Fall
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@AventusId, @SpringId),
    (@AventusId, @SummerId),
    (@AventusId, @FallId);

-- YSL Black Opium - Fall, Winter
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@BlackOpiumId, @FallId),
    (@BlackOpiumId, @WinterId);

-- Armani Acqua di Gio - Spring, Summer
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@AcquaDiGioId, @SpringId),
    (@AcquaDiGioId, @SummerId);

-- Chanel Coco Mademoiselle - All Seasons
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@CocoMademoiselleId, @SpringId),
    (@CocoMademoiselleId, @SummerId),
    (@CocoMademoiselleId, @FallId),
    (@CocoMademoiselleId, @WinterId);

-- Dior J'adore - Spring, Summer
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@JadoreId, @SpringId),
    (@JadoreId, @SummerId);

-- Tom Ford Oud Wood - Fall, Winter
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@OudWoodId, @FallId),
    (@OudWoodId, @WinterId);

-- Versace Eros - Spring, Summer, Fall
INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES 
    (@ErosId, @SpringId),
    (@ErosId, @SummerId),
    (@ErosId, @FallId);

-- ============================================
-- 13. INSERT PERFUME-OCCASION RELATIONSHIPS
-- ============================================

-- Get Occasion IDs
DECLARE @DailyWearId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Daily Wear');
DECLARE @OfficeId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Office');
DECLARE @EveningId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Evening');
DECLARE @NightOutId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Night Out');
DECLARE @DateNightId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Date Night');
DECLARE @FormalEventId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Formal Event');
DECLARE @CasualId UNIQUEIDENTIFIER = (SELECT Id FROM Occasions WHERE Name = 'Casual');

-- Chanel No. 5
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@Chanel5Id, @FormalEventId),
    (@Chanel5Id, @EveningId),
    (@Chanel5Id, @DateNightId);

-- Dior Sauvage
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@SauvageId, @DailyWearId),
    (@SauvageId, @OfficeId),
    (@SauvageId, @CasualId),
    (@SauvageId, @NightOutId);

-- Tom Ford Black Orchid
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@BlackOrchidId, @EveningId),
    (@BlackOrchidId, @NightOutId),
    (@BlackOrchidId, @DateNightId);

-- Creed Aventus
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@AventusId, @DailyWearId),
    (@AventusId, @OfficeId),
    (@AventusId, @FormalEventId);

-- YSL Black Opium
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@BlackOpiumId, @NightOutId),
    (@BlackOpiumId, @DateNightId),
    (@BlackOpiumId, @EveningId);

-- Armani Acqua di Gio
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@AcquaDiGioId, @DailyWearId),
    (@AcquaDiGioId, @CasualId),
    (@AcquaDiGioId, @OfficeId);

-- Chanel Coco Mademoiselle
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@CocoMademoiselleId, @DailyWearId),
    (@CocoMademoiselleId, @OfficeId),
    (@CocoMademoiselleId, @EveningId);

-- Dior J'adore
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@JadoreId, @DailyWearId),
    (@JadoreId, @EveningId),
    (@JadoreId, @FormalEventId);

-- Tom Ford Oud Wood
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@OudWoodId, @EveningId),
    (@OudWoodId, @FormalEventId);

-- Versace Eros
INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES 
    (@ErosId, @NightOutId),
    (@ErosId, @DateNightId),
    (@ErosId, @CasualId);

-- ============================================
-- 14. INSERT PERFUME-TAG RELATIONSHIPS
-- ============================================

-- Get Tag IDs
DECLARE @ClassicId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Classic');
DECLARE @ModernId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Modern');
DECLARE @LuxuryId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Luxury');
DECLARE @BestsellerId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Bestseller');
DECLARE @IconicId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Iconic');
DECLARE @SexyId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Sexy');
DECLARE @SophisticatedId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Sophisticated');
DECLARE @BoldId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Bold');
DECLARE @RomanticId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Romantic');
DECLARE @FreshTagId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Fresh');
DECLARE @VersatileId UNIQUEIDENTIFIER = (SELECT Id FROM Tags WHERE Name = 'Versatile');

-- Chanel No. 5
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@Chanel5Id, @ClassicId),
    (@Chanel5Id, @IconicId),
    (@Chanel5Id, @LuxuryId),
    (@Chanel5Id, @SophisticatedId);

-- Dior Sauvage
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@SauvageId, @ModernId),
    (@SauvageId, @BestsellerId),
    (@SauvageId, @VersatileId),
    (@SauvageId, @FreshTagId);

-- Tom Ford Black Orchid
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@BlackOrchidId, @LuxuryId),
    (@BlackOrchidId, @SexyId),
    (@BlackOrchidId, @BoldId),
    (@BlackOrchidId, @IconicId);

-- Creed Aventus
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@AventusId, @LuxuryId),
    (@AventusId, @IconicId),
    (@AventusId, @SophisticatedId),
    (@AventusId, @BestsellerId);

-- YSL Black Opium
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@BlackOpiumId, @ModernId),
    (@BlackOpiumId, @SexyId),
    (@BlackOpiumId, @BoldId),
    (@BlackOpiumId, @BestsellerId);

-- Armani Acqua di Gio
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@AcquaDiGioId, @ClassicId),
    (@AcquaDiGioId, @BestsellerId),
    (@AcquaDiGioId, @FreshTagId),
    (@AcquaDiGioId, @VersatileId);

-- Chanel Coco Mademoiselle
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@CocoMademoiselleId, @ModernId),
    (@CocoMademoiselleId, @SophisticatedId),
    (@CocoMademoiselleId, @RomanticId),
    (@CocoMademoiselleId, @BestsellerId);

-- Dior J'adore
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@JadoreId, @ClassicId),
    (@JadoreId, @RomanticId),
    (@JadoreId, @SophisticatedId),
    (@JadoreId, @IconicId);

-- Tom Ford Oud Wood
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@OudWoodId, @LuxuryId),
    (@OudWoodId, @SophisticatedId),
    (@OudWoodId, @BoldId);

-- Versace Eros
INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES 
    (@ErosId, @ModernId),
    (@ErosId, @SexyId),
    (@ErosId, @BoldId),
    (@ErosId, @BestsellerId);

-- ============================================
-- VERIFICATION QUERIES
-- ============================================

PRINT 'Database seeding completed successfully!';
PRINT '';
PRINT 'Summary:';
SELECT 'Brands' AS Entity, COUNT(*) AS Count FROM Brands
UNION ALL
SELECT 'Families', COUNT(*) FROM Families
UNION ALL
SELECT 'Notes', COUNT(*) FROM Notes
UNION ALL
SELECT 'Accords', COUNT(*) FROM Accords
UNION ALL
SELECT 'Seasons', COUNT(*) FROM Seasons
UNION ALL
SELECT 'Occasions', COUNT(*) FROM Occasions
UNION ALL
SELECT 'Tags', COUNT(*) FROM Tags
UNION ALL
SELECT 'Perfumes', COUNT(*) FROM Perfumes;

GO