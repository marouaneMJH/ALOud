import axios from "axios";
import * as cheerio from "cheerio";
import * as fs from "fs";
import * as path from "path";
import { CreateProductDto, ScrapedProduct } from "./types";

class MenPerfumeScraper {
    private readonly baseUrl = "https://www.amazon.com";
    private readonly searchUrl = "/s?k=men+perfume+cologne";
    private readonly userAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
    private readonly defaultStock = 35; // Default stock level

    private getRandomCategoryId(): number {
        return Math.floor(Math.random() * 9) + 1; // Random number between 1 and 9
    }

    private delay(ms: number): Promise<void> {
        return new Promise((resolve) => setTimeout(resolve, ms));
    }

    private cleanText(text: string): string {
        return text.trim().replace(/\s+/g, " ").replace(/\n/g, " ");
    }

    private extractPrice(priceText: string): number {
        const match = priceText.match(/[\d,]+\.?\d*/);
        if (match) {
            return parseFloat(match[0].replace(",", ""));
        }
        // Generate random price between $50 and $200 for men's perfumes
        return Math.floor(Math.random() * (200 - 50 + 1)) + 50;
    }

    private generateDescription(name: string): string {
        const descriptors = [
            "A sophisticated blend with woody and spicy notes",
            "Rich composition featuring amber and sandalwood",
            "Bold masculine fragrance with leather and tobacco accords",
            "Fresh and energetic with citrus and aquatic notes",
            "Timeless elegance with cedar and vetiver",
            "Modern and confident with oud and bergamot",
            "Classic masculine scent with patchouli and musk",
            "Intense and captivating with pepper and incense",
            "Refined fragrance with cardamom and tonka bean",
            "Dynamic blend of grapefruit and woody notes",
        ];

        return descriptors[Math.floor(Math.random() * descriptors.length)];
    }

    private getHighResImageUrl(imageUrl: string): string {
        if (!imageUrl || imageUrl.includes('placeholder')) {
            return imageUrl;
        }
        
        // Convert Amazon low-res image URLs to high-res
        // Replace common size parameters with larger ones
        return imageUrl
            .replace(/_AC_UL320_/g, '_AC_UL1500_')
            .replace(/_AC_UL\d+_/g, '_AC_UL1500_')
            .replace(/\._AC_SR\d+,\d+_/g, '._AC_SL1500_')
            .replace(/\._SS\d+_/g, '._SS1500_')
            .replace(/\._SX\d+_/g, '._SX1500_')
            .replace(/\._SY\d+_/g, '._SY1500_');
    }

    async scrapePage(pageUrl: string): Promise<ScrapedProduct[]> {
        try {
            console.log(`Scraping: ${pageUrl}`);

            const response = await axios.get(pageUrl, {
                headers: {
                    "User-Agent": this.userAgent,
                    Accept: "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    "Accept-Language": "en-US,en;q=0.5",
                    "Accept-Encoding": "gzip, deflate, br",
                    Connection: "keep-alive",
                    "Upgrade-Insecure-Requests": "1",
                },
                timeout: 15000,
            });

            const $ = cheerio.load(response.data);
            const products: ScrapedProduct[] = [];

            // Amazon product selectors
            $('[data-component-type="s-search-result"]').each((_, element) => {
                try {
                    const $el = $(element);

                    const name = this.cleanText(
                        $el.find("h2 a span").first().text() ||
                            $el.find(".a-text-normal").first().text()
                    );

                    if (!name || name.length < 3) return;

                    const priceWhole = $el
                        .find(".a-price-whole")
                        .first()
                        .text();
                    const priceFraction = $el
                        .find(".a-price-fraction")
                        .first()
                        .text();
                    const priceText = priceWhole + (priceFraction || ".00");
                    const price = this.extractPrice(priceText);

                    const rawImageUrl =
                        $el.find("img.s-image").attr("src") ||
                        $el.find("img").first().attr("src") ||
                        "https://via.placeholder.com/1500/f7f7f5/0b0b0b?text=Men+Perfume";

                    const imageUrl = this.getHighResImageUrl(rawImageUrl);
                    const description = this.generateDescription(name);

                    const asin = $el.attr("data-asin");

                    products.push({
                        name: name.substring(0, 200), // Max length constraint
                        description: description.substring(0, 2000),
                        price: price,
                        imageUrl: imageUrl,
                        asin: asin,
                    });
                } catch (err) {
                    console.error("Error parsing product:", err);
                }
            });

            console.log(`Found ${products.length} products on this page`);
            return products;
        } catch (error) {
            if (axios.isAxiosError(error)) {
                console.error(
                    `HTTP Error: ${error.response?.status} - ${error.message}`
                );
            } else {
                console.error("Scraping error:", error);
            }
            return [];
        }
    }

    convertToCreateProductDto(products: ScrapedProduct[]): CreateProductDto[] {
        return products.map((product) => ({
            Name: product.name,
            Description: product.description,
            Price: product.price === 0 
                ? Math.floor(Math.random() * (3000 - 100 + 1)) + 100 
                : product.price,
            Stock: Math.floor(Math.random() * (60 - 20 + 1)) + 20, // Random stock 20-60
            ImageUrl: product.imageUrl,
            CategoryId: this.getRandomCategoryId(), // Random category between 1-9
        }));
    }

    async scrapeMultiplePages(
        maxPages: number = 5
    ): Promise<CreateProductDto[]> {
        const allProducts: ScrapedProduct[] = [];

        for (let page = 1; page <= maxPages; page++) {
            const pageUrl =
                page === 1
                    ? `${this.baseUrl}${this.searchUrl}`
                    : `${this.baseUrl}${this.searchUrl}&page=${page}`;

            const products = await this.scrapePage(pageUrl);
            allProducts.push(...products);

            // Be respectful with delays between requests
            if (page < maxPages) {
                const delayTime =
                    Math.floor(Math.random() * (3000 - 2000 + 1)) + 2000;
                console.log(`Waiting ${delayTime}ms before next page...`);
                await this.delay(delayTime);
            }
        }

        // Remove duplicates based on name
        const uniqueProducts = allProducts.filter(
            (product, index, self) =>
                index ===
                self.findIndex(
                    (p) => p.name.toLowerCase() === product.name.toLowerCase()
                )
        );

        console.log(
            `\nTotal unique products scraped: ${uniqueProducts.length}`
        );
        return this.convertToCreateProductDto(uniqueProducts);
    }

    saveToJson(
        products: CreateProductDto[],
        filename: string = "men-perfumes.json"
    ): void {
        const outputPath = path.join(__dirname, "..", "output", filename);
        const outputDir = path.dirname(outputPath);

        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }

        fs.writeFileSync(
            outputPath,
            JSON.stringify(products, null, 2),
            "utf-8"
        );
        console.log(`\nData saved to: ${outputPath}`);
        console.log(`Total products: ${products.length}`);
    }

    generateSqlInsert(products: CreateProductDto[]): string {
        const values = products
            .map((p) => {
                const name = p.Name.replace(/'/g, "''");
                const description = p.Description.replace(/'/g, "''");
                const imageUrl = p.ImageUrl.replace(/'/g, "''");

                return `('${name}', '${description}', ${p.Price}, ${p.Stock}, '${imageUrl}', ${p.CategoryId})`;
            })
            .join(",\n");

        return `-- Men's Perfumes Data
-- Generated on ${new Date().toISOString()}

INSERT INTO Products (Name, Description, Price, Stock, ImageUrl, CategoryId) VALUES
${values};
`;
    }

    saveToSql(
        products: CreateProductDto[],
        filename: string = "men-perfumes.sql"
    ): void {
        const outputPath = path.join(__dirname, "..", "output", filename);
        const outputDir = path.dirname(outputPath);

        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }

        const sqlContent = this.generateSqlInsert(products);
        fs.writeFileSync(outputPath, sqlContent, "utf-8");
        console.log(`SQL file saved to: ${outputPath}`);
    }
}

// Main execution
async function main() {
    console.log("=== Men's Perfume Scraper ===\n");

    const scraper = new MenPerfumeScraper();

    try {
        // Scrape multiple pages (adjust number as needed)
        const products = await scraper.scrapeMultiplePages(5);

        if (products.length > 0) {
            // Save as JSON
            scraper.saveToJson(products);

            // Save as SQL
            scraper.saveToSql(products);

            // Display sample
            console.log("\n=== Sample Products ===");
            products.slice(0, 3).forEach((p, i) => {
                console.log(`\n${i + 1}. ${p.Name}`);
                console.log(`   Price: $${p.Price}`);
                console.log(`   Stock: ${p.Stock}`);
                console.log(`   Description: ${p.Description}`);
            });
        } else {
            console.log("\nNo products were scraped. This might be due to:");
            console.log(
                "- Amazon blocking the requests (try with proxy or different approach)"
            );
            console.log("- Changes in Amazon's HTML structure");
            console.log("- Network issues");
        }
    } catch (error) {
        console.error("Fatal error:", error);
        process.exit(1);
    }
}

// Run if executed directly
if (require.main === module) {
    main();
}

export { MenPerfumeScraper };
