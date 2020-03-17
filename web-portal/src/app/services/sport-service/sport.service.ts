import { BaseApiHelper } from './../../helpers/base-api.helper';
import { Injectable } from '@angular/core';
import Sport from 'src/app/models/sport';

@Injectable({
  providedIn: 'root'
})
export class SportService {

  constructor(private _baseApi: BaseApiHelper) { }

  /**
   * Get all sport from web portal api
   */
  public async getAll(): Promise<Array<Sport>> {
    const url = `${BaseApiHelper.baseUrl}/Sport/All`;
    return await this._baseApi.get<Array<Sport>>(url);
  }
}
