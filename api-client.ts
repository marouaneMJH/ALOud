/**
 * ALOud REST API Client Service
 * 
 * Provides type-safe methods for interacting with the ALOud REST API
 * All methods include proper error handling and type safety
 */

import {
  ApiResponse,
  PaginatedResponse,
  PaginationParams,
  // Dashboard
  DashboardStatsDto,
  // Brands
  BrandDto,
  BrandSelectDto,
  CreateBrandDto,
  UpdateBrandDto,
  // Families
  FamilyDto,
  FamilySelectDto,
  CreateFamilyDto,
  UpdateFamilyDto,
  // Tags
  TagDto,
  TagSelectDto,
  CreateTagDto,
  UpdateTagDto,
  // Seasons
  SeasonDto,
  SeasonSelectDto,
  CreateSeasonDto,
  UpdateSeasonDto,
  // Occasions
  OccasionDto,
  OccasionSelectDto,
  CreateOccasionDto,
  UpdateOccasionDto,
  // Notes
  NoteDto,
  NoteSelectDto,
  CreateNoteDto,
  UpdateNoteDto,
  // Accords
  AccordDto,
  AccordSelectDto,
  CreateAccordDto,
  UpdateAccordDto,
  // Perfumes
  PerfumeDto,
  PerfumeDetailsDto,
  PerfumeBrowseDto,
  CreatePerfumeDto,
  UpdatePerfumeDto,
  // Auth
  RegisterDto,
  LoginDto,
  LoginResponseDto,
  UserDto,
  UpdateProfileDto,
  // Cart
  AddToCartDto,
  CartDto,
} from './types';

/**
 * ALOud API Client - Type-safe REST API wrapper
 */
export class AloudApiClient {
  private baseUrl: string;
  private token: string | null = null;

  constructor(baseUrl: string = '/api/v1') {
    this.baseUrl = baseUrl;
  }

  /**
   * Set JWT token for authenticated requests
   */
  setToken(token: string): void {
    this.token = token;
  }

  /**
   * Clear JWT token
   */
  clearToken(): void {
    this.token = null;
  }

  /**
   * Make HTTP request with proper headers and error handling
   */
  private async request<T>(
    method: string,
    endpoint: string,
    body?: unknown
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }

    const options: RequestInit = {
      method,
      headers,
    };

    if (body) {
      options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);

    if (!response.ok) {
      const error = await response.json().catch(() => ({
        error: 'Unknown error',
      }));
      throw new Error(error.message || error.error || 'Request failed');
    }

    return response.json();
  }

  // =====================================================
  // HEALTH
  // =====================================================

  async getHealth(): Promise<ApiResponse> {
    return this.request('GET', '/health');
  }

  // =====================================================
  // DASHBOARD
  // =====================================================

  async getDashboardStats(): Promise<ApiResponse<DashboardStatsDto>> {
    return this.request('GET', '/admin/dashboard');
  }

  // =====================================================
  // BRANDS
  // =====================================================

  async listBrands(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<BrandDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/brands?${query.toString()}`);
  }

  async selectBrands(): Promise<ApiResponse<BrandSelectDto[]>> {
    return this.request('GET', '/admin/brands/select');
  }

  async createBrand(data: CreateBrandDto): Promise<ApiResponse<BrandDto>> {
    return this.request('POST', '/admin/brands', data);
  }

  async getBrandForEdit(id: string): Promise<ApiResponse<BrandDto>> {
    return this.request('GET', `/admin/brands/${id}`);
  }

  async updateBrand(id: string, data: UpdateBrandDto): Promise<ApiResponse<BrandDto>> {
    return this.request('PUT', `/admin/brands/${id}`, data);
  }

  async deleteBrand(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/brands/${id}`);
  }

  async validateBrandExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/brands/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // FAMILIES
  // =====================================================

  async listFamilies(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<FamilyDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/families?${query.toString()}`);
  }

  async selectFamilies(): Promise<ApiResponse<FamilySelectDto[]>> {
    return this.request('GET', '/admin/families/select');
  }

  async createFamily(data: CreateFamilyDto): Promise<ApiResponse<FamilyDto>> {
    return this.request('POST', '/admin/families', data);
  }

  async getFamilyForEdit(id: string): Promise<ApiResponse<FamilyDto>> {
    return this.request('GET', `/admin/families/${id}`);
  }

  async updateFamily(id: string, data: UpdateFamilyDto): Promise<ApiResponse<FamilyDto>> {
    return this.request('PUT', `/admin/families/${id}`, data);
  }

  async deleteFamily(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/families/${id}`);
  }

  async validateFamilyExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/families/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // TAGS
  // =====================================================

  async listTags(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<TagDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/tags?${query.toString()}`);
  }

  async selectTags(): Promise<ApiResponse<TagSelectDto[]>> {
    return this.request('GET', '/admin/tags/select');
  }

  async createTag(data: CreateTagDto): Promise<ApiResponse<TagDto>> {
    return this.request('POST', '/admin/tags', data);
  }

  async getTagForEdit(id: string): Promise<ApiResponse<TagDto>> {
    return this.request('GET', `/admin/tags/${id}`);
  }

  async updateTag(id: string, data: UpdateTagDto): Promise<ApiResponse<TagDto>> {
    return this.request('PUT', `/admin/tags/${id}`, data);
  }

  async deleteTag(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/tags/${id}`);
  }

  async validateTagExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/tags/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // SEASONS
  // =====================================================

  async listSeasons(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<SeasonDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/seasons?${query.toString()}`);
  }

  async selectSeasons(): Promise<ApiResponse<SeasonSelectDto[]>> {
    return this.request('GET', '/admin/seasons/select');
  }

  async createSeason(data: CreateSeasonDto): Promise<ApiResponse<SeasonDto>> {
    return this.request('POST', '/admin/seasons', data);
  }

  async getSeasonForEdit(id: string): Promise<ApiResponse<SeasonDto>> {
    return this.request('GET', `/admin/seasons/${id}`);
  }

  async updateSeason(id: string, data: UpdateSeasonDto): Promise<ApiResponse<SeasonDto>> {
    return this.request('PUT', `/admin/seasons/${id}`, data);
  }

  async deleteSeason(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/seasons/${id}`);
  }

  async validateSeasonExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/seasons/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // OCCASIONS
  // =====================================================

  async listOccasions(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<OccasionDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/occasions?${query.toString()}`);
  }

  async selectOccasions(): Promise<ApiResponse<OccasionSelectDto[]>> {
    return this.request('GET', '/admin/occasions/select');
  }

  async createOccasion(data: CreateOccasionDto): Promise<ApiResponse<OccasionDto>> {
    return this.request('POST', '/admin/occasions', data);
  }

  async getOccasionForEdit(id: string): Promise<ApiResponse<OccasionDto>> {
    return this.request('GET', `/admin/occasions/${id}`);
  }

  async updateOccasion(id: string, data: UpdateOccasionDto): Promise<ApiResponse<OccasionDto>> {
    return this.request('PUT', `/admin/occasions/${id}`, data);
  }

  async deleteOccasion(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/occasions/${id}`);
  }

  async validateOccasionExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/occasions/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // NOTES
  // =====================================================

  async listNotes(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<NoteDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/notes?${query.toString()}`);
  }

  async selectNotes(): Promise<ApiResponse<NoteSelectDto[]>> {
    return this.request('GET', '/admin/notes/select');
  }

  async createNote(data: CreateNoteDto): Promise<ApiResponse<NoteDto>> {
    return this.request('POST', '/admin/notes', data);
  }

  async getNoteForEdit(id: string): Promise<ApiResponse<NoteDto>> {
    return this.request('GET', `/admin/notes/${id}`);
  }

  async updateNote(id: string, data: UpdateNoteDto): Promise<ApiResponse<NoteDto>> {
    return this.request('PUT', `/admin/notes/${id}`, data);
  }

  async deleteNote(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/notes/${id}`);
  }

  async getNotesByCategory(category: 'Top' | 'Middle' | 'Base'): Promise<ApiResponse<NoteSelectDto[]>> {
    return this.request('GET', `/admin/notes/by-category?category=${encodeURIComponent(category)}`);
  }

  async validateNoteExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/notes/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // ACCORDS
  // =====================================================

  async listAccords(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<AccordDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/admin/accords?${query.toString()}`);
  }

  async selectAccords(): Promise<ApiResponse<AccordSelectDto[]>> {
    return this.request('GET', '/admin/accords/select');
  }

  async createAccord(data: CreateAccordDto): Promise<ApiResponse<AccordDto>> {
    return this.request('POST', '/admin/accords', data);
  }

  async getAccordForEdit(id: string): Promise<ApiResponse<AccordDto>> {
    return this.request('GET', `/admin/accords/${id}`);
  }

  async updateAccord(id: string, data: UpdateAccordDto): Promise<ApiResponse<AccordDto>> {
    return this.request('PUT', `/admin/accords/${id}`, data);
  }

  async deleteAccord(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/admin/accords/${id}`);
  }

  async validateAccordExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/admin/accords/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // PERFUMES
  // =====================================================

  async listPerfumes(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<PerfumeDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/perfumes?${query.toString()}`);
  }

  async selectPerfumes(): Promise<ApiResponse> {
    return this.request('GET', '/perfumes/select');
  }

  async createPerfume(data: CreatePerfumeDto): Promise<ApiResponse<PerfumeDto>> {
    return this.request('POST', '/perfumes', data);
  }

  async getPerfumeForEdit(id: string): Promise<ApiResponse<PerfumeDetailsDto>> {
    return this.request('GET', `/perfumes/${id}`);
  }

  async updatePerfume(id: string, data: UpdatePerfumeDto): Promise<ApiResponse<PerfumeDto>> {
    return this.request('PUT', `/perfumes/${id}`, data);
  }

  async deletePerfume(id: string): Promise<ApiResponse> {
    return this.request('DELETE', `/perfumes/${id}`);
  }

  async browsePerfumes(params?: PaginationParams): Promise<ApiResponse<PaginatedResponse<PerfumeBrowseDto>>> {
    const query = new URLSearchParams();
    if (params?.pageIndex) query.append('pageIndex', params.pageIndex.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params?.search) query.append('search', params.search);

    return this.request('GET', `/perfumes/customer/browse?${query.toString()}`);
  }

  async getPerfumeDetails(id: string): Promise<ApiResponse<PerfumeDetailsDto>> {
    return this.request('GET', `/perfumes/customer/details/${id}`);
  }

  async addToCart(perfumeId: string, data: AddToCartDto): Promise<ApiResponse> {
    return this.request('POST', `/perfumes/customer/add-to-cart/${perfumeId}`, data);
  }

  async validatePerfumeExists(name: string): Promise<ApiResponse<{ exists: boolean }>> {
    return this.request('GET', `/perfumes/validate-exists?name=${encodeURIComponent(name)}`);
  }

  // =====================================================
  // AUTHENTICATION
  // =====================================================

  async register(data: RegisterDto): Promise<ApiResponse<UserDto>> {
    return this.request('POST', '/account/register', data);
  }

  async login(data: LoginDto): Promise<ApiResponse<LoginResponseDto>> {
    const response = await this.request<ApiResponse<LoginResponseDto>>(
      'POST',
      '/account/login',
      data
    );

    if (response.data?.token) {
      this.setToken(response.data.token);
    }

    return response;
  }

  async logout(): Promise<void> {
    this.clearToken();
  }

  async getProfile(): Promise<ApiResponse<UserDto>> {
    return this.request('GET', '/account/profile');
  }

  async updateProfile(data: UpdateProfileDto): Promise<ApiResponse<UserDto>> {
    return this.request('PUT', '/account/profile', data);
  }

  async verifyEmail(code: string): Promise<ApiResponse> {
    return this.request('GET', `/account/verify-email?code=${encodeURIComponent(code)}`);
  }
}

// Export singleton instance
export const aloudApi = new AloudApiClient();
