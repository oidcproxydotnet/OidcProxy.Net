import {Component, OnDestroy, OnInit} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {MeService} from "./me.Service";
import {Observable, Subject} from "rxjs";
import {map} from "rxjs/operators";
import {AsyncPipe,CommonModule} from "@angular/common";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, AsyncPipe, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {

  private userSubSubject: Subject<any> = new Subject<any>();

  userSub$ = this.userSubSubject.asObservable();

  availableActions$: Observable<any>;

  title = 'ClientApp';

  constructor(private meService: MeService) {
    this.availableActions$ = this.meService.getUserData().pipe(
      map(user => {
        if (user) {
          this.userSubSubject.next(user.sub);

          return [
            { title: 'Sign out', link: '/.auth/end-session' },
            { title: '/.auth/me', link: '/.auth/me' }
          ];
        } else {
          return [{ title: 'Sign in', link: '/.auth/login' }];
        }
      })
    );
  }
}
