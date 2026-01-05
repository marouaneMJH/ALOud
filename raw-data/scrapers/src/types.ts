export interface CreateProductDto {
  Name: string;
  Description: string;
  Price: number;
  Stock: number;
  ImageUrl: string;
  CategoryId: number;
}

export interface ScrapedProduct {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  asin?: string;
  rating?: number;
  reviewCount?: number;
}
