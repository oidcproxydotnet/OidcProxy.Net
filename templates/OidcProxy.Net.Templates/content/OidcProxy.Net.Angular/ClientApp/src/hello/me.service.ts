import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserInfo } from 'src/models/userInfo';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MeService {
  constructor(private readonly httpClient: HttpClient) {}

  getUserInfo(): Observable<UserInfo> {
    return this.httpClient.get<UserInfo>('/.auth/me');
  }
}
