// Types for the perfume scraper matching ALOud database schema

export interface ScrapedPerfume {
    name: string;
    brand: string;
    description: string;
    imageUrl: string;
    gender: "Male" | "Female" | "Unisex";
    intensity: string;
    longevity: string;
    sillage: string;
    priceRange: string;
    price: number;
    stockQuantity: number;
    families: string[];
    notes: {
        name: string;
        level: "Top" | "Middle" | "Base";
    }[];
    accords: {
        name: string;
        intensity: string;
    }[];
    tags: string[];
    seasons: string[];
    occasions: string[];
}

export interface PerfumeDbData {
    brands: { id: string; name: string }[];
    families: { id: string; name: string }[];
    notes: { id: string; name: string; category: string }[];
    accords: { id: string; name: string }[];
    tags: { id: string; name: string }[];
    seasons: { id: string; name: string }[];
    occasions: { id: string; name: string }[];
    perfumes: {
        id: string;
        name: string;
        brandId: string;
        description: string;
        imageUrl: string;
        gender: string;
        intensity: string;
        longevity: string;
        sillage: string;
        priceRange: string;
        price: number;
        stockQuantity: number;
    }[];
    perfumeFamilies: { perfumeId: string; familyId: string }[];
    perfumeNotes: { perfumeId: string; noteId: string; noteLevel: string }[];
    perfumeAccords: {
        perfumeId: string;
        accordId: string;
        intensity: string;
    }[];
    perfumeTags: { perfumeId: string; tagId: string }[];
    perfumeSeasons: { perfumeId: string; seasonId: string }[];
    perfumeOccasions: { perfumeId: string; occasionId: string }[];
}
