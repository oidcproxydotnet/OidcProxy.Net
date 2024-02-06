import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'Demo';
  
  user: any = '( not signed in.. )';
  
  forecast: any = null;

  constructor(private http: HttpClient) {
    http.get("/.auth/me").subscribe((r) => {
      this.user = r;
    })
  }

  get(url: string) {
    this.http.get(url).subscribe((r) => {
      this.forecast = r;
    }, (e) => {
      if(e.status == 502) {
        alert("Failed to get weather forecast. " +
            "Status 502 (bad gateway)." +
            "This typically happens if the API is not running. " +
            "Make sure the API has started and has been configured correctly.")
      }

      this.forecast = e;
    })
  }
}