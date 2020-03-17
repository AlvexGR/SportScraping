import { Constant } from 'src/app/helpers/constants.helper';
import { Injectable } from "@angular/core";
import { BaseApiHelper } from "src/app/helpers/base-api.helper";
import * as moment from "moment";
@Injectable({
  providedIn: "root"
})
export class MetricService {
  constructor(private _baseApi: BaseApiHelper) {}

  async exportMetricData(
    countryCode: string,
    sportCode: string,
    dateTime: Date
  ): Promise<Blob> {
    if (!sportCode) {
      throw new Error("sportCode is null");
    }

    if (!countryCode) {
      throw new Error("countryCode is null");
    }

    if (!dateTime) {
      throw new Error("dateTime is null");
    }

    const url = `${
      BaseApiHelper.baseUrl
    }/MetricData/Export/${countryCode}/${sportCode}/${moment(dateTime).format(Constant.API_DATETIME_FORMAT)}`;
    return await this._baseApi.downloadFile(url);
  }
}
