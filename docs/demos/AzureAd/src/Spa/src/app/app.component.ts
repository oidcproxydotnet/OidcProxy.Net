import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, tap } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  forecastSubject = new BehaviorSubject<Object>(new Object());

  forecast$: Observable<Object> = this.forecastSubject.asObservable();
  
  constructor(private http: HttpClient) {

  }

  getWeatherForecast() {
    this.http.get('/api/weatherforecast')
      .pipe(
        tap((response) => {
          this.forecastSubject.next(response);
        }),
        catchError((e) => {
          if(e.status == 401) {
            this.forecastSubject.next({ 'error': 'You are not logged in' });
          }

          throw e;
        })
      ).subscribe();
  }

}
