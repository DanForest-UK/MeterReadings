export interface UploadResult {
  validated: number;
  failed: number;
  committed: number;
  errors: string[];
}
