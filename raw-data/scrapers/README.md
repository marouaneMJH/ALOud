# ALOud Perfume Scrapers

A collection of TypeScript scrapers to populate the ALOud perfume database with real data from the internet.

## Setup

Install dependencies:

```bash
cd raw-data/scrapers
npm install
```

## Scrapers Available

### 1. Database-Aware Scraper (Recommended)

This scraper fetches **real perfume data from the internet** and matches it with existing database entities (brands, families, notes, accords, etc.).

#### Run Scraper for Database Brands

```bash
npm run scrape:db
```

This will:
1. Connect to your database and load existing brands, families, notes, accords, tags, seasons, and occasions
2. For each brand in the database, search Fragrantica for perfumes
3. Scrape detailed perfume information from the web
4. Match scraped data with existing database entities
5. Generate SQL INSERT statements that use existing entity IDs

#### Scrape with Search Query

```bash
npm run scrape:search "oud perfumes"
```

#### Command Line Options

```bash
# Scrape perfumes for brands in the database (default)
npx ts-node src/db-aware-scraper.ts --brands

# Limit perfumes per brand
npx ts-node src/db-aware-scraper.ts --brands --max 5

# Search for specific perfumes
npx ts-node src/db-aware-scraper.ts --search "luxury oud"

# Scrape specific URLs
npx ts-node src/db-aware-scraper.ts --urls "https://www.fragrantica.com/perfume/..."
```

#### Output Files

- `output/scraped-perfumes-raw.json` - Raw scraped data from the internet
- `output/scraped-perfumes-db.json` - Database-ready format with matched IDs
- `output/scraped-perfumes-insert.sql` - SQL INSERT statements ready to run

### 2. Fragrantica Scraper (Curated Data)

Uses pre-defined curated perfume data:

```bash
npm run scrape:perfumes
```

### 3. Amazon Men's Perfume Scraper

Scrapes men's perfume listings from Amazon:

```bash
npm run scrape:men
```

## Database Connection

The database-aware scraper reads connection settings from your `appsettings.json`:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=VeloStoreDB;User Id=SA;Password=...;TrustServerCertificate=True"
    }
}
```

Or you can pass a connection string programmatically:

```typescript
const scraper = new DbAwarePerfumeScraper("Server=localhost;Database=MyDB;...");
```

## How It Works

1. **Load Master Data**: Connects to SQL Server and loads all brands, families, notes, accords, tags, seasons, and occasions

2. **Scrape Web Data**: Fetches real perfume information from Fragrantica including:
   - Name and brand
   - Description
   - Image URL
   - Gender profile
   - Notes (top, middle, base)
   - Accords with intensity
   - Price estimation

3. **Match to Database**: Uses fuzzy matching to link scraped data to existing entities:
   - Brand names are matched exactly or partially
   - Notes, accords, families are matched using word similarity
   - Seasons and occasions are mapped from English to French

4. **Generate SQL**: Creates INSERT statements that reference existing entity IDs

## Example Output

```json
{
    "perfumeId": "uuid-here",
    "name": "Sauvage",
    "brandId": "existing-brand-id",
    "families": ["existing-family-id-1"],
    "notes": [
        { "noteId": "existing-note-id", "level": "Top" }
    ],
    "accords": [
        { "accordId": "existing-accord-id", "intensity": "Strong" }
    ]
}
```

## Notes

⚠️ **Important**:
- The scraper includes respectful delays (2-4 seconds) between requests
- Fragrantica may block excessive scraping - use responsibly
- Make sure you have brands, families, notes, etc. in your database first
- Run seed data migrations before using this scraper

## Troubleshooting

**"Brand not found in DB"**
- The brand from the scraped perfume doesn't exist in your database
- Add the brand first using the admin panel

**Connection errors**
- Ensure SQL Server is running
- Check connection string in appsettings.json
- Verify database user has read access

**No perfumes scraped**
- Check internet connection
- Fragrantica might be blocking requests
- Try with --search option for specific queries

