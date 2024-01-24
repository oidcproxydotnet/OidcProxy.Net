import { Component, OnInit } from '@angular/core';
import { MeService } from './me.service';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-hello',
  templateUrl: './hello.component.html',
  styleUrls: ['./hello.component.scss']
})
export class HelloComponent {

  userInfo$: Observable<any> = this.meService.getUserInfo().pipe(
    map(response => {
      return {
        userInfo: response
      };
    }),
    catchError(err => {
      if(err.status == 404) {
        return of({});
      }

      throw err;
    })
  );  

  constructor(private meService: MeService, private router: Router) {

  }

}
