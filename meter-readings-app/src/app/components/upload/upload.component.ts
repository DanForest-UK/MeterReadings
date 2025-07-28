import { Component, ViewChild, ElementRef } from '@angular/core';
import { MeterReadingService } from '../../services/meter-reading.service';
import { UploadResult } from '../../models/upload-result.model';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  @ViewChild('fileInput', { static: false }) fileInput!: ElementRef<HTMLInputElement>;
  
  selectedFile: File | null = null;
  uploadResult: UploadResult | null = null;
  isUploading = false;
  errorMessage = '';

  constructor(private meterReadingService: MeterReadingService) { }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && file.name.toLowerCase().endsWith('.csv')) {
      this.selectedFile = file;
      this.uploadResult = null;
      this.errorMessage = '';
    } else {
      this.selectedFile = null;
      this.errorMessage = 'Please select a valid CSV file';
    }
  }

  onUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a CSV file first';
      return;
    }

    console.log('Starting upload for file:', this.selectedFile.name, 'Size:', this.selectedFile.size);

    this.isUploading = true;
    this.errorMessage = '';
    this.uploadResult = null;

    this.meterReadingService.uploadMeterReadings(this.selectedFile).subscribe({
      next: (result) => {
        console.log('Upload successful:', result);
        this.uploadResult = result;
        this.isUploading = false;
        
        // Reset file input to allow re-uploading same file
        this.resetFileInput();
      },
      error: (error: any) => {
        console.error('Upload failed - Full error object:', error);
        this.errorMessage = this.handleUploadError(error);
        this.isUploading = false;
      }
    });
  }

  private resetFileInput(): void {
    // Clear the file input value to allow re-uploading the same file
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
    this.selectedFile = null;
  }

  private handleUploadError(error: any): string {
    // Handle HTTP errors from server
    if (error instanceof HttpErrorResponse) {
      switch (error.status) {
        case 400:
          return this.extractServerErrorMessage(error);
        case 500:
          return 'Server error occurred. Please try again later.';
        default:
          return `Server error (${error.status}): ${error.statusText || 'Unknown error'}`;
      }
    }

    // Extract error message from different possible structures
    if (error.error && typeof error.error === 'string') {
      return error.error.includes('Expected CSV format:') ? error.error : `Upload failed: ${error.error}`;
    }

    return error.message ? `Upload failed: ${error.message}` : 'Upload failed. Please try again.';
  }

  private extractServerErrorMessage(error: HttpErrorResponse): string {
    if (error.error && typeof error.error === 'string') {
      return error.error.includes('Expected CSV format:') ? error.error : `Upload failed: ${error.error}`;
    }
    return `Server validation error: ${error.statusText || 'Invalid request'}`;
  }

  // Check if error message contains CSV format validation
  isValidationError(): boolean {
    if (!this.errorMessage || typeof this.errorMessage !== 'string') {
      return false;
    }
    return this.errorMessage.includes('Expected CSV format:') || 
           this.errorMessage.includes('CSV header validation failed:') ||
           this.errorMessage.includes('CSV contains extra data');
  }

  // Parse validation error into structured parts
  getValidationErrorParts(): { summary: string; details: string[]; example: string } | null {
    if (!this.isValidationError() || typeof this.errorMessage !== 'string') {
      return null;
    }

    const parts = this.errorMessage.split('Expected CSV format:');
    const summary = parts[0]?.replace('Upload failed: ', '').trim() || '';
    
    if (parts.length < 2) {
      return { summary, details: [], example: '' };
    }

    const formatSection = parts[1];
    const lines = formatSection.split('\n').map(line => line.trim()).filter(line => line);
    
    const details: string[] = [];
    let example = '';
    let inExample = false;
    
    for (const line of lines) {
      if (line.startsWith('Example:')) {
        inExample = true;
        continue;
      }
      
      if (inExample) {
        if (example) example += '\n';
        example += line;
      } else if (line.startsWith('•')) {
        details.push(line);
      }
    }
    
    return { summary, details, example };
  }

  getValidationMessage(): string {
    if (!this.uploadResult || this.uploadResult.failed === 0) {
      return '';
    }
    
    const errorCount = this.uploadResult.failed;
    const plural = errorCount === 1 ? '' : 's';
    return `${errorCount} error${plural} encountered during validation and must be corrected before the data can be saved. Please correct and re-upload the full list.`;
  }

  getStatusMessage(): string {
    if (!this.uploadResult) {
      return '';
    }

    if (this.uploadResult.failed > 0) {
      return 'Upload failed - no data was saved.';
    } else if (this.uploadResult.committed === 0) {
      return 'No records were committed.';
    } else {
      return 'All records committed successfully!';
    }
  }

  getResultClass(): string {
    if (!this.uploadResult) {
      return '';
    }

    if (this.uploadResult.failed > 0) {
      return 'error';
    } else {
      return 'success';
    }
  }

  // Method to start fresh upload (reset everything)
  startNewUpload(): void {
    this.uploadResult = null;
    this.errorMessage = '';
    this.resetFileInput();
  }
}
