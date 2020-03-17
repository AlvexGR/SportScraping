import { TestBed } from '@angular/core/testing';

import { MetricService } from './metric.service';

describe('MetricService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: MetricService = TestBed.get(MetricService);
    expect(service).toBeTruthy();
  });
});
