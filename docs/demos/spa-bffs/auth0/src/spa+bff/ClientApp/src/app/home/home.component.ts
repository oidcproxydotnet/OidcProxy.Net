import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  user: any = { message: 'not signed in' };

  constructor(private http: HttpClient) {
    http.get("/account/me").subscribe((r) => {
      this.user = r;
    })
  }
}
