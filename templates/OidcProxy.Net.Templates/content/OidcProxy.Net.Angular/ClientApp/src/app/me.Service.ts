import { Injectable } from '@angular/core';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class MeService {

  private apiUrl = '/.auth/me';

  constructor(private http: HttpClient) { }

  getUserData(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      map(response => response),
      catchError(this.handleError())
    );
  }

  private handleError() {
    return (error: HttpErrorResponse): Observable<any> => {
      if (error.status === 401) {
        return of(null);
      } else {
        console.error(`/.auth/me failed: ${error.message}`);
        throw error.message;
      }
    };
  }
}
