// import axios from "axios";
// import * as cheerio from "cheerio";
// import * as fs from "fs";
// import * as path from "path";
// import { v4 as uuidv4 } from "uuid";
// import { ScrapedPerfume, PerfumeDbData } from "./perfume-types";

// /**
//  * Fragrantica Perfume Scraper
//  * Scrapes real perfume data from Fragrantica.com
//  */
// class FragranticaScraper {
//     private readonly baseUrl = "https://www.fragrantica.com";
//     private readonly userAgent =
//         "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

//     // Master data collections
//     private brands = new Map<string, string>();
//     private families = new Map<string, string>();
//     private notes = new Map<string, { id: string; category: string }>();
//     private accords = new Map<string, string>();
//     private tags = new Map<string, string>();
//     private seasons = new Map<string, string>();
//     private occasions = new Map<string, string>();

//     // Initialize with default seasons & occasions
//     constructor() {
//         // Seasons
//         ["Printemps", "Été", "Automne", "Hiver"].forEach((s) => {
//             this.seasons.set(s, uuidv4());
//         });

//         // Occasions
//         [
//             "Quotidien",
//             "Bureau",
//             "Soirée",
//             "Rendez-vous",
//             "Mariage",
//             "Sport",
//             "Vacances",
//             "Cérémonie",
//         ].forEach((o) => {
//             this.occasions.set(o, uuidv4());
//         });
//     }

//     private delay(ms: number): Promise<void> {
//         return new Promise((resolve) => setTimeout(resolve, ms));
//     }

//     private cleanText(text: string): string {
//         return text.trim().replace(/\s+/g, " ").replace(/\n/g, " ");
//     }

//     private getOrCreateId(map: Map<string, string>, key: string): string {
//         const normalized = key.trim();
//         if (!map.has(normalized)) {
//             map.set(normalized, uuidv4());
//         }
//         return map.get(normalized)!;
//     }

//     private getOrCreateNoteId(
//         name: string,
//         category: string = "General"
//     ): string {
//         const normalized = name.trim();
//         if (!this.notes.has(normalized)) {
//             this.notes.set(normalized, { id: uuidv4(), category });
//         }
//         return this.notes.get(normalized)!.id;
//     }

//     /**
//      * Scrape a single perfume page from Fragrantica
//      */
//     async scrapePerfumePage(url: string): Promise<ScrapedPerfume | null> {
//         try {
//             console.log(`  Scraping: ${url}`);
//             const response = await axios.get(url, {
//                 headers: {
//                     "User-Agent": this.userAgent,
//                     Accept: "text/html,application/xhtml+xml",
//                     "Accept-Language": "en-US,en;q=0.9",
//                 },
//                 timeout: 15000,
//             });

//             const $ = cheerio.load(response.data);

//             // Extract perfume name and brand
//             const titleText = $("h1").first().text().trim();
//             const brandMatch = titleText.match(/^(.+?)\s+(.+)$/);

//             let brand = "Unknown";
//             let name = titleText;

//             // Try to extract brand from the page
//             const brandLink = $('span[itemprop="name"] a')
//                 .first()
//                 .text()
//                 .trim();
//             if (brandLink) {
//                 brand = brandLink;
//                 name = titleText.replace(brand, "").trim();
//             } else if (brandMatch) {
//                 brand = brandMatch[1];
//                 name = brandMatch[2];
//             }

//             if (!name || name.length < 2) return null;

//             // Extract image
//             let imageUrl =
//                 $('img[itemprop="image"]').attr("src") ||
//                 $(".perfume-image img").attr("src") ||
//                 $('img[alt*="perfume"]').first().attr("src") ||
//                 "";

//             if (imageUrl && !imageUrl.startsWith("http")) {
//                 imageUrl = `https://www.fragrantica.com${imageUrl}`;
//             }

//             // Extract gender
//             let gender: "Male" | "Female" | "Unisex" = "Unisex";
//             const genderText = $(".vote-button-name").text().toLowerCase();
//             if (genderText.includes("men") || genderText.includes("male")) {
//                 gender = "Male";
//             } else if (
//                 genderText.includes("women") ||
//                 genderText.includes("female")
//             ) {
//                 gender = "Female";
//             }

//             // Extract description
//             const description =
//                 this.cleanText($('[itemprop="description"]').text()) ||
//                 this.cleanText($(".fragrantica-blockquote").first().text()) ||
//                 `${name} by ${brand} - A distinctive fragrance.`;

//             // Extract accords
//             const accords: { name: string; intensity: string }[] = [];
//             $(".accord-bar").each((_, el) => {
//                 const accordName = $(el).find(".accord-box").text().trim();
//                 const width = $(el)
//                     .attr("style")
//                     ?.match(/width:\s*(\d+)/)?.[1];
//                 const intensity = width
//                     ? parseInt(width) > 70
//                         ? "Strong"
//                         : parseInt(width) > 40
//                         ? "Medium"
//                         : "Light"
//                     : "Medium";
//                 if (accordName) {
//                     accords.push({ name: accordName, intensity });
//                 }
//             });

//             // Extract notes
//             const notes: { name: string; level: "Top" | "Middle" | "Base" }[] =
//                 [];

//             // Top notes
//             $(
//                 'pyramid-level[notes-layer="top"] div.notes-box, .notes-box:contains("Top")'
//             ).each((_, el) => {
//                 $(el)
//                     .find("a, span")
//                     .each((_, noteEl) => {
//                         const noteName = $(noteEl).text().trim();
//                         if (
//                             noteName &&
//                             noteName.length > 1 &&
//                             noteName.length < 50
//                         ) {
//                             notes.push({ name: noteName, level: "Top" });
//                         }
//                     });
//             });

//             // Heart/Middle notes
//             $(
//                 'pyramid-level[notes-layer="heart"] div.notes-box, .notes-box:contains("Heart"), .notes-box:contains("Middle")'
//             ).each((_, el) => {
//                 $(el)
//                     .find("a, span")
//                     .each((_, noteEl) => {
//                         const noteName = $(noteEl).text().trim();
//                         if (
//                             noteName &&
//                             noteName.length > 1 &&
//                             noteName.length < 50
//                         ) {
//                             notes.push({ name: noteName, level: "Middle" });
//                         }
//                     });
//             });

//             // Base notes
//             $(
//                 'pyramid-level[notes-layer="base"] div.notes-box, .notes-box:contains("Base")'
//             ).each((_, el) => {
//                 $(el)
//                     .find("a, span")
//                     .each((_, noteEl) => {
//                         const noteName = $(noteEl).text().trim();
//                         if (
//                             noteName &&
//                             noteName.length > 1 &&
//                             noteName.length < 50
//                         ) {
//                             notes.push({ name: noteName, level: "Base" });
//                         }
//                     });
//             });

//             // Extract families from main accords or categories
//             const families: string[] = [];
//             $(".category-name, .perfume-categories a").each((_, el) => {
//                 const family = $(el).text().trim();
//                 if (family && family.length > 2) {
//                     families.push(family);
//                 }
//             });

//             // If no families found, derive from accords
//             if (families.length === 0 && accords.length > 0) {
//                 families.push(accords[0].name);
//             }

//             // Generate realistic attributes
//             const intensityOptions = [
//                 "Légère",
//                 "Modérée",
//                 "Forte",
//                 "Très forte",
//             ];
//             const longevityOptions = [
//                 "2-4 heures",
//                 "4-6 heures",
//                 "6-8 heures",
//                 "8-12 heures",
//                 "+12 heures",
//             ];
//             const sillageOptions = ["Intime", "Modéré", "Fort", "Énorme"];
//             const priceRanges = ["$", "$$", "$$$", "$$$$"];

//             // Determine characteristics based on accords
//             const hasOud = accords.some((a) =>
//                 a.name.toLowerCase().includes("oud")
//             );
//             const hasAmber = accords.some((a) =>
//                 a.name.toLowerCase().includes("amber")
//             );
//             const isFresh = accords.some((a) =>
//                 ["citrus", "fresh", "aquatic", "green"].some((f) =>
//                     a.name.toLowerCase().includes(f)
//                 )
//             );

//             const intensity =
//                 hasOud || hasAmber
//                     ? intensityOptions[2 + Math.floor(Math.random() * 2)]
//                     : isFresh
//                     ? intensityOptions[Math.floor(Math.random() * 2)]
//                     : intensityOptions[1 + Math.floor(Math.random() * 2)];

//             const longevity =
//                 hasOud || hasAmber
//                     ? longevityOptions[3 + Math.floor(Math.random() * 2)]
//                     : isFresh
//                     ? longevityOptions[Math.floor(Math.random() * 2)]
//                     : longevityOptions[1 + Math.floor(Math.random() * 3)];

//             const sillage = hasOud
//                 ? sillageOptions[2 + Math.floor(Math.random() * 2)]
//                 : sillageOptions[Math.floor(Math.random() * 3)];

//             // Price based on brand prestige
//             const luxuryBrands = [
//                 "Creed",
//                 "Tom Ford",
//                 "Maison Francis Kurkdjian",
//                 "Byredo",
//                 "Le Labo",
//                 "Amouage",
//                 "Parfums de Marly",
//                 "Xerjoff",
//                 "Roja Parfums",
//                 "Clive Christian",
//             ];
//             const premiumBrands = [
//                 "Dior",
//                 "Chanel",
//                 "Guerlain",
//                 "YSL",
//                 "Givenchy",
//                 "Hermès",
//                 "Armani",
//                 "Prada",
//                 "Versace",
//                 "Dolce & Gabbana",
//             ];

//             let priceRange = "$$";
//             let price = 250 + Math.floor(Math.random() * 500);

//             if (
//                 luxuryBrands.some((b) =>
//                     brand.toLowerCase().includes(b.toLowerCase())
//                 )
//             ) {
//                 priceRange = "$$$$";
//                 price = 1500 + Math.floor(Math.random() * 3000);
//             } else if (
//                 premiumBrands.some((b) =>
//                     brand.toLowerCase().includes(b.toLowerCase())
//                 )
//             ) {
//                 priceRange = "$$$";
//                 price = 600 + Math.floor(Math.random() * 1200);
//             } else if (hasOud) {
//                 priceRange = "$$$";
//                 price = 800 + Math.floor(Math.random() * 1500);
//             }

//             // Generate tags based on characteristics
//             const tags: string[] = [];
//             if (isFresh) tags.push("Frais", "Énergisant");
//             if (hasOud) tags.push("Oriental", "Luxueux", "Boisé");
//             if (hasAmber) tags.push("Chaleureux", "Sensuel");
//             if (gender === "Male") tags.push("Masculin", "Élégant");
//             if (gender === "Female") tags.push("Féminin", "Raffiné");
//             if (gender === "Unisex") tags.push("Unisexe", "Moderne");
//             tags.push("Signature", "Tendance");

//             // Seasons based on characteristics
//             const seasons: string[] = [];
//             if (isFresh) {
//                 seasons.push("Printemps", "Été");
//             } else if (hasOud || hasAmber) {
//                 seasons.push("Automne", "Hiver");
//             } else {
//                 seasons.push("Printemps", "Automne");
//             }

//             // Occasions
//             const occasions: string[] = [];
//             if (priceRange === "$$$$") {
//                 occasions.push("Cérémonie", "Soirée", "Mariage");
//             } else if (isFresh) {
//                 occasions.push("Quotidien", "Bureau", "Sport");
//             } else {
//                 occasions.push("Soirée", "Rendez-vous", "Bureau");
//             }

//             const stockQuantity = Math.floor(Math.random() * 50) + 5;

//             return {
//                 name,
//                 brand,
//                 description: description.substring(0, 2000),
//                 imageUrl,
//                 gender,
//                 intensity,
//                 longevity,
//                 sillage,
//                 priceRange,
//                 price,
//                 stockQuantity,
//                 families: families.slice(0, 3),
//                 notes: notes.slice(0, 15),
//                 accords: accords.slice(0, 8),
//                 tags: [...new Set(tags)].slice(0, 6),
//                 seasons,
//                 occasions,
//             };
//         } catch (error) {
//             console.error(`    Error scraping ${url}:`, error);
//             return null;
//         }
//     }

//     /**
//      * Get perfume links from a search/category page
//      */
//     async getPerfumeLinks(
//         categoryUrl: string,
//         maxLinks: number = 20
//     ): Promise<string[]> {
//         try {
//             console.log(`Getting perfume links from: ${categoryUrl}`);
//             const response = await axios.get(categoryUrl, {
//                 headers: {
//                     "User-Agent": this.userAgent,
//                     Accept: "text/html",
//                 },
//                 timeout: 15000,
//             });

//             const $ = cheerio.load(response.data);
//             const links: string[] = [];

//             $('a[href*="/perfume/"]').each((_, el) => {
//                 const href = $(el).attr("href");
//                 if (
//                     href &&
//                     !href.includes("/reviews") &&
//                     !href.includes("/votes")
//                 ) {
//                     const fullUrl = href.startsWith("http")
//                         ? href
//                         : `${this.baseUrl}${href}`;
//                     if (!links.includes(fullUrl)) {
//                         links.push(fullUrl);
//                     }
//                 }
//             });

//             console.log(`  Found ${links.length} perfume links`);
//             return links.slice(0, maxLinks);
//         } catch (error) {
//             console.error("Error getting perfume links:", error);
//             return [];
//         }
//     }

//     /**
//      * Generate curated perfume data with real information
//      */
//     generateCuratedPerfumes(): ScrapedPerfume[] {
//         // Real perfume data from well-known fragrances
//         const curatedPerfumes: ScrapedPerfume[] = [
//             // Men's Fragrances
//             {
//                 name: "Sauvage",
//                 brand: "Dior",
//                 description:
//                     "Sauvage est une fragrance audacieuse et radicalement fraîche composée de notes de bergamote de Calabre et d'un accord ambroxan boisé. Un parfum noble et masculin qui évoque les grands espaces.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.56585.jpg",
//                 gender: "Male",
//                 intensity: "Forte",
//                 longevity: "8-12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$",
//                 price: 1200,
//                 stockQuantity: 45,
//                 families: ["Aromatic", "Fougère"],
//                 notes: [
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Poivre", level: "Top" },
//                     { name: "Lavande", level: "Middle" },
//                     { name: "Géranium", level: "Middle" },
//                     { name: "Ambroxan", level: "Base" },
//                     { name: "Cèdre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Aromatic", intensity: "Strong" },
//                     { name: "Fresh Spicy", intensity: "Medium" },
//                     { name: "Woody", intensity: "Medium" },
//                 ],
//                 tags: ["Best-seller", "Masculin", "Signature", "Moderne"],
//                 seasons: ["Printemps", "Été", "Automne"],
//                 occasions: ["Quotidien", "Bureau", "Soirée"],
//             },
//             {
//                 name: "Bleu de Chanel",
//                 brand: "Chanel",
//                 description:
//                     "Bleu de Chanel est un parfum boisé aromatique qui exprime la liberté. Un homme qui s'affranchit des conventions et qui cultive ce qu'il a d'unique. Notes de citrus, menthe, cèdre et bois de santal.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.25967.jpg",
//                 gender: "Male",
//                 intensity: "Modérée",
//                 longevity: "6-8 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$$",
//                 price: 1350,
//                 stockQuantity: 38,
//                 families: ["Woody", "Aromatic"],
//                 notes: [
//                     { name: "Citron", level: "Top" },
//                     { name: "Menthe", level: "Top" },
//                     { name: "Pamplemousse", level: "Top" },
//                     { name: "Gingembre", level: "Middle" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Cèdre", level: "Base" },
//                     { name: "Santal", level: "Base" },
//                     { name: "Encens", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Woody", intensity: "Strong" },
//                     { name: "Fresh", intensity: "Medium" },
//                     { name: "Citrus", intensity: "Medium" },
//                 ],
//                 tags: ["Classique", "Élégant", "Signature", "Intemporel"],
//                 seasons: ["Printemps", "Automne"],
//                 occasions: ["Bureau", "Soirée", "Rendez-vous"],
//             },
//             {
//                 name: "Aventus",
//                 brand: "Creed",
//                 description:
//                     "Aventus célèbre la force, la vision et le succès. Ce parfum emblématique s'ouvre sur des notes fraîches d'ananas et de pomme, révélant un cœur de bouleau et de jasmin, sur un fond de musc et de chêne.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.9828.jpg",
//                 gender: "Male",
//                 intensity: "Forte",
//                 longevity: "+12 heures",
//                 sillage: "Énorme",
//                 priceRange: "$$$$",
//                 price: 3500,
//                 stockQuantity: 15,
//                 families: ["Fruity", "Chypre"],
//                 notes: [
//                     { name: "Ananas", level: "Top" },
//                     { name: "Pomme", level: "Top" },
//                     { name: "Cassis", level: "Top" },
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Rose", level: "Middle" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Bouleau", level: "Middle" },
//                     { name: "Mousse de Chêne", level: "Base" },
//                     { name: "Musc", level: "Base" },
//                     { name: "Ambre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Fruity", intensity: "Strong" },
//                     { name: "Smoky", intensity: "Medium" },
//                     { name: "Woody", intensity: "Strong" },
//                 ],
//                 tags: ["Luxueux", "Iconique", "Signature", "Prestige"],
//                 seasons: ["Printemps", "Été", "Automne"],
//                 occasions: ["Cérémonie", "Soirée", "Bureau"],
//             },
//             {
//                 name: "Oud Wood",
//                 brand: "Tom Ford",
//                 description:
//                     "Oud Wood est une interprétation moderne et rare du bois d'oud. L'exotisme du bois d'oud se mêle au bois de rose, au cardamome et au bois de santal pour créer une composition subtile et fumée.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.7829.jpg",
//                 gender: "Unisex",
//                 intensity: "Forte",
//                 longevity: "8-12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$$",
//                 price: 2800,
//                 stockQuantity: 22,
//                 families: ["Woody", "Oriental"],
//                 notes: [
//                     { name: "Bois de Rose", level: "Top" },
//                     { name: "Cardamome", level: "Top" },
//                     { name: "Poivre Sichuan", level: "Top" },
//                     { name: "Oud", level: "Middle" },
//                     { name: "Santal", level: "Middle" },
//                     { name: "Vétiver", level: "Base" },
//                     { name: "Ambre", level: "Base" },
//                     { name: "Tonka", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Oud", intensity: "Strong" },
//                     { name: "Woody", intensity: "Strong" },
//                     { name: "Spicy", intensity: "Medium" },
//                 ],
//                 tags: ["Oriental", "Luxueux", "Boisé", "Signature"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Soirée", "Cérémonie", "Rendez-vous"],
//             },
//             {
//                 name: "Acqua di Gio Profumo",
//                 brand: "Giorgio Armani",
//                 description:
//                     "Acqua di Gio Profumo est une interprétation plus intense et sophistiquée du classique Acqua di Gio. Un mélange d'aquatique, d'encens et de notes boisées pour un homme moderne et confiant.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.28900.jpg",
//                 gender: "Male",
//                 intensity: "Modérée",
//                 longevity: "6-8 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$$",
//                 price: 950,
//                 stockQuantity: 52,
//                 families: ["Aquatic", "Aromatic"],
//                 notes: [
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Aquatique", level: "Top" },
//                     { name: "Géranium", level: "Middle" },
//                     { name: "Sauge", level: "Middle" },
//                     { name: "Romarin", level: "Middle" },
//                     { name: "Encens", level: "Base" },
//                     { name: "Patchouli", level: "Base" },
//                     { name: "Ambre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Aquatic", intensity: "Strong" },
//                     { name: "Aromatic", intensity: "Medium" },
//                     { name: "Woody", intensity: "Medium" },
//                 ],
//                 tags: ["Frais", "Classique", "Élégant", "Intemporel"],
//                 seasons: ["Printemps", "Été"],
//                 occasions: ["Quotidien", "Bureau", "Vacances"],
//             },
//             // Women's Fragrances
//             {
//                 name: "Miss Dior",
//                 brand: "Dior",
//                 description:
//                     "Miss Dior est un chypre fleuri moderne qui célèbre l'amour et la fraîcheur. Des notes de rose de Grasse et de pivoine s'épanouissent sur un fond de patchouli et de musc blanc.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.48270.jpg",
//                 gender: "Female",
//                 intensity: "Modérée",
//                 longevity: "6-8 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$$",
//                 price: 1150,
//                 stockQuantity: 48,
//                 families: ["Floral", "Chypre"],
//                 notes: [
//                     { name: "Mandarine", level: "Top" },
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Rose de Grasse", level: "Middle" },
//                     { name: "Pivoine", level: "Middle" },
//                     { name: "Iris", level: "Middle" },
//                     { name: "Patchouli", level: "Base" },
//                     { name: "Musc Blanc", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Floral", intensity: "Strong" },
//                     { name: "Powdery", intensity: "Medium" },
//                     { name: "Fresh", intensity: "Medium" },
//                 ],
//                 tags: ["Féminin", "Romantique", "Élégant", "Classique"],
//                 seasons: ["Printemps", "Automne"],
//                 occasions: ["Rendez-vous", "Bureau", "Cérémonie"],
//             },
//             {
//                 name: "Coco Mademoiselle",
//                 brand: "Chanel",
//                 description:
//                     "Coco Mademoiselle est un oriental frais irrésistible et imprévisible. Une essence fraîche et pétillante d'orange et de jasmin, avec un fond sensuel de patchouli et de musc blanc.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.611.jpg",
//                 gender: "Female",
//                 intensity: "Modérée",
//                 longevity: "6-8 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$$",
//                 price: 1400,
//                 stockQuantity: 35,
//                 families: ["Oriental", "Floral"],
//                 notes: [
//                     { name: "Orange", level: "Top" },
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Rose", level: "Middle" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Litchi", level: "Middle" },
//                     { name: "Patchouli", level: "Base" },
//                     { name: "Vétiver", level: "Base" },
//                     { name: "Musc Blanc", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Fresh", intensity: "Strong" },
//                     { name: "Floral", intensity: "Strong" },
//                     { name: "Oriental", intensity: "Medium" },
//                 ],
//                 tags: ["Iconique", "Féminin", "Sensuel", "Best-seller"],
//                 seasons: ["Printemps", "Été", "Automne"],
//                 occasions: ["Quotidien", "Bureau", "Soirée"],
//             },
//             {
//                 name: "La Vie Est Belle",
//                 brand: "Lancôme",
//                 description:
//                     "La Vie Est Belle est une déclaration au bonheur. Un iris gourmand magnifié par un cœur de jasmin et fleur d'oranger, sur un fond de patchouli et de praline pour une douceur addictive.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.15255.jpg",
//                 gender: "Female",
//                 intensity: "Modérée",
//                 longevity: "8-12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$",
//                 price: 980,
//                 stockQuantity: 60,
//                 families: ["Gourmand", "Floral"],
//                 notes: [
//                     { name: "Cassis", level: "Top" },
//                     { name: "Poire", level: "Top" },
//                     { name: "Iris", level: "Middle" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Fleur d'Oranger", level: "Middle" },
//                     { name: "Praline", level: "Base" },
//                     { name: "Patchouli", level: "Base" },
//                     { name: "Vanille", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Gourmand", intensity: "Strong" },
//                     { name: "Floral", intensity: "Medium" },
//                     { name: "Sweet", intensity: "Strong" },
//                 ],
//                 tags: ["Gourmand", "Féminin", "Doux", "Best-seller"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Quotidien", "Rendez-vous", "Soirée"],
//             },
//             {
//                 name: "Black Opium",
//                 brand: "Yves Saint Laurent",
//                 description:
//                     "Black Opium est une overdose de café et de vanille, une addiction féminine et rock. Un parfum mystérieux et envoûtant qui marie café noir, fleur d'oranger et vanille.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.25324.jpg",
//                 gender: "Female",
//                 intensity: "Forte",
//                 longevity: "8-12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$",
//                 price: 1100,
//                 stockQuantity: 42,
//                 families: ["Oriental", "Gourmand"],
//                 notes: [
//                     { name: "Café", level: "Top" },
//                     { name: "Mandarine", level: "Top" },
//                     { name: "Poire", level: "Top" },
//                     { name: "Fleur d'Oranger", level: "Middle" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Vanille", level: "Base" },
//                     { name: "Cèdre", level: "Base" },
//                     { name: "Patchouli", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Coffee", intensity: "Strong" },
//                     { name: "Vanilla", intensity: "Strong" },
//                     { name: "Sweet", intensity: "Medium" },
//                 ],
//                 tags: ["Addictif", "Sensuel", "Moderne", "Rock"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Soirée", "Rendez-vous", "Cérémonie"],
//             },
//             {
//                 name: "Flowerbomb",
//                 brand: "Viktor & Rolf",
//                 description:
//                     "Flowerbomb est une explosion florale qui transforme le négatif en positif. Un bouquet intense de jasmin, rose, freesia et orchidée sur un fond gourmand de patchouli.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.2619.jpg",
//                 gender: "Female",
//                 intensity: "Forte",
//                 longevity: "8-12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$",
//                 price: 1050,
//                 stockQuantity: 55,
//                 families: ["Floral", "Oriental"],
//                 notes: [
//                     { name: "Thé", level: "Top" },
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Jasmin Sambac", level: "Middle" },
//                     { name: "Rose Centifolia", level: "Middle" },
//                     { name: "Freesia", level: "Middle" },
//                     { name: "Orchidée", level: "Middle" },
//                     { name: "Patchouli", level: "Base" },
//                     { name: "Musc", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Floral", intensity: "Strong" },
//                     { name: "Sweet", intensity: "Strong" },
//                     { name: "Powdery", intensity: "Medium" },
//                 ],
//                 tags: ["Floral", "Intense", "Féminin", "Signature"],
//                 seasons: ["Automne", "Hiver", "Printemps"],
//                 occasions: ["Soirée", "Rendez-vous", "Cérémonie"],
//             },
//             // Unisex / Niche
//             {
//                 name: "Baccarat Rouge 540",
//                 brand: "Maison Francis Kurkdjian",
//                 description:
//                     "Baccarat Rouge 540 est une création magistrale qui fusionne le jasmin, le safran et le bois de cèdre avec une overdose d'ambre et de musc. Une signature olfactive unique et moderne.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.33519.jpg",
//                 gender: "Unisex",
//                 intensity: "Très forte",
//                 longevity: "+12 heures",
//                 sillage: "Énorme",
//                 priceRange: "$$$$",
//                 price: 3200,
//                 stockQuantity: 18,
//                 families: ["Amber", "Floral"],
//                 notes: [
//                     { name: "Safran", level: "Top" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Ambre", level: "Base" },
//                     { name: "Cèdre", level: "Base" },
//                     { name: "Musc", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Amber", intensity: "Strong" },
//                     { name: "Sweet", intensity: "Strong" },
//                     { name: "Woody", intensity: "Medium" },
//                 ],
//                 tags: ["Luxueux", "Iconique", "Moderne", "Signature"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Cérémonie", "Soirée", "Rendez-vous"],
//             },
//             {
//                 name: "Tobacco Vanille",
//                 brand: "Tom Ford",
//                 description:
//                     "Tobacco Vanille capture l'essence des clubs privés anglais. Une fusion opulente de feuilles de tabac, vanille, cacao et fruits secs pour une chaleur enveloppante et addictive.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.1825.jpg",
//                 gender: "Unisex",
//                 intensity: "Très forte",
//                 longevity: "+12 heures",
//                 sillage: "Énorme",
//                 priceRange: "$$$$",
//                 price: 2900,
//                 stockQuantity: 20,
//                 families: ["Oriental", "Spicy"],
//                 notes: [
//                     { name: "Tabac", level: "Top" },
//                     { name: "Épices", level: "Top" },
//                     { name: "Vanille", level: "Middle" },
//                     { name: "Cacao", level: "Middle" },
//                     { name: "Fruits Secs", level: "Base" },
//                     { name: "Bois", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Tobacco", intensity: "Strong" },
//                     { name: "Vanilla", intensity: "Strong" },
//                     { name: "Warm Spicy", intensity: "Medium" },
//                 ],
//                 tags: ["Opulent", "Chaleureux", "Addictif", "Signature"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Soirée", "Cérémonie", "Rendez-vous"],
//             },
//             {
//                 name: "Santal 33",
//                 brand: "Le Labo",
//                 description:
//                     "Santal 33 est un santal addictif et mystérieux. Notes de carvi, iris et violet sur un cœur de santal et de cèdre, avec un fond cuiré et musqué. Le parfum culte de NYC.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.15949.jpg",
//                 gender: "Unisex",
//                 intensity: "Modérée",
//                 longevity: "8-12 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$$$",
//                 price: 2600,
//                 stockQuantity: 25,
//                 families: ["Woody", "Aromatic"],
//                 notes: [
//                     { name: "Carvi", level: "Top" },
//                     { name: "Iris", level: "Top" },
//                     { name: "Violet", level: "Top" },
//                     { name: "Santal", level: "Middle" },
//                     { name: "Papyrus", level: "Middle" },
//                     { name: "Cuir", level: "Base" },
//                     { name: "Ambre", level: "Base" },
//                     { name: "Cèdre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Sandalwood", intensity: "Strong" },
//                     { name: "Leather", intensity: "Medium" },
//                     { name: "Woody", intensity: "Strong" },
//                 ],
//                 tags: ["Culte", "Moderne", "Minimaliste", "NYC"],
//                 seasons: ["Printemps", "Automne"],
//                 occasions: ["Quotidien", "Bureau", "Soirée"],
//             },
//             {
//                 name: "Noir de Noir",
//                 brand: "Tom Ford",
//                 description:
//                     "Noir de Noir est un floral sombre et sensuel. Rose noire et safran sur un cœur de truffe et vanille, avec un fond de patchouli, oud et mousse de chêne. Pure décadence.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.3055.jpg",
//                 gender: "Unisex",
//                 intensity: "Très forte",
//                 longevity: "+12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$$",
//                 price: 3100,
//                 stockQuantity: 12,
//                 families: ["Oriental", "Floral"],
//                 notes: [
//                     { name: "Rose Noire", level: "Top" },
//                     { name: "Safran", level: "Top" },
//                     { name: "Truffe", level: "Middle" },
//                     { name: "Vanille", level: "Middle" },
//                     { name: "Patchouli", level: "Base" },
//                     { name: "Oud", level: "Base" },
//                     { name: "Mousse de Chêne", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Rose", intensity: "Strong" },
//                     { name: "Oud", intensity: "Medium" },
//                     { name: "Earthy", intensity: "Strong" },
//                 ],
//                 tags: ["Sombre", "Sensuel", "Décadent", "Luxueux"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Soirée", "Rendez-vous", "Cérémonie"],
//             },
//             {
//                 name: "Lost Cherry",
//                 brand: "Tom Ford",
//                 description:
//                     "Lost Cherry est un élixir gourmand de cerise noire et d'amande amère. Notes de griotte, liqueur de cerise et amande sur un fond de santal, vétiver et cèdre. Irrésistiblement provocant.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.52464.jpg",
//                 gender: "Unisex",
//                 intensity: "Forte",
//                 longevity: "8-12 heures",
//                 sillage: "Fort",
//                 priceRange: "$$$$",
//                 price: 3400,
//                 stockQuantity: 16,
//                 families: ["Gourmand", "Fruity"],
//                 notes: [
//                     { name: "Cerise Noire", level: "Top" },
//                     { name: "Liqueur de Cerise", level: "Top" },
//                     { name: "Amande Amère", level: "Middle" },
//                     { name: "Cerise Griotte", level: "Middle" },
//                     { name: "Santal", level: "Base" },
//                     { name: "Vétiver", level: "Base" },
//                     { name: "Cèdre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Cherry", intensity: "Strong" },
//                     { name: "Almond", intensity: "Medium" },
//                     { name: "Gourmand", intensity: "Strong" },
//                 ],
//                 tags: ["Provocant", "Gourmand", "Fruité", "Addictif"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Soirée", "Rendez-vous"],
//             },
//             // More accessible fragrances
//             {
//                 name: "1 Million",
//                 brand: "Paco Rabanne",
//                 description:
//                     "1 Million est un cuir épicé frais et audacieux. Mandarine et menthe fraîche s'ouvrent sur un cœur de rose et cannelle, avec un fond de cuir et ambre blanc. Pour l'homme qui ose.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.6697.jpg",
//                 gender: "Male",
//                 intensity: "Forte",
//                 longevity: "6-8 heures",
//                 sillage: "Fort",
//                 priceRange: "$$",
//                 price: 750,
//                 stockQuantity: 65,
//                 families: ["Spicy", "Leather"],
//                 notes: [
//                     { name: "Mandarine", level: "Top" },
//                     { name: "Menthe", level: "Top" },
//                     { name: "Pamplemousse", level: "Top" },
//                     { name: "Rose", level: "Middle" },
//                     { name: "Cannelle", level: "Middle" },
//                     { name: "Cuir", level: "Base" },
//                     { name: "Ambre Blanc", level: "Base" },
//                     { name: "Bois", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Spicy", intensity: "Strong" },
//                     { name: "Leather", intensity: "Medium" },
//                     { name: "Fresh", intensity: "Medium" },
//                 ],
//                 tags: ["Audacieux", "Séducteur", "Festif", "Best-seller"],
//                 seasons: ["Automne", "Hiver"],
//                 occasions: ["Soirée", "Rendez-vous", "Cérémonie"],
//             },
//             {
//                 name: "Invictus",
//                 brand: "Paco Rabanne",
//                 description:
//                     "Invictus est un aromatic aquatique frais et puissant. Notes marines et de pamplemousse avec un cœur de laurier et jasmin, sur un fond de bois de gaïac et ambre gris. Pour le champion.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.23088.jpg",
//                 gender: "Male",
//                 intensity: "Modérée",
//                 longevity: "6-8 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$",
//                 price: 680,
//                 stockQuantity: 70,
//                 families: ["Aquatic", "Fresh"],
//                 notes: [
//                     { name: "Marine", level: "Top" },
//                     { name: "Pamplemousse", level: "Top" },
//                     { name: "Laurier", level: "Middle" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Bois de Gaïac", level: "Base" },
//                     { name: "Ambre Gris", level: "Base" },
//                     { name: "Mousse de Chêne", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Fresh", intensity: "Strong" },
//                     { name: "Aquatic", intensity: "Strong" },
//                     { name: "Woody", intensity: "Medium" },
//                 ],
//                 tags: ["Sportif", "Frais", "Dynamique", "Champion"],
//                 seasons: ["Printemps", "Été"],
//                 occasions: ["Quotidien", "Sport", "Bureau"],
//             },
//             {
//                 name: "Dolce & Gabbana Light Blue",
//                 brand: "Dolce & Gabbana",
//                 description:
//                     "Light Blue capture l'essence de l'été méditerranéen. Pomme de Sicile et cèdre s'associent au jasmin et bambou, sur un fond d'ambre et musc blanc. Fraîcheur italienne.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.485.jpg",
//                 gender: "Female",
//                 intensity: "Légère",
//                 longevity: "4-6 heures",
//                 sillage: "Intime",
//                 priceRange: "$$",
//                 price: 720,
//                 stockQuantity: 80,
//                 families: ["Citrus", "Floral"],
//                 notes: [
//                     { name: "Pomme Sicilienne", level: "Top" },
//                     { name: "Citron", level: "Top" },
//                     { name: "Cèdre", level: "Top" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Bambou", level: "Middle" },
//                     { name: "Rose Blanche", level: "Middle" },
//                     { name: "Ambre", level: "Base" },
//                     { name: "Musc Blanc", level: "Base" },
//                     { name: "Cèdre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Citrus", intensity: "Strong" },
//                     { name: "Fresh", intensity: "Strong" },
//                     { name: "Floral", intensity: "Light" },
//                 ],
//                 tags: ["Estival", "Frais", "Méditerranéen", "Classique"],
//                 seasons: ["Printemps", "Été"],
//                 occasions: ["Quotidien", "Vacances", "Bureau"],
//             },
//             {
//                 name: "Versace Pour Homme",
//                 brand: "Versace",
//                 description:
//                     "Versace Pour Homme est un aromatic méditerranéen. Citrus et néroli sur un cœur de cèdre, sauge et ambre, avec un fond de musc et bois précieux. Élégance italienne décontractée.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.4740.jpg",
//                 gender: "Male",
//                 intensity: "Légère",
//                 longevity: "4-6 heures",
//                 sillage: "Intime",
//                 priceRange: "$$",
//                 price: 650,
//                 stockQuantity: 75,
//                 families: ["Aromatic", "Fougère"],
//                 notes: [
//                     { name: "Néroli", level: "Top" },
//                     { name: "Citron", level: "Top" },
//                     { name: "Bergamote", level: "Top" },
//                     { name: "Cèdre", level: "Middle" },
//                     { name: "Sauge", level: "Middle" },
//                     { name: "Ambre", level: "Middle" },
//                     { name: "Musc", level: "Base" },
//                     { name: "Bois Précieux", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Fresh", intensity: "Strong" },
//                     { name: "Citrus", intensity: "Medium" },
//                     { name: "Aromatic", intensity: "Medium" },
//                 ],
//                 tags: ["Italien", "Décontracté", "Élégant", "Classique"],
//                 seasons: ["Printemps", "Été"],
//                 occasions: ["Quotidien", "Bureau", "Vacances"],
//             },
//             {
//                 name: "Chance Eau Tendre",
//                 brand: "Chanel",
//                 description:
//                     "Chance Eau Tendre est une interprétation délicate du hasard. Un tourbillon floral fruité de pamplemousse, jasmin et jacinthe sur un lit de cèdre blanc et iris. Fraîcheur tendre.",
//                 imageUrl: "https://fimgs.net/mdimg/perfume/375x500.12tried.jpg",
//                 gender: "Female",
//                 intensity: "Légère",
//                 longevity: "4-6 heures",
//                 sillage: "Modéré",
//                 priceRange: "$$$",
//                 price: 1200,
//                 stockQuantity: 45,
//                 families: ["Floral", "Fruity"],
//                 notes: [
//                     { name: "Pamplemousse", level: "Top" },
//                     { name: "Coing", level: "Top" },
//                     { name: "Jasmin", level: "Middle" },
//                     { name: "Jacinthe", level: "Middle" },
//                     { name: "Rose", level: "Middle" },
//                     { name: "Cèdre Blanc", level: "Base" },
//                     { name: "Iris", level: "Base" },
//                     { name: "Ambre", level: "Base" },
//                 ],
//                 accords: [
//                     { name: "Floral", intensity: "Medium" },
//                     { name: "Fresh", intensity: "Strong" },
//                     { name: "Citrus", intensity: "Medium" },
//                 ],
//                 tags: ["Délicat", "Romantique", "Tendre", "Printemps"],
//                 seasons: ["Printemps", "Été"],
//                 occasions: ["Quotidien", "Bureau", "Rendez-vous"],
//             },
//         ];

//         return curatedPerfumes;
//     }

//     /**
//      * Convert scraped perfumes to database-ready format
//      */
//     convertToDbFormat(perfumes: ScrapedPerfume[]): PerfumeDbData {
//         const data: PerfumeDbData = {
//             brands: [],
//             families: [],
//             notes: [],
//             accords: [],
//             tags: [],
//             seasons: [],
//             occasions: [],
//             perfumes: [],
//             perfumeFamilies: [],
//             perfumeNotes: [],
//             perfumeAccords: [],
//             perfumeTags: [],
//             perfumeSeasons: [],
//             perfumeOccasions: [],
//         };

//         // Process each perfume
//         for (const perfume of perfumes) {
//             const perfumeId = uuidv4();
//             const brandId = this.getOrCreateId(this.brands, perfume.brand);

//             // Add perfume
//             data.perfumes.push({
//                 id: perfumeId,
//                 name: perfume.name,
//                 brandId: brandId,
//                 description: perfume.description,
//                 imageUrl: perfume.imageUrl,
//                 gender: perfume.gender,
//                 intensity: perfume.intensity,
//                 longevity: perfume.longevity,
//                 sillage: perfume.sillage,
//                 priceRange: perfume.priceRange,
//                 price: perfume.price,
//                 stockQuantity: perfume.stockQuantity,
//             });

//             // Process families
//             for (const family of perfume.families) {
//                 const familyId = this.getOrCreateId(this.families, family);
//                 data.perfumeFamilies.push({ perfumeId, familyId });
//             }

//             // Process notes
//             for (const note of perfume.notes) {
//                 const noteId = this.getOrCreateNoteId(note.name, note.level);
//                 data.perfumeNotes.push({
//                     perfumeId,
//                     noteId,
//                     noteLevel: note.level,
//                 });
//             }

//             // Process accords
//             for (const accord of perfume.accords) {
//                 const accordId = this.getOrCreateId(this.accords, accord.name);
//                 data.perfumeAccords.push({
//                     perfumeId,
//                     accordId,
//                     intensity: accord.intensity,
//                 });
//             }

//             // Process tags
//             for (const tag of perfume.tags) {
//                 const tagId = this.getOrCreateId(this.tags, tag);
//                 data.perfumeTags.push({ perfumeId, tagId });
//             }

//             // Process seasons
//             for (const season of perfume.seasons) {
//                 const seasonId = this.seasons.get(season);
//                 if (seasonId) {
//                     data.perfumeSeasons.push({ perfumeId, seasonId });
//                 }
//             }

//             // Process occasions
//             for (const occasion of perfume.occasions) {
//                 const occasionId = this.occasions.get(occasion);
//                 if (occasionId) {
//                     data.perfumeOccasions.push({ perfumeId, occasionId });
//                 }
//             }
//         }

//         // Convert maps to arrays
//         data.brands = Array.from(this.brands.entries()).map(([name, id]) => ({
//             id,
//             name,
//         }));
//         data.families = Array.from(this.families.entries()).map(
//             ([name, id]) => ({ id, name })
//         );
//         data.notes = Array.from(this.notes.entries()).map(
//             ([name, { id, category }]) => ({ id, name, category })
//         );
//         data.accords = Array.from(this.accords.entries()).map(([name, id]) => ({
//             id,
//             name,
//         }));
//         data.tags = Array.from(this.tags.entries()).map(([name, id]) => ({
//             id,
//             name,
//         }));
//         data.seasons = Array.from(this.seasons.entries()).map(([name, id]) => ({
//             id,
//             name,
//         }));
//         data.occasions = Array.from(this.occasions.entries()).map(
//             ([name, id]) => ({ id, name })
//         );

//         return data;
//     }

//     /**
//      * Generate SQL INSERT statements
//      */
//     generateSql(data: PerfumeDbData): string {
//         const escapeStr = (s: string) => s.replace(/'/g, "''");

//         let sql = `-- ALOud Perfume Database Seed
// -- Generated on ${new Date().toISOString()}
// -- Run this in SQL Server Management Studio or via EF migration

// SET IDENTITY_INSERT [dbo].[Brands] OFF;
// SET IDENTITY_INSERT [dbo].[Families] OFF;
// SET IDENTITY_INSERT [dbo].[Notes] OFF;
// SET IDENTITY_INSERT [dbo].[Accords] OFF;
// SET IDENTITY_INSERT [dbo].[Tags] OFF;
// SET IDENTITY_INSERT [dbo].[Seasons] OFF;
// SET IDENTITY_INSERT [dbo].[Occasions] OFF;

// -- =====================================================
// -- BRANDS
// -- =====================================================
// `;

//         for (const brand of data.brands) {
//             sql += `INSERT INTO Brands (Id, Name) VALUES ('${
//                 brand.id
//             }', N'${escapeStr(brand.name)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- FAMILIES (Fragrance Families)
// -- =====================================================
// `;
//         for (const family of data.families) {
//             sql += `INSERT INTO Families (Id, Name) VALUES ('${
//                 family.id
//             }', N'${escapeStr(family.name)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- NOTES
// -- =====================================================
// `;
//         for (const note of data.notes) {
//             sql += `INSERT INTO Notes (Id, Name, Category) VALUES ('${
//                 note.id
//             }', N'${escapeStr(note.name)}', N'${escapeStr(note.category)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- ACCORDS
// -- =====================================================
// `;
//         for (const accord of data.accords) {
//             sql += `INSERT INTO Accords (Id, Name) VALUES ('${
//                 accord.id
//             }', N'${escapeStr(accord.name)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- TAGS
// -- =====================================================
// `;
//         for (const tag of data.tags) {
//             sql += `INSERT INTO Tags (Id, Name) VALUES ('${
//                 tag.id
//             }', N'${escapeStr(tag.name)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- SEASONS
// -- =====================================================
// `;
//         for (const season of data.seasons) {
//             sql += `INSERT INTO Seasons (Id, Name) VALUES ('${
//                 season.id
//             }', N'${escapeStr(season.name)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- OCCASIONS
// -- =====================================================
// `;
//         for (const occasion of data.occasions) {
//             sql += `INSERT INTO Occasions (Id, Name) VALUES ('${
//                 occasion.id
//             }', N'${escapeStr(occasion.name)}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUMES
// -- =====================================================
// `;
//         for (const p of data.perfumes) {
//             sql += `INSERT INTO Perfumes (Id, Name, BrandId, Description, ImageUrl, GenderProfile, Intensity, Longevity, Sillage, PriceRange, Price, StockQuantity, CreatedAt) 
// VALUES ('${p.id}', N'${escapeStr(p.name)}', '${p.brandId}', N'${escapeStr(
//                 p.description
//             )}', N'${escapeStr(p.imageUrl)}', N'${p.gender}', N'${escapeStr(
//                 p.intensity
//             )}', N'${escapeStr(p.longevity)}', N'${escapeStr(p.sillage)}', N'${
//                 p.priceRange
//             }', ${p.price}, ${p.stockQuantity}, GETUTCDATE());\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUME - FAMILY RELATIONS
// -- =====================================================
// `;
//         for (const pf of data.perfumeFamilies) {
//             sql += `INSERT INTO PerfumeFamilies (PerfumeId, FamilyId) VALUES ('${pf.perfumeId}', '${pf.familyId}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUME - NOTE RELATIONS
// -- =====================================================
// `;
//         for (const pn of data.perfumeNotes) {
//             sql += `INSERT INTO PerfumeNotes (PerfumeId, NoteId, NoteLevel) VALUES ('${pn.perfumeId}', '${pn.noteId}', N'${pn.noteLevel}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUME - ACCORD RELATIONS
// -- =====================================================
// `;
//         for (const pa of data.perfumeAccords) {
//             sql += `INSERT INTO PerfumeAccords (PerfumeId, AccordId, Intensity) VALUES ('${pa.perfumeId}', '${pa.accordId}', N'${pa.intensity}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUME - TAG RELATIONS
// -- =====================================================
// `;
//         for (const pt of data.perfumeTags) {
//             sql += `INSERT INTO PerfumeTags (PerfumeId, TagId) VALUES ('${pt.perfumeId}', '${pt.tagId}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUME - SEASON RELATIONS
// -- =====================================================
// `;
//         for (const ps of data.perfumeSeasons) {
//             sql += `INSERT INTO PerfumeSeasons (PerfumeId, SeasonId) VALUES ('${ps.perfumeId}', '${ps.seasonId}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- PERFUME - OCCASION RELATIONS
// -- =====================================================
// `;
//         for (const po of data.perfumeOccasions) {
//             sql += `INSERT INTO PerfumeOccasions (PerfumeId, OccasionId) VALUES ('${po.perfumeId}', '${po.occasionId}');\n`;
//         }

//         sql += `
// -- =====================================================
// -- SEED COMPLETE
// -- =====================================================
// PRINT 'Perfume database seeded successfully!';
// PRINT 'Total Brands: ${data.brands.length}';
// PRINT 'Total Families: ${data.families.length}';
// PRINT 'Total Notes: ${data.notes.length}';
// PRINT 'Total Accords: ${data.accords.length}';
// PRINT 'Total Tags: ${data.tags.length}';
// PRINT 'Total Perfumes: ${data.perfumes.length}';
// `;

//         return sql;
//     }

//     /**
//      * Save data to files
//      */
//     saveToFiles(data: PerfumeDbData): void {
//         const outputDir = path.join(__dirname, "..", "output");

//         if (!fs.existsSync(outputDir)) {
//             fs.mkdirSync(outputDir, { recursive: true });
//         }

//         // Save JSON
//         const jsonPath = path.join(outputDir, "perfumes-full.json");
//         fs.writeFileSync(jsonPath, JSON.stringify(data, null, 2), "utf-8");
//         console.log(`JSON saved to: ${jsonPath}`);

//         // Save SQL
//         const sqlPath = path.join(outputDir, "perfumes-seed.sql");
//         const sql = this.generateSql(data);
//         fs.writeFileSync(sqlPath, sql, "utf-8");
//         console.log(`SQL saved to: ${sqlPath}`);
//     }

//     /**
//      * Main execution
//      */
//     async run(): Promise<void> {
//         console.log("=== ALOud Perfume Scraper ===\n");
//         console.log("Generating curated perfume database...\n");

//         // Use curated data (reliable and with real images)
//         const perfumes = this.generateCuratedPerfumes();
//         console.log(`Generated ${perfumes.length} perfumes with full data\n`);

//         // Convert to database format
//         const dbData = this.convertToDbFormat(perfumes);

//         // Save to files
//         this.saveToFiles(dbData);

//         // Summary
//         console.log("\n=== Summary ===");
//         console.log(`Brands: ${dbData.brands.length}`);
//         console.log(`Families: ${dbData.families.length}`);
//         console.log(`Notes: ${dbData.notes.length}`);
//         console.log(`Accords: ${dbData.accords.length}`);
//         console.log(`Tags: ${dbData.tags.length}`);
//         console.log(`Seasons: ${dbData.seasons.length}`);
//         console.log(`Occasions: ${dbData.occasions.length}`);
//         console.log(`Perfumes: ${dbData.perfumes.length}`);

//         console.log("\n=== Sample Perfumes ===");
//         dbData.perfumes.slice(0, 3).forEach((p, i) => {
//             console.log(
//                 `\n${i + 1}. ${p.name} by ${
//                     dbData.brands.find((b) => b.id === p.brandId)?.name
//                 }`
//             );
//             console.log(`   Gender: ${p.gender} | Price: ${p.price} MAD`);
//             console.log(`   ${p.description.substring(0, 100)}...`);
//         });
//     }
// }

// // Run
// if (require.main === module) {
//     const scraper = new FragranticaScraper();
//     scraper.run().catch(console.error);
// }

// export { FragranticaScraper };
