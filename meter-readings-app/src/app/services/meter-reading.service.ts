import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UploadResult } from '../models/upload-result.model';

@Injectable({
  providedIn: 'root'
})
export class MeterReadingService {
  private apiUrl = 'http://localhost:5121/api/meterreadings';

  constructor(private http: HttpClient) { }

  uploadMeterReadings(file: File): Observable<UploadResult> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<UploadResult>(`${this.apiUrl}/meter-reading-uploads`, formData);
  }
}
