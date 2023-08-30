import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  user: any = 'not signed in..';
  forecast: any = null;

  constructor(private http: HttpClient) {
    http.get("/account/me").subscribe((r) => {
      this.user = r;
    })
  }

  get(url: string) {
    this.http.get(url).subscribe((r) => {
      this.forecast = r;
    }, (e) => {
      this.forecast = e;
    })
  }
}
