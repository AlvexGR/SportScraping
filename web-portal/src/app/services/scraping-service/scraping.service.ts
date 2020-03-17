import { Injectable } from '@angular/core';
import { BaseApiHelper } from 'src/app/helpers/base-api.helper';
import ScrapingInformation from 'src/app/models/scraping-information';
import { ScrapeType } from 'src/app/models/provider';
import * as moment from 'moment';
import { Constant } from 'src/app/helpers/constants.helper';

@Injectable({
  providedIn: 'root'
})
export class ScrapingService {

  constructor(private _baseApi: BaseApiHelper) { }

  public async GetTodayScrapingInfo(sportCode: string, scrapeType: ScrapeType): Promise<Array<ScrapingInformation>> {
    if (!sportCode) {
      throw new Error("sportCode is null");
    }

    const url = `${BaseApiHelper.baseUrl}/Scraping/${moment(new Date()).format(Constant.API_DATETIME_FORMAT)}/${moment(new Date()).format(Constant.API_DATETIME_FORMAT)}/${sportCode}/${scrapeType}`;
    return await this._baseApi.get<Array<ScrapingInformation>>(url);
  }
}
