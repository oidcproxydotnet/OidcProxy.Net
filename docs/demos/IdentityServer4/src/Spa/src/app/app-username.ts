import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map } from 'rxjs';
import { User } from './userInfo';

@Component({
  selector: 'app-username',
  template: '<span *ngIf="username$ | async as username">{{username}}</span>',
  styleUrls: ['./app.component.css']
})
export class AppUserName {
  
    username$: Observable<string | null> = this.http.get<User>('/account/me', { observe: 'response' })
        .pipe(
            map((response) => {
                return 'Welcome ' + response.body?.name;
            }),
            catchError((e) => {
                if(e.status == 404) {
                    console.log('You are not logged in');
                    return '';
                }

                throw e;
            })
        )

    constructor(private http: HttpClient) {

    }

}
