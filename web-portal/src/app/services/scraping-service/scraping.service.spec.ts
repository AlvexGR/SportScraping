import { TestBed } from '@angular/core/testing';

import { ScrapingService } from './scraping.service';

describe('ScrapingService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ScrapingService = TestBed.get(ScrapingService);
    expect(service).toBeTruthy();
  });
});
