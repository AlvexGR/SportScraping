import BaseModel from './base.model';

export enum ScrapeType {
  All = -1,
  Competition = 0,
  PlayerOverUnder = 1,
  PlayerHeadToHead = 2
}

export class Provider extends BaseModel {
  name: string;
  scrapeType: ScrapeType;
  sportCode: string;
}
