/**
 * ALOud REST API TypeScript Type Definitions
 * 
 * This file contains all TypeScript interfaces for the ALOud REST API client.
 * Organized by domain and follows REST API structure.
 * 
 * API Base URL: /api/v1
 * Authentication: JWT Bearer Token
 */

// =====================================================
// API RESPONSE WRAPPER TYPES
// =====================================================

/**
 * Standard API response wrapper for all endpoints
 */
export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
  timestamp?: string;
}

/**
 * Paginated response wrapper for list endpoints
 */
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Validation error response
 */
export interface ValidationErrorResponse {
  success: false;
  errors: Record<string, string[]>;
  timestamp?: string;
}

/**
 * Standard error response
 */
export interface ErrorResponse {
  success: false;
  error: string;
  message?: string;
  statusCode?: number;
  timestamp?: string;
}

// =====================================================
// PAGINATION & QUERY PARAMETERS
// =====================================================

/**
 * Standard pagination query parameters
 */
export interface PaginationParams {
  pageIndex?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

// =====================================================
// DASHBOARD TYPES
// =====================================================

export interface DashboardStatsDto {
  totalPerfumes: number;
  totalBrands: number;
  totalFamilies: number;
  totalNotes: number;
  totalAccords: number;
  totalTags: number;
  totalSeasons: number;
  totalOccasions: number;
  topBrands: BrandStatsDto[];
  topFamilies: FamilyStatsDto[];
  recentPerfumes: RecentPerfumeDto[];
}

export interface BrandStatsDto {
  brandId: string;
  brandName: string;
  perfumeCount: number;
}

export interface FamilyStatsDto {
  familyId: string;
  familyName: string;
  perfumeCount: number;
}

export interface RecentPerfumeDto {
  perfumeId: string;
  perfumeName: string;
  brandName: string;
  genderProfile?: string;
  imageUrl?: string;
  createdAt: string;
}

// =====================================================
// BRAND TYPES
// =====================================================

export interface CreateBrandDto {
  name: string;
}

export interface UpdateBrandDto {
  name: string;
}

export interface BrandDto {
  id: string;
  name: string;
  perfumeCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface BrandSelectDto {
  id: string;
  name: string;
}

// =====================================================
// FAMILY TYPES
// =====================================================

export interface CreateFamilyDto {
  name: string;
  description?: string;
}

export interface UpdateFamilyDto {
  name: string;
  description?: string;
}

export interface FamilyDto {
  id: string;
  name: string;
  description?: string;
  perfumeCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface FamilySelectDto {
  id: string;
  name: string;
}

// =====================================================
// TAG TYPES
// =====================================================

export interface CreateTagDto {
  name: string;
  description?: string;
  color?: string;
  category?: string;
}

export interface UpdateTagDto {
  name: string;
  description?: string;
  color?: string;
  category?: string;
}

export interface TagDto {
  id: string;
  name: string;
  description?: string;
  color?: string;
  category?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface TagSelectDto {
  id: string;
  name: string;
  color?: string;
}

// =====================================================
// SEASON TYPES
// =====================================================

export interface CreateSeasonDto {
  name: string;
  description?: string;
  recommendedNotes?: string[];
  colorTheme?: string;
  temperatureRange?: string;
}

export interface UpdateSeasonDto {
  name: string;
  description?: string;
  recommendedNotes?: string[];
  colorTheme?: string;
  temperatureRange?: string;
}

export interface SeasonDto {
  id: string;
  name: string;
  description?: string;
  recommendedNotes?: string[];
  colorTheme?: string;
  temperatureRange?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface SeasonSelectDto {
  id: string;
  name: string;
}

// =====================================================
// OCCASION TYPES
// =====================================================

export interface CreateOccasionDto {
  name: string;
  description?: string;
  recommendedSillage?: string;
  recommendedLongevity?: string;
  timeOfDay?: string[];
  gender?: string[];
}

export interface UpdateOccasionDto {
  name: string;
  description?: string;
  recommendedSillage?: string;
  recommendedLongevity?: string;
  timeOfDay?: string[];
  gender?: string[];
}

export interface OccasionDto {
  id: string;
  name: string;
  description?: string;
  recommendedSillage?: string;
  recommendedLongevity?: string;
  timeOfDay?: string[];
  gender?: string[];
  createdAt?: string;
  updatedAt?: string;
}

export interface OccasionSelectDto {
  id: string;
  name: string;
}

// =====================================================
// NOTE TYPES
// =====================================================

export interface CreateNoteDto {
  name: string;
  description?: string;
  noteType?: string;
  category: 'Top' | 'Middle' | 'Base';
  intensity?: number;
  color?: string;
  synonyms?: string;
}

export interface UpdateNoteDto {
  name: string;
  description?: string;
  noteType?: string;
  category: 'Top' | 'Middle' | 'Base';
  intensity?: number;
  color?: string;
  synonyms?: string;
}

export interface NoteDto {
  id: string;
  name: string;
  description?: string;
  noteType?: string;
  category: 'Top' | 'Middle' | 'Base';
  intensity?: number;
  color?: string;
  synonyms?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface NoteSelectDto {
  id: string;
  name: string;
  category: 'Top' | 'Middle' | 'Base';
}

/**
 * Note selection for perfume with level/position
 */
export interface PerfumeNoteSelectionDto {
  noteId: string;
  noteLevel?: 'Top' | 'Middle' | 'Base';
}

export interface PerfumeNoteDto {
  noteId: string;
  noteName: string;
  noteLevel?: string;
  category?: 'Top' | 'Middle' | 'Base';
}

// =====================================================
// ACCORD TYPES
// =====================================================

export interface CreateAccordDto {
  name: string;
  description?: string;
  compositionNotes?: string[];
  effect?: string;
  popularity?: number;
}

export interface UpdateAccordDto {
  name: string;
  description?: string;
  compositionNotes?: string[];
  effect?: string;
  popularity?: number;
}

export interface AccordDto {
  id: string;
  name: string;
  description?: string;
  compositionNotes?: string[];
  effect?: string;
  popularity?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface AccordSelectDto {
  id: string;
  name: string;
}

/**
 * Accord selection for perfume with intensity
 */
export interface PerfumeAccordSelectionDto {
  accordId: string;
  intensity?: 'Strong' | 'Medium' | 'Light';
}

export interface PerfumeAccordDto {
  accordId: string;
  accordName: string;
  intensity?: 'Strong' | 'Medium' | 'Light';
}

// =====================================================
// PERFUME TYPES
// =====================================================

export interface CreatePerfumeDto {
  name: string;
  brandId: string;
  description?: string;
  imageUrl?: string;
  price: number;
  stockQuantity: number;
  intensity?: string;
  longevity?: string;
  sillage?: string;
  genderProfile?: string;
  priceRange?: string;
  familyIds: string[];
  noteSelections: PerfumeNoteSelectionDto[];
  accordSelections: PerfumeAccordSelectionDto[];
  tagIds: string[];
  seasonIds: string[];
  occasionIds: string[];
}

export interface UpdatePerfumeDto extends Omit<CreatePerfumeDto, 'id'> {}

export interface PerfumeDto {
  id: string;
  name: string;
  brandId: string;
  brandName: string;
  description?: string;
  imageUrl?: string;
  price: number;
  stockQuantity: number;
  intensity?: string;
  longevity?: string;
  sillage?: string;
  genderProfile?: string;
  priceRange?: string;
  families: FamilySelectDto[];
  tags: TagSelectDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface PerfumeDetailsDto extends PerfumeDto {
  notes: PerfumeNoteDto[];
  accords: PerfumeAccordDto[];
  seasons: SeasonSelectDto[];
  occasions: OccasionSelectDto[];
}

export interface PerfumeSelectDto {
  id: string;
  name: string;
  brandName: string;
}

export interface PerfumeBrowseDto {
  id: string;
  name: string;
  brandName: string;
  imageUrl?: string;
  price: number;
  genderProfile?: string;
  intensity?: string;
  families: string[];
}

// =====================================================
// ACCOUNT / AUTHENTICATION TYPES
// =====================================================

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  message: string;
  token: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isEmailVerified?: boolean;
  createdAt?: string;
}

export interface UpdateProfileDto {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  profileImage?: string;
}

// =====================================================
// CART TYPES (Customer)
// =====================================================

export interface AddToCartDto {
  quantity: number;
}

export interface CartItemDto {
  perfumeId: string;
  perfumeName: string;
  brandName: string;
  price: number;
  quantity: number;
  subtotal: number;
  imageUrl?: string;
}

export interface CartDto {
  items: CartItemDto[];
  totalItems: number;
  totalPrice: number;
  currency: string;
}

// =====================================================
// LLM CONFIG TYPES (Admin)
// =====================================================

export interface LlmConfigDto {
  enabled: boolean;
  provider?: string;
  model?: string;
  temperature?: number;
  maxTokens?: number;
}

export interface UpdateLlmConfigDto {
  enabled: boolean;
  provider?: string;
  model?: string;
  temperature?: number;
  maxTokens?: number;
}

// =====================================================
// API CLIENT REQUEST/RESPONSE HELPERS
// =====================================================

/**
 * Helper type for extracting data from paginated response
 */
export type PaginatedData<T> = PaginatedResponse<T>['data'];

/**
 * Helper type for API request body (excludes id and timestamps)
 */
export type CreateRequest<T> = Omit<T, 'id' | 'createdAt' | 'updatedAt'>;

/**
 * Helper type for API update request body (excludes id and timestamps)
 */
export type UpdateRequest<T> = Omit<T, 'id' | 'createdAt' | 'updatedAt'>;
