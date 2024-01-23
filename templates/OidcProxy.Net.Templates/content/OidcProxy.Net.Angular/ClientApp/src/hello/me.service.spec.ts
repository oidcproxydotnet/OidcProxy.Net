import { TestBed } from '@angular/core/testing';

import { MeService } from './me.service';

describe('MeService', () => {
  let service: MeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
