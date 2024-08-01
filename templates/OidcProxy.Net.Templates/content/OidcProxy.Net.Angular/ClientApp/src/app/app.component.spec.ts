import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { MeService } from './me.Service';
import { of } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';

class MeServiceMock {
  getUserData() {
    return of({ sub: 'John Doe' });
  }
}

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule, RouterOutlet, HttpClientTestingModule, AppComponent],
      providers: [
        { provide: MeService, useClass: MeServiceMock },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have the 'ClientApp' title`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('ClientApp');
  });
});
