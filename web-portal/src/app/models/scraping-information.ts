import { Provider } from './provider';
import BaseModel from './base.model';

export enum ScrapeStatus {
  Failed = -1,
  Pending = 0,
  InProgress = 1,
  Done = 2
}

export default class ScrapingInformation extends BaseModel {
  progress: number;
  progressExplanation: string;
  scrapeStatus: ScrapeStatus
  scrapeStatusDisplay: string;
  providerId: number;
  provider: Provider;
  scrapeTypeDisplay: string;
  scrapeTime: Date;
  scrapeTimeDisplay: string;
}
