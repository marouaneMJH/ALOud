# Amazon Perfume Scraper

This TypeScript scraper collects men's perfume data from Amazon to populate the ALOud database.

## Setup

Already installed dependencies. If needed, run:
```bash
npm install
```

## Usage

### Run Men's Perfume Scraper
```bash
npm run scrape:men
```

This will:
1. Scrape men's perfume listings from Amazon
2. Generate `output/men-perfumes.json` with CreateProductDto format
3. Generate `output/men-perfumes.sql` with INSERT statements
4. Default category: Woody (CategoryId: 2)

## Output Files

- `output/men-perfumes.json` - JSON array of CreateProductDto objects
- `output/men-perfumes.sql` - SQL INSERT statements ready to run

## Structure

```typescript
interface CreateProductDto {
  Name: string;          // Product name (max 200 chars)
  Description: string;   // Description (max 2000 chars)
  Price: number;         // Price in dollars
  Stock: number;         // Stock quantity (20-60 random)
  ImageUrl: string;      // Product image URL
  CategoryId: number;    // Category ID (2 = Woody for men)
}
```

## Features

- Respectful scraping with delays between requests
- Duplicate removal based on product name
- Auto-generates descriptions for masculine fragrances
- Handles missing prices with realistic defaults
- SQL injection safe with escaped quotes
- TypeScript type safety

## Notes

⚠️ **Important**: Amazon may block scrapers. If you get no results:
- The scraper includes proper headers and delays
- Consider using a proxy or VPN
- Amazon's HTML structure may change
- Rate limiting may occur

For production use, consider:
- Using Amazon Product Advertising API
- Rotating user agents
- Implementing proxy rotation
- Adding CAPTCHA handling

## Example Output

```json
{
  "Name": "Dior Sauvage Eau de Toilette",
  "Description": "Bold masculine fragrance with leather and tobacco accords",
  "Price": 125.00,
  "Stock": 42,
  "ImageUrl": "https://...",
  "CategoryId": 2
}
```
